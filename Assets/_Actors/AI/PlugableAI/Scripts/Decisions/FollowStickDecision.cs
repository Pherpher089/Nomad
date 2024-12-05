using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Decisions/FollowStick")]
public class FollowStickDecision : Decision
{

    public override bool Decide(StateController controller)
    {
        if (FindObjectsOfType<BeastStick>().Length > 0 && !controller.GetComponent<BeastManager>().m_IsInStable)
        {
            BeastStick[] allBeastSticks = FindObjectsOfType<BeastStick>();
            foreach (BeastStick stick in allBeastSticks)
            {
                if (stick.GetComponent<MeshRenderer>().enabled)
                {
                    controller.target = stick.transform;
                }
            }
            return true;
        }
        controller.target = null;
        return false;
    }
}