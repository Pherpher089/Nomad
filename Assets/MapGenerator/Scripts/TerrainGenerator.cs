using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour
{

    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;


    public int colliderLODIndex;
    public LODInfo[] detailLevels;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureSettings;

    public Transform viewer;
    public Material mapMaterial;

    Vector2 viewerPosition;
    Vector2 viewerPositionOld;

    float meshWorldSize;
    int chunksVisibleInViewDst;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    public List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    PathfinderController pathfinderController;
    bool initialGeneration = true;

    void Start()
    {
        pathfinderController = FindObjectOfType<PathfinderController>();
        textureSettings.ApplyToMaterial(mapMaterial);
        textureSettings.UpdateMeshHeights(mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
        float maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        meshWorldSize = meshSettings.meshWorldSize;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / meshWorldSize);

        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if (viewerPosition != viewerPositionOld)
        {
            foreach (TerrainChunk chunk in visibleTerrainChunks)
            {
                chunk.UpdateCollisionMesh();
            }
        }

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks()
    {
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();
        for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--)
        {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
                {
                    if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                    {
                        terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    }
                    else
                    {
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, transform, viewer, mapMaterial);
                        terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                        newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
                        newChunk.Load();
                    }
                }

            }
        }
    }

    public static void PopulateObjects(TerrainChunk terrainChunk, Mesh terrainMesh)
    {
        TextureData textureData = FindObjectOfType<TerrainGenerator>().textureSettings;
        Random.InitState(terrainChunk.heightMapSettings.noiseSettings.seed);
        int width = terrainChunk.heightMap.values.GetLength(0);
        ItemManager itemManager = FindObjectOfType<ItemManager>();
        float treenLine = terrainChunk.heightMapSettings.maxHeight * textureData.layers[1].startHeight;

        for (int i = 0; i < terrainMesh.vertices.Length; i += 6)
        {

            // //Grass
            float randomNumber = Random.Range(0f, 1f);
            if (randomNumber > 0.7f && terrainMesh.vertices[i].y > treenLine)
            {
                Quaternion grassRotation = Quaternion.FromToRotation(Vector3.up, terrainMesh.normals[i]);
                GameObject newObj = Instantiate(itemManager.environmentItemList[6], terrainMesh.vertices[i] + new Vector3(terrainChunk.sampleCentre.x, 0, terrainChunk.sampleCentre.y) * terrainChunk.meshSettings.meshScale, Quaternion.identity);

                newObj.transform.Rotate(new Vector3(0, Random.Range(-180, 180), 0));

                newObj.transform.parent = terrainChunk.meshObject.transform;
            }
            randomNumber = Random.Range(0f, 1f);
            // //Rocks
            if (randomNumber > 0.99f)
            {
                GameObject newObj = Instantiate(itemManager.environmentItemList[1], terrainMesh.vertices[i] + new Vector3(terrainChunk.sampleCentre.x, 0, terrainChunk.sampleCentre.y) * terrainChunk.meshSettings.meshScale, Quaternion.identity);
                newObj.transform.parent = terrainChunk.meshObject.transform;
                continue;
            }
            //trees
            randomNumber = Random.Range(0f, 1f);
            if (randomNumber > 0.99f && terrainMesh.vertices[i].y > treenLine)
            {
                GameObject newObj = Instantiate(itemManager.environmentItemList[0], terrainMesh.vertices[i] + new Vector3(terrainChunk.sampleCentre.x, 0, terrainChunk.sampleCentre.y) * terrainChunk.meshSettings.meshScale, Quaternion.identity);
                newObj.transform.parent = terrainChunk.meshObject.transform;
                continue;
            }

        }
    }

    void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible)
    {
        if (isVisible)
        {
            visibleTerrainChunks.Add(chunk);
        }
        else
        {
            visibleTerrainChunks.Remove(chunk);
        }
        if (visibleTerrainChunks.Count > 0)
        {
            //pathfinderController.GenerateTerrainNavMeshGraph(visibleTerrainChunks);
        }
    }

}

[System.Serializable]
public struct LODInfo
{
    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int lod;
    public float visibleDstThreshold;


    public float sqrVisibleDstThreshold
    {
        get
        {
            return visibleDstThreshold * visibleDstThreshold;
        }
    }
}