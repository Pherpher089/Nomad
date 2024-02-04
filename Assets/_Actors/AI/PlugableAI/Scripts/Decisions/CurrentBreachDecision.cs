using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "PluggableAI/Decisions/CurrentBreachDecision")]
public class CurrentBreachDecision : Decision
{

    public override bool Decide(StateController controller)
    {
        return CheckForOpenings(controller);

    }

    private bool CheckForOpenings(StateController controller)
    {
        NavMeshPath path = new NavMeshPath();
        //controller.navMeshAgent.CalculatePath(controller.actorTarget.transform.position, path);
        if (path.status == NavMeshPathStatus.PathComplete)
        {
            //controller.target = controller.actorTarget.transform.position;
            return false;
        }

        return false;
    }
}
