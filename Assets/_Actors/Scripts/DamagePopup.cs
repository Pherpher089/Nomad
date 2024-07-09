using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public float disappearTimer = 1f;
    public float moveYSpeed = 1f;
    public Vector3 offset = new Vector3(0, 2, 0);

    private TextMeshPro textMesh;

    void Awake()
    {
        textMesh = transform.GetComponent<TextMeshPro>();
    }

    public void Setup(float damageAmount)
    {
        textMesh.SetText(damageAmount.ToString("F2"));
        // Adjust color and other properties if needed
    }
    public void Setup(string message, Color color)
    {
        textMesh.color = color;
        textMesh.SetText(message);
        // Adjust color and other properties if needed
    }

    void Update()
    {
        transform.position += new Vector3(0, moveYSpeed * Time.deltaTime, 0);

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            float disappearSpeed = 3f;
            textMesh.alpha -= disappearSpeed * Time.deltaTime;
            if (textMesh.alpha < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
