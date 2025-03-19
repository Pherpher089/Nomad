using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaddleStationUIController : MonoBehaviour
{
    public Sprite inventorySlotSprite;
    //The UI GameObject
    public bool isOpen = false;
    public GameObject playerCurrentlyUsing = null;
    public BeastCraftingSlot[] inventorySlots;
    BeastCraftingSlot[] equippedItemSlots;
    GameObject cursorObject;
    BeastCraftingSlot cursorSlot;
    GameObject mouseCursorObject;
    BeastCraftingSlot mouseCursorSlot;
    string playerPrefix;
    Dictionary<Item, List<int>> CraftingItems;
    bool uiReturn = false;                         //Tracks the return of the input axis because they are not boolean input
    int cursorIndex = 0;
    public Dictionary<int[], int> craftingRecipes;
    public ItemStack[] items;
    GameObject infoPanel;
    TMP_Text infoText;
    TMP_Text nameText;
    TMP_Text levelText;
    TMP_Text behaviorText;
    float messageTimeOut = 10;

    BuildingMaterial m_BuildingMaterial;
    GameObject[] buttonPrompts;
    Sprite antlerSlotIcon;
    Sprite headSlotIcon;
    Sprite backSlotIcon;
    Sprite leftSideSlotIcon;
    Sprite rightSideSlotIcon;
    Sprite rumpSlotIcon;
    Sprite shoeSlotIcon;
    Sprite saddleSlotIcon;

    string[] m_gearSlotNames = new string[] { "Antler", "Back", "Head", "Shoe", "Right Side", "Rump", "Left Side" };

    void Start()
    {
        antlerSlotIcon = Resources.Load<Sprite>("Sprites/AntlerSlotIcon");
        headSlotIcon = Resources.Load<Sprite>("Sprites/HeadSlotIcon");
        backSlotIcon = Resources.Load<Sprite>("Sprites/BackSlotIcon");
        leftSideSlotIcon = Resources.Load<Sprite>("Sprites/LeftSideSlotIcon");
        rightSideSlotIcon = Resources.Load<Sprite>("Sprites/RightSideSlotIcon");
        rumpSlotIcon = Resources.Load<Sprite>("Sprites/RumpSlotIcon");
        shoeSlotIcon = Resources.Load<Sprite>("Sprites/ShoeSlotIcon");
        saddleSlotIcon = Resources.Load<Sprite>("Sprites/SaddleSlotIcon");
        equippedItemSlots = new BeastCraftingSlot[7];
        inventorySlots = new BeastCraftingSlot[20];
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
        for (int i = 0; i < 20; i++)
        {
            inventorySlots[i] = transform.GetChild(0).GetChild(i).GetComponent<BeastCraftingSlot>();
            inventorySlots[i].beastGearStack = new BeastGearStack(null, -1, true);
            inventorySlots[i].isOccupied = false;
            inventorySlots[i].spriteRenderer.sprite = null;
        }
        cursorObject = transform.GetChild(0).GetChild(27).gameObject;
        cursorSlot = cursorObject.GetComponent<BeastCraftingSlot>();
        mouseCursorObject = transform.GetChild(0).GetChild(28).gameObject;
        mouseCursorSlot = mouseCursorObject.GetComponent<BeastCraftingSlot>();
        infoPanel = transform.GetChild(0).GetChild(29).gameObject;
        nameText = transform.GetChild(0).GetChild(30).GetChild(1).GetChild(0).GetComponent<TMP_Text>();
        levelText = transform.GetChild(0).GetChild(31).GetChild(1).GetChild(0).GetComponent<TMP_Text>();
        infoText = transform.GetChild(0).GetChild(37).GetChild(0).GetChild(0).GetComponent<TMP_Text>();
        infoText.text = "";
        transform.GetChild(0).gameObject.SetActive(false);
        for (int i = 20; i < 27; i++)
        {
            equippedItemSlots[i - 20] = transform.GetChild(0).GetChild(i).GetComponent<BeastCraftingSlot>();
            equippedItemSlots[i - 20].spriteRenderer.sprite = GetEquipmentIcon(i - 20);
        }
        cursorIndex = 0;
        isOpen = false;
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
                if (cursorSlot.isOccupied)
                {
                    mouseCursorSlot.beastGearStack = new(cursorSlot.beastGearStack);
                    cursorSlot.beastGearStack = new();
                }
                InventoryActionMouse(hoveredSlot);
            }
        }
        else
        {
            InventoryActionMouse(null);
        }

        Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        mouseCursorObject.transform.position = _ray.GetPoint(Vector3.Distance(Camera.main.transform.position, transform.GetChild(0).position)); // Move cursor to the point where the ray hits the plane
    }
    void InventoryActionMouse(GameObject clickedSlot)
    {

        if (clickedSlot != null)
        {
            int slotIndex = clickedSlot.transform.GetSiblingIndex();
            cursorIndex = slotIndex;
            MoveCursor(slotIndex);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (!mouseCursorSlot.isOccupied)
            {
                SelectItem(true);
            }
            else
            {
                PlaceSelectedItem(true);
            }
        }
    }
    private void UpdateAvailableSlots()
    {
        for (int i = 0; i < 7; i++)
        {
            if (BeastManager.Instance.m_BlockedGearSlots.Contains(i))
            {
                equippedItemSlots[i].spriteRenderer.color = new Color(1f, 1f, 1f, .5f);
                equippedItemSlots[i].spriteRenderer.transform.parent.GetChild(1).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, .5f);
            }
            else
            {

                equippedItemSlots[i].spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
                equippedItemSlots[i].spriteRenderer.transform.parent.GetChild(1).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
            }
        }
    }
    void SelectItem(bool isMouse = false)
    {

        if (isMouse)
        {
            if (cursorIndex < 20)
            {
                if (inventorySlots[cursorIndex].isOccupied)
                {
                    mouseCursorSlot.beastGearStack = new(inventorySlots[cursorIndex].beastGearStack);
                    mouseCursorSlot.isOccupied = true;
                    mouseCursorSlot.spriteRenderer.sprite = mouseCursorSlot.beastGearStack.beastGear.icon;
                    inventorySlots[cursorIndex].beastGearStack = new();
                    inventorySlots[cursorIndex].isOccupied = false;
                    inventorySlots[cursorIndex].spriteRenderer.sprite = null;
                }
            }
            else
            {
                if (equippedItemSlots[cursorIndex - 20].isOccupied)
                {
                    // int gearIndex = equippedItemSlots[cursorIndex - 20].beastGearStack.beastGear.gearIndex;
                    int gearIndex = -1;
                    if (equippedItemSlots[cursorIndex - 20].beastGearStack.beastGear.gearIndex.Contains(cursorIndex - 20))
                    {
                        gearIndex = cursorIndex - 20;
                    }

                    mouseCursorSlot.beastGearStack = new(equippedItemSlots[cursorIndex - 20].beastGearStack);
                    int[] emptyBlockedSlots = equippedItemSlots[cursorIndex - 20].beastGearStack.beastGear.blockedSlotIndices;
                    mouseCursorSlot.isOccupied = true;
                    mouseCursorSlot.spriteRenderer.sprite = mouseCursorSlot.beastGearStack.beastGear.icon;
                    equippedItemSlots[cursorIndex - 20].beastGearStack = new();
                    equippedItemSlots[cursorIndex - 20].isOccupied = false;
                    equippedItemSlots[cursorIndex - 20].spriteRenderer.sprite = GetEquipmentIcon(cursorIndex - 20);
                    int[] emptyGear = new int[] { -1, -1, -1, -1 };
                    BeastManager.Instance.EquipGear(emptyGear, gearIndex, 0, emptyBlockedSlots);
                    UpdateAvailableSlots();
                }
            }
        }
        else
        {
            if (cursorIndex < 20)
            {
                if (inventorySlots[cursorIndex].isOccupied)
                {
                    cursorSlot.beastGearStack = new(inventorySlots[cursorIndex].beastGearStack);
                    cursorSlot.isOccupied = true;
                    cursorSlot.spriteRenderer.sprite = cursorSlot.beastGearStack.beastGear.icon;
                    inventorySlots[cursorIndex].beastGearStack = new();
                    inventorySlots[cursorIndex].isOccupied = false;
                    inventorySlots[cursorIndex].spriteRenderer.sprite = null;
                }
            }
            else
            {
                if (equippedItemSlots[cursorIndex - 20].isOccupied)
                {
                    int gearIndex = -1;
                    if (equippedItemSlots[cursorIndex - 20].beastGearStack.beastGear.gearIndex.Contains(cursorIndex - 20))
                    {
                        gearIndex = cursorIndex - 20;
                    }
                    cursorSlot.beastGearStack = new(equippedItemSlots[cursorIndex - 20].beastGearStack);
                    int[] emptyBlockedSlots = equippedItemSlots[cursorIndex - 20].beastGearStack.beastGear.blockedSlotIndices;
                    cursorSlot.isOccupied = true;
                    cursorSlot.spriteRenderer.sprite = cursorSlot.beastGearStack.beastGear.icon;
                    equippedItemSlots[cursorIndex - 20].beastGearStack = new();
                    equippedItemSlots[cursorIndex - 20].isOccupied = false;
                    equippedItemSlots[cursorIndex - 20].spriteRenderer.sprite = GetEquipmentIcon(cursorIndex - 20);
                    int[] emptyGear = new int[] { -1, -1, -1, -1 };
                    BeastManager.Instance.EquipGear(emptyGear, gearIndex, 0, emptyBlockedSlots);
                    UpdateAvailableSlots();

                }
            }
        }
    }
    public void UpdateMessageText(string info, Color textColor)
    {
        messageTimeOut = 10f;
        infoText.text = info;
        infoText.color = textColor;
    }

    Sprite GetEquipmentIcon(int index)
    {
        return index switch
        {
            0 => antlerSlotIcon,
            1 => backSlotIcon,
            2 => headSlotIcon,
            3 => shoeSlotIcon,
            4 => rightSideSlotIcon,
            5 => rumpSlotIcon,
            6 => leftSideSlotIcon,
            _ => null,
        };
    }

    void PlaceSelectedItem(bool isMouse = false)
    {
        if (isMouse && mouseCursorSlot.isOccupied)
        {
            if (cursorIndex < 20)
            {
                inventorySlots[cursorIndex].beastGearStack = new(mouseCursorSlot.beastGearStack);
                inventorySlots[cursorIndex].isOccupied = true;
                inventorySlots[cursorIndex].spriteRenderer.sprite = inventorySlots[cursorIndex].beastGearStack.beastGear.icon;
                mouseCursorSlot.beastGearStack = new();
                mouseCursorSlot.isOccupied = false;
                mouseCursorSlot.spriteRenderer.sprite = null;
            }
            else
            {
                if (mouseCursorSlot.beastGearStack.beastGear.requiredLevel > LevelManager.Instance.beastLevel)
                {
                    UpdateMessageText("The beasts is to young to wear this gear", Color.white);
                    return;
                };
                int beastGearItemListIndex = -1;
                foreach (int idx in mouseCursorSlot.beastGearStack.beastGear.gearItemIndices)
                {
                    if (idx != -1)
                    {
                        beastGearItemListIndex = idx;
                        break;
                    }
                }
                if (beastGearItemListIndex == -1) return;
                for (int i = 0; i < equippedItemSlots.Length; i++)
                {
                    if (mouseCursorSlot.beastGearStack.beastGear.blockedSlotIndices.Contains(i) && equippedItemSlots[i].isOccupied)
                    {
                        UpdateMessageText($"Can not equipped this gear because the slots it blocks off are currently occupied. Please clear the {m_gearSlotNames[i]} slot and try again.", Color.white);
                        return;
                    }
                }

                for (int i = 0; i < mouseCursorSlot.beastGearStack.beastGear.gearIndex.Length; i++)
                {
                    // we are interested in the slot being equipped to
                    // 
                    if (mouseCursorSlot.beastGearStack.beastGear.gearIndex[i] == cursorIndex - 20)
                    {
                        if (BeastManager.Instance.m_BlockedGearSlots.Contains(cursorIndex - 20))
                        {
                            UpdateMessageText("This slot is blocked", Color.white);
                            return;
                        }
                    }
                }
                if (mouseCursorSlot.beastGearStack.beastGear.gearIndex.Contains(cursorIndex - 20))
                {
                    if (equippedItemSlots[cursorIndex - 20].isOccupied)
                    {
                        foreach (BeastCraftingSlot slot in inventorySlots)
                        {
                            if (!slot.isOccupied)
                            {
                                slot.beastGearStack = new(equippedItemSlots[cursorIndex - 20].beastGearStack);
                                slot.isOccupied = true;
                                slot.spriteRenderer.sprite = slot.beastGearStack.beastGear.icon;
                            }
                        }
                    }
                    equippedItemSlots[cursorIndex - 20].beastGearStack = new(mouseCursorSlot.beastGearStack);
                    equippedItemSlots[cursorIndex - 20].isOccupied = true;
                    equippedItemSlots[cursorIndex - 20].spriteRenderer.sprite = equippedItemSlots[cursorIndex - 20].beastGearStack.beastGear.icon;
                    mouseCursorSlot.beastGearStack = new();
                    mouseCursorSlot.isOccupied = false;
                    mouseCursorSlot.spriteRenderer.sprite = null;
                }
                else
                {
                    UpdateMessageText($"This gear can not be equipped in this slot.", Color.white);
                    return;
                }
                BeastManager.Instance.EquipGear(equippedItemSlots[cursorIndex - 20].beastGearStack.beastGear.gearItemIndices, cursorIndex - 20, equippedItemSlots[cursorIndex - 20].beastGearStack.beastGear.requiredLevel, equippedItemSlots[cursorIndex - 20].beastGearStack.beastGear.blockedSlotIndices);
                UpdateAvailableSlots();

            }
        }
        else if (cursorSlot.isOccupied)
        {
            if (cursorIndex < 20)
            {
                inventorySlots[cursorIndex].beastGearStack = new(cursorSlot.beastGearStack);
                inventorySlots[cursorIndex].isOccupied = true;
                inventorySlots[cursorIndex].spriteRenderer.sprite = inventorySlots[cursorIndex].beastGearStack.beastGear.icon;
                cursorSlot.beastGearStack = new();
                cursorSlot.isOccupied = false;
                cursorSlot.spriteRenderer.sprite = null;
            }
            else
            {
                if (mouseCursorSlot.beastGearStack.beastGear.requiredLevel > LevelManager.Instance.beastLevel) return;
                int index = -1;
                foreach (int idx in mouseCursorSlot.beastGearStack.beastGear.gearItemIndices)
                {
                    if (idx != -1)
                    {
                        index = idx;
                        break;
                    }
                }

                if (index == -1) return;

                for (int i = 0; i < mouseCursorSlot.beastGearStack.beastGear.gearIndex.Length; i++)
                {
                    if (mouseCursorSlot.beastGearStack.beastGear.gearIndex[i] != -1)
                    {
                        if (BeastManager.Instance.m_BlockedGearSlots.Contains(mouseCursorSlot.beastGearStack.beastGear.gearIndex[i]))
                        {
                            return;
                        }
                    }
                }

                if (mouseCursorSlot.beastGearStack.beastGear.gearIndex.Contains(cursorIndex - 20))
                {
                    if (equippedItemSlots[cursorIndex - 20].isOccupied)
                    {
                        foreach (BeastCraftingSlot slot in inventorySlots)
                        {
                            if (!slot.isOccupied)
                            {
                                slot.beastGearStack = new(equippedItemSlots[cursorIndex - 20].beastGearStack);
                                slot.isOccupied = true;
                                slot.spriteRenderer.sprite = slot.beastGearStack.beastGear.icon;
                            }
                        }
                    }
                    equippedItemSlots[cursorIndex - 20].beastGearStack = new(mouseCursorSlot.beastGearStack);
                    equippedItemSlots[cursorIndex - 20].isOccupied = true;
                    equippedItemSlots[cursorIndex - 20].spriteRenderer.sprite = equippedItemSlots[cursorIndex - 20].beastGearStack.beastGear.icon;
                    mouseCursorSlot.beastGearStack = new();
                    mouseCursorSlot.isOccupied = false;
                    mouseCursorSlot.spriteRenderer.sprite = null;
                    BeastManager.Instance.EquipGear(equippedItemSlots[cursorIndex - 20].beastGearStack.beastGear.gearItemIndices, cursorIndex - 20, equippedItemSlots[cursorIndex - 20].beastGearStack.beastGear.requiredLevel, equippedItemSlots[cursorIndex - 20].beastGearStack.beastGear.blockedSlotIndices);
                    UpdateAvailableSlots();
                }
            }
        }
    }
    void MoveCursor(int index)
    {
        if (index < 20)
        {
            cursorObject.transform.localScale = new(2, 2, 2);
            cursorObject.transform.position = inventorySlots[index].transform.position;
            if (inventorySlots[index].beastGearStack.beastGear != null)
            {
                UpdateInfoPanel(inventorySlots[index].beastGearStack.beastGear.name, inventorySlots[index].beastGearStack.beastGear.description, inventorySlots[index].beastGearStack.beastGear.requiredLevel, inventorySlots[index].beastGearStack.beastGear.blockedSlotIndices);
            }
            else
            {
                UpdateInfoPanel("", "", -1, null);
            }
        }
        else if (index < 30)
        {

            cursorObject.transform.localScale = new(4, 2, 2);
            cursorObject.transform.position = equippedItemSlots[index - 20].transform.position;
            if (equippedItemSlots[index - 20].beastGearStack.beastGear != null && !BeastManager.Instance.m_BlockedGearSlots.Contains(index - 20))
            {
                UpdateInfoPanel(equippedItemSlots[index - 20].beastGearStack.beastGear.name, equippedItemSlots[index - 20].beastGearStack.beastGear.description, equippedItemSlots[index - 20].beastGearStack.beastGear.requiredLevel, equippedItemSlots[index - 20].beastGearStack.beastGear.blockedSlotIndices);
            }
            else
            {
                UpdateInfoPanel("", "", -1, null);
            }
        }
    }

    public void Update()
    {
        if (playerCurrentlyUsing != null)
        {
            if (playerPrefix == "sp")
            {
                if (mouseCursorObject.activeSelf == false)
                {
                    mouseCursorObject.SetActive(true);
                }
                HandleMouseInput();
            }
            else
            {
                mouseCursorObject.SetActive(false);
                ListenToDirectionalInput();
                ListenToActionInput();
            }
            if (messageTimeOut > 0)
            {
                messageTimeOut -= Time.deltaTime;
            }
            else
            {
                UpdateMessageText("", Color.white);
            }
        }
        else
        {
            UpdateMessageText("", Color.white);
        }
    }

    void ListenToActionInput()
    {
        if (Input.GetButtonDown(playerPrefix + "Grab"))
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

    void MoveCursor(Vector2 direction)
    {
        if (direction.x > 0 && cursorIndex != 3 && cursorIndex != 7 && cursorIndex != 11 && cursorIndex != 15 && cursorIndex != 19 && cursorIndex < 20)
        {
            if (cursorIndex + 1 < inventorySlots.Length)
            {
                cursorIndex += 1;
            }
        }
        else if (direction.x < 0 && cursorIndex != 0 && cursorIndex != 4 && cursorIndex != 8 && cursorIndex != 12 && cursorIndex != 16 && cursorIndex < 20)
        {
            if (cursorIndex - 1 > -1)
            {
                cursorIndex -= 1;
            }
        }
        else
        {
            if (direction.x > 0)
            {
                switch (cursorIndex)
                {
                    case 3:
                        cursorIndex = 20;
                        break;
                    case 7:
                    case 11:
                        cursorIndex = 23;
                        break;
                    case 15:
                        cursorIndex = 12;
                        break;
                    case 19:
                        cursorIndex = 16;
                        break;
                    case 20:
                        cursorIndex = 21;
                        break;
                    case 23:
                        cursorIndex = 24;
                        break;
                    case 21:
                        cursorIndex = 22;
                        break;
                    case 24:
                        cursorIndex = 25;
                        break;
                    case 22:
                        cursorIndex = 0;
                        break;
                    case 25:
                        cursorIndex = 4;
                        break;
                }
            }
            if (direction.x < 0)
            {
                switch (cursorIndex)
                {
                    case 0:
                        cursorIndex = 22;
                        break;
                    case 4:
                    case 8:
                        cursorIndex = 25;
                        break;
                    case 12:
                        cursorIndex = 15;
                        break;
                    case 16:
                        cursorIndex = 19;
                        break;
                    case 22:
                        cursorIndex = 21;
                        break;
                    case 25:
                        cursorIndex = 24;
                        break;
                    case 21:
                        cursorIndex = 20;
                        break;
                    case 24:
                        cursorIndex = 23;
                        break;
                    case 20:
                        cursorIndex = 3;
                        break;
                    case 23:
                        cursorIndex = 7;
                        break;
                }
            }
        }

        if (direction.y < 0)
        {
            if (cursorIndex < 20)
            {
                if (cursorIndex + 4 < inventorySlots.Length)
                {
                    cursorIndex += 4;
                }
                else
                {
                    switch (cursorIndex)
                    {
                        case 16:
                            cursorIndex = 0;
                            break;
                        case 17:
                            cursorIndex = 1;
                            break;
                        case 18:
                            cursorIndex = 2;
                            break;
                        case 19:
                            cursorIndex = 3;
                            break;
                    }
                }
            }
            else
            {
                if (cursorIndex - 3 < 20)
                {
                    cursorIndex += 3;
                }
                else
                {
                    cursorIndex -= 3;
                }
            }
        }
        else if (direction.y > 0)
        {
            if (cursorIndex < 20)
            {


                if (cursorIndex - 4 > -1)
                {
                    cursorIndex -= 4;
                }
                else
                {
                    switch (cursorIndex)
                    {
                        case 0:
                            cursorIndex = 16;
                            break;
                        case 1:
                            cursorIndex = 17;
                            break;
                        case 2:
                            cursorIndex = 18;
                            break;
                        case 3:
                            cursorIndex = 19;
                            break;
                    }
                }
            }
            else
            {
                if (cursorIndex + 3 < 25)
                {
                    cursorIndex += 3;
                }
                else
                {
                    cursorIndex -= 3;
                }
            }
        }
        MoveCursor(cursorIndex);
    }
    public void DisplayItems()
    {
        int[][] equippedItemIndices = BeastManager.Instance.m_GearIndices;
        string id = m_BuildingMaterial.id;
        int underscoreIndex = id.LastIndexOf('_');
        // The state data starts just after the underscore, hence +1.
        // The length of the state data is the length of the id string minus the starting index of the state data.
        string state = id.Substring(underscoreIndex + 1, id.Length - underscoreIndex - 1);
        int[] _equippedItemIndices = new int[7] { -1, -1, -1, -1, -1, -1, -1 };
        int counter = 0;

        for (int i = 0; i < 7; i++)
        {
            int beastGearListIndex = -1;
            for (int j = 0; j < equippedItemIndices[i].Length; j++)
            {
                if (equippedItemIndices[i][j] != -1)
                {
                    beastGearListIndex = equippedItemIndices[i][j];
                    break;
                }
            }

            if (BeastManager.Instance.m_BlockedGearSlots.Contains(i))
            {
                equippedItemSlots[i].beastGearStack = new();
                equippedItemSlots[i].spriteRenderer.sprite = GetEquipmentIcon(i);
                equippedItemSlots[i].isOccupied = false;
            }
            else if (beastGearListIndex != -1)
            {
                equippedItemSlots[i].beastGearStack = new(ItemManager.Instance.beastGearList[beastGearListIndex].GetComponent<BeastGear>(), beastGearListIndex, false);
                equippedItemSlots[i].spriteRenderer.sprite = equippedItemSlots[i].beastGearStack.beastGear.icon;
                equippedItemSlots[i].isOccupied = true;
                _equippedItemIndices[counter] = beastGearListIndex;
                counter++;
            }
            else
            {
                equippedItemSlots[i].beastGearStack = new();
                equippedItemSlots[i].spriteRenderer.sprite = GetEquipmentIcon(i);
                equippedItemSlots[i].isOccupied = false;
            }
            UpdateAvailableSlots();
        }
        int[][] itemsArray;
        try
        {
            itemsArray = JsonConvert.DeserializeObject<int[][]>(state);
        }
        catch (JsonException ex)
        {
            return; // Exit the method if deserialization fails
        }

        if (itemsArray == null) return;
        for (int i = 0; i < itemsArray.Length; i++)
        {
            if (!_equippedItemIndices.Contains(itemsArray[i][0]))
            {
                SpriteRenderer sr = inventorySlots[i].spriteRenderer;
                BeastGearStack stack = inventorySlots[i].beastGearStack;
                stack.beastGear = ItemManager.Instance.GetBeastGearByIndex(itemsArray[i][0]).GetComponent<BeastGear>();
                sr.sprite = stack.beastGear.icon;
                stack.count = itemsArray[i][1];
                stack.isEmpty = false;
                inventorySlots[i].isOccupied = true;
            }
        }

        levelText.text = "Level " + LevelManager.Instance.beastLevel.ToString();
        string whichBeast = LevelManager.Instance.beastLevel switch
        {
            1 => "Mamut the Bull",
            2 => "Mamut the Beast",
            _ => "Mamut the Calf",
        };
        nameText.text = whichBeast;
    }
    public string AddItem(BeastGear itemToAdd)
    {
        string id = m_BuildingMaterial.id;
        int underscoreIndex = id.LastIndexOf('_');
        // The state data starts just after the underscore, hence +1.
        // The length of the state data is the length of the id string minus the starting index of the state data.
        string state = id.Substring(underscoreIndex + 1, id.Length - underscoreIndex - 1);
        int[][] itemsArray = JsonConvert.DeserializeObject<int[][]>(state);
        List<int[]> itemsList = new List<int[]>();
        int gearIndex = -1;
        for (int i = 0; i < itemToAdd.gearItemIndices.Length; i++)
        {
            if (itemToAdd.gearItemIndices[i] != -1)
            {
                gearIndex = itemToAdd.gearItemIndices[i];
                break;
            }
        }
        if (itemsArray != null && itemsArray.Length >= 9) return "Stable storage is full. No more items can be added.";

        if (itemsArray != null && itemsArray.Length > 0)
        {
            itemsList = new List<int[]>(itemsArray);
        }
        int chests = 0;
        foreach (int[] item in itemsList)
        {

            if (item[0] == gearIndex)
            {
                if (itemToAdd.gearName == "Storage Chest")
                {
                    chests++;
                    if (chests > 2)
                    {
                        return "The max number for this item has already been crafted";
                    }
                }
                else
                {
                    return "This item has already been crafted";
                }
            }
        }
        // Convert array to List for easy manipulation

        // Assuming new item is an int array (e.g., new int[] { 5, 10 })
        // You can modify this part to get the actual item data you need to add

        int[] newItem = new int[] { gearIndex, 1 };

        // Add the new item
        itemsList.Add(newItem);

        // Convert back to array if necessary
        itemsArray = itemsList.ToArray();
        // Serialize back to string
        string newState = JsonConvert.SerializeObject(itemsArray);
        SaveChestState(newState);

        return $"New item has been added to the Stable Storage!";
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
            playerCurrentlyUsing = actor;
            ActorEquipment ac = actor.GetComponent<ActorEquipment>();
            items = ac.inventoryManager.items;
            DisplayItems();
            ac.GetComponent<ThirdPersonUserControl>().craftingBenchUI = true;
            playerPrefix = playerCurrentlyUsing.GetComponent<ThirdPersonUserControl>().playerPrefix;
            transform.GetChild(0).gameObject.SetActive(true);
            isOpen = true;
        }
    }

    public void UpdateInfoPanel(string name, string description, int level, int[] blockedSlots)
    {
        infoPanel.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = name;
        infoPanel.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = description;
        infoPanel.transform.GetChild(1).GetChild(2).GetComponent<TextMeshProUGUI>().text = level != -1 ? $"Lvl: {level + 1}" : "";
        string blockedSlotsString = "";
        if (blockedSlots != null)
        {
            for (int i = 0; i < blockedSlots.Length; i++)
            {
                if (blockedSlots[i] != -1)
                {
                    blockedSlotsString += m_gearSlotNames[blockedSlots[i]] + ",  ";
                    if (i == blockedSlots.Length - 2 && i != 0)
                    {
                        blockedSlotsString += "and ";
                    }
                }
            }
        }
        infoPanel.transform.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text = blockedSlotsString != "" && blockedSlots.Length > 0 ? $"Blocks: " + blockedSlotsString
        + " slots" : "";
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
