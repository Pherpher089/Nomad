using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;
public enum ActorState { Alive, Dead }
public class CharacterManager : ObjectManager
{
    public ActorState actorState;
    public bool inBuilding;
    public GameObject currentBuildingObj;
    ThirdPersonUserControl userControl;
    PlayerInventoryManager inventoryManager;
    GenerateLevel levelMaster;
    ActorEquipment equipment;
    bool isLoaded = false;
    public void Start()
    {
        userControl = GetComponent<ThirdPersonUserControl>();
        levelMaster = GameObject.FindWithTag("LevelMaster").GetComponent<GenerateLevel>();
        inventoryManager = GetComponentInParent<PlayerInventoryManager>();
        healthManager = GetComponent<HealthManager>();
        equipment = GetComponent<ActorEquipment>();
        actorState = ActorState.Alive;
    }


    public void Update()
    {
        if (!isLoaded && tag == "Player")
        {
            LoadCharacter();
            isLoaded = true;
        }
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
        try
        {
            string filePath = "C:/Users/Chris/wkspaces/CapsuleCrashers/Capsule Crashers/GameSaveData/Characters/" + userControl.playerName + ".json";
            string json = File.ReadAllText(filePath);
            // Deserialize the data object from the JSON string
            CharacterSaveData data = JsonConvert.DeserializeObject<CharacterSaveData>(json);
            int[,] inventoryIndices = data.inventoryIndices;
            int equippedItemIndex = data.equippedItemIndex;
            if (equippedItemIndex != -1)
            {
                Debug.Log("Equiped index " + equippedItemIndex);
                GameObject obj = Instantiate(levelMaster.itemPrefabs[equippedItemIndex]);
                equipment.EquipItem(levelMaster.itemPrefabs[equippedItemIndex].GetComponent<Item>());
                //Destroy(obj);
            }
            for (int i = 0; i < 9; i++)
            {
                if (inventoryIndices[i, 0] != -1)
                {
                    GameObject inventoryObj = Instantiate(levelMaster.itemPrefabs[inventoryIndices[i, 0]]);
                    inventoryManager.AddItem(inventoryObj.GetComponent<Item>(), inventoryIndices[i, 1]);
                    //Destroy(inventoryObj);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log("New Character. No data to load");
        }
    }

    public void SaveCharacter()
    {
        string filePath = "C:/Users/Chris/wkspaces/CapsuleCrashers/Capsule Crashers/GameSaveData/Characters/" + userControl.playerName + ".json";
        int[,] itemIndices = new int[9, 2];
        int equippedItem = -1;
        for (int i = 0; i <= inventoryManager.items.Length; i++)
        {
            for (int j = 0; j < levelMaster.itemPrefabs.Length; j++)
            {
                if (i < levelMaster.itemPrefabs.Length)
                {
                    if (!inventoryManager.items[i].isEmpty)
                    {
                        string objectName = inventoryManager.items[i].item.name.Replace("(Clone)", "");
                        if (levelMaster.itemPrefabs[j].GetComponent<Item>().name == objectName)
                        {
                            itemIndices[i, 0] = j;
                            itemIndices[i, 1] = inventoryManager.items[i].count;
                        }
                    }
                }
                else
                {
                    if (equipment.hasItem)
                    {
                        string objectName = equipment.equipedItem.name;
                        if (levelMaster.itemPrefabs[j].GetComponent<Item>().name == objectName)
                        {
                            equippedItem = j;
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
        Debug.Log("Saved Character: " + userControl.playerName);
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
