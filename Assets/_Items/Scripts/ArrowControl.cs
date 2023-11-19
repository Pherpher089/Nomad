using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ArrowControl : MonoBehaviour
{
    GameObject ownerObject;
    GameObject bowObject;
    CharacterStats stats;
    public int arrowDamage = 1;
    public TheseHands partner;
    [HideInInspector]
    public List<Collider> m_HaveHit;
    private bool canDealDamage = false;
    ActorEquipment ae;
    Rigidbody rb;
    PhotonView pv;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pv = GetComponent<PhotonView>();
    }
    public void Initialize(GameObject actorObject, GameObject bow)
    {
        if (!pv.IsMine) return;
        canDealDamage = true;
        ownerObject = actorObject;
        bowObject = bow;
        arrowDamage += bow.GetComponent<Tool>().damage;
        stats = actorObject.GetComponentInParent<CharacterStats>();
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

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.name.Contains("Grass")) return;
        if (!GameStateManager.Instance.friendlyFire && other.gameObject.CompareTag("Player")) return;
        if (!canDealDamage || other.gameObject == ownerObject || other.gameObject == bowObject)
        {
            return;
        }
        if (m_HaveHit.Contains(other) || partner.m_HaveHit.Contains(other))
        {
            return;
        }
        if (other.tag == "Tool" || other.tag == "HandSocket")
        {
            return;
        }
        if (pv.IsMine)
        {
            m_HaveHit.Add(other);
            partner.m_HaveHit.Add(other);

        }

        try
        {
            Rigidbody arrowRigidBody = GetComponent<Rigidbody>();
            arrowRigidBody.velocity = Vector3.zero;
            arrowRigidBody.isKinematic = true;
            if (transform.position.y < 0.2f)
            {
                transform.position = new Vector3(transform.position.x, 0.2f, transform.position.z);
            }
            transform.parent = other.transform;
            if (pv.IsMine)
            {
                HealthManager hm = other.gameObject.GetComponent<HealthManager>();
                SourceObject so = other.GetComponent<SourceObject>();
                canDealDamage = false;
                GetComponent<SpawnMotionDriver>().hasSaved = true;
                BuildingMaterial bm = other.gameObject.GetComponent<BuildingMaterial>();
                if (bm != null)
                {
                    LevelManager.Instance.CallUpdateObjectsPRC(bm.id, arrowDamage + stats.attack, ToolType.Hands, transform.position, ownerObject.GetComponent<PhotonView>());
                }
                else if (so != null)
                {
                    LevelManager.Instance.CallUpdateObjectsPRC(so.id, arrowDamage + stats.attack, ToolType.Hands, transform.position, ownerObject.GetComponent<PhotonView>());
                }
                else if (hm != null)
                {
                    hm.Hit(arrowDamage + stats.attack, ToolType.Hands, transform.position, ownerObject);
                }
            }
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
