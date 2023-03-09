using UnityEngine;
using Pathfinding;
//Used to manually move an AI between Nav Meshes
public class AIMover : MonoBehaviour
{
    float m_GroundCheckDistance = 0.2f;
    Rigidbody rigidbody;
    AIPath aiPath;
    GameObject camObj;
    Vector3 m_GroundNormal;
    Animator m_Animator;
    StateController controller;
    bool m_IsGrounded;
    public float m_turnSmooth = 5;

    // Start is called before the first frame update
    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        aiPath = GetComponent<AIPath>();
        camObj = GameObject.FindWithTag("MainCamera");
        m_Animator = transform.GetChild(0).GetComponent<Animator>();
        controller = GetComponent<StateController>();
    }

    // Update is called once per frame
    void Update()
    {
        //Check to see if any auto navmesh links need to happen
        if (aiPath.hasPath == false && controller.target != null)
        {   //This drives the ai across the navmesh joint
            GetComponent<AIMover>().Move(controller.transform.position - transform.position);
        }
        else if (aiPath.hasPath)
        {
            UpdateAnimatorMove(aiPath.velocity);
        }
    }

    public void SetDestination(Vector3 destination)
    {
        aiPath.destination = destination;
        m_Animator.SetBool("IsWalking", true);
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
            Vector3 localVelocity = transform.InverseTransformDirection(rigidbody.velocity.normalized);
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
            rigidbody.rotation = Quaternion.Slerp(rigidbody.rotation, newDir, Time.deltaTime * m_turnSmooth);
        }
        else
        {
            return;
        }
    }

    public void Move(Vector3 move)
    {
        if (m_Animator.GetBool("Attacking"))
        {
            m_Animator.SetBool("IsWalking", false);
            return;
        }

        CheckGroundStatus();
        // convert the world relative moveInput vector into a local-relative
        // turn amount and forward amount required to head in the desired
        // direction.
        if (move.magnitude > 1f) move.Normalize();
        //move = camObj.transform.TransformDirection(move);

        move = Vector3.ProjectOnPlane(move, m_GroundNormal);
        //m_TurnAmount = Mathf.Atan2(move.x, move.z);
        float m_zMovement = move.z * aiPath.maxSpeed;
        float m_xMovement = move.x * aiPath.maxSpeed;
        // control and velocity handling is different when grounded and airborne:
        // send input and other state parameters to the animator
        UpdateAnimatorMove(move);
        rigidbody.velocity = new Vector3(m_xMovement, rigidbody.velocity.y, m_zMovement);
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
                rigidbody.velocity = Vector3.zero;
                m_Animator.SetTrigger("LeftAttack");
                m_Animator.SetBool("Attacking", true);
                m_Animator.SetBool("IsWalking", false);
            }
            else if (secondary)
            {
                rigidbody.velocity = Vector3.zero;
                m_Animator.SetTrigger("RightAttack");
                m_Animator.SetBool("Attacking", true);
                m_Animator.SetBool("IsWalking", false);
            }
        }
    }
}
