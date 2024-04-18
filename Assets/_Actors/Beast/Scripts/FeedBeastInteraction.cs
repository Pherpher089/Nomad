using System.Collections;
using System.Collections.Generic;
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
            return true;
        }

        return false;

    }
}
