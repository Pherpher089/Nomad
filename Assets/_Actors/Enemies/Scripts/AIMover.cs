using UnityEngine;
using Pathfinding;
[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(StateController))]
/// <summary>
/// An interface to the AI actors behaviors. Movement, attacking and other functionality can be accessed here. There are also controls to handle navmesh link behavior when actor is traversing between navmeshs
/// </summary>
public class AIMover : MonoBehaviour
{
    public float m_GroundCheckDistance = 0.2f;
    AIPath m_AiPath;
    GameObject m_CameraObject;
    Animator m_Animator;
    StateController m_Controller;
    Vector3 m_GroundNormal;
    public float m_turnSmooth = 5;
    bool m_IsGrounded;
    EnemyManager m_EnemyManager;

    // Start is called before the first frame update
    void Awake()
    {
        m_EnemyManager = GetComponent<EnemyManager>();
        m_AiPath = GetComponent<AIPath>();
        m_CameraObject = GameObject.FindWithTag("MainCamera");
        m_Animator = transform.GetChild(0).GetComponent<Animator>();
        m_Controller = GetComponent<StateController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Check to see if any auto navmesh links need to happen
        if (m_AiPath.hasPath == false && m_Controller.target != null && !m_EnemyManager.isDead)
        {   //This drives the ai across the navmesh joint
            //Move(m_Controller.target.transform.position - transform.position);
        }
        else if (m_AiPath.hasPath && m_Controller.target != null && !m_EnemyManager.isDead)
        {
            UpdateAnimatorMove(m_AiPath.velocity);
        }
    }
    /// <summary>
    /// Sets the actor on a path set to this destination.
    /// </summary>
    /// <param  name="destination">The position on world space which the actor should travel to. </param>
    public void SetDestination(Vector3 destination)
    {
        m_AiPath.destination = destination;
        m_Animator.SetBool("IsWalking", true);
    }
    /// <summary>
    /// This method updates the actor animator based on it's state
    /// </summary>
    /// <param name="move">The normalized local move value. Works similar to joystick movement.</param>
    void UpdateAnimatorMove(Vector3 move)
    {
        if (move.magnitude > 1)
        {
            move = move.normalized;
        }
        if (m_Animator.GetBool("Attacking") || m_Animator.GetBool("TakeHit"))
        {
            m_Animator.SetFloat("Horizontal", 0);
            m_Animator.SetFloat("Vertical", 0);
            m_Animator.SetBool("IsWalking", false);
            m_AiPath.canMove = false;
        }
        else
        {
            m_AiPath.enabled = true;
            m_AiPath.canMove = true;
        }


        float threshold = 0.3f;
        if (move.x > threshold || move.x < -threshold || move.z > threshold || move.z < -threshold)
        {
            m_Animator.SetBool("IsWalking", true);
            Vector3 localVelocity = transform.InverseTransformDirection(m_AiPath.velocity.normalized);
            m_Animator.SetFloat("Horizontal", localVelocity.x);
            m_Animator.SetFloat("Vertical", localVelocity.z);

            Turning(move);
        }
        else
        {
            m_Animator.SetFloat("Horizontal", 0);
            m_Animator.SetFloat("Vertical", 0);
            m_Animator.SetBool("IsWalking", false);
        }

    }

    public void UpdateAnimatorHit(Vector3 hitDir)
    {
        if (m_Animator.GetBool("TakeHit"))
        {
            m_AiPath.canMove = false;
            Turning(transform.forward);
            m_AiPath.transform.position += (m_AiPath.maxSpeed) * Time.deltaTime * hitDir.normalized;
        }
    }

    public void Turning(Vector3 direction)
    {
        if (m_Animator.GetBool("Attacking"))
        {
            m_Animator.SetBool("IsWalking", false);
            return;
        }
        direction.y = 0.0f;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction, transform.up);
        }
    }

    public void Move(Vector3 move)
    {
        if (m_Animator != null)
        {
            if (m_Animator.GetBool("Attacking") || m_Animator.GetBool("TakeHit"))
            {
                m_Animator.SetBool("IsWalking", false);

                return;
            }
        }
        CheckGroundStatus();
        if (m_Animator != null)
        {
            UpdateAnimatorMove(move);
        }
        if (move.magnitude > 1f) move.Normalize();
        move = Vector3.ProjectOnPlane(move, m_GroundNormal);
        float m_zMovement = move.z * m_AiPath.maxSpeed * Time.deltaTime;
        float m_xMovement = move.x * m_AiPath.maxSpeed * Time.deltaTime;
        Vector3 finalMove = new Vector3(m_xMovement, 0, m_zMovement);
        transform.position += finalMove;
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
        }
        else
        {
            m_IsGrounded = false;
            m_GroundNormal = Vector3.up;
        }
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
                //m_Rigidbody.velocity = Vector3.zero;
                m_Animator.SetTrigger("LeftAttack");
                m_Animator.SetBool("Attacking", true);
                m_Animator.SetBool("IsWalking", false);
            }
            else if (secondary)
            {
                //m_Rigidbody.velocity = Vector3.zero;
                m_Animator.SetTrigger("RightAttack");
                m_Animator.SetBool("Attacking", true);
                m_Animator.SetBool("IsWalking", false);
            }
        }
    }
}
