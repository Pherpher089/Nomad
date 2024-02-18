using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Decisions/IsTutorialBeastAttackDecision")]


public class IsTutorialBeastAttackDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        GameObject beast = GameObject.FindGameObjectWithTag("Beast");

        if (beast != null && Vector3.Distance(GameStateManager.Instance.playersManager.playersCentralPosition, beast.transform.position) < 60)
        {
            controller.target = beast.transform;
            return true;
        }
        return false;
    }

}
