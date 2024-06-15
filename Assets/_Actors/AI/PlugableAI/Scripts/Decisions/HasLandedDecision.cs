using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
[CreateAssetMenu(menuName = "PluggableAI/Decisions/Has Landed")]

public class HasLandedDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        if (controller.GetComponent<NavMeshAgent>().isOnNavMesh)
        {
            controller.rigidbodyRef.isKinematic = true;
            return true;
        }
        return false;
    }
}
