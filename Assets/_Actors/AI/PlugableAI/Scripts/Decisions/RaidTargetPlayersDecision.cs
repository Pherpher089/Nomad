using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Decisions/RaidTargetPlayers")]

public class RaidTargetPlayersDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        if (EnemiesManager.Instance.GetEnemyIndex(controller.enemyManager) % 3 == 0)
        {
            controller.target = PlayersManager.Instance.playerList[UnityEngine.Random.Range(0, PlayersManager.Instance.playerList.Count)].transform;
            return true;
        }
        else
        {
            return false;
        }
    }
}
