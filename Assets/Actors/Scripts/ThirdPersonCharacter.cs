using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(ActorEquipment))]
//[RequireComponent(typeof(Animator))]
public class ThirdPersonCharacter : MonoBehaviour
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

    void Awake()
    {
        m_Animator = transform.GetChild(0).GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Capsule = GetComponent<CapsuleCollider>();
        m_CharacterObject = transform.Find("CharacterBody").gameObject;
        m_CapsuleHeight = m_Capsule.height;
        m_CapsuleCenter = m_Capsule.center;
        charEquipment = GetComponent<ActorEquipment>();
        camObj = GameObject.FindWithTag("MainCamera");
        camFoward = camObj.transform.parent.forward.normalized;
        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        m_OrigGroundCheckDistance = m_GroundCheckDistance;

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

    public void Move(Vector3 move, bool crouch, bool jump)
    {
        if (m_Animator.GetBool("Attacking"))
        {
            m_Animator.SetBool("IsWalking", false);
            return;
        }

        // convert the world relative moveInput vector into a local-relative
        // turn amount and forward amount required to head in the desired
        // direction.
        if (move.magnitude > 1f) move.Normalize();
        move = camObj.transform.TransformDirection(move);
        CheckGroundStatus();
        move = Vector3.ProjectOnPlane(move, m_GroundNormal);
        //m_TurnAmount = Mathf.Atan2(move.x, move.z);
        m_zMovement = move.z * m_MoveSpeedMultiplier;
        m_xMovement = move.x * m_MoveSpeedMultiplier;
        // control and velocity handling is different when grounded and airborne:
        if (m_IsGrounded)
        {
            HandleGroundedMovement(crouch, jump);

        }
        else
        {
            HandleAirborneMovement();
        }

        ScaleCapsuleForCrouching(crouch);
        PreventStandingInLowHeadroom();

        // send input and other state parameters to the animator
        UpdateAnimatorMove(move);
        m_Rigidbody.velocity = new Vector3(m_xMovement, m_Rigidbody.velocity.y, m_zMovement);
    }
    void UpdateAnimatorMove(Vector3 move)
    {
        if (m_Animator.GetBool("Attacking"))
        {
            m_Animator.SetFloat("Horizontal", 0);
            m_Animator.SetFloat("Vertical", 0);
            m_Animator.SetBool("IsWalking", false);
        }
        float threshold = 0.3f;
        if (move.x > threshold || move.x < -threshold || move.z > threshold || move.z < -threshold)
        {
            m_Animator.SetBool("IsWalking", true);
            Vector3 localVelocity = transform.InverseTransformDirection(m_Rigidbody.velocity.normalized);
            m_Animator.SetFloat("Horizontal", localVelocity.x);
            m_Animator.SetFloat("Vertical", localVelocity.z);
        }
        else
        {
            m_Animator.SetFloat("Horizontal", 0);
            m_Animator.SetFloat("Vertical", 0);
            m_Animator.SetBool("IsWalking", false);
        }
    }
    public void Turning(Vector3 direction)
    {
        if (m_Animator.GetBool("Attacking"))
        {
            m_Animator.SetBool("IsWalking", false);
            return;
        }
        if (direction.x > 0.01f || direction.x < -0.01 || direction.z > 0.01f || direction.z < -0.01)
        {
            direction = camObj.transform.TransformDirection(direction);
            direction = new Vector3(direction.x, 0, direction.z);
            Quaternion newDir = Quaternion.LookRotation(direction.normalized, transform.up);
            m_Rigidbody.rotation = Quaternion.Slerp(m_Rigidbody.rotation, newDir, Time.deltaTime * m_turnSmooth);
        }
        else
        {
            return;
        }
    }

    public void Turning(Vector3 direction, Vector3 up)
    {
        if (m_Animator.GetBool("Attacking"))
        {
            m_Animator.SetBool("IsWalking", false);
            return;
        }
        direction.y = 0.0f;
        m_Rigidbody.rotation = Quaternion.LookRotation(direction, up);
    }

    void ScaleCapsuleForCrouching(bool crouch)
    {
        if (m_IsGrounded && crouch)
        {
            if (m_Crouching) return;
            m_Capsule.height = m_Capsule.height / 2f;
            m_Capsule.center = m_Capsule.center / 2f;
            m_Crouching = true;
            m_CharacterObject.transform.localScale = (new Vector3(1, .2f, 1));
        }
        else
        {
            Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
            float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
            if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                m_Crouching = true;
                return;
            }
            m_Capsule.height = m_CapsuleHeight;
            m_Capsule.center = m_CapsuleCenter;
            m_CharacterObject.transform.localScale = (new Vector3(1, 1, 1));
            m_Crouching = false;
        }
    }

    void PreventStandingInLowHeadroom()
    {
        // prevent standing up in crouch-only zones
        if (!m_Crouching)
        {
            Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
            float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
            if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                m_Crouching = true;
            }
        }
    }

    void HandleAirborneMovement()
    {
        // apply extra gravity from multiplier:
        Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
        m_Rigidbody.AddForce(extraGravityForce);

        m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.01f;
    }

    void HandleGroundedMovement(bool crouch, bool jump)
    {
        // check whether conditions are right to allow a jump:
        if (jump && !crouch /*&& m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded")*/)
        {
            // jump!

            m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z);
            m_IsGrounded = false;
            //m_Animator.applyRootMotion = false;
            m_GroundCheckDistance = 0.1f;
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

