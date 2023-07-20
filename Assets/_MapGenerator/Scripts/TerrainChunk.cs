using UnityEngine;

public class TerrainChunk
{

    public string id;
    const float colliderGenerationDistanceThreshold = 75;
    public event System.Action<TerrainChunk, bool> onVisibilityChanged;
    public Vector2 coord;
    [HideInInspector]
    public GameObject meshObject;
    public Vector2 sampleCentre;
    Bounds bounds;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    Rigidbody rigidbody;

    [HideInInspector]
    public HeightMap heightMap;
    bool heightMapReceived;
    bool hasSetCollider;
    float maxViewDst;
    bool hasObjects = false;
    [HideInInspector]
    bool hasGridGraph = false;
    public BiomeData biomeData;
    Transform viewer;
    PathfinderController pathfinderController;
    public TerrainChunkSaveData saveData;
    public TerrainChunk(Vector2 coord, BiomeData biomeData, Transform parent, Transform viewer, Mesh mesh, Material material)
    {
        this.coord = coord;
        id = $"{(int)coord.x}{(int)coord.y}";
        this.biomeData = biomeData;
        this.viewer = viewer;
        sampleCentre = coord * 144;
        Vector2 position = coord * 144;
        bounds = new Bounds(position, Vector2.one * 144);
        meshObject = new GameObject("Terrain Chunk");
        rigidbody = meshObject.AddComponent<Rigidbody>();
        rigidbody.isKinematic = true;
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        meshCollider = meshObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = meshFilter.mesh;
        meshObject.AddComponent<TerrainChunkRef>();
        meshObject.tag = "WorldTerrain";
        meshCollider.GetComponent<TerrainChunkRef>().terrainChunk = this;
        meshRenderer.material = material;
        meshObject.transform.position = new Vector3(position.x, 0, position.y);
        meshObject.transform.parent = parent;
        SetVisible(false);
        maxViewDst = 100;
    }

    public void Load()
    {
        ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(144, 144, biomeData, sampleCentre), OnHeightMapReceived);
    }

    void OnHeightMapReceived(object heightMapObject)
    {
        this.heightMap = (HeightMap)heightMapObject;
        heightMapReceived = true;
        UpdateTerrainChunk();
    }
    Vector2 viewerPosition
    {
        get
        {
            return new Vector2(viewer.position.x, viewer.position.z);
        }
    }
    public void UpdateTerrainChunk()
    {
        if (heightMapReceived)
        {
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));

            bool wasVisible = IsVisible();
            bool visible = viewerDstFromNearestEdge <= maxViewDst;

            if (visible)
            {
                SetVisible(true);
                if (!hasObjects)
                {
                    if (!hasGridGraph)
                    {
                        GameObject.FindObjectOfType<PathfinderController>().GenerateTerrainNavMeshGraph(meshObject.transform.position, meshObject.GetComponent<MeshFilter>().sharedMesh);
                        hasGridGraph = true;
                    }
                    LevelManager.Instance.PopulateObjects(this, meshFilter.sharedMesh);
                    hasObjects = true;
                }

            }
            if (wasVisible != visible)
            {

                SetVisible(visible);
                if (onVisibilityChanged != null)
                {
                    onVisibilityChanged(this, visible);
                }
            }
        }
    }


    public void SetVisible(bool visible)
    {
        meshObject.SetActive(visible);
    }

    public bool IsVisible()
    {
        return meshObject.activeSelf;
    }

}