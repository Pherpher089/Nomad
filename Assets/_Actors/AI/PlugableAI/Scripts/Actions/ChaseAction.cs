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
            controller.aiPath.isStopped = false;
            controller.focusOnTarget = true;
            controller.aiMover.SetDestination(controller.target.position);
        }
    }
}