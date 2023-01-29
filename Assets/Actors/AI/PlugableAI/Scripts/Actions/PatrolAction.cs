using UnityEngine.AI;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/Patrol")]
public class PatrolAction : Action
{
    public override void Act(StateController controller)
    {
        Patrol(controller);
    }

    private void Patrol(StateController controller)
    {
        controller.focusOnTarget = false;
        if(controller.nextWayPoint < controller.wayPointList.Count)
        {
            controller.navMeshAgent.destination = controller.wayPointList[controller.nextWayPoint].position;
        }
        controller.navMeshAgent.isStopped = false;

        if (controller.navMeshAgent.remainingDistance <= controller.navMeshAgent.stoppingDistance  /*&& !controller.navMeshAgent.pathPending*/)
        {

            controller.nextWayPoint += 1;
        }
    }
}