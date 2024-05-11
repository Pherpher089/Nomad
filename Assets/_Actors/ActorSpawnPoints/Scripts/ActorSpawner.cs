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
        //This is proto type below. Just for now. Remove later
        spawnIndex = Random.Range(0, actorsToSpawn.Length);

        string actor = actorsToSpawn[spawnIndex];
        if (transform.parent.gameObject.GetComponent<Collider>() != null)
        {
            GameObject newSpwn = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", actor), transform.position, transform.rotation);
            spawnedActors.Add(newSpwn);
            EnemiesManager.Instance.AddEnemy(newSpwn.GetComponent<EnemyManager>());
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

        List<GameObject> livingSpawnedActors = new();
        foreach (GameObject actor in spawnedActors)
        {
            if (actor != null)
            {
                livingSpawnedActors.Add(actor);
                actor.GetComponent<HealthManager>().Kill();
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
