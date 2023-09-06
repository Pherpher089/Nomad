using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackCurrentTerrainChunk : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "WorldTerrain")
        {
            LevelManager.Instance.currentTerrainChunk = other.gameObject.GetComponent<TerrainChunkRef>().terrainChunk;
        }
    }
}
