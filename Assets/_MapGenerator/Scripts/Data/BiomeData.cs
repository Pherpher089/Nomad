using UnityEngine;

[CreateAssetMenu()]
public class BiomeData : UpdatableData
{
    public string biomeName; // The name of the biome
    public TextureData textureData; // The textures used in this biome
    public HeightMapSettings heightMapSettings; // The height map settings for this biome
}
