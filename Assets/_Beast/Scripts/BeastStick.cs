using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BeastStick : MonoBehaviour
{
    public ToolItem tool;
    public List<Collider> m_HaveHit;

    void Awake()
    {
        tool = GetComponent<ToolItem>();
    }
    void Start()
    {
        m_HaveHit = new List<Collider>();
    }

    void OnTriggerStay(Collider other)
    {
        if (tool.m_OwnerObject == null || !tool.m_OwnerObject.GetComponent<PhotonView>().IsMine)
        {
            return;
        }
        if (tool.isEquipped && tool.m_Animator.GetBool("Attacking") && tool.m_Animator.GetBool("CanHit"))
        {
            if (m_HaveHit.Contains(other))
            {
                return;
            }
            else
            {
                m_HaveHit.Add(other);
            }

            if (other.gameObject.CompareTag("Beast"))
            {
                tool.m_Animator.SetBool("CanHit", false);
                other.GetComponent<BeastManager>().Hit();
            }
            else
            {
                BeastManager bm = BeastManager.Instance;
                if (other.TryGetComponent<DiggableController>(out var digger))
                {
                    Debug.Log("### Hit diggable object: " + other.name);
                    bm.StartDigging(digger.GetComponent<PhotonView>().ViewID);
                }
                else if (other.TryGetComponent<HealthManager>(out var _) && !other.gameObject.CompareTag("Player"))
                {
                    bm.CallSetRamTargetHealthManagerRPR(other.GetComponent<PhotonView>().ViewID);
                }
                else if (other.TryGetComponent<SourceObject>(out var _))
                {
                    bm.CallSetRamTargetSourceObjectRPR(other.GetComponent<SourceObject>().id);
                }
            }


        }
    }
}
