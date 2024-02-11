using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(ActorEquipment))]
//[RequireComponent(typeof(Animator))]
public class ThirdPersonCharacter : MonoBehaviour
{
    [SerializeField] float m_turnSmooth = 30;
    [SerializeField] float m_JumpPower = 12f;
    [SerializeField] float m_RollPower = 0.1f;
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
    public bool m_IsGrounded;
    float m_OrigGroundCheckDistance;
    const float k_Half = 0.5f;
    float m_xMovement;
    float m_zMovement;
    Vector3 m_RollDirection = new Vector3(0, 0, 0);
    Vector3 m_GroundNormal;
    float m_CapsuleHeight;
    Vector3 m_CapsuleCenter;
    bool m_Crouching;
    [HideInInspector] public Animator m_Animator;
    //EquipmentVariables
    ActorEquipment charEquipment;
    public float m_GroundNormalCheckDistance = 0.5f;
    [SerializeField] float m_SlopeAngleLimit = 45f;

    int blockLayerIndex = 1;
    int eatLayerIndex = 2;

    void Awake()
    {
        m_Animator = transform.GetChild(0).GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Capsule = GetComponent<CapsuleCollider>();
        m_CharacterObject = transform.GetChild(0).gameObject;
        m_CapsuleHeight = m_Capsule.height;
        m_CapsuleCenter = m_Capsule.center;
        charEquipment = GetComponent<ActorEquipment>();
        camObj = GameObject.FindWithTag("MainCamera");
        camFoward = camObj.transform.parent.forward.normalized;
        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        m_OrigGroundCheckDistance = m_GroundCheckDistance;

    }
    void AttackAnimatorUpdate(Vector3 move)
    {
        if (m_Animator.GetBool("Attacking"))
        {
            if (m_Animator.GetBool("AttackMove"))
            {
                Vector3 attackMove = Vector3.zero;
                if (move != Vector3.zero && Vector3.Angle(move, transform.forward) < 90)
                {
                    attackMove = transform.forward;
                }
                //m_Rigidbody.MovePosition(transform.position + attackMove * m_MoveSpeedMultiplier * Time.deltaTime * 1.5f);
                transform.position += 1.5f * m_MoveSpeedMultiplier * Time.deltaTime * attackMove;

            }
            else
            {
                m_Rigidbody.velocity = Vector3.zero;

            }
        }
    }

    public void UpdateAnimatorHit(Vector3 hitDir)
    {
        if (m_Animator.GetBool("TakeHit"))
        {
            hitDir.y = 0;
            // transform.position += 3f * m_MoveSpeedMultiplier * Time.deltaTime * hitDir;
            transform.Translate(1.5f * Time.deltaTime * hitDir, Space.World);
        }
    }

    public void Attack(bool primary, bool secondary)
    {
        if (m_Animator.GetBool("Jumping") || m_Animator.GetBool("Sprinting") || m_Animator.GetLayerWeight(1) > 0)
        {
            return;
        }

        m_Animator.ResetTrigger("LeftAttack");
        m_Animator.ResetTrigger("RightAttack");

        if (primary)
        {
            m_Animator.SetTrigger("LeftAttack");
            m_Animator.SetBool("Attacking", true);
            m_Animator.SetBool("IsWalking", false);
            m_Animator.SetBool("Crouched", false);
            m_Crouching = false;

        }
        else if (secondary)
        {
            m_Animator.SetTrigger("RightAttack");
            m_Animator.SetBool("Attacking", true);
            m_Animator.SetBool("IsWalking", false);
            m_Animator.SetBool("Crouched", false);
            m_Crouching = false;
        }

    }

    private void UpdateBlockBlendWeight(bool block)
    {
        float targetWeight = block ? 1 : 0.0f;
        // float currentWeight = m_Animator.GetLayerWeight(blockLayerIndex);
        float newWeight = targetWeight;

        m_Animator.SetLayerWeight(blockLayerIndex, newWeight);
    }

