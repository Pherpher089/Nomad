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
        if (controller.target != null)
        {
            controller.navMeshAgent.isStopped = false;
            controller.focusOnTarget = true;
            if (controller.navMeshAgent.remainingDistance < 30 || controller.navMeshAgent.remainingDistance > 30 && Vector3.Distance(controller.transform.position, controller.target.position) < 10)
            {
                controller.navMeshAgent.SetDestination(controller.target.position);
            }
        }
    }
}