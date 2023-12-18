using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using TMPro;
using UnityEngine;

//TODO add spawning for items that did not private void FixedUpdate() 
//TODO add damage calcualation for items with damage
public class CraftingBenchUIController : MonoBehaviour
{
    public Sprite inventorySlotSprite;
    //The UI GameObject
    public bool isOpen = false;
    public GameObject playerCurrentlyUsing = null;
    public CraftingSlot[] craftingSlots;
    public CraftingSlot[] inventorySlots;
    public CraftingSlot[] slots;
    GameObject cursor;
    CraftingSlot cursorSlot;
    string playerPrefix;
    Dictionary<Item, List<int>> CraftingItems;
    bool uiReturn = false;                         //Tracks the return of the input axis because they are not boolean input
    int cursorIndex = 0;
    public Dictionary<int[], int> craftingRecipes;
    public ItemStack[] items;
    GameObject infoPanel;
    void Start()
    {
        craftingRecipes = new Dictionary<int[], int>(new ArrayComparer());
        craftingRecipes.Add(new int[] { -1, 2, -1,
                                         1, 10, 3,
                                        -1, 2, -1 }, 17);

        craftingRecipes.Add(new int[] {  1, 1,  1,
                                         1, -1, 1,
                                         1, 1, 1 }, 15);
        //spell circle
        craftingRecipes.Add(new int[] {  3, 3, 3,
                                         3, 3, 3,
                                         3, 3, 3 }, 16);
        // Hemp Hood
        craftingRecipes.Add(new int[] {  10, 10, 10,
                                         10, -1, 10,
                                         10, -1, 10}, 24);
        // Wood Helmet
        craftingRecipes.Add(new int[] {  1, 1, 1,
                                         1, -1, 1,
                                         1, -1, 1}, 25);
        // Hemp Vest
        craftingRecipes.Add(new int[] {  -1, -1, -1,
                                         10, 10, 10,
                                         -1, 10, -1}, 26);
        // Wood Plate Armor
        craftingRecipes.Add(new int[] {  -1,-1,-1,
                                          1,1,1,
                                         -1,1,-1}, 27);
        // Hemp Shorts
        craftingRecipes.Add(new int[] {  10,10,-1,
                                         10,10,-1,
                                         10,10,-1}, 28);
        // Wood Plate Skirt
        craftingRecipes.Add(new int[] {  1,1,-1,
                                         1,1,-1,
                                         1,1,-1}, 29);
        // Hemp Shorts
        craftingRecipes.Add(new int[] {  -1,10,10,
                                         -1,10,10,
                                         -1,10,10}, 28);
        // Wood Plate Skirt
        craftingRecipes.Add(new int[] {  -1,1,1,
                                         -1,1,1,
                                         -1,1,1}, 29);
        //Beast stable
        craftingRecipes.Add(new int[] {7,7,7,
                                       1,1,1,
                                       3,3,3}, 18);
        Initialize();
    }
    //for creating crafting recipes in the editor
    public string ArrayToString(int[] array)
    {
        return string.Join(",", array.Select(i => i.ToString()).ToArray());
    }
    public void Initialize()
    {
        CraftingItems = new Dictionary<Item, List<int>>();
        craftingSlots = new CraftingSlot[9];
        inventorySlots = new CraftingSlot[9];
        slots = new CraftingSlot[18];
        int counter = 0;
        for (int i = 3; i < 18; i += 6)
        {
            craftingSlots[counter] = transform.GetChild(0).GetChild(i).GetComponent<CraftingSlot>();
            craftingSlots[counter].currentItemStack = new ItemStack(null, 0, -1, true);
            craftingSlots[counter].isOccupied = false;
            craftingSlots[counter].quantText.text = "";
            craftingSlots[counter].spriteRenderer.sprite = null;
            craftingSlots[counter + 1] = transform.GetChild(0).GetChild(i + 1).GetComponent<CraftingSlot>();
            craftingSlots[counter + 1].currentItemStack = new ItemStack(null, 0, -1, true);
            craftingSlots[counter + 1].isOccupied = false;
            craftingSlots[counter + 1].isOccupied = false;
            craftingSlots[counter + 1].quantText.text = "";
            craftingSlots[counter + 1].spriteRenderer.sprite = null;
            craftingSlots[counter + 2] = transform.GetChild(0).GetChild(i + 2).GetComponent<CraftingSlot>();
            craftingSlots[counter + 2].currentItemStack = new ItemStack(null, 0, -1, true);
            craftingSlots[counter + 2].isOccupied = false;
            craftingSlots[counter + 2].isOccupied = false;
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
        int inventoryCounter = 0;
        int craftingCounter = 0;
        for (int i = 0; i < 18; i++)
        {
            if (i < 3 || i > 5 && i < 9 || i > 11 && i < 15)
            {
                slots[i] = inventorySlots[inventoryCounter];
                inventoryCounter++;
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
        cursor = transform.GetChild(0).GetChild(18).gameObject;
        cursorSlot = cursor.GetComponent<CraftingSlot>();
        infoPanel = transform.GetChild(0).GetChild(19).gameObject;
        transform.GetChild(0).gameObject.SetActive(false);
        isOpen = false;
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
            ListenToDirectionalInput();
            ListenToActionInput();
        }
    }

    void ListenToActionInput()
    {
        if (Input.GetButtonDown(playerPrefix + "Grab"))
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
        if (Input.GetButtonDown(playerPrefix + "Build"))
        {
            CheckForValidRecipe();
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
        if (direction.x > 0 && cursorIndex != 5 && cursorIndex != 11 && cursorIndex != 17)
        {
            if (cursorIndex + 1 < slots.Length)
            {
                cursorIndex += 1;
            }
        }
        else if (direction.x < 0 && cursorIndex != 0 && cursorIndex != 6 && cursorIndex != 12)
        {
            if (cursorIndex - 1 > -1)
            {
                cursorIndex -= 1;
            }
        }

        if (direction.y < 0)
        {
            if (cursorIndex + 6 < slots.Length)
            {
                cursorIndex += 6;
            }
        }
        else if (direction.y > 0)
        {
            if (cursorIndex - 6 > -1)
            {
                cursorIndex -= 6;
            }
        }

        MoveCursor(cursorIndex);
    }

    void SetSelectedItemStack(ItemStack itemStack, bool stack = true)
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
        return false;
    }
    void PlaceSelectedItem(bool stack)
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
                    slots[cursorIndex].currentItemStack.count = 1;
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
            DisplayItems();
            ac.GetComponent<ThirdPersonUserControl>().craftingBenchUI = true;
            playerPrefix = playerCurrentlyUsing.GetComponent<ThirdPersonUserControl>().playerPrefix;
            transform.GetChild(0).gameObject.SetActive(true);
            isOpen = true;
        }

    }
    string PrintRecipe(int[] recipe)
    {
        string stringafiedRecipe = "[";
        for (int i = 0; i < 9; i++)
        {
            stringafiedRecipe += $"{recipe[i]}, ";
        }
        stringafiedRecipe += "]";

        return stringafiedRecipe;
    }

