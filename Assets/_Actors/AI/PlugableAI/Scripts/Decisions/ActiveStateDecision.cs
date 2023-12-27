using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "PluggableAI/Decisions/ActiveState")]
public class ActiveStateDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        Debug.Log("makikng that chase desisinon");

        if (controller.target == null) return false;

        bool chaseTargetIsActive = controller.target.gameObject.activeSelf;
        return chaseTargetIsActive;
    }
}
