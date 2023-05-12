using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "PluggableAI/Decisions/ActiveState")]
public class ActiveStateDecision : Decision
{
    public override bool Decide(StateController controller)
    {

        bool chaseTargetIsActive = controller.target.gameObject.activeSelf;
        return chaseTargetIsActive;
    }
}
