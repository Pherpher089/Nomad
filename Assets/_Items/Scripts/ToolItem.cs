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

    public float range = 2;

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

        // Reference to the centralized AttackManager
        attackManager = FindObjectOfType<AttackManager>();
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

    // Triggered by animation events
    public void StartHitbox()
    {
        if (!pv.IsMine || attackManager == null) return;

        // Create and activate a hitbox via the AttackManager
        attackManager.ActivateHitbox(
            m_OwnerObject,
            transform.position + transform.up,
            transform.forward,
            toolType,
            damage + attack,
            knockBackForce,
            range
        );
    }

    public void EndHitbox()
    {
        if (!pv.IsMine || attackManager == null) return;

        // Deactivate the hitbox via the AttackManager
        attackManager.DeactivateHitbox();
    }
}
