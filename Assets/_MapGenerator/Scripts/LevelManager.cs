using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System;

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
    private bool poolsAreReady = false;
    void Awake()
    {
        itemManager = FindObjectOfType<ItemManager>();

        // Assumes that the grass object is at index 6, rock object at index 1, etc.
        grassObjectPool = new ObjectPool(itemManager.environmentItemList[6], 2000); // Modify the initial pool size to suit your game.
        rockObjectPool = new ObjectPool(itemManager.environmentItemList[1], 100); // Modify the initial pool size to suit your game.
        treeObjectPool = new ObjectPool(itemManager.environmentItemList[0], 200); // Modify the initial pool size to suit your game.
        spawnerObjectPool = new ObjectPool(itemManager.environmentItemList[8], 50); // Modify the initial pool size to suit your game.
        poolsAreReady = true;
    }
    public static TerrainChunkSaveData PopulateObjects(TerrainChunk terrainChunk, Mesh terrainMesh)
    {
        if (gameController == null)
        {
            gameController = GameObject.FindObjectOfType<GameStateManager>();
            textureData = terrainChunk.biomeData.textureData;
            treeLine = terrainChunk.biomeData.heightMapSettings.maxHeight * textureData.layers[1].startHeight;
            itemManager = FindObjectOfType<ItemManager>();
        }

        int width = terrainChunk.heightMap.values.GetLength(0);
        TerrainChunkSaveData chunkSaveData = LevelManager.LoadChunk(terrainChunk);
        Transform parentTransform = terrainChunk.meshObject.transform;

        if (chunkSaveData != null && !gameController.newWorld)
        {
            GameObject newObj = new GameObject();
            foreach (TerrainObjectSaveData item in chunkSaveData.objects)
            {
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
                    }
                    newObj.transform.Rotate(new Vector3(item.rx, item.ry, item.rz));
                    newObj.transform.SetParent(parentTransform);
                }
            }
        }
        else
        {
            int numVertsPerLine = terrainChunk.meshSettings.numVertsPerLine;
            for (int i = 0; i < terrainMesh.vertices.Length; i += 6)
            {
                GameObject newObj = new GameObject();
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
                if (randomNumber > 0.995f && terrainMesh.vertices[i].y > treeLine)
                {
                    newObj = treeObjectPool.GetObject();
                    newObj.transform.position = terrainMesh.vertices[i] + new Vector3(terrainChunk.sampleCentre.x, 0, terrainChunk.sampleCentre.y) * terrainChunk.meshSettings.meshScale;
                    newObj.transform.SetParent(parentTransform);
                    continue;
                }
            }
        }

        //PopulateItems(terrainMesh, terrainChunk);

        return chunkSaveData != null && !gameController.newWorld ? chunkSaveData : new TerrainChunkSaveData(new TerrainObjectSaveData[0]);
    }

    public static void SpawnPlayers(string[] players)
    {
        if (players.Length == 0)
        {
            throw new Exception("No players to load");
        }
        LevelSaveData saveData = LoadLeveL();

        Vector3 spawnPoint;
        if (saveData != null)
        {
            spawnPoint = new Vector3(saveData.playerPosX, saveData.playerPosY + 10, saveData.playerPosZ);
        }
        else
        {
            spawnPoint = new Vector3(0, 100, 0);
        }
        GameObject player;
        for (int i = 0; i < players.Length; i++)
        {
            player = Instantiate(Resources.Load("Prefabs/Donte") as GameObject, spawnPoint + new Vector3(i, 0, i), Quaternion.identity);
            CharacterStats stats = player.GetComponent<CharacterStats>();
            ThirdPersonUserControl user = player.GetComponent<ThirdPersonUserControl>();
            user.playerName = players[i];
            player.name = players[i];
            stats.Initialize(players[i]);

            if (players.Length > 1 && !gameController.firstPlayerKeyboardAndMouse)
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
                    case 4:
                        user.playerNum = PlayerNumber.Single_Player;
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
                    case 4:
                        user.playerNum = PlayerNumber.Player_4;
                        break;
                }
            }

        }
    }

    static void PopulateItems(Mesh terrainMesh, TerrainChunk terrainChunk)
    {
        GameObject newObj;
        TerrainObjectSaveData currentObj = new TerrainObjectSaveData(0, 0, 0, 0, 0, 0, 0);
        for (int i = 0; i < terrainMesh.vertices.Length; i += 6)
        {
            float randomNumber = UnityEngine.Random.Range(0f, 1f);
            //sticks
            if (randomNumber > 0.999f && terrainMesh.vertices[i].y > treeLine)
            {
                int itemIndex = 2;//stones
                newObj = Instantiate(itemManager.itemList[itemIndex], terrainMesh.vertices[i] + new Vector3(terrainChunk.sampleCentre.x, 0, terrainChunk.sampleCentre.y) + Vector3.up * terrainChunk.meshSettings.meshScale, Quaternion.identity);
                newObj.transform.parent = terrainChunk.meshObject.transform;
                currentObj.itemIndex = itemIndex;
                currentObj.x = newObj.transform.position.x;
                currentObj.y = newObj.transform.position.y;
                currentObj.z = newObj.transform.position.z;
                currentObj.rx = newObj.transform.rotation.eulerAngles.x;
                currentObj.ry = newObj.transform.rotation.eulerAngles.y;
                currentObj.rz = newObj.transform.rotation.eulerAngles.z;
                continue;
            }
        }
        for (int i = 0; i < terrainMesh.vertices.Length; i += 6)
        {
            float randomNumber = UnityEngine.Random.Range(0f, 1f);
            //sticks
            if (randomNumber > 0.999f)
            {
                int itemIndex = 3; //Sticks
                newObj = Instantiate(itemManager.itemList[itemIndex], terrainMesh.vertices[i] + new Vector3(terrainChunk.sampleCentre.x, 0, terrainChunk.sampleCentre.y) * terrainChunk.meshSettings.meshScale, Quaternion.identity);
                newObj.transform.parent = terrainChunk.meshObject.transform;
                currentObj.itemIndex = itemIndex;
                currentObj.x = newObj.transform.position.x;
                currentObj.y = newObj.transform.position.y;
                currentObj.z = newObj.transform.position.z;
                currentObj.rx = newObj.transform.rotation.eulerAngles.x;
                currentObj.ry = newObj.transform.rotation.eulerAngles.y;
                currentObj.rz = newObj.transform.rotation.eulerAngles.z;
                continue;
            }
        }
        for (int i = 0; i < terrainMesh.vertices.Length; i += 6)
        {
            float randomNumber = UnityEngine.Random.Range(0f, 1f);
            //sticks
            if (randomNumber > 0.999f)
            {
                int itemIndex = 10; //Apples
                newObj = Instantiate(itemManager.itemList[itemIndex], terrainMesh.vertices[i] + new Vector3(terrainChunk.sampleCentre.x, 0, terrainChunk.sampleCentre.y) * terrainChunk.meshSettings.meshScale, Quaternion.identity);
                newObj.transform.parent = terrainChunk.meshObject.transform;
                currentObj.itemIndex = itemIndex;
                currentObj.x = newObj.transform.position.x;
                currentObj.y = newObj.transform.position.y;
                currentObj.z = newObj.transform.position.z;
                currentObj.rx = newObj.transform.rotation.eulerAngles.x;
                currentObj.ry = newObj.transform.rotation.eulerAngles.y;
                currentObj.rz = newObj.transform.rotation.eulerAngles.z;
                continue;
            }
        }

    }

    public static TerrainChunkSaveData LoadChunk(TerrainChunk terrainChunk)
    {
        string levelName = FindObjectOfType<GameStateManager>().m_WorldName;
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, "Levels");
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
            objs[i] = new TerrainObjectSaveData(itemIndex, objTrans.position.x, objTrans.position.y, objTrans.position.z, objTrans.rotation.eulerAngles.x, objTrans.rotation.eulerAngles.y, objTrans.rotation.eulerAngles.z);
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
        Debug.Log("~ SavingLevel " + playerPos);
        LevelSaveData data = new LevelSaveData(playerPos.x, playerPos.y, playerPos.z);
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
    public LevelSaveData(float playerPosX, float playerPosY, float playerPosZ)
    {
        this.playerPosX = playerPosX;
        this.playerPosY = playerPosY;
        this.playerPosZ = playerPosZ;
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
    public TerrainObjectSaveData(int itemIndex, float x, float y, float z, float rx, float ry, float rz)
    {
        this.itemIndex = itemIndex;
        this.x = x;
        this.y = y;
        this.z = z;
        this.rx = rx;
        this.ry = ry;
        this.rz = rz;
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
            Debug.Log("### jere: " + prefab.name);
            GameObject obj = GameObject.Instantiate(prefab);
            obj.SetActive(false);
            return obj;
        }
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        pool.Push(obj);
    }
}