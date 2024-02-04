using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class MainPortalInteraction : InteractionManager
{
    [Range(0, 8)] public int numberOfFragments;
    PhotonView pv;
    MainPortalManager m_MainPortalManager;
    void Awake()
    {
        m_MainPortalManager = GetComponent<MainPortalManager>();
        pv = GetComponent<PhotonView>();
        SetFragments();
    }
    void SetFragments()
    {
        Transform[] portalFragments = GetComponentsInChildren<Transform>();
        for (int i = 0; i < 8; i++)
        {
            if (i > numberOfFragments - 1)
            {
                portalFragments[i].GetComponent<MeshRenderer>().enabled = false;
            }
            else
            {
                portalFragments[i].GetComponent<MeshRenderer>().enabled = true;
            }
        }
        if (numberOfFragments == 8)
        {
            GetComponent<ParticleSystem>().Play();
        }
        else
        {
            GetComponent<ParticleSystem>().Stop();

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
        if (numberOfFragments < 8 && i.GetComponent<ActorEquipment>().equippedItem.GetComponent<Item>().itemIndex == 30)
        {
            i.GetComponent<ActorEquipment>().UnequippedCurrentItem(true);
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
        numberOfFragments++;
        SetFragments();
        m_MainPortalManager.AdjustPortalHealth();

    }
    [PunRPC]
    public void RemovePortalPiece()
    {
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
