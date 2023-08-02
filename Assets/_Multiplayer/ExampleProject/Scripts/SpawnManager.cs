using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;
    SpawnPoint[] spawnPoints;
    void Awake()
    {
        spawnPoints = GetComponentsInChildren<SpawnPoint>();
        Instance = this;
    }

    public Transform GetSpawnPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length)].transform;
    }
}
