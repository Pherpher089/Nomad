using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public enum ToolType { Default = 0, Axe = 1, Pick = 2, Sword = 3, Hands = 4, Arrow = 5, Beast = 6 }
public class ToolItem : Item
{
    public Animator m_Animator;
    int attack;
    public List<Collider> m_HaveHit;
    public ToolType toolType = ToolType.Default;
    public int damage = 3;
    public float damageResetDelay = 0.5f;
    public float knockBackForce = 0;
    [HideInInspector]
    public bool canDealDamage = false;
    PhotonView pv;
    void Start()
    {
        m_HaveHit = new List<Collider>();
        if (m_OwnerObject && m_OwnerObject.TryGetComponent<CharacterStats>(out var stats))
        {
            attack = stats.attack;
        }
        else if (m_OwnerObject && m_OwnerObject.TryGetComponent<StateController>(out var controller))
        {
            attack = controller.enemyStats.attackDamage;
        }
    }
    private void Update()
    {
        if (m_Animator == null || canDealDamage && !m_Animator.GetBool("Attacking"))
        {
            canDealDamage = false;
        }
    }

    public override void OnEquipped(GameObject character)
    {
        base.OnEquipped(character);
        m_Animator = character.GetComponentInChildren<Animator>();
        pv = character.GetComponent<PhotonView>();
    }

    public override void OnUnequipped()
    {
        base.OnUnequipped();
    }

    void OnTriggerStay(Collider other)
    {

        if (m_OwnerObject == null || !m_OwnerObject.GetComponent<PhotonView>().IsMine)
        {
            return;
        }
        if (other.gameObject == m_OwnerObject) return;

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
                SourceObject so = other.gameObject.GetComponent<SourceObject>();
                BuildingMaterial bm = other.gameObject.GetComponent<BuildingMaterial>();
                if (bm != null)
                {
                    LevelManager.Instance.CallUpdateObjectsPRC(bm.spawnId, damage + attack, toolType, transform.position, m_OwnerObject.GetComponent<PhotonView>());
                }
                else if (so != null)
                {
                    LevelManager.Instance.CallUpdateObjectsPRC(so.id, damage + attack, toolType, transform.position, m_OwnerObject.GetComponent<PhotonView>());
                }
                else if (hm != null)
                {
                    hm.Hit(damage + attack, toolType, transform.position, m_OwnerObject, knockBackForce);
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
