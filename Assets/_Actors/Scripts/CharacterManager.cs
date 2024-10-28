using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;
using Photon.Pun;
/// <summary>
/// Handles all of the saving of items for a player character.
/// </summary>
public class CharacterManager : ActorManager
{
    [HideInInspector] ThirdPersonUserControl userControl;
    [HideInInspector] PlayerInventoryManager inventoryManager;
    public bool isLoaded = false;
    // A string for file Path
    public string m_SaveFilePath;
    [HideInInspector]
    public CharacterStats stats;

    // Needed for quick stats on inventory manager
    [HideInInspector]
    public HealthManager health;
    [HideInInspector]
    public HungerManager hunger;
    [HideInInspector]
    public void Start()
    {
        stats = GetComponent<CharacterStats>();
        userControl = GetComponent<ThirdPersonUserControl>();
        health = GetComponent<HealthManager>();
        hunger = GetComponent<HungerManager>();
        equipment = GetComponent<ActorEquipment>();
        inventoryManager = GetComponentInParent<PlayerInventoryManager>();
        // Get the save directory
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, "Characters");
        Directory.CreateDirectory(saveDirectoryPath);
        m_SaveFilePath = saveDirectoryPath + "/" + userControl.characterName + ".json";
        try
        {
            LoadCharacter();
        }
        catch
        {
            // Debug.Log($"~ Failed loading {m_SaveFilePath}");
        }

    }
    public void LoadCharacter()
    {
        string json;
        try
        {
            json = File.ReadAllText(m_SaveFilePath);
            // Debug.Log($"~ Loading '{userControl.characterName}' - {m_SaveFilePath}");

        }
        catch
        {
            Item stick = Instantiate(ItemManager.Instance.GetItemGameObjectByItemIndex(2)).GetComponent<Item>();
            stick.GetComponent<MeshRenderer>().enabled = false;
            stick.GetComponent<Collider>().enabled = false;
            Item apples = Instantiate(ItemManager.Instance.GetItemGameObjectByItemIndex(8)).GetComponent<Item>();
            apples.GetComponent<MeshRenderer>().enabled = false;
            apples.GetComponent<Collider>().enabled = false;
            stick.isBeltItem = true;
            inventoryManager.AddBeltItem(stick, 1);
            apples.isBeltItem = true;
            inventoryManager.AddBeltItem(apples, 8);
            // Debug.Log($"~ New Character {stats.characterName}. No data to load");
            return;
        }

        // Deserialize the data object from the JSON string
        CharacterSaveData data = JsonConvert.DeserializeObject<CharacterSaveData>(json);
        int[,] inventoryIndices = data.inventoryIndices;
        int[,] beltIndices = data.beltIndices;
        int equippedItemIndex = data.equippedItemIndex;
        int equippedPipeIndex = data.equippedPipeIndex;
        int equippedSpecialIndex = data.equippedSpecialIndex;
        int equippedCapeIndex = data.equippedCapeIndex;
        int equippedUtilityIndex = data.equippedUtilityIndex;
        int[] armorIndices = data.equippedArmorIndices;
        int selectedBeltItem = data.selectedBeltItem;
        if (equippedItemIndex != -1)
        {
            if (equipment.equippedItem != null)
            {
                equipment.UnequippedCurrentItem();
            }
            equipment.EquipItem(m_ItemManager.itemList[equippedItemIndex]);
        }
        if (equippedCapeIndex != -1)
        {

            if (equipment.equippedSpecialItems[0] != null)
            {
                equipment.UnequippedCurrentSpecialItem(0);
            }

            equipment.EquipItem(m_ItemManager.itemList[equippedCapeIndex]);
        }
        if (equippedUtilityIndex != -1)
        {

            if (equipment.equippedSpecialItems[1] != null)
            {
                equipment.UnequippedCurrentSpecialItem(1);
            }

            equipment.EquipItem(m_ItemManager.itemList[equippedUtilityIndex]);
        }
        if (equippedPipeIndex != -1)
        {

            if (equipment.equippedSpecialItems[2] != null)
            {
                equipment.UnequippedCurrentSpecialItem(2);
            }

            equipment.EquipItem(m_ItemManager.itemList[equippedPipeIndex]);
        }
        if (equippedSpecialIndex != -1)
        {

            if (equipment.equippedSpecialItems[3] != null)
            {
                equipment.UnequippedCurrentSpecialItem(3);
            }

            equipment.EquipItem(m_ItemManager.itemList[equippedSpecialIndex]);
        }
        for (int i = 0; i < 3; i++)
        {
            if (armorIndices[i] != -1)
            {
                equipment.EquipItem(m_ItemManager.itemList[armorIndices[i]]);
            }
        }

        for (int i = 0; i < 9; i++)
        {
            if (inventoryIndices[i, 0] != -1 && m_ItemManager.itemList[inventoryIndices[i, 0]].GetComponent<Item>().fitsInBackpack)
            {
                inventoryManager.AddItem(m_ItemManager.itemList[inventoryIndices[i, 0]].GetComponent<Item>(), inventoryIndices[i, 1]);
            }
        }
        for (int i = 0; i < 4; i++)
        {
            if (beltIndices[i, 0] != -1 && m_ItemManager.itemList[beltIndices[i, 0]].GetComponent<Item>().fitsInBackpack)
            {

                Item _item = Instantiate(m_ItemManager.GetItemGameObjectByItemIndex(beltIndices[i, 0])).GetComponent<Item>();
                _item.GetComponent<MeshRenderer>().enabled = false;
                _item.GetComponent<Collider>().enabled = false;
                _item.isBeltItem = true;
                inventoryManager.AddBeltItem(_item, beltIndices[i, 1]);
            }
        }
        inventoryManager.selectedBeltItem = selectedBeltItem;
        GetComponent<ThirdPersonUserControl>().toolBeltIndex = selectedBeltItem;
        SaveCharacter();
        isLoaded = true;
    }

    public void SaveCharacter()
    {
        if (!GetComponent<PhotonView>().IsMine) return;
        int[,] itemIndices = new int[9, 2];
        int[,] beltIndices = new int[4, 2];
        int equippedItem = -1;
        int equippedPipe = -1;
        int equippedSpecial = -1;
        int equippedCape = -1;
        int equippedUtility = -1;
        int[] armorIndices = new int[3];
        if (equipment.hasItem && equipment.equippedItem != null)
        {
            equippedItem = equipment.equippedItem.GetComponent<Item>().itemListIndex;
        }
        if (equipment.equippedSpecialItems[0] != null)
        {
            equippedCape = equipment.equippedSpecialItems[0].GetComponent<Item>().itemListIndex;
        }
        if (equipment.equippedSpecialItems[1] != null)
        {
            equippedUtility = equipment.equippedSpecialItems[1].GetComponent<Item>().itemListIndex;
        }
        if (equipment.equippedSpecialItems[2] != null)
        {
            equippedPipe = equipment.equippedSpecialItems[2].GetComponent<Item>().itemListIndex;
        }
        if (equipment.equippedSpecialItems[3] != null)
        {
            equippedSpecial = equipment.equippedSpecialItems[3].GetComponent<Item>().itemListIndex;
        }


        for (int i = 0; i <= inventoryManager.items.Length; i++)
        {
            for (int j = 0; j < m_ItemManager.itemList.Length; j++)
            {
                if (i < inventoryManager.items.Length)
                {
                    if (inventoryManager.items[i].isEmpty == false)
                    {
                        string objectName = inventoryManager.items[i].item.itemName;

                        if (m_ItemManager.itemList[j].GetComponent<Item>().itemName == objectName)
                        {
                            itemIndices[i, 0] = j;
                            itemIndices[i, 1] = inventoryManager.items[i].count;
                            break;
                        }
                    }
                }
            }
        }
        for (int i = 0; i < inventoryManager.beltItems.Length; i++)
        {
            for (int j = 0; j < m_ItemManager.itemList.Length; j++)
            {

                if (inventoryManager.beltItems[i].isEmpty == false)
                {
                    string objectName = inventoryManager.beltItems[i].item.itemName;

                    if (m_ItemManager.itemList[j].GetComponent<Item>().itemName == objectName)
                    {
                        beltIndices[i, 0] = j;
                        beltIndices[i, 1] = inventoryManager.beltItems[i].count;
                        break;
                    }
                }

            }
        }
        for (int i = 0; i < 3; i++)
        {
            if (equipment.equippedArmor[i] != null)
            {
                armorIndices[i] = equipment.equippedArmor[i].GetComponent<Item>().itemListIndex;
            }
            else
            {
                armorIndices[i] = -1;
            }
        }
        CharacterSaveData data = new CharacterSaveData(itemIndices, equippedItem, equippedPipe, equippedSpecial, equippedCape, equippedUtility, armorIndices, beltIndices, inventoryManager.selectedBeltItem);
        string json = JsonConvert.SerializeObject(data);
        // Open the file for writing
        using (FileStream stream = new FileStream(m_SaveFilePath, FileMode.Create))
        using (StreamWriter writer = new StreamWriter(stream))
        {
            // Write the JSON string to the file
            writer.Write(json);
        }
        stats.SaveCharacter();
    }
    public class CharacterSaveData
    {
        public int[,] inventoryIndices;
        public int[,] beltIndices;
        public int equippedItemIndex;
        public int equippedPipeIndex;
        public int equippedSpecialIndex;
        public int equippedCapeIndex;
        public int equippedUtilityIndex;
        public int[] equippedArmorIndices;
        public int selectedBeltItem;
        public CharacterSaveData(int[,] inventoryIndices, int equippedItemIndex, int equippedPipeIndex, int equippedSpecialIndex, int equippedCapeIndex, int equippedUtilityIndex, int[] equippedArmorIndices, int[,] beltIndices, int selectedBeltItem)
        {
            this.inventoryIndices = inventoryIndices;
            this.beltIndices = beltIndices;
            this.equippedItemIndex = equippedItemIndex;
            this.equippedPipeIndex = equippedPipeIndex;
            this.equippedSpecialIndex = equippedSpecialIndex;
            this.equippedCapeIndex = equippedCapeIndex;
            this.equippedUtilityIndex = equippedUtilityIndex;
            this.equippedArmorIndices = equippedArmorIndices;
            this.selectedBeltItem = selectedBeltItem;
        }
    }
}
