using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    public string spawnName;
    public bool showMesh = false;
    void Start()
    {
        if (!showMesh) GetComponent<MeshRenderer>().enabled = false;
    }
}
