using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;
public enum ActorState { Alive, Dead }
public class CharacterManager : ObjectManager
{
    public ActorState actorState;
    GameStateManager m_GameStateManager;
    public bool inBuilding;
    public GameObject currentBuildingObj;
    ThirdPersonUserControl userControl;
    PlayerInventoryManager inventoryManager;
    //GenerateLevel levelMaster;
    public ItemManager m_ItemManager;
    ActorEquipment equipment;
    bool isLoaded = false;
    // A string for file Path
    string m_SaveFilePath;
    public void Start()
    {
        userControl = GetComponent<ThirdPersonUserControl>();
        m_GameStateManager = GameObject.FindWithTag("GameController").GetComponent<GameStateManager>();
        m_SaveFilePath = m_GameStateManager.saveFilePath;
        m_ItemManager = GameObject.FindWithTag("GameController").GetComponent<ItemManager>();
        inventoryManager = GetComponentInParent<PlayerInventoryManager>();
        healthManager = GetComponent<HealthManager>();
        equipment = GetComponent<ActorEquipment>();
        actorState = ActorState.Alive;
        try
        {
            LoadCharacter();
        }
        catch
        {
            Debug.Log("No character data to load");
        }
    }


    public void Update()
    {
        CharacterStateMachine();
    }

    public void CharacterStateMachine()
    {
        switch (actorState)
        {
            case ActorState.Alive:
                CheckCharacterHealth();
                break;

            case ActorState.Dead:
                Kill();
                break;

            default:
                break;
        }
    }

    private void Kill()
    {
        Instantiate(deathEffectPrefab, transform.position, transform.rotation);
        GameObject.Destroy(this.gameObject);
    }

    private void CheckCharacterHealth()
    {
        if (healthManager.health <= 0)
        {
            actorState = ActorState.Dead;
        }
    }

    public void LoadCharacter()
    {
        string json;
        try
        {
            string filePath = m_SaveFilePath + "/Characters/" + userControl.playerName + ".json";
            json = File.ReadAllText(filePath);
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
            if (inventoryIndices[i, 0] != -1)
            {
                inventoryManager.AddItem(m_ItemManager.itemList[inventoryIndices[i, 0]].GetComponent<Item>(), inventoryIndices[i, 1]);
            }
        }


    }

    public void SaveCharacter()
    {
        string filePath = m_SaveFilePath + "/Characters/" + userControl.playerName + ".json";
        int[,] itemIndices = new int[9, 2];
        int equippedItem = -1;
        for (int i = 0; i <= inventoryManager.items.Length; i++)
        {
            for (int j = 0; j < m_ItemManager.itemList.Length; j++)
            {
                if (i < m_ItemManager.itemList.Length)
                {
                    if (inventoryManager.items[i].isEmpty == false)
                    {
                        string objectName = inventoryManager.items[i].item.name.Replace("(Clone)", "");
                        if (m_ItemManager.itemList[j].GetComponent<Item>().name == objectName)
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
                        string objectName = equipment.equippedItem.name.Replace("(Clone)", "");
                        if (m_ItemManager.itemList[j].GetComponent<Item>().name == objectName)
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
        using (FileStream stream = new FileStream(filePath, FileMode.Create))
        using (StreamWriter writer = new StreamWriter(stream))
        {
            // Write the JSON string to the file
            writer.Write(json);
        }
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
