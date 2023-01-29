using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/ApproachStructure")]
public class ApproachTargetStructureAct : Action {

    public override void Act(StateController controller)
    {
        Destroy(controller);
    }

    private void Destroy(StateController controller)
    {
        controller.focusOnTarget = true;
        controller.navMeshAgent.stoppingDistance = 0;
        Vector3 target = controller.chaseTarget;
        controller.navMeshAgent.destination = target;
    }
}
