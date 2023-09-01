using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;


public class BeastCargoInventoryManager : MonoBehaviour
{
    //The UI GameObject
    public bool isOpen = true;
    public GameObject playerCurrentlyUsing = null;
    BeastManager beastManager;
    public CargoSlot[] cargoSlots;
    Dictionary<List<int>, CargoItem> cargoItems;
    void Start()
    {
        cargoItems = new Dictionary<List<int>, CargoItem>();
        cargoSlots = new CargoSlot[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            cargoSlots[i] = transform.GetChild(i).GetComponent<CargoSlot>();
        }
        beastManager = GetComponentInParent<BeastManager>();
        CloseWhileNotCamping();
    }

    void Update()
    {
        if (!beastManager.isCamping && isOpen)
        {
            CloseWhileNotCamping();
        }
    }

    public CargoSlot GetOpenSlot()
    {
        foreach (CargoSlot slot in cargoSlots)
        {
            if (!slot.isOccupied)
            {
                return slot;
            }
        }
        return null;
    }
    public void PlayerOpenUI(GameObject actor)
    {
        //if actor has a packable item
        // open the cargo inventory with an item in the closest avaliable slot
        if (beastManager.isCamping && !isOpen || isOpen)
        {
            if (isOpen)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(false);
                }
                ActorEquipment ac = actor.GetComponent<ActorEquipment>();
                ac.GetComponent<ThirdPersonUserControl>().cargoUI = false;
                isOpen = false;
                playerCurrentlyUsing = null;
            }
            else
            {
                playerCurrentlyUsing = actor;
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(true);
                }
                ActorEquipment ac = actor.GetComponent<ActorEquipment>();
                ac.GetComponent<ThirdPersonUserControl>().cargoUI = true;
                if (ac != null && ac.equippedItem.GetComponent<CargoItem>() != null)
                {
                    CargoItem ci = ac.equippedItem.GetComponent<CargoItem>();
                    CargoSlot slot = GetOpenSlot();
                    if (slot != null)
                    {
                        slot.spriteRenderer.sprite = ci.cargoIconPacked;
                    }
                }
                isOpen = true;
            }
        }
    }

    public void CloseWhileNotCamping()
    {
        if (!beastManager.isCamping && isOpen)
        {
            if (isOpen)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(false);
                }
                isOpen = false;
                playerCurrentlyUsing = null;
            }
        }
    }
}





