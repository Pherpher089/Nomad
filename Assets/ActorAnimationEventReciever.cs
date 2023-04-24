using UnityEngine;

public class ActorAnimationEventReciever : MonoBehaviour
{
    public ActorEquipment weaponController;
    Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }
    public void StartMove()
    {
        animator.SetBool("AttackMove", true);
    }
    public void Hit()
    {
        GetComponent<Animator>().SetBool("AttackMove", false);
        try
        {

            Tool tool = transform.parent.gameObject.GetComponent<ActorEquipment>().equippedItem.GetComponent<Tool>();
            if (tool != null)
            {
                tool.Hit();
            }
            else
            {
                Debug.LogError("Tool reference not set in AnimationEventReceiver.");
            }
        }
        catch
        {
            Debug.LogError("Tool reference failed.");
        }

        //Check for these hands if no weapon
        try
        {

            TheseHands hands = transform.parent.gameObject.GetComponent<ActorEquipment>().m_TheseHandsArray[0].GetComponent<TheseHands>();
            if (hands != null)
            {
                hands.Hit();
            }
            else
            {
                Debug.LogError("These Hands reference not set in AnimationEventReceiver.");
            }

            hands = transform.parent.gameObject.GetComponent<ActorEquipment>().m_TheseHandsArray[1].GetComponent<TheseHands>();
            if (hands != null)
            {
                hands.Hit();
            }
            else
            {
                Debug.LogError("These Hands reference not set in AnimationEventReceiver.");
            }
        }
        catch
        {
            Debug.LogError("These Hands reference failed.");
        }
    }
}
