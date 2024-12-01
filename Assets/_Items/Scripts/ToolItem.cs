using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public enum ToolType { Default = 0, Axe = 1, Pick = 2, Sword = 3, Hands = 4, Arrow = 5, Beast = 6 }
public class
ToolItem : Item
{
    public Animator m_Animator;
    int attack;
    public List<GameObject> m_HaveHit;
    public ToolType toolType = ToolType.Default;
    public int damage = 3;
    public float damageResetDelay = 0.5f;
    public float knockBackForce = 0;
    [Header("Stat Bonus")]
    public int dexBonus = 0;
    public int strBonus = 0;
    public int intBonus = 0;
    public int conBonus = 0;
    [Header("Equipped Positioning")]
    public Vector3 m_PositionModifier;
    public Vector3 m_RotationModifier;
    [HideInInspector]
    public bool canDealDamage = false;
    PhotonView pv;
    void Start()
    {
        m_HaveHit = new List<GameObject>();
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
            if (m_HaveHit.Contains(other.gameObject))
            {
                return;
            }
            else
            {
                m_HaveHit.Add(other.gameObject);
            }
            try
            {
                HealthManager hm = other.gameObject.GetComponent<HealthManager>();
                if (hm == null)
                {
                    hm = other.GetComponentInParent<HealthManager>();
                    if (hm != null && m_HaveHit.Contains(hm.gameObject))
                    {
                        return;
                    }
                }
                SourceObject so = other.GetComponent<SourceObject>();
                if (so == null)
                {
                    so = other.GetComponentInParent<SourceObject>();
                    if (so != null && m_HaveHit.Contains(so.gameObject))
                    {
                        return;
                    }
                }
                BuildingMaterial bm = other.gameObject.GetComponent<BuildingMaterial>();
                if (bm == null)
                {
                    bm = other.GetComponentInParent<BuildingMaterial>();
                    if (bm != null && m_HaveHit.Contains(bm.gameObject))
                    {
                        return;
                    }
                }
                if (bm != null)
                {
                    if (!m_HaveHit.Contains(bm.gameObject))
                    {
                        m_HaveHit.Add(bm.gameObject);
                    }
                    LevelManager.Instance.CallUpdateObjectsPRC(bm.id, bm.spawnId, damage + attack, toolType, transform.position, m_OwnerObject.GetComponent<PhotonView>());
                }
                else if (so != null)
                {
                    if (!m_HaveHit.Contains(so.gameObject))
                    {
                        m_HaveHit.Add(so.gameObject);
                    }
                    LevelManager.Instance.CallUpdateObjectsPRC(so.id, "", damage + attack, toolType, transform.position, m_OwnerObject.GetComponent<PhotonView>());
                }
                else if (hm != null)
                {
                    if (!m_HaveHit.Contains(hm.gameObject))
                    {
                        m_HaveHit.Add(hm.gameObject);
                    }
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
