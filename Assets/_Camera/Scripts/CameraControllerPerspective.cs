using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

public class CameraControllerPerspective : MonoBehaviour
{
    [Tooltip("In (Units/Sec), how fast will the camera position move to the target position. Lower numbers will slow this down and higher numbers speed it up.")]
    public static CameraControllerPerspective Instance;
    public float zoomSpeedMultiplier;

    [HideInInspector]
    public CamShake camShake;
    private GameObject camObj;
    private Camera cam;
    private Camera uiCam;
    private PlayersManager playersManager;

    public float edgeZoomThreshold = 0.05f;
    public float centerZoomThreshold = 0.5f;
    public Vector2 zoomRange = new Vector2(10f, 20f);

    private float zoomPercentage = 0f;
    private bool isRotating = false; // Flag to prevent rotation during active rotation

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Get the camera components
        camObj = transform.GetChild(0).gameObject;
        cam = camObj.GetComponent<Camera>();
        uiCam = cam.transform.GetChild(0).GetComponent<Camera>();
        playersManager = FindObjectOfType<PlayersManager>();
        cam.fieldOfView = 30;
        uiCam.fieldOfView = 30;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        CalculateCenterPoint(players);
    }

    void Update()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0)
        {
            return;
        }

        // Calculate the center point of all the players
        Vector3 centerPoint = CalculateCenterPoint(players);

        // Move the camera towards the center point
        transform.position = Vector3.Lerp(transform.position, centerPoint, Time.deltaTime * zoomSpeedMultiplier);

        int playersNearEdge = 0;
        int playersNearCenter = 0;

        foreach (GameObject player in players)
        {
            // Check if the player is close to the center of the view
            Vector3 viewportPosition = cam.WorldToViewportPoint(player.transform.position);

            if (viewportPosition.x > centerZoomThreshold && viewportPosition.x < 1 - centerZoomThreshold &&
                viewportPosition.y > centerZoomThreshold && viewportPosition.y < 1 - centerZoomThreshold)
            {
                playersNearCenter++;
            }
            else if (viewportPosition.x <= edgeZoomThreshold || viewportPosition.x >= 1 - edgeZoomThreshold ||
                viewportPosition.y <= edgeZoomThreshold || viewportPosition.y >= 1 - edgeZoomThreshold)
            {
                playersNearEdge++;
            }
        }

        // Zoom out if players are too far from each other to be seen on camera
        AdjustAutomaticZoom(playersNearEdge, playersNearCenter, players.Length);
    }

    private Vector3 CalculateCenterPoint(GameObject[] players)
    {
        Vector3 centerPoint = Vector3.zero;
        foreach (GameObject player in players)
        {
            if (player.GetComponent<ThirdPersonCharacter>().isRiding)
            {
                centerPoint += BeastManager.Instance.transform.position;
            }
            else
            {
                centerPoint += player.transform.position;
            }
        }
        centerPoint /= players.Length;

        if (playersManager != null)
        {
            playersManager.playersCentralPosition = centerPoint;
        }

        return centerPoint;
    }

    public void RotateCameraSmoothly()
    {
        // Start the rotation coroutine if not already rotating
        if (!isRotating)
        {
            StartCoroutine(RotateCamera());
        }
    }

    private IEnumerator RotateCamera()
    {
        isRotating = true;
        float duration = 0.1f; // Duration of the rotation
        float elapsed = 0f;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, 90, 0));

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation; // Ensure it reaches the exact end rotation
        isRotating = false;
    }

    public void UpdateCameraZoom(float scroll, bool zoomButton)
    {
        //Adjust with scroll wheel
        if (scroll != 0)
        {
            AdjustZoomWithScroll(scroll);
        }
        // Check for button press to zoom out 33% increments
        else if (zoomButton)
        {
            AdjustZoomWithButton();
        }
    }

    public void SetCameraForBuild()
    {
        //StartCoroutine(SmoothZoomOut(zoomRange.y, 0.5f)); // Adjust the duration as needed
    }

    private IEnumerator SmoothZoomOut(float targetFOV, float duration)
    {
        float startFOV = cam.fieldOfView;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            cam.fieldOfView = Mathf.Lerp(startFOV, targetFOV, elapsed / duration);
            uiCam.fieldOfView = Mathf.Lerp(startFOV, targetFOV, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.fieldOfView = targetFOV; // Ensure it reaches the exact target FOV
        uiCam.fieldOfView = targetFOV;
    }

    void AdjustAutomaticZoom(int playersNearEdge, int playersNearCenter, int totalPlayers)
    {
        if ((playersNearEdge > 0) && cam.fieldOfView < zoomRange.y)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, cam.fieldOfView + 1, Time.deltaTime * zoomSpeedMultiplier);
            uiCam.fieldOfView = Mathf.Lerp(uiCam.fieldOfView, cam.fieldOfView + 1, Time.deltaTime * zoomSpeedMultiplier);
        }
    }

    public void AdjustZoomWithScroll(float scroll)
    {
        float targetFOV = cam.fieldOfView - scroll * 15; // Adjust the multiplier as needed for sensitivity
        targetFOV = Mathf.Clamp(targetFOV, zoomRange.x, zoomRange.y);
        cam.fieldOfView = targetFOV;
        uiCam.fieldOfView = targetFOV;
    }

    public void AdjustZoomWithButton()
    {
        // Calculate the percentage of the current FOV within the zoom range
        float y = zoomRange.y - zoomRange.x;
        float x = cam.fieldOfView - zoomRange.x; // Adjust to start from the min zoom range
        float currentPercent = x / y * 100f; // Calculate the percentage correctly within the range
        float tolerance = 0.01f;
        // Determine the next zoom percentage tier
        if (currentPercent < 33 - tolerance)
        {
            zoomPercentage = 33;
        }
        else if (currentPercent < 66 - tolerance)
        {
            zoomPercentage = 66;
        }
        else if (currentPercent < 100 - tolerance)
        {
            zoomPercentage = 100;
        }
        else
            zoomPercentage = 0; // Reset to 0 when exceeding 100%

        // Smoothly adjust to the new target field of view based on the updated zoom percentage
        float targetFOV = Mathf.Lerp(zoomRange.x, zoomRange.y, zoomPercentage / 100f);
        cam.fieldOfView = targetFOV;
        uiCam.fieldOfView = targetFOV;
    }

}
