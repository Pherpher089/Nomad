using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Decisions/BeastCamping")]

public class BeastCampingDecision : Decision
{

    public override bool Decide(StateController controller)
    {
        return IsCamping(controller);
    }

    private bool IsCamping(StateController controller)
    {
        return controller.GetComponent<BeastManager>().isCamping;
    }
}