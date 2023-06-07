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
    [Range(1, 60)] public float spawnInterval = 1f;
    /// <summary>
    /// The actual counter for the interval timer
    /// </summary>
    public float spawnCounter;
    GameStateManager gameState;
    public bool spawnOnlyAtNight = true;
    public float playerSpawnDistance = 30;

    private void Awake()
    {
        m_Renderer = GetComponent<MeshRenderer>();
        gameState = FindObjectOfType<GameStateManager>();
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
    }

    private void SpawnActor()
    {
        if (transform.parent.gameObject.GetComponent<MeshCollider>().sharedMesh != null)
        {
            GameObject newSpwn = Instantiate(actor, transform.position, transform.rotation, null);
            spawnedActors.Add(newSpwn);
        }
    }

    void Update()
    {
        if (FindObjectOfType<GameStateManager>().peaceful == true)
            return;
        if (gameState.timeState == TimeState.Day && spawnOnlyAtNight)
        {
            if (spawnedActors.Count > 0)
            {
                Vector3 playerPos = FindObjectOfType<PlayersManager>().GetCenterPoint();
                foreach (GameObject actor in spawnedActors)
                {
                    if (Vector3.Distance(actor.transform.position, playerPos) < playerSpawnDistance * 2)
                    {
                        Debug.Log("### Despawning");
                        actor.GetComponent<HealthManager>().health = 0;
                        spawnedActors.Remove(actor);
                        return;
                    }
                }
                return;
            }
            return;
        }
        else if (gameState.timeState == TimeState.Night && spawnOnlyAtNight || !spawnOnlyAtNight)
        {
            foreach (ThirdPersonUserControl player in gameState.playersManager.playerList)
            {
                if (Vector3.Distance(transform.position, player.transform.position) < playerSpawnDistance)
                {
                    return;
                }
            }
            if (spawnedActors.Count < maxActorCount)
            {

                if (spawnCounter > 0)
                {
                    spawnCounter -= Time.deltaTime;
                }
                else
                {
                    Debug.Log("### spawning");
                    SpawnActor();
                    spawnCounter = spawnInterval;
                }
            }
        }
    }
}
