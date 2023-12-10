using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Decisions/BrokenPillar")]

public class PillarBrokenDesision : Decision
{
    public override bool Decide(StateController controller)
    {
        return controller.transform.position.y < 1;
    }
}
