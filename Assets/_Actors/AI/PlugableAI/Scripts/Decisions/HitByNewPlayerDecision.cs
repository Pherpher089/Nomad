using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Decisions/HitByNewPlayerDecision")]

public class HitByNewPlayerDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        if (controller.target != controller.lastTarget)
        {
            controller.lastTarget = controller.target;
            return true;
        }
        return false;
    }
}