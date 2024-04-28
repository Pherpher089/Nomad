using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using UnityEngine;


public class BeastCargoInventoryManager : MonoBehaviour
{

    public Sprite inventorySlotSprite;
    //The UI GameObject
    public bool isOpen = true;
    public GameObject playerCurrentlyUsing = null;
    BeastManager beastManager;
    public CargoSlot[] cargoSlots;
    public GameObject[] sockets;
    GameObject cursor;
    SpriteRenderer cursorSprite;

    string playerPrefix;
    Dictionary<CargoItem, List<int>> cargoItems;
    CargoItem selectedItem = null;
    bool uiReturn = false;                         //Tracks the return of the input axis because they are not boolean input
    int cursorIndex = 0;
    bool isClosed;

    void Start()
    {
        cargoItems = new Dictionary<CargoItem, List<int>>();
        cargoSlots = new CargoSlot[9];
        sockets = new GameObject[9];
        for (int i = 0; i < 9; i++)
        {
            cargoSlots[i] = transform.GetChild(i).GetComponent<CargoSlot>();
            sockets[i] = transform.parent.GetChild(1).GetChild(i).gameObject;
        }

        inventorySlotSprite = cargoSlots[0].spriteRenderer.sprite;
        //The cursor is the 10th child
        cursor = transform.GetChild(9).gameObject;
        cursorSprite = cursor.GetComponent<SpriteRenderer>();
        beastManager = GetComponentInParent<BeastManager>();
        CloseWhileNotCamping();
    }

    void MoveCursor(int index)
    {
        cursor.transform.position = cargoSlots[index].transform.position;
    }

    void Update()
    {
        isClosed = !isOpen;
        if (!beastManager.m_IsCamping && isOpen)
        {
            CloseWhileNotCamping();
        }
        if (playerCurrentlyUsing != null)
        {
            ListenToDirectionalInput();
            ListenToActionInput();
        }
    }

