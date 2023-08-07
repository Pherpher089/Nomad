using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/FollowStick")]
public class FollowStickAction : Action
{
    public override void Act(StateController controller)
    {
        FollowStick(controller);
    }

    private void FollowStick(StateController controller)
    {
        controller.focusOnTarget = true;

        controller.aiMover.SetDestination(controller.target.position);
    }
}