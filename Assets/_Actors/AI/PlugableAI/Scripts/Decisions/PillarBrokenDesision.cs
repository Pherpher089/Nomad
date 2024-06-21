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
            // controller.navMeshAgent.enabled = false;
            // controller.transform.GetChild(0).GetComponent<Rigidbody>().isKinematic = false;
            Debug.Log("### turning off isKinematic: " + controller.rigidbodyRef.isKinematic);
            return true;
        }
        return false;
    }
}
