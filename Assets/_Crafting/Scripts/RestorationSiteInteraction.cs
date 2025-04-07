using UnityEngine;

public class RestorationSiteInteraction : InteractionManager
{
    GameObject builderGearUI;
    RestorationSiteUIController restorationSiteController;
    bool initialized = false;
    bool isOpen = false;
    // Start is called before the first frame update
    void Awake()
    {
        restorationSiteController = GetComponent<RestorationSiteUIController>();
        builderGearUI = transform.GetChild(0).gameObject;
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
        restorationSiteController.PlayerOpenUI(i);
        return isOpen;
    }
}