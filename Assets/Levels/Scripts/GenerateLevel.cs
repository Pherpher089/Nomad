using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using Newtonsoft.Json;

public class GenerateLevel : MonoBehaviour
{
    // Array of prefabs to use for object placement
    public string levelName = "New World";
    public string[] playerNames;
    public string saveDirectory = "C:/Users/Chris/wkspaces/CapsuleCrashers/Capsule Crashers/GameSaveData/";
    public GameObject[] prefabs;
    public GameObject[] itemPrefabs;

    // Grid size (in units)
    public float gridSize = 1.0f;

    // Size of the terrain (in grid units)
    public int terrainSizeX = 10;
    public int terrainSizeZ = 10;
    int terrainHeight = 0;

    // Density map for object placement
    public float[,] densityMap;
    public float[,] secondaryDensityMap;

    // Dictionary to store placed objects and their positions
    public Dictionary<Transform, int> placedObjects; // This is where I am leaving off

    // This is the seed that the level will be generated on
    public int seed = 22490;
    public int numberOfPlayers = 1;
    public GameObject[] players = new GameObject[5];
    public GameObject[] activePlayers;
    public bool loadLevel = false;

    private GameStateManager gameManager;
    private PlayersManager playersManager;
    void Awake()
    {
        //Load the player objects from recourses
        for (int i = 0; i < 5; i++)
        {
            players[i] = Resources.Load<GameObject>("Prefabs/Player_" + i.ToString());
        }
        gameManager = GameObject.FindWithTag("GameController").GetComponent<GameStateManager>();
        // Initialize the density map and placed objects dictionary
        densityMap = new float[terrainSizeX, terrainSizeZ];
        secondaryDensityMap = new float[terrainSizeX, terrainSizeZ];

        placedObjects = new Dictionary<Transform, int>();
        if (loadLevel)
        {
            LoadLevel(saveDirectory + levelName + ".json");
        }
        else
        {
            // Generate the terrain
            GenerateTerrain();
            // Place objects on the terrain
            PlaceObjects();
            // save the new level
            SaveLevel(saveDirectory + levelName + ".json");
        }
        //Add players to level
        AddPlayers();
    }
    public void Start()
    {
        gameManager.InitializeGameState();
    }

    void AddPlayers()
    {
        Vector3 playerPos = new Vector3((terrainSizeX * gridSize) / 2, 1, (terrainSizeZ * gridSize) / 2);
        activePlayers = new GameObject[playerNames.Length];
        for (int i = 0; i < playerNames.Length; i++)
        {
            GameObject player = Instantiate(players[i], playerPos, Quaternion.identity);
            PlayerSaveData loadedPlayer = LoadPlayer(playerNames[i]);
            Debug.Log(loadedPlayer);
            ThirdPersonUserControl userControl = player.GetComponent<ThirdPersonUserControl>();
            if (loadedPlayer != null)
            {
                player.transform.position = new Vector3(loadedPlayer.x, loadedPlayer.y, loadedPlayer.z);
                userControl.playerName = playerNames[i];
            }
            player.name = playerNames[i];
            activePlayers[i] = player;

            SavePlayer(userControl);
        }
        //TODO recalculate the position of the camera based on loaded players and new player position. 
        GameObject.FindWithTag("MainCamera").transform.parent.transform.position = playerPos;
    }

