using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeastChestInteraction : InteractionManager
{

    GameObject craftingUI;
    BeastStorageContainerController chestController;
    bool initialized = false;
    bool isOpen = false;
    // Start is called before the first frame update
    void Awake()
    {
        chestController = GetComponent<BeastStorageContainerController>();
        craftingUI = transform.GetChild(0).gameObject;
        initialized = true;
    }

    public void OnEnable()
    {
        OnInteract += OpenChest;
    }

    public void OnDisable()
    {
        OnInteract -= OpenChest;
    }

    //Packs or unpacks a packable item. It also adjusts the save data for the new state
    public bool OpenChest(GameObject i)
    {
        Debug.Log("Interacting");
        chestController.PlayerOpenUI(i);
        return isOpen;
    }
}