    public void Eat()
    {
        if (!m_Animator.GetBool("Eating"))
        {
            m_Animator.SetLayerWeight(2, 1);
            m_Animator.SetBool("Eating", true);
        }
    }

    public void Move(Vector3 move, bool crouch, bool jump, bool sprint, bool blocking, bool rolling)
    {
        // convert the world relative moveInput vector into a local-relative
        // turn amount and forward amount required to head in the desired
        // direction.
        if (move.magnitude > 1f) move.Normalize();
        move = camObj.transform.TransformDirection(new Vector3(move.x, move.y, move.z * 1.5f));
        CheckGroundStatus();

        // Update the animator with any attack movement
        if (m_Animator.GetBool("Attacking") || m_Animator.GetBool("TakeHit"))
        {
            AttackAnimatorUpdate(new Vector3(move.x, 0, move.z));
            m_Animator.SetBool("IsWalking", false);
            m_Animator.SetBool("Sprinting", false);
            m_Animator.SetBool("Crouched", false);
            return;
        }

        // Project the move vector on the ground normal and normalize it
        //move = Vector3.ProjectOnPlane(move, m_GroundNormal).normalized;
        float crouchModifier = m_Crouching ? 0.5f : 1;
        float sprintModifier = sprint ? 2f : 1;
        float blockModifier = blocking ? 0.3f : 1;
        m_zMovement = move.z * m_MoveSpeedMultiplier * crouchModifier * sprintModifier * blockModifier;
        m_xMovement = move.x * m_MoveSpeedMultiplier * crouchModifier * sprintModifier * blockModifier;

        // control and velocity handling is different when grounded and airborne:
        if (m_IsGrounded)
        {
            Vector3 rollMoveDir = new Vector3(m_xMovement, 0, m_zMovement);
            HandleGroundedMovement(crouch, jump, rolling, rollMoveDir);
        }
        else
        {
            HandleAirborneMovement();
        }

        ScaleCapsuleForCrouching(crouch);
        PreventStandingInLowHeadroom(crouch);

        // send input and other state parameters to the animator
        UpdateAnimatorMove(move, sprint);
        UpdateBlockBlendWeight(blocking);
        //if (!m_Animator.GetBool("Jumping"))
        // {
        //     m_Rigidbody.velocity = new Vector3(m_xMovement, m_Rigidbody.velocity.y, m_zMovement);
        // }
        if (!m_Animator.GetBool("Rolling"))
        {
            m_Rigidbody.velocity = new Vector3(m_xMovement, m_Rigidbody.velocity.y, m_zMovement);
        }
        else
        {
            m_Rigidbody.velocity = m_RollDirection * m_RollPower;

        }
    }
    void UpdateAnimatorMove(Vector3 move, bool sprint)
    {
        float blockWeight = m_Animator.GetLayerWeight(blockLayerIndex);

        if (m_Animator.GetBool("Attacking") || m_Animator.GetBool("TakeHit"))
        {
            m_Animator.SetFloat("Horizontal", 0);
            m_Animator.SetFloat("Vertical", 0);
            m_Animator.SetBool("IsWalking", false);
            m_Rigidbody.velocity = Vector3.zero;

        }
        float threshold = 0.3f;
        if (move.x > threshold || move.x < -threshold || move.z > threshold || move.z < -threshold)
        {
            m_Animator.SetBool("IsWalking", true);
            if (sprint && blockWeight == 0f)
            {
                m_Animator.SetBool("Sprinting", true);
                m_Animator.SetBool("Crouched", false);
                m_Crouching = false;

            }
            else
            {
                m_Animator.SetBool("Sprinting", false);
            }
            Vector3 localVelocity = transform.InverseTransformDirection(m_Rigidbody.velocity);
            m_Animator.SetFloat("Horizontal", localVelocity.x);
            m_Animator.SetFloat("Vertical", localVelocity.z);
        }
        else
        {
            m_Animator.SetFloat("Horizontal", 0);
            m_Animator.SetFloat("Vertical", 0);
            m_Animator.SetBool("IsWalking", false);
            m_Animator.SetBool("Sprinting", false);

        }
        if (!sprint)
        {
            m_Animator.SetBool("Crouched", m_Crouching);
        }
    }
    public void Turning(Vector3 direction)
    {
        if (m_Animator.GetBool("TakeHit") || m_Animator.GetBool("Jumping"))
        {
            m_Animator.SetBool("IsWalking", false);
            return;
        }
        if (m_Animator.GetBool("Attacking") && !m_Animator.GetBool("AttackMove"))
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
        if (m_Animator.GetBool("TakeHit") || m_Animator.GetBool("Jumping"))
        {
            m_Animator.SetBool("IsWalking", false);
            return;
        }
        if (m_Animator.GetBool("Attacking") && !m_Animator.GetBool("AttackMove"))
        {
            m_Animator.SetBool("IsWalking", false);
            return;
        }
        direction.y = 0.0f;
        if (direction != Vector3.zero)
        {
            Quaternion newDir = Quaternion.LookRotation(direction, up);
            m_Rigidbody.rotation = Quaternion.Slerp(m_Rigidbody.rotation, newDir, Time.deltaTime * m_turnSmooth);
        }
    }

