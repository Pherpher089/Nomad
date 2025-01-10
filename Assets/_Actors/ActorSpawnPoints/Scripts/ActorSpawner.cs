using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using System.IO;
using Pathfinding;

public enum ActorToSpawn { Enemy };
/// <summary>
/// Will spawn the selected Actor to Spawn at this 
/// </summary>
public class ActorSpawner : MonoBehaviour
{
    /// <summary>
    /// The prefab of the actor to spawn from this point.
    /// </summary>
    public string[] actorsToSpawn;
    private MeshRenderer m_Renderer;
    private GameObject[] actor;
    /// <summary>
    /// List of spawned actors
    /// </summary>
    public List<GameObject> spawnedActors;
    /// <summary>
    /// What is the max amount of actors that can exist at once produced buy this
    /// spawner?
    /// </summary>
    [Range(1, 10)] public int maxActorCount = 1;
    [Range(1, 10)] public int maxActorCountNight = 1;
    [Tooltip("How often should the spawner produce an actor?")]
    [Range(1, 270)] public float spawnInterval = 1f;
    [Range(1, 270)] public float spawnIntervalNight = 1f;

    /// <summary>
    /// The actual counter for the interval timer
    /// </summary>
    public float spawnCounter;
    GameStateManager gameState;
    PlayersManager playersManager;
    public bool spawnOnlyAtNight = true;
    public bool increaseNightSpawnDifficulty;
    public float playerSpawnDistance = 30;
    public string id;
    private void Awake()
    {
        if (!PhotonNetwork.IsMasterClient) gameObject.SetActive(false);
        m_Renderer = GetComponent<MeshRenderer>();
        gameState = FindObjectOfType<GameStateManager>();
        playersManager = FindObjectOfType<PlayersManager>();
        spawnCounter = 10;
    }

    private void Start()
    {
        m_Renderer.enabled = false;
    }

    private void SpawnActor()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // Ensure players are not too close to the spawn location
        foreach (ThirdPersonUserControl player in gameState.playersManager.playerList)
        {
            if (Vector3.Distance(transform.position, player.transform.position) < playerSpawnDistance)
            {
                return;
            }
        }

        int spawnIndex = 0;
        if (increaseNightSpawnDifficulty && gameState.timeState == TimeState.Night)
        {
            spawnIndex = Random.Range(0, 2);
        }
        // Prototype logic for spawnIndex - remove later
        spawnIndex = Random.Range(0, actorsToSpawn.Length);

        string actor = actorsToSpawn[spawnIndex];

        // Find a random valid spawn position
        Vector3 randomSpawnPosition = GetRandomSpawnLocation(transform.position, 5f);

        // Instantiate the actor at the valid spawn position
        GameObject newSpwn = PhotonNetwork.Instantiate(System.IO.Path.Combine("PhotonPrefabs", actor), randomSpawnPosition, Quaternion.identity);
        spawnedActors.Add(newSpwn);
        EnemiesManager.Instance.AddEnemy(newSpwn.GetComponent<EnemyManager>());
    }

    /// <summary>
    /// Finds a random position within a given radius and ensures it's walkable using A*.
    /// </summary>
    /// <param name="center">The center point to spawn around.</param>
    /// <param name="radius">The radius within which to find a spawn point.</param>
    /// <returns>A valid walkable position.</returns>
    private Vector3 GetRandomSpawnLocation(Vector3 center, float radius)
    {
        for (int i = 0; i < 10; i++) // Try up to 10 times to find a valid position
        {
            Vector3 randomDirection = Random.insideUnitSphere * radius; // Generate random point in sphere
            randomDirection.y = 0; // Ensure the point is on the horizontal plane
            Vector3 potentialPosition = center + randomDirection;

            // Check if the position is walkable using A* Pathfinding
            if (AstarPath.active != null)
            {
                var graph = AstarPath.active.data.gridGraph;
                if (graph != null)
                {
                    var node = graph.GetNearest(potentialPosition).node;
                    if (node != null && node.Walkable)
                    {
                        return (Vector3)node.position; // Return the valid position
                    }
                }
            }
        }

        // Fallback to center if no valid position is found
        return center;
    }

    private void SpawnBehavior(int _maxActorCount, float _spawnInterval)
    {
        if (spawnedActors.Count < _maxActorCount)
        {

            if (spawnCounter > 0)
            {
                spawnCounter -= Time.deltaTime;
            }
            else
            {
                SpawnActor();
                spawnCounter = _spawnInterval;
            }
        }
    }
    private void DespawnBehavior()
    {

        List<GameObject> livingSpawnedActors = new();
        foreach (GameObject actor in spawnedActors)
        {
            if (actor != null)
            {
                livingSpawnedActors.Add(actor);
            }
        }
        spawnedActors = new List<GameObject>(livingSpawnedActors);
        if (gameState.timeState == TimeState.Day && spawnOnlyAtNight && !gameState.isRaid)
        {
            foreach (GameObject actor in spawnedActors)
            {
                actor.GetComponent<HealthManager>().Kill();
            }
        }
    }
    void Update()
    {
        DespawnBehavior();
        if (gameState.timeState == TimeState.Day && spawnOnlyAtNight && !gameState.isRaid) return;

        if ((gameState.timeState == TimeState.Day || !increaseNightSpawnDifficulty) && !GameStateManager.Instance.isRaid)
        {
            SpawnBehavior(maxActorCount, spawnInterval);
        }
        else
        {
            if (spawnCounter > spawnIntervalNight) spawnCounter = spawnIntervalNight;
            SpawnBehavior(maxActorCountNight, spawnIntervalNight);
        }
    }
}
