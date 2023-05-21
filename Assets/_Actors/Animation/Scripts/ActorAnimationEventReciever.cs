using UnityEngine;

public class ActorAnimationEventReciever : MonoBehaviour
{
    public ActorEquipment actorEquipment;
    public ActorAudioManager audioManager;
    HungerManager hungerManager;
    Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
        audioManager = GetComponentInParent<ActorAudioManager>();
        actorEquipment = GetComponentInParent<ActorEquipment>();
        hungerManager = GetComponentInParent<HungerManager>();
    }
    public void StartMove()
    {
        animator.SetBool("AttackMove", true);

    }
    public void FootL()
    {
        audioManager.PlayStep();
    }
    public void FootR()
    {
        audioManager.PlayStep();
    }
    public void EndMove()
    {
        animator.SetBool("AttackMove", false);
    }
    public void Hit()
    {
        audioManager.PlayAttack();
        animator.SetBool("CanHit", true);
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
    public void EndHit()
    {
        animator.SetBool("CanHit", false);

    }
    public void Eat()
    {
        hungerManager.Eat(actorEquipment.equippedItem.GetComponent<Food>().foodValue);
        actorEquipment.equippedItem.GetComponent<Food>().PrimaryAction(1);
    }

    public void EndEat()
    {
        animator.SetLayerWeight(2, 0);
        animator.SetBool("Eating", false);

    }
}
