using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public enum ToolType { Default = 0, Axe = 1, Pick = 2, Sword = 3, Hands = 4 }
public class Tool : Item
{
    Animator m_Animator;
    CharacterStats stats;
    public List<Collider> m_HaveHit;
    public ToolType toolType = ToolType.Default;
    public int damage = 3;
    public float damageResetDelay = 0.5f;
    private bool canDealDamage = false;
    PhotonView pv;
    void Start()
    {
        m_HaveHit = new List<Collider>();
    }
    private void Update()
    {
        if (canDealDamage && !m_Animator.GetBool("Attacking"))
        {
            canDealDamage = false;
        }
    }

    public override void OnEquipped(GameObject character)
    {
        base.OnEquipped(character);
        m_Animator = character.GetComponentInChildren<Animator>();
        stats = character.GetComponent<CharacterStats>();
        pv = character.GetComponent<PhotonView>();
    }

    public override void OnUnequipped()
    {
        base.OnUnequipped();
    }

    void OnTriggerStay(Collider other)
    {
        if (isEquipped && m_Animator.GetBool("Attacking") && m_Animator.GetBool("CanHit"))
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
                SourceObject so = other.GetComponent<SourceObject>();
                BuildingMaterial bm = other.gameObject.GetComponent<BuildingMaterial>();
                if (bm != null)
                {
                    Debug.Log("### Here 1");
                    LevelManager.Instance.CallUpdateObjectsPRC(bm.id, 1 + stats.attack, ToolType.Hands, transform.position, m_OwnerObject.GetComponent<PhotonView>());
                }
                else if (hm != null)
                {
                    hm.TakeHit(1 + stats.attack, ToolType.Hands, transform.position, m_OwnerObject);
                }
                else if (so != null)
                {
                    LevelManager.Instance.CallUpdateObjectsPRC(so.id, 1 + stats.attack, ToolType.Hands, transform.position, m_OwnerObject.GetComponent<PhotonView>());
                }
                return;
            }
            catch (System.Exception ex)
            {
                //Debug.Log(ex);
            }
        }
    }
    public void Hit()
    {
        canDealDamage = true;
    }
}
