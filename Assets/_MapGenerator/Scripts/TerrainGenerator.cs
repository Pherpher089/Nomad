using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

public class TerrainGenerator : MonoBehaviour
{
    public static TerrainGenerator Instance;
    const float viewerMoveThresholdForChunkUpdate = 50f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    const float maxViewDst = 100;
    public BiomeData biomeMapBiomeData;
    public HeightMap biomeHeightMap;
    public BiomeData[] biomeDataArray;

    public Transform viewer;
    public Material terrainChunkMaterial;

    Vector2 viewerPosition;
    Vector2 viewerPositionOld;

    public int meshSize = 144;
    int seed;
    int chunksVisibleInViewDst;

    public Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    public List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();
    bool initialGeneration = true;
    public bool hasCompletedInitialGeneration = false;
    public Mesh terrainChunkMesh;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        // Ensure that at least one biome is defined
        if (biomeDataArray == null || biomeDataArray.Length == 0)
        {
            return;
        }

        // Use the first biome's texture settings
        BiomeData firstBiomeData = biomeDataArray[0];


        biomeHeightMap = HeightMapGenerator.GenerateHeightMap(1000, 1000, biomeMapBiomeData, new Vector2(0, 0));
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / meshSize);
        if (LevelPrep.Instance.receivedLevelFiles || PhotonNetwork.IsMasterClient)
        {
            UpdateVisibleChunks();
        }
    }


    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            if (LevelPrep.Instance.receivedLevelFiles || PhotonNetwork.IsMasterClient)
            {
                UpdateVisibleChunks();
            }
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

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / 144);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / 144);

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
                        bool firstGeneration = viewedChunkCoord.x == 0 && viewedChunkCoord.y == 0 ? true : false;
                        int biomeIndex = DetermineBiome(val, firstGeneration, viewedChunkCoord);
                        BiomeData biomeData = biomeDataArray[biomeIndex];

                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, biomeData, transform, viewer, terrainChunkMesh, terrainChunkMaterial);
                        terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                        newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
                        newChunk.Load();
                    }
                }

            }
        }
        hasCompletedInitialGeneration = true;
    }
    int DetermineBiome(float height, bool firstGen, Vector2 coords)
    {
        if (Vector2.Distance(new Vector2(0, 0), coords) < 5f)
        {
            return 0;
        }
        if (height < 5.5f)
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