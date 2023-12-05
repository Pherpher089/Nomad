using UnityEngine;
using Pathfinding;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(ActorEquipment))]
public class EnemyManager : ActorManager
{
    [SerializeField] float m_turnSmooth = 5;
    [SerializeField] float m_JumpPower = 12f;
    [Range(1f, 4f)][SerializeField] float m_GravityMultiplier = 2f;
    [SerializeField] float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
    [SerializeField] float m_MoveSpeedMultiplier = 1f;
    [SerializeField] public float m_AnimSpeedMultiplier = 1f;
    [SerializeField] float m_GroundCheckDistance = 0.1f;

    Rigidbody m_Rigidbody;
    //Animator m_Animator;
    CapsuleCollider m_Capsule;
    GameObject m_CharacterObject;
    GameObject camObj;
    Vector3 camFoward;
    [HideInInspector] public bool m_IsGrounded;
    const float k_Half = 0.5f;
    public Animator m_Animator;
    //EquipmentVariables
    ActorEquipment equipment;
    //NavMeshAgent m_NavMeshAgent;
    AIPath aiPath;
    bool hasDiedAndDroppedLoot = false;
    public override void Awake()
    {
        base.Awake();
        if (!PhotonNetwork.IsMasterClient)
        {
            GetComponent<AIPath>().enabled = false;
            GetComponent<StateController>().enabled = false;
            GetComponent<AIMover>().enabled = false;
        }
    }
    public void Start()
    {
        //Gather references from the rest of the game object
        //m_NavMeshAgent = GetComponent<NavMeshAgent>();
        aiPath = GetComponent<AIPath>();
        m_Animator = transform.GetChild(0).GetComponent<Animator>();
        m_CharacterObject = transform.Find("CharacterBody").gameObject;
        m_Capsule = GetComponent<CapsuleCollider>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        camObj = GameObject.FindWithTag("MainCamera");
        camFoward = camObj.transform.parent.forward.normalized;
        equipment = GetComponent<ActorEquipment>();
    }

    public override void Update()
    {
        if (isDead && !hasDiedAndDroppedLoot)
        {
            aiPath.destination = transform.position;
            aiPath.canMove = false;
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<StateController>().currentState = null;
            GetComponent<StateController>().aiActive = false;
            GetComponent<Collider>().enabled = false;
            for (int i = 0; i < 6; i++)
            {
                PlayerInventoryManager.Instance.DropItem(18, transform.position);
            }
            if (equipment != null && equipment.equippedItem != null)
            {
                PlayerInventoryManager.Instance.DropItem(equipment.equippedItem.GetComponent<Item>().itemIndex, transform.position);
            }
            hasDiedAndDroppedLoot = true;
        }
        base.Update();
    }
}
