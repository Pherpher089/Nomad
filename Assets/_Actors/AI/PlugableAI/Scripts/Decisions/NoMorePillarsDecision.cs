using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Decisions/NoMorePillarsDecision")]
public class NoMorePillarsDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        return GameObject.FindGameObjectsWithTag("Pillar").Length <= 0;
    }

}
