using System;
using Photon.Pun;
using UnityEngine;

public class ActorAnimationEventReceiver : MonoBehaviour
{
    public ActorEquipment actorEquipment;
    public ActorAudioManager audioManager;
    HungerManager hungerManager;
    Animator animator;
    ThirdPersonCharacter character;
    AttackManager attackManager;

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

                // Spawn the swing effect
                SpawnSlashEffect(actorEquipment.equippedItem.transform, tool.range);
            }
        }
        // Handle hands
        if (actorEquipment.m_TheseHandsArray != null)
        {
            foreach (var hand in actorEquipment.m_TheseHandsArray)
            {
                hand.GetComponent<TheseHands>()?.Hit();
            }
        }

        // Handle feet
        if (actorEquipment.m_TheseFeetArray != null)
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
        GameObject slashEffect = PhotonNetwork.Instantiate(
            "PhotonPrefabs/SwingTrail",
            weaponTransform.position,
            weaponTransform.rotation
        );

        // Assign the weapon's mesh as the emitter shape
        ParticleSystem particleSystem = slashEffect.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            var shapeModule = particleSystem.shape;
            MeshFilter weaponMeshFilter = weaponTransform.GetComponent<MeshFilter>();

            if (weaponMeshFilter != null)
            {
                shapeModule.shapeType = ParticleSystemShapeType.Mesh;
                shapeModule.mesh = weaponMeshFilter.mesh;
                shapeModule.scale = new(1, 1, 1);
                shapeModule.alignToDirection = true; // Align particles to weapon's direction
            }
        }

        // Optional: Parent the effect to the weapon so it follows
        slashEffect.transform.SetParent(weaponTransform, worldPositionStays: true);
    }



    public void EndHit()
    {
        animator.SetBool("CanHit", false);

        // Deactivate hitboxes for hands and feet
        if (actorEquipment.m_TheseHandsArray != null)
        {
            foreach (var hand in actorEquipment.m_TheseHandsArray)
            {
                hand.GetComponent<TheseHands>()?.EndHit();
            }
        }

        if (actorEquipment.m_TheseFeetArray != null)
        {
            foreach (var foot in actorEquipment.m_TheseFeetArray)
            {
                foot.GetComponent<TheseFeet>()?.EndHit();
            }
        }
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
}
