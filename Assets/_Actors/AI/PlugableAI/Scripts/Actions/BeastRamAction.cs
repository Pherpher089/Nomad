using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Actions/BeastRamAction")]

public class BeastRamAction : Action
{
    bool ramming = false;
    Vector3 restartLocation;
    public override void Act(StateController controller)
    {
        if (controller.aiPath.remainingDistance < 2)
        {
            ramming = !ramming;
            if (!ramming)
            {
                //Maybe a better place for this method?
                restartLocation = ActorUtils.GetRandomValidSpawnPoint(10, controller.transform.position);
            }
        }

        if (controller.target != null)
        {
            if (ramming)
            {
                // if (!animator.GetBool("Ram") && controller.navMeshAgent.remainingDistance < 8)
                // {
                //     animator.SetBool("Ram", true);
                // }
                controller.aiPath.destination = controller.target.position;
            }
            else
            {
                controller.aiPath.destination = restartLocation;
            }
            controller.aiPath.isStopped = false;
            controller.focusOnTarget = true;
        }
    }
}
