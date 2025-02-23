using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Decisions/Is Corral Gate Open")]

public class IsCorralGateOpen : Decision
{
    public override bool Decide(StateController controller)
    {
        bool isGateOpen;

        isGateOpen = BeastManager.Instance.m_BeastStableController.isCorralDoorOpen;
        if (isGateOpen)
        {
            BeastManager.Instance.m_IsInStable = false;
        }

        return isGateOpen;
    }
}
