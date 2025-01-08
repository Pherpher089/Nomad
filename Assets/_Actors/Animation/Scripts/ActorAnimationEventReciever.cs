using System;
using System.Numerics;
using Photon.Pun;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class ActorAnimationEventReceiver : MonoBehaviour
{
    public ActorEquipment actorEquipment;
    public ActorAudioManager audioManager;
    HungerManager hungerManager;
    Animator animator;
    ThirdPersonCharacter character;
    AttackManager attackManager;
    GameObject slashEffect;
    ParticleSystem swingParticles;
    ParticleSystem glow;
    void Start()
    {
        character = GetComponentInParent<ThirdPersonCharacter>();
        animator = GetComponent<Animator>();
        audioManager = GetComponentInParent<ActorAudioManager>();
        actorEquipment = GetComponentInParent<ActorEquipment>();
        hungerManager = GetComponentInParent<HungerManager>();
        attackManager = FindObjectOfType<AttackManager>(); // Centralized attack manager
    }

    public void StartMove()
    {
        animator.SetBool("AttackMove", true);
        if (actorEquipment.equippedItem != null)
        {
            ToolItem tool = actorEquipment.equippedItem.GetComponent<ToolItem>();
            if (tool != null)
            {
                // Spawn the swing effect
                SpawnSlashEffect(actorEquipment.equippedItem.transform, tool.range);
            }
        }
    }

    public void EndMove()
    {
        animator.SetBool("AttackMove", false);

    }

    public void Hit()
    {
        audioManager.PlayAttack();
        animator.SetBool("CanHit", true);
        if (actorEquipment.equippedItem != null)
        {
            ToolItem tool = actorEquipment.equippedItem.GetComponent<ToolItem>();
            if (tool != null)
            {
                tool.StartHitbox();
            }
        }
        // Handle hands
        if (actorEquipment.equippedItem == null && actorEquipment.m_TheseHandsArray != null)
        {
            foreach (var hand in actorEquipment.m_TheseHandsArray)
            {
                hand.GetComponent<TheseHands>()?.Hit();
            }
        }

        // Handle feet
        if (actorEquipment.equippedItem != null && actorEquipment.equippedItem.name.ToLower().Contains("bow") && actorEquipment.m_TheseFeetArray != null)
        {
            foreach (var foot in actorEquipment.m_TheseFeetArray)
            {
                foot.GetComponent<TheseFeet>()?.Hit();
            }
        }
    }
    private void SpawnSlashEffect(Transform weaponTransform, float range)
    {
        // Instantiate the particle system slash prefab
        slashEffect = PhotonNetwork.Instantiate(
            "PhotonPrefabs/vfx_SwingTrail",
            new Vector3(0, range / 2, 0),
            UnityEngine.Quaternion.identity
        );

        // Assign the weapon's mesh as the emitter shape
        swingParticles = slashEffect.transform.GetChild(0).GetComponent<ParticleSystem>();
        swingParticles.transform.localScale = new Vector3(1, range, 1);
        glow = slashEffect.transform.GetChild(1).GetComponent<ParticleSystem>();
        glow.transform.localScale = new Vector3(.75f, range / 2, 1);
        // Optional: Parent the effect to the weapon so it follows
        slashEffect.transform.SetParent(weaponTransform, worldPositionStays: false);
        // slashEffect.transform.position = weaponTransform.position + new UnityEngine.Vector3(0, range / 2, 0);
    }



    public void EndHit()
    {
        animator.SetBool("CanHit", false);

        if (slashEffect != null)
        {
            swingParticles.Stop();
            glow.Stop();
            slashEffect.transform.parent = null;
            slashEffect = null;
        }

        // Deactivate hitboxes for hands and feet
        if (actorEquipment.m_TheseHandsArray != null)
        {
            foreach (var hand in actorEquipment.m_TheseHandsArray)
            {
                hand.GetComponent<TheseHands>()?.EndHit();
            }
        }

        else if (actorEquipment.m_TheseFeetArray != null)
        {
            foreach (var foot in actorEquipment.m_TheseFeetArray)
            {
                foot.GetComponent<TheseFeet>()?.EndHit();
            }
        }
        else if (actorEquipment.equippedItem.TryGetComponent<ToolItem>(out var tool))
        {
            tool.EndHitbox();
        }
    }

    public void Eat()
    {
        actorEquipment.equippedItem.GetComponent<Food>().PrimaryAction(1);
    }

    public void EndEat()
    {
        animator.SetLayerWeight(2, 0);
        animator.SetBool("Eating", false);

    }

    public void FootL()
    {
        audioManager.PlayStep();
    }

    public void FootR()
    {
        audioManager.PlayStep();
    }

    // Other events remain unchanged
    public void Shoot()
    {
        if (animator.transform.parent.GetComponent<PhotonView>().IsMine)
        {
            animator.transform.parent.gameObject.GetComponent<AttackManager>().ShootBow();
        }
    }

    public void Cast()
    {
        if (animator.transform.parent.GetComponent<PhotonView>().IsMine)
        {
            animator.transform.parent.gameObject.GetComponent<AttackManager>().CastWand();
        }
    }

    public void Cast2()
    {
        if (animator.transform.parent.GetComponent<PhotonView>().IsMine)
        {
            animator.transform.parent.gameObject.GetComponent<AttackManager>().CastWandArc();
        }
    }
}
