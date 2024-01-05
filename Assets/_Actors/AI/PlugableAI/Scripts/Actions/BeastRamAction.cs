using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Actions/BeastRamAction")]

public class BeastRamAction : Action
{
    bool ramming = false;
    Vector3 restartLocation;
    public override void Act(StateController controller)
    {
        if (controller.navMeshAgent.velocity == Vector3.zero && controller.navMeshAgent.remainingDistance < 1)
        {
            ramming = !ramming;
            if (!ramming)
            {
                //Maybe a better place for this method
                restartLocation = WanderAction.PickAPoint(controller, 10);
            }
        }

        if (controller.target != null)
        {
            if (ramming)
            {
                controller.aiMover.SetDestination(controller.target.position);
            }
            else
            {
                controller.aiMover.SetDestination(restartLocation);
            }
            controller.navMeshAgent.isStopped = false;
            controller.focusOnTarget = true;
        }
    }
}
