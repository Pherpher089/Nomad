using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HealthManager))]
[RequireComponent(typeof(Collider))]
/// <summary>
/// Manages the state of an object. Objects only have health tracked and will
/// </summary>
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
}
