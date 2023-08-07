using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Decisions/FollowStick")]
public class FollowStickDecision : Decision
{

    public override bool Decide(StateController controller)
    {
        if (FindObjectsOfType<BeastStick>().Length > 0)
        {
            controller.target = FindObjectsOfType<BeastStick>()[0].transform;
            return true;
        }
        controller.target = null;
        return false;
    }
}