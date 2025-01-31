using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;

public class FeedBeastInteraction : InteractionManager
{
    Animator m_Animator;

    // Start is called before the first frame update
    void Start()
    {
        m_Animator = GetComponentInParent<Animator>();
    }

    public void OnEnable()
    {
        OnInteract += Feed;
    }

    public void OnDisable()
    {
        OnInteract -= Feed;
    }

    public bool Feed(GameObject i)
    {
        ActorEquipment ac = i.GetComponent<ActorEquipment>();
        if (ac.equippedItem.TryGetComponent(out Food food))
        {
            ac.SpendItem();
            BeastManager.Instance.CallFeedBeast(food.foodValue);
            if (food.itemListIndex == 122)
            {
                PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "MamutTransformEffect"), transform.parent.position + (transform.parent.forward * 2), transform.rotation);
                BeastManager.Instance.LevelUp(LevelManager.Instance.beastLevel + 1);
            }
            return true;
        }

        return false;

    }
}
