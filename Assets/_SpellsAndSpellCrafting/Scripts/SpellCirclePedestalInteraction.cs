using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SpellCirclePedestalInteraction : InteractionManager
{
    public bool hasItem = false;
    public Item currentItem;
    public Transform socket;
    // Start is called before the first frame update
    void Awake()
    {
        socket = transform.GetChild(0);
    }

    public void OnEnable()
    {
        OnInteract += PlaceItem;
    }

    public void OnDisable()
    {
        OnInteract -= PlaceItem;
    }

    //Packs or unpacks a packable item. It also adjusts the save data for the new state
    public bool PlaceItem(GameObject i)
    {
        ActorEquipment ae = i.GetComponent<ActorEquipment>();
        if (!hasItem && ae.hasItem)
        {
            int itemIndex = ae.equippedItem.GetComponent<Item>().itemIndex;
            LevelManager.Instance.CallSpellCirclePedestalPRC(transform.parent.GetComponent<BuildingMaterial>().id, itemIndex, transform.GetSiblingIndex(), false);
            i.GetComponent<PlayerInventoryManager>().SpendItem(ae.equippedItem.GetComponent<Item>());
            return true;
        }
        else if (hasItem)
        {
            ae.GrabItem(currentItem);
            LevelManager.Instance.CallSpellCirclePedestalPRC(transform.parent.GetComponent<BuildingMaterial>().id, currentItem.itemIndex, transform.GetSiblingIndex(), true);
            return true;
        }
        return false;
    }
}
