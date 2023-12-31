using System.ComponentModel;
using UnityEngine;

public class CompassIconController : MonoBehaviour
{
    [Header("NOTE: for best results, this object should not have a parent object.")]
    [Tooltip("offset from the edge of the screen when icon is tracking off screen object.")]
    public float edgeMargin = 0.05f; // Margin from the edge of the screen
    [Tooltip("The distance from the camera the icon should appear. The higher the number, the smaller the icon will get.")]
    public float iconDepth = 20f; // Distance from the camera
    [Tooltip("The number of standard units(meters) above the tracked object should the icon will appear. ")]
    public float heightOffset = 5f;
    [Tooltip("The object that the icon is tracking")]
    public Transform trackedObjectTransform;

    void Update()
    {
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(trackedObjectTransform.position + new Vector3(0, heightOffset, 0));
        bool isVisible = viewportPoint.x >= 0 && viewportPoint.x <= 1
                 && viewportPoint.y >= 0 && viewportPoint.y <= 1
                 && viewportPoint.z > 0;

        if (!isVisible)
        {
            viewportPoint.x = Mathf.Clamp(viewportPoint.x, edgeMargin, 1 - edgeMargin);
            viewportPoint.y = Mathf.Clamp(viewportPoint.y, edgeMargin, 1 - edgeMargin);
        }

        Vector3 newWorldPosition = Camera.main.ViewportToWorldPoint(new Vector3(viewportPoint.x, viewportPoint.y, iconDepth));
        transform.position = newWorldPosition;
    }
}
