using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public class SaddleStationUIController : MonoBehaviour
{
    public Sprite inventorySlotSprite;
    //The UI GameObject
    public bool isOpen = false;
    public GameObject playerCurrentlyUsing = null;
    public CraftingSlot[] inventorySlots;
    CraftingSlot equippedItemSlot;
    GameObject cursor;
    string playerPrefix;
    Dictionary<Item, List<int>> CraftingItems;
    bool uiReturn = false;                         //Tracks the return of the input axis because they are not boolean input
    int cursorIndex = 0;
    public Dictionary<int[], int> craftingRecipes;
    public ItemStack[] items;
    GameObject infoPanel;
    BuildingMaterial m_BuildingMaterial;
    GameObject[] buttonPrompts;

    void Start()
    {
        Initialize();
    }
    public string ArrayToString(int[] array)
    {
        return string.Join(",", array.Select(i => i.ToString()).ToArray());
    }
    public void Initialize()
    {
        m_BuildingMaterial = GetComponentInParent<BuildingMaterial>();
        CraftingItems = new Dictionary<Item, List<int>>();
        inventorySlots = new CraftingSlot[9];
        for (int i = 0; i < 9; i++)
        {
            inventorySlots[i] = transform.GetChild(0).GetChild(i).GetComponent<CraftingSlot>();
            inventorySlots[i].currentItemStack = new ItemStack(null, 0, -1, true);
            inventorySlots[i].isOccupied = false;
            inventorySlots[i].quantText.text = "";
            inventorySlots[i].spriteRenderer.sprite = null;
        }

        cursor = transform.GetChild(0).GetChild(10).gameObject;
        infoPanel = transform.GetChild(0).GetChild(11).gameObject;
        transform.GetChild(0).gameObject.SetActive(false);
        equippedItemSlot = transform.GetChild(0).GetChild(9).GetComponent<CraftingSlot>();
        isOpen = false;
        UpdateButtonPrompts();
    }
    public void UpdateButtonPrompts()
    {
        if (!GameStateManager.Instance.showOnScreenControls)
        {

            int buttonPromptChildCount = transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).childCount;
            for (int i = 0; i < buttonPromptChildCount; i++)
            {
                transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(1).GetChild(i).gameObject.SetActive(false);
                transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).GetChild(i).gameObject.SetActive(false);

            }
            return;

        }
        if (!LevelPrep.Instance.firstPlayerGamePad)
        {
            int buttonPromptChildCount = transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).childCount;
            buttonPrompts = new GameObject[buttonPromptChildCount];
            for (int i = 0; i < buttonPromptChildCount; i++)
            {
                transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(1).GetChild(i).gameObject.SetActive(true);
                buttonPrompts[i] = transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(1).GetChild(i).gameObject;

            }
            for (int i = 0; i < buttonPromptChildCount; i++)
            {
                transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).GetChild(i).gameObject.SetActive(false);
            }
        }
        else
        {
            int buttonPromptChildCount = transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).childCount;
            buttonPrompts = new GameObject[buttonPromptChildCount];
            for (int i = 0; i < buttonPromptChildCount; i++)
            {
                transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).GetChild(i).gameObject.SetActive(true);
                buttonPrompts[i] = transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).GetChild(i).gameObject;
            }
            for (int i = 0; i < buttonPromptChildCount; i++)
            {
                transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(1).GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    void MoveCursor(int index)
    {
        cursor.transform.position = inventorySlots[index].transform.position;
        if (inventorySlots[index].currentItemStack.item != null)
        {
            UpdateInfoPanel(inventorySlots[index].currentItemStack.item.itemName, inventorySlots[index].currentItemStack.item.itemDescription, inventorySlots[index].currentItemStack.item.value, 0);
        }
        else
        {
            UpdateInfoPanel("", "", 0, 0);
        }
    }

    public void Update()
    {
        if (playerCurrentlyUsing != null)
        {
            ListenToDirectionalInput();
            ListenToActionInput();
        }
    }

    void ListenToActionInput()
    {
        if (Input.GetButtonDown(playerPrefix + "Grab"))
        {
            EquippedBeastItem();
        }
    }

    // listen for input associated to the player prefix;
    void ListenToDirectionalInput()
    {
        float v = Input.GetAxisRaw(playerPrefix + "Vertical");
        float h = Input.GetAxisRaw(playerPrefix + "Horizontal");

        if (uiReturn && v < 0.1f && h < 0.1f && v > -0.1f && h > -0.1f)
        {
            uiReturn = false;
        }

        if (playerPrefix == "sp")
        {
            if (Input.GetButtonDown(playerPrefix + "Horizontal") || Input.GetButtonDown(playerPrefix + "Vertical"))
            {
                MoveCursor(new Vector2(h, v));
            }
        }
        else
        {
            if (!uiReturn && v + h != 0)
            {
                MoveCursor(new Vector2(h, v));
                uiReturn = true;
            }
        }
    }

    void MoveCursor(Vector2 direction)
    {
        if (direction.x > 0 && cursorIndex != 2 && cursorIndex != 5 && cursorIndex != 8)
        {
            if (cursorIndex + 1 < inventorySlots.Length)
            {
                cursorIndex += 1;
            }
        }
        else if (direction.x < 0 && cursorIndex != 0 && cursorIndex != 3 && cursorIndex != 6)
        {
            if (cursorIndex - 1 > -1)
            {
                cursorIndex -= 1;
            }
        }

        if (direction.y < 0)
        {
            if (cursorIndex + 3 < inventorySlots.Length)
            {
                cursorIndex += 3;
            }
        }
        else if (direction.y > 0)
        {
            if (cursorIndex - 3 > -1)
            {
                cursorIndex -= 3;
            }
        }
        MoveCursor(cursorIndex);
    }
    public void DisplayItems()
    {
        int equippedItemIndex = m_BuildingMaterial.GetComponent<BeastStableController>().m_BeastObject.GetComponent<BeastManager>().m_GearIndex;
        string id = m_BuildingMaterial.id;
        int underscoreIndex = id.LastIndexOf('_');
        // The state data starts just after the underscore, hence +1.
        // The length of the state data is the length of the id string minus the starting index of the state data.
        string state = id.Substring(underscoreIndex + 1, id.Length - underscoreIndex - 1);
        if (equippedItemIndex != -1)
        {
            equippedItemSlot.currentItemStack = new ItemStack(ItemManager.Instance.beastGearList[equippedItemIndex].GetComponent<Item>(), 1, 9, false);
            equippedItemSlot.spriteRenderer.sprite = equippedItemSlot.currentItemStack.item.icon;
        }
        else
        {
            equippedItemSlot.currentItemStack = new ItemStack();
            equippedItemSlot.spriteRenderer.sprite = null;
        }
        // Assuming that state data is a JSON array of arrays (2D array)
        int[][] itemsArray;
        try
        {
            itemsArray = JsonConvert.DeserializeObject<int[][]>(state);
        }
        catch (JsonException ex)
        {
            Debug.LogError("JSON deserialization error: " + ex.Message);
            return; // Exit the method if deserialization fails
        }
        // Rest of your method to populate UI elements...

        if (itemsArray == null) return;
        for (int i = 0; i < itemsArray.Length; i++)
        {
            SpriteRenderer sr = inventorySlots[i].spriteRenderer;
            TextMeshPro tm = inventorySlots[i].quantText;
            ItemStack stack = inventorySlots[i].currentItemStack;

            stack.item = ItemManager.Instance.GetBeastGearByIndex(itemsArray[i][0]).GetComponent<Item>();
            sr.sprite = stack.item.icon;
            stack.count = itemsArray[i][1];
            stack.isEmpty = false;
            inventorySlots[i].isOccupied = true;
            if (stack.count > 1)
            {
                if (tm != null)
                {
                    tm.text = stack.count.ToString();
                }
            }
            else
            {
                if (tm != null)
                {
                    tm.text = "";
                }
            }
        }
        UpdateButtonPrompts();
    }
    public string AddItem(Item itemToAdd)
    {
        string id = m_BuildingMaterial.id;
        int underscoreIndex = id.LastIndexOf('_');
        // The state data starts just after the underscore, hence +1.
        // The length of the state data is the length of the id string minus the starting index of the state data.
        string state = id.Substring(underscoreIndex + 1, id.Length - underscoreIndex - 1);
        //"15_-1_0_44_0_[[7,90],[21,12],[4,1],[3,8],[8,75],[1,14],[2,2],[17,13],[28,1],]"
        int[][] itemsArray = JsonConvert.DeserializeObject<int[][]>(state);
        List<int[]> itemsList = new List<int[]>();
        if (itemsArray != null && itemsArray.Length >= 9) return "Stable storage is full. No more items can be added.";

        if (itemsArray != null && itemsArray.Length > 0)
        {
            itemsList = new List<int[]>(itemsArray);
        }

        foreach (int[] item in itemsList)
        {
            if (item[0] == itemToAdd.itemIndex)
            {
                return "This item has already been crafted";
            }
        }
        // Convert array to List for easy manipulation

        // Assuming new item is an int array (e.g., new int[] { 5, 10 })
        // You can modify this part to get the actual item data you need to add
        int[] newItem = new int[] { itemToAdd.itemIndex, 1 };

        // Add the new item
        itemsList.Add(newItem);

        // Convert back to array if necessary
        itemsArray = itemsList.ToArray();

        // Serialize back to string
        string newState = JsonConvert.SerializeObject(itemsArray);
        SaveChestState(newState);

        return $"New item has been added to the Stable Storage!";
    }
    void EquippedBeastItem()
    {
        if (inventorySlots[cursorIndex].isOccupied)
            GetComponentInParent<BeastStableController>().m_BeastObject.GetComponent<BeastManager>().EquipGear(inventorySlots[cursorIndex].currentItemStack.item.itemIndex);
        else GetComponentInParent<BeastStableController>().m_BeastObject.GetComponent<BeastManager>().EquipGear(-1);
        DisplayItems();
    }
    public void SaveChestState(string state)
    {
        LevelManager.Instance.CallSaveObjectsPRC(m_BuildingMaterial.id, false, state);
    }
    public void PlayerOpenUI(GameObject actor)
    {
        if (isOpen)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            ActorEquipment ac = actor.GetComponent<ActorEquipment>();
            isOpen = false;
            ac.GetComponent<ThirdPersonUserControl>().craftingBenchUI = false;
            playerCurrentlyUsing = null;
            playerPrefix = null;
            Initialize();
        }
        else
        {
            Debug.Log("Testing");
            playerCurrentlyUsing = actor;
            ActorEquipment ac = actor.GetComponent<ActorEquipment>();
            items = ac.inventoryManager.items;
            DisplayItems();
            ac.GetComponent<ThirdPersonUserControl>().craftingBenchUI = true;
            playerPrefix = playerCurrentlyUsing.GetComponent<ThirdPersonUserControl>().playerPrefix;
            transform.GetChild(0).gameObject.SetActive(true);
            isOpen = true;
            UpdateButtonPrompts();
        }
    }

    public void UpdateInfoPanel(string name, string description, int value, int damage = 0)
    {
        infoPanel.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = name;
        infoPanel.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = description;
        infoPanel.transform.GetChild(1).GetChild(2).GetComponent<TextMeshProUGUI>().text = damage != 0 ? $"Damage: {damage}" : "";
        infoPanel.transform.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text = value != 0 ? $"{value}Gp" : "";
    }
    public class ArrayComparer : IEqualityComparer<int[]>
    {
        public bool Equals(int[] x, int[] y)
        {
            return x.SequenceEqual(y);
        }

        public int GetHashCode(int[] obj)
        {
            if (obj == null)
                return 0;

            int hash = 17;
            foreach (var item in obj)
            {
                hash = hash * 23 + item.GetHashCode();
            }
            return hash;
        }
    }
}
