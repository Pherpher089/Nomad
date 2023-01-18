using UnityEngine;
using System.Collections.Generic;

public class AttackBehavior : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Set the "LeftAttack" parameter to true when the state is entered
        //animator.SetBool("LeftAttack", true);
        TheseHands[] theseHands = animator.gameObject.GetComponentsInChildren<TheseHands>();
        foreach (TheseHands th in theseHands)
        {
            th.m_HaveHit = new List<Collider>();
        }
        ActorEquipment ae = animator.gameObject.GetComponentInParent<ActorEquipment>();
        if (ae != null && ae.hasItem)
        {
            try
            {
                Tool item = ae.equipedItem as Tool;
                item.m_HaveHit = new List<Collider>();
            }
            catch
            {
                Debug.Log("Item not a tool");
            }
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // If the animation is 75% complete
        if (stateInfo.normalizedTime >= 0.75f)
        {
            // Set the "LeftAttack" parameter to false
            animator.SetBool("Attacking", false);
        }
    }
}
