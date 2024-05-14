using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class TheseHands : MonoBehaviour
{
    Animator m_Animator;
    GameObject m_HansOwner;
    int attack;
    public TheseHands partner;
    [HideInInspector]
    public List<Collider> m_HaveHit;
    private bool canDealDamage = false;
    ActorEquipment ae;
    PhotonView pv;
    void Awake()
    {
        pv = GetComponentInParent<PhotonView>();
    }
    void Start()
    {
        m_Animator = GetComponentInParent<Animator>();
        m_HansOwner = m_Animator.transform.parent.gameObject;
        ae = m_HansOwner.GetComponent<ActorEquipment>();
        partner = ae.m_TheseHandsArray[0].gameObject.name != gameObject.name ? ae.m_TheseHandsArray[0] : ae.m_TheseHandsArray[1];
        if (m_HansOwner.TryGetComponent<CharacterStats>(out var stats))
        {
            attack = stats.attack;
        }
        else if (m_HansOwner.TryGetComponent<StateController>(out var controller))
        {
            attack = controller.enemyStats.attackDamage;
        }
    }
    private void Update()
    {
        if (canDealDamage && !m_Animator.GetBool("Attacking"))
        {
            canDealDamage = false;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!GetComponentInParent<PhotonView>().IsMine)
        {
            return;
        }
        if (ae != null && !ae.hasItem)
        {
            if (m_Animator.GetBool("Attacking") && m_Animator.GetBool("CanHit"))
            {
                if (m_HaveHit.Contains(other) || partner.m_HaveHit.Contains(other))
                {
                    return;
                }
                else
                {
                    m_HaveHit.Add(other);
                    partner.m_HaveHit.Add(other);
                }
                try
                {
                    HealthManager hm = other.gameObject.GetComponent<HealthManager>();
                    SourceObject so = other.GetComponent<SourceObject>();
                    if (other.gameObject.TryGetComponent<BuildingMaterial>(out var bm))
                    {
                        LevelManager.Instance.CallUpdateObjectsPRC(bm.spawnId, 2 + attack, ToolType.Hands, transform.position, m_HansOwner.GetComponent<PhotonView>());
                    }
                    else if (so != null)
                    {
                        LevelManager.Instance.CallUpdateObjectsPRC(so.id, 2 + attack, ToolType.Hands, transform.position, m_HansOwner.GetComponent<PhotonView>());
                    }
                    else if (hm != null)
                    {
                        hm.Hit(2 + attack, ToolType.Hands, transform.position, m_HansOwner);
                    }
                    return;
                }
                catch (System.Exception ex)
                {
                    //Debug.Log(ex);
                }
            }
        }
    }
    public void Hit()
    {
        canDealDamage = true;
    }
}
