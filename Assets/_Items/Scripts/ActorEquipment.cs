using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;

public class ActorEquipment : MonoBehaviour
{
    //Item the player has equipped
    public GameObject equippedItem;
    //The armor the player has equipped
    public GameObject[] equippedArmor = new GameObject[3];
    // Does the player have an item? -- I think this could just be switch to is EquippedItem == null?
    public bool hasItem;
    private Item newItem;
    public Transform[] m_HandSockets = new Transform[2];
    public Transform[] m_ArmorSockets = new Transform[3];
    private List<Item> grabableItems = new List<Item>();
    [HideInInspector]
    public PlayerInventoryManager inventoryManager;
    private bool showInventoryUI;
    CharacterManager characterManager;
    public Animator m_Animator;//public for debug
    public bool isPlayer = false;
    public TheseHands[] m_TheseHandsArray = new TheseHands[2];
    public TheseFeet[] m_TheseFeetArray = new TheseFeet[2];
    ItemManager m_ItemManager;
    PhotonView pv;

    public void Awake()
    {
        characterManager = GetComponent<CharacterManager>();
        inventoryManager = GetComponent<PlayerInventoryManager>();
        m_ItemManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<ItemManager>();
        pv = GetComponent<PhotonView>();
        hasItem = false;
        m_Animator = GetComponentInChildren<Animator>();
        m_TheseHandsArray = GetComponentsInChildren<TheseHands>();
        m_TheseFeetArray = GetComponentsInChildren<TheseFeet>();
        m_HandSockets = new Transform[2];
        equippedArmor = new GameObject[3];
        GetSockets(transform);
        if (tag == "Player")
        {
            isPlayer = true;
        }
        else
        {
            if (equippedItem != null)
            {
                EquipItem(equippedItem);
            }
        }
    }

    void Start()
    {
        ToggleTheseHands(true);
    }


