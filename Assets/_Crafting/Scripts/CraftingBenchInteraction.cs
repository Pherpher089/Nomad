using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingBenchInteraction : InteractionManager
{
    GameObject craftingUI;
    CraftingBenchUIController craftingBenchController;
    bool initialized = false;
    bool isOpen = false;
    // Start is called before the first frame update
    void Awake()
    {
        craftingBenchController = GetComponent<CraftingBenchUIController>();
        craftingUI = transform.GetChild(0).gameObject;
        initialized = true;
    }

    public void OnEnable()
    {
        OnInteract += OpenCraftingBench;
    }

    public void OnDisable()
    {
        OnInteract -= OpenCraftingBench;
    }

    //Packs or unpacks a packable item. It also adjusts the save data for the new state
    public bool OpenCraftingBench(GameObject i)
    {
        Debug.Log("Interacting");
        craftingBenchController.PlayerOpenUI(i);
        return isOpen;
    }

}
