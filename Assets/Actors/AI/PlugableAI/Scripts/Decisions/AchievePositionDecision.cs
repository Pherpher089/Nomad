using UnityEngine;
using System.Collections;
using System;

[CreateAssetMenu(menuName = "PluggableAI/Decisions/AchievePositionDecision")]

public class AchievePositionDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        return CheckPosition(controller);
    }

    private bool CheckPosition(StateController controller)
    {
        //controller.navMeshAgent.stoppingDistance = 0;
        controller.aiPath.endReachedDistance = 0;
        // if (controller.navMeshAgent.remainingDistance <= controller.navMeshAgent.stoppingDistance && !controller.navMeshAgent.pathPending)
        if (controller.aiPath.remainingDistance <= controller.aiPath.endReachedDistance && !controller.aiPath.pathPending)
        {
            return true;
        }

        return false;
    }
}
