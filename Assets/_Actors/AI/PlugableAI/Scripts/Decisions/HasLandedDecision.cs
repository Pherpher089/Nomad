using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Decisions/Has Landed")]

public class HasLandedDecision : Decision
{
    int counter = 0;
    public override bool Decide(StateController controller)
    {
        if (controller.isGrounded || counter > 30)
        {
            controller.rigidbodyRef.isKinematic = true;
            return true;
        }
        counter++;
        return false;
    }
}
