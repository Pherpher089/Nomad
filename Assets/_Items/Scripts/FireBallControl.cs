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
    ActorEquipment ae;
    PhotonView pv;

    [HideInInspector] public bool isLob = false;
    public float knockBackForce = 0;
    public bool fireDamage = false;
    public bool frostDamage = false;
    public bool explosion = true;
    void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    public void Initialize(GameObject actorObject, GameObject bow, bool _isLob)
    {
        if (!pv.IsMine) return;
        ownerObject = actorObject;
        wandObject = bow;
        fireBallDamage += bow.GetComponent<ToolItem>().damage;
        stats = actorObject.GetComponentInParent<CharacterStats>();
        ae = ownerObject.GetComponent<ActorEquipment>();
        partner = ae.m_TheseHandsArray[0].gameObject.name != gameObject.name ? ae.m_TheseHandsArray[0] : ae.m_TheseHandsArray[1];
        isLob = _isLob;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("Grass")) return;
        if (other.gameObject.name.Contains("HitBox")) return;

        if (!GameStateManager.Instance.friendlyFire && other.gameObject.CompareTag("Player")) return;
        if (other.gameObject.CompareTag("Tool")) return;
        if (other.tag is "Tool" or "HandSocket" or "Beast" or "DoNotLand" or "Item")
        {
            return;
        }
        if (other.TryGetComponent(out Item item))
        {
            if (item.isEquipped) return;
        }
        if (!pv.IsMine)
        {
            Destroy(this.gameObject);
            return;
        }
        Rigidbody fireBallRigidBody = GetComponent<Rigidbody>();
        fireBallRigidBody.velocity = Vector3.zero;
        fireBallRigidBody.isKinematic = true;
        GetComponent<Item>().itemListIndex = -1;
        GetComponent<SpawnMotionDriver>().Land(false);

        try
        {
            HealthManager hm = other.gameObject.GetComponent<HealthManager>();
            SourceObject so = other.GetComponent<SourceObject>();

            if (other.gameObject.TryGetComponent<BuildingMaterial>(out var bm))
            {
                //bm.healthManager.statusEffects.CallCatchFire(2, 5);
                LevelManager.Instance.CallUpdateObjectsPRC(bm.id, bm.spawnId, fireBallDamage + stats.attack, ToolType.Arrow, transform.position, ownerObject.GetComponent<PhotonView>());
            }
            else if (so != null)
            {
                LevelManager.Instance.CallUpdateObjectsPRC(so.id, "", fireBallDamage + stats.attack, ToolType.Arrow, transform.position, ownerObject.GetComponent<PhotonView>());
            }
            else if (hm != null)
            {
                if (fireDamage)
                {
                    hm.statusEffects.CallCatchFire(2, 5);
                }
                if (frostDamage)
                {
                    hm.statusEffects.CallFreeze(2, 5);

                }
                hm.Hit(fireBallDamage + stats.magicAttack, ToolType.Arrow, transform.position, ownerObject, knockBackForce);
            }
            if (isLob && explosion)
            {
                GameObject explostion = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "FireBallExplosion"), transform.position + transform.forward, Quaternion.LookRotation(transform.forward));

                explostion.GetComponent<FireBallExplosionControl>().Initialize(ownerObject, wandObject);
            }
            PhotonNetwork.Destroy(this.gameObject);
            return;
        }
        catch (System.Exception ex)
        {
        }
    }

}
