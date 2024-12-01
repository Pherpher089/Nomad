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
    public List<GameObject> m_HaveHit;
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
                LevelManager.Instance.CallUpdateObjectsPRC(bm.id, bm.spawnId, fireBallDamage + stats.magicAttack, ToolType.Arrow, transform.position, ownerObject.GetComponent<PhotonView>());
            }
            else if (so != null)
            {
                if (!m_HaveHit.Contains(so.gameObject))
                {
                    m_HaveHit.Add(so.gameObject);
                }
                LevelManager.Instance.CallUpdateObjectsPRC(so.id, "", fireBallDamage + stats.magicAttack, ToolType.Arrow, transform.position, ownerObject.GetComponent<PhotonView>());
            }
            else if (hm != null)
            {
                if (!m_HaveHit.Contains(hm.gameObject))
                {
                    m_HaveHit.Add(hm.gameObject);
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
