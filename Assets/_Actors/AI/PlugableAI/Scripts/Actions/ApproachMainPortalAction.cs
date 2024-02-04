using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Actions/ApproachMainPortal")]

public class ApproachMainPortalAction : Action
{
    public override void Act(StateController controller)
    {
        Approach(controller);
    }

    public void Approach(StateController controller)
    {
        controller.focusOnTarget = true;
        controller.target = GameObject.FindGameObjectWithTag("MainPortal").transform;
        Vector3 target = controller.target.position;
        controller.navMeshAgent.SetDestination(target);
    }
}
