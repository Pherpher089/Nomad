using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Decisions/IsDigComplete")]
public class IsDigCompleteDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        return BeastManager.Instance.digTarget == null || BeastManager.Instance.digTarget.hasBeenDug;
    }
}