    void ListenToActionInput()
    {
        if (Input.GetButtonDown(playerPrefix + "Craft"))
        {
            if (selectedItem != null)
            {
                PlaceSelectedItem();
            }
            else
            {
                SelectItem();
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

        if (Input.GetButtonDown(playerPrefix + "Grab") && !isClosed)
        {
            RotateSelection();
        }
    }

    void MoveCursor(Vector2 direction)
    {
        if (direction.x > 0 && cursorIndex != 2 && cursorIndex != 5 && cursorIndex != 8)
        {
            if (cursorIndex + 1 < cargoSlots.Length - 1)
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
            if (cursorIndex + 3 < cargoSlots.Length - 1)
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

    void SetSelectedItem(CargoItem item)
    {
        if (item == null)
        {
            selectedItem = null;
            cursorSprite.sprite = null;
            cursor.transform.localEulerAngles = Vector3.zero;
            return;
        }
        selectedItem = item;
        cursorSprite.sprite = item.cargoIconPacked;
        cursor.transform.Rotate(new Vector3(0, 0, -90 * (int)item.rotation), Space.Self);
    }

    bool SelectItem()
    {
        if (cargoSlots[cursorIndex].isOccupied)
        {
            SetSelectedItem(cargoSlots[cursorIndex].cargoItem);
            GameObject obj = sockets[cursorIndex].transform.GetChild(0).gameObject;
            obj.transform.parent = null;
            Destroy(obj);
            sockets[cursorIndex].transform.localEulerAngles = Vector3.zero;
            ActorEquipment ac = playerCurrentlyUsing.GetComponent<ActorEquipment>();
            selectedItem.GetComponent<Item>().isEquipable = true;
            ac.EquipItem(selectedItem.GetComponent<Item>());
            PackableItem pi = ac.equippedItem.GetComponent<PackableItem>();
            if (pi != null)
            {
                pi.PackAndSave(this.gameObject);
                ac.equippedItem.GetComponent<BuildingObject>().isPlaced = true;
            }
            List<int> occupiedIndices = CheckSlotVacancyByIndex();
            if (selectedItem != null)
            {
                cargoSlots[cursorIndex].spriteRenderer.sprite = inventorySlotSprite;
                foreach (int indices in occupiedIndices)
                {
                    cargoItems.Remove(cargoSlots[indices].cargoItem);
                    cargoSlots[indices].isOccupied = false;
                    cargoSlots[indices].cargoItem = null;
                    cargoSlots[indices].transform.localEulerAngles = Vector3.zero;
                }
            }
        }
        return false;
    }
    /// <summary>
    /// Places selected item in cargo inventory.
    /// </summary>
    /// <returns>returns true if item was placed.</returns>
    bool PlaceSelectedItem()
    {
        List<int> occupiedIndices = CheckSlotVacancyByIndex();
        if (selectedItem != null && occupiedIndices != null)
        {
            cargoItems[selectedItem] = occupiedIndices;
            cargoSlots[cursorIndex].spriteRenderer.sprite = selectedItem.cargoIconPacked;
            cursor.transform.localRotation = Quaternion.Euler(Vector3.zero);
            cargoSlots[cursorIndex].transform.Rotate(new Vector3(0, 0, -90 * (int)selectedItem.rotation), Space.Self);
            GameObject placedItem = Instantiate(ItemManager.Instance.GetItemGameObjectByItemIndex(selectedItem.GetComponent<Item>().itemListIndex));
            PackableItem pi = placedItem.GetComponent<PackableItem>();
            if (pi != null && selectedItem.GetComponent<PackableItem>().packed)
            {
                pi.PackAndSave(this.gameObject);
                placedItem.GetComponent<BuildingObject>().isPlaced = true;
                placedItem.GetComponent<Item>().isEquipable = false;
            }
            foreach (int indices in occupiedIndices)
            {

                cargoSlots[indices].isOccupied = true;
                cargoSlots[indices].cargoItem = placedItem.GetComponent<CargoItem>();
                placedItem.transform.parent = sockets[cursorIndex].transform;
                placedItem.transform.position = sockets[cursorIndex].transform.position;
                placedItem.transform.localEulerAngles = new Vector3(0, cursor.transform.localEulerAngles.z, 0);
            }
            SetSelectedItem(null);
            playerCurrentlyUsing.GetComponent<ActorEquipment>().UnequippedCurrentItem(true);
            return true;
        }
        return false;

    }
    /// <summary>
    /// Returns the occupied indices of the object if it is a valid placement
    /// </summary>
    /// <returns>Returns the newly occupied index of cargoSlots. If it is an invalid placement, it will return nul.</returns> <summary>
    List<int> CheckSlotVacancyByIndex()
    {
        //Gather all of the indices that this item will take up. 
        List<int> potentiallyOccupiedIndices = new List<int>();

        //Due to cargo items being a 1D array, we need to compensate the increments according to rotation
        int modY, modX, sizeX, sizeY;
        if (selectedItem == null)
        {
            modX = modY = sizeX = sizeY = 1;
        }
        else
        {
            modY = selectedItem.rotation == CargoRotation.Up || selectedItem.rotation == CargoRotation.Down ? 3 : 1;
            if (modY == 3 && selectedItem.rotation == CargoRotation.Down || modY == 1 && selectedItem.rotation == CargoRotation.Left)
            {
                modY *= -1;
            }
            modX = selectedItem.rotation == CargoRotation.Right || selectedItem.rotation == CargoRotation.Left ? 3 : 1;
            if (modX == 3 && selectedItem.rotation == CargoRotation.Right || modX == 1 && selectedItem.rotation == CargoRotation.Down)
            {
                modX *= -1;
            }
            sizeX = selectedItem.size.x;
            sizeY = selectedItem.size.y;

        }
        for (int i = 0; Mathf.Abs(i) < sizeX;)
        {
            for (int j = 0; Mathf.Abs(j) < sizeY;)
            {
                //Check to see if potential slot index is out of bounds
                if (cursorIndex + i + j > 9 || cursorIndex + i + j < 0)
                {
                    return null;
                }
                potentiallyOccupiedIndices.Add(cursorIndex + i + j);
                j += modY;
            }
            i += modX;
        }

        foreach (int indexToCheck in potentiallyOccupiedIndices)
        {
            if (cargoItems.Count > 0)
            {
                foreach (KeyValuePair<CargoItem, List<int>> item in cargoItems)
                {
                    foreach (int index in item.Value)
                    {
                        if (index == indexToCheck) return null;
                    }
                }
            }
        }

        return potentiallyOccupiedIndices;
    }
    void RotateSelection()
    {
        if (selectedItem != null)
        {
            if (cursor.transform.localEulerAngles.z > 270)
            {
                cursor.transform.localEulerAngles = Vector3.zero;
                selectedItem.rotation = CargoRotation.Up;
            }
            cursor.transform.Rotate(new Vector3(0, 0, -90));
            selectedItem.rotation = (CargoRotation)((int)selectedItem.rotation + 1);
        }
    }

    public int GetOpenSlotIndex()
    {
        for (int i = 0; i < cargoSlots.Length; i++)
        {
            if (!cargoSlots[i].isOccupied)
            {
                return i;
            }
        }
        return -1;
    }
    public void PlayerOpenUI(GameObject actor)
    {
        //if actor has a packable item
        // open the cargo inventory with an item in the closest avaliable slot
        if (beastManager.m_IsCamping && !isOpen || isOpen)
        {
            if (isOpen)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(false);
                }
                ActorEquipment ac = actor.GetComponent<ActorEquipment>();
                ac.GetComponent<ThirdPersonUserControl>().cargoUI = false;
                isOpen = false;
                playerCurrentlyUsing = null;
                playerPrefix = null;
                SetSelectedItem(null);
            }
            else
            {
                playerCurrentlyUsing = actor;
                playerPrefix = playerCurrentlyUsing.GetComponent<ThirdPersonUserControl>().playerPrefix;
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(true);
                }
                ActorEquipment ac = actor.GetComponent<ActorEquipment>();
                ac.GetComponent<ThirdPersonUserControl>().cargoUI = true;
                if (ac != null && ac.equippedItem != null && ac.equippedItem.GetComponent<CargoItem>() != null)
                {
                    CargoItem ci = ac.equippedItem.GetComponent<CargoItem>();
                    int openSlotIndex = GetOpenSlotIndex();
                    if (openSlotIndex != -1)
                    {
                        MoveCursor(openSlotIndex);
                        SetSelectedItem(ci);
                    }
                }
                isOpen = true;
            }
        }
    }

    public void CloseWhileNotCamping()
    {
        if (!beastManager.m_IsCamping && isOpen)
        {
            if (isOpen)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(false);
                }
                isOpen = false;
                playerCurrentlyUsing = null;
            }
        }
    }
}





