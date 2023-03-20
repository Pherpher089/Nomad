using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System;

public class LevelManager : MonoBehaviour
{
    public static TerrainChunkSaveData PopulateObjects(TerrainChunk terrainChunk, Mesh terrainMesh)
    {
        List<TerrainObjectSaveData> placedObjects = new List<TerrainObjectSaveData>();
        TextureData textureData = FindObjectOfType<TerrainGenerator>().textureSettings;
        UnityEngine.Random.InitState(terrainChunk.heightMapSettings.noiseSettings.seed);
        int width = terrainChunk.heightMap.values.GetLength(0);
        ItemManager itemManager = FindObjectOfType<ItemManager>();
        float treenLine = terrainChunk.heightMapSettings.maxHeight * textureData.layers[1].startHeight;
        TerrainChunkSaveData chunkSaveData = LevelManager.LoadChunk(terrainChunk);
        if (chunkSaveData != null)
        {
            foreach (TerrainObjectSaveData item in chunkSaveData.objects)
            {
                GameObject newObj = Instantiate(itemManager.environmentItemList[item.itemIndex], new Vector3(item.x, item.y, item.z), Quaternion.identity);
                newObj.transform.Rotate(new Vector3(item.rx, item.ry, item.rz));
                TerrainObjectSaveData currentObj = new TerrainObjectSaveData(item.itemIndex, newObj.transform.position.x, newObj.transform.position.y, newObj.transform.position.z, newObj.transform.rotation.eulerAngles.x, newObj.transform.rotation.eulerAngles.y, newObj.transform.rotation.eulerAngles.z);
                placedObjects.Add(currentObj);
                newObj.transform.parent = terrainChunk.meshObject.transform;
            }
            return chunkSaveData;
        }
        else
        {

            for (int i = 0; i < terrainMesh.vertices.Length; i += 6)
            {

                // //Grass
                float randomNumber = UnityEngine.Random.Range(0f, 1f);
                if (randomNumber > 0.7f && terrainMesh.vertices[i].y > treenLine)
                {
                    int itemIndex = 6;
                    Quaternion grassRotation = Quaternion.FromToRotation(Vector3.up, terrainMesh.normals[i]);
                    GameObject newObj = Instantiate(itemManager.environmentItemList[itemIndex], terrainMesh.vertices[i] + new Vector3(terrainChunk.sampleCentre.x, 0, terrainChunk.sampleCentre.y) * terrainChunk.meshSettings.meshScale, Quaternion.identity);

                    newObj.transform.Rotate(new Vector3(0, UnityEngine.Random.Range(-180, 180), 0));
                    TerrainObjectSaveData currentObj = new TerrainObjectSaveData(itemIndex, newObj.transform.position.x, newObj.transform.position.y, newObj.transform.position.z, newObj.transform.rotation.eulerAngles.x, newObj.transform.rotation.eulerAngles.y, newObj.transform.rotation.eulerAngles.z);
                    placedObjects.Add(currentObj);
                    newObj.transform.parent = terrainChunk.meshObject.transform;
                }
                randomNumber = UnityEngine.Random.Range(0f, 1f);
                // //Rocks
                if (randomNumber > 0.999f)
                {
                    int itemIndex = 1;
                    GameObject newObj = Instantiate(itemManager.environmentItemList[itemIndex], terrainMesh.vertices[i] + new Vector3(terrainChunk.sampleCentre.x, 0, terrainChunk.sampleCentre.y) * terrainChunk.meshSettings.meshScale, Quaternion.identity);
                    newObj.transform.parent = terrainChunk.meshObject.transform;
                    TerrainObjectSaveData currentObj = new TerrainObjectSaveData(itemIndex, newObj.transform.position.x, newObj.transform.position.y, newObj.transform.position.z, newObj.transform.rotation.eulerAngles.x, newObj.transform.rotation.eulerAngles.y, newObj.transform.rotation.eulerAngles.z);
                    placedObjects.Add(currentObj);
                    continue;
                }
                //trees
                randomNumber = UnityEngine.Random.Range(0f, 1f);
                if (randomNumber > 0.99f && terrainMesh.vertices[i].y > treenLine)
                {
                    int itemIndex = 0;
                    GameObject newObj = Instantiate(itemManager.environmentItemList[itemIndex], terrainMesh.vertices[i] + new Vector3(terrainChunk.sampleCentre.x, 0, terrainChunk.sampleCentre.y) * terrainChunk.meshSettings.meshScale, Quaternion.identity);
                    newObj.transform.parent = terrainChunk.meshObject.transform;
                    TerrainObjectSaveData currentObj = new TerrainObjectSaveData(itemIndex, newObj.transform.position.x, newObj.transform.position.y, newObj.transform.position.z, newObj.transform.rotation.eulerAngles.x, newObj.transform.rotation.eulerAngles.y, newObj.transform.rotation.eulerAngles.z);
                    placedObjects.Add(currentObj);
                    continue;
                }

                // if (randomNumber > 0.99f)
                // {
                //     int itemIndex = 1;
                //     GameObject newObj = Instantiate(itemManager.environmentItemList[itemIndex], terrainMesh.vertices[i] + new Vector3(terrainChunk.sampleCentre.x, 0, terrainChunk.sampleCentre.y) * terrainChunk.meshSettings.meshScale, Quaternion.identity);
                //     newObj.transform.parent = terrainChunk.meshObject.transform;
                //     TerrainObjectSaveData currentObj = new TerrainObjectSaveData(itemIndex, newObj.transform.position.x, newObj.transform.position.y, newObj.transform.position.z, newObj.transform.rotation.eulerAngles.x, newObj.transform.rotation.eulerAngles.y, newObj.transform.rotation.eulerAngles.z);
                //     placedObjects.Add(currentObj);
                //     continue;
                // }
            }


            TerrainObjectSaveData[] saveData = new TerrainObjectSaveData[placedObjects.Count];
            int count = 0;
            foreach (TerrainObjectSaveData item in placedObjects)
            {
                saveData[count] = item;
                count++;
            }
            return new TerrainChunkSaveData(saveData);
        }
    }

    public static TerrainChunkSaveData LoadChunk(TerrainChunk terrainChunk)
    {
        string saveFilePath = FindObjectOfType<GameStateManager>().saveFilePath;
        string levelName = FindObjectOfType<GameStateManager>().m_WorldName;
        string filePath = saveFilePath + "/Levels/" + levelName + terrainChunk.coord.x + '-' + terrainChunk.coord.y + ".json";

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
    public static void SaveChunk(TerrainChunk terrainChunk)
    {
        string saveFilePath = FindObjectOfType<GameStateManager>().saveFilePath;
        string levelName = FindObjectOfType<GameStateManager>().m_WorldName;
        string filePath = saveFilePath + "/Levels/" + levelName + terrainChunk.coord.x + '-' + terrainChunk.coord.y + ".json";
        TerrainChunkSaveData data = terrainChunk.saveData;
        string json = JsonConvert.SerializeObject(data);
        // Open the file for writing
        using (FileStream stream = new FileStream(filePath, FileMode.Create))
        using (StreamWriter writer = new StreamWriter(stream))
        {
            // Write the JSON string to the file
            writer.Write(json);
        }
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
