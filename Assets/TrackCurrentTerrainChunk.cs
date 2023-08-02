using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackCurrentTerrainChunk : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "WorldTerrain" && LevelManager.Instance.currentTerrainChunk != null && other.gameObject.GetComponent<TerrainChunkRef>().terrainChunk.id != LevelManager.Instance.currentTerrainChunk.id)
        {
            LevelManager.Instance.currentTerrainChunk = other.gameObject.GetComponent<TerrainChunkRef>().terrainChunk;
        }
    }
}
