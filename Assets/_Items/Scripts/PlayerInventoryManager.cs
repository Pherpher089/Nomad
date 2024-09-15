using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using System;

public class PlayerInventoryManager : MonoBehaviour
{
    public static PlayerInventoryManager Instance;
    public ItemStack[] items;
    public ItemStack[] beltItems;
    public GameObject UIRoot;
    private GameObject selectedItemSlot;
    public bool isActive = false;
    private int selectedIndex = 4;
    private int inventorySlotCount = 9;
    public Sprite inventorySlotIcon;
    private Sprite weaponInventorySlotIcon;
    private Sprite chestInventorySlotIcon;
    private Sprite legsInventorySlotIcon;
    private Sprite helmetInventorySlotIcon;
    private Sprite pipeInventorySlotIcon;
    private Sprite specialInventorySlotIcon;
    private Sprite capeInventorySlotIcon;
    private Sprite utilityInventorySlotIcon;
    private Sprite craftingSlotIcon;
    public Sprite selectedItemIcon;
    private List<int> currentIngredients;
    private int item1Index, item2Index;
    public ActorEquipment actorEquipment;
    public int[] ingredients;
    public GameObject[] craftingSlots;
    public GameObject[] equipmentSlots;
    public GameObject[] beltSlots;
    public GameObject[] armorSlots;
    public GameObject[] itemSlots;
    public GameObject infoPanel;
    public GameObject quickStatsPanel;
    private int craftingItemCount = 0;
    private CraftingManager craftingManager;
    public bool isCrafting;
    private ItemManager m_ItemManager;
    private CharacterManager m_CharacterManager;
    private GameObject[] craftingProduct;
    PlayersManager playersManager;
    public GameObject[] buttonPrompts;
    GameObject cursor;
    ItemStack cursorStack;

    GameObject mouseCursor;
    ItemStack mouseCursorStack;
    ActorAudioManager audioManager;
    public int selectedBeltItem = -1;
    ThirdPersonUserControl thirdPersonUserControl;
    bool mouseLastUsed = false;
    void Awake()
    {
        if (SceneManager.GetActiveScene().name.Contains("LoadingScene")) return;

        Instance = this;
        craftingProduct = null;
        playersManager = FindObjectOfType<PlayersManager>();
        craftingSlots = new GameObject[5];
        equipmentSlots = new GameObject[2];
        beltSlots = new GameObject[4];
        armorSlots = new GameObject[3];
        itemSlots = new GameObject[4];
        currentIngredients = new List<int>();
        inventorySlotIcon = Resources.Load<Sprite>("Sprites/InventorySlot");
        weaponInventorySlotIcon = Resources.Load<Sprite>("Sprites/InventorySlotWeapon");
        chestInventorySlotIcon = Resources.Load<Sprite>("Sprites/InventorySlotChestArmor");
        legsInventorySlotIcon = Resources.Load<Sprite>("Sprites/InventorySlotLegArmor");
        helmetInventorySlotIcon = Resources.Load<Sprite>("Sprites/InventorySlotHelmet");
        pipeInventorySlotIcon = Resources.Load<Sprite>("Sprites/PipeSlotIcon");
        specialInventorySlotIcon = Resources.Load<Sprite>("Sprites/SpecialSlotIcon");
        capeInventorySlotIcon = Resources.Load<Sprite>("Sprites/CapeSlotIcon");
        utilityInventorySlotIcon = Resources.Load<Sprite>("Sprites/UtilitySlotIcon");
        craftingSlotIcon = Resources.Load<Sprite>("Sprites/CraftingSlotIcon");
        selectedItemIcon = Resources.Load<Sprite>("Sprites/SelectedInventorySlot");
        actorEquipment = GetComponent<ActorEquipment>();
        UIRoot = transform.GetChild(1).gameObject;
        items = new ItemStack[9];
        beltItems = new ItemStack[4];
        craftingManager = GameObject.FindWithTag("GameController").GetComponent<CraftingManager>();
        m_CharacterManager = GetComponent<CharacterManager>();
        audioManager = GetComponent<ActorAudioManager>();
        thirdPersonUserControl = GetComponent<ThirdPersonUserControl>();

        //Initialize items object
        for (int i = 0; i < items.Length; i++)
        {
            items[i] = new ItemStack(null, 1, i, true);
            if (i < 4)
            {
                beltItems[i] = new ItemStack(null, 1, i, true);
            }
        }
        // Get Crafting Slots
        for (int i = 0; i < 5; i++)
        {
            craftingSlots[i] = UIRoot.transform.GetChild(22 + i).gameObject;
            craftingSlots[i].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = craftingSlotIcon;
            if (i == 4)
            {
                craftingSlots[i].SetActive(false);
            }
        }
        for (int i = 0; i < 2; i++)
        {
            equipmentSlots[i] = UIRoot.transform.GetChild(13 + i).gameObject;
        }
        for (int i = 0; i < 4; i++)
        {
            beltSlots[i] = UIRoot.transform.GetChild(9 + i).gameObject;
        }
        infoPanel = UIRoot.transform.GetChild(UIRoot.transform.childCount - 3).gameObject;
        quickStatsPanel = UIRoot.transform.GetChild(UIRoot.transform.childCount - 2).gameObject;

        for (int i = 0; i < 3; i++)
        {
            armorSlots[i] = UIRoot.transform.GetChild(15 + i).gameObject;
        }
        for (int i = 0; i < 4; i++)
        {
            itemSlots[i] = UIRoot.transform.GetChild(18 + i).gameObject;
        }
        mouseCursor = UIRoot.transform.GetChild(UIRoot.transform.childCount - 5).gameObject;
        cursor = UIRoot.transform.GetChild(UIRoot.transform.childCount - 4).gameObject;
        cursorStack = new ItemStack();
        mouseCursorStack = new ItemStack();
        m_ItemManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<ItemManager>();
        SetSelectedItem(5);
        UpdateButtonPrompts();
        UIRoot.SetActive(false);
    }
    void Start()
    {
        UpdateQuickStats();
    }
    void Update()
    {
        if (isActive) UpdateQuickStats();
        if (thirdPersonUserControl.playerPrefix == "sp" && isActive)
        {
            if (mouseCursor.activeSelf == false)
            {
                mouseCursor.SetActive(true);
            }
            HandleMouseInput();
        }
        else
        {
            mouseCursor.SetActive(false);
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

            GameObject clickedSlot = hit.collider.gameObject;
            // Check if the clicked object is an InventorySlot
            if (clickedSlot.CompareTag("InventorySlot"))
            {
                if (!cursorStack.isEmpty)
                {
                    mouseCursorStack = new(cursorStack);
                    cursorStack = new();
                }
                InventoryActionMouse(clickedSlot);
            }
        }
        else
        {
            InventoryActionMouse(null);
        }

        Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        mouseCursor.transform.position = _ray.GetPoint(Vector3.Distance(Camera.main.transform.position, UIRoot.transform.position)); // Move cursor to the point where the ray hits the plane
    }


    public void UpdateButtonPrompts()
    {
        if (!GameStateManager.Instance.showOnScreenControls)
        {
            int buttonPromptChildCount = UIRoot.transform.GetChild(UIRoot.transform.childCount - 1).GetChild(0).childCount;
            for (int i = 0; i < buttonPromptChildCount; i++)
            {
                UIRoot.transform.GetChild(UIRoot.transform.childCount - 1).GetChild(1).GetChild(i).gameObject.SetActive(false);
                UIRoot.transform.GetChild(UIRoot.transform.childCount - 1).GetChild(0).GetChild(i).gameObject.SetActive(false);

            }
            return;

        }
        if (!LevelPrep.Instance.firstPlayerGamePad)
        {
            int buttonPromptChildCount = UIRoot.transform.GetChild(UIRoot.transform.childCount - 1).GetChild(1).childCount;
            buttonPrompts = new GameObject[buttonPromptChildCount];
            for (int i = 0; i < buttonPromptChildCount; i++)
            {
                UIRoot.transform.GetChild(UIRoot.transform.childCount - 1).GetChild(1).GetChild(i).gameObject.SetActive(true);
                buttonPrompts[i] = UIRoot.transform.GetChild(UIRoot.transform.childCount - 1).GetChild(1).GetChild(i).gameObject;

            }
            for (int i = 0; i < buttonPromptChildCount; i++)
            {
                UIRoot.transform.GetChild(UIRoot.transform.childCount - 1).GetChild(0).GetChild(i).gameObject.SetActive(false);
            }
        }
        else
        {
            int buttonPromptChildCount = UIRoot.transform.GetChild(UIRoot.transform.childCount - 1).GetChild(0).childCount;
            buttonPrompts = new GameObject[buttonPromptChildCount];
            for (int i = 0; i < buttonPromptChildCount; i++)
            {
                UIRoot.transform.GetChild(UIRoot.transform.childCount - 1).GetChild(0).GetChild(i).gameObject.SetActive(true);
                buttonPrompts[i] = UIRoot.transform.GetChild(UIRoot.transform.childCount - 1).GetChild(0).GetChild(i).gameObject;
            }
            for (int i = 0; i < buttonPromptChildCount; i++)
            {
                UIRoot.transform.GetChild(UIRoot.transform.childCount - 1).GetChild(1).GetChild(i).gameObject.SetActive(false);
            }
        }
        AdjustButtonPrompts();
    }
    public void RemoveIngredient(int index, bool mouseCursor)
    {
        int ingredientItemIndex = currentIngredients[index];
        currentIngredients[index] = -1;
        craftingSlots[index].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = craftingSlotIcon;
        Item removedItem = ItemManager.Instance.GetItemGameObjectByItemIndex(ingredientItemIndex).GetComponent<Item>();
        if (mouseCursor)
        {
            mouseCursorStack = new(removedItem, 1, index, false);
        }
        else
        {
            cursorStack = new(removedItem, 1, index, false);
        }

        bool allEmpty = true;
        foreach (int _idx in currentIngredients)
        {
            if (_idx != -1)
            {
                allEmpty = false;
            }
        }
        if (allEmpty) CancelCraft();
        CheckCraft();
        DisplayItems();
    }
    public void SwapIngredient(int index, bool mouseCursor)
    {
        // if (craftingSlots[index].transform.GetChild(1).GetComponent<SpriteRenderer>().color.a != 1 && currentIngredients.Count - 1 < index)
        // {
        //     return;
        // }
        // else
        // {

        // }
    }
    public void AddIngredient(int index, bool mouseCursor)
    {
        isCrafting = true;
        if (currentIngredients.Count - 1 < index)
        {
            currentIngredients.Add(m_ItemManager.GetItemIndex(mouseCursor ? mouseCursorStack.item : cursorStack.item));
        }
        else
        {
            currentIngredients[index] = m_ItemManager.GetItemIndex(mouseCursor ? mouseCursorStack.item : cursorStack.item);
        }

        craftingSlots[index].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = mouseCursor ? mouseCursorStack.item.icon : cursorStack.item.icon;
        if (mouseCursor)
        {
            mouseCursorStack.count--;
            if (mouseCursorStack.count <= 0)
            {
                mouseCursorStack = new();
            }
        }
        else
        {
            cursorStack.count--;
            if (cursorStack.count <= 0)
            {
                cursorStack = new();
            }
        }
        CheckCraft();
        DisplayItems();
    }
    public void AddIngredient()
    {
        if (selectedIndex < 9)
        {

            if (items[selectedIndex].isEmpty || craftingItemCount > 4)
            {
                if (craftingItemCount > 4)
                {
                    CancelCraft();
                }
                return;
            }
            if (currentIngredients.Count < 4)
            {
                isCrafting = true;
                craftingItemCount++;
                currentIngredients.Add(m_ItemManager.GetItemIndex(items[selectedIndex].item));
                craftingSlots[currentIngredients.Count - 1].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = items[selectedIndex].item.icon;

                RemoveItem(selectedIndex, 1);

            }
        }
        else if (selectedIndex < 13)
        {

            if (beltItems[selectedIndex - 9].isEmpty || craftingItemCount > 4)
            {
                if (craftingItemCount > 4)
                {
                    CancelCraft();
                }
                return;
            }
            if (currentIngredients.Count < 4)
            {
                isCrafting = true;
                craftingItemCount++;
                currentIngredients.Add(m_ItemManager.GetItemIndex(beltItems[selectedIndex - 9].item));

                craftingSlots[currentIngredients.Count - 1].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = beltItems[selectedIndex - 9].item.icon;
                RemoveBeltItem(selectedIndex - 9, 1);

            }
        }
        else
        {
            return;
        }
        CheckCraft();
    }

    private void CheckCraft()
    {
        ingredients = new int[currentIngredients.Count];
        int c = 0;
        foreach (int index in currentIngredients)
        {
            ingredients[c] = index;
            c++;
        }
        GameObject[] product = craftingManager.TryCraft(ingredients);

        if (product != null)
        {
            craftingProduct = product;
            craftingSlots[4].SetActive(true);
            craftingSlots[4].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = product[0].GetComponent<Item>().icon;
            AdjustButtonPrompts();
        }
        else
        {
            craftingSlots[4].SetActive(false);
            craftingProduct = null;
        }
        m_CharacterManager.SaveCharacter();
    }

