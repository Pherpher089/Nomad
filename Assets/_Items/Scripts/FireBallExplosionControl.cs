using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class FireBallExplosionControl : MonoBehaviour
{
    GameObject ownerObject;
    GameObject WandObject;
    CharacterStats stats;
    public int fireBallDamage = 10;
    public TheseHands partner;
    [HideInInspector]
    public List<GameObject> m_HaveHit;
    ActorEquipment ae;
    Rigidbody rb;
    PhotonView pv;
    public GameObject explosionEffect;
    ParticleSystem ps;
    void Awake()
    {
        pv = GetComponent<PhotonView>();
        ps = GetComponent<ParticleSystem>();
        ps.Play();
        CamShake.Instance.DoShake(.3f, .3f);
    }
    public void Initialize(GameObject actorObject, GameObject wand)
    {
        if (!pv.IsMine) return;
        ownerObject = actorObject;
        WandObject = wand;
        if (wand.TryGetComponent(out ToolItem item))
        {
            fireBallDamage += item.damage;
        }
        else if (wand.TryGetComponent(out EarthMineController mine))
        {
            fireBallDamage += mine.damage;

        }
        stats = actorObject.GetComponentInParent<CharacterStats>();
        ae = ownerObject.GetComponent<ActorEquipment>();
        partner = ae.m_TheseHandsArray[0].gameObject.name != gameObject.name ? ae.m_TheseHandsArray[0] : ae.m_TheseHandsArray[1];
    }
    void FixedUpdate()
    {
        if (!ps.isPlaying)
        {
            Destroy(this.gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!GameStateManager.Instance.friendlyFire && other.gameObject.CompareTag("Player")) return;

        if (other.tag == "Tool" || other.tag == "HandSocket")
        {
            return;
        }
        if (other.TryGetComponent(out Item item))
        {
            if (item.isEquipped) return;
        }
        if (!pv.IsMine)
        {
            return;
        }

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
            hm.Hit(fireBallDamage + stats.magicAttack, ToolType.Arrow, transform.position, ownerObject, 40);
        }
        return;
    }
}
