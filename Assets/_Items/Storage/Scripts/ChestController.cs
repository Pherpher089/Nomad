using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class ChestController : MonoBehaviour
{
    public Sprite m_InventorySlotSprite;
    //The UI GameObject
    public bool m_IsOpen = false;
    public GameObject m_PlayerCurrentlyUsing = null;
    public CraftingSlot[] m_StorageSlots;
    public CraftingSlot[] m_InventorySlots;
    public CraftingSlot[] m_Slots;
    GameObject m_CursorObject;
    CraftingSlot m_CursorSlot;
    string m_CurrentPlayerPrefix;
    bool M_UiReturn = false;                         //Tracks the return of the input axis because they are not boolean input
    int m_CursorIndex = 0;
    public ItemStack[] m_Items;
    GameObject m_InfoPanel;
    BuildingObject m_BuildingObject;
    [HideInInspector]
    public BuildingMaterial m_BuildingMaterial;
    public bool inUse = false;
    GameObject[] buttonPrompts;

    void Start()
    {
        m_BuildingMaterial = GetComponent<BuildingMaterial>();
        m_BuildingObject = GetComponent<BuildingObject>();
        Initialize();
    }
    public void UpdateButtonPrompts()
    {
        if (!GameStateManager.Instance.showOnScreenControls)
        {

            int buttonPromptChildCount = transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 2).GetChild(0).childCount;
            for (int i = 0; i < buttonPromptChildCount; i++)
            {
                transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 2).GetChild(1).GetChild(i).gameObject.SetActive(false);
                transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 2).GetChild(0).GetChild(i).gameObject.SetActive(false);

            }
            return;

        }
        if (!LevelPrep.Instance.firstPlayerGamePad)
        {
            int buttonPromptChildCount = transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 2).GetChild(0).childCount;
            buttonPrompts = new GameObject[buttonPromptChildCount];
            for (int i = 0; i < buttonPromptChildCount; i++)
            {
                transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 2).GetChild(1).GetChild(i).gameObject.SetActive(true);
                buttonPrompts[i] = transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 2).GetChild(1).GetChild(i).gameObject;

            }
            for (int i = 0; i < buttonPromptChildCount; i++)
            {
                transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 2).GetChild(0).GetChild(i).gameObject.SetActive(false);
            }
        }
        else
        {
            int buttonPromptChildCount = transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 2).GetChild(0).childCount;
            buttonPrompts = new GameObject[buttonPromptChildCount];
            for (int i = 0; i < buttonPromptChildCount; i++)
            {
                transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 2).GetChild(0).GetChild(i).gameObject.SetActive(true);
                buttonPrompts[i] = transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 2).GetChild(0).GetChild(i).gameObject;
            }
            for (int i = 0; i < buttonPromptChildCount; i++)
            {
                transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 2).GetChild(1).GetChild(i).gameObject.SetActive(false);
            }
        }
        AdjustButtonPrompts();
    }
    void AdjustButtonPrompts()
    {
        if (!LevelPrep.Instance.settingsConfig.showOnScreenControls) return;
        if (m_CursorSlot.isOccupied)
        {
            buttonPrompts[1].SetActive(false);
            buttonPrompts[2].SetActive(false);
            buttonPrompts[3].SetActive(true);
            buttonPrompts[4].SetActive(true);
        }
        else
        {
            buttonPrompts[1].SetActive(true);
            buttonPrompts[2].SetActive(true);
            buttonPrompts[3].SetActive(false);
            buttonPrompts[4].SetActive(false);
        }
    }
    //for creating crafting recipes in the editor
    public string ArrayToString(int[] array)
    {
        return string.Join(",", array.Select(i => i.ToString()).ToArray());
    }
    void Initialize()
    {
        m_StorageSlots = new CraftingSlot[9];
        m_InventorySlots = new CraftingSlot[9];
        m_Slots = new CraftingSlot[18];
        int counter = 0;
        for (int i = 3; i < 18; i += 6)
        {
            m_StorageSlots[counter] = transform.GetChild(0).GetChild(i).GetComponent<CraftingSlot>();
            m_StorageSlots[counter].currentItemStack = new ItemStack(null, 0, -1, true);
            m_StorageSlots[counter].isOccupied = false;
            m_StorageSlots[counter].quantText.text = "";
            m_StorageSlots[counter].spriteRenderer.sprite = null;
            m_StorageSlots[counter + 1] = transform.GetChild(0).GetChild(i + 1).GetComponent<CraftingSlot>();
            m_StorageSlots[counter + 1].currentItemStack = new ItemStack(null, 0, -1, true);
            m_StorageSlots[counter + 1].isOccupied = false;
            m_StorageSlots[counter + 1].isOccupied = false;
            m_StorageSlots[counter + 1].quantText.text = "";
            m_StorageSlots[counter + 1].spriteRenderer.sprite = null;
            m_StorageSlots[counter + 2] = transform.GetChild(0).GetChild(i + 2).GetComponent<CraftingSlot>();
            m_StorageSlots[counter + 2].currentItemStack = new ItemStack(null, 0, -1, true);
            m_StorageSlots[counter + 2].isOccupied = false;
            m_StorageSlots[counter + 2].isOccupied = false;
            m_StorageSlots[counter + 2].isOccupied = false;
            m_StorageSlots[counter + 2].quantText.text = "";
            m_StorageSlots[counter + 2].spriteRenderer.sprite = null;
            counter += 3;
        }
        counter = 0;
        for (int i = 0; i < 18; i += 6)
        {
            m_InventorySlots[counter] = transform.GetChild(0).GetChild(i).GetComponent<CraftingSlot>();
            m_InventorySlots[counter + 1] = transform.GetChild(0).GetChild(i + 1).GetComponent<CraftingSlot>();
            m_InventorySlots[counter + 2] = transform.GetChild(0).GetChild(i + 2).GetComponent<CraftingSlot>();
            counter += 3;

        }
        int inventoryCounter = 0;
        int craftingCounter = 0;
        for (int i = 0; i < 18; i++)
        {
            if (i < 3 || i > 5 && i < 9 || i > 11 && i < 15)
            {
                m_Slots[i] = m_InventorySlots[inventoryCounter];
                inventoryCounter++;
            }
            else
            {

                m_Slots[i] = m_StorageSlots[craftingCounter];
                m_Slots[i].currentItemStack.item = null;
                m_Slots[i].currentItemStack.count = 0;
                m_Slots[i].isOccupied = false;
                m_Slots[i].spriteRenderer.sprite = null;
                m_Slots[i].quantText.text = "";
                craftingCounter++;
            }
        }

        m_InventorySlotSprite = m_StorageSlots[0].spriteRenderer.sprite;
        //The cursor is the 10th child
        m_CursorObject = transform.GetChild(0).GetChild(18).gameObject;
        m_CursorSlot = m_CursorObject.GetComponent<CraftingSlot>();
        m_InfoPanel = transform.GetChild(0).GetChild(19).gameObject;
        transform.GetChild(0).gameObject.SetActive(false);
        m_IsOpen = false;
        UpdateButtonPrompts();
    }

    void MoveCursor(int index)
    {
        m_CursorObject.transform.position = m_Slots[index].transform.position;
        if (m_Slots[index].currentItemStack.item != null)
        {
            UpdateInfoPanel(m_Slots[index].currentItemStack.item.itemName, m_Slots[index].currentItemStack.item.itemDescription, m_Slots[index].currentItemStack.item.value, 0);
        }
        else
        {
            UpdateInfoPanel("", "", 0, 0);
        }
    }

    void Update()
    {
        if (m_PlayerCurrentlyUsing != null)
        {
            ListenToDirectionalInput();
            ListenToActionInput();
        }
    }

    void ListenToActionInput()
    {
        if (Input.GetButtonDown(m_CurrentPlayerPrefix + "Grab") && m_BuildingObject.isPlaced)
        {
            if (!m_CursorSlot.isOccupied)
            {
                SelectItem(true);
            }
            else
            {
                PlaceSelectedItem(true);
            }
        }
        if (Input.GetButtonDown(m_CurrentPlayerPrefix + "Block"))
        {
            if (!m_CursorSlot.isOccupied)
            {
                SelectItem(false);
            }
            else
            {
                PlaceSelectedItem(false);
            }
        }
    }



    // listen for input associated to the player prefix;
    void ListenToDirectionalInput()
    {
        float v = Input.GetAxisRaw(m_CurrentPlayerPrefix + "Vertical");
        float h = Input.GetAxisRaw(m_CurrentPlayerPrefix + "Horizontal");

        if (M_UiReturn && v < GameStateManager.Instance.inventoryControlDeadZone && h < GameStateManager.Instance.inventoryControlDeadZone && v > -GameStateManager.Instance.inventoryControlDeadZone && h > -GameStateManager.Instance.inventoryControlDeadZone)
        {
            M_UiReturn = false;
        }

        if (m_CurrentPlayerPrefix == "sp")
        {
            if (Input.GetButtonDown(m_CurrentPlayerPrefix + "Horizontal") || Input.GetButtonDown(m_CurrentPlayerPrefix + "Vertical"))
            {
                MoveCursor(new Vector2(h, v));
            }
        }
        else
        {
            if (!M_UiReturn && v + h != 0)
            {
                MoveCursor(new Vector2(h, v));
                M_UiReturn = true;
            }
        }
    }

    void MoveCursor(Vector2 direction)
    {
        if (direction.x > 0 && m_CursorIndex != 5 && m_CursorIndex != 11 && m_CursorIndex != 17)
        {
            if (m_CursorIndex + 1 < m_Slots.Length)
            {
                m_CursorIndex += 1;
            }
        }
        else if (direction.x < 0 && m_CursorIndex != 0 && m_CursorIndex != 6 && m_CursorIndex != 12)
        {
            if (m_CursorIndex - 1 > -1)
            {
                m_CursorIndex -= 1;
            }
        }

        if (direction.y < 0)
        {
            if (m_CursorIndex + 6 < m_Slots.Length)
            {
                m_CursorIndex += 6;
            }
        }
        else if (direction.y > 0)
        {
            if (m_CursorIndex - 6 > -1)
            {
                m_CursorIndex -= 6;
            }
        }

        MoveCursor(m_CursorIndex);
    }

    void SetSelectedItemStack(ItemStack itemStack, bool stack = true)
    {
        if (itemStack.count == 0)
        {
            m_CursorSlot.spriteRenderer.sprite = null;
            m_CursorSlot.currentItemStack = new ItemStack(null, 0, -1, true);
            m_CursorSlot.isOccupied = false;
            return;
        }
        else if (stack)
        {
            m_CursorSlot.spriteRenderer.sprite = itemStack.item.icon;
            m_CursorSlot.currentItemStack = new ItemStack(itemStack);
            m_CursorSlot.quantText.text = itemStack.count.ToString();
            m_CursorSlot.isOccupied = true;
        }
        else
        {
            m_CursorSlot.spriteRenderer.sprite = itemStack.item.icon;
            m_CursorSlot.currentItemStack = new ItemStack(itemStack);
            m_CursorSlot.currentItemStack.count = 1;
            m_CursorSlot.quantText.text = "1";
            m_CursorSlot.isOccupied = true;
        }
        AdjustButtonPrompts();
    }

    int ConvertToInventoryIndex(int index)
    {
        if (index > 2 && index < 6 || index > 8 && index < 12 || index > 14)
        {
            return index;
        }
        else if (index > 5 && index < 9)
        {
            return index + 3;
        }
        else if (index > 11 && index < 15)
        {
            return index + 6;
        }
        else
        {
            return index;
        }
    }

    bool SelectItem(bool stack)
    {
        //If cursor is on crafting side
        if (m_CursorIndex < m_Slots.Length && m_Slots[m_CursorIndex].isOccupied)
        {
            if (stack)
            {
                ItemStack oldStack;
                if (m_CursorSlot.currentItemStack)
                {
                    oldStack = new ItemStack(m_CursorSlot.currentItemStack);
                }
                else
                {
                    oldStack = new ItemStack(null, 0, ConvertToInventoryIndex(m_CursorIndex), true);
                }
                SetSelectedItemStack(m_Slots[m_CursorIndex].currentItemStack);
                m_Slots[m_CursorIndex].isOccupied = false;
                m_Slots[m_CursorIndex].spriteRenderer.sprite = m_InventorySlotSprite;
                m_Slots[m_CursorIndex].currentItemStack = new ItemStack(oldStack);
                m_Slots[m_CursorIndex].quantText.text = "";
            }
            else
            {
                SetSelectedItemStack(m_Slots[m_CursorIndex].currentItemStack, false);
                m_Slots[m_CursorIndex].currentItemStack.count--;
                m_Slots[m_CursorIndex].quantText.text = m_Slots[m_CursorIndex].currentItemStack.count.ToString();
                if (m_Slots[m_CursorIndex].currentItemStack.count <= 0)
                {
                    m_Slots[m_CursorIndex].isOccupied = false;
                    m_Slots[m_CursorIndex].spriteRenderer.sprite = m_InventorySlotSprite;
                    m_Slots[m_CursorIndex].quantText.text = "";
                }
            }
        }
        return false;
    }
    void PlaceSelectedItem(bool stack)
    {
        if (stack)
        {
            if (m_Slots[m_CursorIndex].isOccupied)
            {
                if (m_Slots[m_CursorIndex].currentItemStack.item.itemName == m_CursorSlot.currentItemStack.item.itemName)
                {
                    m_Slots[m_CursorIndex].currentItemStack.count += m_CursorSlot.currentItemStack.count;
                    m_Slots[m_CursorIndex].quantText.text = m_Slots[m_CursorIndex].currentItemStack.count.ToString();
                    m_Slots[m_CursorIndex].currentItemStack.index = ConvertToInventoryIndex(m_CursorIndex);
                    m_CursorSlot.currentItemStack = new ItemStack(null, 0, -1, true);
                    m_CursorSlot.spriteRenderer.sprite = null;
                    m_CursorSlot.quantText.text = "";
                    m_CursorSlot.isOccupied = false;
                }
                else
                {
                    ItemStack oldStack = new ItemStack(m_Slots[m_CursorIndex].currentItemStack);
                    m_Slots[m_CursorIndex].currentItemStack = new ItemStack(m_CursorSlot.currentItemStack);
                    m_Slots[m_CursorIndex].spriteRenderer.sprite = m_CursorSlot.currentItemStack.item.icon;
                    m_Slots[m_CursorIndex].currentItemStack.index = ConvertToInventoryIndex(m_CursorIndex);
                    m_Slots[m_CursorIndex].quantText.text = m_CursorSlot.currentItemStack.count.ToString();
                    SetSelectedItemStack(oldStack);
                }
            }
            else
            {
                m_Slots[m_CursorIndex].currentItemStack = new ItemStack(m_CursorSlot.currentItemStack);
                m_Slots[m_CursorIndex].spriteRenderer.sprite = m_CursorSlot.currentItemStack.item.icon;
                m_Slots[m_CursorIndex].quantText.text = m_CursorSlot.currentItemStack.count.ToString();
                m_Slots[m_CursorIndex].currentItemStack.index = ConvertToInventoryIndex(m_CursorIndex);
                m_Slots[m_CursorIndex].isOccupied = true;
                m_CursorSlot.isOccupied = false;
                m_CursorSlot.currentItemStack = new ItemStack(null, 0, -1, true);
                m_CursorSlot.quantText.text = "";
                m_CursorSlot.spriteRenderer.sprite = null;
            }
        }
        else
        {
            if (m_Slots[m_CursorIndex].isOccupied)
            {
                if (m_Slots[m_CursorIndex].currentItemStack.item.itemName == m_CursorSlot.currentItemStack.item.itemName)
                {
                    m_Slots[m_CursorIndex].currentItemStack.count += 1;
                    m_Slots[m_CursorIndex].quantText.text = m_Slots[m_CursorIndex].currentItemStack.count.ToString();
                    if (m_CursorSlot.currentItemStack.count - 1 <= 0)
                    {
                        m_CursorSlot.isOccupied = false;
                        m_CursorSlot.currentItemStack = new ItemStack(null, 0, -1, true);
                        m_CursorSlot.quantText.text = "";
                        m_CursorSlot.spriteRenderer.sprite = null;
                    }
                    else
                    {
                        m_CursorSlot.currentItemStack.count--;
                        m_CursorSlot.quantText.text = m_CursorSlot.currentItemStack.count.ToString();
                    }
                }
                else
                {
                    ItemStack oldStack = new ItemStack(m_Slots[m_CursorIndex].currentItemStack);
                    m_Slots[m_CursorIndex].currentItemStack = new ItemStack(m_CursorSlot.currentItemStack);
                    m_Slots[m_CursorIndex].currentItemStack.count = 1;
                    m_Slots[m_CursorIndex].spriteRenderer.sprite = m_CursorSlot.currentItemStack.item.icon;
                    m_Slots[m_CursorIndex].quantText.text = m_CursorSlot.currentItemStack.count.ToString();
                    SetSelectedItemStack(oldStack);
                }

            }
            else
            {
                m_Slots[m_CursorIndex].currentItemStack = new ItemStack(m_CursorSlot.currentItemStack);
                m_Slots[m_CursorIndex].currentItemStack.count = 1;
                m_Slots[m_CursorIndex].spriteRenderer.sprite = m_CursorSlot.currentItemStack.item.icon;
                m_Slots[m_CursorIndex].quantText.text = m_Slots[m_CursorIndex].currentItemStack.count.ToString();
                m_Slots[m_CursorIndex].isOccupied = true;
                if (m_CursorSlot.currentItemStack.count - 1 <= 0)
                {
                    m_CursorSlot.isOccupied = false;
                    m_CursorSlot.currentItemStack = new ItemStack(null, 0, -1, true);
                    m_CursorSlot.quantText.text = "";
                    m_CursorSlot.spriteRenderer.sprite = null;
                }
                else
                {
                    m_CursorSlot.currentItemStack.count--;
                    m_CursorSlot.quantText.text = m_CursorSlot.currentItemStack.count.ToString();
                }

            }
        }
        AdjustButtonPrompts();
    }

    //This saves the chest with the items it has
    // I think this needs to be a PRC
    public void SaveChestState()
    {
        string newState = "[";
        for (int i = 3; i < m_Slots.Length; i++)
        {
            if ((i > 5 && i < 9) || (i > 11 && i < 15))
            {
                continue;
            }
            if (m_Slots[i].isOccupied)
            {
                newState += $"[{m_Slots[i].currentItemStack.item.itemIndex},{m_Slots[i].currentItemStack.count}],";
            }
        }
        newState += "]";
        LevelManager.Instance.CallSaveObjectsPRC(m_BuildingMaterial.id, false, newState);
    }

    //Updates inventory with changes made in the chest UI
    public void ReconcileItems(PlayerInventoryManager actor)
    {
        ItemStack[] _items = new ItemStack[9];
        int c = 0;

        //Gather all items in inventory portion of ui into an array
        for (int i = 0; i < 15; i++)
        {
            if ((i > 2 && i < 6) || (i > 8 && i < 12))
            {
                continue;
            }
            _items[c] = m_Slots[i].currentItemStack;
            m_Slots[i].currentItemStack = new ItemStack(null, 0, -1, true);
            m_Slots[i].isOccupied = false;
            m_Slots[i].quantText.text = "";
            m_Slots[i].spriteRenderer.sprite = null;
            c++;

        }
        actor.items = _items;
        actor.GetComponent<CharacterManager>().SaveCharacter();
    }
    public void PlayerOpenUI(GameObject actor)
    {
        //If the player is holding this, do not open
        if (!m_BuildingObject.isPlaced) return;
        // open the cargo inventory with an item in the closest avaliable slot
        if (m_IsOpen)
        {
            LevelManager.Instance.CallChestInUsePRC(m_BuildingMaterial.id, false);
            transform.GetChild(0).gameObject.SetActive(false);
            ActorEquipment ac = actor.GetComponent<ActorEquipment>();
            m_IsOpen = false;
            ac.GetComponent<ThirdPersonUserControl>().chestUI = false;
            m_PlayerCurrentlyUsing = null;
            m_CurrentPlayerPrefix = null;
            m_CursorSlot.isOccupied = false;
            m_CursorSlot.spriteRenderer.sprite = null;
            m_CursorSlot.quantText.text = "";
            SaveChestState();
            ReconcileItems(ac.inventoryManager);
            Initialize();
        }
        else
        {
            if (inUse) return;
            LevelManager.Instance.CallChestInUsePRC(m_BuildingMaterial.id, true);
            m_PlayerCurrentlyUsing = actor;
            ActorEquipment ac = actor.GetComponent<ActorEquipment>();
            m_Items = ac.inventoryManager.items;
            DisplayItems();
            ac.GetComponent<ThirdPersonUserControl>().chestUI = true;
            m_CurrentPlayerPrefix = m_PlayerCurrentlyUsing.GetComponent<ThirdPersonUserControl>().playerPrefix;
            transform.GetChild(0).gameObject.SetActive(true);
            m_IsOpen = true;
        }

    }

    public void UpdateInfoPanel(string name, string description, int value, int damage = 0)
    {
        m_InfoPanel.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = name;
        m_InfoPanel.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = description;
        m_InfoPanel.transform.GetChild(1).GetChild(2).GetComponent<TextMeshProUGUI>().text = damage != 0 ? $"Damage: {damage}" : "";
        m_InfoPanel.transform.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text = value != 0 ? $"{value}Gp" : "";

    }
    public void DisplayItems()
    {
        string id = m_BuildingMaterial.id;
        int underscoreIndex = id.LastIndexOf('_');
        // The state data starts just after the underscore, hence +1.
        // The length of the state data is the length of the id string minus the starting index of the state data.
        string state = id.Substring(underscoreIndex + 1, id.Length - underscoreIndex - 1);

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
        for (int i = 0; i < m_Items.Length; i++)
        {
            SpriteRenderer sr = m_InventorySlots[i].spriteRenderer;
            TextMeshPro tm = m_InventorySlots[i].quantText;
            ItemStack stack = m_InventorySlots[i].currentItemStack;
            if (!m_Items[i].isEmpty)
            {
                sr.sprite = m_Items[i].item.icon;
                stack.item = m_Items[i].item;
                stack.count = m_Items[i].count;
                int slotCount = 0;
                if (i > 2 && i < 6)
                {
                    slotCount = i + 3;
                }
                else if (i > 5)
                {
                    slotCount = i + 6;
                }

                stack.isEmpty = false;
                m_InventorySlots[i].isOccupied = true;
                if (m_Items[i].count > 1)
                {
                    if (tm != null)
                    {
                        tm.text = m_Items[i].count.ToString();
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
            else
            {
                sr.sprite = null;
            }
        }
        if (itemsArray == null) return;
        for (int i = 0; i < itemsArray.Length; i++)
        {
            SpriteRenderer sr = m_StorageSlots[i].spriteRenderer;
            TextMeshPro tm = m_StorageSlots[i].quantText;
            ItemStack stack = m_StorageSlots[i].currentItemStack;

            stack.item = ItemManager.Instance.GetItemGameObjectByItemIndex(itemsArray[i][0]).GetComponent<Item>();
            sr.sprite = stack.item.icon;
            stack.count = itemsArray[i][1];
            stack.isEmpty = false;
            m_StorageSlots[i].isOccupied = true;
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
