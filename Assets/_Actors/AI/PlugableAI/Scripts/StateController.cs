using System.Collections.Generic;
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

    [HideInInspector] public NavMeshAgent navMeshAgent;
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
    public bool aiActive;

    private void Awake()
    {
        CacheComponents();
        InitializeWayPoints();
        moveSpeed = enemyStats.moveSpeed;
    }

    private void CacheComponents()
    {
        m_Animator = GetComponentInChildren<Animator>();
        m_ActorEquipment = GetComponent<ActorEquipment>();
        navMeshAgent = GetComponent<NavMeshAgent>();
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

    public void SetupAI(bool aiActivationFromTankManager)
    {
        aiActive = aiActivationFromTankManager;
        navMeshAgent.enabled = aiActive;
    }

    private void Update()
    {
        if (!CompareTag("Beast"))
        {
            aiActive = PlayersManager.Instance.GetDistanceToClosestPlayer(transform) <= 50 || GameStateManager.Instance.isRaid;
        }

        if (!aiActive || currentState == null) return;

        currentState.UpdateState(this);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(navMeshAgent.destination, 1);

        if (currentState != null)
        {
            Gizmos.color = currentState.SceneGizmoColor;
            Gizmos.DrawWireSphere(eyes.position, enemyStats.lookSphereCastRadius);
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
