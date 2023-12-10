using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Decisions/TargetTooCloseDecision")]

public class TargetTooCloseDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        return Vector3.Distance(controller.target.position, controller.transform.position) < 5;
    }

}
