using UnityEngine;
using TMPro;

public class PlayerInventoryManager : MonoBehaviour
{
    public ItemStack[] items;
    public GameObject UIRoot;
    private GameObject selectedItemSlot;
    public bool isActive = false;
    private int selectedIndex = 4;
    private int inventorySlotCount = 9;
    public Sprite inventorySlotIcon;
    public Sprite selectedItemIcon;
    private GameObject item1, item2;
    private int item1Index, item2Index;
    public ActorEquipment actorEquipment;
    public GameObject[] craftingSlots = new GameObject[2];
    public GameObject[] equipmentSlots = new GameObject[1];
    public GameObject infoPanel;
    private int craftingItemCount = 0;
    private CraftingManager craftingManager;
    public bool isCrafting;
    private ItemManager m_ItemManager;
    private CharacterManager m_CharacterManager;

    void Start()
    {
        inventorySlotIcon = Resources.Load<Sprite>("Sprites/InventorySlot");
        selectedItemIcon = Resources.Load<Sprite>("Sprites/SelectedInventorySlot");
        actorEquipment = GetComponent<ActorEquipment>();
        items = new ItemStack[9];
        craftingManager = GameObject.FindWithTag("GameController").GetComponent<CraftingManager>();
        m_CharacterManager = GetComponent<CharacterManager>();
        for (int i = 0; i < items.Length; i++)
        {
            items[i] = new ItemStack(null, 1, i, true);
        }
        for (int i = 0; i < 2; i++)
        {
            craftingSlots[i] = UIRoot.transform.GetChild(10 + i).gameObject;
            craftingSlots[i].SetActive(false);
        }
        for (int i = 0; i < 1; i++)
        {
            equipmentSlots[i] = UIRoot.transform.GetChild(9 + i).gameObject;
        }

        infoPanel = UIRoot.transform.GetChild(UIRoot.transform.childCount - 1).gameObject;
        m_ItemManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<ItemManager>();
        UIRoot = transform.GetChild(1).gameObject;
        SetSelectedItem(4);
    }
    public void UpdateUiWithEquippedItem(Sprite icon)
    {
        if (equipmentSlots.Length > 0)
        {
            equipmentSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = icon;
        }
        else
        {
            equipmentSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = actorEquipment.equippedItem.GetComponent<Item>().icon;
            UpdateUiWithEquippedItem(icon);
        }
    }
    public void Craft()
    {
        if (items[selectedIndex].isEmpty && craftingItemCount > 2)
        {
            return;
        }
        if (craftingItemCount < 2)
        {
            isCrafting = true;

            if (craftingItemCount < 1)
            {
                craftingSlots[0].SetActive(true);
                craftingSlots[0].transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().sprite = items[selectedIndex].item.icon;
                item1 = items[selectedIndex].item.gameObject;
                item1Index = selectedIndex;
                craftingItemCount++;
                RemoveItem(selectedIndex, 1);
            }
            else if (craftingItemCount < 2)
            {
                craftingSlots[1].SetActive(true);
                craftingSlots[1].transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().sprite = items[selectedIndex].item.icon;
                item2 = items[selectedIndex].item.gameObject;
                item2Index = selectedIndex;
                craftingItemCount++;
                RemoveItem(selectedIndex, 1);
            }

        }
        else if (craftingItemCount >= 2)
        {
            GameObject craftedItem = craftingManager.TryCraft(item1, item2);
            if (craftedItem == null)
            {
                CancelCraft();
            }
            else
            {
                GameObject obj = Instantiate(craftedItem, transform.position + Vector3.forward + Vector3.up, Quaternion.identity);
                // actorEquipment.EquipItem(obj.GetComponent<Item>());

                craftingSlots[0].SetActive(false);
                craftingSlots[1].SetActive(false);
                craftingItemCount = 0;
                isCrafting = false;
            }
        }
        m_CharacterManager.SaveCharacter();
    }

    public void CancelCraft()
    {
        if (craftingItemCount > 1)
        {
            AddItem(item1.GetComponent<Item>(), 1);
            AddItem(item2.GetComponent<Item>(), 1);
        }
        else
        {
            AddItem(item1.GetComponent<Item>(), 1);
        }
        isCrafting = false;
        craftingSlots[0].SetActive(false);
        craftingSlots[1].SetActive(false);
        craftingItemCount = 0;
    }

    public void EquipSelection()
    {
        int slotIndex = selectedItemSlot.transform.GetSiblingIndex();
        if (actorEquipment.hasItem)
        {
            actorEquipment.UnequippedToInventory();
            equipmentSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = null;
        }
        if (selectedIndex > 8)
        {
            return;
        }
        if (!items[slotIndex].isEmpty)
        {
            Debug.Log("$$$ Here " + slotIndex + " " + selectedIndex);

            actorEquipment.EquipItem(items[slotIndex].item);
            equipmentSlots[0].transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = items[slotIndex].item.icon;
            RemoveItem(slotIndex, 1);
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
                Instantiate(m_ItemManager.GetPrefabByItem(items[selectedIndex].item), transform.position + transform.forward + transform.up, Quaternion.identity);
                RemoveItem(selectedIndex, 1);
            }
        }
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
            }
        }
        m_CharacterManager.SaveCharacter();
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

    public void AddItem(Item _item, int count)
    {
        int index = FirstAvailableSlot();
        Item item = _item;
        item.inventoryIndex = index;
        ItemStack stack = new ItemStack(item, count, index, false);
        bool hasItem = false;
        // Check if the item is already in the inventory
        for (int i = 0; i < items.Length; i++)
        {
            if (!items[i].isEmpty && items[i].item.itemName == item.itemName)
            {
                hasItem = true;
                stack = items[i];
            }
        }
        // If the item already exists in a stack, increment the stack value
        if (hasItem)
        {

            // If the item is already in the inventory, add to the stack count
            stack.count += count;
        }
        else
        {

            // If the item is not in the inventory, add a new stack
            stack.item.inventoryIndex = index;
            items[index] = stack;

            //make sure that the item equipped has the correct item index as it's stack
            if (actorEquipment.hasItem)
            {
                actorEquipment.equippedItem.GetComponent<Item>().inventoryIndex = index;
            }
            else
            {
                if (actorEquipment.hasItem) actorEquipment.equippedItem.GetComponent<Item>().inventoryIndex = -1;
            }

        }

        // reprint items into inventory
        DisplayItems();

    }

    private int FirstAvailableSlot()
    {
        //Finds the first empty slot in the inventory
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].isEmpty)
            {
                return i;
            }
        }

        return -1;
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
}

