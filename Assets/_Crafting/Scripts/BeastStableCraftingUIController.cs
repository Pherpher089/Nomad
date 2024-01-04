using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class BeastStableCraftingUIController : MonoBehaviour
{
    public bool isOpen = false;
    public GameObject playerCurrentlyUsing = null;
    CraftingSlot[] craftingSlots;
    CraftingSlot[] inventorySlots;
    CraftingSlot productSlot;
    [HideInInspector]
    public TextMeshPro uiMessage;
    public BeastSaddleCraftingRecipe[] m_Recipes;
    GameObject cursor;
    string playerPrefix;
    bool uiReturn = false;//Tracks the return of the input axis because they are not boolean input
    int cursorIndex = 0;
    public ItemStack[] items;
    GameObject infoPanel;
    bool isCrafting = false;
    SaddleStationUIController saddleStation;
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
        saddleStation = transform.parent.GetComponentInChildren<SaddleStationUIController>();
        craftingSlots = new CraftingSlot[4];
        inventorySlots = new CraftingSlot[9];
        for (int i = 0; i < 9; i++)
        {
            inventorySlots[i] = transform.GetChild(0).GetChild(i).GetComponent<CraftingSlot>();
            inventorySlots[i].currentItemStack = new ItemStack(null, 0, -1, true);
            inventorySlots[i].isOccupied = false;
            inventorySlots[i].quantText.text = "";
            inventorySlots[i].spriteRenderer.sprite = null;
        }
        for (int i = 0; i < 4; i++)
        {
            craftingSlots[i] = transform.GetChild(0).GetChild(9 + i).GetComponent<CraftingSlot>();
            craftingSlots[i].gameObject.SetActive(false);
        }

        productSlot = transform.GetChild(0).GetChild(13).gameObject.GetComponent<CraftingSlot>();
        productSlot.gameObject.SetActive(false);
        uiMessage = transform.GetChild(0).GetChild(16).GetComponent<TextMeshPro>();
        cursor = transform.GetChild(0).GetChild(14).gameObject;
        infoPanel = transform.GetChild(0).GetChild(15).gameObject;
        transform.GetChild(0).gameObject.SetActive(false);
        isOpen = false;
    }

    void MoveCursor(int index)
    {
        cursor.transform.position = inventorySlots[index].transform.position;
        if (inventorySlots[index].currentItemStack.item != null)
        {
            UpdateInfoPanel(inventorySlots[index].currentItemStack.item.itemName, inventorySlots[index].currentItemStack.item.itemDescription, inventorySlots[index].currentItemStack.item.value, 0);
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
        if (Input.GetButtonDown(playerPrefix + "Block"))
        {
            AddIngredient();
        }
        if (Input.GetButtonDown(playerPrefix + "Grab"))
        {
            Debug.Log("Grabbing");
            TryBeastCraft();
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
        if (direction.x > 0 && cursorIndex != 2 && cursorIndex != 5 && cursorIndex != 8)
        {
            if (cursorIndex + 1 < inventorySlots.Length)
            {
                cursorIndex += 1;
            }
        }
        else if (direction.x < 0 && cursorIndex != 0 && cursorIndex != 3 && cursorIndex != 6)
        {
            if (cursorIndex - 1 > -1)
            {
                cursorIndex -= 1;
            }
        }

        if (direction.y < 0)
        {
            if (cursorIndex + 3 < inventorySlots.Length)
            {
                cursorIndex += 3;
            }
        }
        else if (direction.y > 0)
        {
            if (cursorIndex - 3 > -1)
            {
                cursorIndex -= 3;
            }
        }

        MoveCursor(cursorIndex);
    }

    public void PlayerOpenUI(GameObject actor)
    {
        //if actor has a packable item
        // open the cargo inventory with an item in the closest avaliable slot

        if (isOpen)
        {
            if (isCrafting)
            {
                ClearCraftingSlots();
            }
            else
            {
                transform.GetChild(0).gameObject.SetActive(false);
                ActorEquipment ac = actor.GetComponent<ActorEquipment>();
                isOpen = false;
                ac.GetComponent<ThirdPersonUserControl>().craftingBenchUI = false;
                playerCurrentlyUsing = null;
                playerPrefix = null;
                ReconcileItems(actor.GetComponent<PlayerInventoryManager>());
                Initialize();
            }
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
    void AddIngredient()
    {
        uiMessage.text = "";
        if (inventorySlots[cursorIndex].currentItemStack.isEmpty) return;
        // Loop through the crafting and check to see if they are enabled and thus empty
        for (int i = 0; i < 4; i++)
        {
            if (!craftingSlots[i].gameObject.activeSelf)
            {
                //Turn on crafting slot game object
                craftingSlots[i].gameObject.SetActive(true);
                // Add item to crafting slot
                craftingSlots[i].currentItemStack = new ItemStack(inventorySlots[cursorIndex].currentItemStack);
                craftingSlots[i].spriteRenderer.sprite = inventorySlots[cursorIndex].currentItemStack.item.icon;

                //Reduce quantity of item in inventory
                inventorySlots[cursorIndex].currentItemStack.count -= 1;

                //Handle cases where item count reduced to one and zero
                if (inventorySlots[cursorIndex].currentItemStack.count > 1)
                {
                    inventorySlots[cursorIndex].quantText.text = inventorySlots[cursorIndex].currentItemStack.count.ToString();
                }
                else if (inventorySlots[cursorIndex].currentItemStack.count == 1)
                {
                    inventorySlots[cursorIndex].quantText.text = "";
                }
                else
                {
                    inventorySlots[cursorIndex].currentItemStack = new ItemStack(null, 0, -1, true);
                    inventorySlots[cursorIndex].quantText.text = "";
                    inventorySlots[cursorIndex].spriteRenderer.sprite = null;
                }
                isCrafting = true;
                break;
            }
        }
        if (isCrafting)
        {
            Item[] currentIngredients = new Item[4];
            for (int i = 0; i < 4; i++)
            {
                if (craftingSlots[i].gameObject.activeSelf)
                {
                    currentIngredients[i] = craftingSlots[i].currentItemStack.item;
                }
            }
            foreach (BeastSaddleCraftingRecipe recipe in m_Recipes)
            {
                if (currentIngredients.SequenceEqual(recipe.ingredientsList))
                {
                    productSlot.gameObject.SetActive(true);
                    productSlot.currentItemStack = new ItemStack(recipe.product.GetComponent<Item>(), 1, 14, false);
                    productSlot.spriteRenderer.sprite = recipe.product.GetComponent<Item>().icon;
                }
                else
                {
                    productSlot.currentItemStack = new ItemStack();
                    productSlot.spriteRenderer.sprite = null;
                    productSlot.gameObject.SetActive(false);

                }
            }
        }
        else
        {
            productSlot.currentItemStack = new ItemStack();
            productSlot.spriteRenderer.sprite = null;
            productSlot.gameObject.SetActive(false);
        }
    }
    void ClearCraftingSlots(bool returnIngredients = true)
    {
        ActorEquipment ae = playerCurrentlyUsing.GetComponent<ActorEquipment>();
        for (int i = 0; i < 4; i++)
        {
            if (craftingSlots[i].gameObject.activeSelf && !craftingSlots[i].currentItemStack.isEmpty)
            {
                if (!returnIngredients)
                {
                    ae.SpendItem(craftingSlots[i].currentItemStack.item);
                }
                // Add item to crafting slot
                craftingSlots[i].currentItemStack = new ItemStack();
                craftingSlots[i].spriteRenderer.sprite = null;
                craftingSlots[i].quantText.text = "";
                craftingSlots[i].gameObject.SetActive(false);
            }
        }
        productSlot.currentItemStack = new ItemStack();
        productSlot.spriteRenderer.sprite = null;
        productSlot.gameObject.SetActive(false);
        DisplayItems();
        isCrafting = false;
    }
    public void TryBeastCraft()
    {
        Item[] currentIngredients = new Item[4];
        for (int i = 0; i < 4; i++)
        {
            if (craftingSlots[i].gameObject.activeSelf)
            {
                currentIngredients[i] = craftingSlots[i].currentItemStack.item;
            }
        }
        foreach (BeastSaddleCraftingRecipe recipe in m_Recipes)
        {
            if (currentIngredients.SequenceEqual(recipe.ingredientsList))
            {
                //Put object into beast saddle storage
                string message = saddleStation.AddItem(recipe.product.GetComponent<Item>());
                Debug.Log("### message " + message);
                if (message.Contains("!"))
                {
                    ClearCraftingSlots(false);
                }
                else
                {
                    ClearCraftingSlots(true);
                }
                uiMessage.text = message;
            }
        }
    }
    public void ReconcileItems(PlayerInventoryManager actor)
    {
        ItemStack[] _items = new ItemStack[9];
        Dictionary<int, ItemStack> itemsInBench = new Dictionary<int, ItemStack>();

        //Gather all items in inventory portion of ui into an array
        for (int i = 0; i < 9; i++)
        {

            _items[i] = inventorySlots[i].currentItemStack;
            if (inventorySlots[i].currentItemStack.item != null && itemsInBench.ContainsKey(inventorySlots[i].currentItemStack.item.itemIndex))
            {
                _items[i].count += itemsInBench[inventorySlots[i].currentItemStack.item.itemIndex].count;

                itemsInBench.Remove(inventorySlots[i].currentItemStack.item.itemIndex);
            }
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
