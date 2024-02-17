using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeastSpawnPoint : MonoBehaviour
{
    public string spawnName;
    public State startingState;

    void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }
}