    void GetSockets(Transform _transform)
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
            else if (t.gameObject.tag == "ArmorSocket")
            {
                switch (t.gameObject.name)
                {
                    case "HeadSocket":
                        m_ArmorSockets[0] = t;
                        break;
                    case "ChestSocket":
                        m_ArmorSockets[1] = t;
                        break;
                    case "LegsSocket":
                        m_ArmorSockets[2] = t;
                        break;
                }
            }
            else
            {
                if (t.childCount > 0)
                {
                    GetSockets(t);
                }
            }

        }
    }

    public bool AddItemToInventory(Item item)
    {
        bool wasAdded = false;
        if (item.fitsInBackpack)
        {
            wasAdded = inventoryManager.AddItem(ItemManager.Instance.GetItemGameObjectByItemIndex(item.itemIndex).GetComponent<Item>(), 1);
        }
        if (isPlayer) characterManager.SaveCharacter();
        return wasAdded;
    }
    void ToggleTheseHands(bool toggle)
    {
        foreach (TheseHands th in m_TheseHandsArray)
        {
            th.gameObject.GetComponent<Collider>().enabled = toggle;
        }
    }

    public void EquipItem(GameObject item)
    {
        Item _item = item.GetComponent<Item>();
        int socketIndex;
        GameObject _newItem;
        if (_item.isEquipable)
        {
            // if item is armor
            if (item.TryGetComponent<Armor>(out var armor))
            {
                socketIndex = (int)armor.m_ArmorType;
                if (m_ArmorSockets[socketIndex].transform.childCount > 0)
                {
                    Destroy(m_ArmorSockets[socketIndex].transform.GetChild(0).gameObject);
                }
                _newItem = Instantiate(m_ItemManager.GetPrefabByItem(_item), m_ArmorSockets[socketIndex].position, m_ArmorSockets[socketIndex].rotation, m_ArmorSockets[socketIndex]);
                equippedArmor[socketIndex] = _newItem;
            }
            else
            { // If item is not armor, which means, is held in the hands
                hasItem = true;
                socketIndex = _item.itemAnimationState == 1 || _item.itemAnimationState == 4 ? 0 : 1;
                if (m_HandSockets[socketIndex].transform.childCount > 0)
                {
                    Destroy(m_HandSockets[socketIndex].transform.GetChild(0).gameObject);
                }
                _newItem = Instantiate(m_ItemManager.GetPrefabByItem(_item), m_HandSockets[socketIndex].position, m_HandSockets[socketIndex].rotation, m_HandSockets[socketIndex]);
                equippedItem = _newItem;
                //Change the animator state to handle the item equipped
                m_Animator.SetInteger("ItemAnimationState", _item.itemAnimationState);
                ToggleTheseHands(false);
            }
            Item[] itemScripts = _newItem.GetComponents<Item>();
            foreach (Item itm in itemScripts)
            {
                itm.OnEquipped(this.gameObject);
                itm.gameObject.SetActive(true);
            }
            if (_newItem.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = true;
            }
            if (_newItem.TryGetComponent<SpawnMotionDriver>(out var smd))
            {
                //Crafting benches or other packables do not have or need a spawn motion driver.
                _newItem.GetComponent<SpawnMotionDriver>().hasSaved = true;
            }
            pv.RPC("EquipItemClient", RpcTarget.OthersBuffered, _newItem.GetComponent<Item>().itemIndex, socketIndex != 0, pv.ViewID);
            if (isPlayer) characterManager.SaveCharacter();
        }
    }
    public void EquipItem(Item item)
    {
        int socketIndex;
        GameObject _newItem;
        if (item.isEquipable)
        {
            // if item is armor
            if (item.TryGetComponent<Armor>(out var armor))
            {
                socketIndex = (int)armor.m_ArmorType;
                if (m_ArmorSockets[socketIndex].transform.childCount > 0)
                {
                    Destroy(m_ArmorSockets[socketIndex].transform.GetChild(0).gameObject);
                }
                _newItem = Instantiate(m_ItemManager.GetPrefabByItem(item), m_ArmorSockets[socketIndex].position, m_ArmorSockets[socketIndex].rotation, m_ArmorSockets[socketIndex]);
                equippedArmor[socketIndex] = _newItem;
            }
            else
            { // If item is not armor, which means, is held in the hands
                hasItem = true;
                socketIndex = item.itemAnimationState == 1 || item.itemAnimationState == 4 ? 0 : 1;
                _newItem = Instantiate(m_ItemManager.GetPrefabByItem(item), m_HandSockets[socketIndex].position, m_HandSockets[socketIndex].rotation, m_HandSockets[socketIndex]);
                equippedItem = _newItem;
                //Change the animator state to handle the item equipped
                m_Animator.SetInteger("ItemAnimationState", item.itemAnimationState);
                ToggleTheseHands(false);
            }
            Item[] itemScripts = _newItem.GetComponents<Item>();
            foreach (Item itm in itemScripts)
            {
                itm.OnEquipped(this.gameObject);
                itm.gameObject.SetActive(true);
            }
            if (_newItem.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = true;
            }
            if (_newItem.TryGetComponent<SpawnMotionDriver>(out var smd))
            {
                //Crafting benches or other packables do not have or need a spawn motion driver.
                _newItem.GetComponent<SpawnMotionDriver>().hasSaved = true;
            }
            pv.RPC("EquipItemClient", RpcTarget.OthersBuffered, _newItem.GetComponent<Item>().itemIndex, socketIndex != 0, pv.ViewID);
            if (isPlayer) characterManager.SaveCharacter();
        }
    }

    [PunRPC]
    public void EquipItemClient(int itemIndex, bool offHand, int viewId)
    {
        ActorEquipment targetView = PhotonView.Find(viewId).GetComponent<ActorEquipment>();
        Debug.Log("Calling equipment RPC");
        // Fetch the item from the manager using the ID
        GameObject item = targetView.m_ItemManager.GetItemGameObjectByItemIndex(itemIndex);
        int socketIndex;
        Item _item = item.GetComponent<Item>();
        GameObject _newItem;
        // Make sure the item is equipable
        if (item != null && _item.isEquipable == true)
        {
            // if item is armor
            if (item.TryGetComponent<Armor>(out var armor))
            {
                socketIndex = (int)armor.m_ArmorType;
                if (targetView.m_ArmorSockets[socketIndex].transform.childCount > 0)
                {
                    Destroy(targetView.m_ArmorSockets[socketIndex].transform.GetChild(0).gameObject);
                }
                _newItem = Instantiate(targetView.m_ItemManager.GetPrefabByItem(_item), targetView.m_ArmorSockets[socketIndex].position, targetView.m_ArmorSockets[socketIndex].rotation, targetView.m_ArmorSockets[socketIndex]);
                targetView.equippedArmor[socketIndex] = _newItem;
            }
            else
            { // If item is not armor, which means, is held in the hands
                targetView.hasItem = true;
                socketIndex = _item.itemAnimationState == 1 || _item.itemAnimationState == 4 ? 0 : 1;
                _newItem = Instantiate(targetView.m_ItemManager.GetPrefabByItem(_item), targetView.m_HandSockets[socketIndex].position, targetView.m_HandSockets[socketIndex].rotation, targetView.m_HandSockets[socketIndex]);
                targetView.equippedItem = _newItem;
                //Change the animator state to handle the item equipped
                targetView.m_Animator.SetInteger("ItemAnimationState", _item.itemAnimationState);
                ToggleTheseHands(false);
            }
            Item[] itemScripts = _newItem.GetComponents<Item>();
            foreach (Item itm in itemScripts)
            {
                itm.OnEquipped(this.gameObject);
                itm.gameObject.SetActive(true);
            }
            if (_newItem.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = true;
            }
            if (_newItem.TryGetComponent<SpawnMotionDriver>(out var smd))
            {
                //Crafting benches or other packables do not have or need a spawn motion driver.
                _newItem.GetComponent<SpawnMotionDriver>().hasSaved = true;
            }
        }
    }
    public float GetArmorBonus()
    {
        float bonus = 0;
        for (int i = 0; i < 3; i++)
        {
            if (equippedArmor[i] != null)
            {
                bonus += equippedArmor[i].GetComponent<Armor>().m_DefenseValue;
            }
        }
        return bonus;
    }
    public void UnequippedCurrentArmor(ArmorType armorType)
    {
        Item item = equippedArmor[(int)armorType].GetComponent<Item>();
        item.inventoryIndex = -1;
        equippedArmor[(int)armorType] = null;
        Destroy(item);
        pv.RPC("UnequippedCurrentArmorClient", RpcTarget.OthersBuffered, armorType);
        if (isPlayer) characterManager.SaveCharacter();
    }
    public bool UnequippedCurrentArmorToInventory(ArmorType armorType)
    {
        if (equippedArmor[(int)armorType] != null)
        {
            bool canUnequipped = AddItemToInventory(ItemManager.Instance.GetItemGameObjectByItemIndex(equippedArmor[(int)armorType].GetComponent<Item>().itemIndex).GetComponent<Item>());
            if (!canUnequipped) return false;
            //Set animator state to unarmed
            // Turn these hands on
            Destroy(equippedArmor[(int)armorType]);
            equippedArmor[(int)armorType] = null;
            pv.RPC("UnequippedCurrentArmorClient", RpcTarget.AllBuffered, armorType);
            //If this is not an npc, save the character
            if (isPlayer) characterManager.SaveCharacter();
        }
        return true;
    }

    [PunRPC]
    public void UnequippedCurrentArmorClient(ArmorType armorType)
    {
        if (pv.IsMine) return;
        if (equippedArmor[(int)armorType] != null)
        {
            equippedArmor[(int)armorType].GetComponent<Item>().OnUnequipped();
            Destroy(equippedArmor[(int)armorType]);
        }
    }
    public void UnequippedCurrentItem()
    {
        if (equippedItem != null)
        {
            hasItem = false;
            Item item = equippedItem.GetComponent<Item>();
            item.OnUnequipped();
            item.inventoryIndex = -1;
            equippedItem.transform.parent = null;

            Destroy(equippedItem);
            m_Animator.SetInteger("ItemAnimationState", 0);
            ToggleTheseHands(true);
            pv.RPC("UnequippedCurrentItemClient", RpcTarget.OthersBuffered);
            if (isPlayer) characterManager.SaveCharacter();
        }

    }


    public void UnequippedCurrentItem(bool spendItem)
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
        pv.RPC("UnequippedCurrentItemClient", RpcTarget.OthersBuffered);
        if (isPlayer) characterManager.SaveCharacter();
    }

    [PunRPC]
    public void UnequippedCurrentItemClient()
    {
        if (pv.IsMine) return;
        hasItem = false;
        if (equippedItem != null)
        {
            equippedItem?.GetComponent<Item>()?.OnUnequipped();
            Destroy(equippedItem.gameObject);
        }
    }

    public bool UnequippedCurrentItemToInventory()
    {
        if (equippedItem != null && equippedItem.GetComponent<Item>().fitsInBackpack)
        {
            bool canReturnToInventory = AddItemToInventory(ItemManager.Instance.GetItemGameObjectByItemIndex(equippedItem.GetComponent<Item>().itemIndex).GetComponent<Item>());

            if (!canReturnToInventory)
            {
                return false;
            }
            //Set animator state to unarmed
            m_Animator.SetInteger("ItemAnimationState", 0);
            // Turn these hands on
            ToggleTheseHands(true);
            Destroy(equippedItem);
            equippedItem = null;
            hasItem = false;
            pv.RPC("UnequippedCurrentItemClient", RpcTarget.AllBuffered);
            //If this is not an npc, save the character
            if (isPlayer) characterManager.SaveCharacter();

        }
        else
        {
            //If this item is not able to fit in the back pack, unequip
            UnequippedCurrentItem();
        }
        return true;

    }

    public void SpendItem()
    {
        Item item; equippedItem.GetComponent<Item>();
        if (equippedItem == null) return;
        item = equippedItem.GetComponent<Item>();
        if (item == null) return;
        if (item.inventoryIndex >= 0 && inventoryManager.items[item.inventoryIndex].count > 0)
        {
            inventoryManager.RemoveItem(item.inventoryIndex, 1);
            if (isPlayer) characterManager.SaveCharacter();
        }
        else
        {
            UnequippedCurrentItem(true);
        }
    }
    public void SpendItem(Item item)
    {
        foreach (ItemStack stack in inventoryManager.items)
        {
            if (stack.item != null && stack.item.name == item.name)
            {
                if (item.inventoryIndex >= 0 && inventoryManager.items[item.inventoryIndex].count > 0)
                {
                    inventoryManager.RemoveItem(item.inventoryIndex, 1);
                    if (isPlayer) characterManager.SaveCharacter();
                    break;
                }
                else
                {
                    UnequippedCurrentItem(true);
                    break;
                }
            }

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

    public void ShootBow()
    {
        Vector3 direction = transform.forward;
        if (tag == "Enemy")
        {

            direction = GetComponent<StateController>().target.position + Vector3.up * 2 - (transform.position + transform.forward + (transform.up * 1.5f));
            direction = direction.normalized;
        }
        else
        {
            bool hasArrows = false;
            for (int i = 0; i < inventoryManager.items.Length; i++)
            {
                if (inventoryManager.items[i].item && inventoryManager.items[i].item.itemIndex == 14 && inventoryManager.items[i].count > 0)
                {
                    hasArrows = true;
                    inventoryManager.RemoveItem(i, 1);
                    break;
                }
            }

            if (!hasArrows) return;
        }
        GameObject arrow = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Arrow"), transform.position + (transform.forward * 1.5f) + (transform.up * 2f), Quaternion.LookRotation(direction));
        arrow.GetComponent<ArrowControl>().Initialize(gameObject, equippedItem);
        arrow.GetComponent<Rigidbody>().velocity = direction * 80;
        arrow.GetComponent<Rigidbody>().useGravity = true;
    }

    public void CastWand()
    {
        //TODO check for mana?
        // bool hasArrows = false;
        // foreach (ItemStack stack in inventoryManager.items)
        // {
        //     if (stack.item && stack.item.name.Contains("Arrow") && stack.count > 1)
        //     {
        //         hasArrows = true;
        //         arrowItem = stack.item;
        //         inventoryManager.RemoveItem(stack.index, 1);
        //         break;
        //     }
        // }
        // if (!hasArrows) return;
        GameObject fireBall = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "FireBall"), transform.position + transform.forward + (transform.up * 1.5f), Quaternion.LookRotation(transform.forward));
        fireBall.GetComponent<FireBallControl>().Initialize(gameObject, equippedItem);
        fireBall.GetComponent<Rigidbody>().velocity = (transform.forward * 20);
    }
    public void CastWandArc()
    {
        //TODO check for mana?
        // bool hasArrows = false;
        // foreach (ItemStack stack in inventoryManager.items)
        // {
        //     if (stack.item && stack.item.name.Contains("Arrow") && stack.count > 1)
        //     {
        //         hasArrows = true;
        //         arrowItem = stack.item;
        //         inventoryManager.RemoveItem(stack.index, 1);
        //         break;
        //     }
        // }
        // if (!hasArrows) return;
        GameObject fireBall = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "FireBall"), transform.position + (transform.forward * 1.5f) + (transform.up * 1.5f), Quaternion.LookRotation(transform.forward));
        fireBall.GetComponent<FireBallControl>().Initialize(gameObject, equippedItem);
        fireBall.GetComponent<Rigidbody>().velocity = (transform.forward * 7) + (transform.up * 15);
        fireBall.GetComponent<Rigidbody>().useGravity = true;
    }

    public void GrabItem()
    {
        newItem = GatherAllItemsInScene();
        if (newItem == null || newItem.itemName == "Fire Pit" || !newItem.hasLanded)
        {
            return;
        }
        if (newItem.TryGetComponent(out BuildingObject bo) && bo.isPlaced)
        {
            return;
        }
        if (hasItem || newItem.gameObject.tag != "Tool" && newItem.gameObject.tag != "Food")
        {
            if (newItem != null)
            {
                if (!newItem.isEquipable)
                {
                    Debug.LogError($"{newItem.name} has isEquipable set to false");
                    return;
                };
                if (newItem.fitsInBackpack)
                {
                    bool wasAdded = AddItemToInventory(newItem);
                    if (!wasAdded)
                    {
                        Debug.Log("### 2");
                        LevelManager.Instance.CallUpdateItemsRPC(newItem.spawnId);
                        PlayerInventoryManager.Instance.DropItem(newItem.itemIndex, newItem.transform.position);
                        return;
                    };
                }
                else
                {
                    if (hasItem)
                    {
                        UnequippedCurrentItem();
                    }
                    EquipItem(m_ItemManager.GetPrefabByItem(newItem));
                }
                LevelManager.Instance.CallUpdateItemsRPC(newItem.spawnId);
                //newItem.SaveItem(newItem.parentChunk, true);
                if (isPlayer) characterManager.SaveCharacter();
            }
        }
        else
        {
            if (newItem != null)
            {
                newItem.inventoryIndex = -1;
                EquipItem(m_ItemManager.GetPrefabByItem(newItem));
                LevelManager.Instance.CallUpdateItemsRPC(newItem.spawnId);
                //newItem.SaveItem(newItem.parentChunk, true);
                if (isPlayer) characterManager.SaveCharacter();
            }
        }
    }
    public void GrabItem(Item item)
    {
        newItem = item;
        if (hasItem)
        {
            if (newItem != null)
            {
                Debug.Log("### 0");
                if (!newItem.isEquipable) return;
                if (newItem.fitsInBackpack && inventoryManager)
                {
                    Debug.Log("### 1");
                    bool wasAdded = AddItemToInventory(m_ItemManager.GetPrefabByItem(newItem).GetComponent<Item>());
                    if (!wasAdded)
                    {
                        Debug.Log("### 2");
                        LevelManager.Instance.CallUpdateItemsRPC(newItem.spawnId);
                        PlayerInventoryManager.Instance.DropItem(newItem.itemIndex, newItem.transform.position);
                        return;
                    };
                }
                else
                {
                    Debug.Log("### 3");
                    if (hasItem)
                    {
                        Debug.Log("### 4");
                        UnequippedCurrentItem();
                    }
                    Debug.Log("### 5");
                    EquipItem(newItem);
                }
                LevelManager.Instance.CallUpdateItemsRPC(newItem.spawnId);
                Debug.Log("### 6");
            }
        }
        else
        {
            if (newItem != null)
            {
                newItem.inventoryIndex = -1;
                EquipItem(m_ItemManager.GetPrefabByItem(newItem));
                LevelManager.Instance.CallUpdateItemsRPC(newItem.spawnId);
            }
        }
        if (isPlayer) characterManager.SaveCharacter();
    }

}
