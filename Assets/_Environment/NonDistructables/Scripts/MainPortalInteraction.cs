using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPortalInteraction : InteractionManager
{
    [Range(0, 8)] public int numberOfFragments;
    void Awake()
    {
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
        OnInteract += AddPortalPiece;
    }

    public void OnDisable()
    {
        OnInteract -= AddPortalPiece;
    }

    public bool AddPortalPiece(GameObject i)
    {
        if (numberOfFragments < 8 && i.GetComponent<ActorEquipment>().equippedItem.GetComponent<Item>().itemIndex == 30)
        {
            numberOfFragments++;
            i.GetComponent<ActorEquipment>().UnequippedCurrentItem(true);
            SetFragments();
        }
        return true;
    }
}
