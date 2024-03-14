using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class MainPortalInteraction : InteractionManager
{
    [Range(0, 8)] public int numberOfFragments;
    PhotonView pv;
    MainPortalManager m_MainPortalManager;
    void Start()
    {
        m_MainPortalManager = GetComponent<MainPortalManager>();
        pv = GetComponent<PhotonView>();
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
        if (numberOfFragments == 8 || GameStateManager.Instance.isRaid)
        {
            GetComponent<ParticleSystem>().Play();
        }
        else if (numberOfFragments < 8 && !GameStateManager.Instance.isRaid)
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
