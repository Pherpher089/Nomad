using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using System.Threading;
using System.Linq;
using UnityEngine.SceneManagement;
using Unity.AI.Navigation;

public class LevelManager : MonoBehaviour
{
    private static GameStateManager m_Controller;
    public static LevelManager Instance;
    public const string LevelDataKey = "levelData";
    public PhotonView m_PhotonView;
    Transform parentTerrain;
    public LevelSaveData saveData;

    //TODO wondering if this should live somewhere else
    //Spell Crafting stuff.
    public GameObject m_SpellCraftingSuccessParticleEffect;
    public float m_SpellCraftingSuccessParticleEffectDuration = 1f;
    public Material[] playerColors;
    public int worldProgress;

    void Awake()
    {
        Instance = this;
        m_PhotonView = GetComponent<PhotonView>();
        DontDestroyOnLoad(gameObject);
    }


    public void InitializeLevel(string levelName)
    {
        saveData = LoadLevel(levelName);
        if (saveData == null)
        {
            Debug.Log("~ no save file for " + levelName + ". Creating new file.");
            saveData = new LevelSaveData(levelName);
            SaveLevel();
        }
        parentTerrain = GameObject.FindWithTag("WorldTerrain").transform;
        PopulateObjects();
    }
    private void OnEnable()
    {
        // Subscribe to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Initialize level once a non main menu scene has been loaded. When a scene is loaded 
    // it should have come from the gamePrep object which holds the current scene
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex != 0)
        {
            InitializeLevel(scene.name);
        }
    }
    public void PopulateObjects()
    {
        StartCoroutine(PopulateObjectsCoroutine());
    }
    IEnumerator PopulateObjectsCoroutine()
    {
        List<SourceObject> objectsInLevel = new List<SourceObject>(parentTerrain.transform.GetComponentsInChildren<SourceObject>());

        if (saveData != null && saveData.removedObjects != null && saveData.removedObjects.Length > 0)
        {
            List<string> removedList = new List<string>(saveData.removedObjects);
            foreach (SourceObject obj in objectsInLevel)
            {
                if (removedList.Contains(obj.id))
                {
                    Destroy(obj.gameObject);
                }
            }
        }

        if (saveData != null && saveData.objects != null && saveData.objects.Length > 0)
        {
            List<string> objectsList = new List<string>(saveData.objects);
            foreach (string obj in objectsList)
            {
                string[] splitData = obj.Split('_');
                //Get the object prefab from the item manager with the item index at index 0
                GameObject newObject = Instantiate(ItemManager.Instance.environmentItemList[int.Parse(splitData[0])]);
                newObject.transform.SetParent(parentTerrain);
                newObject.transform.position = new Vector3(float.Parse(splitData[1]), float.Parse(splitData[2]), float.Parse(splitData[3]));
                newObject.transform.rotation = Quaternion.Euler(new Vector3(0, float.Parse(splitData[4]), 0));
                SourceObject so = newObject.GetComponent<SourceObject>();
                if (so != null)
                {
                    so.id = obj;
                }
                else
                {
                    newObject.GetComponent<Item>().id = obj;
                }
            }
        }

        yield return null;
    }
    public void CallUpdatePlayerColorPRC(int viewID, int colorIndex)
    {
        m_PhotonView.RPC("UpdatePlayerColorPRC", RpcTarget.AllBuffered, viewID, colorIndex);
    }
    [PunRPC]
    public void UpdatePlayerColorPRC(int viewID, int colorIndex)
    {
        PhotonView targetView = PhotonView.Find(viewID);
        targetView.transform.GetComponentInChildren<SkinnedMeshRenderer>().material = playerColors[colorIndex];
    }
    public void CallChestInUsePRC(string _id, bool _inUse)
    {
        m_PhotonView.RPC("InUsePRC", RpcTarget.AllBuffered, _id, _inUse);

    }
    [PunRPC]
    public void InUsePRC(string _id, bool _inUse)
    {
        int underscoreIndex = _id.LastIndexOf('_');
        string incomingTrimmedId = _id.Substring(0, underscoreIndex - 1);
        ChestController[] allChests = FindObjectsOfType<ChestController>();
        foreach (ChestController chest in allChests)
        {
            underscoreIndex = chest.m_BuildingMaterial.id.LastIndexOf('_');
            string currentTrimmedId = chest.m_BuildingMaterial.id.Substring(0, underscoreIndex - 1);
            if (currentTrimmedId == incomingTrimmedId)
            {
                chest.inUse = _inUse;
            }
        }

    }

    public void CallSaveGameProgress(int progress)
    {
        m_PhotonView.RPC("SaveGameProgress", RpcTarget.All, progress);
    }

    [PunRPC]
    public void SaveGameProgress(int progress)
    {
        worldProgress = progress;
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{LevelPrep.Instance.settlementName}/");
        Directory.CreateDirectory(saveDirectoryPath);
        string name = LevelPrep.Instance.currentLevel;
        string filePath = saveDirectoryPath + "GameProgress.json";
        string json = JsonConvert.SerializeObject(new GameSaveData(progress));
        // Open the file for writing
        using (FileStream stream = new FileStream(filePath, FileMode.Create))
        using (StreamWriter writer = new StreamWriter(stream))
        {
            writer.Write(json);
        }
    }

    public void CallBeastChestInUsePRC(string _id, bool _inUse)
    {
        m_PhotonView.RPC("BeastChestInUsePRC", RpcTarget.AllBuffered, _id, _inUse);

    }

    [PunRPC]
    public void BeastChestInUsePRC(string _id, bool _inUse)
    {
        BeastStorageContainerController[] allChests = FindObjectsOfType<BeastStorageContainerController>();
        foreach (BeastStorageContainerController chest in allChests)
        {
            if (_id == chest.gameObject.name)
            {
                chest.inUse = _inUse;
            }
        }

    }
    public void RebuildNavMesh()
    {
        NavMeshSurface[] surfaces = FindObjectsOfType<NavMeshSurface>();

        foreach (NavMeshSurface surface in surfaces)
        {
            surface.BuildNavMesh();
        }
    }
    public void CallSpellCirclePedestalPRC(string circleId, int itemIndex, int pedestalIndex, bool removeItem)
    {
        m_PhotonView.RPC("SpellCirclePedestalPRC", RpcTarget.AllBuffered, circleId, itemIndex, pedestalIndex, removeItem);

    }
    [PunRPC]
    public void SpellCirclePedestalPRC(string circleId, int itemIndex, int pedestalIndex, bool removeItem)
    {
        SpellCraftingManager[] spellCircles = FindObjectsOfType<SpellCraftingManager>();
        foreach (SpellCraftingManager spellCircle in spellCircles)
        {
            if (spellCircle.GetComponent<BuildingMaterial>().id == circleId)
            {
                if (spellCircle.transform.GetChild(pedestalIndex).TryGetComponent<SpellCirclePedestalInteraction>(out var pedestal))
                {
                    if (removeItem)
                    {
                        pedestal.hasItem = false;
                        if (pedestal.socket.childCount > 0)
                        {
                            Destroy(pedestal.socket.GetChild(0).gameObject);
                        }
                        pedestal.currentItem = null;
                    }
                    else
                    {
                        GameObject offeredObject = Instantiate(ItemManager.Instance.GetItemGameObjectByItemIndex(itemIndex), pedestal.socket);
                        Item currentItem = offeredObject.GetComponent<Item>();
                        currentItem.isEquipable = false;
                        pedestal.hasItem = true;
                        pedestal.currentItem = currentItem;
                    }
                }
                break;
            }
        }
    }

    public void CallSpellCircleProducePRC(string circleId, int productIndex)
    {
        m_PhotonView.RPC("SpellCircleProducePRC", RpcTarget.AllBuffered, circleId, productIndex);

    }
    [PunRPC]
    public void SpellCircleProducePRC(string circleId, int productIndex)
    {
        SpellCraftingManager[] spellCircles = FindObjectsOfType<SpellCraftingManager>();
        foreach (SpellCraftingManager spellCircle in spellCircles)
        {
            if (spellCircle.GetComponent<BuildingMaterial>().id == circleId)
            {
                StartCoroutine(CraftingEffectCoroutine(spellCircle, productIndex));
            }
        }
    }

    IEnumerator CraftingEffectCoroutine(SpellCraftingManager spellCircle, int productIndex)
    {
        // Instantiate and play the particle effect
        GameObject particleEffect = Instantiate(m_SpellCraftingSuccessParticleEffect, spellCircle.m_Alter.m_Socket.position, Quaternion.identity);
        m_SpellCraftingSuccessParticleEffect.GetComponent<ParticleSystem>().Play();

        // Wait for the particle effect to finish or for a set duration
        yield return new WaitForSeconds(m_SpellCraftingSuccessParticleEffectDuration); // particleEffectDuration is the time you want to wait

        // Destroy the particle effect if needed
        Destroy(particleEffect);

        // Spawn the crafted item
        GameObject product = Instantiate(ItemManager.Instance.GetItemGameObjectByItemIndex(productIndex), spellCircle.m_Alter.m_Socket.position, Quaternion.identity);
        product.GetComponent<SpawnMotionDriver>().Land();
    }

    public void CallSaveObjectsPRC(string id, bool destroyed, string state = "")
    {
        m_PhotonView.RPC("SaveObjectsPRC", RpcTarget.AllBuffered, id, destroyed, state);
    }
    [PunRPC]
    public void SaveObjectsPRC(string id, bool destroyed, string state = "")
    {
        SaveObject(id, destroyed, state);
    }
    // Adds the object to the save data and saves the level
    public string SaveObject(string id, bool destroyed, string state = "")
    {
        string returnid = id;
        if (destroyed)
        {
            int startLength = saveData.objects.Length;
            // If id is in saveData.objects, remove it.
            if (saveData.objects.Length > 0)
            {
                foreach (string obj in saveData.objects)
                {
                    if (obj == id)
                    {
                        List<string> list = new List<string>(saveData.objects);
                        list.Remove(obj);
                        saveData.objects = list.ToArray();
                        break;
                    }
                }
            }
            //was not removed
            if (saveData.objects.Length == startLength)
            {
                List<string> list = new List<string>(saveData.removedObjects);
                list.Add(id);
                saveData.removedObjects = list.ToArray();
            }
        }
        else
        {
            // Find the index of the last underscore to separate the ID
            int underscoreIndex = id.LastIndexOf('_');
            string baseId = id.Substring(0, underscoreIndex + 1); // Include the underscore
            string fullId = baseId + state;

            Item[] allItems = FindObjectsOfType<Item>();
            Item itemToUpdate = null;

            foreach (Item _item in allItems)
            {
                if (_item.id == id)
                {
                    itemToUpdate = _item;
                }
            }
            if (itemToUpdate == null)
            {
                Debug.LogError("~ Item with id (" + id + ") does not exist");
            }
            else
            {
                itemToUpdate.id = fullId;
            }


            // Check if the object ID already exists and update the state data if it does
            bool idExists = false;
            for (int i = 0; i < saveData.objects.Length; i++)
            {
                if (saveData.objects[i].StartsWith(baseId))
                {
                    saveData.objects[i] = fullId; // Update the existing entry with new state data
                    idExists = true;
                    break;
                }
            }

            // If the ID doesn't exist, add it as a new entry
            if (!idExists)
            {
                List<string> objectsList = saveData.objects.ToList();
                objectsList.Add(fullId); // Add the full ID with state data
                saveData.objects = objectsList.ToArray();
            }
            returnid = fullId;
        }
        SaveLevel();
        return returnid;
    }


    public void SaveLevel()
    {
        if (SceneManager.GetActiveScene().name == "HubWorld" || SceneManager.GetActiveScene().name == "TutorialWorld")
        {
            string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{LevelPrep.Instance.settlementName}/");
            Directory.CreateDirectory(saveDirectoryPath);
            string name = LevelPrep.Instance.currentLevel;
            string filePath = saveDirectoryPath + name + ".json";
            string json = JsonConvert.SerializeObject(saveData);
            // Open the file for writing
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                // Write the JSON string to the file
                writer.Write(json);
            }
            //SaveParty();
        }

    }
    public static LevelSaveData LoadLevel(string levelName)
    {
        if (SceneManager.GetActiveScene().name != "HubWorld") return new LevelSaveData(levelName);
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{LevelPrep.Instance.settlementName}/");
        Directory.CreateDirectory(saveDirectoryPath);
        string filePath = saveDirectoryPath + levelName + ".json";
        string json;
        try
        {
            json = File.ReadAllText(filePath);
            LevelSaveData data = JsonConvert.DeserializeObject<LevelSaveData>(json);
            return data;
        }
        catch
        {
            Debug.Log("~ Level Data does not exist");
            return new LevelSaveData(levelName);
        }
    }
    public static PartySaveData LoadParty(string settlementName)
    {
        return null;
    }
    public static void SaveParty()
    {
        if (!FindObjectOfType<GameStateManager>().initialized)
        {
            return;
        }
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{LevelPrep.Instance.settlementName}/");
        Directory.CreateDirectory(saveDirectoryPath);
        Vector3 playerPos = m_Controller.playersManager.playersCentralPosition;
        Debug.LogWarning("~ SavingLevel " + playerPos);
        GameStateManager.Instance.spawnPoint = playerPos;
        PartySaveData data = new PartySaveData(playerPos.x, playerPos.y, playerPos.z, m_Controller.currentRespawnPoint.x, m_Controller.currentRespawnPoint.y, m_Controller.currentRespawnPoint.z, GameStateManager.Instance.timeCounter, GameStateManager.Instance.sun.transform.rotation.eulerAngles.x);
        string json = JsonConvert.SerializeObject(data);
        string filePath = saveDirectoryPath + LevelPrep.Instance.settlementName + ".json";
        // Open the file for writing
        using (FileStream stream = new FileStream(filePath, FileMode.Create))
        using (StreamWriter writer = new StreamWriter(stream))
        {
            // Write the JSON string to the file
            writer.Write(json);
        }
    }
    public static void SaveParty(Vector3 SpawnPoint)
    {
        if (!FindObjectOfType<GameStateManager>().initialized)
        {
            return;
        }
        string levelName = LevelPrep.Instance.settlementName;
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{LevelPrep.Instance.settlementName}/");
        Directory.CreateDirectory(saveDirectoryPath);
        Vector3 playerPos = m_Controller.playersManager.playersCentralPosition;
        Debug.LogWarning("~ SavingLevel " + playerPos);
        GameStateManager.Instance.spawnPoint = playerPos;
        PartySaveData data = new PartySaveData(SpawnPoint.x, SpawnPoint.y, SpawnPoint.z, SpawnPoint.x, SpawnPoint.y, SpawnPoint.z, GameStateManager.Instance.timeCounter, GameStateManager.Instance.sun.transform.rotation.x);
        string json = JsonConvert.SerializeObject(data);
        string filePath = saveDirectoryPath + LevelPrep.Instance.settlementName + ".json";
        // Open the file for writing
        using (FileStream stream = new FileStream(filePath, FileMode.Create))
        using (StreamWriter writer = new StreamWriter(stream))
        {
            // Write the JSON string to the file
            writer.Write(json);
        }
    }

    //This is for non master clients to take the level data provided when they join and to save it so the world can use the level data
    public void SaveProvidedLevelData(string levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("No level data to load " + PhotonNetwork.LocalPlayer.UserId);
            return;
        }
        string[] separateFileStrings = levelData.Split(new string[] { "|-|" }, StringSplitOptions.RemoveEmptyEntries);
        string settlementName = LevelPrep.Instance.settlementName;
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{settlementName}/");
        try
        {
            Directory.Delete(saveDirectoryPath, true);
        }
        catch
        {
            Debug.LogWarning("No existing directory to remove for level");
        }
        Directory.CreateDirectory(saveDirectoryPath);
        for (int i = 0; i < separateFileStrings.Length; i++)
        {
            LevelSaveData level = JsonConvert.DeserializeObject<LevelSaveData>(separateFileStrings[i]);
            GameSaveData saveData = JsonConvert.DeserializeObject<GameSaveData>(separateFileStrings[i]);
            if (level.id != null)
            {
                string filePath;
                filePath = saveDirectoryPath + level.id + ".json";
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(separateFileStrings[i]);
                }
            }
            if (saveData != null)
            {
                worldProgress = saveData.gameProgress;
                string filePath = saveDirectoryPath + "GameProgress.json";
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(separateFileStrings[i]);
                }
            }
        }
        LevelPrep.Instance.receivedLevelFiles = true;
    }


    public void OpenCraftingBench(string id)
    {
        m_PhotonView.RPC("PackItemRPC", RpcTarget.AllBuffered, id);
    }
    [PunRPC]
    public void PackItemRPC(string id)
    {
        PackableItem[] packabels = FindObjectsOfType<PackableItem>();
        foreach (PackableItem item in packabels)
        {
            if (item.GetComponent<Item>().id == id)
            {
                Item _item = item.GetComponent<Item>();
                if (item.packed)
                {
                    // _item.SaveItem(_item.parentChunk, false, "Packed");

                }
                else
                {
                    //_item.SaveItem(_item.parentChunk, false, "");
                }
                //_item.SaveItem(_item.parentChunk, false);
                item.PackAndSave(item.gameObject);
            }
        }
    }
    public void CallPlaceObjectPRC(int activeChildIndex, Vector3 position, Vector3 rotation, string id, bool isPacked)
    {
        m_PhotonView.RPC("PlaceObjectPRC", RpcTarget.AllBuffered, activeChildIndex, position, rotation, id, isPacked);
    }

    [PunRPC]
    void PlaceObjectPRC(int activeChildIndex, Vector3 _position, Vector3 _rotation, string id, bool isPacked)
    {
        GameObject newObject = ItemManager.Instance.environmentItemList[activeChildIndex];
        GameObject finalObject = Instantiate(newObject, _position, Quaternion.Euler(_rotation));
        //Check the final object for a source object script and set the ID
        SourceObject so = finalObject.GetComponent<SourceObject>();
        if (so != null)
        {
            so.id = GenerateObjectId.GenerateSourceObjectId(so);
        }
        else// If no source object is found, we need to set the id on the item.
        {   // This is for crafting benches and fire pits.
            finalObject.GetComponent<Item>().id = GenerateObjectId.GenerateItemId(finalObject.GetComponent<Item>());
        }
        finalObject.GetComponent<BuildingObject>().isPlaced = true;
        string state = isPacked ? "Packed" : "";
        if (isPacked)
        {
            finalObject.GetComponent<PackableItem>().PackAndSave(finalObject);
        }
        LevelManager.Instance.SaveObject(id, false, state);
    }

    public void CallUpdateObjectsPRC(string objectId, int damage, ToolType toolType, Vector3 hitPos, PhotonView attacker)
    {
        m_PhotonView.RPC("UpdateObject_PRC", RpcTarget.All, objectId, damage, toolType, hitPos, attacker.ViewID);
    }

    [PunRPC]
    public void UpdateObject_PRC(string objectId, int damage, ToolType toolType, Vector3 hitPos, int attackerViewId)
    {
        PhotonView attacker = PhotonView.Find(attackerViewId);

        // Get all SourceObjects in the scene
        SourceObject[] sourceObjects = FindObjectsOfType<SourceObject>();
        foreach (var so in sourceObjects)
        {
            if (so.id == objectId)
            {
                so.TakeDamage(damage, toolType, hitPos, attacker.gameObject);
                return; // Exit the method if the object is found and damage applied
            }
        }

        // Get all BuildingMaterials in the scene
        BuildingMaterial[] buildingMaterials = FindObjectsOfType<BuildingMaterial>();
        foreach (var bm in buildingMaterials)
        {
            if (bm.id == objectId && bm.GetComponent<HealthManager>() != null)
            {
                bm.GetComponent<HealthManager>().TakeHit(damage, toolType, hitPos, attacker.gameObject);
                return; // Exit the method if the object is found and damage applied
            }
        }
    }

    public void CallUpdateItemsRPC(string itemId)
    {
        m_PhotonView.RPC("UpdateItems_RPC", RpcTarget.AllBuffered, itemId);
    }

    [PunRPC]
    public void UpdateItems_RPC(string itemId)
    {
        Item[] items = FindObjectsOfType<Item>();
        foreach (Item item in items)
        {
            if (item.id == itemId && item.gameObject != null)
            {
                Destroy(item.gameObject);
            }
        }
    }
    public void CallUpdateFirePitRPC(string firePitId)
    {
        m_PhotonView.RPC("UpdateFirePit_RPC", RpcTarget.AllBuffered, firePitId);
    }

    [PunRPC]
    public void UpdateFirePit_RPC(string firePitId)
    {
        BuildingMaterial[] items = FindObjectsOfType<BuildingMaterial>();
        foreach (BuildingMaterial item in items)
        {
            if (item.id == firePitId)
            {
                item.GetComponent<FirePitInteraction>().StokeFire();
            }
        }
    }

    public void CallChangeLevelRPC(string LevelName, string spawnName)
    {
        LevelPrep.Instance.playerSpawnName = spawnName;
        m_PhotonView.RPC("UpdateLevelInfo_RPC", RpcTarget.MasterClient, LevelName);
    }

    [PunRPC]
    public void UpdateLevelInfo_RPC(string LevelName)
    {
        GameStateManager.Instance.setLoadingScreenOn();
        LevelPrep.Instance.currentLevel = LevelName;
        m_PhotonView.RPC("LoadLevel_RPC", RpcTarget.AllBuffered, LevelName);
    }

    [PunRPC]
    public void LoadLevel_RPC(string LevelName)
    {
        LevelPrep.Instance.currentLevel = LevelName;
        SceneManager.LoadScene(LevelName);
    }
}

