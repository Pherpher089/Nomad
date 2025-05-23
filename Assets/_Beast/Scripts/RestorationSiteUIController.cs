using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class RestorationSiteUIController : MonoBehaviour
{
    // Resources Needed for Restoration
    public GameObject[] requiredResources;
    public int[] requiredResourcesCount;

    //UI Elements
    CraftingSlot[] m_currentResources;
    public CraftingSlot[] inventorySlots;
    CraftingSlot[] slots;
    GameObject cursor;
    CraftingSlot cursorSlot;
    GameObject m_MouseCursorObject;
    CraftingSlot m_MouseCursorSlot;
    GameObject[] requiredItemsImagesObjects;
    GameObject[] requiredItemsCountObjects;
    SpriteRenderer[] requiredItemsIcons;
    TMP_Text[] requiredItemsCountTexts;
    SpriteRenderer startButtonSprite;
    public ItemStack[] items;
    public ItemStack[] m_BeltItems;

    //State Variables
    public bool isOpen = false;
    public GameObject playerCurrentlyUsing = null;
    string playerPrefix;
    bool uiReturn = false;//Tracks the return of the input axis because they are not boolean input

    int cursorIndex = 0;
    public int currentIndex = 0;

    //Resources
    public Sprite inventorySlotSprite;
    public bool m_IsOpen = false;
    public string state;
    PhotonView photonView;
    DiggableController diggableController;
    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
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

    public void Initialize()
    {
        diggableController = GetComponent<DiggableController>();
        photonView = GetComponent<PhotonView>();
        LoadRestorationState();

        inventorySlots = new CraftingSlot[13];
        slots = new CraftingSlot[13 + requiredResources.Length];
        m_currentResources = new CraftingSlot[requiredResources.Length];
        requiredItemsImagesObjects = new GameObject[requiredResources.Length];
        requiredItemsCountObjects = new GameObject[requiredResources.Length]; ;
        requiredItemsIcons = new SpriteRenderer[requiredResources.Length];
        requiredItemsCountTexts = new TMP_Text[requiredResources.Length];
        for (int i = 0; i < 13; i += 1)
        {
            inventorySlots[i] = transform.GetChild(0).GetChild(i).GetComponent<CraftingSlot>();
        }
        for (int i = 0; i < 5; i++)
        {
            if (i < requiredResources.Length)
            {
                m_currentResources[i] = transform.GetChild(0).GetChild(i + 13).GetComponent<CraftingSlot>();
            }
            else
            {
                transform.GetChild(0).GetChild(i + 13).gameObject.SetActive(false);
            }
        }
        //Initalize the resource slots based on the required resources
        int inventoryCounter = 0;
        int craftingCounter = 0;
        for (int i = 0; i < 13 + m_currentResources.Length; i++)
        {
            if (i < 13)
            {

                slots[i] = inventorySlots[inventoryCounter];
                inventoryCounter++;
            }
            else
            {

                slots[i] = m_currentResources[craftingCounter];
                slots[i].currentItemStack.item = null;
                slots[i].currentItemStack.count = 0;
                slots[i].isOccupied = false;
                slots[i].spriteRenderer.sprite = null;
                slots[i].quantText.text = "";
                craftingCounter++;
            }
        }

        for (int i = 0; i < 5; i++)
        {
            if (i < requiredResources.Length)
            {
                requiredItemsImagesObjects[i] = transform.GetChild(0).GetChild(i + 18).gameObject;
                requiredItemsCountObjects[i] = transform.GetChild(0).GetChild(i + 23).gameObject;
                requiredItemsIcons[i] = requiredItemsImagesObjects[i].transform.GetChild(0).GetComponent<SpriteRenderer>();
                requiredItemsIcons[i].sprite = requiredResources[i].GetComponent<Item>().icon;
                requiredItemsCountTexts[i] = requiredItemsCountObjects[i].transform.GetChild(0).GetComponent<TMP_Text>();
                requiredItemsCountTexts[i].text = $"{m_currentResources[i].currentItemStack.count}/{requiredResourcesCount[i]}";
            }
            else
            {
                transform.GetChild(0).GetChild(i + 18).gameObject.SetActive(false);
                transform.GetChild(0).GetChild(i + 23).gameObject.SetActive(false);
            }
        }

        inventorySlotSprite = inventorySlots[0].spriteRenderer.sprite;
        //The cursor is the 10th child
        cursor = transform.GetChild(0).GetChild(28).gameObject;
        cursorSlot = cursor.GetComponent<CraftingSlot>();
        m_MouseCursorObject = transform.GetChild(0).GetChild(29).gameObject;
        m_MouseCursorSlot = m_MouseCursorObject.GetComponent<CraftingSlot>();
        transform.GetChild(0).gameObject.SetActive(false);
        startButtonSprite = transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(1).GetComponent<SpriteRenderer>();
        startButtonSprite.color = new Color(9, 9, 9, 0.5f);
        m_IsOpen = false;
        UpdateRequirementCounts();
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
    void HandleMouseInput()
    {
        // Raycast to detect UI element under the mouse in world space
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        int layerMask = ~LayerMask.GetMask("MousePlane", "Default", "TransparentFX", "Ignore Raycast", "Water", "Terrain", "NewMousePLane", "Item", "Bullet", "OutLine", "Door", "Player", "Interact", "Build", "Floor", "Enemy", "Structure", "Terrain", "PostProcessing", "EnemyPlayerCollision", "Arrow", "Wall");

        if (Physics.Raycast(ray, out hit, 1000f, layerMask, QueryTriggerInteraction.Collide)) // Use 1000f or any max distance that suits your setup
        {

            GameObject hoveredSlot = hit.collider.gameObject;

            // Check if the clicked object is an InventorySlot
            if (hoveredSlot.CompareTag("InventorySlot") && hoveredSlot.transform.childCount > 0)
            {
                if (!cursorSlot.currentItemStack.isEmpty)
                {
                    m_MouseCursorSlot.currentItemStack = new(cursorSlot.currentItemStack);
                    cursorSlot.currentItemStack = new();
                }
                InventoryActionMouse(hoveredSlot);
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
                    m_MouseCursorSlot.quantText.text = m_MouseCursorSlot.currentItemStack.count.ToString();
                    playerCurrentlyUsing.GetComponent<PlayerInventoryManager>().SpendItem(m_MouseCursorSlot.currentItemStack.item);
                    if (m_MouseCursorSlot.currentItemStack.count <= 0)
                    {
                        m_MouseCursorSlot.currentItemStack = new ItemStack(null, 0, -1, true);
                        m_MouseCursorSlot.spriteRenderer.sprite = null;
                        m_MouseCursorSlot.quantText.text = "";
                        m_MouseCursorSlot.isOccupied = false;
                    }
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
                    playerCurrentlyUsing.GetComponent<PlayerInventoryManager>().SpendItem(m_MouseCursorSlot.currentItemStack.item, m_MouseCursorSlot.currentItemStack.count);

                    m_MouseCursorSlot.currentItemStack = new ItemStack(null, 0, -1, true);
                    m_MouseCursorSlot.spriteRenderer.sprite = null;
                    m_MouseCursorSlot.quantText.text = "";
                    m_MouseCursorSlot.isOccupied = false;
                }
            }
            return;
        }
        else
        {
            int slotIndex = clickedSlot.transform.GetSiblingIndex();
            cursorIndex = slotIndex;
            MoveCursor(slotIndex);

            if (Input.GetMouseButtonDown(0))
            {
                if (slotIndex == startButtonSprite.gameObject.transform.parent.GetSiblingIndex() && UpdateRequirementCounts())
                {
                    StartRestoration();
                    return;
                }
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

    void StartRestoration()
    {
        Debug.Log("### Start Button Clicked");
        startButtonSprite.color = new Color(1, 1, 1, 1f);
        BeastManager.Instance.StartDigging(photonView.ViewID);
        PlayerOpenUI(playerCurrentlyUsing);
    }

    bool UpdateRequirementCounts()
    {
        bool allResources = true;
        for (int i = 0; i < requiredResources.Length; i++)
        {
            if (requiredResources[i] != null)
            {
                requiredItemsCountTexts[i].text = $"{m_currentResources[i].currentItemStack.count}/{requiredResourcesCount[i]}";
                if (m_currentResources[i].currentItemStack.count < requiredResourcesCount[i] || m_currentResources[i].currentItemStack.item.itemListIndex != requiredResources[i].GetComponent<Item>().itemListIndex)
                {
                    allResources = false;
                }
            }
        }
        if (allResources)
        {
            startButtonSprite.color = new Color(9, 9, 9, 1);
        }
        else
        {
            startButtonSprite.color = new Color(9, 9, 9, 0.5f);
        }
        return allResources;
    }

    void MoveCursor(int index)
    {
        if (index > slots.Length - 1)
        {
            return;
        }
        cursor.transform.position = slots[index].transform.position;
        if (slots[index].currentItemStack.item != null)
        {
            // UpdateInfoPanel(slots[index].currentItemStack.item.itemName, slots[index].currentItemStack.item.itemDescription, slots[index].currentItemStack.item.value, 0);
        }
        else
        {
            // UpdateInfoPanel("", "", 0, 0);
        }
    }
    void MoveCursor(Vector2 direction)
    {
        if (direction.x > 0 && cursorIndex != 5 && cursorIndex != 11 && cursorIndex != 17 && cursorIndex <= 17)
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
                    cursorIndex = 22;
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
                case 22:
                    cursorIndex = 6;
                    break;
            }
        }
        else if (direction.x < 0 && cursorIndex != 0 && cursorIndex != 6 && cursorIndex != 12 && cursorIndex < 17)
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
                    cursorIndex = 22;
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
                case 22:
                    cursorIndex = 11;
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

        // Should be something like check for all resources or something
        // CheckForValidRecipe();
        UpdateRequirementCounts();
    }
    bool SelectItem(bool stack, bool isMouse = false)
    {

        if (isMouse)
        {
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
            UpdateRequirementCounts();
            return false;
        }

        //If cursor is on crafting side
        if (cursorIndex < slots.Length && slots[cursorIndex].isOccupied)
        {
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
        // CheckForValidRecipe();
        UpdateRequirementCounts();
        return false;
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

    [PunRPC]
    public void SaveRequiredResources(string _state)
    {
        state = _state;
    }

    [PunRPC]
    public void SaveRestorationState()
    {
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{LevelPrep.Instance.settlementName}/Restorations/");
        Directory.CreateDirectory(saveDirectoryPath);
        string name = LevelPrep.Instance.currentLevel;
        string filePath = saveDirectoryPath + name + ".json";
        string json = "";
        if (File.Exists(filePath))
        {
            json = File.ReadAllText(filePath);
        }
        int[] data = json != "" ? JsonConvert.DeserializeObject<int[]>(json) : new int[0];
        if (!data.Contains(photonView.ViewID))
        {
            // Convert to a list, append the value, and convert back to an array
            data = data.Concat(new[] { photonView.ViewID }).ToArray();
        }
        using (FileStream stream = new FileStream(filePath, FileMode.Create))
        using (StreamWriter writer = new StreamWriter(stream))
        {
            // Write the JSON string to the file
            writer.Write(JsonConvert.SerializeObject(data));
        }
    }

    public void LoadRestorationState()
    {
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{LevelPrep.Instance.settlementName}/Restorations/");
        Directory.CreateDirectory(saveDirectoryPath);
        string name = LevelPrep.Instance.currentLevel;
        string filePath = saveDirectoryPath + name + ".json";
        string json;
        if (!File.Exists(filePath))
        {
            return;
        }
        json = File.ReadAllText(filePath);
        if (json == null)
        {
            return;
        }
        int[] data = JsonConvert.DeserializeObject<int[]>(json);
        if (data.Contains(photonView.ViewID))
        {
            photonView.RPC("QuickCompleteDig", RpcTarget.All);
        }

    }

    [PunRPC]
    void QuickCompleteDig()
    {
        diggableController.QuickCompleteDig();
    }

    public void ReconcileItems(PlayerInventoryManager actor)
    {
        ItemStack[] _items = new ItemStack[9];
        ItemStack[] _beltItems = new ItemStack[4];

        int c = 0;
        Dictionary<int, ItemStack> itemsInBench = new Dictionary<int, ItemStack>();

        //Search bench for remaining items
        for (int i = 13; i < 18; i++)
        {
            if (requiredResources.Length > i - 13)
            {
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
        for (int i = 0; i < 9; i++)
        {
            _items[c] = slots[i].currentItemStack;
            slots[i].currentItemStack = new(null, 0, -1, true);
            slots[i].quantText.text = "";
            slots[i].spriteRenderer.sprite = null;
            // if (slots[i].currentItemStack.item != null && itemsInBench.ContainsKey(slots[i].currentItemStack.item.itemListIndex))
            // {
            //     _items[c].count += itemsInBench[slots[i].currentItemStack.item.itemListIndex].count;

            //     itemsInBench.Remove(slots[i].currentItemStack.item.itemListIndex);
            // }
            c++;

        }
        string _state = "[";
        foreach (KeyValuePair<int, ItemStack> entry in itemsInBench)
        {
            _state += "[" + entry.Value.item.itemListIndex + ", " + entry.Value.count + "],";
        }
        _state += "]";

        state = _state;
        photonView.RPC("SaveRequiredResources", RpcTarget.All, state);
        bool inventoryFull = false;
        // foreach (KeyValuePair<int, ItemStack> entry in itemsInBench)
        // {
        //     bool wasAdded = false;
        //     for (int i = 0; i < 9; i++)
        //     {
        //         if (_items[i].isEmpty)
        //         {
        //             _items[i] = entry.Value;
        //             wasAdded = true;
        //             if (i == 8)
        //             {
        //                 inventoryFull = true;
        //             }
        //             break;
        //         }
        //     }
        //     if (wasAdded)
        //     {
        //         continue;
        //     }
        //     else
        //     {
        //         inventoryFull = true;
        //         for (int i = 0; i < entry.Value.count; i++)
        //         {
        //             ItemManager.Instance.CallDropItemRPC(entry.Value.item.itemListIndex, transform.position + Vector3.up * 2);
        //         }
        //     }
        // }
        c = 0;
        for (int i = 9; i < 13; i++)
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
        if (state != null && state != "")
        {
            int[][] itemsArray;

            itemsArray = JsonConvert.DeserializeObject<int[][]>(state);

            for (int i = 0; i < itemsArray.Length; i++)
            {
                int itemIndex = itemsArray[i][0];
                int itemCount = itemsArray[i][1];
                ItemStack stack = new ItemStack(ItemManager.Instance.itemList[itemIndex].GetComponent<Item>(), itemCount, -1, false);
                slots[13 + i].currentItemStack = stack;
                slots[13 + i].spriteRenderer.sprite = stack.item.icon;
                slots[13 + i].quantText.text = stack.count.ToString();
                slots[13 + i].isOccupied = true;
            }
        }
    }
}
