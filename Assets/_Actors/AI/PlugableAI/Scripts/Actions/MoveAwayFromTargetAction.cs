using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAwayFromTargetAction : Action
{
    public override void Act(StateController stateController)
    {
        stateController.navMeshAgent.Move((stateController.target.position - stateController.transform.position).normalized);
    }
}
