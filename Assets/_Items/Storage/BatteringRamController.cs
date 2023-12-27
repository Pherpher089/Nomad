using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class BatteringRamController : BeastGear
{
    void OnTriggerEnter(Collider other)
    {
        if (beastManager == null) return;
        if (beastManager.m_RamTarget == null) return;
        if (other.CompareTag("Player") || other.CompareTag("Beast")) return;
        if (other.gameObject.name.Contains("BeastStable")) return;
        if (beastManager.GetComponent<StateController>().target == null) return;
        if (other.gameObject != beastManager.GetComponent<StateController>().target.gameObject) return;
        bool hit = false;
        if (other.TryGetComponent<HealthManager>(out var healthManager))
        {
            if (healthManager.health > 0)
            {
                hit = true;
                healthManager.Hit(50, ToolType.Default, other.ClosestPoint(transform.position), beastManager.gameObject);
            }
        }
        else if (other.TryGetComponent<SourceObject>(out var sourceObject))
        {
            if (sourceObject.hitPoints > 0)
            {
                hit = true;
                sourceObject.Hit(50, ToolType.Default, other.ClosestPoint(transform.position), beastManager.gameObject);
            }
        }

        if (hit) beastManager.GetComponent<AIPath>().Move(transform.forward * -3);
    }
}
