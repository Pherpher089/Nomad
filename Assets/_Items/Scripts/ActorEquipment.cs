using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ActorEquipment : MonoBehaviour
{
    public GameObject equippedItem;
    public bool hasItem;
    private Item newItem;
    public Transform[] m_HandSockets = new Transform[2];
    private List<Item> grabableItems = new List<Item>();
    private PlayerInventoryManager inventoryManager;
    private bool showInventoryUI;
    CharacterManager characterManager;
    public Animator m_Animator;//public for debug
    public bool isPlayer = false;
    public TheseHands[] m_TheseHandsArray = new TheseHands[2];
    ItemManager m_ItemManager;
    PhotonView pv;

    public void Awake()
    {
        if (tag == "Player")
        {
            isPlayer = true;
        }
        characterManager = GetComponent<CharacterManager>();
        inventoryManager = GetComponent<PlayerInventoryManager>();
        m_ItemManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<ItemManager>();
        pv = GetComponent<PhotonView>();
        hasItem = false;
        m_Animator = GetComponentInChildren<Animator>();
        m_TheseHandsArray = GetComponentsInChildren<TheseHands>();
        m_HandSockets = new Transform[2];
        GetHandSockets(transform);
    }

    void Start()
    {
        if (equippedItem != null)
        {
            PackableItem pi = equippedItem.GetComponent<PackableItem>();
            GameObject newEquipment = Instantiate(equippedItem);
            newEquipment.GetComponent<SpawnMotionDriver>().hasSaved = true;
            newEquipment.GetComponent<Rigidbody>().isKinematic = true;
            EquipItem(equippedItem.GetComponent<Item>());
            if (pi != null)
            {
                pi.PackAndSave(this.gameObject);
            }
            if (inventoryManager != null)
            {
                hasItem = true;
                inventoryManager.UpdateUiWithEquippedItem(newEquipment.GetComponent<Item>().icon);
            }
        }
        else
        {
            ToggleTheseHands(true);
        }
    }


    void GetHandSockets(Transform _transform)
    {
        foreach (Transform t in _transform.GetComponentInChildren<Transform>())
        {
            if (t.gameObject.tag == "HandSocket")
            {
                if (t.gameObject.name == "LeftHandSocket")
                {
                    m_HandSockets[0] = t;
                }
                else
                {
                    m_HandSockets[1] = t;
                }
            }
            else
            {
                if (t.childCount > 0)
                {
                    GetHandSockets(t);
                }
            }
        }
    }

    public void AddItemToInventory(Item item)
    {
        if (item.fitsInBackpack)
        {
            inventoryManager.AddItem(item, 1);
            item.gameObject.SetActive(false);
        }
        if (isPlayer) characterManager.SaveCharacter();

    }

    public void EquipItem(GameObject item)
    {
        Item _item = item.GetComponent<Item>();
        if (_item.itemName == "Basic Crafting Bench")
        {
            _item.isEquipable = true;
        }
        if (_item.isEquipable)
        {
            hasItem = true;
            int handSocketIndex = _item.itemAnimationState == 1 ? 0 : 1;
            GameObject newItem = Instantiate(m_ItemManager.GetPrefabByItem(_item), m_HandSockets[handSocketIndex].position, m_HandSockets[handSocketIndex].rotation, m_HandSockets[handSocketIndex]);
            SpawnMotionDriver smd = newItem.GetComponent<SpawnMotionDriver>();
            if (smd != null)
            {
                //Crafting benches or other packables do not have or need a spawn motion dirver.
                newItem.GetComponent<SpawnMotionDriver>().hasSaved = true;
            }
            newItem.GetComponent<Rigidbody>().isKinematic = true;
            equippedItem = newItem;
            Item[] itemScripts = equippedItem.GetComponents<Item>();
            foreach (Item itm in itemScripts)
            {
                itm.OnEquipped(this.gameObject);
                itm.gameObject.SetActive(true);
            }
            equippedItem.GetComponent<Item>().OnEquipped(this.gameObject);
            equippedItem.gameObject.SetActive(true);
            //Change the animator state to handle the item equipped
            m_Animator.SetInteger("ItemAnimationState", _item.itemAnimationState);
            //Destroy(item);
            ToggleTheseHands(false);
            if (equippedItem.GetComponent<Item>().itemName == "Basic Crafting Bench")
            {
                equippedItem.GetComponent<BuildingObject>().isPlaced = true;
                equippedItem.GetComponent<PackableItem>().JustPack();
            }
        }
    }
    public GameObject EquipItem(Item item)
    {
        if (item.isEquipable)
        {
            hasItem = true;
            int handSocketIndex = item.itemAnimationState == 1 ? 0 : 1;
            equippedItem = Instantiate(m_ItemManager.GetPrefabByItem(item), m_HandSockets[handSocketIndex].position, m_HandSockets[handSocketIndex].rotation, m_HandSockets[handSocketIndex]);
            equippedItem.GetComponent<SpawnMotionDriver>().hasSaved = true;
            equippedItem.GetComponent<Rigidbody>().isKinematic = true;
            equippedItem.GetComponent<Item>().OnEquipped(this.gameObject);
            equippedItem.gameObject.SetActive(true);
            //Change the animator state to handle the item equipped
            m_Animator.SetInteger("ItemAnimationState", item.itemAnimationState);
            ToggleTheseHands(false);
            pv.RPC("EquipItemClient", RpcTarget.OthersBuffered, equippedItem.GetComponent<Item>().itemIndex, handSocketIndex != 0);

        }
        if (isPlayer) characterManager.SaveCharacter();
        return equippedItem;
    }

    [PunRPC]
    public void EquipItemClient(int itemIndex, bool offHand)
    {
        Debug.Log("Calling equipment RPC");
        // Fetch the item from the manager using the ID
        GameObject item = m_ItemManager.GetItemByIndex(itemIndex);

        // Make sure the item is equipable
        if (item != null && item.GetComponent<Item>().isEquipable == true)
        {
            hasItem = true;
            int handSocketIndex = offHand == false ? 0 : 1;
            GameObject newItem = Instantiate(item, m_HandSockets[handSocketIndex].position, m_HandSockets[handSocketIndex].rotation, m_HandSockets[handSocketIndex]);
            newItem.GetComponent<SpawnMotionDriver>().hasSaved = true;
            newItem.GetComponent<Rigidbody>().isKinematic = true;
            equippedItem = newItem;
            equippedItem.GetComponent<Item>().OnEquipped(this.gameObject);
            equippedItem.gameObject.SetActive(true);
        }
    }

    void ToggleTheseHands(bool toggle)
    {
        foreach (TheseHands th in m_TheseHandsArray)
        {
            th.gameObject.GetComponent<Collider>().enabled = toggle;
        }
    }

    public void UnequippedItem()
    {
        hasItem = false;
        Item item = equippedItem.GetComponent<Item>();
        item.OnUnequipped();
        item.inventoryIndex = -1;
        equippedItem.transform.parent = null;
        bool isPacked = false;
        if (item.GetComponent<PackableItem>() != null)
        {
            item.GetComponent<BuildingObject>().isPlaced = true;
            isPacked = true;
            ItemManager.Instance.CallDropItemRPC(item.itemIndex, transform.position, isPacked);
        }
        Destroy(equippedItem);
        m_Animator.SetInteger("ItemAnimationState", 0);
        ToggleTheseHands(true);
        pv.RPC("UnequippedItemClient", RpcTarget.OthersBuffered);
        if (isPlayer) characterManager.SaveCharacter();

    }


    public void UnequippedItem(bool spendItem)
    {
        hasItem = false;
        GameObject itemToDestroy = equippedItem;
        equippedItem.GetComponent<Item>().OnUnequipped();
        if (m_HandSockets[0].childCount > 0)
        {
            foreach (Transform child in m_HandSockets[0])
            {
                Destroy(child.gameObject);
            }
        }
        if (m_HandSockets[1].childCount > 0)
        {
            foreach (Transform child in m_HandSockets[1])
            {
                Destroy(child.gameObject);
            }
        }

        equippedItem = null;
        m_Animator.SetInteger("ItemAnimationState", 0);
        ToggleTheseHands(true);
        pv.RPC("UnequippedItemClient", RpcTarget.OthersBuffered);
        if (isPlayer) characterManager.SaveCharacter();
    }

    [PunRPC]
    public void UnequippedItemClient()
    {
        if (pv.IsMine) return;
        hasItem = false;
        if (equippedItem != null)
        {
            equippedItem?.GetComponent<Item>()?.OnUnequipped();
            Destroy(equippedItem.gameObject);
        }
    }

    public void UnequippedToInventory()
    {
        if (equippedItem.GetComponent<Item>().fitsInBackpack)
        {
            AddItemToInventory(equippedItem.GetComponent<Item>());
            //Set animator state to unarmed
            m_Animator.SetInteger("ItemAnimationState", 0);
            // Turn these hands on
            ToggleTheseHands(true);
            Destroy(equippedItem);
            equippedItem = null;
            hasItem = false;
            pv.RPC("UnequippedItemClient", RpcTarget.AllBuffered);
            //If this is not an npc, save the character
            if (isPlayer) characterManager.SaveCharacter();

        }
        else
        {
            //If this item is not able to fit in the back pack, unequip
            UnequippedItem();
        }

    }

    public void SpendItem()
    {
        Item item = equippedItem.GetComponent<Item>();
        if (item.inventoryIndex >= 0 && inventoryManager.items[item.inventoryIndex].count > 0)
        {
            inventoryManager.RemoveItem(item.inventoryIndex, 1);
            if (isPlayer) characterManager.SaveCharacter();
        }
        else
        {
            UnequippedItem(true);
        }
    }


    // Finds all not equiped items in the screen that are close enough to the player to grab and adds them to the grabableItems list. This function also returns the closest
    Item GatherAllItemsInScene()
    {
        Item[] allItems = GameObject.FindObjectsOfType<Item>();
        Item closestItem = null;
        float closestDist = 5;
        foreach (Item item in allItems)
        {
            if (!item.isEquipped && item.isEquipable)
            {
                float currentItemDist = Vector3.Distance(transform.position, item.gameObject.transform.position);

                if (currentItemDist < 3)
                {
                    if (currentItemDist < closestDist)
                    {
                        //TODO check for player direction as well to stop players from picking up unintended items

                        closestDist = currentItemDist;
                        closestItem = item;
                        Outline outline = closestItem.GetComponent<Outline>();
                        if (outline != null)
                        {
                            outline.enabled = true;
                        }
                    }
                    else
                    {
                        if (item.GetComponent<Outline>() != null)
                        {
                            item.GetComponent<Outline>().enabled = false;
                        }
                    }

                    grabableItems.Add(item);//TODO a list?
                }
            }
        }

        if (grabableItems.Count <= 0)
            return null;
        else
            return closestItem;
    }

    public void GrabItem()
    {
        newItem = GatherAllItemsInScene();

        if (newItem == null || newItem.itemName == "Fire Pit" || !newItem.hasLanded)
        {
            return;
        }
        if (hasItem || newItem.gameObject.tag != "Tool" && newItem.gameObject.tag != "Food")
        {
            if (newItem != null)
            {
                if (!newItem.isEquipable) return;
                if (newItem.fitsInBackpack)
                {
                    AddItemToInventory(newItem);
                }
                else
                {
                    if (hasItem)
                    {
                        UnequippedItem();
                    }
                    EquipItem(m_ItemManager.GetPrefabByItem(newItem));
                }
                LevelManager.Instance.CallUpdateItemsRPC(newItem.id);
                newItem.SaveItem(newItem.parentChunk, true);
                if (isPlayer) characterManager.SaveCharacter();
            }
        }
        else
        {
            if (newItem != null)
            {
                newItem.inventoryIndex = -1;
                EquipItem(m_ItemManager.GetPrefabByItem(newItem));
                LevelManager.Instance.CallUpdateItemsRPC(newItem.id);
                newItem.SaveItem(newItem.parentChunk, true);
                if (isPlayer) characterManager.SaveCharacter();
            }
        }
    }
}
