using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Decisions/TargetFarEnoughDecision")]

public class TargetFarEnoughDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        return Vector3.Distance(controller.target.position, controller.transform.position) > controller.enemyStats.attackRange - (controller.enemyStats.attackRange * 0.25f) || controller.aiPath.remainingDistance < 2;
    }

}