    public void CheckForValidRecipe()
    {
        int[] recipe = new int[9];
        int c = 0;
        for (int i = 3; i < 18; i++)
        {
            if ((i > 5 && i < 9) || (i > 11 && i < 15))
            {
                continue;
            }

            if (slots[i].currentItemStack != null || slots[i].currentItemStack.item != null && slots[i].isOccupied)
            {

                recipe[c] = ItemManager.Instance.GetItemIndex(slots[i].currentItemStack.item);
            }
            else
            {
                recipe[c] = -1;
            }
            c++;
        }

        foreach (KeyValuePair<int[], int> kvp in craftingRecipes)
        {
            UnityEngine.Debug.Log($"Key = {PrintRecipe(kvp.Key)}, Value = {kvp.Value}");
        }

        bool recipeExists = craftingRecipes.Keys.Any(k => k.SequenceEqual(recipe));
        if (recipeExists)
        {
            int productIndex = craftingRecipes[recipe];
            GameObject newItem = ItemManager.Instance.GetItemGameObjectByItemIndex(productIndex);
            c = 0;
            for (int i = 3; i < 18; i++)
            {
                if ((i > 5 && i < 9) || (i > 11 && i < 15))
                {
                    continue;
                }
                if (i != 10)
                {
                    slots[i].currentItemStack = new ItemStack(null, 0, -1, true);
                    slots[i].spriteRenderer.sprite = null;
                    slots[i].isOccupied = false;
                    slots[i].quantText.text = "";
                }
                else
                {

                    if (newItem.GetComponent<BuildingMaterial>() == null)
                    {
                        slots[i].currentItemStack = new ItemStack(newItem.GetComponent<Item>(), 1, c, false);
                        slots[i].spriteRenderer.sprite = slots[i].currentItemStack.item.icon;
                        slots[i].currentItemStack.count = 1;
                        slots[i].isOccupied = true;
                    }
                    else
                    {
                        slots[i].currentItemStack = new ItemStack(null, 0, -1, true);
                        slots[i].spriteRenderer.sprite = null;
                        slots[i].isOccupied = false;
                        slots[i].quantText.text = "";
                    }
                }
                c++;
            }
            BuildingMaterial buildMat = newItem.GetComponent<BuildingMaterial>();
            if (buildMat != null && !buildMat.fitsInBackpack)
            {
                GameObject player = playerCurrentlyUsing;
                PlayerOpenUI(playerCurrentlyUsing);
                player.GetComponent<BuilderManager>().Build(player.GetComponent<ThirdPersonUserControl>(), buildMat);

            }
        }
    }
    public void ReconcileItems(PlayerInventoryManager actor)
    {
        ItemStack[] _items = new ItemStack[9];
        int c = 0;
        Dictionary<int, ItemStack> itemsInBench = new Dictionary<int, ItemStack>();

        //Search bench for remaining items
        for (int i = 3; i < 18; i++)
        {
            if ((i > 5 && i < 9) || (i > 11 && i < 15))
            {
                continue;
            }

            if (slots[i].currentItemStack.item != null && itemsInBench.ContainsKey(slots[i].currentItemStack.item.itemIndex))
            {
                itemsInBench[slots[i].currentItemStack.item.itemIndex].count += slots[i].currentItemStack.count;
            }
            else if (slots[i].currentItemStack.item != null)
            {
                itemsInBench.Add(slots[i].currentItemStack.item.itemIndex, slots[i].currentItemStack);
            }
            slots[i].currentItemStack = new ItemStack(null, 0, -1, true);
            slots[i].isOccupied = false;
            slots[i].quantText.text = "";
            slots[i].spriteRenderer.sprite = null;
        }
        if (cursorSlot.currentItemStack.item != null && itemsInBench.ContainsKey(cursorSlot.currentItemStack.item.itemIndex))
        {
            itemsInBench[cursorSlot.currentItemStack.item.itemIndex].count += cursorSlot.currentItemStack.count;
        }
        else if (cursorSlot.currentItemStack.item != null)
        {
            itemsInBench.Add(cursorSlot.currentItemStack.item.itemIndex, cursorSlot.currentItemStack);
        }
        cursorSlot.currentItemStack = new ItemStack(null, 0, -1, true);
        cursorSlot.isOccupied = false;
        cursorSlot.quantText.text = "";
        cursorSlot.spriteRenderer.sprite = null;
        foreach (KeyValuePair<int, ItemStack> kvp in itemsInBench)
        {
            UnityEngine.Debug.Log($"Key = {kvp.Key}, Value = {kvp.Value.count}");
        }
        //Gather all items in inventory portion of ui into an array
        for (int i = 0; i < 15; i++)
        {
            if ((i > 2 && i < 6) || (i > 8 && i < 12))
            {
                continue;
            }
            _items[c] = slots[i].currentItemStack;
            if (slots[i].currentItemStack.item != null && itemsInBench.ContainsKey(slots[i].currentItemStack.item.itemIndex))
            {
                _items[c].count += itemsInBench[slots[i].currentItemStack.item.itemIndex].count;

                itemsInBench.Remove(slots[i].currentItemStack.item.itemIndex);
            }
            c++;

        }
        foreach (KeyValuePair<int, ItemStack> entry in itemsInBench)
        {
            bool wasAdded = false;
            for (int i = 0; i < 9; i++)
            {
                if (_items[i].isEmpty)
                {
                    _items[i] = entry.Value;
                    wasAdded = true;
                    break;
                }
            }
            if (wasAdded)
            {
                continue;
            }
            else
            {
                //TODO Spawn item from crafting bench - no room left in inventory
            }
        }
        actor.items = _items;
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
        //AdjustButtonPrompts();
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





