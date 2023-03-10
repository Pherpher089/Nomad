using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheseHands : MonoBehaviour
{
    public bool canHit = true;
    Animator m_Animator;
    [HideInInspector]
    public List<Collider> m_HaveHit;

    void Start()
    {
        m_Animator = GetComponentInParent<Animator>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (m_Animator.GetBool("Attacking"))
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
                hm.TakeDamage(1);
            }
            catch (System.Exception ex)
            {
                Debug.Log(ex);
            }

            try
            {
                SourceObject so = other.gameObject.GetComponent<SourceObject>();
                so.TakeDamage(1);
            }
            catch (System.Exception ex)
            {
                Debug.Log(ex);
            }
        }
    }
}
