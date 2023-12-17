using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/Chase")]
public class ChaseAction : Action
{
    public override void Act(StateController controller)
    {
        Debug.Log("chasen 1");

        Chase(controller);
    }

    private void Chase(StateController controller)
    {
        Debug.Log("chasen 2");
        controller.aiPath.isStopped = false;
        controller.focusOnTarget = true;
        controller.aiMover.SetDestination(controller.target.position);
    }
}