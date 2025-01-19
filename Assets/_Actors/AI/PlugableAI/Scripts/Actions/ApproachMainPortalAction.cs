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
        Transform target = controller.target;
        controller.aiPath.destination = target.position;
    }
}
