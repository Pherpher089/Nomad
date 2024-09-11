using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
public class CraftingBenchUIController : MonoBehaviour
{
    public Sprite inventorySlotSprite;
    //The UI GameObject
    public bool isOpen = false;
    public GameObject playerCurrentlyUsing = null;
    CraftingSlot[] craftingSlots;
    CraftingSlot[] inventorySlots;
    CraftingSlot[] slots;
    CraftingSlot productSlot;
    GameObject cursor;
    CraftingSlot cursorSlot;
    GameObject m_MouseCursorObject;
    CraftingSlot m_MouseCursorSlot;
    string playerPrefix;
    public CraftingBenchRecipe[] _craftingRecipes;
    bool uiReturn = false;//Tracks the return of the input axis because they are not boolean input
    int cursorIndex = 0;
    //public Dictionary<int[], int> craftingRecipes;
    public ItemStack[] items;
    public ItemStack[] m_BeltItems;
    GameObject infoPanel;
    GameObject[] buttonPrompts;
    [HideInInspector] public GameObject damagePopup;
    bool canCraft = false;
    CraftingBenchRecipe currentProductRecipe;
    void Start()
    {
        Initialize();
    }
    //for creating crafting recipes in the editor
    public string ArrayToString(int[] array)
    {
        return string.Join(",", array.Select(i => i.ToString()).ToArray());
    }
    public void Initialize()
    {
        damagePopup = Resources.Load("Prefabs/DamagePopup") as GameObject;
        craftingSlots = new CraftingSlot[9];
        inventorySlots = new CraftingSlot[13];
        slots = new CraftingSlot[23];
        int counter = 0;
        for (int i = 3; i < 16; i += 6)
        {
            craftingSlots[counter] = transform.GetChild(0).GetChild(i).GetComponent<CraftingSlot>();
            craftingSlots[counter].currentItemStack = new ItemStack(null, 0, -1, true);
            craftingSlots[counter].isOccupied = false;
            craftingSlots[counter].quantText.text = "";
            craftingSlots[counter].spriteRenderer.sprite = null;
            craftingSlots[counter + 1] = transform.GetChild(0).GetChild(i + 1).GetComponent<CraftingSlot>();
            craftingSlots[counter + 1].currentItemStack = new ItemStack(null, 0, -1, true);
            craftingSlots[counter + 1].isOccupied = false;
            craftingSlots[counter + 1].quantText.text = "";
            craftingSlots[counter + 1].spriteRenderer.sprite = null;
            craftingSlots[counter + 2] = transform.GetChild(0).GetChild(i + 2).GetComponent<CraftingSlot>();
            craftingSlots[counter + 2].currentItemStack = new ItemStack(null, 0, -1, true);
            craftingSlots[counter + 2].isOccupied = false;
            craftingSlots[counter + 2].quantText.text = "";
            craftingSlots[counter + 2].spriteRenderer.sprite = null;
            counter += 3;
        }

        counter = 0;
        for (int i = 0; i < 18; i += 6)
        {
            inventorySlots[counter] = transform.GetChild(0).GetChild(i).GetComponent<CraftingSlot>();
            inventorySlots[counter + 1] = transform.GetChild(0).GetChild(i + 1).GetComponent<CraftingSlot>();
            inventorySlots[counter + 2] = transform.GetChild(0).GetChild(i + 2).GetComponent<CraftingSlot>();
            counter += 3;

        }
        for (int i = 18; i < 22; i++)
        {
            inventorySlots[counter] = transform.GetChild(0).GetChild(i).GetComponent<CraftingSlot>();
            counter++;
        }
        productSlot = transform.GetChild(0).GetChild(22).GetComponent<CraftingSlot>();
        productSlot.currentItemStack = new ItemStack(null, 0, -1, true);
        productSlot.isOccupied = false;
        productSlot.quantText.text = "";
        productSlot.spriteRenderer.sprite = null;
        currentProductRecipe = null;
        int inventoryCounter = 0;
        int craftingCounter = 0;
        for (int i = 0; i < 23; i++)
        {
            if (i < 3 || i > 5 && i < 9 || i > 11 && i < 15 || i > 17)
            {
                if (i == 22)
                {
                    slots[i] = productSlot;
                }
                else
                {
                    slots[i] = inventorySlots[inventoryCounter];
                    inventoryCounter++;
                }
            }
            else
            {

                slots[i] = craftingSlots[craftingCounter];
                slots[i].currentItemStack.item = null;
                slots[i].currentItemStack.count = 0;
                slots[i].isOccupied = false;
                slots[i].spriteRenderer.sprite = null;
                slots[i].quantText.text = "";
                craftingCounter++;
            }
        }

        inventorySlotSprite = craftingSlots[0].spriteRenderer.sprite;
        //The cursor is the 10th child
        cursor = transform.GetChild(0).GetChild(23).gameObject;
        cursorSlot = cursor.GetComponent<CraftingSlot>();
        m_MouseCursorObject = transform.GetChild(0).GetChild(24).gameObject;
        m_MouseCursorSlot = m_MouseCursorObject.GetComponent<CraftingSlot>();
        infoPanel = transform.GetChild(0).GetChild(25).gameObject;
        transform.GetChild(0).gameObject.SetActive(false);
        isOpen = false;
        UpdateButtonPrompts();
    }
    void HandleMouseInput()
    {

        // Raycast to detect UI element under the mouse in world space
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        int layerMask = ~LayerMask.GetMask("MousePlane", "Default", "TransparentFX", "Ignore Raycast", "Water", "Terrain", "NewMousePLane", "Item", "Bullet", "OutLine", "Door", "Player", "Interact", "Build", "Floor", "Enemy", "Structure", "Terrain", "PostProcessing", "EnemyPlayerCollision", "Arrow", "Wall");

        if (Physics.Raycast(ray, out hit, 1000f, layerMask, QueryTriggerInteraction.Collide)) // Use 1000f or any max distance that suits your setup
        {

            GameObject clickedSlot = hit.collider.gameObject;

            // Check if the clicked object is an InventorySlot
            if (clickedSlot.CompareTag("InventorySlot"))
            {
                if (!cursorSlot.currentItemStack.isEmpty)
                {
                    m_MouseCursorSlot.currentItemStack = new(cursorSlot.currentItemStack);
                    cursorSlot.currentItemStack = new();
                }
                InventoryActionMouse(clickedSlot);
            }
        }
        else
        {
            InventoryActionMouse(null);
        }

        Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        m_MouseCursorObject.transform.position = _ray.GetPoint(Vector3.Distance(Camera.main.transform.position, transform.GetChild(0).position)); // Move cursor to the point where the ray hits the plane
    }
    void InventoryActionMouse(GameObject clickedSlot)
    {
        if (clickedSlot == null)
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (!m_MouseCursorSlot.currentItemStack.isEmpty)
                {

                    PlayerInventoryManager.Instance.DropItem(m_MouseCursorSlot.currentItemStack.item.itemListIndex, transform.position);
                    m_MouseCursorSlot.currentItemStack.count--;
                    if (m_MouseCursorSlot.currentItemStack.count <= 0)
                    {
                        m_MouseCursorSlot.currentItemStack = new ItemStack();
                    }
                    DisplayItems();
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (!m_MouseCursorSlot.currentItemStack.isEmpty)
                {
                    for (int i = 0; i < m_MouseCursorSlot.currentItemStack.count; i++)
                    {
                        PlayerInventoryManager.Instance.DropItem(m_MouseCursorSlot.currentItemStack.item.itemListIndex, transform.position);
                    }
                    m_MouseCursorSlot.currentItemStack = new ItemStack();
                    DisplayItems();
                }
            }
            return;
        }
        else
        {
            int slotIndex = clickedSlot.transform.GetSiblingIndex();
            Debug.Log("Slot index " + slotIndex);
            cursorIndex = slotIndex;
            MoveCursor(slotIndex);

            if (Input.GetMouseButtonDown(0))
            {
                if (!m_MouseCursorSlot.isOccupied)
                {
                    SelectItem(true, true);
                }
                else
                {
                    PlaceSelectedItem(true, true);
                }
            }
            if (Input.GetMouseButtonDown(1))
            {
                if (!m_MouseCursorSlot.isOccupied)
                {
                    SelectItem(false, true);
                }
                else
                {
                    PlaceSelectedItem(false, true);
                }
            }
        }
    }

    void MoveCursor(int index)
    {
        cursor.transform.position = slots[index].transform.position;
        if (slots[index].currentItemStack.item != null)
        {
            UpdateInfoPanel(slots[index].currentItemStack.item.itemName, slots[index].currentItemStack.item.itemDescription, slots[index].currentItemStack.item.value, 0);
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
            if (playerPrefix == "sp")
            {
                if (m_MouseCursorObject.activeSelf == false)
                {
                    m_MouseCursorObject.SetActive(true);
                }
                HandleMouseInput();
            }
            else
            {
                m_MouseCursorObject.SetActive(false);
                ListenToDirectionalInput();
                ListenToActionInput();
            }
        }
    }

    void ListenToActionInput()
    {
        if (Input.GetButtonDown(playerPrefix + "Grab") && playerCurrentlyUsing != null)
        {
            if (!cursorSlot.isOccupied)
            {
                SelectItem(true);
            }
            else
            {
                PlaceSelectedItem(true);
            }
        }
        if (Input.GetButtonDown(playerPrefix + "Block"))
        {
            if (!cursorSlot.isOccupied)
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
        float v = Input.GetAxisRaw(playerPrefix + "Vertical");
        float h = Input.GetAxisRaw(playerPrefix + "Horizontal");

        if (uiReturn && v < GameStateManager.Instance.inventoryControlDeadZone && h < GameStateManager.Instance.inventoryControlDeadZone && v > -GameStateManager.Instance.inventoryControlDeadZone && h > -GameStateManager.Instance.inventoryControlDeadZone)
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

    void MoveCursor(Vector2 direction)
    {
        if (direction.x > 0 && cursorIndex != 5 && cursorIndex != 11 && cursorIndex != 17)
        {
            if (cursorIndex + 1 < slots.Length)
            {
                cursorIndex += 1;
            }
        }
        else if (direction.x > 0 && (cursorIndex == 5 || cursorIndex == 11 || cursorIndex == 17))
        {
            switch (cursorIndex)
            {
                case 5:
                    cursorIndex = 0;
                    break;
                case 11:
                    cursorIndex = 6;
                    break;
                case 17:
                    cursorIndex = 12;
                    break;
            }
        }
        else if (direction.x > 0 && cursorIndex > 17)
        {
            switch (cursorIndex)
            {
                case 18:
                    cursorIndex = 19;
                    break;
                case 19:
                    cursorIndex = 18;
                    break;
                case 20:
                    cursorIndex = 21;
                    break;
                case 21:
                    cursorIndex = 20;
                    break;
            }
        }
        else if (direction.x < 0 && cursorIndex != 0 && cursorIndex != 6 && cursorIndex != 12)
        {
            if (cursorIndex - 1 > -1)
            {
                cursorIndex -= 1;
            }
        }
        else if (direction.x < 0 && (cursorIndex == 0 || cursorIndex == 6 || cursorIndex == 12))
        {
            switch (cursorIndex)
            {
                case 0:
                    cursorIndex = 5;
                    break;
                case 6:
                    cursorIndex = 11;
                    break;
                case 12:
                    cursorIndex = 17;
                    break;
            }
        }
        else if (direction.x < 0 && cursorIndex > 17)
        {
            switch (cursorIndex)
            {
                case 18:
                    cursorIndex = 19;
                    break;
                case 19:
                    cursorIndex = 18;
                    break;
                case 20:
                    cursorIndex = 21;
                    break;
                case 21:
                    cursorIndex = 20;
                    break;
            }
        }

        if (direction.y < 0)
        {
            if (cursorIndex + 6 < slots.Length && cursorIndex < 18)
            {
                cursorIndex += 6;
            }
            else
            {
                switch (cursorIndex)
                {
                    case 12:
                        cursorIndex = 18;
                        break;
                    case 13:
                        cursorIndex = 19;
                        break;
                    case 14:
                        cursorIndex = 19;
                        break;
                    case 15:
                        cursorIndex = 3;
                        break;
                    case 16:
                        cursorIndex = 4;
                        break;
                    case 17:
                        cursorIndex = 5;
                        break;
                    case 18:
                        cursorIndex = 20;
                        break;
                    case 19:
                        cursorIndex = 21;
                        break;
                    case 20:
                        cursorIndex = 1;
                        break;
                    case 21:
                        cursorIndex = 2;
                        break;

                }
            }
        }
        else if (direction.y > 0)
        {
            if (cursorIndex - 6 > -1)
            {
                cursorIndex -= 6;
            }
            else
            {
                switch (cursorIndex)
                {
                    case 0:
                        cursorIndex = 18;
                        break;
                    case 2:
                        cursorIndex = 19;
                        break;
                    case 3:
                        cursorIndex = 15;
                        break;
                    case 4:
                        cursorIndex = 16;
                        break;
                    case 5:
                        cursorIndex = 17;
                        break;
                    case 18:
                        cursorIndex = 13;
                        break;
                    case 19:
                        cursorIndex = 14;
                        break;
                    case 20:
                        cursorIndex = 18;
                        break;
                    case 21:
                        cursorIndex = 19;
                        break;

                }
            }
        }

        MoveCursor(cursorIndex);
    }

    void SetSelectedItemStack(ItemStack itemStack, bool stack = true, bool isMouse = false)
    {
        if (isMouse)
        {
            if (itemStack.count == 0)
            {
                m_MouseCursorSlot.spriteRenderer.sprite = null;
                m_MouseCursorSlot.currentItemStack = new ItemStack(null, 0, -1, true);
                m_MouseCursorSlot.isOccupied = false;
                return;
            }
            else if (stack)
            {
                m_MouseCursorSlot.spriteRenderer.sprite = itemStack.item.icon;
                m_MouseCursorSlot.currentItemStack = new ItemStack(itemStack);
                m_MouseCursorSlot.quantText.text = itemStack.count.ToString();
                m_MouseCursorSlot.isOccupied = true;
            }
            else
            {
                m_MouseCursorSlot.spriteRenderer.sprite = itemStack.item.icon;
                m_MouseCursorSlot.currentItemStack = new ItemStack(itemStack);
                m_MouseCursorSlot.currentItemStack.count = 1;
                m_MouseCursorSlot.quantText.text = "1";
                m_MouseCursorSlot.isOccupied = true;
            }
        }
        else
        {
            if (itemStack.count == 0)
            {
                cursorSlot.spriteRenderer.sprite = null;
                cursorSlot.currentItemStack = new ItemStack(null, 0, -1, true);
                cursorSlot.isOccupied = false;
                return;
            }
            else if (stack)
            {
                cursorSlot.spriteRenderer.sprite = itemStack.item.icon;
                cursorSlot.currentItemStack = new ItemStack(itemStack);
                cursorSlot.quantText.text = itemStack.count.ToString();
                cursorSlot.isOccupied = true;
            }
            else
            {
                cursorSlot.spriteRenderer.sprite = itemStack.item.icon;
                cursorSlot.currentItemStack = new ItemStack(itemStack);
                cursorSlot.currentItemStack.count = 1;
                cursorSlot.quantText.text = "1";
                cursorSlot.isOccupied = true;
            }
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

    bool SelectItem(bool stack, bool isMouse = false)
    {

        if (isMouse)
        {
            if (cursorIndex == 22)
            {
                if (canCraft)
                {
                    Craft(currentProductRecipe, true);
                    return false;
                }
            }
            if (cursorIndex < slots.Length && slots[cursorIndex].isOccupied)
            {
                if (stack)
                {

                    ItemStack oldStack;
                    if (m_MouseCursorSlot.currentItemStack)
                    {
                        oldStack = new ItemStack(m_MouseCursorSlot.currentItemStack);
                    }
                    else
                    {
                        oldStack = new ItemStack(null, 0, ConvertToInventoryIndex(cursorIndex), true);
                    }
                    SetSelectedItemStack(slots[cursorIndex].currentItemStack, true, true);
                    slots[cursorIndex].currentItemStack = new ItemStack(oldStack);
                    slots[cursorIndex].isOccupied = !oldStack.isEmpty;
                    slots[cursorIndex].spriteRenderer.sprite = !oldStack.isEmpty ? oldStack.item.icon : inventorySlotSprite;
                    slots[cursorIndex].quantText.text = !oldStack.isEmpty ? oldStack.count.ToString() : "";
                }
                else
                {
                    SetSelectedItemStack(slots[cursorIndex].currentItemStack, false, true);
                    slots[cursorIndex].currentItemStack.count--;
                    slots[cursorIndex].quantText.text = slots[cursorIndex].currentItemStack.count.ToString();
                    if (slots[cursorIndex].currentItemStack.count <= 0)
                    {
                        slots[cursorIndex].isOccupied = false;
                        slots[cursorIndex].spriteRenderer.sprite = inventorySlotSprite;
                        slots[cursorIndex].quantText.text = "";
                    }
                }
            }
            return false;
        }

        //If cursor is on crafting side
        if (cursorIndex < slots.Length && slots[cursorIndex].isOccupied)
        {
            if (cursorIndex == 22)
            {
                if (canCraft)
                {
                    Craft(currentProductRecipe, false);
                    return false;
                }
            }
            if (stack)
            {
                ItemStack oldStack;
                if (cursorSlot.currentItemStack)
                {
                    oldStack = new ItemStack(cursorSlot.currentItemStack);
                }
                else
                {
                    oldStack = new ItemStack(null, 0, ConvertToInventoryIndex(cursorIndex), true);
                }
                SetSelectedItemStack(slots[cursorIndex].currentItemStack);
                slots[cursorIndex].isOccupied = false;
                slots[cursorIndex].spriteRenderer.sprite = inventorySlotSprite;
                slots[cursorIndex].currentItemStack = new ItemStack(oldStack);
                slots[cursorIndex].quantText.text = "";
            }
            else
            {
                SetSelectedItemStack(slots[cursorIndex].currentItemStack, false);
                slots[cursorIndex].currentItemStack.count--;
                slots[cursorIndex].quantText.text = slots[cursorIndex].currentItemStack.count.ToString();
                if (slots[cursorIndex].currentItemStack.count <= 0)
                {
                    slots[cursorIndex].isOccupied = false;
                    slots[cursorIndex].spriteRenderer.sprite = inventorySlotSprite;
                    slots[cursorIndex].quantText.text = "";
                }
            }

        }
        CheckForValidRecipe();
        return false;
    }
    void PlaceSelectedItem(bool stack, bool isMouse = false)
    {
        if (isMouse)
        {
            if (stack)
            {
                if (slots[cursorIndex].isOccupied)
                {
                    if (slots[cursorIndex].currentItemStack.item.itemName == m_MouseCursorSlot.currentItemStack.item.itemName)
                    {
                        slots[cursorIndex].currentItemStack.count += m_MouseCursorSlot.currentItemStack.count;
                        slots[cursorIndex].quantText.text = slots[cursorIndex].currentItemStack.count.ToString();
                        slots[cursorIndex].currentItemStack.index = ConvertToInventoryIndex(cursorIndex);
                        m_MouseCursorSlot.currentItemStack = new ItemStack(null, 0, -1, true);
                        m_MouseCursorSlot.spriteRenderer.sprite = null;
                        m_MouseCursorSlot.quantText.text = "";
                        m_MouseCursorSlot.isOccupied = false;
                    }
                    else
                    {
                        ItemStack oldStack = new ItemStack(slots[cursorIndex].currentItemStack);
                        slots[cursorIndex].currentItemStack = new ItemStack(m_MouseCursorSlot.currentItemStack);
                        slots[cursorIndex].spriteRenderer.sprite = m_MouseCursorSlot.currentItemStack.item.icon;
                        slots[cursorIndex].currentItemStack.index = ConvertToInventoryIndex(cursorIndex);
                        slots[cursorIndex].quantText.text = m_MouseCursorSlot.currentItemStack.count.ToString();
                        SetSelectedItemStack(oldStack, true, true);
                    }
                }
                else
                {
                    slots[cursorIndex].currentItemStack = new ItemStack(m_MouseCursorSlot.currentItemStack);
                    slots[cursorIndex].spriteRenderer.sprite = m_MouseCursorSlot.currentItemStack.item.icon;
                    slots[cursorIndex].quantText.text = m_MouseCursorSlot.currentItemStack.count.ToString();
                    slots[cursorIndex].currentItemStack.index = ConvertToInventoryIndex(cursorIndex);
                    slots[cursorIndex].isOccupied = true;
                    m_MouseCursorSlot.isOccupied = false;
                    m_MouseCursorSlot.currentItemStack = new ItemStack(null, 0, -1, true);
                    m_MouseCursorSlot.quantText.text = "";
                    m_MouseCursorSlot.spriteRenderer.sprite = null;
                }
            }
            else
            {
                if (slots[cursorIndex].isOccupied)
                {
                    if (slots[cursorIndex].currentItemStack.item.itemName == m_MouseCursorSlot.currentItemStack.item.itemName)
                    {
                        slots[cursorIndex].currentItemStack.count += 1;
                        slots[cursorIndex].quantText.text = slots[cursorIndex].currentItemStack.count.ToString();
                        if (m_MouseCursorSlot.currentItemStack.count - 1 <= 0)
                        {
                            m_MouseCursorSlot.isOccupied = false;
                            m_MouseCursorSlot.currentItemStack = new ItemStack(null, 0, -1, true);
                            m_MouseCursorSlot.quantText.text = "";
                            m_MouseCursorSlot.spriteRenderer.sprite = null;
                        }
                        else
                        {
                            m_MouseCursorSlot.currentItemStack.count--;
                            m_MouseCursorSlot.quantText.text = m_MouseCursorSlot.currentItemStack.count.ToString();
                        }
                    }
                    else
                    {
                        ItemStack oldStack = new ItemStack(slots[cursorIndex].currentItemStack);
                        slots[cursorIndex].currentItemStack = new ItemStack(m_MouseCursorSlot.currentItemStack);
                        slots[cursorIndex].spriteRenderer.sprite = m_MouseCursorSlot.currentItemStack.item.icon;
                        slots[cursorIndex].quantText.text = m_MouseCursorSlot.currentItemStack.count.ToString();
                        SetSelectedItemStack(oldStack, true, true);
                    }

                }
                else
                {
                    slots[cursorIndex].currentItemStack = new ItemStack(m_MouseCursorSlot.currentItemStack);
                    slots[cursorIndex].currentItemStack.count = 1;
                    slots[cursorIndex].spriteRenderer.sprite = m_MouseCursorSlot.currentItemStack.item.icon;
                    slots[cursorIndex].quantText.text = slots[cursorIndex].currentItemStack.count.ToString();
                    slots[cursorIndex].isOccupied = true;
                    if (m_MouseCursorSlot.currentItemStack.count - 1 <= 0)
                    {
                        m_MouseCursorSlot.isOccupied = false;
                        m_MouseCursorSlot.currentItemStack = new ItemStack(null, 0, -1, true);
                        m_MouseCursorSlot.quantText.text = "";
                        m_MouseCursorSlot.spriteRenderer.sprite = null;
                    }
                    else
                    {
                        m_MouseCursorSlot.currentItemStack.count--;
                        m_MouseCursorSlot.quantText.text = m_MouseCursorSlot.currentItemStack.count.ToString();
                    }

                }
            }
        }
        else
        {

            if (stack)
            {
                if (slots[cursorIndex].isOccupied)
                {
                    if (slots[cursorIndex].currentItemStack.item.itemName == cursorSlot.currentItemStack.item.itemName)
                    {
                        slots[cursorIndex].currentItemStack.count += cursorSlot.currentItemStack.count;
                        slots[cursorIndex].quantText.text = slots[cursorIndex].currentItemStack.count.ToString();
                        slots[cursorIndex].currentItemStack.index = ConvertToInventoryIndex(cursorIndex);
                        cursorSlot.currentItemStack = new ItemStack(null, 0, -1, true);
                        cursorSlot.spriteRenderer.sprite = null;
                        cursorSlot.quantText.text = "";
                        cursorSlot.isOccupied = false;
                    }
                    else
                    {
                        ItemStack oldStack = new ItemStack(slots[cursorIndex].currentItemStack);
                        slots[cursorIndex].currentItemStack = new ItemStack(cursorSlot.currentItemStack);
                        slots[cursorIndex].spriteRenderer.sprite = cursorSlot.currentItemStack.item.icon;
                        slots[cursorIndex].currentItemStack.index = ConvertToInventoryIndex(cursorIndex);
                        slots[cursorIndex].quantText.text = cursorSlot.currentItemStack.count.ToString();
                        SetSelectedItemStack(oldStack);
                    }
                }
                else
                {
                    slots[cursorIndex].currentItemStack = new ItemStack(cursorSlot.currentItemStack);
                    slots[cursorIndex].spriteRenderer.sprite = cursorSlot.currentItemStack.item.icon;
                    slots[cursorIndex].quantText.text = cursorSlot.currentItemStack.count.ToString();
                    slots[cursorIndex].currentItemStack.index = ConvertToInventoryIndex(cursorIndex);
                    slots[cursorIndex].isOccupied = true;
                    cursorSlot.isOccupied = false;
                    cursorSlot.currentItemStack = new ItemStack(null, 0, -1, true);
                    cursorSlot.quantText.text = "";
                    cursorSlot.spriteRenderer.sprite = null;
                }
            }
            else
            {
                if (slots[cursorIndex].isOccupied)
                {
                    if (slots[cursorIndex].currentItemStack.item.itemName == cursorSlot.currentItemStack.item.itemName)
                    {
                        slots[cursorIndex].currentItemStack.count += 1;
                        slots[cursorIndex].quantText.text = slots[cursorIndex].currentItemStack.count.ToString();
                        if (cursorSlot.currentItemStack.count - 1 <= 0)
                        {
                            cursorSlot.isOccupied = false;
                            cursorSlot.currentItemStack = new ItemStack(null, 0, -1, true);
                            cursorSlot.quantText.text = "";
                            cursorSlot.spriteRenderer.sprite = null;
                        }
                        else
                        {
                            cursorSlot.currentItemStack.count--;
                            cursorSlot.quantText.text = cursorSlot.currentItemStack.count.ToString();
                        }
                    }
                    else
                    {
                        ItemStack oldStack = new ItemStack(slots[cursorIndex].currentItemStack);
                        slots[cursorIndex].currentItemStack = new ItemStack(cursorSlot.currentItemStack);
                        slots[cursorIndex].spriteRenderer.sprite = cursorSlot.currentItemStack.item.icon;
                        slots[cursorIndex].quantText.text = cursorSlot.currentItemStack.count.ToString();
                        SetSelectedItemStack(oldStack);
                    }

                }
                else
                {
                    slots[cursorIndex].currentItemStack = new ItemStack(cursorSlot.currentItemStack);
                    slots[cursorIndex].currentItemStack.count = 1;
                    slots[cursorIndex].spriteRenderer.sprite = cursorSlot.currentItemStack.item.icon;
                    slots[cursorIndex].quantText.text = slots[cursorIndex].currentItemStack.count.ToString();
                    slots[cursorIndex].isOccupied = true;
                    if (cursorSlot.currentItemStack.count - 1 <= 0)
                    {
                        cursorSlot.isOccupied = false;
                        cursorSlot.currentItemStack = new ItemStack(null, 0, -1, true);
                        cursorSlot.quantText.text = "";
                        cursorSlot.spriteRenderer.sprite = null;
                    }
                    else
                    {
                        cursorSlot.currentItemStack.count--;
                        cursorSlot.quantText.text = cursorSlot.currentItemStack.count.ToString();
                    }

                }
            }
        }
        CheckForValidRecipe();
        AdjustButtonPrompts();
    }
    public void PlayerOpenUI(GameObject actor)
    {
        //if actor has a packable item
        // open the cargo inventory with an item in the closest avaliable slot

        if (isOpen)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            ActorEquipment ac = actor.GetComponent<ActorEquipment>();
            isOpen = false;
            ac.GetComponent<ThirdPersonUserControl>().craftingBenchUI = false;
            playerCurrentlyUsing = null;
            playerPrefix = null;
            cursorSlot.isOccupied = false;
            cursorSlot.spriteRenderer.sprite = null;
            cursorSlot.quantText.text = "";
            ReconcileItems(actor.GetComponent<PlayerInventoryManager>());
            Initialize();
        }
        else
        {
            playerCurrentlyUsing = actor;
            ActorEquipment ac = actor.GetComponent<ActorEquipment>();
            items = ac.inventoryManager.items;
            m_BeltItems = ac.inventoryManager.beltItems;
            DisplayItems();
            ac.GetComponent<ThirdPersonUserControl>().craftingBenchUI = true;
            playerPrefix = playerCurrentlyUsing.GetComponent<ThirdPersonUserControl>().playerPrefix;
            transform.GetChild(0).gameObject.SetActive(true);
            isOpen = true;
        }

    }

    public void Craft(CraftingBenchRecipe _recipe, bool mouse)
    {
        GameObject newItem = _recipe.product;
        int c = 0;
        for (int i = 3; i < 18; i++)
        {
            if ((i > 5 && i < 9) || (i > 11 && i < 15))
            {
                continue;
            }

            slots[i].currentItemStack = new ItemStack(null, 0, -1, true);
            slots[i].spriteRenderer.sprite = null;
            slots[i].isOccupied = false;
            slots[i].quantText.text = "";
            canCraft = false;
            currentProductRecipe = null;
            c++;
        }

        BuildingMaterial buildMat = newItem.GetComponent<BuildingMaterial>();
        if (buildMat != null && !buildMat.fitsInBackpack)
        {
            GameObject player = playerCurrentlyUsing;
            PlayerOpenUI(playerCurrentlyUsing);
            CameraControllerPerspective.Instance.SetCameraForBuild();
            player.GetComponent<BuilderManager>().Build(player.GetComponent<ThirdPersonUserControl>(), buildMat);
        }
        else
        {
            if (buildMat == null || buildMat != null && buildMat.fitsInBackpack)
            {
                if (mouse)
                {
                    if (m_MouseCursorSlot.isOccupied)
                    {
                        playerCurrentlyUsing.GetComponent<PlayerInventoryManager>().AddItem(newItem.GetComponent<Item>(), _recipe.quantity);
                    }
                    else
                    {
                        m_MouseCursorSlot.currentItemStack = new ItemStack(newItem.GetComponent<Item>(), 1, c, false);
                        m_MouseCursorSlot.spriteRenderer.sprite = productSlot.currentItemStack.item.icon;
                        m_MouseCursorSlot.currentItemStack.count = productSlot.currentItemStack.count;
                        m_MouseCursorSlot.isOccupied = true;
                    }
                }
                else
                {
                    if (cursorSlot.isOccupied)
                    {
                        playerCurrentlyUsing.GetComponent<PlayerInventoryManager>().AddItem(newItem.GetComponent<Item>(), _recipe.quantity);
                    }
                    else
                    {
                        cursorSlot.currentItemStack = new ItemStack(newItem.GetComponent<Item>(), 1, c, false);
                        cursorSlot.spriteRenderer.sprite = productSlot.currentItemStack.item.icon;
                        cursorSlot.currentItemStack.count = productSlot.currentItemStack.count;
                        cursorSlot.isOccupied = true;
                    }
                }
            }
        }
        productSlot.currentItemStack = new(null, 0, -1, true);
        productSlot.spriteRenderer.sprite = null;
        productSlot.quantText.text = "";
        productSlot.isOccupied = false;
        currentProductRecipe = null;
        canCraft = false;
    }

    public void CheckForValidRecipe()
    {
        Item[] recipe = new Item[9];
        int c = 0;
        for (int i = 3; i < 18; i++)
        {
            if ((i > 5 && i < 9) || (i > 11 && i < 15))
            {
                continue;
            }

            if (slots[i].currentItemStack != null || slots[i].currentItemStack.item != null && slots[i].isOccupied)
            {

                recipe[c] = slots[i].currentItemStack.item;
            }
            else
            {
                recipe[c] = null;
            }
            c++;
        }
        foreach (CraftingBenchRecipe _recipe in _craftingRecipes)
        {
            if (recipe.SequenceEqual(_recipe.ingredientsList))
            {
                if (_recipe.product.name.Contains("RealmwalkerDesk") && SceneManager.GetActiveScene().name != "HubWorld" && SceneManager.GetActiveScene().name != "TutorialWorld")
                {
                    ShowDamagePopup("Can not craft Realmwalker Desk in the Wilds", transform.position);
                    return;
                };
                if (currentProductRecipe != _recipe)
                {
                    Item productItem = _recipe.product.GetComponent<Item>();
                    productSlot.currentItemStack = new ItemStack(productItem, _recipe.quantity, 22, false);
                    productSlot.isOccupied = true;
                    productSlot.quantText.text = _recipe.quantity.ToString();
                    productSlot.spriteRenderer.sprite = productItem.icon;
                    canCraft = true;
                    currentProductRecipe = _recipe;
                }
                return;
            }
        }
        if (currentProductRecipe != null)
        {
            productSlot.currentItemStack = new ItemStack(null, 0, -1, true);
            productSlot.isOccupied = false;
            productSlot.quantText.text = "";
            productSlot.spriteRenderer.sprite = null;
            canCraft = false;
            currentProductRecipe = null;
        }
    }
    private void ShowDamagePopup(string message, Vector3 position)
    {
        GameObject popup = Instantiate(damagePopup, position + (Vector3.up * -4), Quaternion.identity);
        popup.GetComponent<DamagePopup>().Setup(message, Color.red);
    }
    public void ReconcileItems(PlayerInventoryManager actor)
    {
        ItemStack[] _items = new ItemStack[9];
        ItemStack[] _beltItems = new ItemStack[4];

        int c = 0;
        Dictionary<int, ItemStack> itemsInBench = new Dictionary<int, ItemStack>();

        //Search bench for remaining items
        for (int i = 3; i < 18; i++)
        {
            if ((i > 5 && i < 9) || (i > 11 && i < 15))
            {
                continue;
            }

            if (slots[i].currentItemStack.item != null && itemsInBench.ContainsKey(slots[i].currentItemStack.item.itemListIndex))
            {
                itemsInBench[slots[i].currentItemStack.item.itemListIndex].count += slots[i].currentItemStack.count;
            }
            else if (slots[i].currentItemStack.item != null)
            {
                itemsInBench.Add(slots[i].currentItemStack.item.itemListIndex, slots[i].currentItemStack);
            }
            slots[i].currentItemStack = new ItemStack(null, 0, -1, true);
            slots[i].isOccupied = false;
            slots[i].quantText.text = "";
            slots[i].spriteRenderer.sprite = null;
        }
        if (cursorSlot.currentItemStack.item != null && itemsInBench.ContainsKey(cursorSlot.currentItemStack.item.itemListIndex))
        {
            itemsInBench[cursorSlot.currentItemStack.item.itemListIndex].count += cursorSlot.currentItemStack.count;
        }
        else if (cursorSlot.currentItemStack.item != null)
        {
            itemsInBench.Add(cursorSlot.currentItemStack.item.itemListIndex, cursorSlot.currentItemStack);
        }
        cursorSlot.currentItemStack = new ItemStack(null, 0, -1, true);
        cursorSlot.isOccupied = false;
        cursorSlot.quantText.text = "";
        cursorSlot.spriteRenderer.sprite = null;

        if (m_MouseCursorSlot.currentItemStack.item != null && itemsInBench.ContainsKey(m_MouseCursorSlot.currentItemStack.item.itemListIndex))
        {
            itemsInBench[m_MouseCursorSlot.currentItemStack.item.itemListIndex].count += m_MouseCursorSlot.currentItemStack.count;
        }
        else if (m_MouseCursorSlot.currentItemStack.item != null)
        {
            itemsInBench.Add(m_MouseCursorSlot.currentItemStack.item.itemListIndex, m_MouseCursorSlot.currentItemStack);
        }
        m_MouseCursorSlot.currentItemStack = new ItemStack(null, 0, -1, true);
        m_MouseCursorSlot.isOccupied = false;
        m_MouseCursorSlot.quantText.text = "";
        m_MouseCursorSlot.spriteRenderer.sprite = null;

        //Gather all items in inventory portion of ui into an array
        for (int i = 0; i < 15; i++)
        {
            if ((i > 2 && i < 6) || (i > 8 && i < 12))
            {
                continue;
            }
            _items[c] = slots[i].currentItemStack;
            slots[i].currentItemStack = new(null, 0, -1, true);
            slots[i].quantText.text = "";
            slots[i].spriteRenderer.sprite = null;
            if (slots[i].currentItemStack.item != null && itemsInBench.ContainsKey(slots[i].currentItemStack.item.itemListIndex))
            {
                _items[c].count += itemsInBench[slots[i].currentItemStack.item.itemListIndex].count;

                itemsInBench.Remove(slots[i].currentItemStack.item.itemListIndex);
            }
            c++;

        }


        bool inventoryFull = false;
        foreach (KeyValuePair<int, ItemStack> entry in itemsInBench)
        {
            bool wasAdded = false;
            for (int i = 0; i < 9; i++)
            {
                if (_items[i].isEmpty)
                {
                    _items[i] = entry.Value;
                    wasAdded = true;
                    if (i == 8)
                    {
                        inventoryFull = true;
                    }
                    break;
                }
            }
            if (wasAdded)
            {
                continue;
            }
            else
            {
                inventoryFull = true;
                for (int i = 0; i < entry.Value.count; i++)
                {
                    ItemManager.Instance.CallDropItemRPC(entry.Value.item.itemListIndex, transform.position + Vector3.up * 2);
                }
            }
        }
        c = 0;
        for (int i = 18; i < 22; i++)
        {
            _beltItems[c] = slots[i].currentItemStack;
            slots[i].currentItemStack = new ItemStack(null, 0, -1, true);
            slots[i].isOccupied = false;
            slots[i].quantText.text = "";
            slots[i].spriteRenderer.sprite = null;
            c++;
        }
        actor.items = _items;
        actor.beltItems = _beltItems;
        for (int i = 0; i < cursorSlot.currentItemStack.count; i++)
        {
            if (inventoryFull)
            {
                ItemManager.Instance.CallDropItemRPC(cursorSlot.currentItemStack.item.itemListIndex, transform.position + Vector3.up * 2);
            }
            else
            {
                actor.GetComponent<ActorEquipment>().AddItemToInventory(cursorSlot.currentItemStack.item);
            }
        }


        actor.GetComponent<CharacterManager>().SaveCharacter();
    }
    public void UpdateInfoPanel(string name, string description, int value, int damage = 0)
    {
        infoPanel.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = name;
        infoPanel.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = description;
        infoPanel.transform.GetChild(1).GetChild(2).GetComponent<TextMeshProUGUI>().text = damage != 0 ? $"Damage: {damage}" : "";
        infoPanel.transform.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text = value != 0 ? $"{value}Gp" : "";

    }
    public void DisplayItems()
    {
        for (int i = 0; i < items.Length; i++)
        {
            SpriteRenderer sr = inventorySlots[i].spriteRenderer;
            TextMeshPro tm = inventorySlots[i].quantText;
            ItemStack stack = inventorySlots[i].currentItemStack;
            if (!items[i].isEmpty)
            {
                sr.sprite = items[i].item.icon;
                stack.item = items[i].item;
                stack.count = items[i].count;
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
                inventorySlots[i].isOccupied = true;
                if (items[i].count > 1)
                {
                    if (tm != null)
                    {
                        tm.text = items[i].count.ToString();
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
        for (int i = 0; i < m_BeltItems.Length; i++)
        {
            SpriteRenderer sr = inventorySlots[9 + i].spriteRenderer;
            TextMeshPro tm = inventorySlots[9 + i].quantText;
            ItemStack stack = inventorySlots[9 + i].currentItemStack;
            if (!m_BeltItems[i].isEmpty)
            {
                sr.sprite = m_BeltItems[i].item.icon;
                stack.item = m_BeltItems[i].item;
                stack.count = m_BeltItems[i].count;
                stack.isEmpty = false;
                inventorySlots[9 + i].isOccupied = true;

                if (m_BeltItems[i].count > 1)
                {
                    if (tm != null)
                    {
                        tm.text = m_BeltItems[i].count.ToString();
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
        UpdateButtonPrompts();
    }

    void AdjustButtonPrompts()
    {
        if (!LevelPrep.Instance.settingsConfig.showOnScreenControls) return;
        if (cursorSlot.isOccupied)
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





