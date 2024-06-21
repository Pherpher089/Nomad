using System;
using System.Collections;
using System.Globalization;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(StateController))]
/// <summary>
/// An interface to the AI actors behaviors. Movement, attacking and other functionality can be accessed here. There are also controls to handle navmesh link behavior when actor is traversing between navmeshs
/// </summary>
public class AIMover : MonoBehaviour
{
    public float m_GroundCheckDistance = 0.2f;
    NavMeshAgent m_NavMeshAgent;
    GameObject m_CameraObject;
    Animator m_Animator;
    StateController m_Controller;
    Vector3 m_GroundNormal;
    public float m_turnSmooth = 5;
    bool m_IsGrounded;
    EnemyManager m_EnemyManager;
    PhotonView pv;
    // Start is called before the first frame update
    void Awake()
    {
        pv = GetComponent<PhotonView>();
        m_EnemyManager = GetComponent<EnemyManager>();
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
        m_CameraObject = GameObject.FindWithTag("MainCamera");
        m_Animator = transform.GetChild(0).GetComponent<Animator>();
        if (!PhotonNetwork.IsMasterClient) return;
        m_Controller = GetComponent<StateController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        //Check to see if any auto navmesh links need to happen
        if (!m_EnemyManager.isDead)
        {
            UpdateAnimatorMove(m_NavMeshAgent.velocity);
        }
    }
    /// <summary>
    /// Sets the actor on a path set to this destination.
    /// </summary>
    /// <param  name="destination">The position on world space which the actor should travel to. </param>
    public void SetDestination(Vector3 destination)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        m_NavMeshAgent.SetDestination(destination);
        m_Animator.SetBool("IsWalking", true);
    }
    /// <summary>
    /// This method updates the actor animator based on it's state
    /// </summary>
    /// <param name="move">The normalized local move value. Works similar to joystick movement.</param>
    void UpdateAnimatorMove(Vector3 move)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (move.magnitude > 1)
        {
            move = move.normalized;
        }
        if (m_Animator.GetBool("Attacking") || m_Animator.GetBool("TakeHit"))
        {
            m_Animator.SetFloat("Horizontal", 0);
            m_Animator.SetFloat("Vertical", 0);
            m_Animator.SetBool("IsWalking", false);
            if (m_NavMeshAgent.isOnNavMesh) m_NavMeshAgent.isStopped = true;
        }
        else
        {
            m_NavMeshAgent.enabled = true;
            if (m_NavMeshAgent.isOnNavMesh) m_NavMeshAgent.isStopped = false;
        }


        float threshold = 0.3f;
        if (move.x > threshold || move.x < -threshold || move.z > threshold || move.z < -threshold)
        {
            m_Animator.SetBool("IsWalking", true);
            Vector3 localVelocity = transform.InverseTransformDirection(m_NavMeshAgent.velocity.normalized);
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
    public void CallUpdateAnimatorHit(Vector3 hitDir, float knockBackForce)
    {
        pv.RPC("UpdateAnimatorHit", RpcTarget.All, hitDir.x, hitDir.y, hitDir.z, knockBackForce);
    }

    [PunRPC]
    public void UpdateAnimatorHit(float x, float y, float z, float knockBackForce)
    {
        Vector3 hitDir = new Vector3(x, y, z);
        if (PhotonNetwork.IsMasterClient) StartCoroutine(ApplyKnockback(hitDir, knockBackForce));
        if (m_Animator.GetBool("TakeHit"))
        {
            m_NavMeshAgent.isStopped = true;
            Turning(transform.forward);
        }
    }
    private IEnumerator ApplyKnockback(Vector3 direction, float force)
    {
        float elapsedTime = 0f;

        while (elapsedTime < 0.06)
        {
            if (m_NavMeshAgent.isOnNavMesh) m_NavMeshAgent.Move(force * Time.deltaTime * direction.normalized);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

    }

    public void Turning(Vector3 direction)
    {
        if (!PhotonNetwork.IsMasterClient) return;
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

    public void CallAttack_RPC(bool primary, bool secondary, bool ranged = false)
    {
        pv.RPC("Attack", RpcTarget.All, primary, secondary, ranged, false);
    }

    [PunRPC]
    public void Attack(bool primary, bool secondary, bool ranged = false, bool dummy = false)
    {
        if (!primary && !secondary)
        {
            return;
        }
        m_Animator.SetBool("TakeHit", false);
        if (!m_Animator.GetBool("Attacking"))
        {
            m_Animator.ResetTrigger("LeftAttack");
            m_Animator.ResetTrigger("RightAttack");

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

    public void AttackOld(bool primary, bool secondary)
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
