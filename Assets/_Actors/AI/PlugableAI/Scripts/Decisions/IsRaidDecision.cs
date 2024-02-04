using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Decisions/IsRaid")]

public class IsRaidDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        return GameStateManager.Instance.isRaid;
    }
}
