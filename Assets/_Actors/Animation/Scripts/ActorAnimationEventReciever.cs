using Photon.Pun;
using UnityEngine;

public class ActorAnimationEventReciever : MonoBehaviour
{
    public ActorEquipment actorEquipment;
    public ActorAudioManager audioManager;
    HungerManager hungerManager;
    Animator animator;
    ThirdPersonCharacter character;
    void Start()
    {
        character = GetComponentInParent<ThirdPersonCharacter>();
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
    public void EndRoll()
    {
        animator.SetBool("Rolling", false);
    }
    public void Land()
    {
        character.m_JumpedWhileSprinting = false;
        //quieting errors
    }
    public void Hit()
    {
        audioManager.PlayAttack();
        animator.SetBool("CanHit", true);
        try
        {

            ToolItem tool = transform.parent.gameObject.GetComponent<ActorEquipment>().equippedItem.GetComponent<ToolItem>();
            if (tool != null)
            {
                tool.Hit();
            }
            else
            {
                Debug.LogWarning("Tool reference not set in AnimationEventReceiver.");
            }
        }
        catch
        {
            Debug.LogWarning("Tool reference failed.");
        }

        //Check for these hands if no weapon
        try
        {

            TheseHands hands = transform.parent.gameObject.GetComponent<ActorEquipment>().m_TheseHandsArray[0].GetComponent<TheseHands>();
            TheseFeet feet = transform.parent.gameObject.GetComponent<ActorEquipment>().m_TheseHandsArray[0].GetComponent<TheseFeet>();

            if (hands != null && animator.GetInteger("ItemAnimationState") == 0)
            {
                hands.Hit();
            }
            else if (feet != null && animator.GetInteger("ItemAnimationState") == 4)
            {
                hands.Hit();
            }


            hands = transform.parent.gameObject.GetComponent<ActorEquipment>().m_TheseHandsArray[1].GetComponent<TheseHands>();
            if (hands != null && animator.GetInteger("ItemAnimationState") == 0)
            {
                hands.Hit();
            }
            else if (feet != null && animator.GetInteger("ItemAnimationState") == 4)
            {
                hands.Hit();
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

    public void Shoot()
    {
        if (animator.transform.parent.GetComponent<PhotonView>().IsMine)
        {
            animator.transform.parent.gameObject.GetComponent<ActorEquipment>().ShootBow();
        }
    }
    public void Cast()
    {
        if (animator.transform.parent.GetComponent<PhotonView>().IsMine)
        {
            animator.transform.parent.gameObject.GetComponent<ActorEquipment>().CastWand();

        }
    }
    public void Cast2()
    {
        if (animator.transform.parent.GetComponent<PhotonView>().IsMine)
        {
            animator.transform.parent.gameObject.GetComponent<ActorEquipment>().CastWandArc();

        }
    }

    public void EndRam()
    {
        animator.SetBool("Ram", false);
    }
    public void EndEatMamut()
    {
        animator.SetBool("Eating", false);
    }
}
