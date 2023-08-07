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
        controller.focusOnTarget = true;
        if (controller.target != null)
        {
            controller.aiMover.SetDestination(controller.target.position);
        }
    }
}