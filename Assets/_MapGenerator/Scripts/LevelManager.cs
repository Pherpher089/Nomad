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
    private static TextureData textureData;
    private static ItemManager itemManager;
    private static ObjectPool grassObjectPool;
    private static ObjectPool rockObjectPool;
    private static ObjectPool treeObjectPool;
    static ObjectPool spawnerObjectPool;
    static ObjectPool appleObjectPool;
    static ObjectPool stickObjectPool;
    static ObjectPool stoneObjectPool;
    private bool poolsAreReady = false;
    private int initFrameCounter = 0;
    public int seed;
    public static LevelManager Instance;
    public const string LevelDataKey = "levelData";
    public PhotonView pv;
    public bool initialized = false;
    public TerrainChunk currentTerrainChunk;

    void Awake()
    {
        Instance = this;
        pv = GetComponent<PhotonView>();
        DontDestroyOnLoad(gameObject);
    }

    public void InitializeLevelManager()
    {
        gameController = FindObjectOfType<GameStateManager>();
        itemManager = FindObjectOfType<ItemManager>();
        seed = FindObjectOfType<TerrainGenerator>().biomeDataArray[0].heightMapSettings.noiseSettings.seed;
        UnityEngine.Random.InitState(seed);
        // Assumes that the grass object is at index 6, rock object at index 1, etc.
        grassObjectPool = new ObjectPool(itemManager.environmentItemList[6].gameObject, 300);
        rockObjectPool = new ObjectPool(itemManager.environmentItemList[1].gameObject, 100);
        treeObjectPool = new ObjectPool(itemManager.environmentItemList[0].gameObject, 200);
        spawnerObjectPool = new ObjectPool(itemManager.environmentItemList[8].gameObject, 50);
        // appleObjectPool = new ObjectPool(itemManager.itemList[8], 50);
        // stickObjectPool = new ObjectPool(itemManager.itemList[2], 50);
        // stoneObjectPool = new ObjectPool(itemManager.itemList[3], 50);
        poolsAreReady = true;
        initialized = true;

    }
    public List<string> GetAllChunkSaveData()
    {
        List<string> data = new List<string>();
        foreach (KeyValuePair<Vector2, TerrainChunk> kvp in TerrainGenerator.Instance.terrainChunkDictionary)
        {
            data.Add(LoadChunkJson(kvp.Value));
        }
        return data;
    }
    public void PopulateObjects(TerrainChunk terrainChunk, Mesh terrainMesh)
    {
        StartCoroutine(PopulateObjectsCoroutine(terrainChunk, terrainMesh));
    }
    IEnumerator PopulateObjectsCoroutine(TerrainChunk terrainChunk, Mesh terrainMesh)
    {
        if (gameController != null)
        {
            textureData = terrainChunk.biomeData.textureData;
            itemManager = FindObjectOfType<ItemManager>();
        }

        TerrainChunkSaveData chunkSaveData = LevelManager.LoadChunk(terrainChunk);
        Transform parentTransform = terrainChunk.meshObject.transform;
        int c = 0;

        int objectDensity = 20;  // Higher values will place more objects
        float objectScale = 144f;
        float maxRandomOffset = objectScale / objectDensity * 0.5f;
        bool hasSpawner = false;
        for (int x = 0; x < objectDensity; x++)
        {
            for (int z = 0; z < objectDensity; z++)
            {
                // Use Perlin noise to get a value between 0 and 1
                float noiseValue = terrainChunk.heightMap.values[x, objectDensity - z];

                float fractionalPart = noiseValue % 1;
                int randValue = fractionalPart > .1 && fractionalPart < .2 || fractionalPart > .3 && fractionalPart < .4 || fractionalPart > .5 && fractionalPart < .6 || fractionalPart > .7 && fractionalPart < .9 ? 1 : -1;
                // Calculate position based on noise value
                Vector3 position = new Vector3(x * objectScale / objectDensity, 0, z * objectScale / objectDensity) + new Vector3(terrainChunk.sampleCentre.x - 72, 0, terrainChunk.sampleCentre.y - 72);
                System.Random random = new System.Random(seed);
                position += new Vector3(fractionalPart * 10, 0f, fractionalPart * 10 * randValue);

                GameObject newObj;
                ObjectPool objPl;
                if ((x == objectDensity / 2 || x + 1 == objectDensity / 2) && (z == objectDensity / 2 || z + 1 == objectDensity / 2) && !hasSpawner)
                {
                    hasSpawner = true;
                    if (PhotonNetwork.IsMasterClient)
                    {
                        newObj = spawnerObjectPool.GetObject();
                        objPl = spawnerObjectPool;
                    }
                    else
                    {
                        continue;
                    }
                }
                else if (noiseValue > 5 && randValue == 1)
                {
                    newObj = treeObjectPool.GetObject();
                    objPl = treeObjectPool;
                }
                else if (noiseValue > 4.4 && noiseValue < 4.5)
                {
                    newObj = rockObjectPool.GetObject();
                    objPl = rockObjectPool;

                }
                else if (noiseValue > 1)
                {
                    newObj = grassObjectPool.GetObject();
                    objPl = grassObjectPool;
                }
                else
                {
                    continue;
                }

                // If newObject does not have a sourceObject component, set the prefab index to match the actor spawner index. Apparently that is not saved anywhere on that object. 
                SourceObject sourceObj = newObj.GetComponent<SourceObject>();
                int prefabIndex = sourceObj ? sourceObj.itemIndex : 8;
                string _id = $"{(int)terrainChunk.coord.x}{(int)terrainChunk.coord.y}_{prefabIndex}_{(int)position.x}_{(int)position.z}_{(int)0}";
                if (chunkSaveData != null && chunkSaveData.removedObjects != null)
                {
                    foreach (string obj in chunkSaveData.removedObjects)
                    {
                        if (obj == _id)
                        {
                            objPl.ReturnObject(newObj);
                            continue;
                        }
                    }
                }

                newObj.transform.position = position;
                newObj.transform.SetParent(terrainChunk.meshObject.transform);
                if (sourceObj)
                {
                    sourceObj.id = _id;
                }
                else
                {
                    newObj.GetComponent<ActorSpawner>().id = _id;
                }

                int objectPerFrame = initFrameCounter > 1 ? 30 : 100000;
                c++;
                if (c % objectPerFrame == 0)
                {
                    yield return null;
                }
            }
        }

        if (chunkSaveData != null && chunkSaveData.objects != null && chunkSaveData.objects.Length > 0 && chunkSaveData.objects[0] != null)
        {
            foreach (TerrainObjectSaveData obj in chunkSaveData.objects)
            {
                GameObject _obj = obj.isItem ? itemManager.itemList[obj.itemIndex] : itemManager.environmentItemList[obj.itemIndex];
                GameObject newObj = Instantiate(_obj, new Vector3(obj.x, obj.y, obj.z), Quaternion.Euler(obj.rx, obj.ry, obj.rz));
                if (obj.isItem)
                {
                    newObj.GetComponent<SpawnMotionDriver>().hasSaved = true;
                    newObj.GetComponent<Item>().hasLanded = true;
                }

                newObj.GetComponent<Rigidbody>().isKinematic = true;
                newObj.transform.SetParent(terrainChunk.meshObject.transform);
                if (obj.isItem)
                {
                    newObj.GetComponent<Item>().id = obj.id;
                    newObj.GetComponent<Item>().parentChunk = terrainChunk;
                }
                else
                {
                    newObj.GetComponent<SourceObject>().id = obj.id;
                }
            }
        }
        //TODO: FIX
        //PopulateItems(terrainMesh, terrainChunk);
    }

    public void PopulateItems(Mesh terrainMesh, TerrainChunk terrainChunk)
    {
        StartCoroutine(PopulateItemsCoroutine(terrainMesh, terrainChunk));
    }

    IEnumerator PopulateItemsCoroutine(Mesh terrainMesh, TerrainChunk terrainChunk)
    {
        if (gameController != null)
        {
            textureData = terrainChunk.biomeData.textureData;
            itemManager = FindObjectOfType<ItemManager>();
        }

        int width = terrainChunk.heightMap.values.GetLength(0);
        Transform parentTransform = terrainChunk.meshObject.transform;

        GameObject newItem;
        for (int i = 0; i < terrainMesh.vertices.Length; i += 6)
        {
            float randomNumber = UnityEngine.Random.value;

            //apples
            if (randomNumber > 0.9996f)
            {
                Quaternion itemRotation = Quaternion.FromToRotation(Vector3.up, terrainMesh.normals[i]);
                newItem = appleObjectPool.GetObject(); //Assumed itemObjectPool similar to object pools for tree, rock etc
                newItem.transform.position = terrainMesh.vertices[i] + new Vector3(terrainChunk.sampleCentre.x, 0, terrainChunk.sampleCentre.y);
                newItem.transform.Rotate(new Vector3(0, UnityEngine.Random.Range(-180, 180), 0));
                newItem.transform.SetParent(parentTransform);
                newItem.GetComponent<Rigidbody>().isKinematic = true;
                continue;
            }
            //Stones
            if (randomNumber > 0.9994f)
            {
                Quaternion itemRotation = Quaternion.FromToRotation(Vector3.up, terrainMesh.normals[i]);
                newItem = stoneObjectPool.GetObject(); //Assumed itemObjectPool similar to object pools for tree, rock etc
                newItem.transform.position = terrainMesh.vertices[i] + new Vector3(terrainChunk.sampleCentre.x, 0, terrainChunk.sampleCentre.y);
                newItem.transform.Rotate(new Vector3(0, UnityEngine.Random.Range(-180, 180), 0));
                newItem.transform.SetParent(parentTransform);
                newItem.GetComponent<Rigidbody>().isKinematic = true;
                continue;

            }
            //Sticks
            if (randomNumber > 0.9993f)
            {

                Quaternion itemRotation = Quaternion.FromToRotation(Vector3.up, terrainMesh.normals[i]);
                newItem = stickObjectPool.GetObject(); //Assumed itemObjectPool similar to object pools for tree, rock etc
                newItem.transform.position = terrainMesh.vertices[i] + new Vector3(terrainChunk.sampleCentre.x, 0, terrainChunk.sampleCentre.y);
                newItem.transform.Rotate(new Vector3(0, UnityEngine.Random.Range(-180, 180), 0));
                newItem.transform.SetParent(parentTransform);
                newItem.GetComponent<Rigidbody>().isKinematic = true;
                continue;
            }

            int objectPerFrame = initFrameCounter > 1 ? 1 : 100000;

            if (i % objectPerFrame == 0)  // Choose the number that works best for you.
            {
                yield return null;
            }
        }

    }

    public static TerrainChunkSaveData LoadChunk(TerrainChunk terrainChunk)
    {
        string levelName = LevelPrep.Instance.worldName;
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{levelName}/");
        Directory.CreateDirectory(saveDirectoryPath);

        string filePath = saveDirectoryPath + terrainChunk.id + ".json";
        string json;
        try
        {
            json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<TerrainChunkSaveData>(json);

        }
        catch
        {
            Debug.Log("~ New Chunk. No data to load");
            return null;
        }
    }
    public static string LoadChunkJson(TerrainChunk terrainChunk)
    {
        string levelName = LevelPrep.Instance.worldName;
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{levelName}");
        Directory.CreateDirectory(saveDirectoryPath);
        string filePath = saveDirectoryPath + terrainChunk.id + ".json";

        string json;
        try
        {
            json = File.ReadAllText(filePath);
            return json;

        }
        catch
        {
            Debug.Log("~ New Chunk. No data to load");
            return null;
        }
    }

    public void UpdateSaveData(TerrainChunk terrainChunk, int itemIndex, string objectId, bool isDestroyed, Vector3 pos, Vector3 rot, bool isItem)
    {
        TerrainChunkSaveData data = LoadChunk(terrainChunk);
        List<TerrainObjectSaveData> currentData = data?.objects?.ToList() ?? new List<TerrainObjectSaveData>();

        // Saving objects that have been removed from the world 
        if (isDestroyed)
        {
            // Remove the object from the currentData
            currentData.RemoveAll(_obj => _obj.id == objectId);

            if (!isItem)
            {
                // Saving items being added to the world
                List<string> _removedObjects = data?.removedObjects?.ToList() ?? new List<string>();
                _removedObjects.Add(objectId);
                data.removedObjects = _removedObjects.ToArray();
            }
        }
        else
        {
            currentData.Add(new TerrainObjectSaveData(itemIndex, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, objectId, isItem));
        }

        string id = LevelPrep.Instance.worldName + terrainChunk.coord.x + '-' + terrainChunk.coord.y;
        terrainChunk.saveData = new TerrainChunkSaveData(terrainChunk.id, currentData.ToArray(), data?.removedObjects);
        LevelManager.SaveChunk(terrainChunk);
        LevelManager.Instance.UpdateLevelData();
    }

    public RoomOptions UpdateLevelData()
    {

        string levelName = FindObjectOfType<LevelPrep>().worldName;
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
    public static LevelSaveData LoadLeveL()
    {
        string levelName = LevelPrep.Instance.worldName;
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{levelName}/");
        string filePath = saveDirectoryPath + levelName + ".json";

        string json;
        try
        {
            json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<LevelSaveData>(json);

        }
        catch
        {
            Debug.Log("~ New Level. No data to load");
            return null;
        }
    }
    public static void SaveLevel()
    {
        if (!FindObjectOfType<GameStateManager>().initialized)
        {
            return;
        }
        string levelName = LevelPrep.Instance.worldName;
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{levelName}/");
        Directory.CreateDirectory(saveDirectoryPath);
        Vector3 playerPos = gameController.playersManager.playersCentralPosition;
        Debug.LogWarning("~ SavingLevel " + playerPos);
        LevelSaveData data = new LevelSaveData(playerPos.x, playerPos.y, playerPos.z, gameController.currentRespawnPoint.x, gameController.currentRespawnPoint.y, gameController.currentRespawnPoint.z);
        string json = JsonConvert.SerializeObject(data);
        string filePath = saveDirectoryPath + levelName + ".json";
        // Open the file for writing
        using (FileStream stream = new FileStream(filePath, FileMode.Create))
        using (StreamWriter writer = new StreamWriter(stream))
        {
            // Write the JSON string to the file
            writer.Write(json);
        }
    }
    public static void SaveLevel(Vector3 SpawnPoint)
    {
        if (!FindObjectOfType<GameStateManager>().initialized)
        {
            return;
        }
        string levelName = LevelPrep.Instance.worldName;
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{LevelPrep.Instance.worldName}/");
        Directory.CreateDirectory(saveDirectoryPath);
        Vector3 playerPos = gameController.playersManager.playersCentralPosition;
        Debug.LogWarning("~ SavingLevel " + playerPos);
        LevelSaveData data = new LevelSaveData(SpawnPoint.x, SpawnPoint.y, SpawnPoint.z, SpawnPoint.x, SpawnPoint.y, SpawnPoint.z);
        string json = JsonConvert.SerializeObject(data);
        string filePath = saveDirectoryPath + levelName + ".json";
        // Open the file for writing
        using (FileStream stream = new FileStream(filePath, FileMode.Create))
        using (StreamWriter writer = new StreamWriter(stream))
        {
            // Write the JSON string to the file
            writer.Write(json);
        }
    }
    public static void SaveChunk(TerrainChunk terrainChunk)
    {
        string levelName = LevelPrep.Instance.worldName;
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{levelName}/");
        Directory.CreateDirectory(saveDirectoryPath);
        string filePath = saveDirectoryPath + terrainChunk.id + ".json";
        TerrainChunkSaveData data = terrainChunk.saveData;
        string json = JsonConvert.SerializeObject(data);
        // Open the file for writing
        using (FileStream stream = new FileStream(filePath, FileMode.Create))
        using (StreamWriter writer = new StreamWriter(stream))
        {
            // Write the JSON string to the file
            writer.Write(json);
        }
        SaveLevel();
    }
    public void SaveProvidedLevelData(string levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("No level data to load " + PhotonNetwork.LocalPlayer.UserId);
            return;
        }
        string[] separateFileStrings = levelData.Split(new string[] { "|-|" }, StringSplitOptions.RemoveEmptyEntries);
        string levelName = LevelPrep.Instance.worldName;
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{levelName}/");
        Directory.Delete(saveDirectoryPath, true);
        Directory.CreateDirectory(saveDirectoryPath);
        for (int i = 0; i < separateFileStrings.Length; i++)
        {
            TerrainObjectSaveData level = JsonConvert.DeserializeObject<TerrainObjectSaveData>(separateFileStrings[i]);
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
    public void CallPlaceObjectPRC(int activeChildIndex, Vector3 position, Vector3 rotation, string id)
    {
        Debug.Log($"### {activeChildIndex}, {position}, {rotation}, {id}");
        pv.RPC("PlaceObjectPRC", RpcTarget.AllBuffered, activeChildIndex, position, rotation, id);
    }

    [PunRPC]
    void PlaceObjectPRC(int activeChildIndex, Vector3 _position, Vector3 _rotation, string id)
    {
        Debug.Log($"### {activeChildIndex}, {_position}, {_rotation}, {id}");
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
    }

    public void CallUpdateObjectsPRC(string objectId, int damage, ToolType toolType, Vector3 hitPos, PhotonView attacker)
    {
        pv.RPC("UpdateObject_PRC", RpcTarget.AllBuffered, objectId, damage, toolType, hitPos, attacker.ViewID);
    }

    [PunRPC]
    public void UpdateObject_PRC(string objectId, int damage, ToolType toolType, Vector3 hitPos, int attackerViewId)
    {
        PhotonView attacker = PhotonView.Find(attackerViewId);
        string[] idSubStrings = objectId.Split('_');
        foreach (TerrainChunk terrain in TerrainGenerator.Instance.visibleTerrainChunks)
        {

            if (terrain.id == idSubStrings[0])
            {
                int childCount = terrain.meshObject.transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    SourceObject so = terrain.meshObject.transform.GetChild(i).GetComponent<SourceObject>();
                    HealthManager hm = terrain.meshObject.transform.GetChild(i).GetComponent<HealthManager>();

                    if (so != null)
                    {
                        if (so.id == objectId)
                        {
                            so.TakeDamage(damage, toolType, hitPos, attacker.gameObject);
                        }
                    }
                    else if (terrain.meshObject.transform.GetChild(i).GetComponent<BuildingMaterial>() != null)
                    {
                        if (terrain.meshObject.transform.GetChild(i).GetComponent<BuildingMaterial>().id == objectId)
                        {
                            hm.TakeHit(damage, toolType, hitPos, attacker.gameObject);
                        }
                    }

                }

            }
        }
        // Your code to add or remove object
    }
    public void CallUpdateItemsRPC(string itemId)
    {
        pv.RPC("UpdateItems_RPC", RpcTarget.OthersBuffered, itemId);
    }

    [PunRPC]
    public void UpdateItems_RPC(string itemId)
    {
        Item[] items = FindObjectsOfType<Item>();
        foreach (Item item in items)
        {
            if (item.id == itemId)
            {
                bool isSaved = item.SaveItem(item.parentChunk, true);
                if (isSaved) Destroy(item.gameObject);
            }
        }
        // Your code to add or remove object
    }
}

