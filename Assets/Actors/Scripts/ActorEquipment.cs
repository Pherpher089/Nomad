using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorEquipment : MonoBehaviour
{
    public Item equipedItem;
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

    public void Awake()
    {
        if (tag == "Player")
        {
            isPlayer = true;
        }
        characterManager = GetComponent<CharacterManager>();
        inventoryManager = gameObject.GetComponent<PlayerInventoryManager>();
    }
    public void Start()
    {
        hasItem = false;
        m_Animator = GetComponentInChildren<Animator>();
        m_TheseHandsArray = GetComponentsInChildren<TheseHands>();
        m_HandSockets = new Transform[2];
        GetHandSockets(transform);

        if (equipedItem)
            EquipItem(equipedItem);
    }
    void GetHandSockets(Transform _transform)
    {
        foreach (Transform t in _transform.GetComponentInChildren<Transform>())
        {
            if (t.gameObject.tag == "HandSocket")
            {
                if (t.gameObject.name == "LeftHandSocket")
                {
                    Debug.Log("$$$ " + t.gameObject.tag);

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

    public void EquipItem(Item item)
    {
        if (item.isEquipable)
        {
            hasItem = true;
            int handSocketIndex = item.itemAnimationState == 1 ? 0 : 1;
            equipedItem = Instantiate(item, m_HandSockets[handSocketIndex].position, m_HandSockets[handSocketIndex].rotation, m_HandSockets[handSocketIndex]);
            equipedItem.transform.Rotate(90, 0, 0);
            equipedItem.GetComponent<Item>().OnEquipt(this.gameObject);
            equipedItem.gameObject.SetActive(true);
            //Change the animator state to handle the item equipped
            m_Animator.SetInteger("ItemAnimationState", item.itemAnimationState);
            ToggleTheseHands(false);
        }
        if (isPlayer) characterManager.SaveCharacter();
    }

    void ToggleTheseHands(bool toggle)
    {
        foreach (TheseHands th in m_TheseHandsArray)
        {
            th.gameObject.GetComponent<SphereCollider>().enabled = toggle;
        }
    }

    public void UnequipItem()
    {
        hasItem = false;
        equipedItem.GetComponent<Item>().OnUnequipt();
        equipedItem.inventoryIndex = -1;
        equipedItem.transform.parent = null;
        m_Animator.SetInteger("ItemAnimationState", 0);

        ToggleTheseHands(true);
        if (isPlayer) characterManager.SaveCharacter();

    }
    public void UnequipItem(bool spendItem)
    {
        hasItem = false;
        equipedItem.GetComponent<Item>().OnUnequipt();
        Destroy(equipedItem.gameObject);
        m_Animator.SetInteger("ItemAnimationState", 0);

        ToggleTheseHands(true);
        if (isPlayer) characterManager.SaveCharacter();

    }

    public void UnequippedToInventory()
    {
        if (equipedItem.fitsInBackpack)
        {
            AddItemToInventory(equipedItem);
            //Set animator state to unarmed
            m_Animator.SetInteger("ItemAnimationState", 0);
            // Turn these hands on
            ToggleTheseHands(true);
            //If this is not an npc, save the character
            if (isPlayer) characterManager.SaveCharacter();

        }
        else
        {
            //If this item is not able to fit in the back pack, unequip
            UnequipItem();
        }

    }

    public void SpendItem()
    {
        if (equipedItem.inventoryIndex >= 0 && inventoryManager.items[equipedItem.inventoryIndex].count > 0)
        {
            inventoryManager.RemoveItem(equipedItem.inventoryIndex, 1);
            if (isPlayer) characterManager.SaveCharacter();

        }
        else
        {
            UnequipItem(true);
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
            if (!item.isEquiped)
            {
                float currentItemDist = Vector3.Distance(transform.position, item.gameObject.transform.position);

                if (currentItemDist < 3)
                {
                    if (currentItemDist < closestDist)
                    {
                        //TODO check for player direction as well to stop players from picking up unintended items

                        closestDist = currentItemDist;
                        closestItem = item;
                        closestItem.GetComponent<Outline>().enabled = true;
                    }
                    else
                    {
                        item.GetComponent<Outline>().enabled = false;
                    }

                    grabableItems.Add(item);//TODO a list?
                    if (isPlayer) characterManager.SaveCharacter();

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

        if (hasItem)
        {
            if (newItem != null)
            {
                if (newItem.fitsInBackpack)
                {
                    AddItemToInventory(newItem);
                    if (isPlayer) characterManager.SaveCharacter();

                }
                else
                {
                    UnequipItem();
                    EquipItem(newItem);
                    //Destroy(newItem.gameObject);
                    if (isPlayer) characterManager.SaveCharacter();

                }
            }
        }
        else
        {
            if (newItem != null)
            {
                EquipItem(newItem);
                //Destroy(newItem.gameObject);
                if (isPlayer) characterManager.SaveCharacter();
            }
        }
    }
}
