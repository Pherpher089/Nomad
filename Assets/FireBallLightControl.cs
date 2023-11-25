using System.Collections;
using UnityEngine;

public class FireBallLightControl : MonoBehaviour
{
    public Light explosionLight;
    public float maxIntensity = 5f;
    public float explosionDuration = 1f;

    void Start()
    {
        explosionLight = GetComponent<Light>();
        // Assuming the light starts off
        explosionLight.intensity = 0f;
        StartCoroutine(ExplosionLightEffect());
    }

    IEnumerator ExplosionLightEffect()
    {
        float halfDuration = explosionDuration / 2f;

        // Increase intensity
        for (float t = 0; t < halfDuration; t += Time.deltaTime)
        {
            explosionLight.intensity = Mathf.Lerp(0, maxIntensity, t / halfDuration);
            yield return null;
        }

        // Decrease intensity
        for (float t = 0; t < halfDuration; t += Time.deltaTime)
        {
            explosionLight.intensity = Mathf.Lerp(maxIntensity, 0, t / halfDuration);
            yield return null;
        }

        // Ensure the light is turned off after the effect
        explosionLight.intensity = 0f;
    }
}
