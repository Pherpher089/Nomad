using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChestController : MonoBehaviour
{
    [HideInInspector] public Sprite m_InventorySlotSprite;
    //The UI GameObject
    [HideInInspector] public bool m_IsOpen = false;
    [HideInInspector] public GameObject m_PlayerCurrentlyUsing = null;
    [HideInInspector] public CraftingSlot[] m_StorageSlots;
    [HideInInspector] public CraftingSlot[] m_InventorySlots;
    [HideInInspector] public CraftingSlot[] m_Slots;

    [HideInInspector] public CraftingSlot[] m_ToolBeltSlots;
    GameObject m_CursorObject;
    CraftingSlot m_CursorSlot;
    GameObject m_MouseCursorObject;
    CraftingSlot m_MouseCursorSlot;
    string m_CurrentPlayerPrefix;
    bool M_UiReturn = false;                         //Tracks the return of the input axis because they are not boolean input
    int m_CursorIndex = 0;
    [HideInInspector] public ItemStack[] m_Items;
    [HideInInspector] public ItemStack[] m_BeltItems;
    GameObject m_InfoPanel;
    BuildingObject m_BuildingObject;
    [HideInInspector]
    public BuildingMaterial m_BuildingMaterial;
    [HideInInspector] public bool inUse = false;
    GameObject[] buttonPrompts;
    LootGenerator lootGenerator = null;
    ItemStack[] loot;
    public bool isLootChest = false;
    void Awake()
    {
        m_BuildingMaterial = GetComponent<BuildingMaterial>();
        if (isLootChest)
        {
            m_BuildingMaterial.id = GenerateObjectId.GenerateItemId(m_BuildingMaterial);
            lootGenerator = GetComponent<LootGenerator>();
            loot = lootGenerator.GenerateLoot();
            string lootState = lootGenerator.GenerateLootState(loot);
            LevelManager.Instance.CallSaveObjectsPRC(m_BuildingMaterial.id, false, lootState);

        }
    }
    void Start()
    {
        m_BuildingObject = GetComponent<BuildingObject>();
        Initialize();
    }
    void Initialize()
    {
        m_StorageSlots = new CraftingSlot[9];
        m_InventorySlots = new CraftingSlot[9];
        m_ToolBeltSlots = new CraftingSlot[4];
        m_Slots = new CraftingSlot[22];
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
            m_StorageSlots[counter + 1].quantText.text = "";
            m_StorageSlots[counter + 1].spriteRenderer.sprite = null;
            m_StorageSlots[counter + 2] = transform.GetChild(0).GetChild(i + 2).GetComponent<CraftingSlot>();
            m_StorageSlots[counter + 2].currentItemStack = new ItemStack(null, 0, -1, true);
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

        for (int i = 18; i < 22; i++)
        {
            m_ToolBeltSlots[i - 18] = transform.GetChild(0).GetChild(i).GetComponent<CraftingSlot>();
        }

        int inventoryCounter = 0;
        int craftingCounter = 0;
        int toolBeltCounter = 0;
        for (int i = 0; i < 22; i++)
        {
            if (i < 3 || i > 5 && i < 9 || i > 11 && i < 15)
            {
                m_Slots[i] = m_InventorySlots[inventoryCounter];
                inventoryCounter++;
            }
            else if (i > 17)
            {
                m_Slots[i] = m_ToolBeltSlots[toolBeltCounter];
                toolBeltCounter++;
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
        m_CursorObject = transform.GetChild(0).GetChild(22).gameObject;
        m_CursorSlot = m_CursorObject.GetComponent<CraftingSlot>();
        m_CursorSlot.currentItemStack = new ItemStack();
        m_CursorSlot.isOccupied = false;
        m_CursorSlot.spriteRenderer.sprite = null;
        m_CursorSlot.quantText.text = "";
        m_MouseCursorObject = transform.GetChild(0).GetChild(23).gameObject;
        m_MouseCursorSlot = m_MouseCursorObject.GetComponent<CraftingSlot>();
        m_MouseCursorSlot.currentItemStack = new ItemStack();
        m_MouseCursorSlot.isOccupied = false;
        m_MouseCursorSlot.spriteRenderer.sprite = null;
        m_MouseCursorSlot.quantText.text = "";
        m_InfoPanel = transform.GetChild(0).GetChild(24).gameObject;
        transform.GetChild(0).gameObject.SetActive(false);
        m_IsOpen = false;
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
                if (!m_CursorSlot.currentItemStack.isEmpty)
                {
                    m_MouseCursorSlot.currentItemStack = new(m_CursorSlot.currentItemStack);
                    m_CursorSlot.currentItemStack = new();
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
                    m_MouseCursorSlot.quantText.text = m_MouseCursorSlot.currentItemStack.count.ToString();
                    m_PlayerCurrentlyUsing.GetComponent<PlayerInventoryManager>().SpendItem(m_MouseCursorSlot.currentItemStack.item);
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
                    m_PlayerCurrentlyUsing.GetComponent<PlayerInventoryManager>().SpendItem(m_MouseCursorSlot.currentItemStack.item, m_MouseCursorSlot.currentItemStack.count);

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
            m_CursorIndex = slotIndex;
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
            if (m_PlayerCurrentlyUsing.GetComponent<ThirdPersonUserControl>().playerPrefix == "sp")
            {
                m_MouseCursorObject.SetActive(true);
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
        if (m_PlayerCurrentlyUsing.GetComponent<ThirdPersonUserControl>().playerPrefix == "sp")
        {
            if (!m_MouseCursorSlot.isOccupied)
            {
                m_CursorSlot.currentItemStack = new(m_MouseCursorSlot.currentItemStack);
                m_MouseCursorSlot.currentItemStack = new();
                DisplayItems();
            }
        };
        if (direction.x > 0)
        {
            if (m_CursorIndex + 1 < m_Slots.Length && m_CursorIndex != 5 && m_CursorIndex != 11 && m_CursorIndex != 17)
            {
                m_CursorIndex += 1;
            }
            else if (m_CursorIndex == 5)
            {
                m_CursorIndex = 0;
            }
            else if (m_CursorIndex == 11)
            {
                m_CursorIndex = 6;
            }
            else if (m_CursorIndex == 17)
            {
                m_CursorIndex = 12;
            }
        }
        else if (direction.x < 0)
        {
            if (m_CursorIndex - 1 > -1 && m_CursorIndex != 0 && m_CursorIndex != 6 && m_CursorIndex != 12)
            {
                m_CursorIndex -= 1;
            }
            else if (m_CursorIndex == 0)
            {
                m_CursorIndex = 5;
            }
            else if (m_CursorIndex == 6)
            {
                m_CursorIndex = 11;
            }
            else if (m_CursorIndex == 12)
            {
                m_CursorIndex = 17;
            }
        }

        if (direction.y < 0) // down
        {
            if (m_CursorIndex == 12 || m_CursorIndex == 13)
            {
                m_CursorIndex = 18;
            }
            else if (m_CursorIndex == 14)
            {
                m_CursorIndex = 19;
            }
            else if (m_CursorIndex == 18)
            {
                m_CursorIndex = 20;
            }
            else if (m_CursorIndex == 19)
            {
                m_CursorIndex = 21;
            }
            else if (m_CursorIndex == 20)
            {
                m_CursorIndex = 0;
            }
            else if (m_CursorIndex == 21)
            {
                m_CursorIndex = 1;
            }
            else if (m_CursorIndex + 6 < m_Slots.Length)
            {
                m_CursorIndex += 6;
            }
        }
        else if (direction.y > 0) // up
        {

            if (m_CursorIndex == 0 || m_CursorIndex == 1)
            {
                m_CursorIndex = 20;
            }
            else if (m_CursorIndex == 2)
            {
                m_CursorIndex = 21;
            }
            else if (m_CursorIndex == 20)
            {
                m_CursorIndex = 18;
            }
            else if (m_CursorIndex == 21)
            {
                m_CursorIndex = 19;
            }
            else if (m_CursorIndex == 18)
            {
                m_CursorIndex = 12;
            }
            else if (m_CursorIndex == 19)
            {
                m_CursorIndex = 13;
            }
            else if (m_CursorIndex - 6 > -1)
            {
                m_CursorIndex -= 6;
            }
        }

        MoveCursor(m_CursorIndex);
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

    bool SelectItem(bool stack, bool isMouse = false)
    {

        if (isMouse)
        {
            if (m_CursorIndex < m_Slots.Length && m_Slots[m_CursorIndex].isOccupied)
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
                        oldStack = new ItemStack(null, 0, ConvertToInventoryIndex(m_CursorIndex), true);
                    }
                    SetSelectedItemStack(m_Slots[m_CursorIndex].currentItemStack, true, true);
                    m_Slots[m_CursorIndex].currentItemStack = new ItemStack(oldStack);
                    m_Slots[m_CursorIndex].isOccupied = !oldStack.isEmpty;
                    m_Slots[m_CursorIndex].spriteRenderer.sprite = !oldStack.isEmpty ? oldStack.item.icon : m_InventorySlotSprite;
                    m_Slots[m_CursorIndex].quantText.text = !oldStack.isEmpty ? oldStack.count.ToString() : "";
                }
                else
                {
                    SetSelectedItemStack(m_Slots[m_CursorIndex].currentItemStack, false, true);
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
    void PlaceSelectedItem(bool stack, bool isMouse = false)
    {
        if (isMouse)
        {
            if (stack)
            {
                if (m_Slots[m_CursorIndex].isOccupied)
                {
                    if (m_Slots[m_CursorIndex].currentItemStack.item.itemName == m_MouseCursorSlot.currentItemStack.item.itemName)
                    {
                        m_Slots[m_CursorIndex].currentItemStack.count += m_MouseCursorSlot.currentItemStack.count;
                        m_Slots[m_CursorIndex].quantText.text = m_Slots[m_CursorIndex].currentItemStack.count.ToString();
                        m_Slots[m_CursorIndex].currentItemStack.index = ConvertToInventoryIndex(m_CursorIndex);
                        m_MouseCursorSlot.currentItemStack = new ItemStack(null, 0, -1, true);
                        m_MouseCursorSlot.spriteRenderer.sprite = null;
                        m_MouseCursorSlot.quantText.text = "";
                        m_MouseCursorSlot.isOccupied = false;
                    }
                    else
                    {
                        ItemStack oldStack = new ItemStack(m_Slots[m_CursorIndex].currentItemStack);
                        m_Slots[m_CursorIndex].currentItemStack = new ItemStack(m_MouseCursorSlot.currentItemStack);
                        m_Slots[m_CursorIndex].spriteRenderer.sprite = m_MouseCursorSlot.currentItemStack.item.icon;
                        m_Slots[m_CursorIndex].currentItemStack.index = ConvertToInventoryIndex(m_CursorIndex);
                        m_Slots[m_CursorIndex].quantText.text = m_MouseCursorSlot.currentItemStack.count.ToString();
                        SetSelectedItemStack(oldStack, true, true);
                    }
                }
                else
                {
                    m_Slots[m_CursorIndex].currentItemStack = new ItemStack(m_MouseCursorSlot.currentItemStack);
                    m_Slots[m_CursorIndex].spriteRenderer.sprite = m_MouseCursorSlot.currentItemStack.item.icon;
                    m_Slots[m_CursorIndex].quantText.text = m_MouseCursorSlot.currentItemStack.count.ToString();
                    m_Slots[m_CursorIndex].currentItemStack.index = ConvertToInventoryIndex(m_CursorIndex);
                    m_Slots[m_CursorIndex].isOccupied = true;
                    m_MouseCursorSlot.isOccupied = false;
                    m_MouseCursorSlot.currentItemStack = new ItemStack(null, 0, -1, true);
                    m_MouseCursorSlot.quantText.text = "";
                    m_MouseCursorSlot.spriteRenderer.sprite = null;
                }
            }
            else
            {
                if (m_PlayerCurrentlyUsing.GetComponent<ThirdPersonUserControl>().playerPrefix == "sp")
                {
                    if (!m_MouseCursorSlot.isOccupied)
                    {
                        m_CursorSlot.currentItemStack = new(m_MouseCursorSlot.currentItemStack);
                        m_MouseCursorSlot.currentItemStack = new();
                        DisplayItems();
                    }
                };
                if (m_Slots[m_CursorIndex].isOccupied)
                {
                    if (m_Slots[m_CursorIndex].currentItemStack.item.itemName == m_MouseCursorSlot.currentItemStack.item.itemName)
                    {
                        m_Slots[m_CursorIndex].currentItemStack.count += 1;
                        m_Slots[m_CursorIndex].quantText.text = m_Slots[m_CursorIndex].currentItemStack.count.ToString();
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
                        ItemStack oldStack = new ItemStack(m_Slots[m_CursorIndex].currentItemStack);
                        m_Slots[m_CursorIndex].currentItemStack = new ItemStack(m_MouseCursorSlot.currentItemStack);
                        m_Slots[m_CursorIndex].spriteRenderer.sprite = m_MouseCursorSlot.currentItemStack.item.icon;
                        m_Slots[m_CursorIndex].quantText.text = m_MouseCursorSlot.currentItemStack.count.ToString();
                        SetSelectedItemStack(oldStack, true, true);
                    }

                }
                else
                {
                    m_Slots[m_CursorIndex].currentItemStack = new ItemStack(m_MouseCursorSlot.currentItemStack);
                    m_Slots[m_CursorIndex].currentItemStack.count = 1;
                    m_Slots[m_CursorIndex].spriteRenderer.sprite = m_MouseCursorSlot.currentItemStack.item.icon;
                    m_Slots[m_CursorIndex].quantText.text = m_Slots[m_CursorIndex].currentItemStack.count.ToString();
                    m_Slots[m_CursorIndex].isOccupied = true;
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
        }
    }

    //This saves the chest with the items it has
    // I think this needs to be a PRC
    public void SaveChestState()
    {
        string newState = "[";
        for (int i = 3; i < m_Slots.Length; i++)
        {
            if ((i > 5 && i < 9) || (i > 11 && i < 15) || i > 17)
            {
                continue;
            }
            if (m_Slots[i].isOccupied)
            {
                newState += $"[{m_Slots[i].currentItemStack.item.itemListIndex},{m_Slots[i].currentItemStack.count}],";
            }
        }
        newState += "]";
        LevelManager.Instance.CallSaveObjectsPRC(m_BuildingMaterial.id, false, newState);
    }

    //Updates inventory with changes made in the chest UI
    public void ReconcileItems(PlayerInventoryManager actor)
    {
        ItemStack[] _items = new ItemStack[9];
        ItemStack[] _beltItems = new ItemStack[4];
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
        c = 0;
        for (int i = 18; i < m_Slots.Length; i++)
        {
            _beltItems[c] = m_Slots[i].currentItemStack;
            m_Slots[i].currentItemStack = new ItemStack(null, 0, -1, true);
            m_Slots[i].isOccupied = false;
            m_Slots[i].quantText.text = "";
            m_Slots[i].spriteRenderer.sprite = null;
            c++;
        }
        if (m_CursorSlot.currentItemStack.item != null)
        {
            bool wasAdded = false;
            for (int i = 0; i < _items.Length; i++)
            {
                if (_items[i].item.itemListIndex == m_CursorSlot.currentItemStack.item.itemListIndex)
                {
                    _items[i].count += m_CursorSlot.currentItemStack.count;
                    wasAdded = true;
                    break;
                }
            }
            if (!wasAdded)
            {
                for (int i = 0; i < _beltItems.Length; i++)
                {
                    if (_beltItems[i].item.itemListIndex == m_CursorSlot.currentItemStack.item.itemListIndex)
                    {
                        _beltItems[i].count += m_CursorSlot.currentItemStack.count;
                        wasAdded = true;
                        break;
                    }
                }
                if (!wasAdded)
                {
                    for (int i = 0; i < _items.Length; i++)
                    {
                        if (_items[i].isEmpty)
                        {
                            _items[i] = new(m_CursorSlot.currentItemStack);
                            wasAdded = true;
                            break;
                        }
                    }
                    if (!wasAdded)
                    {
                        for (int i = 0; i < m_CursorSlot.currentItemStack.count; i++)
                            PlayerInventoryManager.Instance.DropItem(m_CursorSlot.currentItemStack.item.itemListIndex, transform.position);
                    }
                }
            }
        }
        m_CursorSlot.currentItemStack = new ItemStack(null, 0, -1, true);
        m_CursorSlot.isOccupied = false;
        m_CursorSlot.quantText.text = "";
        m_CursorSlot.spriteRenderer.sprite = null;

        if (m_MouseCursorSlot.currentItemStack.item != null)
        {
            bool wasAdded = false;
            for (int i = 0; i < _items.Length; i++)
            {
                if (_items[i].item.itemListIndex == m_MouseCursorSlot.currentItemStack.item.itemListIndex)
                {
                    _items[i].count += m_MouseCursorSlot.currentItemStack.count;
                    wasAdded = true;
                    break;
                }
            }
            if (!wasAdded)
            {
                for (int i = 0; i < _beltItems.Length; i++)
                {
                    if (_beltItems[i].item.itemListIndex == m_MouseCursorSlot.currentItemStack.item.itemListIndex)
                    {
                        _beltItems[i].count += m_MouseCursorSlot.currentItemStack.count;
                        wasAdded = true;
                        break;
                    }
                }
                if (!wasAdded)
                {
                    for (int i = 0; i < _items.Length; i++)
                    {
                        if (_items[i].isEmpty)
                        {
                            _items[i] = new(m_MouseCursorSlot.currentItemStack);
                            wasAdded = true;
                            break;
                        }
                    }
                    if (!wasAdded)
                    {
                        for (int i = 0; i < m_MouseCursorSlot.currentItemStack.count; i++)
                            PlayerInventoryManager.Instance.DropItem(m_MouseCursorSlot.currentItemStack.item.itemListIndex, transform.position);
                    }
                }
            }
        }
        m_MouseCursorSlot.currentItemStack = new ItemStack(null, 0, -1, true);
        m_MouseCursorSlot.isOccupied = false;
        m_MouseCursorSlot.quantText.text = "";
        m_MouseCursorSlot.spriteRenderer.sprite = null;

        actor.items = _items;
        actor.beltItems = _beltItems;
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
            m_BeltItems = ac.inventoryManager.beltItems;
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

        itemsArray = JsonConvert.DeserializeObject<int[][]>(state);

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
        for (int i = 0; i < m_BeltItems.Length; i++)
        {
            SpriteRenderer sr = m_ToolBeltSlots[i].spriteRenderer;
            TextMeshPro tm = m_ToolBeltSlots[i].quantText;
            ItemStack stack = m_ToolBeltSlots[i].currentItemStack;
            if (!m_BeltItems[i].isEmpty)
            {
                sr.sprite = m_BeltItems[i].item.icon;
                stack.item = m_BeltItems[i].item;
                stack.count = m_BeltItems[i].count;
                stack.isEmpty = false;
                m_ToolBeltSlots[i].isOccupied = true;

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
    }

    //for creating crafting recipes in the editor
    public string ArrayToString(int[] array)
    {
        return string.Join(",", array.Select(i => i.ToString()).ToArray());
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
