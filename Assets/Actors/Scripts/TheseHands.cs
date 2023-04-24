using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheseHands : MonoBehaviour
{
    public bool canHit = true;
    Animator m_Animator;
    GameObject m_HansOwner;
    [HideInInspector]
    public List<Collider> m_HaveHit;

    private bool canDealDamage = false;

    void Start()
    {
        m_Animator = GetComponentInParent<Animator>();
        m_HansOwner = m_Animator.transform.parent.gameObject;
    }
    private void Update()
    {
        if (canDealDamage && !m_Animator.GetBool("Attacking"))
        {
            canDealDamage = false;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (m_Animator.GetBool("Attacking") && canDealDamage)
        {
            if (m_HaveHit.Contains(other))
            {
                return;
            }
            else
            {
                m_HaveHit.Add(other);
            }

            try
            {
                HealthManager hm = other.gameObject.GetComponent<HealthManager>();
                hm.TakeDamage(1, ToolType.Default, transform.position, m_HansOwner);
                return;
            }
            catch (System.Exception ex)
            {
                Debug.Log(ex);
            }
            try
            {
                SourceObject so = other.gameObject.GetComponent<SourceObject>();
                so.TakeDamage(1, ToolType.Default, transform.position);
                return;
            }
            catch (System.Exception ex)
            {
                Debug.Log(ex);
            }
        }
    }

    public void Hit()
    {
        canDealDamage = true;
    }
}
