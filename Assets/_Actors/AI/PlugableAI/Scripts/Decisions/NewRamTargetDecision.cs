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
            controller.aiPath.maxSpeed *= 3;
            controller.aiPath.endReachedDistance = 0;
            return true;
        }

        return false;
    }
}
