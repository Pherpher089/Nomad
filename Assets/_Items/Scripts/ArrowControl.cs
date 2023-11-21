using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        if (!canDealDamage) return;
        if (other.gameObject.name.Contains("Grass")) return;
        if (!GameStateManager.Instance.friendlyFire && other.gameObject.CompareTag("Player")) return;

        if (other.tag == "Tool" || other.tag == "HandSocket")
        {
            return;
        }
        if (transform.position.y <= 0)
        {
            transform.position = new Vector3(transform.position.x, 0.3f, transform.position.z);
        }
        Rigidbody arrowRigidBody = GetComponent<Rigidbody>();
        arrowRigidBody.velocity = Vector3.zero;
        arrowRigidBody.isKinematic = true;
        GetComponent<Item>().itemIndex = -1;
        GetComponent<SpawnMotionDriver>().Land(false);

        if (!pv.IsMine)
        {
            if (other.gameObject.tag == "Enemy")
            {
                Destroy(this.gameObject);
            }
            return;
        }
        Debug.Log("### made it this far");
        try
        {

            HealthManager hm = other.gameObject.GetComponent<HealthManager>();
            SourceObject so = other.GetComponent<SourceObject>();
            canDealDamage = false;
            Debug.Log("### made it this far1");

            if (other.gameObject.TryGetComponent<BuildingMaterial>(out var bm))
            {
                LevelManager.Instance.CallUpdateObjectsPRC(bm.id, arrowDamage + stats.attack, ToolType.Arrow, transform.position, ownerObject.GetComponent<PhotonView>());
                Debug.Log("### made it this far2");

            }
            else if (so != null)
            {
                LevelManager.Instance.CallUpdateObjectsPRC(so.id, arrowDamage + stats.attack, ToolType.Arrow, transform.position, ownerObject.GetComponent<PhotonView>());
                Debug.Log("### made it this far3");

            }
            else if (hm != null)
            {
                hm.Hit(arrowDamage + stats.attack, ToolType.Hands, transform.position, ownerObject);
                Debug.Log("### made it this far4");

            }
            if (other.CompareTag("Enemy"))
            {
                Destroy(this.gameObject);
            }
            Debug.Log("### made it this far5");

            return;
        }
        catch (System.Exception ex)
        {
            Debug.Log("### " + ex);
        }
    }
    public void Hit()
    {
        canDealDamage = true;
    }
}
