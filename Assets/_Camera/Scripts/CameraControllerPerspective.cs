using UnityEngine;

public class CameraControllerPerspective : MonoBehaviour
{
    [Tooltip("In (Units/Sec), how fast will the camera position move to the target position. Lower numbers will slow this down and higher numbers speed it up.")]
    public float Smoothing;

    [HideInInspector]
    public CamShake camShake;
    GameObject camObj;
    Camera cam;
    Camera uiCam;

    public float edgeZoomThreshold = 0.1f;
    public float centerZoomThreshold = 0.5f;
    public Vector2 zoomRange = new Vector2(10f, 20f);

    private float camFOVVelocity = 0f;
    private float uiCamFOVVelocity = 0f;

    void Start()
    {
        // Get the camera component
        camObj = transform.GetChild(0).gameObject;
        cam = camObj.GetComponent<Camera>();
        uiCam = cam.transform.GetChild(0).GetComponent<Camera>();
    }

    void Update()
    {
        // Get all objects tagged "Player"
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length == 0)
        {
            Debug.Log("**No objects with the tag 'Player' were found in the scene**");
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


        int playersNearEdge = 0;
        int playersNearCenter = 0;



        foreach (GameObject player in players)
        {
            Vector3 viewportPosition = cam.WorldToViewportPoint(player.transform.position);

            // Check if the player is close to the edge of the view
            if (viewportPosition.x <= edgeZoomThreshold || viewportPosition.x >= 1 - edgeZoomThreshold ||
                viewportPosition.y <= edgeZoomThreshold || viewportPosition.y >= 1 - edgeZoomThreshold)
            {
                playersNearEdge++;
            }

            // Check if the player is close to the center of the view
            if (viewportPosition.x > centerZoomThreshold && viewportPosition.x < 1 - centerZoomThreshold &&
                viewportPosition.y > centerZoomThreshold && viewportPosition.y < 1 - centerZoomThreshold)
            {
                playersNearCenter++;
            }
        }

        if (playersNearEdge > 0 && cam.fieldOfView < zoomRange.y)
        {
            // Zoom out
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, cam.fieldOfView + 1, Time.deltaTime * Smoothing);
            uiCam.fieldOfView = Mathf.Lerp(uiCam.fieldOfView, cam.fieldOfView + 1, Time.deltaTime * Smoothing);
        }
        else if (playersNearCenter == players.Length && cam.fieldOfView > zoomRange.x)
        {
            // Zoom in if all players are close to the center
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, cam.fieldOfView - 1, Time.deltaTime * Smoothing);
            uiCam.fieldOfView = Mathf.Lerp(uiCam.fieldOfView, cam.fieldOfView - 1, Time.deltaTime * Smoothing);
        }

    }
}