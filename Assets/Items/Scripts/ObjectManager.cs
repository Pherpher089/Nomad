using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HealthManager))]
[RequireComponent(typeof(Collider))]

public class ObjectManager : MonoBehaviour
{
    public HealthManager healthManager;
    [HideInInspector] public GameObject deathEffectPrefab;
    Collider col;
    // Start is called before the first frame update
    void Awake()
    {
        healthManager = GetComponent<HealthManager>();
        deathEffectPrefab = Resources.Load("DeathEffect") as GameObject;
        col = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
