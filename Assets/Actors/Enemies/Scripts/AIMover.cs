using UnityEngine;
using Pathfinding;
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(StateController))]
/// <summary>
/// An interface to the AI actors behaviors. Movement, attacking and other functionality can be accessed here. There are also controls to handle navmesh link behavior when actor is traversing between navmeshs
/// </summary>
public class AIMover : MonoBehaviour
{
    /// <summary>
    /// The distance from the bottom of the player downward to check for the 
    /// ground. This is to determine the behavior of the actor. i.e. Falling, 
    /// Jumping, Walking
    /// </summary>
    public float m_GroundCheckDistance = 0.2f;
    /// <summary>
    /// The interpolation modifier for actor rotation on the y axis. 0 will 
    /// prevent rotation.
    /// </summary>
    public Rigidbody m_Rigidbody;
    AIPath m_AiPath;
    GameObject m_CameraObject;
    Animator m_Animator;
    StateController m_Controller;
    Vector3 m_GroundNormal;
    public float m_turnSmooth = 5;
    bool m_IsGrounded;

    // Start is called before the first frame update
    void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_AiPath = GetComponent<AIPath>();
        m_CameraObject = GameObject.FindWithTag("MainCamera");
        m_Animator = transform.GetChild(0).GetComponent<Animator>();
        m_Controller = GetComponent<StateController>();
    }

    // Update is called once per frame
    void Update()
    {
        //m_Rigidbody.isKinematic = true;
        //Check to see if any auto navmesh links need to happen
        if (m_AiPath.hasPath == false && m_Controller.target != null)
        {   //This drives the ai across the navmesh joint
            // m_Rigidbody.isKinematic = false;
            Move(m_Controller.target.transform.position - transform.position);
        }
        else if (m_AiPath.hasPath)
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
            m_Rigidbody.velocity = Vector3.zero;
            m_AiPath.canMove = false;
        }

        m_AiPath.canMove = true;

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
    /// <summary>
    /// Adjusts the actors rotation based on a provided direction.
    /// </summary>
    /// <param name="direction">The direction that the actor is turning to</param>
    public void Turning(Vector3 direction)
    {
        Debug.Log("### rotation");
        if (m_Animator.GetBool("Attacking"))
        {
            m_Animator.SetBool("IsWalking", false);
            return;
        }
        direction.y = 0.0f;
        if (direction != Vector3.zero)
        {
            Debug.Log("### Here");
            transform.rotation = Quaternion.LookRotation(direction, transform.up);
        }
    }
    /// <summary>
    /// Moves the actor in the direction provided.
    /// </summary>
    /// <param name="move">The direction to move the actor. The speed is pulled from the AiPath component.</param>
    public void Move(Vector3 move)
    {

        if (m_Animator.GetBool("Attacking") || m_Animator.GetBool("TakeHit"))
        {
            m_Animator.SetBool("IsWalking", false);

            return;
        }
        CheckGroundStatus();
        UpdateAnimatorMove(move);
        if (move.magnitude > 1f) move.Normalize();

        move = Vector3.ProjectOnPlane(move, m_GroundNormal);
        float m_zMovement = move.z * m_AiPath.maxSpeed;
        float m_xMovement = move.x * m_AiPath.maxSpeed;
        Vector3 finalMove = new Vector3(m_xMovement, m_Rigidbody.velocity.y, m_zMovement);
        m_Rigidbody.velocity = finalMove;
    }
    /// <summary>
    /// Checks if the character is on the ground based on the Ground Check Distance value.
    /// </summary>
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

    /// <summary>
    /// Performs the attack action of the actor.
    /// </summary>
    /// <param name="primary">Primary attack value. Will be performed if both secondary and primary are true.</param>
    /// <param name="secondary">Secondary attack value.</param>
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
}
