using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackCurrentTerrainChunk : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("#### getting current chunk");
        if (other.gameObject.tag == "WorldTerrain")
        {
            Debug.Log("#### SETTING current chunk");
            LevelManager.Instance.currentTerrainChunk = other.gameObject.GetComponent<TerrainChunkRef>().terrainChunk;
        }
    }
}
