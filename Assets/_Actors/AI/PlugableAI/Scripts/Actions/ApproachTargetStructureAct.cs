using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/ApproachStructure")]
public class ApproachTargetStructureAct : Action
{

    public override void Act(StateController controller)
    {
        ApproachStructure(controller);
    }

    private void ApproachStructure(StateController controller)
    {

    }
}
