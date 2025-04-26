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
        if (GameStateManager.Instance.raidTarget.TryGetComponent<RestorationSiteUIController>(out var restorationSiteUIController))
        {
            controller.target = BeastManager.Instance.transform;
        }
        else
        {
            controller.target = GameStateManager.Instance.raidTarget;
        }
        Transform target = controller.target;
        controller.aiPath.destination = target.position;
    }
}
