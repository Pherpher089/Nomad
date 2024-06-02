using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class AoeHeal : MonoBehaviour
{
    GameObject ownerObject;
    public int healthValue = 10;
    PhotonView pv;
    ParticleSystem ps;
    void Awake()
    {
        pv = GetComponent<PhotonView>();
        ps = GetComponent<ParticleSystem>();
        ps.Play();
    }
    public void Initialize(GameObject actorObject)
    {
        if (!pv.IsMine) return;
        ownerObject = actorObject;
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

        if (hm != null)
        {
            hm.Heal(healthValue, ownerObject);
        }
        return;
    }
}
