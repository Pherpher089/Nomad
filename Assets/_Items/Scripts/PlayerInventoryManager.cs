using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Photon.Realtime;
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
    public Sprite selectedItemIcon;
    private List<int> currentIngredients;
    private int item1Index, item2Index;
    public ActorEquipment actorEquipment;
    public int[] ingredients;
    public GameObject[] craftingSlots;
    public GameObject[] equipmentSlots;
    public GameObject infoPanel;
    private int craftingItemCount = 0;
    private CraftingManager craftingManager;
    public bool isCrafting;
    private ItemManager m_ItemManager;
    private CharacterManager m_CharacterManager;
    private GameObject[] craftingProduct;
    PlayersManager playersManager;
    public GameObject[] buttonPrompts;

    void Awake()
    {
        Instance = this;
        craftingProduct = null;
        playersManager = FindObjectOfType<PlayersManager>();
        craftingSlots = new GameObject[5];
        equipmentSlots = new GameObject[1];
        currentIngredients = new List<int>();
        inventorySlotIcon = Resources.Load<Sprite>("Sprites/InventorySlot");
        selectedItemIcon = Resources.Load<Sprite>("Sprites/SelectedInventorySlot");
        actorEquipment = GetComponent<ActorEquipment>();
        UIRoot = transform.GetChild(1).gameObject;
        UpdateButtonPrompts();
        items = new ItemStack[9];
        craftingManager = GameObject.FindWithTag("GameController").GetComponent<CraftingManager>();
        m_CharacterManager = GetComponent<CharacterManager>();
        for (int i = 0; i < items.Length; i++)
        {
            items[i] = new ItemStack(null, 1, i, true);
        }
        for (int i = 0; i < 5; i++)
        {
            craftingSlots[i] = UIRoot.transform.GetChild(10 + i).gameObject;
            craftingSlots[i].SetActive(false);
        }
        for (int i = 0; i < 1; i++)
        {
            equipmentSlots[i] = UIRoot.transform.GetChild(9 + i).gameObject;
        }

        infoPanel = UIRoot.transform.GetChild(UIRoot.transform.childCount - 2).gameObject;
        m_ItemManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<ItemManager>();
        SetSelectedItem(4);

    }
    public void UpdateButtonPrompts()
    {
        if (GetComponent<ThirdPersonUserControl>().playerPrefix == "sp")
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
    }
    public void UpdateUiWithEquippedItem(Sprite icon)
    {
        //todo need to handle the armor slots on the left as well
        equipmentSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = icon;
    }
    public void AddIngredient()
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
                GameObject newItem = Instantiate(craftingProduct[0], null);
                bool didAdd = AddItem(newItem.GetComponent<Item>(), craftingProduct.Length);
                if (!didAdd)
                {
                    Instantiate(craftingProduct[0], transform.forward + transform.up, Quaternion.identity);
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
    }
    public void SpendItem(Item item)
    {
        int itemIndex = FindItemInInventory(item);
        if (itemIndex >= 0)
        {
            RemoveItem(itemIndex, 1);
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

    public void InventoryActionButton()
    {
        if (isCrafting && craftingProduct != null)
        {
            Craft();
        }
        else
        {

        }
        int slotIndex = selectedItemSlot.transform.GetSiblingIndex();
        if (selectedIndex > 8)
        {
            return;
        }

        if (!items[slotIndex].isEmpty)
        {
            if (items[slotIndex].item.TryGetComponent<Armor>(out var armor))
            {
                if (actorEquipment.equippedArmor[(int)armor.m_ArmorType] != null)
                {
                    actorEquipment.UnequippedCurrentArmorToInventory(armor.m_ArmorType);
                }
                actorEquipment.EquipItem(items[slotIndex].item);
                RemoveItem(slotIndex, 1);

            }
            else
            {
                if (actorEquipment.hasItem)
                {
                    actorEquipment.UnequippedCurrentItemToInventory();
                    equipmentSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = inventorySlotIcon;
                }
                if (actorEquipment.hasItem && items[slotIndex].item.itemName == actorEquipment.equippedItem.GetComponent<Item>().itemName)
                {
                    actorEquipment.UnequippedCurrentItemToInventory();
                    equipmentSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = inventorySlotIcon;
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
            actorEquipment.UnequippedCurrentItemToInventory();
        }
    }

    public void UpdateInfoPanel(string name, string description, int value, int damage = 0)
    {
        infoPanel.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = name;
        infoPanel.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = description;
        infoPanel.transform.GetChild(1).GetChild(2).GetComponent<TextMeshProUGUI>().text = damage != 0 ? $"Damage: {damage}" : "";
        infoPanel.transform.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text = value != 0 ? $"{value}Gp" : "";

    }

    public void DropItem()
    {
        if (items[selectedIndex].isEmpty == false)
        {
            if (items[selectedIndex].count > 0)
            {
                //Call Prc on ItemsManager
                // Instantiate(m_ItemManager.GetPrefabByItem(items[selectedIndex].item), transform.position + transform.forward + transform.up, Quaternion.identity);
                ItemManager.Instance.CallDropItemRPC(items[selectedIndex].item.itemIndex, transform.position);
                RemoveItem(selectedIndex, 1);
            }
        }
    }
    public void DropItem(int itemIndex, Vector3 pos)
    {
        ItemManager.Instance.CallDropItemRPC(itemIndex, pos);
    }

    public void MoveSelection(Vector2 input)
    {
        if (input.x > 0)
        {
            if (selectedIndex == 2 || selectedIndex == 5 || selectedIndex == 8)
            {
                SetSelectedItem(9);
            }
            else if (selectedIndex == 9)
            {
                SetSelectedItem(0);
            }
            if (selectedIndex + 1 < inventorySlotCount)
                SetSelectedItem(selectedIndex + 1);
        }
        else if (input.x < 0)
        {
            if (selectedIndex == 0 || selectedIndex == 3 || selectedIndex == 6)
            {
                SetSelectedItem(9);
            }
            else if (selectedIndex == 9)
            {
                SetSelectedItem(2);
            }
            if (selectedIndex - 1 >= 0)
                SetSelectedItem(selectedIndex - 1);
        }
        else if (input.y < 0)
        {
            if (selectedIndex == 9)
            {
                return;
            }
            if (selectedIndex + 3 < inventorySlotCount)
                SetSelectedItem(selectedIndex + 3);
        }
        else if (input.y > 0)
        {
            if (selectedIndex == 9)
            {
                return;
            }
            if (selectedIndex - 3 >= 0)
                SetSelectedItem(selectedIndex - 3);
        }
    }

    private void SetSelectedItem(int idx)
    {
        selectedIndex = idx;
        for (int i = 0; i < items.Length + equipmentSlots.Length; i++)
        {
            if (i == idx)
            {
                selectedItemSlot = UIRoot.transform.GetChild(i).gameObject;
                UIRoot.transform.GetChild(i).gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = selectedItemIcon;
                UIRoot.transform.GetChild(i).transform.GetChild(2).GetComponent<TextMeshPro>().color = Color.gray;
                if (items[idx].isEmpty == false)
                {
                    UpdateInfoPanel(items[idx].item.itemName, items[idx].item.itemDescription, items[idx].item.value, 0);
                }
                else
                {
                    UpdateInfoPanel("", "", 0, 0);
                }
            }
            else
            {
                UIRoot.transform.GetChild(i).gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = inventorySlotIcon;
                if (i != 9)
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
        AdjustButtonPrompts();
        //m_CharacterManager.SaveCharacter();
    }

    public void ToggleInventoryUI()
    {
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


    public void RemoveItem(int idx, int count)
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
                UpdateInfoPanel("", "", 0, 0);
            }
        }
        DisplayItems();
    }

    public void AdjustButtonPrompts()
    {
        if (craftingProduct != null)
        {
            buttonPrompts[1].SetActive(true);
            buttonPrompts[2].SetActive(false);
        }
        else
        {
            buttonPrompts[1].SetActive(false);
            buttonPrompts[2].SetActive(true);
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

    public ItemStack(Item item, int count, int index, bool isEmpty = false)
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
}

