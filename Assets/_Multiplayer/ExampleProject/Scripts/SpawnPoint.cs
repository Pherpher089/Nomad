using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    void Awake()
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
    }
}
