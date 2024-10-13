using UnityEngine;

public class DeadZone : MonoBehaviour
{
    public bool instantDeath = false;
    public float m_DamagePerSecond = 5;
    int counter = 0;

    void Update()
    {
        if (!instantDeath && counter >= 0)
        {
            counter += 1;
        }
    }
    public void OnCollisionStay(Collision other)
    {
        if (other.collider.TryGetComponent<HealthManager>(out var healthManager))
        {
            if (instantDeath)
            {
                healthManager.TakeHit(healthManager.health);
            }
            else
            {
                if (counter < 0) counter = 0;
                if (counter % 30 == 0)
                {
                    healthManager.TakeHit(m_DamagePerSecond);
                }
            }
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<HealthManager>(out var healthManager))
        {
            if (instantDeath)
            {
                healthManager.TakeHit(healthManager.health);
            }
            else
            {
                if (counter < 0) counter = 0;
                if (counter % 30 == 0)
                {
                    healthManager.TakeHit(m_DamagePerSecond);
                }
            }
        }
    }
}
