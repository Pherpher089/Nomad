using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class SaddleStationUIController : MonoBehaviour
{
    public Sprite inventorySlotSprite;
    //The UI GameObject
    public bool isOpen = false;
    public GameObject playerCurrentlyUsing = null;
    public CraftingSlot[] inventorySlots;
    GameObject cursor;
    string playerPrefix;
    Dictionary<Item, List<int>> CraftingItems;
    bool uiReturn = false;                         //Tracks the return of the input axis because they are not boolean input
    int cursorIndex = 0;
    public Dictionary<int[], int> craftingRecipes;
    public ItemStack[] items;
    GameObject infoPanel;
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
        CraftingItems = new Dictionary<Item, List<int>>();
        inventorySlots = new CraftingSlot[9];
        for (int i = 0; i < 9; i++)
        {
            inventorySlots[i] = transform.GetChild(0).GetChild(i).GetComponent<CraftingSlot>();
            inventorySlots[i].currentItemStack = new ItemStack(null, 0, -1, true);
            inventorySlots[i].isOccupied = false;
            inventorySlots[i].quantText.text = "";
            inventorySlots[i].spriteRenderer.sprite = null;
        }

        cursor = transform.GetChild(0).GetChild(9).gameObject;
        infoPanel = transform.GetChild(0).GetChild(11).gameObject;
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
        // if (Input.GetButtonDown(playerPrefix + "Grab"))
        // {
        //     if (!cursorSlot.isOccupied)
        //     {
        //         SelectItem(true);
        //     }
        //     else
        //     {
        //         PlaceSelectedItem(true);
        //     }
        // }
        // if (Input.GetButtonDown(playerPrefix + "Block"))
        // {
        //     if (!cursorSlot.isOccupied)
        //     {
        //         SelectItem(false);
        //     }
        //     else
        //     {
        //         PlaceSelectedItem(false);
        //     }
        // }
        // if (Input.GetButtonDown(playerPrefix + "Build"))
        // {
        //     CheckForValidRecipe();
        // }
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

    public void UpdateInfoPanel(string name, string description, int value, int damage = 0)
    {
        infoPanel.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = name;
        infoPanel.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = description;
        infoPanel.transform.GetChild(1).GetChild(2).GetComponent<TextMeshProUGUI>().text = damage != 0 ? $"Damage: {damage}" : "";
        infoPanel.transform.GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text = value != 0 ? $"{value}Gp" : "";

    }
    public void DisplayItems()
    {
        //nothing yet
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
