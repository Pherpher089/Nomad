using System.IO;
using Photon.Pun;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class ActorAnimationEventReceiver : MonoBehaviour
{
    [HideInInspector] public ActorEquipment actorEquipment;
    [HideInInspector] public ActorAudioManager audioManager;
    Animator animator;
    ThirdPersonCharacter character;
    GameObject slashEffect;
    ParticleSystem swingParticles;
    ParticleSystem glow;
    PhotonView pv;
    void Start()
    {
        character = GetComponentInParent<ThirdPersonCharacter>();
        animator = GetComponent<Animator>();
        audioManager = GetComponentInParent<ActorAudioManager>();
        actorEquipment = GetComponentInParent<ActorEquipment>();
        if (actorEquipment == null)
        {
            actorEquipment = GetComponent<ActorEquipment>();
        }
        pv = GetComponent<PhotonView>();
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
    public void Land()
    {
        character.m_JumpedWhileSprinting = false;
        //quieting errors
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
            Path.Combine("PhotonPrefabs", "vfx_SwingTrail"),
            new Vector3(0, range / 2, 0),
            Quaternion.identity
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

        if (actorEquipment.m_TheseFeetArray != null)
        {
            foreach (var foot in actorEquipment.m_TheseFeetArray)
            {
                foot.GetComponent<TheseFeet>()?.EndHit();
            }
        }

        if (actorEquipment != null && actorEquipment.equippedItem != null && actorEquipment.equippedItem.TryGetComponent<ToolItem>(out var tool))
        {
            tool.EndHit();
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
    public void EndRam()
    {
        animator.SetBool("Ram", false);
    }
    public void EndEatMamut()
    {
        animator.SetBool("Eating", false);
        GetComponent<BeastManager>().CheckAndCallEvolve();
    }
}
