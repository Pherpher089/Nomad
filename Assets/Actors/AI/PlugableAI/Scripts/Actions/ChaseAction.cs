using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/Chase")]
public class ChaseAction : Action
{
    public override void Act(StateController controller)
    {
        Chase(controller);
    }

    private void Chase(StateController controller)
    {

        if (controller.chaseTarget != controller.actorTarget.transform.position)
        {
            controller.chaseTarget = controller.actorTarget.transform.position;
        }
        controller.focusOnTarget = true;
        // controller.navMeshAgent.destination = controller.chaseTarget;
        // controller.navMeshAgent.isStopped = false;
        controller.aiPath.destination = controller.chaseTarget;
        controller.aiPath.isStopped = false;

        Debug.Log("### chasing");
    }
}