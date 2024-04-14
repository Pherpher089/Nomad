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
    public List<Collider> m_HaveHit;
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
        fireBallDamage += wand.GetComponent<ToolItem>().damage;
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
        SourceObject so = other.GetComponent<SourceObject>();

        if (other.gameObject.TryGetComponent<BuildingMaterial>(out var bm))
        {
            LevelManager.Instance.CallUpdateObjectsPRC(bm.id, fireBallDamage, ToolType.Arrow, transform.position, ownerObject.GetComponent<PhotonView>());
        }
        else if (so != null)
        {
            LevelManager.Instance.CallUpdateObjectsPRC(so.id, fireBallDamage + stats.attack, ToolType.Arrow, transform.position, ownerObject.GetComponent<PhotonView>());
        }
        else if (hm != null)
        {
            hm.Hit(fireBallDamage + stats.attack, ToolType.Arrow, transform.position, ownerObject);
        }
        return;
    }
}
