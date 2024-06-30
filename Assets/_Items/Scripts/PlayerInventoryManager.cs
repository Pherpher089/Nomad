using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerInventoryManager : MonoBehaviour
{
    public static PlayerInventoryManager Instance;
    public ItemStack[] items;
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
    ActorAudioManager audioManager;

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
        selectedItemIcon = Resources.Load<Sprite>("Sprites/SelectedInventorySlot");
        actorEquipment = GetComponent<ActorEquipment>();
        UIRoot = transform.GetChild(1).gameObject;
        items = new ItemStack[9];
        craftingManager = GameObject.FindWithTag("GameController").GetComponent<CraftingManager>();
        m_CharacterManager = GetComponent<CharacterManager>();
        audioManager = GetComponent<ActorAudioManager>();

        //Initialize items object
        for (int i = 0; i < items.Length; i++)
        {
            items[i] = new ItemStack(null, 1, i, true);
        }
        // Get Crafting Slots
        for (int i = 0; i < 5; i++)
        {
            craftingSlots[i] = UIRoot.transform.GetChild(22 + i).gameObject;
            craftingSlots[i].SetActive(false);
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
        cursor = UIRoot.transform.GetChild(UIRoot.transform.childCount - 4).gameObject;
        cursorStack = new ItemStack();
        m_ItemManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<ItemManager>();
        SetSelectedItem(5);
        UpdateButtonPrompts();

    }
    void Start()
    {
        UpdateQuickStats();
    }
    void Update()
    {
        UpdateQuickStats();
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
    public void AddIngredient()
    {
        if (selectedIndex > 8) return;
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
            craftingSlots[currentIngredients.Count - 1].SetActive(true);
            craftingSlots[currentIngredients.Count - 1].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = items[selectedIndex].item.icon;
            RemoveItem(selectedIndex, 1);

        }
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
    public bool Craft()
    {
        if (craftingProduct != null)
        {

            BuildingMaterial buildMat = craftingProduct[0].GetComponent<BuildingMaterial>();
            //we are checking to see if an auto build object was crafted like a crafting bench or camp fire;
            if (buildMat != null && !buildMat.fitsInBackpack)
            {
                ToggleInventoryUI();
                GetComponent<BuilderManager>().Build(GetComponent<ThirdPersonUserControl>(), buildMat);
            }
            else
            {
                bool wasItemAdded = AddItem(craftingProduct[0].GetComponent<Item>(), craftingProduct.Length);
                if (!wasItemAdded)
                {
                    DropItem(craftingProduct[0].GetComponent<Item>().itemListIndex, transform.position + Vector3.up * 2);
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
                AddItem(m_ItemManager.itemList[itemIndex].GetComponent<Item>(), 1);
            }
        }
        foreach (GameObject slot in craftingSlots)
        {
            slot.SetActive(false);
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
            if (remainingCount == 0)
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
    public void InventoryActionButton(bool primary, bool secondary)
    {
        if (isCrafting && craftingProduct != null && primary)
        {
            Craft();
            return;
        }

        if (selectedIndex <= 8)
        {
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

        if (selectedIndex > 8)
        {
            if (cursorStack.isEmpty)
            {
                switch (selectedIndex)
                {
                    case 13:
                        if (actorEquipment.hasItem)
                        {
                            cursorStack = new ItemStack(actorEquipment.equippedItem.GetComponent<Item>(), 1, -1, false);
                            equipmentSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = weaponInventorySlotIcon;
                            actorEquipment.UnequippedCurrentItem();
                        }
                        break;
                    case 15:
                        if (actorEquipment.equippedArmor[(int)ArmorType.Helmet] != null)
                        {
                            cursorStack = new ItemStack(actorEquipment.equippedArmor[(int)ArmorType.Helmet].GetComponent<Item>(), 1, -1, false);
                            armorSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = helmetInventorySlotIcon;
                            actorEquipment.UnequippedCurrentArmor(ArmorType.Helmet);
                        }
                        break;
                    case 16:
                        if (actorEquipment.equippedArmor[(int)ArmorType.Chest] != null)
                        {
                            cursorStack = new ItemStack(actorEquipment.equippedArmor[(int)ArmorType.Chest].GetComponent<Item>(), 1, -1, false);
                            armorSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = chestInventorySlotIcon;
                            actorEquipment.UnequippedCurrentArmor(ArmorType.Chest);
                        }
                        break;
                    case 17:
                        if (actorEquipment.equippedArmor[(int)ArmorType.Legs] != null)
                        {
                            cursorStack = new ItemStack(actorEquipment.equippedArmor[(int)ArmorType.Legs].GetComponent<Item>(), 1, -1, false);
                            armorSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = legsInventorySlotIcon;
                            actorEquipment.UnequippedCurrentArmor(ArmorType.Legs);
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
                            if (cursorStack.count > 1 && cursorStack.item.itemListIndex != actorEquipment.equippedItem.GetComponent<Item>().itemListIndex)
                            {
                                TryUnequippedItem();
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
                                TryUnequippedItem();
                                actorEquipment.EquipItem(cursorStack.item);
                                equipmentSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = cursorStack.item.icon;

                                cursorStack = new(temp, 1, -1, false);

                            }

                        }
                        else
                        {
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
                    default:
                        break;
                }
                AdjustButtonPrompts();
                DisplayItems();
                return;
            }

        }

        DisplayItems();
    }

    public void EquipFromInventory(int slotIndex)
    {
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

    private void TryUnequippedItem()
    {
        bool canUnequipped = actorEquipment.UnequippedCurrentItemToInventory();
        if (!canUnequipped)
        {
            int itemIndex = actorEquipment.equippedItem.GetComponent<Item>().itemListIndex;
            actorEquipment.UnequippedCurrentItem();
            DropItem(itemIndex, transform.position);
        };
    }

    private void TryUnequippedArmor(ArmorType armorType)
    {
        bool canUnequipped = actorEquipment.UnequippedCurrentArmorToInventory(armorType);
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
        quickStatsPanel.transform.GetChild(1).GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{m_CharacterManager.stats.health.ToString("F1")}/{m_CharacterManager.health.health.ToString("F1")}";
        quickStatsPanel.transform.GetChild(1).GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{m_CharacterManager.stats.stomachCapacity.ToString("F1")}/{m_CharacterManager.hunger.m_StomachValue.ToString("F1")}";
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
        if (items[selectedIndex].isEmpty == false)
        {
            if (items[selectedIndex].count > 0)
            {
                //Call Prc on ItemsManager
                ItemManager.Instance.CallDropItemRPC(items[selectedIndex].item.itemListIndex, transform.position);
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

            else if (selectedIndex + 1 < inventorySlotCount)
                SetSelectedItem(selectedIndex + 1);
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
            else if (selectedIndex == 2 || selectedIndex == 6)
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
            else if (selectedIndex - 1 >= 0)
                SetSelectedItem(selectedIndex - 1);
        }
        else if (input.y < 0) // Down
        {
            if (selectedIndex == 9)
            {
                SetSelectedItem(11);
            }
            else if (selectedIndex == 10)
            {
                SetSelectedItem(12);
            }
            else if (selectedIndex == 11)
            {
                SetSelectedItem(14);
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
                SetSelectedItem(20);
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
        }
        else if (input.y > 0) // Up
        {
            if (selectedIndex == 9)
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
                SetSelectedItem(12);
            }
            else if (selectedIndex == 14)
            {
                SetSelectedItem(11);
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
                SetSelectedItem(19);
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
        for (int i = 0; i < 22; i++)
        {
            if (i == idx)
            {

                selectedItemSlot = UIRoot.transform.GetChild(i).gameObject;
                cursor.transform.position = UIRoot.transform.GetChild(i).position;
                Item item = null;
                if (idx < 9)
                {
                    Debug.Log("### item section" + idx);

                    UIRoot.transform.GetChild(i).transform.GetChild(2).GetComponent<TextMeshPro>().color = Color.gray;
                    if (items[idx].isEmpty == false)
                    {
                        item = items[idx].item;
                    }

                }
                else if (idx == 13)
                {
                    Debug.Log("### tool section" + idx);

                    if (actorEquipment.equippedItem != null)
                    {
                        Debug.Log("### tool not null" + idx);

                        item = actorEquipment.equippedItem.GetComponent<Item>();
                    }
                }
                else if (idx > 14 && idx < 18)
                {
                    Debug.Log("### armor section" + idx);
                    if (actorEquipment.equippedArmor[idx - 15] != null)
                    {
                        Debug.Log("### armor not null" + idx);

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
        }
        isActive = !isActive;
        UIRoot.SetActive(isActive);
        if (isActive == true)
        {
            DisplayItems();
        }
    }

    public bool AddItem(Item _item, int count)
    {
        // Check for an existing stack of the item in the inventory
        foreach (ItemStack stack in items)
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
        int index = FirstAvailableSlot();
        if (index == -1)
        {
            // No available slots in inventory
            return false;
        }

        // Create a new stack in the first available slot
        ItemStack newStack = new ItemStack(_item, count, index, false);
        newStack.item.inventoryIndex = index;
        items[index] = newStack;
        //GameObject.Destroy(_item.gameObject);
        DisplayItems(); // Update the inventory UI
        return true;
    }

    private int FirstAvailableSlot()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].isEmpty)
            {
                return i;
            }
        }
        return -1; // No available slots
    }


    public int RemoveItem(int idx, int count)
    {
        ItemStack stack = items[idx];
        // Check if the item is in the inventory

        if (!stack.isEmpty)
        {
            // If the item is in the inventory, remove from the stack count
            stack.count -= count;

            // If the stack count becomes zero or negative, remove the stack from the inventory
            if (stack.count < 1)
            {
                items[idx] = new ItemStack(null, 0, idx, true);
                UpdateInfoPanel("", "", 0, 0, 0, 0, 0, 0, 0, 0);
            }
        }
        DisplayItems();
        return items[idx].count;
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

