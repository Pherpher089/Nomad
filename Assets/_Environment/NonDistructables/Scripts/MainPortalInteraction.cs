using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.Mathematics;
using UnityEngine;

public class MainPortalInteraction : InteractionManager
{
    [Range(0, 8)] public int numberOfFragments;
    PhotonView pv;
    MainPortalManager m_MainPortalManager;
    ParticleSystem portalEffect;
    public ParticleSystem winEffect;
    public ParticleSystem looseEffect;
    void Start()
    {
        m_MainPortalManager = GetComponent<MainPortalManager>();
        pv = GetComponent<PhotonView>();
        portalEffect = GetComponent<ParticleSystem>();
        SetFragments();
    }
    public void SetFragments()
    {
        for (int i = 0; i < 8; i++)
        {
            if (i > numberOfFragments - 1)
            {
                transform.GetChild(i).GetComponent<MeshRenderer>().enabled = false;
            }
            else
            {
                transform.GetChild(i).GetComponent<MeshRenderer>().enabled = true;
            }
        }
        if (!GameStateManager.Instance.isRaid && !GameStateManager.Instance.isRaidComplete && numberOfFragments == 8)
        {
            portalEffect.Play();
        }
        else if (!GameStateManager.Instance.isRaid && GameStateManager.Instance.isRaidComplete && numberOfFragments > 0)
        {
            Instantiate(winEffect, transform.position + Vector3.up * 3, Quaternion.identity);
            portalEffect.startColor = Color.white;
            if (PhotonNetwork.IsMasterClient)
            {
                HealthManager[] enemyHealth = FindObjectsOfType<HealthManager>();
                foreach (HealthManager hm in enemyHealth)
                {
                    if (hm.gameObject.CompareTag("Enemy"))
                    {
                        hm.Hit(10000, ToolType.Default, hm.transform.position + Vector3.up * 2, this.gameObject);
                    }
                }
            }
            //portalEffect.Stop();
        }
        else if (!GameStateManager.Instance.isRaid)
        {
            if (GameStateManager.Instance.isRaidComplete)
            {
                Instantiate(looseEffect, transform.position + Vector3.up * 3, Quaternion.identity);
                CamShake.Instance.DoShake(3, 1);
            }
            portalEffect.Stop();
        }
    }

    public void OnEnable()
    {
        OnInteract += CallAddPortalPiece;
    }

    public void OnDisable()
    {
        OnInteract -= CallAddPortalPiece;
    }

    public bool CallAddPortalPiece(GameObject i)
    {
        if (numberOfFragments < 8 && i.GetComponent<ActorEquipment>().equippedItem.GetComponent<Item>().itemIndex == 25)
        {
            i.GetComponent<ActorEquipment>().SpendItem();
            pv.RPC("AddPortalPiece", RpcTarget.AllBuffered);
            return true;
        }
        return false;
    }
    public bool CallRemovePortalPiece()
    {
        if (numberOfFragments > 0)
        {
            pv.RPC("RemovePortalPiece", RpcTarget.AllBuffered);
            return true;
        }
        return false;
    }


    [PunRPC]
    public void AddPortalPiece()
    {
        if (numberOfFragments + 1 > 8) return;
        numberOfFragments++;
        SetFragments();
        m_MainPortalManager.AdjustPortalHealth();

    }
    [PunRPC]
    public void RemovePortalPiece()
    {
        if (numberOfFragments - 1 < 0) return;
        numberOfFragments--;
        SetFragments();
    }

    [PunRPC]
    public void SetPortalPieces(int numPieces)
    {
        numberOfFragments = numPieces;
        SetFragments();
    }
}