    public void SavePlayers()
    {
        foreach (GameObject player in activePlayers)
        {
            SavePlayer(player.GetComponent<ThirdPersonUserControl>());
        }
        Debug.Log("Saved All Players");
    }
    public PlayerSaveData LoadPlayer(string playerName)
    {
        PlayerSaveData[] playerDataArray;
        try
        {
            string jsonData = File.ReadAllText(saveDirectory + levelName + "Players.json");
            playerDataArray = JsonConvert.DeserializeObject<PlayerSaveData[]>(jsonData);
            foreach (PlayerSaveData savedPlayer in playerDataArray)
            {
                if (savedPlayer.playerName == playerName)
                {
                    Debug.Log("Found save data for " + playerName);
                    return savedPlayer;
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
    public void SavePlayer(ThirdPersonUserControl player)
    {
        PlayerSaveData[] playerDataArray;
        //SAVE PLAYERS HERE
        try
        {
            string jsonData = File.ReadAllText(saveDirectory + levelName + "Players.json");
            playerDataArray = JsonConvert.DeserializeObject<PlayerSaveData[]>(jsonData);
            Debug.Log("Found existing players when saving");
            bool foundPlayer = false;
            for (int i = 0; i < playerDataArray.Length; i++)
            {
                PlayerSaveData existingPlayer = playerDataArray[i];
                if (existingPlayer.playerName == player.playerName)
                {
                    Debug.Log("Saving over existing player");
                    playerDataArray[i] = new PlayerSaveData(player.playerName, player.transform.position.x, player.transform.position.y, player.transform.position.z);
                    foundPlayer = true;
                }
            }
            if (!foundPlayer)
            {
                Debug.Log("Saving new player to existing file");
                playerDataArray[playerDataArray.Length] = new PlayerSaveData(player.playerName, player.transform.position.x, player.transform.position.y, player.transform.position.z);
            }
        }
        catch (Exception ex)
        {
            Debug.Log("No players saved");
            playerDataArray = new PlayerSaveData[1];
            playerDataArray[0] = new PlayerSaveData(player.playerName, player.transform.position.x, player.transform.position.y, player.transform.position.z);

        }

        string newJsonData = JsonConvert.SerializeObject(playerDataArray);
        Debug.Log("Saved Player " + player.playerName);
        File.WriteAllText(saveDirectory + levelName + "Players.json", newJsonData);
    }
    void GenerateTerrain()
    {
        // Create a new TerrainData object with the specified size
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = terrainSizeX + 1;
        terrainData.size = new Vector3(terrainSizeX * gridSize, terrainHeight, terrainSizeZ * gridSize);

        // Create a new heightmap for the terrain
        float[,] heights = new float[terrainSizeX, terrainSizeZ];

        // Set the heightmap for the terrain
        terrainData.SetHeights(0, 0, heights);

        // Create a new Terrain game object and set its TerrainData
        GameObject terrain = Terrain.CreateTerrainGameObject(terrainData);
        terrain.layer = 15;

        // Set the position of the terrain to the center of the grid
        //terrain.transform.position = new Vector3((terrainSizeX * gridSize) / 2.0f, 0, (terrainSizeZ * gridSize) / 2.0f);
    }

    float[,] GenerateDensityMap()
    {
        // Seed the random number generator with the specified seed value
        UnityEngine.Random.InitState(seed);
        //create a map to generate
        float[,] map = new float[terrainSizeX, terrainSizeZ];
        // Generate the density map
        for (int x = 0; x < terrainSizeX; x++)
        {
            for (int z = 0; z < terrainSizeZ; z++)
            {
                // Generate a random density value between 0 and 1
                map[x, z] = UnityEngine.Random.value;
            }
        }
        return map;
    }

    float[,] GenerateDensityMapPerlin(float densityRange = 1, float densityMin = 0)
    {
        // Seed the random number generator with the specified seed value
        UnityEngine.Random.InitState(seed);
        //create a map to generate
        float[,] map = new float[terrainSizeX, terrainSizeZ];
        // Generate the density map
        for (int x = 0; x < terrainSizeX; x++)
        {
            for (int z = 0; z < terrainSizeZ; z++)
            {
                // Generate a Perlin noise value at this position
                float noise = Mathf.PerlinNoise((x + seed) * 0.1f, (z + seed) * 0.1f);

                // Scale the noise value to the desired density range
                map[x, z] = noise * densityRange + densityMin;
            }
        }
        return map;
    }


    void PlaceObjects()
    {
        int distance = 1;
        float threshold = 0;
        float secondaryThreshold = 0;
        int index = -1;
        foreach (GameObject prefab in prefabs)
        {
            switch (prefab.name)
            {
                case "Tree":
                    densityMap = GenerateDensityMapPerlin(1, 0);
                    secondaryDensityMap = GenerateDensityMap();
                    threshold = .4f;
                    secondaryThreshold = 0.8f;
                    index = 0;
                    break;
                case "Bolder":
                    densityMap = GenerateDensityMap();
                    distance = 2;
                    threshold = .95f;
                    secondaryThreshold = 0f;
                    index = 1;
                    break;
            }

            for (int x = 0; x < terrainSizeX; x++)
            {
                for (int z = 0; z < terrainSizeZ; z++)
                {
                    // Calculate the densities at this position
                    float density = densityMap[x, z];
                    float secondaryDensity = secondaryDensityMap[x, z];
                    // If the densities are above their appropriate threshold, place an object at this position
                    if (density > threshold && secondaryDensity > secondaryThreshold)
                    {

                        // Check if there is an object within the specified distance
                        Vector3 position = new Vector3(x * gridSize + gridSize / 2.0f, 0, z * gridSize + gridSize / 2.0f);
                        Collider[] colliders = Physics.OverlapSphere(position, distance, 15);
                        if (colliders.Length > 0)
                        {
                            continue;
                        }
                        // Instantiate the object and snap its position to the grid
                        GameObject obj = Instantiate(prefab);
                        obj.transform.position = position;
                        // Generate a random angle between 0 and 360 degrees
                        float angle = UnityEngine.Random.Range(0.0f, 360.0f);

                        // Convert the angle to radians
                        float radians = angle * Mathf.Deg2Rad;

                        // Create a quaternion representing the rotation around the y-axis
                        Quaternion rotation = Quaternion.Euler(0, angle, 0);

                        // Set the rotation of the object
                        obj.transform.rotation = rotation;

                        // Add the object and its position to the dictionary
                        placedObjects.Add(obj.transform, index);
                    }
                }
            }
        }
    }

    public void UpdateObjects(GameObject levelObject, bool destroy = false)
    {
        Debug.Log("Updating objects");
        if (placedObjects.ContainsKey(levelObject.transform))
        {
            if (destroy)
            {
                placedObjects.Remove(levelObject.transform);
            }
        }
        else
        {
            for (int i = 0; i < prefabs.Length; i++)
            {
                Debug.Log("Looping prefabs");
                // removing the clone suffix from the instantiated object name
                string objectName = levelObject.name.Replace("(Clone)", "");

                if (objectName == prefabs[i].gameObject.name)
                {
                    Debug.Log("Adding object to placedObjects");
                    placedObjects[levelObject.transform] = i;
                }
            }
        }
        SavePlayers();
        SaveLevel(saveDirectory + levelName + ".json");
        Debug.Log("Objects Updated and Level Saved");

    }


    public void SaveLevel(string filePath)
    {
        // Create a data object to store the level data
        LevelData data = new LevelData();
        int count = 0;
        //map over all of the placed objects and add them to the placedObjects array for saving
        data.placedObjects = new PlacedObject[placedObjects.Count];
        foreach (KeyValuePair<Transform, int> kvp in placedObjects)
        {
            data.placedObjects[count] = new PlacedObject(kvp.Value, kvp.Key.position.x, kvp.Key.position.y, kvp.Key.position.z, kvp.Key.eulerAngles.y);
            count++;
        }
        data.seed = seed;
        data.gridSize = gridSize;
        data.terrainSizeX = terrainSizeX;
        data.terrainSizeZ = terrainSizeZ;

        // Serialize the data object to a JSON string
        string json = JsonConvert.SerializeObject(data);
        // Open the file for writing
        using (FileStream stream = new FileStream(filePath, FileMode.Create))
        using (StreamWriter writer = new StreamWriter(stream))
        {
            // Write the JSON string to the file
            writer.Write(json);
        }
    }

    public void LoadLevel(string filePath)
    {
        // Read the JSON string from the file
        string json = File.ReadAllText(filePath);

        // Deserialize the data object from the JSON string
        LevelData data = JsonConvert.DeserializeObject<LevelData>(json);

        // Set the seed, grid size, and terrain size
        seed = data.seed;
        gridSize = data.gridSize;
        terrainSizeX = data.terrainSizeX;
        terrainSizeZ = data.terrainSizeZ;
        GenerateTerrain();

        // Assign the deserialized placed objects dictionary to the placedObjects field
        PlacedObject[] placedObj = data.placedObjects;

        // Iterate over the placed objects in the dictionary
        foreach (PlacedObject obj in placedObj)
        {
            //Get the object Rotation
            Quaternion objRotation = Quaternion.Euler(0, obj.yRotation, 0);
            // Instantiate the object at the stored position
            GameObject _obj = Instantiate(prefabs[obj.prefabIndex], new Vector3(obj.x, obj.y, obj.z), objRotation);
            UpdateObjects(_obj, false);
        }
        SaveLevel(saveDirectory + levelName + ".json");
    }
    // Define a data class to store the level data
    [Serializable]
    class LevelData
    {
        public PlacedObject[] placedObjects;
        public int seed;
        public float gridSize;
        public int terrainSizeX;
        public int terrainSizeZ;
    }

    class PlacedObject
    {
        public int prefabIndex;
        public float x;
        public float y;
        public float z;
        public float yRotation;
        public PlacedObject(int prefabIndex, float x, float y, float z, float yRotation)
        {
            this.prefabIndex = prefabIndex;
            this.x = x;
            this.y = y;
            this.z = z;
            this.yRotation = yRotation;
        }
    }

    public class PlayerSaveData
    {
        public string playerName;
        public float x;
        public float y;
        public float z;
        public PlayerSaveData(string playerName, float x, float y, float z)
        {
            this.playerName = playerName;
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

}
