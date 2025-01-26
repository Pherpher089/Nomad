using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class BatteringRamController : BeastGear
{
    Animator animator;
    BeastManager beastManager;
    void Start()
    {
        beastManager = BeastManager.Instance;
        animator = BeastManager.Instance.GetComponent<Animator>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.tag is "Player" or "Beast") return;
        if (beastManager == null) return;
        if (beastManager.m_RamTarget == null && !beastManager.hasDriver) return;
        if (!animator.GetBool("Ram") && beastManager.hasDriver) return;
        if (other.CompareTag("Player") || other.CompareTag("Beast") || other.CompareTag("MainPortal")) return;
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
                healthManager.Hit(50, ToolType.Beast, other.transform.position, beastManager.gameObject, 50);
            }
        }
        HealthManager parentHealthManager = other.GetComponentInParent<HealthManager>();
        if (parentHealthManager != null)
        {

            if (parentHealthManager.health > 0)
            {
                hit = true;
                parentHealthManager.Hit(50, ToolType.Beast, other.transform.position, beastManager.gameObject, 50);
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
        SourceObject parentSrouceObject = other.GetComponentInParent<SourceObject>();
        if (parentSrouceObject != null)
        {

            if (parentSrouceObject.hitPoints > 0)
            {
                hit = true;
                parentSrouceObject.Hit(50, ToolType.Beast, other.ClosestPoint(transform.position), beastManager.gameObject);
            }
        }
        if (hit && !beastManager.hasDriver) beastManager.GetComponent<NavMeshAgent>().Move(transform.forward * -3);
        if (hit && beastManager.hasDriver) beastManager.BeastMove(transform.forward * -3, false);
    }
}
