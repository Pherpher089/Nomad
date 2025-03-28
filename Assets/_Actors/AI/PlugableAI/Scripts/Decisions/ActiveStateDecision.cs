using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "PluggableAI/Decisions/ActiveState")]
public class ActiveStateDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        if (controller.target == null) return false;

        bool chaseTargetIsActive = controller.target.gameObject.activeSelf;

        ActorManager actorManager = controller.target.GetComponent<ActorManager>();
        if (chaseTargetIsActive) chaseTargetIsActive = actorManager && actorManager.actorState != ActorState.Dead;

        return chaseTargetIsActive;
    }
}
