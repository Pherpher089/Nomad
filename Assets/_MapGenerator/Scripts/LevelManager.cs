using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    private static GameStateManager gameController;
    private static TextureData textureData;
    private static ItemManager itemManager;
    public static float treeLine;
    private static ObjectPool grassObjectPool;
    private static ObjectPool rockObjectPool;
    private static ObjectPool treeObjectPool;
    static ObjectPool spawnerObjectPool;
    static ObjectPool appleObjectPool;
    static ObjectPool stickObjectPool;
    static ObjectPool stoneObjectPool;
    private bool poolsAreReady = false;
    private int initFramCounter = 0;
    private int seed;

    void Awake()
    {
        gameController = FindObjectOfType<GameStateManager>();
        itemManager = FindObjectOfType<ItemManager>();
        seed = FindObjectOfType<TerrainGenerator>().biomeDataArray[0].heightMapSettings.noiseSettings.seed;
        UnityEngine.Random.InitState(seed);
        // Assumes that the grass object is at index 6, rock object at index 1, etc.
        grassObjectPool = new ObjectPool(itemManager.environmentItemList[6].gameObject, 3000);
        rockObjectPool = new ObjectPool(itemManager.environmentItemList[1].gameObject, 100);
        treeObjectPool = new ObjectPool(itemManager.environmentItemList[0].gameObject, 200);
        spawnerObjectPool = new ObjectPool(itemManager.environmentItemList[8].gameObject, 50);

        appleObjectPool = new ObjectPool(itemManager.itemList[8], 50);
        stickObjectPool = new ObjectPool(itemManager.itemList[2], 50);
        stoneObjectPool = new ObjectPool(itemManager.itemList[3], 50);

        poolsAreReady = true;
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
            treeLine = terrainChunk.biomeData.heightMapSettings.maxHeight * textureData.layers[1].startHeight;
            itemManager = FindObjectOfType<ItemManager>();
        }

        int width = terrainChunk.heightMap.values.GetLength(0);
        TerrainChunkSaveData chunkSaveData = LevelManager.LoadChunk(terrainChunk);
        Transform parentTransform = terrainChunk.meshObject.transform;
        int c = 0;
        if (chunkSaveData != null && !gameController.newWorld)
        {
            GameObject newObj = new GameObject();
            foreach (TerrainObjectSaveData item in chunkSaveData.objects)
            {
                if (item.isItem) continue;
                if (item.itemIndex >= 0 && item.itemIndex < itemManager.environmentItemList.Length)
                {
                    switch (item.itemIndex)
                    {
                        case 0:
                            newObj = treeObjectPool.GetObject();
                            break;
                        case 1:
                            newObj = rockObjectPool.GetObject();
                            break;
                        case 6:
                            newObj = grassObjectPool.GetObject();
                            break;
                        case 8:
                            newObj = spawnerObjectPool.GetObject();
                            break;
                        default:
                            newObj = Instantiate(itemManager.environmentItemList[item.itemIndex]);
                            break;
                    }
                    newObj.transform.SetPositionAndRotation(new Vector3(item.x, item.y, item.z), Quaternion.Euler(new Vector3(item.rx, item.ry, item.rz)));
                    newObj.transform.SetParent(parentTransform);
                    int objectPerFrame = initFramCounter > 5 ? 3 : 100000;
                    c++;
                    if (c % objectPerFrame == 0)  // Choose the number that works best for you.
                    {
                        yield return null;
                    }
                }
            }
        }
        else
        {
            int numVertsPerLine = terrainChunk.meshSettings.numVertsPerLine;

            GameObject newObj = new GameObject();
            for (int i = 0; i < terrainMesh.vertices.Length; i += 6)
            {
                //Spawner
                if ((terrainMesh.vertices[i].x == numVertsPerLine / 4 || terrainMesh.vertices[i].x == (numVertsPerLine / 4) * 3) && (terrainMesh.vertices[i].z == (numVertsPerLine / 4) || terrainMesh.vertices[i].z == (numVertsPerLine / 4) * 3))
                {
                    newObj = spawnerObjectPool.GetObject();
                    newObj.transform.position = terrainMesh.vertices[i] + new Vector3(terrainChunk.sampleCentre.x, 0, terrainChunk.sampleCentre.y) * terrainChunk.meshSettings.meshScale;
                    newObj.transform.SetParent(parentTransform);
                    continue;
                }

                float randomNumber = UnityEngine.Random.value;

                //Grass
                if (randomNumber > 0.95f && terrainMesh.vertices[i].y > treeLine)
                {
                    Quaternion grassRotation = Quaternion.FromToRotation(Vector3.up, terrainMesh.normals[i]);
                    newObj = grassObjectPool.GetObject();
                    newObj.transform.position = terrainMesh.vertices[i] + new Vector3(terrainChunk.sampleCentre.x, 0, terrainChunk.sampleCentre.y) * terrainChunk.meshSettings.meshScale;
                    newObj.transform.Rotate(new Vector3(0, UnityEngine.Random.Range(-180, 180), 0));
                    newObj.transform.SetParent(parentTransform);
                }

                //Rocks
                if (randomNumber > 0.999f)
                {
                    newObj = rockObjectPool.GetObject();
                    newObj.transform.position = terrainMesh.vertices[i] + new Vector3(terrainChunk.sampleCentre.x, 0, terrainChunk.sampleCentre.y) * terrainChunk.meshSettings.meshScale;
                    newObj.transform.SetParent(parentTransform);
                    continue;
                }

                //Trees
                if (randomNumber > 0.997f && terrainMesh.vertices[i].y > treeLine)
                {
                    newObj = treeObjectPool.GetObject();
                    newObj.transform.position = terrainMesh.vertices[i] + new Vector3(terrainChunk.sampleCentre.x, 0, terrainChunk.sampleCentre.y) * terrainChunk.meshSettings.meshScale;
                    newObj.transform.SetParent(parentTransform);
                    continue;
                }
                int objectPerFrame = initFramCounter > 1 ? 1 : 100000;

                if (i % objectPerFrame == 0)  // Choose the number that works best for you.
                {
                    yield return null;
                }
            }
        }

        PopulateItems(terrainMesh, terrainChunk);

        terrainChunk.SaveTerrainAfterPopulation(chunkSaveData != null && !gameController.newWorld ? chunkSaveData : new TerrainChunkSaveData(new TerrainObjectSaveData[0]));
    }
    public static void SpawnPlayers(string[] players)
    {
        if (gameController == null)
        {
            gameController = FindObjectOfType<GameStateManager>();
        }
        if (players.Length == 0)
        {
            throw new Exception("No players to load");
        }
        LevelSaveData saveData = LoadLeveL();

        Vector3 spawnPoint;
        if (saveData != null)
        {
            spawnPoint = new Vector3(saveData.playerPosX, saveData.playerPosY + 30, saveData.playerPosZ);
            gameController.currentRespawnPoint = new Vector3(saveData.respawnPosX, saveData.respawnPosY, saveData.respawnPosZ);
        }
        else
        {
            gameController.currentRespawnPoint = new Vector3(0, 100, 0);
            spawnPoint = new Vector3(0, 100, 0);
        }
        for (int i = 0; i < players.Length; i++)
        {
            GameObject player = Instantiate(Resources.Load("Prefabs/Donte") as GameObject, spawnPoint + new Vector3(i, 0, i), Quaternion.identity);
            CharacterStats stats = player.GetComponent<CharacterStats>();
            ThirdPersonUserControl user = player.GetComponent<ThirdPersonUserControl>();
            user.playerName = players[i];
            player.name = players[i];
            player.GetComponentInChildren<SkinnedMeshRenderer>().material = gameController.playerMats[i];
            stats.Initialize(players[i]);

            if (!gameController.firstPlayerKeyboardAndMouse)
            {
                switch (i)
                {
                    case 0:
                        user.playerNum = PlayerNumber.Player_1;
                        break;
                    case 1:
                        user.playerNum = PlayerNumber.Player_2;
                        break;
                    case 2:
                        user.playerNum = PlayerNumber.Player_3;
                        break;
                    case 3:
                        user.playerNum = PlayerNumber.Player_4;
                        break;
                }
            }
            else
            {
                switch (i)
                {
                    case 0:
                        user.playerNum = PlayerNumber.Single_Player;
                        break;
                    case 1:
                        user.playerNum = PlayerNumber.Player_1;
                        break;
                    case 2:
                        user.playerNum = PlayerNumber.Player_2;
                        break;
                    case 3:
                        user.playerNum = PlayerNumber.Player_3;
                        break;
                }
            }
        }
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
        int numVertsPerLine = terrainChunk.meshSettings.numVertsPerLine;

        GameObject newItem;
        for (int i = 0; i < terrainMesh.vertices.Length; i += 6)
        {
            float randomNumber = UnityEngine.Random.value;

            //apples
            if (randomNumber > 0.9996f)
            {
                Quaternion itemRotation = Quaternion.FromToRotation(Vector3.up, terrainMesh.normals[i]);
                newItem = appleObjectPool.GetObject(); //Assumed itemObjectPool similar to object pools for tree, rock etc
                newItem.transform.position = terrainMesh.vertices[i] + new Vector3(terrainChunk.sampleCentre.x, 0, terrainChunk.sampleCentre.y) * terrainChunk.meshSettings.meshScale;
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
                newItem.transform.position = terrainMesh.vertices[i] + new Vector3(terrainChunk.sampleCentre.x, 0, terrainChunk.sampleCentre.y) * terrainChunk.meshSettings.meshScale;
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
                newItem.transform.position = terrainMesh.vertices[i] + new Vector3(terrainChunk.sampleCentre.x, 0, terrainChunk.sampleCentre.y) * terrainChunk.meshSettings.meshScale;
                newItem.transform.Rotate(new Vector3(0, UnityEngine.Random.Range(-180, 180), 0));
                newItem.transform.SetParent(parentTransform);
                newItem.GetComponent<Rigidbody>().isKinematic = true;
                continue;
            }

            int objectPerFrame = initFramCounter > 1 ? 1 : 100000;

            if (i % objectPerFrame == 0)  // Choose the number that works best for you.
            {
                yield return null;
            }
        }

    }


    public static TerrainChunkSaveData LoadChunk(TerrainChunk terrainChunk)
    {
        string levelName = FindObjectOfType<GameStateManager>().m_WorldName;
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{levelName}/");
        Directory.CreateDirectory(saveDirectoryPath);
        string filePath = saveDirectoryPath + levelName + terrainChunk.coord.x + '-' + terrainChunk.coord.y + ".json";

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


    public static TerrainChunkSaveData GetSaveData(GameObject terrainObj)
    {
        ItemManager itemManager = FindObjectOfType<ItemManager>();
        TerrainObjectSaveData[] objs = new TerrainObjectSaveData[terrainObj.transform.childCount];
        for (int i = 0; i < terrainObj.transform.childCount; i++)
        {
            Transform objTrans = terrainObj.transform.GetChild(i);
            int itemIndex = itemManager.GetEnvItemIndex(objTrans.gameObject);
            objs[i] = new TerrainObjectSaveData(itemIndex, objTrans.position.x, objTrans.position.y, objTrans.position.z, objTrans.rotation.eulerAngles.x, objTrans.rotation.eulerAngles.y, objTrans.rotation.eulerAngles.z, false);
        }
        return new TerrainChunkSaveData(objs);
    }
    public static LevelSaveData LoadLeveL()
    {
        string levelName = FindObjectOfType<GameStateManager>().m_WorldName;
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
        string levelName = FindObjectOfType<GameStateManager>().m_WorldName;
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{gameController.m_WorldName}/");
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
        string levelName = FindObjectOfType<GameStateManager>().m_WorldName;
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{gameController.m_WorldName}/");
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
        string levelName = FindObjectOfType<GameStateManager>().m_WorldName;
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, $"Levels/{gameController.m_WorldName}/");
        Directory.CreateDirectory(saveDirectoryPath);
        string filePath = saveDirectoryPath + levelName + terrainChunk.coord.x + '-' + terrainChunk.coord.y + ".json";
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
    public TerrainObjectSaveData[] objects;
    public TerrainChunkSaveData(TerrainObjectSaveData[] objects)
    {
        this.objects = objects;
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
    public bool isItem;
    public TerrainObjectSaveData(int itemIndex, float x, float y, float z, float rx, float ry, float rz, bool isItem)
    {
        this.itemIndex = itemIndex;
        this.x = x;
        this.y = y;
        this.z = z;
        this.rx = rx;
        this.ry = ry;
        this.rz = rz;
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