    void ScaleCapsuleForCrouching(bool crouch)
    {
        if (m_IsGrounded && crouch && !m_Crouching)
        {
            if (m_Crouching) return;
            m_Capsule.height = m_Capsule.height / 3f;
            m_Capsule.center = m_Capsule.center / 3f;
            m_Crouching = true;
        }
        else if (m_IsGrounded && !crouch && m_Crouching)
        {
            Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
            float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
            Debug.DrawRay(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up, Color.blue);
            if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                m_Crouching = true;
                return;
            }
            m_Capsule.height = m_CapsuleHeight;
            m_Capsule.center = m_CapsuleCenter;
            m_Crouching = false;
        }
    }

    void PreventStandingInLowHeadroom(bool crouch)
    {
        // prevent standing up in crouch-only zones
        if (m_Crouching && !crouch)
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
        m_Crouching = false;
        m_Animator.SetBool("Crouched", false);
        m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.02f;
    }

    void HandleGroundedMovement(bool crouch, bool jump, bool rolling, Vector3 move)
    {
        // check whether conditions are right to allow a jump:
        float blockWeight = m_Animator.GetLayerWeight(blockLayerIndex);
        if (rolling && !m_Animator.GetBool("Rolling") && !crouch && blockWeight == 0f)
        {

            float rollPower = 20;
            m_IsGrounded = false;
            m_RollDirection = move;// Or move?
            m_Animator.SetTrigger("Roll");
            m_Animator.SetBool("Rolling", true);
            m_Animator.SetBool("Jumping", false);
            m_Crouching = false;
            m_Animator.SetBool("Crouched", false);
        }
        else if (jump && !m_Animator.GetBool("Rolling") && !crouch && blockWeight == 0f/*&& m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded")*/)
        {
            // jump!
            m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z);
            m_IsGrounded = false;
            m_Animator.SetTrigger("Jump");
            m_Animator.SetBool("Jumping", true);
            m_GroundCheckDistance = 0.2f;
            m_Crouching = false;
            m_Animator.SetBool("Crouched", false);
        }
    }

    void CheckGroundStatus()
    {
        RaycastHit hitInfo;
        // helper to visualise the ground check ray in the scene view
        Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance), Color.red);

        // 0.1f is a small offset to start the ray from inside the character
        // it is also good to note that the transform position in the sample assets is at the base of the character
        if (Physics.Raycast(transform.position + (Vector3.up * 0.2f), Vector3.down, out hitInfo, m_GroundCheckDistance))
        {
            m_GroundNormal = hitInfo.normal;
            m_IsGrounded = true;
            m_Animator.SetBool("Jumping", false);

        }
        else
        {
            m_Animator.SetBool("Jumping", true);
            m_Animator.SetBool("Crouched", false);
            m_Animator.SetBool("Sprinting", false);
            m_IsGrounded = false;
            m_GroundNormal = Vector3.up;
        }
    }
}

