using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class TheseHands : MonoBehaviour
{
    Animator m_Animator;
    GameObject m_Owner;
    CharacterStats stats;
    public TheseHands partner;
    public int damage = 2; // Default damage for hands attacks
    private bool canDealDamage = false;
    ActorEquipment ae;
    PhotonView pv;
    AttackManager attackManager;

    void Awake()
    {
        pv = GetComponentInParent<PhotonView>();
    }

    void Start()
    {
        m_Animator = GetComponentInParent<Animator>();
        m_Owner = m_Animator.transform.parent.gameObject;
        ae = m_Owner.GetComponent<ActorEquipment>();
        partner = ae.m_TheseHandsArray[0].gameObject.name != gameObject.name ? ae.m_TheseHandsArray[0] : ae.m_TheseHandsArray[1];
        attackManager = FindObjectOfType<AttackManager>();

        if (m_Owner.TryGetComponent<CharacterStats>(out var characterStats))
        {
            stats = characterStats;
        }
    }

    private void Update()
    {
        if (canDealDamage && !m_Animator.GetBool("Attacking"))
        {
            canDealDamage = false;
        }
    }

    public void Hit()
    {
        if (!pv.IsMine || attackManager == null) return;

        canDealDamage = true;

        // Activate hitbox through AttackManager
        attackManager.ActivateHitbox(
            m_Owner,
            transform.position,
            transform.forward,
            ToolType.Hands,
            damage + (stats != null ? stats.attack : 0),
            0f, // Adjust knockback force for hands
            1
        );
    }

    public void EndHit()
    {
        canDealDamage = false;
        if (attackManager != null)
        {
            attackManager.DeactivateHitbox();
        }
    }
}
