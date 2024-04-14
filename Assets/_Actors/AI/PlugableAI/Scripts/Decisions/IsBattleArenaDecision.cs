using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
[CreateAssetMenu(menuName = "PluggableAI/Decisions/BattleArenaDecision")]
public class IsBattleArenaDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        bool isArena = SceneManager.GetActiveScene().name == "BattleArena";
        if (isArena)
        {
            controller.target = PlayersManager.Instance.playerList[Random.Range(0, PlayersManager.Instance.playerList.Count)].transform;
        }
        return isArena;
    }
}
