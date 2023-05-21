using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;
public class CharacterManager : ActorManager
{
    ThirdPersonUserControl userControl;
    PlayerInventoryManager inventoryManager;
    bool isLoaded = false;
    // A string for file Path
    public string m_SaveFilePath;
    CharacterStats stats;
    public override void Start()
    {
        base.Start();
        stats = GetComponent<CharacterStats>();
        userControl = GetComponent<ThirdPersonUserControl>();
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, "Characters/");
        Directory.CreateDirectory(saveDirectoryPath);
        m_SaveFilePath = saveDirectoryPath + userControl.name + ".json";
        inventoryManager = GetComponentInParent<PlayerInventoryManager>();
        try
        {
            LoadCharacter();
        }
        catch
        {
            Debug.Log("No character data to load");
        }
    }
    public void LoadCharacter()
    {

        string json;
        try
        {
            json = File.ReadAllText(m_SaveFilePath);
        }
        catch (Exception ex)
        {
            Debug.Log("~ New Character. No data to load");
            return;
        }

        // Deserialize the data object from the JSON string
        CharacterSaveData data = JsonConvert.DeserializeObject<CharacterSaveData>(json);
        int[,] inventoryIndices = data.inventoryIndices;
        int equippedItemIndex = data.equippedItemIndex;

        if (equippedItemIndex != -1)
        {
            GameObject obj = Instantiate(m_ItemManager.itemList[equippedItemIndex]);
            equipment.EquipItem(obj);
            Destroy(obj);
        }

        for (int i = 0; i < 9; i++)
        {
            if (inventoryIndices[i, 0] != -1 && m_ItemManager.itemList[inventoryIndices[i, 0]].GetComponent<Item>().fitsInBackpack)
            {
                inventoryManager.AddItem(m_ItemManager.itemList[inventoryIndices[i, 0]].GetComponent<Item>(), inventoryIndices[i, 1]);
            }
        }
    }

    public void SaveCharacter()
    {
        int[,] itemIndices = new int[9, 2];
        int equippedItem = -1;
        for (int i = 0; i < inventoryManager.items.Length; i++)
        {
            for (int j = 0; j < m_ItemManager.itemList.Length; j++)
            {
                if (i < m_ItemManager.itemList.Length)
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
                    if (equipment.hasItem)
                    {
                        string objectName = equipment.equippedItem.GetComponent<Item>().itemName.Replace("(Clone)", "");
                        if (m_ItemManager.itemList[j].GetComponent<Item>().itemName == objectName)
                        {
                            equippedItem = j;
                            break;
                        }
                    }
                }
            }
        }

        CharacterSaveData data = new CharacterSaveData(itemIndices, equippedItem);
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
        public CharacterSaveData(int[,] inventoryIndices, int equipmentItemIndex)
        {
            this.inventoryIndices = inventoryIndices;
            this.equippedItemIndex = equipmentItemIndex;
        }
    }
}
