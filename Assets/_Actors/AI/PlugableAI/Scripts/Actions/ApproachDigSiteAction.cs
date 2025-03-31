using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Actions/ApproachDigSite")]

public class ApproachDigSiteAction : Action
{
    public override void Act(StateController controller)
    {
        ApproachDigSite(controller);
    }

    private void ApproachDigSite(StateController controller)
    {
        controller.attackCoolDown = 0;
        controller.aiPath.destination = BeastManager.Instance.digTarget.transform.position;
        controller.target = BeastManager.Instance.digTarget.transform;
        if (controller.target != null)
        {
            controller.focusOnTarget = true;
            controller.aiPath.maxSpeed = controller.moveSpeed;
            controller.aiPath.endReachedDistance = 5f;
            // Check if the AI is in a "TakeHit" state
            if (controller.m_Animator.GetBool("TakeHit"))
            {
                // Stop moving while taking a hit
                controller.aiPath.destination = controller.transform.position;
            }
            else
            {
                // Ensure AIPath is enabled
                if (controller.aiPath.enabled)
                {
                    // Update the destination if the target is within a reasonable range
                    if (controller.name.ToLower().Contains("mamut"))
                    {
                        controller.aiPath.destination = controller.target.position;
                    }
                }
            }
        }
    }
}
