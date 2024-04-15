using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadZone : MonoBehaviour
{
    public bool instantDeath = false;
    public float m_DamagePerSecond = 5;
    int counter = -1;


    void Update()
    {
        if (!instantDeath && counter >= 0)
        {
            counter += 1;
        }
    }

    public void OnCollisionStay(Collision other)
    {
        if (other.gameObject.tag == "Player")
        {
            HealthManager healthManager = other.gameObject.GetComponent<HealthManager>();
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
