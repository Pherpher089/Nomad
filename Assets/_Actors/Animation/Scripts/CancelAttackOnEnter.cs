using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CancelAttackOnEnter : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("Attacking", false);
        animator.SetBool("CoolDown", false);

    }
}
