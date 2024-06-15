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
        controller.attackCoolDown = 0;
        if (controller.target != null)
        {
            if (controller.navMeshAgent.isOnNavMesh) controller.navMeshAgent.isStopped = false;
            controller.focusOnTarget = true;
            controller.navMeshAgent.speed = controller.moveSpeed;
            if (controller.m_Animator.GetBool("TakeHit"))
            {
                controller.navMeshAgent.SetDestination(controller.transform.position);
            }
            else
            {
                if (controller.navMeshAgent.isOnNavMesh)
                {
                    if (controller.navMeshAgent.remainingDistance < 30 || controller.navMeshAgent.remainingDistance > 30 && Vector3.Distance(controller.transform.position, controller.target.position) < 10)
                    {
                        controller.navMeshAgent.SetDestination(controller.target.position);
                    }
                }
            }
        }
    }
}