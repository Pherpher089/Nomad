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

public class LevelManager : MonoBehaviour
{
    private static GameStateManager gameController;
    private int initFrameCounter = 0;
    public static LevelManager Instance;
    public const string LevelDataKey = "levelData";
    public PhotonView pv;
    public bool initialized = false;
    Transform parentTerrain;
    LevelSaveData saveData;

    void Awake()
    {
        Instance = this;
        pv = GetComponent<PhotonView>();
        DontDestroyOnLoad(gameObject);
    }
    public void InitializeLevel(string levelName)
    {
        saveData = LoadLevel(levelName);
        if(saveData == null)
        {
            saveData = new LevelSaveData(levelName);
        }
        parentTerrain = GameObject.FindWithTag("WorldTerrain").transform;
        PopulateObjects(levelName);
    }
    public void PopulateObjects(string levelName)
    {
        StartCoroutine(PopulateObjectsCoroutine(levelName));
    }
    IEnumerator PopulateObjectsCoroutine(string levelName)
    {
        HashSet<string> instantiatedObjectIds = new HashSet<string>();

        if (saveData != null && saveData.objects != null && saveData.objects.Length > 0 && saveData.objects[0] != null)
        {
            foreach (string objId in saveData.objects)
            {
                // If this object has already been instantiated, skip to the next one
                if (instantiatedObjectIds.Contains(objId))
                {
                    continue;
                }
                string[] saveDataArr = objId.Split("_");
                GameObject _obj = saveDataArr[5] == "True" ? ItemManager.Instance.itemList[int.Parse(saveDataArr[1])] : ItemManager.Instance.environmentItemList[int.Parse(saveDataArr[1])];

                GameObject newObj = Instantiate(_obj, new Vector3(float.Parse(saveDataArr[1]), 0, float.Parse(saveDataArr[2])), Quaternion.Euler(0, float.Parse(saveDataArr[3]), 0));
                BuildingMaterial bm = newObj.GetComponent<BuildingMaterial>();
                if (saveDataArr[5] == "True")
                {
                    if (bm == null) newObj.GetComponent<SpawnMotionDriver>().hasSaved = true;
                    newObj.GetComponent<Item>().hasLanded = true;
                }
                newObj.GetComponent<Rigidbody>().isKinematic = true;
                newObj.transform.SetParent(parentTerrain);
                if (saveDataArr[5] == "True")
                {
                    if (bm != null)
                    {
                        bm.id = objId;
                    }
                    else
                    {
                        Item itm = newObj.GetComponent<Item>();
                        itm.id = objId;
                    }
                }
                else
                {
                    newObj.GetComponent<SourceObject>().id = objId;
                }
                if (saveDataArr[6] != "")
                {
                    string sateData = saveDataArr[4];
                    switch (int.Parse(saveDataArr[1]))
                    {
                        case 9:
                            if (saveDataArr[6] == "Packed")
                            {
                                newObj.GetComponent<PackableItem>().PackAndSave(newObj);
                            }
                            break;
                    }
                }
                instantiatedObjectIds.Add(objId);
            }
        }
        yield return null;
    }

