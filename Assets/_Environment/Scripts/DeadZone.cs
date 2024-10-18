using UnityEngine;

public class DeadZone : MonoBehaviour
{
    public bool instantDeath = false;
    public float m_DamagePerSecond = 5;
    private float damageInterval = 1.0f;  // Damage every second
    private float timer = 0.0f;

    private void Update()
    {
        // Increment the timer by the time passed since the last frame.
        timer += Time.deltaTime;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<HealthManager>(out var healthManager))
        {
            if (instantDeath)
            {
                healthManager.TakeHit(healthManager.health);
            }
            else
            {
                // Check if the timer exceeds the interval to apply damage.
                if (timer >= damageInterval)
                {
                    healthManager.TakeHit(m_DamagePerSecond);
                    // For spike barriers
                    if (TryGetComponent<SourceObject>(out var sourceObject))
                    {
                        sourceObject.Hit(25, ToolType.Default, other.transform.position, other.gameObject);
                    }
                    timer = 0.0f;  // Reset the timer after applying damage.
                }
            }


        }
    }

    // Optional: Use this only if you want collision-based damage instead of trigger.
    private void OnCollisionStay(Collision other)
    {
        if (other.collider.TryGetComponent<HealthManager>(out var healthManager))
        {
            if (instantDeath)
            {
                healthManager.TakeHit(healthManager.health);
            }
            else
            {
                if (timer >= damageInterval)
                {
                    healthManager.TakeHit(m_DamagePerSecond);
                    timer = 0.0f;
                }
            }
        }
    }
}
