using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using System.IO;

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
    [Range(1, 5)] public int maxActorCount = 1;
    [Range(1, 5)] public int maxActorCountNight = 1;
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
        string actor = actorsToSpawn[spawnIndex];
        if (transform.parent.gameObject.GetComponent<BoxCollider>() != null)
        {
            GameObject newSpwn = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", actor), transform.position, transform.rotation);
            spawnedActors.Add(newSpwn);
        }
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
        if (spawnedActors.Count > 0)
        {
            Vector3 playerPos = playersManager.GetCenterPoint();
            List<GameObject> toRemove = new List<GameObject>();
            foreach (GameObject actor in spawnedActors)
            {
                if ((actor.transform.position - playerPos).sqrMagnitude < playerSpawnDistance * playerSpawnDistance * 4)
                {
                    actor.GetComponent<HealthManager>().health = 0;
                    toRemove.Add(actor);
                }
            }
            foreach (GameObject actor in toRemove)
            {
                spawnedActors.Remove(actor);
            }
        }
    }
    void Update()
    {
        if (gameState.peaceful || (gameState.timeState == TimeState.Day && spawnOnlyAtNight))
            return;

        if (gameState.timeState == TimeState.Day)
        {
            SpawnBehavior(maxActorCount, spawnInterval);
        }
        else
        {
            SpawnBehavior(maxActorCountNight, spawnIntervalNight);
        }
    }
}
