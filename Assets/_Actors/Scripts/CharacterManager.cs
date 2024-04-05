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
    ThirdPersonUserControl userControl;
    PlayerInventoryManager inventoryManager;
    bool isLoaded = false;
    // A string for file Path
    public string m_SaveFilePath;
    CharacterStats stats;
    public void Start()
    {

        stats = GetComponent<CharacterStats>();
        userControl = GetComponent<ThirdPersonUserControl>();
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
            Debug.Log($"~ Failed loading {m_SaveFilePath}");
        }

    }
    public void LoadCharacter()
    {
        string json;
        try
        {
            json = File.ReadAllText(m_SaveFilePath);
            Debug.Log($"~ Loading '{userControl.characterName}' - {m_SaveFilePath}");

        }
        catch
        {
            Debug.Log($"~ New Character {stats.characterName}. No data to load");
            return;
        }

        // Deserialize the data object from the JSON string
        CharacterSaveData data = JsonConvert.DeserializeObject<CharacterSaveData>(json);
        int[,] inventoryIndices = data.inventoryIndices;
        int equippedItemIndex = data.equippedItemIndex;
        int[] armorIndices = data.equippedArmorIndices;
        if (equippedItemIndex != -1)
        {
            if (equipment.equippedItem != null)
            {
                equipment.UnequippedCurrentItem();
            }
            equipment.EquipItem(m_ItemManager.itemList[equippedItemIndex]);
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
        SaveCharacter();
    }

    public void SaveCharacter()
    {
        if (!GetComponent<PhotonView>().IsMine) return;
        int[,] itemIndices = new int[9, 2];
        int equippedItem = -1;
        int[] armorIndices = new int[3];
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
                else
                {

                    if (equipment.hasItem && equipment.equippedItem != null)
                    {
                        string objectName = equipment.equippedItem.GetComponent<Item>().itemName;
                        if (m_ItemManager.itemList[j].GetComponent<Item>().itemName == objectName)
                        {
                            equippedItem = j;
                            break;
                        }
                    }
                }
            }
        }
        for (int i = 0; i < 3; i++)
        {
            if (equipment.equippedArmor[i] != null)
            {
                armorIndices[i] = equipment.equippedArmor[i].GetComponent<Item>().itemIndex;
            }
            else
            {
                armorIndices[i] = -1;
            }
        }
        CharacterSaveData data = new CharacterSaveData(itemIndices, equippedItem, armorIndices);
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
        public int equippedItemIndex;
        public int[] equippedArmorIndices;
        public CharacterSaveData(int[,] inventoryIndices, int equipmentItemIndex, int[] equippedArmorIndices)
        {
            this.inventoryIndices = inventoryIndices;
            this.equippedItemIndex = equipmentItemIndex;
            this.equippedArmorIndices = equippedArmorIndices;
        }
    }
}
