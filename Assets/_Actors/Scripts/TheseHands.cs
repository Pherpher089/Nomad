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
    public List<GameObject> m_HaveHit;
    private bool canDealDamage = false;
    ActorEquipment ae;
    PhotonView pv;
    Collider col;
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
        col = GetComponent<Collider>();
    }
    private void Update()
    {
        if (canDealDamage && !m_Animator.GetBool("Attacking"))
        {
            canDealDamage = false;
        }

        col.enabled = canDealDamage;


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
                if (m_HaveHit.Contains(other.gameObject) || partner.m_HaveHit.Contains(other.gameObject))
                {
                    return;
                }
                else
                {
                    m_HaveHit.Add(other.gameObject);
                    partner.m_HaveHit.Add(other.gameObject);
                }
                try
                {
                    HealthManager hm = other.gameObject.GetComponent<HealthManager>();
                    if (hm == null)
                    {
                        hm = other.GetComponentInParent<HealthManager>();
                        if (hm != null && (m_HaveHit.Contains(hm.gameObject) || partner.m_HaveHit.Contains(hm.gameObject)))
                        {
                            return;
                        }
                    }
                    SourceObject so = other.GetComponent<SourceObject>();
                    if (so == null)
                    {
                        so = other.GetComponentInParent<SourceObject>();
                        if (so != null && (m_HaveHit.Contains(so.gameObject) || partner.m_HaveHit.Contains(so.gameObject)))
                        {
                            return;
                        }
                    }
                    BuildingMaterial bm = other.gameObject.GetComponent<BuildingMaterial>();
                    if (bm == null)
                    {
                        bm = other.GetComponentInParent<BuildingMaterial>();
                        if (bm != null && (m_HaveHit.Contains(bm.gameObject) || partner.m_HaveHit.Contains(bm.gameObject)))
                        {
                            return;
                        }
                    }

                    if (bm != null)
                    {
                        if (!m_HaveHit.Contains(bm.gameObject) && !partner.m_HaveHit.Contains(bm.gameObject))
                        {
                            m_HaveHit.Add(bm.gameObject);
                            partner.m_HaveHit.Add(bm.gameObject);
                        }
                        LevelManager.Instance.CallUpdateObjectsPRC(bm.id, bm.spawnId, 2 + attack, ToolType.Hands, transform.position, m_HansOwner.GetComponent<PhotonView>());
                    }
                    else if (so != null)
                    {
                        if (!m_HaveHit.Contains(so.gameObject) && !partner.m_HaveHit.Contains(so.gameObject))
                        {
                            m_HaveHit.Add(so.gameObject);
                            partner.m_HaveHit.Add(so.gameObject);
                        }
                        LevelManager.Instance.CallUpdateObjectsPRC(so.id, "", 2 + attack, ToolType.Hands, transform.position, m_HansOwner.GetComponent<PhotonView>());
                    }
                    else if (hm != null)
                    {
                        if (!m_HaveHit.Contains(hm.gameObject) && !partner.m_HaveHit.Contains(hm.gameObject))
                        {
                            m_HaveHit.Add(hm.gameObject);
                            partner.m_HaveHit.Add(hm.gameObject);
                        }
                        hm.Hit(2 + attack, ToolType.Hands, transform.position, m_HansOwner, 30f);
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
