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

    public override void OnEquipt(GameObject character)
    {
        base.OnEquipt(character);
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
            Debug.Log("$$$ HIT ");
            if (m_HaveHit.Contains(other))
            {
                return;

            }
            else
            {
                m_HaveHit.Add(other);
            }
            Debug.Log("$$$ HIT 2");

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
                Debug.Log("$$$ HIT " + so.gameObject.name);
                so.TakeDamage(1);
            }
            catch (System.Exception ex)
            {
                Debug.Log(ex);
            }
        }
    }

}
