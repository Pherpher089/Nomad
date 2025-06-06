using System;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class StateController : MonoBehaviour
{
    public State currentState;
    public EnemyStats enemyStats;
    public Transform eyes;
    public GameObject wayPointParent;
    public List<Transform> wayPointList = new List<Transform>();
    public State remainState;

    [Header("AI Settings")]
    [SerializeField] private float updateInterval = 0.2f;
    [SerializeField] private float playerCheckInterval = 0.3f;
    [SerializeField] private float activationDistance = 30f;

    [Header("Despawn Settings")]
    [SerializeField] private float despawnTimeLimit = 25f;
    [SerializeField] private float despawnDistance = 60f; // How far is too far?

    [HideInInspector] public AIPath aiPath;
    [HideInInspector] public int nextWayPoint;
    [HideInInspector] public Transform target;
    [HideInInspector] public Transform lastTarget;
    [HideInInspector] public bool focusOnTarget;
    [HideInInspector] public SphereCollider sphereCollider;
    [HideInInspector] public Rigidbody rigidbodyRef;
    [HideInInspector] public EnemyManager enemyManager;
    [HideInInspector] public AIMover aiMover;
    [HideInInspector] public Dictionary<string, float> playerDamageMap = new();
    [HideInInspector] public float reevaluateTargetCounter = 0;
    [HideInInspector] public float attackCoolDown = 0;
    [HideInInspector] public ActorEquipment m_ActorEquipment;
    [HideInInspector] public Animator m_Animator;
    [HideInInspector] public float moveSpeed = 0;
    [HideInInspector] public bool aiActive;
    [HideInInspector] public bool isGrounded;
    public Vector3 retreatPosition = Vector3.zero;

    private Vector3 lastDestination = Vector3.zero;
    private float nextUpdateTime = 0f;
    private float nextPlayerCheckTime = 0f;
    private float despawnTimer = 0f;
    public bool wasSpawned = false;

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
        m_Animator.enabled = aiActive;
        rigidbodyRef.isKinematic = aiActive;
        aiMover.enabled = aiActive;
    }

    private void Update()
    {
        float time = Time.time;

        if (time >= nextPlayerCheckTime)
        {
            nextPlayerCheckTime = time + playerCheckInterval;

            float distanceToPlayer = PlayersManager.Instance?.GetDistanceToClosestPlayer(transform) ?? float.MaxValue;

            if (!CompareTag("Beast"))
            {
                aiActive = distanceToPlayer <= activationDistance || GameStateManager.Instance.isRaid;
            }
            else
            {
                if (lastDestination != aiPath.destination && aiPath.destination != Mathf.Infinity * Vector3.one)
                {
                    lastDestination = aiPath.destination;
                }
            }

            HandleDespawnCheck(distanceToPlayer);
        }

        if (!aiActive || currentState == null || time < nextUpdateTime)
            return;

        nextUpdateTime = time + updateInterval;
        currentState.UpdateState(this);
    }

    private void HandleDespawnCheck(float distanceToPlayer)
    {
        if (distanceToPlayer > despawnDistance)
        {
            despawnTimer += playerCheckInterval;
            if (despawnTimer >= despawnTimeLimit)
            {
                Despawn();
            }
        }
        else
        {
            despawnTimer = 0f; // Reset timer if player is nearby
        }
    }

    private void Despawn()
    {
        if (wasSpawned)
        {
            // Optional: play animation or fade before destruction
            Destroy(gameObject);
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
