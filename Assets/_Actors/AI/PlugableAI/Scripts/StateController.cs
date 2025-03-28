using System;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.AI;

public class StateController : MonoBehaviour
{
    public State currentState;
    public EnemyStats enemyStats;
    public Transform eyes;
    public GameObject wayPointParent;
    public List<Transform> wayPointList = new List<Transform>();
    public State remainState;

    [HideInInspector] public AIPath aiPath;
    public int nextWayPoint;
    public Transform target;
    public Transform lastTarget;
    [HideInInspector] public bool focusOnTarget;
    [HideInInspector] public SphereCollider sphereCollider;
    [HideInInspector] public ActorEquipment equipment;
    [HideInInspector] public Rigidbody rigidbodyRef;
    [HideInInspector] public EnemyManager enemyManager;
    [HideInInspector] public AIMover aiMover;
    [HideInInspector] public Dictionary<string, float> playerDamageMap = new Dictionary<string, float>();
    [HideInInspector] public float reevaluateTargetCounter = 0;
    [HideInInspector] public float attackCoolDown = 0;
    [HideInInspector] public ActorEquipment m_ActorEquipment;
    [HideInInspector] public Animator m_Animator;
    [HideInInspector] public float moveSpeed = 0;
    [HideInInspector] public float despawnTimer = 25;
    [HideInInspector] public float despawnTimeLimit = 25;
    Vector3 lastDestination = Vector3.zero;
    int timeSlice = 3;
    int sliceCounter = 0;
    public bool aiActive;
    public bool isGrounded;
    public Vector3 retreatPosition = Vector3.zero;
    private void Awake()
    {
        CacheComponents();
        aiPath.maxSpeed = enemyStats.moveSpeed;
        InitializeWayPoints();
        moveSpeed = enemyStats.moveSpeed;
    }

    private void CacheComponents()
    {
        m_Animator = GetComponentInChildren<Animator>();
        m_ActorEquipment = GetComponent<ActorEquipment>();
        aiPath = GetComponent<AIPath>();
        sphereCollider = GetComponent<SphereCollider>();
        equipment = GetComponent<ActorEquipment>();
        rigidbodyRef = GetComponent<Rigidbody>();
        enemyManager = GetComponent<EnemyManager>();
        aiMover = GetComponent<AIMover>();
    }

    private void InitializeWayPoints()
    {
        if (wayPointParent != null)
        {
            foreach (Transform child in wayPointParent.transform)
            {
                wayPointList.Add(child);
            }
        }
    }

    public void EnableAi(bool aiActivationFromTankManager)
    {
        aiActive = aiActivationFromTankManager;
        aiPath.enabled = aiActive;
    }

    private void Update()
    {
        // if (sliceCounter >= timeSlice)
        if (true)
        {
            sliceCounter = 0;
            if (!CompareTag("Beast"))
            {
                aiActive = (PlayersManager.Instance.GetDistanceToClosestPlayer(transform) <= 40) || GameStateManager.Instance.isRaid;
            }
            else
            {
                if (lastDestination != aiPath.destination && aiPath.destination != Mathf.Infinity * Vector3.one)
                {
                    lastDestination = aiPath.destination;
                }
            }

            if (!aiActive || currentState == null) return;

            currentState.UpdateState(this);
        }
        else
        {
            sliceCounter++;
        }

    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(aiPath.destination, 1);

        if (currentState != null)
        {
            Gizmos.color = currentState.SceneGizmoColor;
            // Gizmos.DrawWireSphere(eyes.position, enemyStats.lookSphereCastRadius);
        }
    }

    public void TransitionToState(State nextState)
    {
        if (nextState != remainState)
        {
            currentState = nextState;
        }
    }
}
