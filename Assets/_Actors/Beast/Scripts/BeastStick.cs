using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BeastStick : MonoBehaviour
{
    public Tool tool;
    public List<Collider> m_HaveHit;

    void Awake()
    {
        tool = GetComponent<Tool>();
    }
    void Start()
    {
        m_HaveHit = new List<Collider>();
    }
    void Update()
    {
        if (!tool.canDealDamage && m_HaveHit.Count > 0)
        {
            //m_HaveHit = new List<Collider>();
        }
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
            if (other.gameObject.tag == "Beast")
            {
                other.GetComponent<BeastManager>().Hit();
            }
        }
    }
}