public class PartySaveData
{
    public float playerPosX;
    public float playerPosY;
    public float playerPosZ;
    public float respawnPosX;
    public float respawnPosY;
    public float respawnPosZ;
    public float time;
    public float sunRot;
    public PartySaveData(float playerPosX, float playerPosY, float playerPosZ, float respawnPosX, float respawnPosY, float respawnPosZ, float time, float sunRot)
    {
        this.playerPosX = playerPosX;
        this.playerPosY = playerPosY;
        this.playerPosZ = playerPosZ;
        this.respawnPosX = respawnPosX;
        this.respawnPosY = respawnPosY;
        this.respawnPosZ = respawnPosZ;
        this.time = time;
        this.sunRot = sunRot;
    }
}
public class LevelSaveData
{
    public string id;
    public string[] objects;
    public string[] removedObjects;
    public LevelSaveData() { }
    public LevelSaveData(string[] objects, string[] removedObjects, string id)
    {
        this.objects = objects;
        this.removedObjects = removedObjects;
        this.id = id;
    }
    public LevelSaveData(string id)
    {
        this.objects = new string[0];
        this.removedObjects = new string[0];
        this.id = id;
    }
}

public class GameSaveData
{
    public int gameProgress;

    public GameSaveData(int gameProgress)
    {
        this.gameProgress = gameProgress;
    }
}


public class ObjectPool
{
    public GameObject prefab;
    public Stack<GameObject> pool;

    public ObjectPool(GameObject prefab, int initialSize)
    {

        this.prefab = prefab;
        pool = new Stack<GameObject>(initialSize);

        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = GameObject.Instantiate(prefab);
            obj.SetActive(false);
            pool.Push(obj);
        }
    }

    public GameObject GetObject()
    {
        if (pool.Count > 0)
        {
            GameObject newObj = pool.Pop();
            newObj.SetActive(true);
            return newObj;
        }
        else
        {
            GameObject obj = GameObject.Instantiate(prefab);
            obj.SetActive(true);
            return obj;
        }
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        pool.Push(obj);
    }
}