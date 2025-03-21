using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;

public class ArrowControl : MonoBehaviour
{
    GameObject ownerObject;
    GameObject bowObject;
    int attack;
    public int arrowDamage = 1;
    public TheseHands partner;
    [HideInInspector]
    public List<GameObject> m_HaveHit;
    private bool canDealDamage = false;
    ActorEquipment ae;
    Rigidbody rb;
    PhotonView pv;
    public bool isFireArrow = false;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pv = GetComponent<PhotonView>();
        if (!pv.IsMine)
        {
            rb.isKinematic = true;
        }
    }
    public void Initialize(GameObject actorObject, GameObject bow)
    {
        if (!pv.IsMine) return;
        canDealDamage = true;
        ownerObject = actorObject;
        bowObject = bow;
        arrowDamage += bow.GetComponent<ToolItem>().damage;
        if (actorObject.TryGetComponent<CharacterStats>(out var stats))
        {
            attack = stats.attack;
        }
        else if (actorObject.TryGetComponent<StateController>(out var controller))
        {
            attack = controller.enemyStats.attackDamage;
        }
        ae = ownerObject.GetComponent<ActorEquipment>();
        partner = ae.m_TheseHandsArray[0].gameObject.name != gameObject.name ? ae.m_TheseHandsArray[0] : ae.m_TheseHandsArray[1];
    }
    void FixedUpdate()
    {
        if (!pv.IsMine) return;
        if (rb.velocity != Vector3.zero && canDealDamage)
        {
            rb.rotation = Quaternion.LookRotation(rb.velocity);
        }
    }

    [System.Obsolete]
    void OnTriggerStay(Collider other)
    {
        if (!pv.IsMine)
        {
            return;
        }
        if (!canDealDamage) return;
        if (other.gameObject == ae.gameObject) return;
        if (!GameStateManager.Instance.friendlyFire && other.gameObject.CompareTag("Player") && ae.CompareTag("Player")) return;
        if (other.CompareTag("Tool") || other.CompareTag("HandSocket") || other.name.Contains("Grass") || other.name.Contains("HitBox") || other.CompareTag("Item") || other.CompareTag("Arrow"))
        {
            return;
        }
        if (ownerObject.tag == "Enemy" && other.tag == "Enemy")
        {
            return;
        }
        if (isFireArrow)
        {
            transform.GetChild(1).GetComponent<ParticleSystem>().loop = false;
        }
        transform.DetachChildren();
        if (m_HaveHit.Contains(other.gameObject))
        {
            return;
        }
        else
        {
            m_HaveHit.Add(other.gameObject);
        }
        try
        {
            HealthManager hm = other.gameObject.GetComponent<HealthManager>();
            if (hm == null)
            {
                hm = other.GetComponentInParent<HealthManager>();
                if (hm != null && m_HaveHit.Contains(hm.gameObject))
                {
                    return;
                }
            }
            SourceObject so = other.GetComponent<SourceObject>();
            if (so == null)
            {
                so = other.GetComponentInParent<SourceObject>();
                if (so != null && m_HaveHit.Contains(so.gameObject))
                {
                    return;
                }
            }
            BuildingMaterial bm = other.gameObject.GetComponent<BuildingMaterial>();
            if (bm == null)
            {
                bm = other.GetComponentInParent<BuildingMaterial>();
                if (bm != null && m_HaveHit.Contains(bm.gameObject))
                {
                    return;
                }
            }
            if (bm != null)
            {
                if (!m_HaveHit.Contains(bm.gameObject))
                {
                    m_HaveHit.Add(bm.gameObject);
                }
                LevelManager.Instance.CallUpdateObjectsPRC(bm.id, bm.spawnId, arrowDamage + attack, ToolType.Arrow, transform.position, ownerObject.GetComponent<PhotonView>());
            }
            else if (so != null)
            {
                if (!m_HaveHit.Contains(so.gameObject))
                {
                    m_HaveHit.Add(so.gameObject);
                }
                LevelManager.Instance.CallUpdateObjectsPRC(so.id, "", arrowDamage + attack, ToolType.Arrow, transform.position, ownerObject.GetComponent<PhotonView>());
            }
            else if (hm != null)
            {
                if (!m_HaveHit.Contains(hm.gameObject))
                {
                    m_HaveHit.Add(hm.gameObject);
                }
                hm.Hit(arrowDamage + attack, ToolType.Arrow, transform.position, ownerObject, 0f);
                if (isFireArrow)
                {
                    hm.statusEffects.CallCatchFire(2, 5);
                }
            }
            PhotonNetwork.Destroy(GetComponent<PhotonView>());
            return;
        }
        catch (System.Exception ex)
        {
            //Debug.Log(ex);
        }
    }
    public void Hit()
    {
        canDealDamage = true;
    }
}
