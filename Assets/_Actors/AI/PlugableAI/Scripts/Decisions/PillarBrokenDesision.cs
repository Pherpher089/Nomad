using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Decisions/BrokenPillar")]

public class PillarBrokenDesision : Decision
{
    public override bool Decide(StateController controller)
    {
        if (controller.GetComponent<FireHeadBoss>().m_TargetPillar == null)
        {
            controller.rigidbodyRef.isKinematic = false;
            return true;
        }
        return false;
    }
}
