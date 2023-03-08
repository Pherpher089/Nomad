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
        if (controller.nextWayPoint < controller.wayPointList.Count)
        {
            //controller.navMeshAgent.destination = controller.wayPointList[controller.nextWayPoint].position;
            controller.aiPath.destination = controller.wayPointList[controller.nextWayPoint].position;
        }
        //controller.navMeshAgent.isStopped = false;
        controller.aiPath.isStopped = false;

        // if (controller.navMeshAgent.remainingDistance <= controller.navMeshAgent.stoppingDistance  /*&& !controller.navMeshAgent.pathPending*/)
        if (controller.aiPath.remainingDistance <= controller.aiPath.pickNextWaypointDist)
        {

            controller.nextWayPoint += 1;
        }
    }
}