using UnityEngine;
using System.Collections.Generic;
public enum ActorToSpawn { Enemy };
/// <summary>
/// Will spawn the selected Actor to Spawn at this 
/// </summary>
public class ActorSpawner : MonoBehaviour
{

    /// <summary>
    /// The prefab of the actor to spawn from this point.
    /// </summary>
    public ActorToSpawn actorToSpawn;
    private MeshRenderer m_Renderer;
    private GameObject actor;
    /// <summary>
    /// List of spawned actors
    /// </summary>
    public List<GameObject> spawnedActors;
    /// <summary>
    /// What is the max amount of actors that can exist at once produced buy this
    /// spawner?
    /// </summary>
    [Range(1, 5)] public int maxActorCount = 1;
    [Tooltip("How often should the spawner produce an actor?")]
    [Range(1, 60)] public float spawnInterval = 30f;
    /// <summary>
    /// The actual counter for the interval timer
    /// </summary>
    public float spawnCounter;


    private void Awake()
    {
        m_Renderer = GetComponent<MeshRenderer>();
        switch (actorToSpawn)
        {
            case ActorToSpawn.Enemy:
                actor = Resources.Load("Enemy_Axe_01") as GameObject;

                break;
            default:
                break;
        }
    }

    private void Start()
    {
        m_Renderer.enabled = false;
        SpawnActor();
    }

    private void SpawnActor()
    {
        Debug.Log("### Collider index " + gameObject.name + transform.parent.gameObject.GetComponent<TerrainChunkRef>().terrainChunk.colliderLODIndex);
        if (transform.parent.gameObject.GetComponent<MeshCollider>().sharedMesh != null)
        {
            GameObject newSpwn = Instantiate(actor, transform.position, transform.rotation, null);
            spawnedActors.Add(newSpwn);
        }
    }

    void Update()
    {
        if (spawnedActors.Count < maxActorCount)
            if (spawnCounter > 0)
            {
                spawnCounter -= Time.deltaTime;
            }
            else
            {
                spawnCounter = spawnInterval;
                SpawnActor();
            }
    }
}
