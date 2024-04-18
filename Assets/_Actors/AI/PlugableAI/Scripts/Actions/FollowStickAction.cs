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
        if (controller.GetComponent<Animator>().GetBool("Eating"))
        {
            controller.aiMover.SetDestination(controller.transform.position);
            return;
        }
        controller.focusOnTarget = true;

        controller.aiMover.SetDestination(controller.target.position);
    }
}