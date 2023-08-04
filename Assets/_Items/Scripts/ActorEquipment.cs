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
        if (equippedItem != null)
        {
            GameObject newEquipment = Instantiate(equippedItem);
            newEquipment.GetComponent<SpawnMotionDriver>().hasSaved = true;
            newEquipment.GetComponent<Rigidbody>().isKinematic = true;
            EquipItem(equippedItem.GetComponent<Item>());
        }
    }

    void Start()
    {

        if (equippedItem != null)
        {
            if (tag == "Player")
            {
                hasItem = true;
                inventoryManager.UpdateUiWithEquippedItem(equippedItem.GetComponent<Item>().icon);
            }
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
        if (_item.isEquipable)
        {
            hasItem = true;
            int handSocketIndex = _item.itemAnimationState == 1 ? 0 : 1;
            GameObject newItem = Instantiate(m_ItemManager.GetPrefabByItem(_item), m_HandSockets[handSocketIndex].position, m_HandSockets[handSocketIndex].rotation, m_HandSockets[handSocketIndex]);
            newItem.GetComponent<SpawnMotionDriver>().hasSaved = true;
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
        }
    }
    public void EquipItem(Item item)
    {
        if (item.isEquipable)
        {
            hasItem = true;
            int handSocketIndex = item.itemAnimationState == 1 ? 0 : 1;
            GameObject newItem = Instantiate(m_ItemManager.GetPrefabByItem(item), m_HandSockets[handSocketIndex].position, m_HandSockets[handSocketIndex].rotation, m_HandSockets[handSocketIndex]);
            newItem.GetComponent<SpawnMotionDriver>().hasSaved = true;
            newItem.GetComponent<Rigidbody>().isKinematic = true;
            equippedItem = newItem;
            equippedItem.GetComponent<Item>().OnEquipped(this.gameObject);
            equippedItem.gameObject.SetActive(true);
            //Change the animator state to handle the item equipped
            m_Animator.SetInteger("ItemAnimationState", item.itemAnimationState);
            ToggleTheseHands(false);
            pv.RPC("EquipItemClient", RpcTarget.OthersBuffered, equippedItem.GetComponent<Item>().itemIndex, handSocketIndex == 0 ? false : true);

        }
        if (isPlayer) characterManager.SaveCharacter();
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
        equippedItem.GetComponent<Item>().OnUnequipped();
        equippedItem.GetComponent<Item>().inventoryIndex = -1;
        equippedItem.transform.parent = null;
        m_Animator.SetInteger("ItemAnimationState", 0);

        ToggleTheseHands(true);
        if (isPlayer) characterManager.SaveCharacter();
        pv.RPC("UnequippedItemClient", RpcTarget.AllBuffered);

    }


    public void UnequippedItem(bool spendItem)
    {
        hasItem = false;
        equippedItem.GetComponent<Item>().OnUnequipped();
        Object.Destroy(equippedItem.gameObject);
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
        equippedItem?.GetComponent<Item>()?.OnUnequipped();
        Destroy(equippedItem.gameObject);
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
        if (equippedItem.GetComponent<Item>().inventoryIndex >= 0 && inventoryManager.items[equippedItem.GetComponent<Item>().inventoryIndex].count > 0)
        {
            inventoryManager.RemoveItem(equippedItem.GetComponent<Item>().inventoryIndex, 1);
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
            if (!item.isEquipped)
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

        if (hasItem)
        {
            if (newItem != null)
            {

                if (newItem.fitsInBackpack)
                {
                    AddItemToInventory(newItem);
                }
                else
                {
                    UnequippedItem();
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
