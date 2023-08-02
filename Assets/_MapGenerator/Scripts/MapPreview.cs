using UnityEngine;
using System.Collections;

public class MapPreview : MonoBehaviour
{
    public Renderer textureRender;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public enum DrawMode { NoiseMap, Mesh, FalloffMap };
    public DrawMode drawMode;
    public MeshSettings meshSettings;
    public BiomeData biomeData;
    public Material terrainMaterial;
    public int editorPreviewLOD;
    public bool autoUpdate;

    public void DrawMapInEditor()
    {
        biomeData.textureData.ApplyToMaterial(terrainMaterial);
        biomeData.textureData.UpdateMeshHeights(terrainMaterial, biomeData.heightMapSettings.minHeight, biomeData.heightMapSettings.maxHeight);
        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(TerrainGenerator.Instance.meshSize, TerrainGenerator.Instance.meshSize, biomeData, new Vector2(-this.transform.position.x, this.transform.position.z));

        if (drawMode == DrawMode.NoiseMap)
        {
            DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            int[,] surroundingBiomes = new int[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    surroundingBiomes[i, j] = 0;
                }
            }
            BiomeData[] biomeDataArray = FindObjectOfType<TerrainGenerator>().biomeDataArray;
            //DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD, surroundingBiomes, biomeDataArray));
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(FalloffGenerator.GenerateFalloffMap(TerrainGenerator.Instance.meshSize), 0, 1)));
        }
    }

    public void DrawTexture(Texture2D texture)
    {
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height) / 10f;

        textureRender.gameObject.SetActive(true);
        meshFilter.gameObject.SetActive(false);
    }

    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    void OnTextureValuesUpdated()
    {
        biomeData.textureData.ApplyToMaterial(terrainMaterial);
    }

    void OnValidate()
    {

        if (meshSettings != null)
        {
            meshSettings.OnValuesUpdated -= OnValuesUpdated;
            meshSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (biomeData.heightMapSettings != null)
        {
            biomeData.heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
            biomeData.heightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (biomeData.textureData != null)
        {
            biomeData.textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            biomeData.textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }

    }

}