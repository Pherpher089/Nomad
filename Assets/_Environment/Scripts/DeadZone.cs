using Photon.Pun;
using UnityEngine;

public enum DeadZoneType { Water, Lava, Spikes }
public class DeadZone : MonoBehaviour
{
    public bool instantDeath = false;
    public float m_DamagePerSecond = 5;
    private float damageInterval = 1.0f;  // Damage every second
    private float timer = 0.0f;
    public DeadZoneType deadZoneType;
    private void Update()
    {
        // Increment the timer by the time passed since the last frame.
        timer += Time.deltaTime;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Enemy") && (deadZoneType == DeadZoneType.Spikes || deadZoneType == DeadZoneType.Water)) return;
        if (other.CompareTag("Beast") && deadZoneType == DeadZoneType.Lava && BeastManager.Instance.m_GearIndices[3][0] == 8) return;
        if (!other.CompareTag("Player") && deadZoneType == DeadZoneType.Water && other.GetComponent<ThirdPersonCharacter>().isRiding) return;
        if (other.transform.parent != null && other.transform.parent.gameObject.tag.Contains("Seat")) return;
        if (other.TryGetComponent<PhotonView>(out var pv) && !pv.IsMine) return;
        if (other.TryGetComponent<HealthManager>(out var healthManager) && healthManager.enabled)
        {
            healthManager.statusEffects.CallCatchFire(m_DamagePerSecond / 3, 5);
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
        if (!other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("Enemy") && (deadZoneType == DeadZoneType.Spikes || deadZoneType == DeadZoneType.Water)) return;
        if (other.gameObject.CompareTag("Beast") && deadZoneType == DeadZoneType.Lava && BeastManager.Instance.m_GearIndices[3][0] == 8) return;
        if (!other.gameObject.CompareTag("Player") && deadZoneType == DeadZoneType.Water && other.gameObject.GetComponent<ThirdPersonCharacter>().isRiding) return;
        if (other.transform.parent != null && other.transform.parent.gameObject.tag.Contains("Seat")) return;
        if (other.gameObject.TryGetComponent<PhotonView>(out var pv) && !pv.IsMine) return;
        if (other.collider.TryGetComponent<HealthManager>(out var healthManager) && healthManager.enabled)
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
