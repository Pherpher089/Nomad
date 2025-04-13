using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Collections;
using Photon.Pun;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Pathfinding;
using Path = System.IO.Path;
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public const string LevelDataKey = "levelData";
    public PhotonView m_PhotonView;
    Transform parentTerrain;
    public LevelSaveData saveData;

    //TODO wondering if this should live somewhere else
    //Spell Crafting stuff.
    public GameObject m_SpellCraftingSuccessParticleEffect;
    public float m_SpellCraftingSuccessParticleEffectDuration = 1f;
    public Material[] playerMaterials;
    public Color[] playerColors;
    public int worldProgress;
    public int beastLevel;
    public List<Item> allItems = new List<Item>();
    void Awake()
    {
        if (SceneManager.GetActiveScene().name == "LoadingScene") return;
        // if (Instance != null && Instance != this)
        // {
        //     Destroy(gameObject);
        // }
        DontDestroyOnLoad(gameObject);
        Instance = this;
        m_PhotonView = GetComponent<PhotonView>();
    }


    public void InitializeLevel(string levelName)
    {
        if (SceneManager.GetActiveScene().name == "LoadingScene" || SceneManager.GetActiveScene().name == "MainMenu") return;
        saveData = LoadLevel(levelName);
        if (saveData == null)
        {
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
        if (scene.name != "MainMenu" || scene.name != "LoadingScene")
        {
            InitializeLevel(scene.name);
        }
    }
    public void FinishTutorial()
    {
        worldProgress = 1;
        CallSaveGameProgress(worldProgress, beastLevel);
    }

    public void AddItemsToMasterList(Item item)
    {
        allItems.Add(item);
    }
    public void RemoveItemsFromMasterList(Item item)
    {
        allItems.Remove(item);
    }
    public void PopulateObjects()
    {
        StartCoroutine(PopulateObjectsCoroutine());
    }
    IEnumerator PopulateObjectsCoroutine()
    {
        List<SourceObject> objectsInLevel = new(parentTerrain.transform.GetComponentsInChildren<SourceObject>());

        if (saveData != null && saveData.removedObjects != null && saveData.removedObjects.Length > 0)
        {
            List<string> removedList = new(saveData.removedObjects);
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

                string baseId = obj.Substring(0, obj.LastIndexOf('_'));
                //Get the object prefab from the item manager with the item index at index 0
                bool alreadyExists = false;
                BuildingMaterial[] allBuildingMats = FindObjectsOfType<BuildingMaterial>();
                foreach (BuildingMaterial mat in allBuildingMats)
                {
                    if (mat.id.Contains(baseId))
                    {
                        mat.id = obj;
                        alreadyExists = true;
                        break;
                    }

                }
                if (alreadyExists) continue;
                GameObject newObject = Instantiate(ItemManager.Instance.environmentItemList[int.Parse(splitData[0])]);
                UpdateGraphForNewStructure(newObject);
                newObject.transform.SetParent(parentTerrain);
                newObject.transform.SetPositionAndRotation(new Vector3(float.Parse(splitData[1]), float.Parse(splitData[2]), float.Parse(splitData[3])), Quaternion.Euler(new Vector3(0, float.Parse(splitData[4]), 0)));
                if (newObject.TryGetComponent<TentManager>(out var tent))
                {
                    GameStateManager.Instance.currentTent = tent;
                }
                if (newObject.TryGetComponent<SourceObject>(out var so))
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
    public void CallUpdatePlayerColorPRC(int viewID, int colorIndex, int playerNum)
    {
        m_PhotonView.RPC("UpdatePlayerColorPRC", RpcTarget.AllBuffered, viewID, colorIndex, playerNum);
    }
    [PunRPC]
    public void UpdatePlayerColorPRC(int viewID, int colorIndex, int playerNum)
    {
        PhotonView targetView = PhotonView.Find(viewID);
        targetView.transform.GetComponentInChildren<SkinnedMeshRenderer>().material = playerMaterials[colorIndex];
        targetView.transform.GetComponentInChildren<CircularStatBarSliderController>().transform.GetChild(colorIndex).gameObject.SetActive(true);
        GameStateManager.Instance.hudControl.hudParent.backgroundIndices.Add(colorIndex);
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

    public void CallSaveGameProgress(int progress, int beastLevel = 0)
    {
        m_PhotonView.RPC("SaveGameProgress", RpcTarget.All, progress, beastLevel);
    }

    [PunRPC]
    public void SaveGameProgress(int progress, int beastLevel)
    {
        worldProgress = progress;
        this.beastLevel = beastLevel;
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{LevelPrep.Instance.settlementName}/");
        Directory.CreateDirectory(saveDirectoryPath);
        string name = LevelPrep.Instance.currentLevel;
        string filePath = saveDirectoryPath + "GameProgress.json";
        string json = JsonConvert.SerializeObject(new GameSaveData(progress, beastLevel));
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
    public void CallSpellCirclePedestalPRC(string circleId, int itemIndex, int pedestalIndex, bool removeItem)
    {
        m_PhotonView.RPC("SpellCirclePedestalPRC", RpcTarget.AllBuffered, circleId, itemIndex, pedestalIndex, removeItem, UnityEngine.Random.Range(0, 1000).ToString());

    }
    [PunRPC]
    public void SpellCirclePedestalPRC(string circleId, int itemIndex, int pedestalIndex, bool removeItem, string spawnIdSalt)
    {
        StationCraftingManager[] spellCircles = FindObjectsOfType<StationCraftingManager>();
        foreach (StationCraftingManager spellCircle in spellCircles)
        {
            if (spellCircle.GetComponent<BuildingMaterial>().id == circleId)
            {
                if (spellCircle.transform.GetChild(pedestalIndex).TryGetComponent<StationPedestalInteraction>(out var pedestal))
                {
                    if (removeItem)
                    {

                        pedestal.hasItem = false;
                        if (pedestal.m_Socket.childCount > 0)
                        {
                            pedestal.currentItem.transform.parent = null;
                            Destroy(pedestal.currentItem.gameObject);
                        }
                        pedestal.currentItem = null;
                    }
                    else
                    {
                        GameObject offeredObject = Instantiate(ItemManager.Instance.GetItemGameObjectByItemIndex(itemIndex), pedestal.m_Socket);
                        Item currentItem = offeredObject.GetComponent<Item>();
                        currentItem.isEquipable = false;
                        pedestal.hasItem = true;
                        pedestal.currentItem = currentItem;
                        currentItem.spawnId = $"{circleId}_{spawnIdSalt}";
                    }
                }
                break;
            }
        }
    }

    public void CallSpellCircleProducePRC(string circleId, int productIndex)
    {
        int salt = UnityEngine.Random.Range(0, 1000);
        m_PhotonView.RPC("SpellCircleProducePRC", RpcTarget.AllBuffered, circleId, productIndex, salt);

    }
    [PunRPC]
    public void SpellCircleProducePRC(string circleId, int productIndex, int salt)
    {
        StationCraftingManager[] spellCircles = FindObjectsOfType<StationCraftingManager>();
        foreach (StationCraftingManager spellCircle in spellCircles)
        {
            if (spellCircle.GetComponent<BuildingMaterial>().id == circleId)
            {
                StartCoroutine(CraftingEffectCoroutine(spellCircle, productIndex, salt));
            }
        }
    }
    public void CallBeastCraftPRC(string circleId, string uiMessage)
    {
        int salt = UnityEngine.Random.Range(0, 1000);
        m_PhotonView.RPC("BeastCraftRPC", RpcTarget.AllBuffered, circleId, uiMessage);

    }
    [PunRPC]
    public void BeastCraftRPC(string circleId, string uiMessage)
    {
        StationCraftingManager[] spellCircles = FindObjectsOfType<StationCraftingManager>();
        foreach (StationCraftingManager spellCircle in spellCircles)
        {
            if (spellCircle.GetComponent<BuildingMaterial>().id == circleId)
            {
                StartCoroutine(BeastCraftingEffectCoroutine(spellCircle, uiMessage));
            }
        }
    }

    IEnumerator CraftingEffectCoroutine(StationCraftingManager spellCircle, int productIndex, int salt)
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
        product.GetComponent<Item>().spawnId = $"{spellCircle.GetComponent<BuildingMaterial>().id}_{salt}";
    }
    IEnumerator BeastCraftingEffectCoroutine(StationCraftingManager spellCircle, string uiMessage)
    {
        // Instantiate and play the particle effect
        GameObject particleEffect = Instantiate(m_SpellCraftingSuccessParticleEffect, spellCircle.m_Alter.m_Socket.position, Quaternion.identity);
        m_SpellCraftingSuccessParticleEffect.GetComponent<ParticleSystem>().Play();

        // Wait for the particle effect to finish or for a set duration
        yield return new WaitForSeconds(m_SpellCraftingSuccessParticleEffectDuration); // particleEffectDuration is the time you want to wait

        // Destroy the particle effect if needed
        Destroy(particleEffect);
        spellCircle.uiMessage.text = uiMessage;
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
                    if (id != "" && id != null && obj[..obj.LastIndexOf('_')] == id[..id.LastIndexOf('_')])
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
                List<string> list = new List<string>(saveData.removedObjects)
                {
                    id
                };
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
            }
            else
            {
                itemToUpdate.id = fullId;
            }


            // Check if the object ID already exists and update the state data if it does
            bool idExists = false;
            if (saveData != null && saveData.objects != null && saveData.objects.Length > 0)
            {

                for (int i = 0; i < saveData.objects.Length; i++)
                {
                    if (saveData.objects[i].StartsWith(baseId))
                    {
                        saveData.objects[i] = fullId; // Update the existing entry with new state data
                        idExists = true;
                        break;
                    }
                }
            }
            // If the ID doesn't exist, add it as a new entry
            if (!idExists)
            {
                if (saveData == null)
                {
                    saveData = new LevelSaveData();
                }

                List<string> objectsList;
                if (saveData != null && saveData.objects != null)
                {
                    objectsList = saveData.objects.ToList();
                }
                else
                {
                    objectsList = new List<string>();
                }
                objectsList.Add(fullId); // Add the full ID with state data

                saveData.objects = objectsList.ToArray();
            }
            returnid = fullId;
        }
        if (PhotonNetwork.IsMasterClient)
        {
            UpdateRoomPropertySaveData(saveData);
        }
        // SaveLevel();
        return returnid;
    }
    public void SaveLevel()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != "HubWorld" && sceneName != "TutorialWorld" && GameStateManager.Instance.currentTent != null)
        {

            // Potentially need to filter through the destroyed objects and see if any land in the new tent bounds
            List<string> removesToKeep = new List<string>();
            foreach (string obj in saveData.removedObjects)
            {
                string[] splitData = obj.Split('_');
                if (GameStateManager.Instance.currentTent.transform.GetChild(0).GetComponent<BoxCollider>().bounds.Contains(new Vector3(float.Parse(splitData[1]), float.Parse(splitData[2]), float.Parse(splitData[3]))))
                {
                    removesToKeep.Add(obj);
                }
            }

            List<string> addsToKeep = new List<string>();
            foreach (string obj in saveData.objects)
            {
                string[] splitData = obj.Split('_');
                if (GameStateManager.Instance.currentTent.transform.GetChild(0).GetComponent<BoxCollider>().bounds.Contains(new Vector3(float.Parse(splitData[1]), float.Parse(splitData[2]), float.Parse(splitData[3]))))
                {
                    addsToKeep.Add(obj);
                }
            }

            saveData = new LevelSaveData(sceneName)
            {
                objects = addsToKeep.ToArray(),
                removedObjects = removesToKeep.ToArray()
            };
        }
        else if (sceneName != "HubWorld" && sceneName != "TutorialWorld" && GameStateManager.Instance.currentTent == null)
        {
            return;
        }

        // Filter save data if tent
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
        if (PhotonNetwork.IsMasterClient)
        {
            UpdateRoomPropertySaveData(saveData);
        }
    }

    void UpdateRoomPropertySaveData(LevelSaveData saveData)
    {
        string json = JsonConvert.SerializeObject(saveData);
        ExitGames.Client.Photon.Hashtable playerProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        playerProperties[LevelDataKey] = json;
        PhotonNetwork.CurrentRoom.SetCustomProperties(playerProperties);
    }
    public static LevelSaveData LoadLevel(string levelName)
    {
        //if (SceneManager.GetActiveScene().name != "HubWorld" && SceneManager.GetActiveScene().name != "TutorialWorld") return new LevelSaveData(levelName);
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
        Vector3 playerPos = GameStateManager.Instance.playersManager.playersCentralPosition;
        Debug.LogWarning("~ SavingLevel " + playerPos);
        GameStateManager.Instance.spawnPoint = playerPos;
        PartySaveData data = new PartySaveData(playerPos.x, playerPos.y, playerPos.z, GameStateManager.Instance.currentRespawnPoint.x, GameStateManager.Instance.currentRespawnPoint.y, GameStateManager.Instance.currentRespawnPoint.z, GameStateManager.Instance.timeCounter, GameStateManager.Instance.sun.transform.rotation.eulerAngles.x);
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
        Vector3 playerPos = GameStateManager.Instance.playersManager.playersCentralPosition;
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
            //ere
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
            else if (saveData != null)
            {
                Debug.Log("Updating world progress: " + saveData.gameProgress);

                worldProgress = saveData.gameProgress;
                beastLevel = saveData.beastLevel;
                string filePath = saveDirectoryPath + "GameProgress.json";
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(separateFileStrings[i]);
                }
            }
            else
            {
                worldProgress = 0;
                beastLevel = 0;
            }
        }
        LevelPrep.Instance.receivedLevelFiles = true;
    }
    public void CallSetPartySpawnCriteria()
    {
        m_PhotonView.RPC("SetPartySpawnCriteria", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void SetPartySpawnCriteria()
    {
        switch (worldProgress)
        {
            case 0:
                LevelPrep.Instance.currentLevel = "HubWorld";
                LevelPrep.Instance.playerSpawnName = "start-tutorial";
                break;
            case 1:
                LevelPrep.Instance.currentLevel = "HubWorld";
                LevelPrep.Instance.playerSpawnName = "start";
                break;
        }
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
            string _id = GenerateObjectId.GenerateItemId(finalObject.GetComponent<Item>());
            Item _item = finalObject.GetComponent<Item>();
            _item.id = _id;
            _item.spawnId = _id;
        }
        finalObject.GetComponent<BuildingObject>().isPlaced = true;
        if (finalObject.TryGetComponent<BeastStableController>(out var beastStable))
        {
            beastStable.m_BeastObject = BeastManager.Instance.gameObject;
        }

        if (finalObject.TryGetComponent<TentManager>(out var _tent))
        {
            if (GameStateManager.Instance.currentTent != null)
            {
                GameStateManager.Instance.currentTent.GetComponent<HealthManager>().TakeHit(100);
            }
            GameStateManager.Instance.currentTent = _tent;
        }
        SaveObject(id, false);
        UpdateGraphForNewStructure(finalObject);
    }

    public void UpdateGraphForNewStructure(GameObject builtStructure)
    {
        //AstarPath.active.Scan();
        StartCoroutine(StartScan(builtStructure));
    }

    IEnumerator StartScan(GameObject builtStructure)
    {
        yield return new WaitForSeconds(1);
        Bounds structureBounds = builtStructure.GetComponent<Collider>().bounds;

        GraphUpdateObject guo = new GraphUpdateObject(structureBounds);
        AstarPath.active.UpdateGraphs(guo);
    }
    public void CallOpenDoorSourceObjectPRC(string objectId)
    {
        m_PhotonView.RPC("UpdateDoorSourceObject_PRC", RpcTarget.AllBuffered, objectId);
    }

    [PunRPC]
    public void UpdateDoorSourceObject_PRC(string objectId)
    {

        // Get all SourceObjects in the scene
        SourceObject[] sourceObjects = FindObjectsOfType<SourceObject>();
        foreach (var so in sourceObjects)
        {
            if (so.id == objectId)
            {
                so.GetComponentInChildren<DoorControl>().OpenDoor();
            }
        }
    }
    public void CallOpenDoorBuildingMaterialPRC(string objectId)
    {
        m_PhotonView.RPC("UpdateDoorBuildingMaterial_PRC", RpcTarget.AllBuffered, objectId);
    }
    [PunRPC]
    public void UpdateDoorBuildingMaterial_PRC(string objectId)
    {

        // Get all SourceObjects in the scene
        BuildingMaterial[] buildingMaterials = FindObjectsOfType<BuildingMaterial>();
        foreach (var bm in buildingMaterials)
        {
            if (bm.id == objectId)
            {
                foreach (DoorControl door in bm.GetComponentsInChildren<DoorControl>())
                {
                    door.OpenDoor();
                }
            }
        }
    }
    public void CallUpdateObjectsPRC(string objectId, string spawnId, int damage, ToolType toolType, Vector3 hitPos, PhotonView attacker)
    {
        m_PhotonView.RPC("UpdateObject_PRC", RpcTarget.All, objectId, spawnId, damage, toolType, hitPos, attacker.ViewID, 0f);
    }

    [PunRPC] // Syncs up attacking objects across the clients
    public void UpdateObject_PRC(string objectId, string spawnId, int damage, ToolType toolType, Vector3 hitPos, int attackerViewId, float knockBackForce)
    {
        // Get all BuildingMaterials in the scene
        BuildingMaterial[] buildingMaterials = FindObjectsOfType<BuildingMaterial>();
        PhotonView attacker = PhotonView.Find(attackerViewId);
        foreach (var bm in buildingMaterials)
        {
            if ((bm.id == objectId || bm.spawnId != null && bm.spawnId != "" && bm.spawnId == spawnId) && bm.GetComponent<HealthManager>() != null)
            {

                bm.GetComponent<HealthManager>().TakeHit(damage, toolType, hitPos, attacker.gameObject, knockBackForce);
                return; // Exit the method if the object is found and damage applied
            }
        }
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
    }
    public void CallShutOffObjectRPC(string id, bool save = true)
    {
        m_PhotonView.RPC("ShutOffObjectRPC_RPC", RpcTarget.AllBuffered, id, save);
    }

    [PunRPC]
    public void ShutOffObjectRPC_RPC(string id, bool save)
    {
        SourceObject[] objects = FindObjectsOfType<SourceObject>();
        foreach (SourceObject @object in objects)
        {
            if (@object.id == id && @object.gameObject != null)
            {
                @object.ShutOffObject(@object.gameObject, save);
            }
        }
    }
    public void CallShutOffBuildingMaterialRPC(string id, bool save = true)
    {
        m_PhotonView.RPC("ShutOffBuildingMaterialRPC_RPC", RpcTarget.AllBuffered, id, save);
    }

    [PunRPC]
    public void ShutOffBuildingMaterialRPC_RPC(string id, bool save)
    {
        BuildingMaterial[] objects = FindObjectsOfType<BuildingMaterial>();
        foreach (BuildingMaterial @object in objects)
        {
            if (@object.id != "" && @object.id[..@object.id.LastIndexOf("_")] == id[..id.LastIndexOf("_")] && @object.gameObject != null)
            {
                @object.ShutOffObject(@object.gameObject, save);
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
            if (item.spawnId == itemId && item.gameObject != null)
            {
                RemoveItemsFromMasterList(item);
                Destroy(item.gameObject);
            }
        }
    }
    public void CallUpdateInteractionResourceRPC(string nodeId, int itemIndex, int counter)
    {
        m_PhotonView.RPC("UpdateNodeResource_RPC", RpcTarget.AllBuffered, nodeId, itemIndex, counter);
    }

    [PunRPC]
    public void UpdateNodeResource_RPC(string nodeId, int itemIndex, int counter)
    {
        ManaGeyserInteraction[] geysers = FindObjectsOfType<ManaGeyserInteraction>();
        foreach (ManaGeyserInteraction geyser in geysers)
        {
            if (geyser.m_NodeId == nodeId && geyser.gameObject != null && geyser.m_CurrentResource > 0)
            {
                geyser.m_Counter = counter; //Syncs counter across clients
                geyser.m_CurrentResource--;
                PlayerInventoryManager.Instance.DropItem(itemIndex, geyser.transform.position);
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
    public int beastLevel;

    public GameSaveData(int gameProgress, int beastLevel)
    {
        this.gameProgress = gameProgress;
        this.beastLevel = beastLevel;
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