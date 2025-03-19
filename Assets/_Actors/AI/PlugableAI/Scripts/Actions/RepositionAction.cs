using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Actions/Reposition")]

public class RepositionAction : Action
{
    public override void Act(StateController controller)
    {
        Reposition(controller);
    }

    private void Reposition(StateController controller)
    {
        if (controller.target == null || controller.aiPath == null)
        {
            return;
        }

        Vector3 directionAwayFromTarget = (controller.transform.position - controller.target.position).normalized;

        // Calculate the new position in the opposite direction
        Vector3 newPosition = controller.transform.position + directionAwayFromTarget * controller.enemyStats.attackRange;

        // Set the destination for the NavMeshAgent
        Debug.Log("### setting new position: " + newPosition);
        controller.aiPath.destination = newPosition;

    }
}
