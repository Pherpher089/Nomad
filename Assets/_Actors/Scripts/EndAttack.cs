using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class AttackBehavior : StateMachineBehaviour
{
    bool hasTurnedOffCooldown = false;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Cooldown and attack flags
        animator.SetBool("CoolDown", true);
        animator.SetBool("Attacking", true);
        animator.SetBool("CanHit", false);
        hasTurnedOffCooldown = false;

        // Reset hit lists
        ResetHitLists(animator);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Turn off cooldown at 45% of the animation
        if (stateInfo.normalizedTime >= 0.45f && hasTurnedOffCooldown == false)
        {
            animator.SetBool("CoolDown", false);
            hasTurnedOffCooldown = true;
        }

        // Reset "Attacking" at the end of the animation
        if (stateInfo.normalizedTime >= 0.95f)
        {
            animator.SetBool("Attacking", false);
        }
    }

    private void ResetHitLists(Animator animator)
    {
        AttackManager attackManager = animator.gameObject.GetComponentInParent<AttackManager>();
        if (attackManager != null)
        {
            attackManager.ResetHitbox(); // Add a method to clear m_HaveHit in AttackManager
        }
    }

}