public class LevelSaveData
{
    public float playerPosX;
    public float playerPosY;
    public float playerPosZ;
    public float respawnPosX;
    public float respawnPosY;
    public float respawnPosZ;
    public LevelSaveData(float playerPosX, float playerPosY, float playerPosZ, float respawnPosX, float respawnPosY, float respawnPosZ)
    {
        this.playerPosX = playerPosX;
        this.playerPosY = playerPosY;
        this.playerPosZ = playerPosZ;
        this.respawnPosX = respawnPosX;
        this.respawnPosY = respawnPosY;
        this.respawnPosZ = respawnPosZ;
    }
}
public class TerrainChunkSaveData
{
    public string id;
    public TerrainObjectSaveData[] objects;
    public string[] removedObjects;
    public TerrainChunkSaveData(string id, TerrainObjectSaveData[] objects, string[] removedObjects)
    {
        this.id = id;
        this.objects = objects;
        this.removedObjects = removedObjects;
    }
}

public class TerrainObjectSaveData
{
    public int itemIndex;
    public float x;
    public float y;
    public float z;
    public float rx;
    public float ry;
    public float rz;
    public string id;
    public bool isItem;
    public TerrainObjectSaveData(int itemIndex, float x, float y, float z, float rx, float ry, float rz, string id, bool isItem)
    {
        this.itemIndex = itemIndex;
        this.x = x;
        this.y = y;
        this.z = z;
        this.rx = rx;
        this.ry = ry;
        this.rz = rz;
        this.id = id;
        this.isItem = isItem;
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