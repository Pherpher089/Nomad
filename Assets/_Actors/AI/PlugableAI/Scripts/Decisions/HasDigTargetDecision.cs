using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Decisions/HasDigTarget")]

public class HasDigTargetDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        return BeastManager.Instance.isDigging && BeastManager.Instance.digTarget != null && !BeastManager.Instance.digTarget.hasBeenDug && BeastManager.Instance.m_GearIndices[1][1] == 9;
    }
}
