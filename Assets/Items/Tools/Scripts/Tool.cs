using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool : Item
{
    Animator m_Animator;
    public List<Collider> m_HaveHit;

    void Start()
    {
        m_HaveHit = new List<Collider>();
    }

    public override void OnEquipped(GameObject character)
    {
        base.OnEquipped(character);
        m_Animator = character.GetComponentInChildren<Animator>();
    }

    public override void OnUnequipt()
    {
        base.OnUnequipt();
    }

    void OnTriggerEnter(Collider other)
    {
        if (isEquiped && m_Animator.GetBool("Attacking"))
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
