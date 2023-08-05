using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class TheseHands : MonoBehaviour
{
    Animator m_Animator;
    GameObject m_HansOwner;
    CharacterStats stats;
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
        stats = GetComponentInParent<CharacterStats>();
        m_Animator = GetComponentInParent<Animator>();
        m_HansOwner = m_Animator.transform.parent.gameObject;
        ae = m_HansOwner.GetComponent<ActorEquipment>();
        partner = ae.m_TheseHandsArray[0].gameObject.name != gameObject.name ? ae.m_TheseHandsArray[0] : ae.m_TheseHandsArray[1];
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
                    BuildingMaterial bm = other.gameObject.GetComponent<BuildingMaterial>();
                    if (bm != null)
                    {
                        Debug.Log("### Here 1");
                        LevelManager.Instance.CallUpdateObjectsPRC(bm.id, 1 + stats.attack, ToolType.Hands, transform.position, m_HansOwner.GetComponent<PhotonView>());
                    }
                    else if (hm != null)
                    {
                        hm.TakeHit(1 + stats.attack, ToolType.Hands, transform.position, m_HansOwner);
                    }
                    else if (so != null)
                    {
                        LevelManager.Instance.CallUpdateObjectsPRC(so.id, 1 + stats.attack, ToolType.Hands, transform.position, m_HansOwner.GetComponent<PhotonView>());
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