    // This one should update the level data so that it is available to new clients when they join.
    public RoomOptions UpdateLevelData()
    {
        string levelName = FindObjectOfType<LevelPrep>().settlementName;
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{levelName}/");
        Directory.CreateDirectory(saveDirectoryPath);
        string[] filePaths = Directory.GetFiles(saveDirectoryPath);

        // Read file contents and add to levelData
        List<string> levelDataList = new List<string>();
        foreach (string filePath in filePaths)
        {
            int retries = 5;
            string fileContent = "";
            while (retries > 0)
            {
                try
                {
                    fileContent = File.ReadAllText(filePath);
                    retries = 0;
                }
                catch (IOException)
                {
                    if (retries <= 0)
                        throw; // If we've retried enough times, rethrow the exception.
                    retries--;
                    Thread.Sleep(1000); // Wait a second before retrying.
                }
            }
            if (fileContent != "") levelDataList.Add(fileContent);
        }

        // Convert the list of strings to a single string
        string levelData = string.Join("|-|", levelDataList);

        // Pass level data to network
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { LevelDataKey, levelData } });
        }
        return null;
    }

    // Adds the object to the save data and saves the level
    public void SaveObject(string id, bool destroyed, string state = "")
    {
        Debug.Log("### are we even saving the object?");

        if (destroyed)
        {
            Debug.Log("### destroyed?");

            // If id is in saveData.objects, remove it.
            saveData.objects = saveData.objects.Where(obj => obj != id).ToArray();

            // If it's not in saveData.objects, then add it to saveData.removedObjects.
            if (!saveData.objects.Contains(id) && !saveData.removedObjects.Contains(id))
            {
                Debug.Log("### could not find existing object");
                List<string> removedObjectsList = saveData.removedObjects.ToList();
                removedObjectsList.Add(id);
                saveData.removedObjects = removedObjectsList.ToArray();
            }
        }
        else
        {
            Debug.Log("### not destroyed??");

            // Check if the id doesn't exist in saveData.objects and then add it.
            if (!saveData.objects.Contains(id))
            {
                List<string> objectsList = saveData.objects.ToList();
                objectsList.Add(id);
                saveData.objects = objectsList.ToArray();
            }
        }
        SaveLevel();
    }


    public void SaveLevel()
    {
        Debug.Log("### are we even saving the level?");
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{LevelPrep.Instance.settlementName}/");
        Directory.CreateDirectory(saveDirectoryPath);
        string name = saveData.id != null ? saveData.id : LevelPrep.Instance.currentLevel;
        string filePath = saveDirectoryPath + name + ".json";
        string json = JsonConvert.SerializeObject(saveData);
        // Open the file for writing
        using (FileStream stream = new FileStream(filePath, FileMode.Create))
        using (StreamWriter writer = new StreamWriter(stream))
        {
            Debug.Log("### are we here?");
            // Write the JSON string to the file
            writer.Write(json);
        }
        //SaveParty();
    } 
    public static LevelSaveData LoadLevel(string levelName)
    {
        string settlementName = LevelPrep.Instance.settlementName;
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{settlementName}/");
        Directory.CreateDirectory(saveDirectoryPath);
        string filePath = saveDirectoryPath + levelName + ".json";
        string json;
        try
        {
            json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<LevelSaveData>(json);
        }
        catch
        {
            Debug.Log("~ New Chunk. No data to load");
            return null;
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
        Vector3 playerPos = gameController.playersManager.playersCentralPosition;
        Debug.LogWarning("~ SavingLevel " + playerPos);
        GameStateManager.Instance.spawnPoint = playerPos;
        PartySaveData data = new PartySaveData(playerPos.x, playerPos.y, playerPos.z, gameController.currentRespawnPoint.x, gameController.currentRespawnPoint.y, gameController.currentRespawnPoint.z, GameStateManager.Instance.timeCounter, GameStateManager.Instance.sun.transform.rotation.eulerAngles.x);
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
        Vector3 playerPos = gameController.playersManager.playersCentralPosition;
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
        string levelName = LevelPrep.Instance.settlementName;
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{levelName}/");
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
            string filePath;
            if (i < separateFileStrings.Length - 1)
            {
                filePath = saveDirectoryPath + level.id + ".json";
            }
            else
            {
                filePath = saveDirectoryPath + levelName + ".json";
            }
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                // Write the JSON string to the file
                writer.Write(separateFileStrings[i]);
            }
        }
        LevelPrep.Instance.receivedLevelFiles = true;
    }


    public void CallPackItem(string id)
    {
        pv.RPC("PackItemRPC", RpcTarget.AllBuffered, id);
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
        pv.RPC("PlaceObjectPRC", RpcTarget.AllBuffered, activeChildIndex, position, rotation, id, isPacked);
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
            so.id = id;
        }
        else// If no source object is found, we need to set the id on the item.
        {   // This is for crafting benches and fire pits.
            finalObject.GetComponent<Item>().id = id;
        }
        finalObject.GetComponent<BuildingObject>().isPlaced = true;
        if (isPacked)
        {
            finalObject.GetComponent<PackableItem>().PackAndSave(finalObject);
        }
    }

    public void CallUpdateObjectsPRC(string objectId, int damage, ToolType toolType, Vector3 hitPos, PhotonView attacker)
    {
        pv.RPC("UpdateObject_PRC", RpcTarget.All, objectId, damage, toolType, hitPos, attacker.ViewID);
    }

    [PunRPC]
    public void UpdateObject_PRC(string objectId, int damage, ToolType toolType, Vector3 hitPos, int attackerViewId)
    {
        PhotonView attacker = PhotonView.Find(attackerViewId);
        GameObject terrain = GameObject.FindWithTag("WorldTerrain");
        int childCount = terrain.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            SourceObject so = terrain.transform.GetChild(i).GetComponent<SourceObject>();
            HealthManager hm = terrain.transform.GetChild(i).GetComponent<HealthManager>();

            if (so != null)
            {
                if (so.id == objectId)
                {
                    so.TakeDamage(damage, toolType, hitPos, attacker.gameObject);
                }
            }
            else if (terrain.transform.GetChild(i).GetComponent<BuildingMaterial>() != null)
            {
                if (terrain.transform.GetChild(i).GetComponent<BuildingMaterial>().id == objectId && hm != null)
                {
                    hm.TakeHit(damage, toolType, hitPos, attacker.gameObject);
                }
            }

        }


    }
    public void CallUpdateItemsRPC(string itemId)
    {
        pv.RPC("UpdateItems_RPC", RpcTarget.OthersBuffered, itemId);
    }

    [PunRPC]
    public void UpdateItems_RPC(string itemId)
    {
        Debug.Log("### we are updating the items as well");
        Item[] items = FindObjectsOfType<Item>();
        foreach (Item item in items)
        {
            if (item.id == itemId)
            {
                Destroy(item.gameObject);
            }
        }
        // Your code to add or remove object
    }
    public void CallUpdateFirePitRPC(string firePitId)
    {
        pv.RPC("UpdateFirePit_RPC", RpcTarget.AllBuffered, firePitId);
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