using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/Chase")]
public class ChaseAction : Action
{
    public override void Act(StateController controller)
    {
        Chase(controller);
    }

    private void Chase(StateController controller)
    {
        controller.attackCoolDown = 0;

        if (controller.target != null)
        {
            controller.focusOnTarget = true;
            controller.aiPath.maxSpeed = controller.moveSpeed;

            // Check if the AI is in a "TakeHit" state
            if (!controller.name.ToLower().Contains("mamut") && controller.m_Animator.GetBool("TakeHit"))
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
                    if (Vector3.Distance(controller.transform.position, controller.target.position) < 50 || controller.name.ToLower().Contains("mamut"))
                    {
                        controller.aiPath.destination = controller.target.position;
                    }
                }
            }
        }
    }
}
