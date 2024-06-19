using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingBenchInteraction : InteractionManager
{
    GameObject craftingUI;
    CraftingBenchUIController craftingBenchController;
    BeastStableCraftingUIController beastStableCraftingUIController;
    SaddleStationUIController saddleStationUIController;
    bool initialized = false;
    bool isOpen = false;
    // Start is called before the first frame update
    void Awake()
    {
        Init();
    }
    void Init()
    {
        craftingBenchController = GetComponent<CraftingBenchUIController>();
        beastStableCraftingUIController = GetComponent<BeastStableCraftingUIController>();
        saddleStationUIController = GetComponent<SaddleStationUIController>();

        craftingUI = transform.GetChild(0).gameObject;
        initialized = true;
    }

    public void OnEnable()
    {
        if (!initialized) Init();
        OnInteract += OpenCraftingBench;
    }

    public void OnDisable()
    {
        OnInteract -= OpenCraftingBench;
    }

    //Packs or unpacks a packable item. It also adjusts the save data for the new state
    public bool OpenCraftingBench(GameObject i)
    {
        if (craftingBenchController != null)
        {
            craftingBenchController.PlayerOpenUI(i);
        }
        else if (beastStableCraftingUIController != null)
        {
            beastStableCraftingUIController.PlayerOpenUI(i);
        }
        else if (saddleStationUIController != null)
        {
            saddleStationUIController.PlayerOpenUI(i);
        }
        return isOpen;
    }

}
