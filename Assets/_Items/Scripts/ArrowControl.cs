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

    void OnTriggerStay(Collider other)
    {
        if (!pv.IsMine)
        {
            return;
        }
        if (!canDealDamage) return;
        if (other.gameObject == ae.gameObject) return;
        if (!GameStateManager.Instance.friendlyFire && other.gameObject.CompareTag("Player") && ae.CompareTag("Player")) return;
        if (other.CompareTag("Tool") || other.CompareTag("HandSocket") || other.name.Contains("Grass") || other.name.Contains("HitBox"))
        {
            return;
        }
        transform.DetachChildren();
        try
        {

            HealthManager hm = other.gameObject.GetComponent<HealthManager>();
            SourceObject so = other.GetComponent<SourceObject>();
            canDealDamage = false;

            if (other.gameObject.TryGetComponent<BuildingMaterial>(out var bm))
            {
                LevelManager.Instance.CallUpdateObjectsPRC(bm.id, arrowDamage + attack, ToolType.Arrow, transform.position, ownerObject.GetComponent<PhotonView>());
            }
            else if (so != null)
            {
                LevelManager.Instance.CallUpdateObjectsPRC(so.id, arrowDamage + attack, ToolType.Arrow, transform.position, ownerObject.GetComponent<PhotonView>());
            }
            else if (hm != null)
            {
                hm.Hit(arrowDamage + attack, ToolType.Arrow, transform.position, ownerObject, 0);
            }
            PhotonNetwork.Destroy(GetComponent<PhotonView>());
            return;
        }
        catch (System.Exception ex)
        {
            // Debug.LogError(ex);
        }
    }
    public void Hit()
    {
        canDealDamage = true;
    }
}
