using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    public string spawnName;
    void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }
}
