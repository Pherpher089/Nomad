using UnityEngine;

public class StationPedestalInteraction : InteractionManager
{
    [HideInInspector] public bool hasItem = false;
    [HideInInspector] public Item currentItem;
    [HideInInspector] public Transform m_Socket;
    // Start is called before the first frame update
    void Awake()
    {
        m_Socket = transform.GetChild(0);
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
            int itemIndex = ae.equippedItem.GetComponent<Item>().itemListIndex;
            LevelManager.Instance.CallSpellCirclePedestalPRC(transform.parent.GetComponent<BuildingMaterial>().id, itemIndex, transform.GetSiblingIndex(), false);
            i.GetComponent<PlayerInventoryManager>().SpendItem(ae.equippedItem.GetComponent<Item>());
            return true;
        }
        else if (hasItem)
        {
            currentItem.isEquipable = true;
            ae.GrabItem(currentItem);
            LevelManager.Instance.CallSpellCirclePedestalPRC(transform.parent.GetComponent<BuildingMaterial>().id, currentItem.itemListIndex, transform.GetSiblingIndex(), true);

            return true;
        }
        return false;
    }
}
