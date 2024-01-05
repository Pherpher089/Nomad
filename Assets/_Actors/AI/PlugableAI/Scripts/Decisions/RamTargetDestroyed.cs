using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Decisions/RamTargetDestroyedDecision")]

public class RamTargetDestroyed : Decision
{
    public override bool Decide(StateController controller)
    {
        if (controller.target == null)
        {
            controller.navMeshAgent.speed /= 3;
            controller.navMeshAgent.stoppingDistance = 10;
            return true;
        }
        if (controller.target.TryGetComponent<EnemyManager>(out var enemyManager))
        {
            if (enemyManager.actorState == ActorState.Dead)
            {
                controller.navMeshAgent.speed /= 3;
                controller.GetComponent<BeastManager>().m_RamTarget = null;
                return true;
            }
        }
        return false;
    }
}
