using UnityEngine;
using UnityEngine.AI;

public class BatteringRamController : BeastGear
{
    Animator animator;

    void Start()
    {
        animator = BeastManager.Instance.GetComponent<Animator>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (beastManager == null) return;
        if (beastManager.m_RamTarget == null && !beastManager.hasDriver) return;
        if (!animator.GetBool("Ram") && beastManager.hasDriver) return;
        if (other.CompareTag("Player") || other.CompareTag("Beast")) return;
        if (other.gameObject.name.Contains("BeastStable")) return;
        if (other.TryGetComponent(out Item item))
        {
            if (item.isEquipped) return;
        }
        if (beastManager.GetComponent<StateController>().target == null && !beastManager.hasDriver) return;
        if (!beastManager.hasDriver && other.gameObject != beastManager.GetComponent<StateController>().target.gameObject) return;
        bool hit = false;
        if (other.TryGetComponent<HealthManager>(out var healthManager))
        {
            if (healthManager.health > 0)
            {
                hit = true;
                healthManager.Hit(50, ToolType.Beast, other.ClosestPoint(transform.position), beastManager.gameObject);
            }
        }
        else if (other.TryGetComponent<SourceObject>(out var sourceObject))
        {
            if (sourceObject.hitPoints > 0)
            {
                hit = true;
                sourceObject.Hit(50, ToolType.Beast, other.ClosestPoint(transform.position), beastManager.gameObject);
            }
        }

        if (hit) beastManager.GetComponent<NavMeshAgent>().Move(transform.forward * -3);
    }
}
