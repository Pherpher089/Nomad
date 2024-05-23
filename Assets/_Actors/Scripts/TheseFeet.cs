using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TheseFeet : MonoBehaviour
{
    Animator m_Animator;
    GameObject m_HansOwner;
    CharacterStats stats;
    public TheseFeet partner;
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
        if (SceneManager.GetActiveScene().name.Contains("LoadingScene")) return;

        stats = GetComponentInParent<CharacterStats>();
        m_Animator = GetComponentInParent<Animator>();
        m_HansOwner = m_Animator.transform.parent.gameObject;
        ae = m_HansOwner.GetComponent<ActorEquipment>();
        partner = ae.m_TheseFeetArray[0].gameObject.name != gameObject.name ? ae.m_TheseFeetArray[0] : ae.m_TheseFeetArray[1];
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
                    BuildingMaterial bm = other.gameObject.GetComponent<BuildingMaterial>();
                    if (bm != null)
                    {
                        LevelManager.Instance.CallUpdateObjectsPRC(bm.id, 2 + stats.attack, ToolType.Hands, transform.position, m_HansOwner.GetComponent<PhotonView>());
                    }
                    else if (so != null)
                    {
                        LevelManager.Instance.CallUpdateObjectsPRC(so.id, 2 + stats.attack, ToolType.Hands, transform.position, m_HansOwner.GetComponent<PhotonView>());
                    }
                    else if (hm != null)
                    {
                        hm.Hit(2 + stats.attack, ToolType.Hands, transform.position, m_HansOwner);
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
