using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public enum ToolType { Default = 0, Axe = 1, Pick = 2, Sword = 3, Hands = 4, Arrow = 5, Beast = 6 }

public class ToolItem : Item
{
    public Animator m_Animator;
    public ToolType toolType = ToolType.Default;
    public int damage = 3;
    public float knockBackForce = 0;

    public float range;

    [Header("Stat Bonus")]
    public int dexBonus = 0;
    public int strBonus = 0;
    public int intBonus = 0;
    public int conBonus = 0;

    [Header("Equipped Positioning")]
    public Vector3 m_PositionModifier;
    public Vector3 m_RotationModifier;

    private int attack;
    private PhotonView pv;
    private AttackManager attackManager;

    void Start()
    {
        if (m_OwnerObject && m_OwnerObject.TryGetComponent<CharacterStats>(out var stats))
        {
            attack = stats.attack;
        }
        else if (m_OwnerObject && m_OwnerObject.TryGetComponent<StateController>(out var controller))
        {
            attack = controller.enemyStats.attackDamage;
        }
        if (m_OwnerObject)
        {
            // Reference to the centralized AttackManager
            attackManager = m_OwnerObject.GetComponent<AttackManager>();
        }
    }

    public override void OnEquipped(GameObject character)
    {
        base.OnEquipped(character);
        m_Animator = character.GetComponentInChildren<Animator>();
        pv = character.GetComponent<PhotonView>();
        attackManager = m_OwnerObject.GetComponent<AttackManager>();
        attackManager.ClearHits();
    }

    public override void OnUnequipped()
    {
        attackManager.ClearHits();
        attackManager = null;
        base.OnUnequipped();
    }

    // Triggered by animation events
    public void StartHitbox()
    {
        if (!pv.IsMine || attackManager == null) return;
        // Create and activate a hitbox via the AttackManager
        attackManager.ActivateHitbox(
            toolType,
            damage + attack,
            knockBackForce,
            range
        );

    }

    public void EndHit()
    {
        Debug.Log("### deactivating hit box in endHitBox in tool");
        if (!pv.IsMine || attackManager == null) return;
        Debug.Log("### deactivating hit box in endHitBox in tool 3");
        // Deactivate the hitbox via the AttackManager
        attackManager.DeactivateHitbox();
    }
}
