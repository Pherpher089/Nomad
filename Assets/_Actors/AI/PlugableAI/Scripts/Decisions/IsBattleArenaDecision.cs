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
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            controller.target = players[Random.Range(0, players.Length)].transform;
        }
        return isArena;
    }
}
