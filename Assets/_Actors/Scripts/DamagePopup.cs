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
        if (damageAmount % 1 != 0)
        {
            textMesh.SetText(damageAmount.ToString("F2"));
        }
        else
        {
            textMesh.SetText(damageAmount.ToString());
        }
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
