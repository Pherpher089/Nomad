using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Decisions/NewRamTargetDecision")]

public class NewRamTargetDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        BeastManager bm = controller.GetComponent<BeastManager>();
        if (bm.m_GearIndex == 0 && bm.m_RamTarget != null)
        {
            controller.target = bm.m_RamTarget.transform;
            controller.navMeshAgent.speed *= 3;
            controller.navMeshAgent.stoppingDistance = 0;
            return true;
        }

        return false;
    }
}
