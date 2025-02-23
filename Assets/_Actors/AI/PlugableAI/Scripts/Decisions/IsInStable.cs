using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Decisions/IsInStable")]

public class IsInStable : Decision
{
    public override bool Decide(StateController controller)
    {
        if (controller.GetComponent<BeastManager>().m_IsInStable)
        {
            return true;
        }
        else return false;
    }
}
