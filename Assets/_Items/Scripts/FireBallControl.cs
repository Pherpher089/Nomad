using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;

public class FireBallControl : MonoBehaviour
{
    GameObject ownerObject;
    GameObject wandObject;
    CharacterStats stats;
    public int fireBallDamage = 10;
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
        ownerObject = actorObject;
        wandObject = bow;
        fireBallDamage += bow.GetComponent<Tool>().damage;
        stats = actorObject.GetComponentInParent<CharacterStats>();
        ae = ownerObject.GetComponent<ActorEquipment>();
        partner = ae.m_TheseHandsArray[0].gameObject.name != gameObject.name ? ae.m_TheseHandsArray[0] : ae.m_TheseHandsArray[1];
    }

    void OnTriggerStay(Collider other)
    {

        if (other.gameObject.name.Contains("Grass")) return;
        if (!GameStateManager.Instance.friendlyFire && other.gameObject.CompareTag("Player")) return;

        if (other.tag is "Tool" or "HandSocket")
        {
            return;
        }
        if (!pv.IsMine)
        {
            Debug.Log("### PV NOT MINE");
            Destroy(this.gameObject);
            return;
        }

        Rigidbody fireBallRigidBody = GetComponent<Rigidbody>();
        fireBallRigidBody.velocity = Vector3.zero;
        fireBallRigidBody.isKinematic = true;
        GetComponent<Item>().itemIndex = -1;
        GetComponent<SpawnMotionDriver>().Land(false);

        try
        {

            HealthManager hm = other.gameObject.GetComponent<HealthManager>();
            SourceObject so = other.GetComponent<SourceObject>();
            canDealDamage = false;

            if (other.gameObject.TryGetComponent<BuildingMaterial>(out var bm))
            {
                LevelManager.Instance.CallUpdateObjectsPRC(bm.id, fireBallDamage + stats.attack, ToolType.Arrow, transform.position, ownerObject.GetComponent<PhotonView>());
            }
            else if (so != null)
            {
                LevelManager.Instance.CallUpdateObjectsPRC(so.id, fireBallDamage + stats.attack, ToolType.Arrow, transform.position, ownerObject.GetComponent<PhotonView>());
            }
            else if (hm != null)
            {
                Debug.Log("### attack " + stats.attack);
                hm.Hit(fireBallDamage + stats.attack, ToolType.Arrow, transform.position, ownerObject);
            }
            GameObject explostion = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "FireBallExplosion"), transform.position + transform.forward, Quaternion.LookRotation(transform.forward));

            explostion.GetComponent<FireBallExplosionControl>().Initialize(ownerObject, wandObject);
            Destroy(this.gameObject);
            return;
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }
    }
    public void Hit()
    {
        canDealDamage = true;
    }
}
