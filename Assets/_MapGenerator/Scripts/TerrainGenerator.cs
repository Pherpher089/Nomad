using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour
{

    const float viewerMoveThresholdForChunkUpdate = 75f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public int colliderLODIndex;
    public LODInfo[] detailLevels;

    public MeshSettings meshSettings;

    public BiomeData biomeMapBiomeData;
    public HeightMap biomeHeightMap;
    public BiomeData[] biomeDataArray;

    public Transform viewer;
    public Material originalMat;
    public Material[] mapMaterials;
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
        // Ensure that at least one biome is defined
        if (biomeDataArray == null || biomeDataArray.Length == 0)
        {
            Debug.LogError("No biomes defined!");
            return;
        }

        // Use the first biome's texture settings
        BiomeData firstBiomeData = biomeDataArray[0];
        mapMaterials = new Material[biomeDataArray.Length];
        for (int i = 0; i < biomeDataArray.Length; i++)
        {
            Material mat = new Material(originalMat);
            biomeDataArray[i].textureData.ApplyToMaterial(mat);
            biomeDataArray[i].textureData.UpdateMeshHeights(mat, biomeDataArray[i].heightMapSettings.minHeight, biomeDataArray[i].heightMapSettings.maxHeight);
            mapMaterials[i] = mat;
        }
        // firstBiomeData.textureData.ApplyToMaterial(originalMat);
        // firstBiomeData.textureData.UpdateMeshHeights(originalMat, firstBiomeData.heightMapSettings.minHeight, firstBiomeData.heightMapSettings.maxHeight);
        biomeHeightMap = HeightMapGenerator.GenerateHeightMap(1000, 1000, biomeMapBiomeData, new Vector2(0, 0));
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
                        //float testVal = biomeHeightMap.values[xOffset, yOffset];
                        int x = xOffset + (biomeHeightMap.values.GetLength(0) / 2);
                        int y = yOffset + (biomeHeightMap.values.GetLength(1) / 2);
                        float val = 0;
                        if (x < biomeHeightMap.values.GetLength(0) && x >= 0 && y < biomeHeightMap.values.GetLength(1) && x >= 0)
                        {
                            val = biomeHeightMap.values[x, y];
                        }

                        int biomeIndex = DetermineBiome(val);
                        BiomeData biomeData = biomeDataArray[biomeIndex];
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, biomeData, meshSettings, detailLevels, colliderLODIndex, transform, viewer, mapMaterials[biomeIndex]);
                        terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                        newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
                        newChunk.Load();
                    }
                }

            }
        }
    }
    int DetermineBiome(float height)
    {
        if (height < 5f)
            return 0;
        else
            return 1;
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