    public bool Craft(bool cursorPickup = false, bool isMouse = false)
    {
        if (craftingProduct != null)
        {
            BuildingMaterial buildMat = craftingProduct[0].GetComponent<BuildingMaterial>();
            if (buildMat != null && !buildMat.fitsInBackpack)
            {
                ToggleInventoryUI();
                CameraControllerPerspective.Instance.SetCameraForBuild();
                GetComponent<BuilderManager>().Build(GetComponent<ThirdPersonUserControl>(), buildMat);
            }
            else if (!cursorPickup)
            {
                //we are checking to see if an auto build object was crafted like a crafting bench or camp fire;
                bool wasItemAdded = AddItem(craftingProduct[0].GetComponent<Item>(), craftingProduct.Length);
                if (!wasItemAdded)
                {
                    DropItem(craftingProduct[0].GetComponent<Item>().itemListIndex, transform.position + Vector3.up * 2);
                }
            }
            else
            {
                if (isMouse)
                {
                    if (mouseCursorStack.isEmpty)
                    {
                        mouseCursorStack = new(craftingProduct[0].GetComponent<Item>(), craftingProduct.Length, -1, false);
                    }
                    else
                    {
                        Craft(false);
                    }
                }
                else
                {
                    {
                        if (cursorStack.isEmpty)
                        {
                            cursorStack = new(craftingProduct[0].GetComponent<Item>(), craftingProduct.Length, -1, false);
                        }
                        else
                        {
                            Craft(false);
                        }
                    }
                }
            }
            AdjustButtonPrompts();
            CancelCraft(true);
            return true;
        }
        return false;
    }
    public void CancelCraft(bool spendItems = false)
    {
        isCrafting = false;
        craftingItemCount = 0;
        if (!spendItems)
        {
            foreach (int itemIndex in currentIngredients)
            {
                if (itemIndex != -1)
                {
                    AddItem(m_ItemManager.itemList[itemIndex].GetComponent<Item>(), 1);
                }
            }
        }
        for (int i = 0; i < craftingSlots.Length; i++)
        {
            craftingSlots[i].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = craftingSlotIcon;

            if (i == 4)
            {
                craftingSlots[i].SetActive(false);
            }
        }

        currentIngredients = new List<int>();
        craftingProduct = null;
        AdjustButtonPrompts();
    }
    public void SpendItem(Item item)
    {
        int itemIndex = FindItemInInventory(item);
        if (itemIndex >= 0)
        {
            int remainingCount = RemoveItem(itemIndex, 1);
            if (remainingCount == 0 && actorEquipment.equippedItem != null && actorEquipment.equippedItem.GetComponent<Item>().inventoryIndex == item.itemListIndex)
            {
                actorEquipment.UnequippedCurrentItem(true);
            }
            m_CharacterManager.SaveCharacter();
        }
        else
        {
            actorEquipment.UnequippedCurrentItem(true);
        }
    }
    public void SpendItem(Item item, int count)
    {

        int itemIndex = FindItemInInventory(item);
        if (itemIndex >= 0)
        {
            int remainingCount = RemoveItem(itemIndex, count);
            if (remainingCount == 0 && actorEquipment.equippedItem != null && actorEquipment.equippedItem.GetComponent<Item>().inventoryIndex == item.itemListIndex)
            {
                actorEquipment.UnequippedCurrentItem(true);
            }
            m_CharacterManager.SaveCharacter();
        }
        else
        {
            actorEquipment.UnequippedCurrentItem(true);
        }
    }
    private int FindItemInInventory(Item item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (!items[i].isEmpty && items[i].item.itemName == item.itemName)
            {
                return i;
            }
        }
        return -1; // Item not found
    }
    void InventoryActionMouse(GameObject clickedSlot)
    {

        if (clickedSlot == null)
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (!mouseCursorStack.isEmpty)
                {

                    DropItem(mouseCursorStack.item.itemListIndex, transform.position);
                    mouseCursorStack.count--;
                    if (mouseCursorStack.count <= 0)
                    {
                        mouseCursorStack = new ItemStack();
                    }
                    DisplayItems();
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (!mouseCursorStack.isEmpty)
                {
                    for (int i = 0; i < mouseCursorStack.count; i++)
                    {
                        DropItem(mouseCursorStack.item.itemListIndex, transform.position);
                    }
                    mouseCursorStack = new ItemStack();
                    DisplayItems();
                }
            }
            return;
        }

        int slotIndex = clickedSlot.transform.GetSiblingIndex();
        if (slotIndex > 21 && craftingSlots[slotIndex - 22].transform.GetChild(1).GetComponent<SpriteRenderer>().color.a != 1)
        {
            return;
        }
        SetSelectedItem(slotIndex);

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            if (slotIndex < 9)
            {
                if (!mouseCursorStack.isEmpty)
                {
                    if (!items[selectedIndex].isEmpty)
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            if (items[selectedIndex].item.itemListIndex == mouseCursorStack.item.itemListIndex)
                            {
                                items[selectedIndex].count += mouseCursorStack.count;
                                mouseCursorStack = new();
                            }
                            else
                            {
                                ItemStack temp = new(items[selectedIndex]);
                                items[selectedIndex] = new(mouseCursorStack);
                                mouseCursorStack = new(temp);
                            }

                        }
                        if (Input.GetMouseButtonDown(1) && items[selectedIndex].item.itemListIndex == mouseCursorStack.item.itemListIndex)
                        {
                            items[selectedIndex].count++;
                            mouseCursorStack.count--;
                            if (mouseCursorStack.count == 0)
                            {
                                mouseCursorStack = new();
                            }
                        }
                    }
                    else
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            items[selectedIndex] = new(mouseCursorStack);
                            mouseCursorStack = new ItemStack();
                        }
                        else if (Input.GetMouseButtonDown(1))
                        {
                            items[selectedIndex] = new ItemStack(mouseCursorStack.item, 1, selectedIndex, false);
                            mouseCursorStack.count--;
                            if (mouseCursorStack.count == 0)
                            {
                                mouseCursorStack = new();
                            }
                        }

                    }
                }
                else
                {
                    if (!items[selectedIndex].isEmpty)
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            mouseCursorStack = items[selectedIndex];
                            items[selectedIndex] = new ItemStack();
                        }
                        else if (Input.GetMouseButtonDown(1))
                        {
                            mouseCursorStack = new ItemStack(items[selectedIndex].item, 1, -1, false);
                            items[selectedIndex].count--;
                            if (items[selectedIndex].count == 0)
                            {
                                items[selectedIndex] = new ItemStack();
                            }
                        }
                    }
                }
            }
            if (selectedIndex > 8 && selectedIndex < 22)
            {
                if (mouseCursorStack.isEmpty)
                {
                    switch (selectedIndex)
                    {
                        case 13:
                            if (actorEquipment.hasItem)
                            {
                                Item _item = actorEquipment.equippedItem.GetComponent<Item>();
                                if (_item.isBeltItem)
                                {
                                    for (int i = 0; i < beltItems.Length; i++)
                                    {
                                        if (beltItems[i].item != null && _item.itemListIndex == beltItems[i].item.itemListIndex)
                                        {
                                            if (beltItems[i].count > 1)
                                            {
                                                beltItems[i].count--;
                                            }
                                            else
                                            {
                                                beltSlots[i].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = null;
                                                beltItems[i] = new ItemStack(null, 0, -1, true);
                                            }
                                        }
                                    }
                                }
                                mouseCursorStack = new ItemStack(_item, 1, -1, false);
                                equipmentSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = weaponInventorySlotIcon;
                                selectedBeltItem = -1;
                                actorEquipment.UnequippedCurrentItem();
                            }
                            break;
                        case 15:
                            if (actorEquipment.equippedArmor[(int)ArmorType.Helmet] != null)
                            {
                                Item _item = actorEquipment.equippedArmor[(int)ArmorType.Helmet].GetComponent<Item>();
                                if (_item.isBeltItem)
                                {
                                    for (int i = 0; i < beltItems.Length; i++)
                                    {
                                        if (beltItems[i].item != null && _item.itemListIndex == beltItems[i].item.itemListIndex)
                                        {
                                            if (beltItems[i].count > 1)
                                            {
                                                beltItems[i].count--;
                                            }
                                            else
                                            {
                                                beltSlots[i].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = null;
                                                beltItems[i] = new ItemStack(null, 0, -1, true);
                                            }
                                        }
                                    }
                                }
                                mouseCursorStack = new ItemStack(actorEquipment.equippedArmor[(int)ArmorType.Helmet].GetComponent<Item>(), 1, -1, false);
                                armorSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = helmetInventorySlotIcon;
                                actorEquipment.UnequippedCurrentArmor(ArmorType.Helmet);
                            }
                            break;
                        case 16:
                            if (actorEquipment.equippedArmor[(int)ArmorType.Chest] != null)
                            {
                                Item _item = actorEquipment.equippedArmor[(int)ArmorType.Chest].GetComponent<Item>();
                                if (_item.isBeltItem)
                                {
                                    for (int i = 0; i < beltItems.Length; i++)
                                    {
                                        if (beltItems[i].item != null && _item.itemListIndex == beltItems[i].item.itemListIndex)
                                        {
                                            if (beltItems[i].count > 1)
                                            {
                                                beltItems[i].count--;
                                            }
                                            else
                                            {
                                                beltSlots[i].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = null;
                                                beltItems[i] = new ItemStack(null, 0, -1, true);
                                            }
                                        }
                                    }
                                }
                                mouseCursorStack = new ItemStack(actorEquipment.equippedArmor[(int)ArmorType.Chest].GetComponent<Item>(), 1, -1, false);
                                armorSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = chestInventorySlotIcon;
                                actorEquipment.UnequippedCurrentArmor(ArmorType.Chest);
                            }
                            break;
                        case 17:
                            if (actorEquipment.equippedArmor[(int)ArmorType.Legs] != null)
                            {
                                Item _item = actorEquipment.equippedArmor[(int)ArmorType.Legs].GetComponent<Item>();
                                if (_item.isBeltItem)
                                {
                                    for (int i = 0; i < beltItems.Length; i++)
                                    {
                                        if (beltItems[i].item != null && _item.itemListIndex == beltItems[i].item.itemListIndex)
                                        {
                                            if (beltItems[i].count > 1)
                                            {
                                                beltItems[i].count--;
                                            }
                                            else
                                            {
                                                beltSlots[i].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = null;
                                                beltItems[i] = new ItemStack(null, 0, -1, true);
                                            }
                                        }
                                    }
                                }
                                mouseCursorStack = new ItemStack(actorEquipment.equippedArmor[(int)ArmorType.Legs].GetComponent<Item>(), 1, -1, false);
                                armorSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = legsInventorySlotIcon;
                                actorEquipment.UnequippedCurrentArmor(ArmorType.Legs);
                            }
                            break;
                        case 9:
                        case 10:
                        case 11:
                        case 12:
                            //IS CURSOR EMPTY
                            if (!beltItems[selectedIndex - 9].isEmpty)
                            {
                                if (Input.GetMouseButtonDown(0))
                                {
                                    if (actorEquipment.equippedItem != null && actorEquipment.equippedItem.GetComponent<Item>().itemListIndex == beltItems[selectedIndex - 9].item.itemListIndex)
                                    {
                                        beltItems[selectedIndex - 9].item.isBeltItem = false;
                                        equipmentSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = weaponInventorySlotIcon;
                                        selectedBeltItem = -1;
                                        actorEquipment.UnequippedCurrentItem();
                                    }
                                    else if (beltItems[selectedIndex - 9].item.TryGetComponent<Armor>(out var armor) && actorEquipment.equippedArmor[(int)armor.m_ArmorType] != null && actorEquipment.equippedArmor[(int)armor.m_ArmorType].GetComponent<Armor>().itemListIndex == armor.itemListIndex)
                                    {
                                        beltItems[selectedIndex - 9].item.isBeltItem = false;
                                        switch (armor.m_ArmorType)
                                        {
                                            case ArmorType.Helmet:
                                                armorSlots[(int)armor.m_ArmorType].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = helmetInventorySlotIcon;
                                                break;
                                            case ArmorType.Chest:
                                                armorSlots[(int)armor.m_ArmorType].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = chestInventorySlotIcon;
                                                break;
                                            case ArmorType.Legs:
                                                armorSlots[(int)armor.m_ArmorType].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = legsInventorySlotIcon;
                                                break;
                                        }
                                        actorEquipment.UnequippedCurrentArmor(armor.m_ArmorType);
                                    }

                                    mouseCursorStack = new ItemStack(beltItems[selectedIndex - 9]);
                                    beltItems[selectedIndex - 9] = new ItemStack();
                                }
                                else if (Input.GetMouseButtonDown(1))
                                {
                                    mouseCursorStack = new ItemStack(beltItems[selectedIndex - 9].item, 1, -1, false);
                                    beltItems[selectedIndex - 9].count--;
                                    if (beltItems[selectedIndex - 9].count == 0)
                                    {
                                        if (actorEquipment.equippedItem.GetComponent<Item>().itemListIndex == beltItems[selectedIndex - 9].item.itemListIndex)
                                        {
                                            beltItems[selectedIndex - 9].item.isBeltItem = false;
                                            equipmentSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = weaponInventorySlotIcon;
                                            actorEquipment.UnequippedCurrentItem();
                                        }
                                        else if (beltItems[selectedIndex - 9].item.TryGetComponent<Armor>(out var armor) && actorEquipment.equippedArmor[(int)armor.m_ArmorType] != null && actorEquipment.equippedArmor[(int)armor.m_ArmorType].GetComponent<Armor>().itemListIndex == armor.itemListIndex)
                                        {
                                            beltItems[selectedIndex - 9].item.isBeltItem = false;
                                            switch (armor.m_ArmorType)
                                            {
                                                case ArmorType.Helmet:
                                                    armorSlots[selectedIndex - 9].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = helmetInventorySlotIcon;
                                                    break;
                                                case ArmorType.Chest:
                                                    armorSlots[selectedIndex - 9].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = chestInventorySlotIcon;
                                                    break;
                                                case ArmorType.Legs:
                                                    armorSlots[selectedIndex - 9].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = legsInventorySlotIcon;
                                                    break;
                                            }
                                            actorEquipment.UnequippedCurrentArmor(armor.m_ArmorType);
                                        }
                                        beltItems[selectedIndex - 9] = new ItemStack();
                                    }
                                }
                            }
                            break;
                        case 18:
                            if (actorEquipment.equippedSpecialItems[0] != null)
                            {
                                Item _item = actorEquipment.equippedSpecialItems[0].GetComponent<Item>();
                                if (_item.isBeltItem)
                                {
                                    for (int i = 0; i < beltItems.Length; i++)
                                    {
                                        if (beltItems[i].item != null && _item.itemListIndex == beltItems[i].item.itemListIndex)
                                        {
                                            if (beltItems[i].count > 1)
                                            {
                                                beltItems[i].count--;
                                            }
                                            else
                                            {
                                                beltSlots[i].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = null;
                                                beltItems[i] = new ItemStack(null, 0, -1, true);
                                            }
                                        }
                                    }
                                }
                                mouseCursorStack = new ItemStack(_item, 1, -1, false);
                                itemSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = capeInventorySlotIcon;
                                selectedBeltItem = -1;
                                actorEquipment.UnequippedCurrentSpecialItem(0);
                            }
                            break;
                        case 19:
                            if (actorEquipment.equippedSpecialItems[1] != null)
                            {
                                Item _item = actorEquipment.equippedSpecialItems[1].GetComponent<Item>();
                                if (_item.isBeltItem)
                                {
                                    for (int i = 0; i < beltItems.Length; i++)
                                    {
                                        if (beltItems[i].item != null && _item.itemListIndex == beltItems[i].item.itemListIndex)
                                        {
                                            if (beltItems[i].count > 1)
                                            {
                                                beltItems[i].count--;
                                            }
                                            else
                                            {
                                                beltSlots[i].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = null;
                                                beltItems[i] = new ItemStack(null, 0, -1, true);
                                            }
                                        }
                                    }
                                }
                                mouseCursorStack = new ItemStack(_item, 1, -1, false);
                                itemSlots[1].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = utilityInventorySlotIcon;
                                selectedBeltItem = -1;
                                actorEquipment.UnequippedCurrentSpecialItem(1);
                            }
                            break;
                        case 20:
                            if (actorEquipment.equippedSpecialItems[2] != null)
                            {
                                Item _item = actorEquipment.equippedSpecialItems[2].GetComponent<Item>();
                                if (_item.isBeltItem)
                                {
                                    for (int i = 0; i < beltItems.Length; i++)
                                    {
                                        if (beltItems[i].item != null && _item.itemListIndex == beltItems[i].item.itemListIndex)
                                        {
                                            if (beltItems[i].count > 1)
                                            {
                                                beltItems[i].count--;
                                            }
                                            else
                                            {
                                                beltSlots[i].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = null;
                                                beltItems[i] = new ItemStack(null, 0, -1, true);
                                            }
                                        }
                                    }
                                }
                                mouseCursorStack = new ItemStack(_item, 1, -1, false);
                                itemSlots[2].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = pipeInventorySlotIcon;
                                selectedBeltItem = -1;
                                actorEquipment.UnequippedCurrentSpecialItem(2);
                            }
                            break;
                        case 21:
                            if (actorEquipment.equippedSpecialItems[3] != null)
                            {
                                Item _item = actorEquipment.equippedSpecialItems[3].GetComponent<Item>();
                                if (_item.isBeltItem)
                                {
                                    for (int i = 0; i < beltItems.Length; i++)
                                    {
                                        if (beltItems[i].item != null && _item.itemListIndex == beltItems[i].item.itemListIndex)
                                        {
                                            if (beltItems[i].count > 1)
                                            {
                                                beltItems[i].count--;
                                            }
                                            else
                                            {
                                                beltSlots[i].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = null;
                                                beltItems[i] = new ItemStack(null, 0, -1, true);
                                            }
                                        }
                                    }
                                }
                                mouseCursorStack = new ItemStack(_item, 1, -1, false);
                                itemSlots[3].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = specialInventorySlotIcon;
                                selectedBeltItem = -1;
                                actorEquipment.UnequippedCurrentSpecialItem(3);
                            }
                            break;
                        default:
                            break;
                    }
                    AdjustButtonPrompts();
                    DisplayItems();
                    return;
                }
                else
                {
                    switch (selectedIndex)
                    {
                        case 13:
                            if (actorEquipment.equippedItem != null)
                            {
                                if (mouseCursorStack.count > 1 && mouseCursorStack.item.itemListIndex != actorEquipment.equippedItem.GetComponent<Item>().itemListIndex)
                                {
                                    TryUnequippedItem();
                                    selectedBeltItem = -1;
                                    actorEquipment.EquipItem(mouseCursorStack.item);
                                    equipmentSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = mouseCursorStack.item.icon;
                                    mouseCursorStack.count--;
                                    if (mouseCursorStack.count == 0)
                                    {
                                        mouseCursorStack = new();
                                    }
                                }
                                else if (mouseCursorStack.item.itemListIndex != actorEquipment.equippedItem.GetComponent<Item>().itemListIndex)
                                {
                                    Item temp = actorEquipment.equippedItem.GetComponent<Item>();
                                    selectedBeltItem = -1;
                                    TryUnequippedItem();
                                    actorEquipment.EquipItem(mouseCursorStack.item);
                                    equipmentSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = mouseCursorStack.item.icon;

                                    mouseCursorStack = new(temp, 1, -1, false);

                                }

                            }
                            else
                            {
                                selectedBeltItem = -1;
                                actorEquipment.EquipItem(mouseCursorStack.item);
                                equipmentSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = mouseCursorStack.item.icon;

                                mouseCursorStack.count--;
                                if (mouseCursorStack.count == 0)
                                {
                                    mouseCursorStack = new();
                                }
                            }
                            break;
                        case 15:
                            if (actorEquipment.equippedArmor[0] != null)
                            {
                                if (mouseCursorStack.item.TryGetComponent<Armor>(out var _armor))
                                {
                                    if (mouseCursorStack.count > 1)
                                    {
                                        TryUnequippedArmor(_armor.m_ArmorType);
                                        actorEquipment.EquipItem(mouseCursorStack.item);
                                        mouseCursorStack.count--;
                                    }
                                    else
                                    {

                                        Item temp = actorEquipment.equippedArmor[(int)_armor.m_ArmorType].GetComponent<Item>();
                                        actorEquipment.UnequippedCurrentArmor(_armor.m_ArmorType);
                                        actorEquipment.EquipItem(mouseCursorStack.item);
                                        mouseCursorStack = new(temp, 1, -1, false);

                                    }
                                }
                            }
                            else
                            {
                                if (mouseCursorStack.item.TryGetComponent<Armor>(out var _armor))
                                {
                                    if (mouseCursorStack.count > 1)
                                    {
                                        TryUnequippedArmor(_armor.m_ArmorType);
                                        actorEquipment.EquipItem(mouseCursorStack.item);
                                        mouseCursorStack.count--;
                                    }
                                    else
                                    {
                                        actorEquipment.EquipItem(mouseCursorStack.item);
                                        mouseCursorStack = new();
                                    }
                                }
                            }
                            break;
                        case 16:
                            if (actorEquipment.equippedArmor[1] != null)
                            {
                                if (mouseCursorStack.item.TryGetComponent<Armor>(out var _armor))
                                {
                                    if (mouseCursorStack.count > 1)
                                    {
                                        TryUnequippedArmor(_armor.m_ArmorType);
                                        actorEquipment.EquipItem(mouseCursorStack.item);
                                        mouseCursorStack.count--;
                                    }
                                    else
                                    {
                                        actorEquipment.UnequippedCurrentArmor(_armor.m_ArmorType);
                                        Item temp = actorEquipment.equippedArmor[(int)_armor.m_ArmorType].GetComponent<Item>();
                                        actorEquipment.EquipItem(mouseCursorStack.item);
                                        mouseCursorStack = new(temp, 1, -1, false);

                                    }
                                }
                            }
                            else
                            {
                                if (mouseCursorStack.item.TryGetComponent<Armor>(out var _armor))
                                {
                                    if (mouseCursorStack.count > 1)
                                    {
                                        TryUnequippedArmor(_armor.m_ArmorType);
                                        actorEquipment.EquipItem(mouseCursorStack.item);
                                        mouseCursorStack.count--;
                                    }
                                    else
                                    {
                                        actorEquipment.EquipItem(mouseCursorStack.item);
                                        mouseCursorStack = new();
                                    }
                                }
                            }
                            break;
                        case 17:
                            if (actorEquipment.equippedArmor[2] != null)
                            {
                                if (mouseCursorStack.item.TryGetComponent<Armor>(out var _armor))
                                {

                                    if (mouseCursorStack.count > 1)
                                    {
                                        TryUnequippedArmor(_armor.m_ArmorType);

                                        actorEquipment.EquipItem(mouseCursorStack.item);
                                        mouseCursorStack.count--;
                                    }
                                    else
                                    {
                                        actorEquipment.UnequippedCurrentArmor(_armor.m_ArmorType);

                                        Item temp = actorEquipment.equippedArmor[(int)_armor.m_ArmorType].GetComponent<Item>();

                                        actorEquipment.EquipItem(mouseCursorStack.item);
                                        mouseCursorStack = new(temp, 1, -1, false);

                                    }
                                }
                            }
                            else
                            {
                                if (mouseCursorStack.item.TryGetComponent<Armor>(out var _armor))
                                {
                                    if (mouseCursorStack.count > 1)
                                    {
                                        TryUnequippedArmor(_armor.m_ArmorType);

                                        actorEquipment.EquipItem(mouseCursorStack.item);
                                        mouseCursorStack.count--;
                                    }
                                    else
                                    {

                                        actorEquipment.EquipItem(mouseCursorStack.item);
                                        mouseCursorStack = new();
                                    }
                                }
                            }
                            break;
                        case 18:
                            if (!mouseCursorStack.item.TryGetComponent<Cape>(out var cape))
                            {
                                return;
                            }
                            if (actorEquipment.equippedSpecialItems[0] != null)
                            {
                                if (mouseCursorStack.count > 1 && mouseCursorStack.item.itemListIndex != actorEquipment.equippedSpecialItems[0].GetComponent<Item>().itemListIndex)
                                {
                                    TryUnequippedSpecialItem(0);
                                    selectedBeltItem = -1;
                                    actorEquipment.EquipItem(mouseCursorStack.item);
                                    itemSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = mouseCursorStack.item.icon;
                                    mouseCursorStack.count--;
                                    if (mouseCursorStack.count == 0)
                                    {
                                        mouseCursorStack = new();
                                    }
                                }
                                else if (mouseCursorStack.item.itemListIndex != actorEquipment.equippedItem.GetComponent<Item>().itemListIndex)
                                {
                                    Item temp = actorEquipment.equippedSpecialItems[0].GetComponent<Item>();
                                    selectedBeltItem = -1;
                                    TryUnequippedItem();
                                    actorEquipment.EquipItem(mouseCursorStack.item);
                                    itemSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = mouseCursorStack.item.icon;

                                    mouseCursorStack = new(temp, 1, -1, false);

                                }

                            }
                            else
                            {
                                selectedBeltItem = -1;
                                actorEquipment.EquipItem(mouseCursorStack.item);
                                itemSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = mouseCursorStack.item.icon;

                                mouseCursorStack.count--;
                                if (mouseCursorStack.count == 0)
                                {
                                    mouseCursorStack = new();
                                }
                            }
                            break;
                        case 19:
                            if (!mouseCursorStack.item.TryGetComponent<UtilityItem>(out var utilItem))
                            {
                                return;
                            }
                            if (actorEquipment.equippedSpecialItems[1] != null)
                            {
                                if (mouseCursorStack.count > 1 && mouseCursorStack.item.itemListIndex != actorEquipment.equippedSpecialItems[1].GetComponent<Item>().itemListIndex)
                                {
                                    TryUnequippedSpecialItem(1);
                                    selectedBeltItem = -1;
                                    actorEquipment.EquipItem(mouseCursorStack.item);
                                    itemSlots[1].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = mouseCursorStack.item.icon;
                                    mouseCursorStack.count--;
                                    if (mouseCursorStack.count == 1)
                                    {
                                        mouseCursorStack = new();
                                    }
                                }
                                else if (mouseCursorStack.item.itemListIndex != actorEquipment.equippedItem.GetComponent<Item>().itemListIndex)
                                {
                                    Item temp = actorEquipment.equippedSpecialItems[1].GetComponent<Item>();
                                    selectedBeltItem = -1;
                                    TryUnequippedItem();
                                    actorEquipment.EquipItem(mouseCursorStack.item);
                                    itemSlots[1].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = mouseCursorStack.item.icon;

                                    mouseCursorStack = new(temp, 1, -1, false);

                                }

                            }
                            else
                            {
                                selectedBeltItem = -1;
                                actorEquipment.EquipItem(mouseCursorStack.item);
                                itemSlots[1].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = mouseCursorStack.item.icon;

                                mouseCursorStack.count--;
                                if (mouseCursorStack.count == 0)
                                {
                                    mouseCursorStack = new();
                                }
                            }
                            break;
                        case 20:
                            if (!mouseCursorStack.item.TryGetComponent<Pipe>(out var pipe))
                            {
                                return;
                            }
                            if (actorEquipment.equippedSpecialItems[2] != null)
                            {
                                if (mouseCursorStack.count > 1 && mouseCursorStack.item.itemListIndex != actorEquipment.equippedSpecialItems[2].GetComponent<Item>().itemListIndex)
                                {
                                    TryUnequippedSpecialItem(2);
                                    selectedBeltItem = -1;
                                    actorEquipment.EquipItem(mouseCursorStack.item);
                                    itemSlots[2].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = mouseCursorStack.item.icon;
                                    mouseCursorStack.count--;
                                    if (mouseCursorStack.count == 0)
                                    {
                                        mouseCursorStack = new();
                                    }
                                }
                                else if (mouseCursorStack.item.itemListIndex != actorEquipment.equippedItem.GetComponent<Item>().itemListIndex)
                                {
                                    Item temp = actorEquipment.equippedSpecialItems[2].GetComponent<Item>();
                                    selectedBeltItem = -1;
                                    TryUnequippedItem();
                                    actorEquipment.EquipItem(mouseCursorStack.item);
                                    itemSlots[2].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = mouseCursorStack.item.icon;

                                    mouseCursorStack = new(temp, 1, -1, false);

                                }

                            }
                            else
                            {
                                selectedBeltItem = -1;
                                actorEquipment.EquipItem(mouseCursorStack.item);
                                itemSlots[2].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = mouseCursorStack.item.icon;

                                mouseCursorStack.count--;
                                if (mouseCursorStack.count == 0)
                                {
                                    mouseCursorStack = new();
                                }
                            }
                            break;
                        case 21:
                            if (!mouseCursorStack.item.TryGetComponent<Jewelry>(out var jewelry))
                            {
                                return;
                            }
                            if (actorEquipment.equippedSpecialItems[3] != null)
                            {
                                if (mouseCursorStack.count > 1 && mouseCursorStack.item.itemListIndex != actorEquipment.equippedSpecialItems[2].GetComponent<Item>().itemListIndex)
                                {
                                    TryUnequippedSpecialItem(3);
                                    selectedBeltItem = -1;
                                    actorEquipment.EquipItem(mouseCursorStack.item);
                                    itemSlots[3].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = mouseCursorStack.item.icon;
                                    mouseCursorStack.count--;
                                    if (mouseCursorStack.count == 0)
                                    {
                                        mouseCursorStack = new();
                                    }
                                }
                                else if (mouseCursorStack.item.itemListIndex != actorEquipment.equippedItem.GetComponent<Item>().itemListIndex)
                                {
                                    Item temp = actorEquipment.equippedSpecialItems[2].GetComponent<Item>();
                                    selectedBeltItem = -1;
                                    TryUnequippedItem();
                                    actorEquipment.EquipItem(mouseCursorStack.item);
                                    itemSlots[2].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = mouseCursorStack.item.icon;

                                    mouseCursorStack = new(temp, 1, -1, false);

                                }

                            }
                            else
                            {
                                selectedBeltItem = -1;
                                actorEquipment.EquipItem(mouseCursorStack.item);
                                itemSlots[3].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = mouseCursorStack.item.icon;

                                mouseCursorStack.count--;
                                if (mouseCursorStack.count == 0)
                                {
                                    mouseCursorStack = new();
                                }
                            }
                            break;
                        case 9:
                        case 10:
                        case 11:
                        case 12:
                            if (!beltItems[selectedIndex - 9].isEmpty)
                            {
                                if (Input.GetMouseButtonDown(0))
                                {
                                    if (beltItems[selectedIndex - 9].item.itemListIndex == mouseCursorStack.item.itemListIndex)
                                    {
                                        beltItems[selectedIndex - 9].count += mouseCursorStack.count;
                                        mouseCursorStack = new();
                                    }
                                    else
                                    {
                                        if (actorEquipment.equippedItem != null && beltItems[selectedIndex - 9].item.itemListIndex == actorEquipment.equippedItem.GetComponent<Item>().itemListIndex)
                                        {
                                            beltItems[selectedIndex - 9].item.isBeltItem = false;
                                            actorEquipment.UnequippedCurrentItem();
                                        }
                                        else
                                        {
                                            for (int i = 0; i < actorEquipment.equippedArmor.Length; i++)
                                            {
                                                if (actorEquipment.equippedArmor[i] != null && beltItems[selectedIndex - 9].item.itemListIndex == actorEquipment.equippedArmor[i].GetComponent<Armor>().itemListIndex)
                                                {
                                                    beltItems[selectedIndex - 9].item.isBeltItem = false;

                                                    actorEquipment.UnequippedCurrentArmor(actorEquipment.equippedArmor[i].GetComponent<Armor>().m_ArmorType);
                                                }
                                            }
                                        }
                                        ItemStack temp = new(beltItems[selectedIndex - 9]);
                                        beltItems[selectedIndex - 9] = new(mouseCursorStack);
                                        mouseCursorStack = new(temp);
                                    }

                                }
                                if (Input.GetMouseButtonDown(1) && beltItems[selectedIndex - 9].item.itemListIndex == mouseCursorStack.item.itemListIndex)
                                {
                                    beltItems[selectedIndex - 9].count++;
                                    mouseCursorStack.count--;
                                    if (mouseCursorStack.count == 0)
                                    {
                                        mouseCursorStack = new();
                                    }
                                }
                            }
                            else
                            {
                                if (Input.GetMouseButtonDown(0))
                                {
                                    Item newItem = Instantiate(mouseCursorStack.item).GetComponent<Item>();
                                    newItem.GetComponent<MeshRenderer>().enabled = false;
                                    newItem.GetComponent<Collider>().enabled = false;
                                    beltItems[selectedIndex - 9] = new(newItem, mouseCursorStack.count, selectedIndex - 9, false);
                                    mouseCursorStack = new ItemStack();
                                }
                                else if (Input.GetMouseButtonDown(1))
                                {
                                    Item newItem = Instantiate(mouseCursorStack.item).GetComponent<Item>();
                                    newItem.GetComponent<MeshRenderer>().enabled = false;
                                    newItem.GetComponent<Collider>().enabled = false;
                                    beltItems[selectedIndex - 9] = new ItemStack(newItem, 1, selectedIndex - 9, false);
                                    mouseCursorStack.count--;
                                    if (mouseCursorStack.count == 0)
                                    {
                                        mouseCursorStack = new();
                                    }
                                }
                            }
                            beltItems[selectedIndex - 9].item.isBeltItem = true;
                            break;
                        default:
                            break;
                    }
                    AdjustButtonPrompts();
                    DisplayItems();
                    return;
                }

            }
            else if (selectedIndex > 21 && selectedIndex < 26)
            {
                if (mouseCursorStack.isEmpty)
                {
                    if (craftingSlots[selectedIndex - 22].transform.GetChild(1).GetComponent<SpriteRenderer>().color.a == 1)
                    { // if the slot is active
                        if (currentIngredients.Count - 1 >= selectedIndex - 22 && currentIngredients[selectedIndex - 22] != -1)
                        {
                            RemoveIngredient(selectedIndex - 22, true);
                        }
                    }
                }
                else
                {
                    if (craftingSlots[selectedIndex - 22].transform.GetChild(1).GetComponent<SpriteRenderer>().color.a == 1)
                    { // if the slot is active
                        if (currentIngredients.Count - 1 < selectedIndex - 22 || currentIngredients[selectedIndex - 22] == -1)
                        {
                            AddIngredient(selectedIndex - 22, true);
                        }
                    }
                }
            }
            else if (selectedIndex == 26)
            {
                Craft(true, true);
            }
        }
        DisplayItems();
    }
    public void InventoryActionButton(bool primary, bool secondary)
    {
        if (thirdPersonUserControl.playerPrefix == "sp") return; // maybe find a way for hybrid play? For now, it's only one or the other.


        if (isCrafting && craftingProduct != null && primary)
        {
            Craft();
            return;
        }
        if (thirdPersonUserControl.playerPrefix == "sp")
        {
            if (!mouseCursorStack.isEmpty)
            {
                cursorStack = new(mouseCursorStack);
                mouseCursorStack = new();
                DisplayItems();
            }
        };
        Debug.Log("Are we even getting here?" + selectedIndex);
        if (selectedIndex <= 8)
        {
            Debug.Log("Are we even getting here 2?");

            if (!cursorStack.isEmpty)
            {
                if (!items[selectedIndex].isEmpty)
                {
                    if (primary)
                    {
                        if (items[selectedIndex].item.itemListIndex == cursorStack.item.itemListIndex)
                        {
                            items[selectedIndex].count += cursorStack.count;
                            cursorStack = new();
                        }
                        else
                        {
                            ItemStack temp = new(items[selectedIndex]);
                            items[selectedIndex] = new(cursorStack);
                            cursorStack = new(temp);
                        }

                    }
                    if (secondary && items[selectedIndex].item.itemListIndex == cursorStack.item.itemListIndex)
                    {
                        items[selectedIndex].count++;
                        cursorStack.count--;
                        if (cursorStack.count == 0)
                        {
                            cursorStack = new();
                        }
                    }
                }
                else
                {
                    if (primary)
                    {
                        items[selectedIndex] = new(cursorStack);
                        cursorStack = new ItemStack();
                    }
                    else if (secondary)
                    {
                        items[selectedIndex] = new ItemStack(cursorStack.item, 1, selectedIndex, false);
                        cursorStack.count--;
                        if (cursorStack.count == 0)
                        {
                            cursorStack = new();
                        }
                    }

                }
            }
            else
            {
                Debug.Log("Are we even getting here 4?");

                if (!items[selectedIndex].isEmpty)
                {
                    if (primary)
                    {
                        cursorStack = items[selectedIndex];
                        items[selectedIndex] = new ItemStack();
                    }
                    else if (secondary)
                    {
                        cursorStack = new ItemStack(items[selectedIndex].item, 1, -1, false);
                        items[selectedIndex].count--;
                        if (items[selectedIndex].count == 0)
                        {
                            items[selectedIndex] = new ItemStack();
                        }
                    }
                }
            }
        }

        if (selectedIndex > 8 && selectedIndex < 22)
        {
            if (cursorStack.isEmpty)
            {
                switch (selectedIndex)
                {
                    case 13:
                        if (actorEquipment.hasItem)
                        {
                            Item _item = actorEquipment.equippedItem.GetComponent<Item>();
                            if (_item.isBeltItem)
                            {
                                for (int i = 0; i < beltItems.Length; i++)
                                {
                                    if (beltItems[i].item != null && _item.itemListIndex == beltItems[i].item.itemListIndex)
                                    {
                                        if (beltItems[i].count > 1)
                                        {
                                            beltItems[i].count--;
                                        }
                                        else
                                        {
                                            beltSlots[i].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = null;
                                            beltItems[i] = new ItemStack(null, 0, -1, true);
                                        }
                                    }
                                }
                            }
                            cursorStack = new ItemStack(_item, 1, -1, false);
                            equipmentSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = weaponInventorySlotIcon;
                            selectedBeltItem = -1;
                            actorEquipment.UnequippedCurrentItem();
                        }
                        break;
                    case 15:
                        if (actorEquipment.equippedArmor[(int)ArmorType.Helmet] != null)
                        {
                            Item _item = actorEquipment.equippedArmor[(int)ArmorType.Helmet].GetComponent<Item>();
                            if (_item.isBeltItem)
                            {
                                for (int i = 0; i < beltItems.Length; i++)
                                {
                                    if (beltItems[i].item != null && _item.itemListIndex == beltItems[i].item.itemListIndex)
                                    {
                                        if (beltItems[i].count > 1)
                                        {
                                            beltItems[i].count--;
                                        }
                                        else
                                        {
                                            beltSlots[i].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = null;
                                            beltItems[i] = new ItemStack(null, 0, -1, true);
                                        }
                                    }
                                }
                            }
                            cursorStack = new ItemStack(actorEquipment.equippedArmor[(int)ArmorType.Helmet].GetComponent<Item>(), 1, -1, false);
                            armorSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = helmetInventorySlotIcon;
                            actorEquipment.UnequippedCurrentArmor(ArmorType.Helmet);
                        }
                        break;
                    case 16:
                        if (actorEquipment.equippedArmor[(int)ArmorType.Chest] != null)
                        {
                            Item _item = actorEquipment.equippedArmor[(int)ArmorType.Chest].GetComponent<Item>();
                            if (_item.isBeltItem)
                            {
                                for (int i = 0; i < beltItems.Length; i++)
                                {
                                    if (beltItems[i].item != null && _item.itemListIndex == beltItems[i].item.itemListIndex)
                                    {
                                        if (beltItems[i].count > 1)
                                        {
                                            beltItems[i].count--;
                                        }
                                        else
                                        {
                                            beltSlots[i].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = null;
                                            beltItems[i] = new ItemStack(null, 0, -1, true);
                                        }
                                    }
                                }
                            }
                            cursorStack = new ItemStack(actorEquipment.equippedArmor[(int)ArmorType.Chest].GetComponent<Item>(), 1, -1, false);
                            armorSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = chestInventorySlotIcon;
                            actorEquipment.UnequippedCurrentArmor(ArmorType.Chest);
                        }
                        break;
                    case 17:
                        if (actorEquipment.equippedArmor[(int)ArmorType.Legs] != null)
                        {
                            Item _item = actorEquipment.equippedArmor[(int)ArmorType.Legs].GetComponent<Item>();
                            if (_item.isBeltItem)
                            {
                                for (int i = 0; i < beltItems.Length; i++)
                                {
                                    if (beltItems[i].item != null && _item.itemListIndex == beltItems[i].item.itemListIndex)
                                    {
                                        if (beltItems[i].count > 1)
                                        {
                                            beltItems[i].count--;
                                        }
                                        else
                                        {
                                            beltSlots[i].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = null;
                                            beltItems[i] = new ItemStack(null, 0, -1, true);
                                        }
                                    }
                                }
                            }
                            cursorStack = new ItemStack(actorEquipment.equippedArmor[(int)ArmorType.Legs].GetComponent<Item>(), 1, -1, false);
                            armorSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = legsInventorySlotIcon;
                            actorEquipment.UnequippedCurrentArmor(ArmorType.Legs);
                        }
                        break;
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                        //IS CURSOR EMPTY
                        if (!beltItems[selectedIndex - 9].isEmpty)
                        {
                            if (primary)
                            {
                                if (actorEquipment.equippedItem != null && actorEquipment.equippedItem.GetComponent<Item>().itemListIndex == beltItems[selectedIndex - 9].item.itemListIndex)
                                {
                                    beltItems[selectedIndex - 9].item.isBeltItem = false;
                                    equipmentSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = weaponInventorySlotIcon;
                                    selectedBeltItem = -1;
                                    actorEquipment.UnequippedCurrentItem();
                                }
                                else if (beltItems[selectedIndex - 9].item.TryGetComponent<Armor>(out var armor) && actorEquipment.equippedArmor[(int)armor.m_ArmorType] != null && actorEquipment.equippedArmor[(int)armor.m_ArmorType].GetComponent<Armor>().itemListIndex == armor.itemListIndex)
                                {
                                    beltItems[selectedIndex - 9].item.isBeltItem = false;
                                    switch (armor.m_ArmorType)
                                    {
                                        case ArmorType.Helmet:
                                            armorSlots[(int)armor.m_ArmorType].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = helmetInventorySlotIcon;
                                            break;
                                        case ArmorType.Chest:
                                            armorSlots[(int)armor.m_ArmorType].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = chestInventorySlotIcon;
                                            break;
                                        case ArmorType.Legs:
                                            armorSlots[(int)armor.m_ArmorType].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = legsInventorySlotIcon;
                                            break;
                                    }
                                    actorEquipment.UnequippedCurrentArmor(armor.m_ArmorType);
                                }

                                cursorStack = new ItemStack(beltItems[selectedIndex - 9]);
                                beltItems[selectedIndex - 9] = new ItemStack();
                            }
                            else if (secondary)
                            {
                                cursorStack = new ItemStack(beltItems[selectedIndex - 9].item, 1, -1, false);
                                beltItems[selectedIndex - 9].count--;
                                if (beltItems[selectedIndex - 9].count == 0)
                                {
                                    if (actorEquipment.equippedItem.GetComponent<Item>().itemListIndex == beltItems[selectedIndex - 9].item.itemListIndex)
                                    {
                                        beltItems[selectedIndex - 9].item.isBeltItem = false;
                                        equipmentSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = weaponInventorySlotIcon;
                                        actorEquipment.UnequippedCurrentItem();
                                    }
                                    else if (beltItems[selectedIndex - 9].item.TryGetComponent<Armor>(out var armor) && actorEquipment.equippedArmor[(int)armor.m_ArmorType] != null && actorEquipment.equippedArmor[(int)armor.m_ArmorType].GetComponent<Armor>().itemListIndex == armor.itemListIndex)
                                    {
                                        beltItems[selectedIndex - 9].item.isBeltItem = false;
                                        switch (armor.m_ArmorType)
                                        {
                                            case ArmorType.Helmet:
                                                armorSlots[selectedIndex - 9].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = helmetInventorySlotIcon;
                                                break;
                                            case ArmorType.Chest:
                                                armorSlots[selectedIndex - 9].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = chestInventorySlotIcon;
                                                break;
                                            case ArmorType.Legs:
                                                armorSlots[selectedIndex - 9].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = legsInventorySlotIcon;
                                                break;
                                        }
                                        actorEquipment.UnequippedCurrentArmor(armor.m_ArmorType);
                                    }
                                    beltItems[selectedIndex - 9] = new ItemStack();
                                }
                            }
                        }
                        break;
                    case 18:
                        if (actorEquipment.equippedSpecialItems[0] != null)
                        {
                            Item _item = actorEquipment.equippedSpecialItems[0].GetComponent<Item>();
                            if (_item.isBeltItem)
                            {
                                for (int i = 0; i < beltItems.Length; i++)
                                {
                                    if (beltItems[i].item != null && _item.itemListIndex == beltItems[i].item.itemListIndex)
                                    {
                                        if (beltItems[i].count > 1)
                                        {
                                            beltItems[i].count--;
                                        }
                                        else
                                        {
                                            beltSlots[i].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = null;
                                            beltItems[i] = new ItemStack(null, 0, -1, true);
                                        }
                                    }
                                }
                            }
                            cursorStack = new ItemStack(_item, 1, -1, false);
                            itemSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = capeInventorySlotIcon;
                            selectedBeltItem = -1;
                            actorEquipment.UnequippedCurrentSpecialItem(0);
                        }
                        break;
                    case 19:
                        if (actorEquipment.equippedSpecialItems[1] != null)
                        {
                            Item _item = actorEquipment.equippedSpecialItems[1].GetComponent<Item>();
                            if (_item.isBeltItem)
                            {
                                for (int i = 0; i < beltItems.Length; i++)
                                {
                                    if (beltItems[i].item != null && _item.itemListIndex == beltItems[i].item.itemListIndex)
                                    {
                                        if (beltItems[i].count > 1)
                                        {
                                            beltItems[i].count--;
                                        }
                                        else
                                        {
                                            beltSlots[i].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = null;
                                            beltItems[i] = new ItemStack(null, 0, -1, true);
                                        }
                                    }
                                }
                            }
                            cursorStack = new ItemStack(_item, 1, -1, false);
                            itemSlots[1].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = utilityInventorySlotIcon;
                            selectedBeltItem = -1;
                            actorEquipment.UnequippedCurrentSpecialItem(1);
                        }
                        break;
                    case 20:
                        if (actorEquipment.equippedSpecialItems[2] != null)
                        {
                            Item _item = actorEquipment.equippedSpecialItems[2].GetComponent<Item>();
                            if (_item.isBeltItem)
                            {
                                for (int i = 0; i < beltItems.Length; i++)
                                {
                                    if (beltItems[i].item != null && _item.itemListIndex == beltItems[i].item.itemListIndex)
                                    {
                                        if (beltItems[i].count > 1)
                                        {
                                            beltItems[i].count--;
                                        }
                                        else
                                        {
                                            beltSlots[i].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = null;
                                            beltItems[i] = new ItemStack(null, 0, -1, true);
                                        }
                                    }
                                }
                            }
                            cursorStack = new ItemStack(_item, 1, -1, false);
                            itemSlots[2].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = pipeInventorySlotIcon;
                            selectedBeltItem = -1;
                            actorEquipment.UnequippedCurrentSpecialItem(2);
                        }
                        break;
                    case 21:
                        if (actorEquipment.equippedSpecialItems[3] != null)
                        {
                            Item _item = actorEquipment.equippedSpecialItems[3].GetComponent<Item>();
                            if (_item.isBeltItem)
                            {
                                for (int i = 0; i < beltItems.Length; i++)
                                {
                                    if (beltItems[i].item != null && _item.itemListIndex == beltItems[i].item.itemListIndex)
                                    {
                                        if (beltItems[i].count > 1)
                                        {
                                            beltItems[i].count--;
                                        }
                                        else
                                        {
                                            beltSlots[i].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = null;
                                            beltItems[i] = new ItemStack(null, 0, -1, true);
                                        }
                                    }
                                }
                            }
                            cursorStack = new ItemStack(_item, 1, -1, false);
                            itemSlots[3].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = specialInventorySlotIcon;
                            selectedBeltItem = -1;
                            actorEquipment.UnequippedCurrentSpecialItem(3);
                        }
                        break;
                    default:
                        break;
                }
                AdjustButtonPrompts();
                DisplayItems();
            }
            else
            {
                switch (selectedIndex)
                {
                    case 13:
                        if (actorEquipment.equippedItem != null)
                        {
                            if (cursorStack.count > 1 && cursorStack.item.itemListIndex != actorEquipment.equippedItem.GetComponent<Item>().itemListIndex)
                            {
                                TryUnequippedItem();
                                selectedBeltItem = -1;
                                actorEquipment.EquipItem(cursorStack.item);
                                equipmentSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = cursorStack.item.icon;
                                cursorStack.count--;
                                if (cursorStack.count == 0)
                                {
                                    cursorStack = new();
                                }
                            }
                            else if (cursorStack.item.itemListIndex != actorEquipment.equippedItem.GetComponent<Item>().itemListIndex)
                            {
                                Item temp = actorEquipment.equippedItem.GetComponent<Item>();
                                selectedBeltItem = -1;
                                TryUnequippedItem();
                                actorEquipment.EquipItem(cursorStack.item);
                                equipmentSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = cursorStack.item.icon;

                                cursorStack = new(temp, 1, -1, false);

                            }

                        }
                        else
                        {
                            selectedBeltItem = -1;
                            actorEquipment.EquipItem(cursorStack.item);
                            equipmentSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = cursorStack.item.icon;

                            cursorStack.count--;
                            if (cursorStack.count == 0)
                            {
                                cursorStack = new();
                            }
                        }
                        break;
                    case 15:
                        if (actorEquipment.equippedArmor[0] != null)
                        {
                            if (cursorStack.item.TryGetComponent<Armor>(out var _armor))
                            {
                                if (cursorStack.count > 1)
                                {
                                    TryUnequippedArmor(_armor.m_ArmorType);
                                    actorEquipment.EquipItem(cursorStack.item);
                                    cursorStack.count--;
                                }
                                else
                                {

                                    Item temp = actorEquipment.equippedArmor[(int)_armor.m_ArmorType].GetComponent<Item>();
                                    actorEquipment.UnequippedCurrentArmor(_armor.m_ArmorType);
                                    actorEquipment.EquipItem(cursorStack.item);
                                    cursorStack = new(temp, 1, -1, false);

                                }
                            }
                        }
                        else
                        {
                            if (cursorStack.item.TryGetComponent<Armor>(out var _armor))
                            {
                                if (cursorStack.count > 1)
                                {
                                    TryUnequippedArmor(_armor.m_ArmorType);
                                    actorEquipment.EquipItem(cursorStack.item);
                                    cursorStack.count--;
                                }
                                else
                                {
                                    actorEquipment.EquipItem(cursorStack.item);
                                    cursorStack = new();
                                }
                            }
                        }
                        break;
                    case 16:
                        if (actorEquipment.equippedArmor[1] != null)
                        {
                            if (cursorStack.item.TryGetComponent<Armor>(out var _armor))
                            {
                                if (cursorStack.count > 1)
                                {
                                    TryUnequippedArmor(_armor.m_ArmorType);
                                    actorEquipment.EquipItem(cursorStack.item);
                                    cursorStack.count--;
                                }
                                else
                                {
                                    actorEquipment.UnequippedCurrentArmor(_armor.m_ArmorType);
                                    Item temp = actorEquipment.equippedArmor[(int)_armor.m_ArmorType].GetComponent<Item>();
                                    actorEquipment.EquipItem(cursorStack.item);
                                    cursorStack = new(temp, 1, -1, false);

                                }
                            }
                        }
                        else
                        {
                            if (cursorStack.item.TryGetComponent<Armor>(out var _armor))
                            {
                                if (cursorStack.count > 1)
                                {
                                    TryUnequippedArmor(_armor.m_ArmorType);
                                    actorEquipment.EquipItem(cursorStack.item);
                                    cursorStack.count--;
                                }
                                else
                                {
                                    actorEquipment.EquipItem(cursorStack.item);
                                    cursorStack = new();
                                }
                            }
                        }
                        break;
                    case 17:
                        if (actorEquipment.equippedArmor[2] != null)
                        {
                            if (cursorStack.item.TryGetComponent<Armor>(out var _armor))
                            {

                                if (cursorStack.count > 1)
                                {
                                    TryUnequippedArmor(_armor.m_ArmorType);

                                    actorEquipment.EquipItem(cursorStack.item);
                                    cursorStack.count--;
                                }
                                else
                                {
                                    actorEquipment.UnequippedCurrentArmor(_armor.m_ArmorType);

                                    Item temp = actorEquipment.equippedArmor[(int)_armor.m_ArmorType].GetComponent<Item>();

                                    actorEquipment.EquipItem(cursorStack.item);
                                    cursorStack = new(temp, 1, -1, false);

                                }
                            }
                        }
                        else
                        {
                            if (cursorStack.item.TryGetComponent<Armor>(out var _armor))
                            {
                                if (cursorStack.count > 1)
                                {
                                    TryUnequippedArmor(_armor.m_ArmorType);

                                    actorEquipment.EquipItem(cursorStack.item);
                                    cursorStack.count--;
                                }
                                else
                                {

                                    actorEquipment.EquipItem(cursorStack.item);
                                    cursorStack = new();
                                }
                            }
                        }
                        break;
                    case 18:
                        if (!cursorStack.item.TryGetComponent<Cape>(out var cape))
                        {
                            return;
                        }
                        if (actorEquipment.equippedSpecialItems[0] != null)
                        {
                            if (cursorStack.count > 1 && cursorStack.item.itemListIndex != actorEquipment.equippedSpecialItems[0].GetComponent<Item>().itemListIndex)
                            {
                                TryUnequippedSpecialItem(0);
                                selectedBeltItem = -1;
                                actorEquipment.EquipItem(cursorStack.item);
                                itemSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = cursorStack.item.icon;
                                cursorStack.count--;
                                if (cursorStack.count == 0)
                                {
                                    cursorStack = new();
                                }
                            }
                            else if (cursorStack.item.itemListIndex != actorEquipment.equippedItem.GetComponent<Item>().itemListIndex)
                            {
                                Item temp = actorEquipment.equippedSpecialItems[0].GetComponent<Item>();
                                selectedBeltItem = -1;
                                TryUnequippedItem();
                                actorEquipment.EquipItem(cursorStack.item);
                                itemSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = cursorStack.item.icon;

                                cursorStack = new(temp, 1, -1, false);

                            }

                        }
                        else
                        {
                            selectedBeltItem = -1;
                            actorEquipment.EquipItem(cursorStack.item);
                            itemSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = cursorStack.item.icon;

                            cursorStack.count--;
                            if (cursorStack.count == 0)
                            {
                                cursorStack = new();
                            }
                        }
                        break;
                    case 19:
                        if (!cursorStack.item.TryGetComponent<UtilityItem>(out var utilItem))
                        {
                            return;
                        }
                        if (actorEquipment.equippedSpecialItems[1] != null)
                        {
                            if (cursorStack.count > 1 && cursorStack.item.itemListIndex != actorEquipment.equippedSpecialItems[1].GetComponent<Item>().itemListIndex)
                            {
                                TryUnequippedSpecialItem(1);
                                selectedBeltItem = -1;
                                actorEquipment.EquipItem(cursorStack.item);
                                itemSlots[1].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = cursorStack.item.icon;
                                cursorStack.count--;
                                if (cursorStack.count == 1)
                                {
                                    cursorStack = new();
                                }
                            }
                            else if (cursorStack.item.itemListIndex != actorEquipment.equippedItem.GetComponent<Item>().itemListIndex)
                            {
                                Item temp = actorEquipment.equippedSpecialItems[1].GetComponent<Item>();
                                selectedBeltItem = -1;
                                TryUnequippedItem();
                                actorEquipment.EquipItem(cursorStack.item);
                                itemSlots[1].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = cursorStack.item.icon;

                                cursorStack = new(temp, 1, -1, false);

                            }

                        }
                        else
                        {
                            selectedBeltItem = -1;
                            actorEquipment.EquipItem(cursorStack.item);
                            itemSlots[1].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = cursorStack.item.icon;

                            cursorStack.count--;
                            if (cursorStack.count == 0)
                            {
                                cursorStack = new();
                            }
                        }
                        break;
                    case 20:
                        if (!cursorStack.item.TryGetComponent<Pipe>(out var pipe))
                        {
                            return;
                        }
                        if (actorEquipment.equippedSpecialItems[2] != null)
                        {
                            if (cursorStack.count > 1 && cursorStack.item.itemListIndex != actorEquipment.equippedSpecialItems[2].GetComponent<Item>().itemListIndex)
                            {
                                TryUnequippedSpecialItem(2);
                                selectedBeltItem = -1;
                                actorEquipment.EquipItem(cursorStack.item);
                                itemSlots[2].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = cursorStack.item.icon;
                                cursorStack.count--;
                                if (cursorStack.count == 0)
                                {
                                    cursorStack = new();
                                }
                            }
                            else if (cursorStack.item.itemListIndex != actorEquipment.equippedItem.GetComponent<Item>().itemListIndex)
                            {
                                Item temp = actorEquipment.equippedSpecialItems[2].GetComponent<Item>();
                                selectedBeltItem = -1;
                                TryUnequippedItem();
                                actorEquipment.EquipItem(cursorStack.item);
                                itemSlots[2].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = cursorStack.item.icon;

                                cursorStack = new(temp, 1, -1, false);

                            }

                        }
                        else
                        {
                            selectedBeltItem = -1;
                            actorEquipment.EquipItem(cursorStack.item);
                            itemSlots[2].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = cursorStack.item.icon;

                            cursorStack.count--;
                            if (cursorStack.count == 0)
                            {
                                cursorStack = new();
                            }
                        }
                        break;
                    case 21:
                        if (!cursorStack.item.TryGetComponent<Jewelry>(out var jewelry))
                        {
                            return;
                        }
                        if (actorEquipment.equippedSpecialItems[3] != null)
                        {
                            if (cursorStack.count > 1 && cursorStack.item.itemListIndex != actorEquipment.equippedSpecialItems[2].GetComponent<Item>().itemListIndex)
                            {
                                TryUnequippedSpecialItem(3);
                                selectedBeltItem = -1;
                                actorEquipment.EquipItem(cursorStack.item);
                                itemSlots[3].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = cursorStack.item.icon;
                                cursorStack.count--;
                                if (cursorStack.count == 0)
                                {
                                    cursorStack = new();
                                }
                            }
                            else if (cursorStack.item.itemListIndex != actorEquipment.equippedItem.GetComponent<Item>().itemListIndex)
                            {
                                Item temp = actorEquipment.equippedSpecialItems[2].GetComponent<Item>();
                                selectedBeltItem = -1;
                                TryUnequippedItem();
                                actorEquipment.EquipItem(cursorStack.item);
                                itemSlots[2].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = cursorStack.item.icon;

                                cursorStack = new(temp, 1, -1, false);

                            }

                        }
                        else
                        {
                            selectedBeltItem = -1;
                            actorEquipment.EquipItem(cursorStack.item);
                            itemSlots[3].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = cursorStack.item.icon;

                            cursorStack.count--;
                            if (cursorStack.count == 0)
                            {
                                cursorStack = new();
                            }
                        }
                        break;
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                        if (!beltItems[selectedIndex - 9].isEmpty)
                        {
                            if (primary)
                            {
                                if (beltItems[selectedIndex - 9].item.itemListIndex == cursorStack.item.itemListIndex)
                                {
                                    beltItems[selectedIndex - 9].count += cursorStack.count;
                                    cursorStack = new();
                                }
                                else
                                {
                                    if (actorEquipment.equippedItem != null && beltItems[selectedIndex - 9].item.itemListIndex == actorEquipment.equippedItem.GetComponent<Item>().itemListIndex)
                                    {
                                        beltItems[selectedIndex - 9].item.isBeltItem = false;
                                        actorEquipment.UnequippedCurrentItem();
                                    }
                                    else
                                    {
                                        for (int i = 0; i < actorEquipment.equippedArmor.Length; i++)
                                        {
                                            if (actorEquipment.equippedArmor[i] != null && beltItems[selectedIndex - 9].item.itemListIndex == actorEquipment.equippedArmor[i].GetComponent<Armor>().itemListIndex)
                                            {
                                                beltItems[selectedIndex - 9].item.isBeltItem = false;

                                                actorEquipment.UnequippedCurrentArmor(actorEquipment.equippedArmor[i].GetComponent<Armor>().m_ArmorType);
                                            }
                                        }
                                    }
                                    ItemStack temp = new(beltItems[selectedIndex - 9]);
                                    beltItems[selectedIndex - 9] = new(cursorStack);
                                    cursorStack = new(temp);
                                }

                            }
                            if (secondary && beltItems[selectedIndex - 9].item.itemListIndex == cursorStack.item.itemListIndex)
                            {
                                beltItems[selectedIndex - 9].count++;
                                cursorStack.count--;
                                if (cursorStack.count == 0)
                                {
                                    cursorStack = new();
                                }
                            }
                        }
                        else
                        {
                            if (primary)
                            {
                                Item newItem = Instantiate(cursorStack.item).GetComponent<Item>();
                                newItem.GetComponent<MeshRenderer>().enabled = false;
                                newItem.GetComponent<Collider>().enabled = false;
                                beltItems[selectedIndex - 9] = new(newItem, cursorStack.count, selectedIndex - 9, false);
                                cursorStack = new ItemStack();
                            }
                            else if (secondary)
                            {
                                Item newItem = Instantiate(cursorStack.item).GetComponent<Item>();
                                newItem.GetComponent<MeshRenderer>().enabled = false;
                                newItem.GetComponent<Collider>().enabled = false;
                                beltItems[selectedIndex - 9] = new ItemStack(newItem, 1, selectedIndex - 9, false);
                                cursorStack.count--;
                                if (cursorStack.count == 0)
                                {
                                    cursorStack = new();
                                }
                            }
                        }
                        beltItems[selectedIndex - 9].item.isBeltItem = true;
                        break;
                    default:
                        break;
                }
                AdjustButtonPrompts();
                DisplayItems();
            }

        }
        else if (selectedIndex > 21 && selectedIndex < 26)
        {
            if (cursorStack.isEmpty)
            {
                if (craftingSlots[selectedIndex - 22].transform.GetChild(1).GetComponent<SpriteRenderer>().color.a == 1)
                { // if the slot is active
                    if (currentIngredients.Count - 1 >= selectedIndex - 22 && currentIngredients[selectedIndex - 22] != -1)
                    {
                        RemoveIngredient(selectedIndex - 22, false);
                    }
                }
            }
            else
            {
                if (craftingSlots[selectedIndex - 22].transform.GetChild(1).GetComponent<SpriteRenderer>().color.a == 1)
                { // if the slot is active
                    if (currentIngredients.Count - 1 < selectedIndex - 22 || currentIngredients[selectedIndex - 22] == -1)
                    {
                        AddIngredient(selectedIndex - 22, false);
                    }
                }
            }
        }
        else if (selectedIndex == 26)
        {
            Craft(true, false);
        }
        DisplayItems();
    }

    public void EquipFromInventory(int slotIndex)
    {
        selectedBeltItem = -1;
        if (!items[slotIndex].isEmpty)
        {
            if (items[slotIndex].item.TryGetComponent<Armor>(out var armor))
            {
                Item temp = items[slotIndex].item;
                RemoveItem(slotIndex, 1);
                if (actorEquipment.equippedArmor[(int)armor.m_ArmorType] != null)
                {
                    TryUnequippedArmor(armor.m_ArmorType);
                }
                actorEquipment.EquipItem(temp);

            }
            else
            {
                if (actorEquipment.hasItem)
                {
                    Item temp = items[slotIndex].item;
                    RemoveItem(slotIndex, 1);
                    TryUnequippedItem();
                    equipmentSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = inventorySlotIcon;
                    actorEquipment.EquipItem(temp);

                }
                else
                {
                    actorEquipment.EquipItem(items[slotIndex].item);
                    equipmentSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = items[slotIndex].item.icon;
                    RemoveItem(slotIndex, 1);
                }
            }
        }
        else
        {
            TryUnequippedItem();
        }
    }
    public void EquipFromToolBelt(int slotIndex)
    {
        if (!beltItems[slotIndex].isEmpty)  // If there is something in the belt slot
        {
            if (actorEquipment.equippedItem != null && beltItems[slotIndex].item.itemListIndex == actorEquipment.equippedItem.GetComponent<Item>().itemListIndex) return;
            if (beltItems[slotIndex].item.TryGetComponent<Armor>(out var armor)) //If that item is armor
            {
                Item temp = beltItems[slotIndex].item;
                if (actorEquipment.equippedArmor[(int)armor.m_ArmorType] != null)
                {
                    TryUnequippedArmor(armor.m_ArmorType);
                }
                actorEquipment.EquipItem(temp);
            }
            else
            {
                selectedBeltItem = slotIndex;
                if (actorEquipment.hasItem)
                {

                    Item temp = beltItems[slotIndex].item;
                    TryUnequippedItem();
                    equipmentSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = inventorySlotIcon;
                    actorEquipment.EquipItem(temp, true);

                }
                else
                {
                    actorEquipment.EquipItem(beltItems[slotIndex].item, true);
                    equipmentSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = beltItems[slotIndex].item.icon;
                }
            }
        }
        else
        {
            selectedBeltItem = slotIndex;
            TryUnequippedItem();
        }
    }

    private void TryUnequippedItem()
    {
        if (actorEquipment.equippedItem == null) return;
        Item currentItem = actorEquipment.equippedItem.GetComponent<Item>();
        bool isBeltItem = currentItem.isBeltItem;
        if (isBeltItem)
        {
            actorEquipment.UnequippedCurrentItem();
            return;
        }
        bool canUnequipped = actorEquipment.UnequippedCurrentItemToInventory();
        if (!canUnequipped)
        {
            int itemIndex = actorEquipment.equippedItem.GetComponent<Item>().itemListIndex;
            actorEquipment.UnequippedCurrentItem();
            DropItem(itemIndex, transform.position);
        };
    }
    private void TryUnequippedSpecialItem(int index)
    {
        if (actorEquipment.equippedSpecialItems[index] == null) return;
        bool isBeltItem = actorEquipment.equippedSpecialItems[index].GetComponent<Item>().isBeltItem;
        bool canUnequipped = actorEquipment.UnequippedCurrentSpecialItemToInventory(index);
        if (isBeltItem)
        {
            return;
        }
        if (!canUnequipped)
        {
            int itemIndex = actorEquipment.equippedSpecialItems[index].GetComponent<Item>().itemListIndex;
            DropItem(itemIndex, transform.position);
        };
    }

    private void TryUnequippedArmor(ArmorType armorType)
    {
        bool isBeltItem = actorEquipment.equippedItem.GetComponent<Item>().isBeltItem;
        bool canUnequipped = actorEquipment.UnequippedCurrentArmorToInventory(armorType);
        if (isBeltItem)
        {
            return;
        }
        if (!canUnequipped)
        {
            int itemIndex = actorEquipment.equippedArmor[(int)armorType].GetComponent<Item>().itemListIndex;
            actorEquipment.UnequippedCurrentArmor(armorType);
            DropItem(itemIndex, transform.position);
        };
    }


    public void UpdateInfoPanel(string name, string description, int damage, int armor, int energy, int health, int con, int str, int dex, int intg)
    {
        infoPanel.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = name;
        infoPanel.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = description;
        if (damage != 0)
        {
            infoPanel.transform.GetChild(1).GetChild(2).GetChild(0).gameObject.SetActive(true);
            infoPanel.transform.GetChild(1).GetChild(2).GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{damage}";
        }
        else
        {
            infoPanel.transform.GetChild(1).GetChild(2).GetChild(0).gameObject.SetActive(false);
        }

        if (armor != 0)
        {
            infoPanel.transform.GetChild(1).GetChild(2).GetChild(1).gameObject.SetActive(true);
            infoPanel.transform.GetChild(1).GetChild(2).GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{armor}";
        }
        else
        {
            infoPanel.transform.GetChild(1).GetChild(2).GetChild(1).gameObject.SetActive(false);
        }

        if (health != 0 || energy != 0)
        {
            infoPanel.transform.GetChild(1).GetChild(2).GetChild(3).gameObject.SetActive(true);
            infoPanel.transform.GetChild(1).GetChild(2).GetChild(3).GetChild(1).GetComponent<TextMeshProUGUI>().text = $"+{energy}";
        }
        else
        {
            infoPanel.transform.GetChild(1).GetChild(2).GetChild(3).gameObject.SetActive(false);
        }

        if (health != 0 || energy != 0)
        {
            infoPanel.transform.GetChild(1).GetChild(2).GetChild(2).gameObject.SetActive(true);
            infoPanel.transform.GetChild(1).GetChild(2).GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>().text = $"+{health}";
        }
        else
        {
            infoPanel.transform.GetChild(1).GetChild(2).GetChild(2).gameObject.SetActive(false);
        }
        int i = 0;
        infoPanel.transform.GetChild(1).GetChild(3).gameObject.SetActive(false);
        infoPanel.transform.GetChild(1).GetChild(4).gameObject.SetActive(false);
        infoPanel.transform.GetChild(1).GetChild(5).gameObject.SetActive(false);
        infoPanel.transform.GetChild(1).GetChild(6).gameObject.SetActive(false);
        if (con != 0)
        {
            infoPanel.transform.GetChild(1).GetChild(3 + i).gameObject.SetActive(true);
            infoPanel.transform.GetChild(1).GetChild(3 + i).GetChild(0).gameObject.SetActive(true);
            infoPanel.transform.GetChild(1).GetChild(3 + i).GetChild(0).GetChild(0).gameObject.SetActive(true);
            infoPanel.transform.GetChild(1).GetChild(3 + i).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = con.ToString();
            infoPanel.transform.GetChild(1).GetChild(3 + i).GetChild(1).gameObject.SetActive(false);
            infoPanel.transform.GetChild(1).GetChild(3 + i).GetChild(2).gameObject.SetActive(false);
            infoPanel.transform.GetChild(1).GetChild(3 + i).GetChild(3).gameObject.SetActive(false);
            i++;
        }
        if (str != 0)
        {
            infoPanel.transform.GetChild(1).GetChild(3 + i).GetChild(0).gameObject.SetActive(false);
            infoPanel.transform.GetChild(1).GetChild(3 + i).gameObject.SetActive(true);
            infoPanel.transform.GetChild(1).GetChild(3 + i).GetChild(1).gameObject.SetActive(true);
            infoPanel.transform.GetChild(1).GetChild(3 + i).GetChild(1).GetChild(0).gameObject.SetActive(true);
            infoPanel.transform.GetChild(1).GetChild(3 + i).GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = str.ToString();
            infoPanel.transform.GetChild(1).GetChild(3 + i).GetChild(2).gameObject.SetActive(false);
            infoPanel.transform.GetChild(1).GetChild(3 + i).GetChild(3).gameObject.SetActive(false);
            i++;
        }
        if (intg != 0)
        {
            infoPanel.transform.GetChild(1).GetChild(3 + i).GetChild(0).gameObject.SetActive(false);
            infoPanel.transform.GetChild(1).GetChild(3 + i).GetChild(1).gameObject.SetActive(false);
            infoPanel.transform.GetChild(1).GetChild(3 + i).gameObject.SetActive(true);
            infoPanel.transform.GetChild(1).GetChild(3 + i).GetChild(2).gameObject.SetActive(true);
            infoPanel.transform.GetChild(1).GetChild(3 + i).GetChild(2).GetChild(0).gameObject.SetActive(true);
            infoPanel.transform.GetChild(1).GetChild(3 + i).GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = intg.ToString();
            infoPanel.transform.GetChild(1).GetChild(3 + i).GetChild(3).gameObject.SetActive(false);
            i++;
        }
        if (dex != 0)
        {
            infoPanel.transform.GetChild(1).GetChild(3 + i).GetChild(0).gameObject.SetActive(false);
            infoPanel.transform.GetChild(1).GetChild(3 + i).GetChild(1).gameObject.SetActive(false);
            infoPanel.transform.GetChild(1).GetChild(3 + i).GetChild(2).gameObject.SetActive(false);
            infoPanel.transform.GetChild(1).GetChild(3 + i).gameObject.SetActive(true);
            infoPanel.transform.GetChild(1).GetChild(3 + i).GetChild(3).gameObject.SetActive(true);
            infoPanel.transform.GetChild(1).GetChild(3 + i).GetChild(3).GetChild(0).gameObject.SetActive(true);
            infoPanel.transform.GetChild(1).GetChild(3 + i).GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = dex.ToString();
        }

    }
    public void UpdateQuickStats()
    {
        quickStatsPanel.transform.GetChild(1).GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{m_CharacterManager.stats.maxHealth.ToString("F1")}/{m_CharacterManager.stats.health.ToString("F1")}";
        quickStatsPanel.transform.GetChild(1).GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{m_CharacterManager.stats.stomachCapacity.ToString("F1")}/{m_CharacterManager.stats.stomachValue.ToString("F1")}";
        float attackValue = 0;
        attackValue += m_CharacterManager.stats.attack;
        if (m_CharacterManager.equipment.hasItem && m_CharacterManager.equipment.equippedItem.TryGetComponent<ToolItem>(out var tool))
        {
            attackValue += tool.damage;
        }
        quickStatsPanel.transform.GetChild(1).GetChild(3).GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{m_CharacterManager.equipment.GetArmorBonus() + m_CharacterManager.stats.defense}";
        quickStatsPanel.transform.GetChild(1).GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{attackValue}";
        quickStatsPanel.transform.GetChild(1).GetChild(5).GetComponent<TextMeshProUGUI>().text = $"{m_CharacterManager.stats.constitution}";
        quickStatsPanel.transform.GetChild(1).GetChild(7).GetComponent<TextMeshProUGUI>().text = $"{m_CharacterManager.stats.strength}";
        quickStatsPanel.transform.GetChild(1).GetChild(9).GetComponent<TextMeshProUGUI>().text = $"{m_CharacterManager.stats.intelligence}";
        quickStatsPanel.transform.GetChild(1).GetChild(11).GetComponent<TextMeshProUGUI>().text = $"{m_CharacterManager.stats.dexterity}";

    }
    public void DropItem()
    {
        if (selectedIndex > 12) return;
        ItemStack stack = selectedIndex < 9 ? items[selectedIndex] : beltItems[selectedIndex - 9];
        if (stack.isEmpty == false)
        {
            if (stack.count > 0)
            {
                //Call Prc on ItemsManager
                ItemManager.Instance.CallDropItemRPC(stack.item.itemListIndex, transform.position);
                RemoveItem(selectedIndex, 1);
            }
        }
    }
    public void DropItem(int itemIndex, Vector3 pos)
    {
        ItemManager.Instance.CallDropItemRPC(itemIndex, pos);
        audioManager.PlayDropItem();
    }

    public void MoveSelection(Vector2 input)
    {
        if (thirdPersonUserControl.playerPrefix == "sp")
        {
            if (!mouseCursorStack.isEmpty)
            {
                cursorStack = new(mouseCursorStack);
                mouseCursorStack = new();
                DisplayItems();
            }
        };

        if (input.x > 0) // Right
        {
            if (selectedIndex == 2 || selectedIndex == 5)
            {
                SetSelectedItem(14);
            }
            else if (selectedIndex == 8)
            {
                SetSelectedItem(9);
            }
            else if (selectedIndex == 9)
            {
                SetSelectedItem(10);
            }
            else if (selectedIndex == 10)
            {
                SetSelectedItem(18);
            }
            else if (selectedIndex == 11)
            {
                SetSelectedItem(12);
            }
            else if (selectedIndex == 12)
            {
                SetSelectedItem(18);
            }
            else if (selectedIndex == 14)
            {
                SetSelectedItem(13);
            }
            else if (selectedIndex == 9)
            {
                SetSelectedItem(10);
            }
            else if (selectedIndex == 11)
            {
                SetSelectedItem(12);
            }
            else if (selectedIndex == 10 || selectedIndex == 12)
            {
                SetSelectedItem(18);
            }
            else if (selectedIndex == 13)
            {
                SetSelectedItem(21);
            }
            else if (selectedIndex == 18)
            {
                SetSelectedItem(16);
            }
            else if (selectedIndex == 21)
            {
                SetSelectedItem(15);
            }
            else if (selectedIndex == 15)
            {
                SetSelectedItem(20);
            }
            else if (selectedIndex == 16 || selectedIndex == 17)
            {
                SetSelectedItem(19);
            }
            else if (selectedIndex == 19)
            {
                SetSelectedItem(6);
            }
            else if (selectedIndex == 20)
            {
                SetSelectedItem(3);
            }
            else if (selectedIndex > 21)
            {
                if (craftingSlots[selectedIndex + 1 - 22].transform.GetChild(1).GetComponent<SpriteRenderer>().color.a != 1)
                {
                    SetSelectedItem(14);
                    return;
                }
                else
                {
                    SetSelectedItem(selectedIndex + 1);
                }
            }
            else if (selectedIndex + 1 < 27)
            {
                SetSelectedItem(selectedIndex + 1);
            }

        }
        else if (input.x < 0) // Left
        {
            if (selectedIndex == 0)
            {
                SetSelectedItem(20);
            }
            else if (selectedIndex == 3 || selectedIndex == 6)
            {
                SetSelectedItem(19);
            }
            else if (selectedIndex == 9 || selectedIndex == 11)
            {
                SetSelectedItem(8);
            }
            else if (selectedIndex == 13)
            {
                SetSelectedItem(14);
            }
            else if (selectedIndex == 14)
            {
                SetSelectedItem(5);
            }
            else if (selectedIndex == 10)
            {
                SetSelectedItem(9);
            }
            else if (selectedIndex == 12)
            {
                SetSelectedItem(11);
            }
            else if (selectedIndex == 0)
            {
                SetSelectedItem(20);
            }
            else if (selectedIndex == 3 || selectedIndex == 6)
            {
                SetSelectedItem(19);
            }
            else if (selectedIndex == 20)
            {
                SetSelectedItem(15);
            }
            else if (selectedIndex == 19)
            {
                SetSelectedItem(16);
            }
            else if (selectedIndex == 15)
            {
                SetSelectedItem(21);
            }
            else if (selectedIndex == 16 || selectedIndex == 17)
            {
                SetSelectedItem(18);
            }
            else if (selectedIndex == 18)
            {
                SetSelectedItem(10);
            }
            else if (selectedIndex == 21)
            {
                SetSelectedItem(13);
            }
            else if (selectedIndex == 22)
            {
                SetSelectedItem(15);
            }
            else if (selectedIndex - 1 >= 0)
                SetSelectedItem(selectedIndex - 1);
        }
        else if (input.y < 0) // Down
        {
            if (selectedIndex == 6)
            {
                if (craftingSlots[1].transform.GetChild(0).GetComponent<SpriteRenderer>().color.a == 1)
                {
                    SetSelectedItem(23);
                }
                else if (craftingSlots[0].transform.GetChild(0).GetComponent<SpriteRenderer>().color.a == 1)
                {
                    SetSelectedItem(22);
                }
            }
            else if (selectedIndex == 7)
            {
                if (craftingSlots[3].transform.GetChild(0).GetComponent<SpriteRenderer>().color.a == 1)
                {
                    SetSelectedItem(24);
                }
                else
                {
                    SetSelectedItem(1);
                }
            }
            else if (selectedIndex == 8)
            {
                if (craftingSlots[4].transform.GetChild(0).GetComponent<SpriteRenderer>().color.a == 1)
                {
                    SetSelectedItem(25);
                }
                else
                {
                    SetSelectedItem(2);
                }
            }
            else if (selectedIndex == 9)
            {
                SetSelectedItem(11);
            }
            else if (selectedIndex == 10)
            {
                SetSelectedItem(12);
            }
            else if (selectedIndex == 11)
            {
                if (craftingSlots[4].transform.GetChild(1).GetComponent<SpriteRenderer>().color.a == 1)
                {
                    SetSelectedItem(26);
                }
                else
                {
                    SetSelectedItem(14);
                }
            }
            else if (selectedIndex == 12)
            {
                SetSelectedItem(13);
            }
            else if (selectedIndex == 13)
            {
                SetSelectedItem(10);
            }
            else if (selectedIndex == 14)
            {
                SetSelectedItem(9);
            }
            else if (selectedIndex == 15)
            {
                SetSelectedItem(16);
            }
            else if (selectedIndex == 16)
            {
                SetSelectedItem(17);
            }
            else if (selectedIndex == 17)
            {
                SetSelectedItem(15);
            }
            else if (selectedIndex == 20)
            {
                SetSelectedItem(19);
            }
            else if (selectedIndex == 19)
            {
                SetSelectedItem(22);
            }
            else if (selectedIndex == 21)
            {
                SetSelectedItem(18);
            }
            else if (selectedIndex == 18)
            {
                SetSelectedItem(21);
            }
            else if (selectedIndex + 3 < inventorySlotCount)
            {
                SetSelectedItem(selectedIndex + 3);
            }
            else if (selectedIndex == 22)
            {
                SetSelectedItem(0);
            }
            else if (selectedIndex == 23)
            {
                SetSelectedItem(0);
            }
            else if (selectedIndex == 24)
            {
                SetSelectedItem(1);
            }
            else if (selectedIndex == 25)
            {
                SetSelectedItem(2);
            }
        }
        else if (input.y > 0) // Up
        {
            if (selectedIndex == 0)
            {
                if (craftingSlots[1].transform.GetChild(0).GetComponent<SpriteRenderer>().color.a == 1)
                {
                    SetSelectedItem(23);
                }
                else if (craftingSlots[0].transform.GetChild(0).GetComponent<SpriteRenderer>().color.a == 1)
                {
                    SetSelectedItem(22);
                }
            }
            else if (selectedIndex == 1)
            {
                if (craftingSlots[2].transform.GetChild(0).GetComponent<SpriteRenderer>().color.a == 1)
                {
                    SetSelectedItem(24);
                }
                else
                {
                    SetSelectedItem(7);
                }
            }
            else if (selectedIndex == 2)
            {
                if (craftingSlots[3].transform.GetChild(0).GetComponent<SpriteRenderer>().color.a == 1)
                {
                    SetSelectedItem(25);
                }
                else
                {
                    SetSelectedItem(8);
                }
            }
            else if (selectedIndex == 9)
            {
                SetSelectedItem(14);
            }
            else if (selectedIndex == 10)
            {
                SetSelectedItem(13);
            }
            else if (selectedIndex == 11)
            {
                SetSelectedItem(9);
            }
            else if (selectedIndex == 12)
            {
                SetSelectedItem(10);
            }
            else if (selectedIndex == 13)
            {
                if (craftingSlots[4].transform.GetChild(1).GetComponent<SpriteRenderer>().color.a == 1)
                {
                    SetSelectedItem(26);
                }
                else
                {
                    SetSelectedItem(12);
                }
            }
            else if (selectedIndex == 14)
            {
                SetSelectedItem(13);
            }
            else if (selectedIndex == 17)
            {
                SetSelectedItem(16);
            }
            else if (selectedIndex == 16)
            {
                SetSelectedItem(15);
            }
            else if (selectedIndex == 15)
            {
                SetSelectedItem(17);
            }
            else if (selectedIndex == 18)
            {
                SetSelectedItem(21);
            }
            else if (selectedIndex == 21)
            {
                SetSelectedItem(18);
            }
            else if (selectedIndex == 19)
            {
                SetSelectedItem(20);
            }
            else if (selectedIndex == 20)
            {
                SetSelectedItem(22);
            }
            else if (selectedIndex == 22)
            {
                SetSelectedItem(6);
            }
            else if (selectedIndex == 23)
            {
                SetSelectedItem(6);
            }
            else if (selectedIndex == 24)
            {
                SetSelectedItem(7);
            }
            else if (selectedIndex == 25)
            {
                SetSelectedItem(8);
            }
            else if (selectedIndex - 3 >= 0)
            {
                SetSelectedItem(selectedIndex - 3);
            }
        }
        AdjustButtonPrompts();
    }

    private void SetSelectedItem(int idx)
    {
        selectedIndex = idx;
        for (int i = 0; i < 27; i++)
        {
            if (i == idx)
            {

                selectedItemSlot = UIRoot.transform.GetChild(i).gameObject;
                cursor.transform.position = UIRoot.transform.GetChild(i).position;
                Item item = null;
                if (idx < 9)
                {
                    UIRoot.transform.GetChild(i).transform.GetChild(2).GetComponent<TextMeshPro>().color = Color.gray;
                    if (items[idx].isEmpty == false)
                    {
                        item = items[idx].item;
                    }

                }
                else if (idx == 13)
                {
                    if (actorEquipment.equippedItem != null)
                    {
                        item = actorEquipment.equippedItem.GetComponent<Item>();
                    }
                }
                else if (idx > 14 && idx < 18)
                {
                    if (actorEquipment.equippedArmor[idx - 15] != null)
                    {
                        item = actorEquipment.equippedArmor[idx - 15].GetComponent<Item>();
                    }
                }

                if (item != null)
                {
                    string name = item.itemName;
                    string desc = item.itemDescription;
                    int damage = 0;
                    int armor = 0;
                    int energy = 0;
                    int health = 0;
                    int con = 0;
                    int dex = 0;
                    int intg = 0;
                    int str = 0;

                    if (item.TryGetComponent<ToolItem>(out var tool))
                    {
                        damage = tool.damage;
                        con = tool.conBonus;
                        dex = tool.dexBonus;
                        intg = tool.intBonus;
                        str = tool.strBonus;
                    }

                    if (item.TryGetComponent<Armor>(out var _armor))
                    {
                        armor = (int)_armor.m_DefenseValue;
                        con = _armor.conBonus;
                        dex = _armor.dexBonus;
                        intg = _armor.intBonus;
                        str = _armor.strBonus;
                    }

                    if (item.TryGetComponent<Food>(out var food))
                    {
                        if (food.hunger) energy = (int)food.foodValue;
                        if (food.health) health = (int)food.healthValue;
                    }
                    UpdateInfoPanel(name, desc, damage, armor, energy, health, con, str, dex, intg);
                }
                else
                {
                    UpdateInfoPanel("", "", 0, 0, 0, 0, 0, 0, 0, 0);
                }
            }
            else
            {
                UIRoot.transform.GetChild(i).gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = inventorySlotIcon;
                if (i < 9)
                {
                    UIRoot.transform.GetChild(i).transform.GetChild(2).GetComponent<TextMeshPro>().color = Color.white;
                }
            }
        }

    }

    public void DisplayItems()
    {
        for (int i = 0; i < items.Length; i++)
        {
            SpriteRenderer sr = UIRoot.transform.GetChild(i).GetChild(1).GetComponent<SpriteRenderer>();
            TextMeshPro tm = UIRoot.transform.GetChild(i).GetChild(2).GetComponent<TextMeshPro>();
            if (!items[i].isEmpty)
            {
                sr.sprite = items[i].item.icon;
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
                if (tm != null)
                {
                    tm.text = "";
                }
            }
        }
        for (int i = 0; i < beltItems.Length; i++)
        {
            SpriteRenderer sr = UIRoot.transform.GetChild(i + 9).GetChild(1).GetComponent<SpriteRenderer>();
            TextMeshPro tm = UIRoot.transform.GetChild(i + 9).GetChild(2).GetComponent<TextMeshPro>();
            if (!beltItems[i].isEmpty)
            {
                sr.sprite = beltItems[i].item.icon;
                if (beltItems[i].count > 1)
                {
                    if (tm != null)
                    {
                        tm.text = beltItems[i].count.ToString();
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
                if (tm != null)
                {
                    tm.text = "";
                }
            }
        }
        for (int i = 0; i < 3; i++)
        {
            SpriteRenderer armorSr = armorSlots[i].transform.GetChild(1).GetComponent<SpriteRenderer>();
            if (actorEquipment.equippedArmor[i] != null)
            {
                armorSr.sprite = actorEquipment.equippedArmor[i].GetComponent<Item>().icon;
            }
            else
            {
                switch (i)
                {
                    case 0:
                        armorSr.sprite = helmetInventorySlotIcon;
                        break;
                    case 1:
                        armorSr.sprite = chestInventorySlotIcon;
                        break;
                    case 2:
                        armorSr.sprite = legsInventorySlotIcon;
                        break;
                }
            }
        }
        SpriteRenderer equipmentSr = equipmentSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>();
        if (actorEquipment.equippedItem != null)
        {
            equipmentSr.sprite = actorEquipment.equippedItem.GetComponent<Item>().icon;
        }
        else
        {
            equipmentSr.sprite = weaponInventorySlotIcon;
        }
        SpriteRenderer pipeSprite = itemSlots[2].transform.GetChild(1).GetComponent<SpriteRenderer>();
        if (actorEquipment.equippedSpecialItems[2] != null)
        {
            pipeSprite.sprite = actorEquipment.equippedSpecialItems[2].GetComponent<Item>().icon;
        }
        else
        {
            pipeSprite.sprite = pipeInventorySlotIcon;
        }
        SpriteRenderer jewelrySprite = itemSlots[3].transform.GetChild(1).GetComponent<SpriteRenderer>();
        if (actorEquipment.equippedSpecialItems[3] != null)
        {
            jewelrySprite.sprite = actorEquipment.equippedSpecialItems[3].GetComponent<Item>().icon;
        }
        else
        {
            jewelrySprite.sprite = specialInventorySlotIcon;
        }
        SpriteRenderer capeSprite = itemSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>();
        if (actorEquipment.equippedSpecialItems[0] != null)
        {
            capeSprite.sprite = actorEquipment.equippedSpecialItems[0].GetComponent<Item>().icon;
        }
        else
        {
            capeSprite.sprite = capeInventorySlotIcon;
        }
        SpriteRenderer utilitySprite = itemSlots[1].transform.GetChild(1).GetComponent<SpriteRenderer>();
        if (actorEquipment.equippedSpecialItems[1] != null)
        {
            utilitySprite.sprite = actorEquipment.equippedSpecialItems[1].GetComponent<Item>().icon;
        }
        else
        {
            utilitySprite.sprite = utilityInventorySlotIcon;
        }
        if (mouseCursor.activeSelf)
        {

            if (mouseCursorStack.isEmpty)
            {
                mouseCursor.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = null;
                mouseCursor.transform.GetChild(1).GetComponent<TMP_Text>().text = "";
            }
            else
            {
                mouseCursor.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = mouseCursorStack.item.icon;
                mouseCursor.transform.GetChild(1).GetComponent<TMP_Text>().text = mouseCursorStack.count.ToString();
            }
        }

        if (cursorStack.isEmpty)
        {
            cursor.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = null;
            cursor.transform.GetChild(1).GetComponent<TMP_Text>().text = "";
        }
        else
        {
            cursor.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = cursorStack.item.icon;
            cursor.transform.GetChild(1).GetComponent<TMP_Text>().text = cursorStack.count.ToString();
        }
        UpdateButtonPrompts();
        m_CharacterManager.SaveCharacter();
    }

    public void ToggleInventoryUI()
    {
        if (isActive)
        {
            if (!cursorStack.isEmpty)
            {
                int slot = FirstAvailableSlot();
                if (slot == -1)
                {
                    for (int i = 0; i < cursorStack.count; i++)
                    {
                        DropItem(cursorStack.item.itemListIndex, transform.position);
                    }
                }
                else
                {
                    items[slot] = new(cursorStack.item, cursorStack.count, slot, false);
                }
                SetSelectedItem(5);
                cursorStack = new();
            }
            if (!mouseCursorStack.isEmpty)
            {
                int slot = FirstAvailableSlot();
                if (slot == -1)
                {
                    for (int i = 0; i < mouseCursorStack.count; i++)
                    {
                        DropItem(mouseCursorStack.item.itemListIndex, transform.position);
                    }
                }
                else
                {
                    items[slot] = new(mouseCursorStack.item, mouseCursorStack.count, slot, false);
                }
                SetSelectedItem(5);
                mouseCursorStack = new();
            }
            DisplayItems();
        }
        isActive = !isActive;
        UIRoot.SetActive(isActive);
        DisplayItems();

    }

    public bool AddItem(Item _item, int count)
    {
        // Check for an existing stack of the item in the inventory
        for (int i = 0; i < items.Length + beltItems.Length; i++)
        {
            ItemStack stack = i < 9 ? items[i] : beltItems[i - 9];
            if (!stack.isEmpty && stack.item.itemName == _item.itemName)
            {
                // If a stack is found, increase the count and update UI
                stack.count += count;
                //GameObject.Destroy(_item);
                DisplayItems(); // Update the inventory UI
                return true;
            }
        }

        // If no existing stack is found, find the first available slot
        int index = FirstAvailableSlot();
        if (index == -1)
        {
            // No available slots in inventory
            return false;
        }

        // Create a new stack in the first available slot
        ItemStack newStack = new ItemStack(_item, count, index, false);
        newStack.item.inventoryIndex = index;
        if (index < 9)
        {
            items[index] = newStack;

        }
        else
        {
            beltItems[index] = newStack;
        }
        DisplayItems(); // Update the inventory UI
        return true;
    }


    private int FirstAvailableSlot()
    {
        for (int i = 0; i < items.Length + beltItems.Length; i++)
        {
            ItemStack stack = i < 9 ? items[i] : beltItems[i - 9];
            if (stack.isEmpty)
            {
                return i;
            }
        }
        return -1; // No available slots
    }
    private int FirstAvailableBeltSlot()
    {
        for (int i = 0; i < beltItems.Length; i++)
        {
            if (beltItems[i].isEmpty)
            {
                return i;
            }
        }
        return -1; // No available slots
    }
    public int RemoveItem(int idx, int count)
    {
        ItemStack stack = idx < 9 ? items[idx] : beltItems[idx - 9];
        // Check if the item is in the inventory

        if (!stack.isEmpty)
        {
            // If the item is in the inventory, remove from the stack count
            stack.count -= count;
            if (stack.count < 1)
            {
                if (idx < 9)
                {
                    items[idx] = new ItemStack(null, 0, idx, true);
                }
                else
                {
                    if (selectedBeltItem == idx - 9)
                    {
                        actorEquipment.UnequippedCurrentItem();
                    }
                    beltItems[idx - 9] = new ItemStack(null, 0, idx, true);
                }
                // If the stack count becomes zero or negative, remove the stack from the inventory
                UpdateInfoPanel("", "", 0, 0, 0, 0, 0, 0, 0, 0);
            }
        }
        DisplayItems();
        if (idx < 9)
        {
            return items[idx].count;
        }
        else
        {
            return beltItems[idx - 9].count;
        }
    }
    public int RemoveBeltItem(int idx, int count)
    {
        ItemStack stack = beltItems[idx];
        // Check if the item is in the inventory

        if (!stack.isEmpty)
        {
            // If the item is in the inventory, remove from the stack count
            stack.count -= count;

            // If the stack count becomes zero or negative, remove the stack from the inventory
            if (stack.count < 1)
            {
                beltItems[idx] = new ItemStack(null, 0, idx, true);
                UpdateInfoPanel("", "", 0, 0, 0, 0, 0, 0, 0, 0);
                actorEquipment.UnequippedCurrentItem();
            }
        }
        DisplayItems();
        return beltItems[idx].count;
    }
    public bool AddBeltItem(Item _item, int count)
    {
        // Check for an existing stack of the item in the inventory
        foreach (ItemStack stack in beltItems)
        {
            if (!stack.isEmpty && stack.item.itemName == _item.itemName)
            {
                // If a stack is found, increase the count and update UI
                stack.count += count;
                //GameObject.Destroy(_item);
                DisplayItems(); // Update the inventory UI
                return true;
            }
        }

        // If no existing stack is found, find the first available slot
        int index = FirstAvailableBeltSlot();
        if (index == -1)
        {
            // No available slots in inventory
            return false;
        }

        // Create a new stack in the first available slot
        ItemStack newStack = new ItemStack(_item, count, index, false);
        newStack.item.inventoryIndex = index;
        beltItems[index] = newStack;
        beltItems[index].item.isBeltItem = true;
        //GameObject.Destroy(_item.gameObject);
        DisplayItems(); // Update the inventory UI
        return true;
    }

    public void AdjustButtonPrompts()
    {
        if (!LevelPrep.Instance.settingsConfig.showOnScreenControls) return;
        if (craftingProduct == null)
        {
            if (selectedIndex > 8)
            {
                //Handle Equipment prompts
                if (cursorStack.isEmpty)
                {
                    //Show Unequip
                    buttonPrompts[14].SetActive(true);
                    buttonPrompts[10].SetActive(true);
                    // Turn everything else off
                    buttonPrompts[9].SetActive(false);
                    buttonPrompts[1].SetActive(false);
                    buttonPrompts[2].SetActive(false);
                    buttonPrompts[7].SetActive(false);
                    buttonPrompts[8].SetActive(false);
                    buttonPrompts[9].SetActive(false);
                    buttonPrompts[11].SetActive(false);
                    buttonPrompts[12].SetActive(false);
                    buttonPrompts[13].SetActive(false);
                    buttonPrompts[15].SetActive(false);

                }
                else
                {
                    //Show Equip
                    buttonPrompts[2].SetActive(true);
                    buttonPrompts[11].SetActive(true);
                    // Turn everything else off
                    buttonPrompts[9].SetActive(false);
                    buttonPrompts[14].SetActive(false);
                    buttonPrompts[1].SetActive(false);
                    buttonPrompts[7].SetActive(false);
                    buttonPrompts[8].SetActive(false);
                    buttonPrompts[9].SetActive(false);
                    buttonPrompts[12].SetActive(false);
                    buttonPrompts[13].SetActive(false);
                    buttonPrompts[15].SetActive(false);
                    buttonPrompts[10].SetActive(false);

                }
            }
            else
            {
                //Handle regular inventory spaces
                if (cursorStack.isEmpty)
                {
                    // show select stack
                    // show select single
                    buttonPrompts[7].SetActive(true);
                    buttonPrompts[15].SetActive(true);
                    // Turn everything else off
                    buttonPrompts[2].SetActive(false);
                    buttonPrompts[11].SetActive(false);
                    buttonPrompts[9].SetActive(false);
                    buttonPrompts[14].SetActive(false);
                    buttonPrompts[1].SetActive(false);
                    buttonPrompts[8].SetActive(false);
                    buttonPrompts[9].SetActive(false);
                    buttonPrompts[12].SetActive(false);
                    buttonPrompts[10].SetActive(false);
                    buttonPrompts[13].SetActive(false);


                }
                else
                {
                    if (items[selectedIndex].isEmpty)
                    {
                        //show place stack
                        // show place single
                        buttonPrompts[12].SetActive(true);
                        buttonPrompts[8].SetActive(true);
                        // Turn everything else off
                        buttonPrompts[1].SetActive(false);
                        buttonPrompts[2].SetActive(false);
                        buttonPrompts[7].SetActive(false);
                        buttonPrompts[9].SetActive(false);
                        buttonPrompts[10].SetActive(false);
                        buttonPrompts[11].SetActive(false);
                        buttonPrompts[13].SetActive(false);
                        buttonPrompts[14].SetActive(false);
                        buttonPrompts[15].SetActive(false);
                    }
                    else
                    {
                        //Show swap
                        buttonPrompts[9].SetActive(true);
                        // Turn everything else off
                        buttonPrompts[13].SetActive(false);
                        buttonPrompts[15].SetActive(false);
                        buttonPrompts[8].SetActive(false);
                        buttonPrompts[7].SetActive(false);
                        buttonPrompts[2].SetActive(false);
                        buttonPrompts[11].SetActive(false);
                        buttonPrompts[14].SetActive(false);
                        buttonPrompts[1].SetActive(false);
                        buttonPrompts[12].SetActive(false);
                        buttonPrompts[10].SetActive(false);
                    }
                }
            }
        }
        else if (craftingProduct != null)
        {
            buttonPrompts[1].SetActive(true);
            buttonPrompts[13].SetActive(false);
            buttonPrompts[9].SetActive(false);
            // Turn everything else off
            buttonPrompts[15].SetActive(false);
            buttonPrompts[8].SetActive(false);
            buttonPrompts[7].SetActive(false);
            buttonPrompts[2].SetActive(false);
            buttonPrompts[11].SetActive(false);
            buttonPrompts[14].SetActive(false);
            buttonPrompts[9].SetActive(false);
            buttonPrompts[12].SetActive(false);
        }

        if (isCrafting)
        {
            buttonPrompts[5].SetActive(false);
            buttonPrompts[6].SetActive(true);
        }
        else
        {
            buttonPrompts[5].SetActive(true);
            buttonPrompts[6].SetActive(false);
        }
    }
}

public class ItemStack : MonoBehaviour
{
    public Item item;
    public int count;
    public int index;
    public bool isEmpty;

    public ItemStack(Item item, int count, int index, bool isEmpty = true)
    {
        this.item = item;
        this.count = count;
        this.index = index;
        this.isEmpty = isEmpty;
    }
    public ItemStack(ItemStack item)
    {
        this.item = item.item;
        this.count = item.count;
        this.index = item.index;
        this.isEmpty = item.isEmpty;
    }
    public ItemStack()
    {
        this.item = null;
        this.count = 0;
        this.index = -1;
        this.isEmpty = true;
    }
}

public class BeastGearStack : MonoBehaviour
{
    public BeastGear beastGear;
    public int count;
    public int index;
    public bool isEmpty;

    public BeastGearStack(BeastGear beastGear, int count, int index, bool isEmpty = true)
    {
        this.beastGear = beastGear;
        this.count = count;
        this.index = index;
        this.isEmpty = isEmpty;
    }
    public BeastGearStack(BeastGearStack beastGear)
    {
        this.beastGear = beastGear.beastGear;
        this.count = beastGear.count;
        this.index = beastGear.index;
        this.isEmpty = beastGear.isEmpty;
    }
    public BeastGearStack()
    {
        this.beastGear = null;
        this.count = 0;
        this.index = -1;
        this.isEmpty = true;
    }
}

