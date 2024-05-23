using UnityEngine;
using System.Collections.Generic;

public class AttackBehavior : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    bool hasTurnedOffCooldown = false;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //animator.SetBool("AttackMove", true);
        animator.SetBool("CoolDown", true);
        animator.SetBool("Attacking", true);
        animator.SetBool("CanHit", false);
        hasTurnedOffCooldown = false;
        TheseHands[] theseHands = animator.gameObject.GetComponentsInChildren<TheseHands>();
        foreach (TheseHands th in theseHands)
        {
            th.GetComponent<Collider>().enabled = true;
            th.m_HaveHit = new List<Collider>();
        }
        ActorEquipment ae = animator.gameObject.GetComponentInParent<ActorEquipment>();
        if (ae != null && ae.hasItem)
        {
            try
            {
                ToolItem item = ae.equippedItem.GetComponent<Item>() as ToolItem;
                item.m_HaveHit = new List<Collider>();
            }
            catch
            {
                //Debug.Log("**Item not a tool**");
            }
            try
            {
                BeastStick beastStick = ae.equippedItem.gameObject.GetComponent<BeastStick>();
                beastStick.m_HaveHit = new List<Collider>();
            }
            catch
            {
                //Debug.Log("**Item not a tool**");
            }
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {


        if (stateInfo.normalizedTime >= .1f)
        {
            // Set the "LeftAttack" parameter to false
        }

        // If the animation is 75% complete
        if (stateInfo.normalizedTime >= 0.45f && hasTurnedOffCooldown == false)
        {
            // Set the "LeftAttack" parameter to false
            animator.SetBool("CoolDown", false);
            hasTurnedOffCooldown = true;

        }
        if (stateInfo.normalizedTime >= .95f)
        {
            // Set the "LeftAttack" parameter to false
            animator.SetBool("Attacking", false);
        }
    }

    // override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    // {
    //     animator.SetBool("Attacking", false);
    // }
}
