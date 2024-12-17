using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TheseFeet : MonoBehaviour
{
    Animator m_Animator;
    GameObject m_Owner;
    CharacterStats stats;
    public TheseFeet partner;
    public int damage = 2; // Default damage for feet attacks
    public int knockBackForce = 3;
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
        if (SceneManager.GetActiveScene().name.Contains("LoadingScene")) return;

        stats = GetComponentInParent<CharacterStats>();
        m_Animator = GetComponentInParent<Animator>();
        m_Owner = m_Animator.transform.parent.gameObject;
        ae = m_Owner.GetComponent<ActorEquipment>();
        partner = ae.m_TheseFeetArray[0].gameObject.name != gameObject.name ? ae.m_TheseFeetArray[0] : ae.m_TheseFeetArray[1];
        attackManager = FindObjectOfType<AttackManager>();
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
            ToolType.Beast,
            damage + stats.attack,
            30f,
            1 // Default knockback force
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
