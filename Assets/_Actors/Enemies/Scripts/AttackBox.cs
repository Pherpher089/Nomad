using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBox : MonoBehaviour
{
    Animator animator;
    List<Collider> alreadyHit;
    void Start()
    {
        alreadyHit = new List<Collider>();
        animator = transform.parent.GetComponentInChildren<Animator>();
    }
    void OnTriggerStay(Collider other)
    {
        // if (!animator.GetBool("Attacking")) return;
        if (other.CompareTag("Enemy")) return;
        if (other.TryGetComponent(out Item item))
        {
            if (item.isEquipped) return;
        }
        if (!animator.GetBool("Attacking"))
        {
            alreadyHit = new List<Collider>();
            return;
        }
        if (alreadyHit.Contains(other))
        {
            return;
        }
        else
        {
            alreadyHit.Add(other);
        }
        if (other.TryGetComponent<HealthManager>(out var healthManager))
        {
            if (healthManager.health > 0)
            {
                healthManager.Hit(10, ToolType.Beast, other.transform.position, transform.parent.gameObject);
            }
        }

        else if (other.TryGetComponent<SourceObject>(out var sourceObject))
        {
            if (sourceObject.hitPoints > 0)
            {
                sourceObject.Hit(10, ToolType.Beast, other.ClosestPoint(transform.position), transform.parent.gameObject);
            }
        }
    }
}
