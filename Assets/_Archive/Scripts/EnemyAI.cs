using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public enum EnemyTarget { None, Player, Survivor, Structure }

public class EnemyAI : MonoBehaviour
{

    //Component Refs
    NavMeshAgent navMeshAgent;                              //A ref to the objects nav mesh agent
    SphereCollider sphereCol;                               //Ref to the object's sphere collider
    AudioSource audioSrc;                                   //A ref to this objects audio source

    //Public variables
    [Header("Player Detection")]
    [Tooltip("The field of view in degrees")]
    public float fieldOfViewAngel = 180;                    //The angle at which the enemy sight ray can detect a player
    [Tooltip("The distance in meters at wich the enemy will detect the player. This is the radious of the sphere collider set to trigger.")]
    public float maxSightDistance = 20;

    //Player detection variables
    List<GameObject> playerList = new List<GameObject>();  //A ref to the player Object
    Transform lastPlayerPos;                                //The last Position this enemy sighted the player charater
    Transform lastSurviverPos;                              //Last know position of any surviver seen by the enemy
    Transform target;
    Transform structurePos;                                 //The target position of the struture that is targeted
    public bool targetInSight;
    public EnemyTarget enemyTarget;

    bool hasAttackPosition;                                 //Is this object occupying an attack 

    int buildLayer;                                         //The Build layer index

    public NavMeshPathStatus pathStatus;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        sphereCol = GetComponent<SphereCollider>();
    }

    private void Start()
    {
        AdjustMasSightDistance(maxSightDistance);

        //Find all player objects in scene
        GatherAllPlayers();

        buildLayer = LayerMask.GetMask("Build");
    }

    public void Update()
    {
        pathStatus = navMeshAgent.pathStatus;
        HandleRotation();
        MoveAgent();
        EnemyStateMachine();
    }

    /// <summary>
    /// Tracks enemy target state.
    /// </summary>
    private void EnemyStateMachine()
    {
        switch (enemyTarget)
        {
            case EnemyTarget.None:
                break;

            case EnemyTarget.Player:
                SetTarget(lastPlayerPos);
                //attack player until the player is dead
                //look for new target
                break;

            case EnemyTarget.Survivor:

                SetTarget(lastSurviverPos);
                //pick up the srvr
                //retreat to a given pos
                break;

            case EnemyTarget.Structure:

                SetTarget(structurePos);

                float dist = navMeshAgent.remainingDistance;
                if (dist != Mathf.Infinity && navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete && navMeshAgent.remainingDistance == 0)
                {
                    //EnemyRotation(targetwall.gameObject.transform);
                }
                //go to the target position
                //attack the structure unill its dead
                //look for new target
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Gathers all players in the scene into a list
    /// </summary>
    private void GatherAllPlayers() //TODO probably could keep this in a static enemy manager class
    {
        GameObject[] playerArray = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in playerArray)
            playerList.Add(player);
    }

    /// <summary>
    /// Sets the radious of the sphere collider. This determines sight distance.
    /// </summary>
    /// <param name="dist"></param>
    private void AdjustMasSightDistance(float dist)
    {
        sphereCol.radius = dist;
    }

    /// <summary>
    /// Rotates the object toward it's foward direction
    /// </summary>
    private void EnemyRotation()
    {
        transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.up);
    }

    /// <summary>
    /// Rotates the object toward a spesific topic
    /// </summary>
    /// <param name="target"></param>
    private void EnemyRotation(Transform target)
    {
        Vector3 targetDir = target.position - transform.position;
        targetDir = new Vector3(targetDir.x, 0, targetDir.z);
        transform.rotation = Quaternion.LookRotation(targetDir, Vector3.up);

    }

    /// <summary>
    /// Manages which rotation is used on this object based on weather there is a target or not
    /// </summary>
    private void HandleRotation()
    {
        if (target != null)
            EnemyRotation(target);
        else
            EnemyRotation();
    }

    /// <summary>
    /// Moves the navemesh agent toward it's target
    /// </summary>
    public void MoveAgent()
    {
        if (target != null)
            navMeshAgent.destination = target.transform.position;
    }

    /// <summary>
    /// Sets the target of the navmesh agent
    /// </summary>
    /// <param name="target"></param>
    public void SetTarget(Transform _target)
    {
        target = _target;
    }

    /// <summary>
    /// Checks line of sight to a target object or the "Player" based on a raycast to the target object. This raycast will not hit trigger colliders;
    /// </summary>
    /// <param name="actor"></param>
    /// <returns></returns>
    private void LookForActor(GameObject actor)
    {
        //TODO set up field of view
        RaycastHit hit;
        Vector3 direction = actor.transform.position - this.transform.position;
        Ray sightLine = new Ray(this.transform.position, direction);

        //make sure if the player isn't seen, the flag is not true
        targetInSight = false;

        if (Physics.Raycast(sightLine, out hit))
        {
            //Raycasts to the player to check line of sight
            if (hit.collider.gameObject.transform.tag == "Player")
            {
                targetInSight = true;
                lastPlayerPos = actor.transform;
                enemyTarget = EnemyTarget.Player;
            }

            //Raycasts to the player to check line of sight
            if (hit.collider.gameObject.transform.tag == "Survivor")
            {
                targetInSight = true;
                lastPlayerPos = actor.transform;
                enemyTarget = EnemyTarget.Survivor;
            }
        }

        if (Physics.Raycast(sightLine, out hit, maxSightDistance, buildLayer, QueryTriggerInteraction.Collide))
        {
            //If the player can not be seen and a wall is in the way, target that wall

            enemyTarget = EnemyTarget.Structure;
            targetInSight = true;
            //targetwall = hit.collider.gameObject.GetComponent<BaseWall>();
            hasAttackPosition = true;
        }

        if (!targetInSight)
        {
            enemyTarget = EnemyTarget.None;
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player" || other.gameObject.tag == "Survivor")
        {
            LookForActor(other.gameObject);
        }
    }
    public void OnCollisionEnter(Collision collision)
    {
        //if(collision.collider.gameObject.tag == "Player")
        //{
        //    collision.collider.gameObject.GetComponent<HealthManager>().TakeDamage(1);
        //}
    }
}
