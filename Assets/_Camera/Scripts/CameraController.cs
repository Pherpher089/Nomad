using UnityEngine;

public class CameraController : MonoBehaviour
{

    [Tooltip("In (Units/Sec), how fast will the camera position move to the target position. Lower numbers will slow this down and higher numbers speed it up.")]
    public float Smoothing;

    [HideInInspector]
    public CamShake camShake;
    GameObject camObj;
    Camera cam;

    // Threshold for when to zoom in or out
    [Tooltip("The distance between players at which the camera will start to zoom in or out")]
    public float zoomThreshold = 10f;

    // The minimum and maximum allowed zoom values
    [Tooltip("The minimum and maximum allowed zoom values")]
    public Vector2 zoomRange = new Vector2(10f, 20f);

    // The minimum and maximum allowed pan values
    [Tooltip("The minimum and maximum allowed pan values")]
    public Vector2 panRange = new Vector2(-10f, 10f);

    void Start()
    {
        // Get the camera component
        camObj = transform.GetChild(0).gameObject;
        cam = camObj.GetComponent<Camera>();
    }

    void Update()
    {
        // Get all objects tagged "Player"
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length == 0)
        {
            return;
        }

        // Calculate the center point of all the players
        Vector3 centerPoint = Vector3.zero;
        foreach (GameObject player in players)
        {
            centerPoint += player.transform.position;
        }
        centerPoint /= players.Length;

        // Move the camera towards the center point
        transform.position = Vector3.Lerp(transform.position, centerPoint, Time.deltaTime * Smoothing);

        // Calculate the distance between all the players
        float maxDistance = 0f;
        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(centerPoint, player.transform.position);
            if (distance > maxDistance)
            {
                maxDistance = distance;
            }
        }

        // Zoom in or out based on the distance between the players
        if (maxDistance > zoomThreshold)
        {
            // Calculate the desired size of the camera based on the distance between the players
            float desiredSize = Mathf.Lerp(zoomRange.x, zoomRange.y, maxDistance / zoomThreshold);

            // Clamp the size to the allowed range
            desiredSize = Mathf.Clamp(desiredSize, zoomRange.x, zoomRange.y);

            // Set the size of the camera
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, desiredSize, Time.deltaTime * Smoothing);
        }
        else if (maxDistance < zoomThreshold)
        {
            // Zoom back in if the players are close enough
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, zoomRange.x, Time.deltaTime * Smoothing);
        }
    }
}
