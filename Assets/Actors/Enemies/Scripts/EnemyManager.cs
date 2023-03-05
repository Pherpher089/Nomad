using UnityEngine;
using UnityEngine.AI;
using Pathfinding;
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(ActorEquipment))]
public class EnemyManager : CharacterManager
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
    float m_OrigGroundCheckDistance;
    const float k_Half = 0.5f;
    float m_xMovement;
    float m_zMovement;
    Vector3 m_GroundNormal;
    float m_CapsuleHeight;
    Vector3 m_CapsuleCenter;
    bool m_Crouching;
    public Animator m_Animator;
    //EquipmentVariables
    ActorEquipment charEquipment;
    //NavMeshAgent m_NavMeshAgent;
    AIPath aiPath;
    void Awake()
    {
        //Gather references from the rest of the game object
        //m_NavMeshAgent = GetComponent<NavMeshAgent>();
        aiPath = GetComponent<AIPath>();
        m_Animator = transform.GetChild(0).GetComponent<Animator>();
        charEquipment = GetComponent<ActorEquipment>();
        m_CharacterObject = transform.Find("CharacterBody").gameObject;
        m_Capsule = GetComponent<CapsuleCollider>();
        m_CapsuleHeight = m_Capsule.height;
        m_CapsuleCenter = m_Capsule.center;
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        m_OrigGroundCheckDistance = m_GroundCheckDistance;
        camObj = GameObject.FindWithTag("MainCamera");
        camFoward = camObj.transform.parent.forward.normalized;
    }

    void Update()
    {
        //Vector3 worldVelocity = m_NavMeshAgent.velocity;
        Vector3 worldVelocity = aiPath.velocity;
        //Vector3 localVelocity = m_NavMeshAgent.transform.InverseTransformDirection(worldVelocity);
        Vector3 localVelocity = aiPath.transform.InverseTransformDirection(worldVelocity);
        UpdateAnimatorMove(localVelocity.normalized);
    }
    public void Attack(bool primary, bool secondary)
    {
        if (!primary && !secondary)
        {
            //weapon attack animation control
            return;
        }
        if (!m_Animator.GetBool("Attacking"))
        {
            m_Animator.ResetTrigger("LeftAttack");
            m_Animator.ResetTrigger("RightAttack");

            AnimatorClipInfo[] clipInfo = m_Animator.GetCurrentAnimatorClipInfo(0);
            if (primary)
            {
                m_Rigidbody.velocity = Vector3.zero;
                m_Animator.SetTrigger("LeftAttack");
                m_Animator.SetBool("Attacking", true);
                m_Animator.SetBool("IsWalking", false);
            }
            else if (secondary)
            {
                m_Rigidbody.velocity = Vector3.zero;
                m_Animator.SetTrigger("RightAttack");
                m_Animator.SetBool("Attacking", true);
                m_Animator.SetBool("IsWalking", false);
            }
        }
    }
    void UpdateAnimatorMove(Vector3 move)
    {
        bool xIsZero = false;
        if (m_Animator.GetBool("Attacking"))
        {
            m_Animator.SetFloat("Horizontal", 0);
            m_Animator.SetFloat("Vertical", 0);
            m_Animator.SetBool("IsWalking", false);
        }
        float threshold = 0.1f;
        if (move.x > threshold || move.x < -threshold)
        {
            m_Animator.SetBool("IsWalking", true);
            m_Animator.SetFloat("Horizontal", move.x);
        }
        else
        {
            xIsZero = true;
            m_Animator.SetFloat("Horizontal", 0);
        }
        if (move.z > threshold || move.z < -threshold)
        {
            m_Animator.SetBool("IsWalking", true);
            m_Animator.SetFloat("Vertical", move.z);
        }
        else
        {
            m_Animator.SetFloat("Vertical", 0);
            if (xIsZero)
            {
                m_Animator.SetBool("IsWalking", false);
            }
        }
    }

    void CheckGroundStatus()
    {
        RaycastHit hitInfo;
        // helper to visualise the ground check ray in the scene view
        Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance), Color.red);

        // 0.1f is a small offset to start the ray from inside the character
        // it is also good to note that the transform position in the sample assets is at the base of the character
        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance))
        {
            m_GroundNormal = hitInfo.normal;
            m_IsGrounded = true;
            //m_Animator.applyRootMotion = true;
        }
        else
        {
            m_IsGrounded = false;
            m_GroundNormal = Vector3.up;
            //m_Animator.applyRootMotion = false;
        }
    }
}
