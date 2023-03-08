using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/AttackActor")]

public class AttackActorTargetAct : Action
{

    float coolDown = 0;


    public override void Act(StateController controller)
    {
        AttackActor(controller);
    }

    private void AttackActor(StateController controller)
    {
        //controller.navMeshAgent.stoppingDistance = controller.enemyStats.attackRange;
        controller.aiPath.endReachedDistance = controller.enemyStats.attackRange;
        controller.focusOnTarget = true;

        if (coolDown > 0)
        {
            coolDown -= 2 * Time.deltaTime;
            controller.enemyManager.Attack(true, false);
        }
        else
        {

            if (controller.enemyManager)
            {
                controller.enemyManager.Attack(true, false);
            }
            coolDown = controller.enemyStats.attackRate;
        }
    }
}

