using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class AttackManager : MonoBehaviour
{
    private List<GameObject> m_HaveHit = new List<GameObject>();
    private GameObject owner;
    private Vector3 hitboxPosition;
    private Vector3 hitboxDirection;
    private ToolType toolType;
    private int damage;
    private float hitRange;
    private float knockBackForce;

    private bool hitboxActive = false;
    public void ResetHitbox()
    {
        m_HaveHit.Clear();
    }
    public void ActivateHitbox(GameObject ownerObject, Vector3 position, Vector3 direction, ToolType type, int damageValue, float knockback, float range)
    {
        owner = ownerObject;
        hitboxPosition = position;
        hitboxDirection = direction;
        toolType = type;
        damage = damageValue;
        knockBackForce = knockback;
        hitRange = range;
        hitboxActive = true;

        m_HaveHit.Clear();
    }

    public void DeactivateHitbox()
    {
        hitboxActive = false;
    }

    void Update()
    {
        if (!hitboxActive) return;

        // Perform hit detection (e.g., Physics.OverlapBox or custom logic)
        Collider[] hits = Physics.OverlapBox(new(hitboxPosition.x, hitboxPosition.y, hitboxPosition.z + (hitRange / 2)), new Vector3(2, 6, hitRange), Quaternion.LookRotation(hitboxDirection));

        foreach (Collider hit in hits)
        {
            if (m_HaveHit.Contains(hit.gameObject)) continue;

            m_HaveHit.Add(hit.gameObject);

            // Handle damage logic
            if (hit.TryGetComponent<HealthManager>(out var hm))
            {
                hm.Hit(damage, toolType, hitboxPosition, owner, knockBackForce);
            }
            else if (hit.TryGetComponent<SourceObject>(out var so))
            {
                LevelManager.Instance.CallUpdateObjectsPRC(so.id, "", damage, toolType, hitboxPosition, owner.GetComponent<PhotonView>());
            }
            else if (hit.TryGetComponent<BuildingMaterial>(out var bm))
            {
                LevelManager.Instance.CallUpdateObjectsPRC(bm.id, bm.spawnId, damage, toolType, hitboxPosition, owner.GetComponent<PhotonView>());
            }
        }
    }
}
