using UnityEngine;

public class HoverSpinEffect : MonoBehaviour
{
    public float hoverHeight = 0.5f; // The maximum height the object hovers.
    public float hoverSpeed = 2f; // Speed of the hover effect.
    public float spinSpeed = 50f; // Speed of the spin effect in degrees per second.

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Hovering
        float newY = Mathf.Sin(Time.time * hoverSpeed) * hoverHeight + startPosition.y;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        // Spinning
        //transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
    }
}
