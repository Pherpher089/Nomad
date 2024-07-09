using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBox : MonoBehaviour
{
    Animator animator;
    List<Collider> inRange;
    void Start()
    {
        inRange = new List<Collider>();
        animator = transform.parent.GetComponentInChildren<Animator>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (inRange.Contains(other))
        {
            return;
        }
        else
        {
            inRange.Add(other);
        }
    }
    void OnTriggerExit(Collider other)
    {

        if (inRange.Contains(other))
        {
            inRange.Remove(other);
        }
    }



    public void Bite()
    {
        foreach (Collider other in inRange)
        {
            if (other.CompareTag("Enemy")) return;
            if (other.TryGetComponent(out Item item))
            {
                if (item.isEquipped) return;
            }

            if (other.TryGetComponent<HealthManager>(out var healthManager))
            {
                if (healthManager.health > 0)
                {
                    healthManager.Hit(10, ToolType.Beast, other.transform.position, transform.parent.gameObject, 0);
                }
            }

            else if (other.TryGetComponent<SourceObject>(out var sourceObject))
            {
                if (sourceObject.hitPoints > 0)
                {
                    sourceObject.Hit(10, ToolType.Beast, other.GetComponent<Collider>().ClosestPoint(transform.position), transform.parent.gameObject);
                }
            }
        }
    }
}
