using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class RockWallParticleController : MonoBehaviour
{
    GameObject m_OwnerObject;
    List<GameObject> m_HaveHit;

    public void Initialize(GameObject owner)
    {

        m_OwnerObject = owner;

    }
    void OnParticleCollision(GameObject other)
    {
        if (other.tag == "Enemy")
        {
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
                CharacterStats stats = m_OwnerObject.GetComponent<CharacterStats>();
                if (bm != null)
                {
                    if (!m_HaveHit.Contains(bm.gameObject))
                    {
                        m_HaveHit.Add(bm.gameObject);
                    }
                    LevelManager.Instance.CallUpdateObjectsPRC(bm.id, bm.spawnId, 10 + stats.magicAttack, ToolType.Arrow, transform.position, m_OwnerObject.GetComponent<PhotonView>());
                }
                else if (so != null)
                {
                    if (!m_HaveHit.Contains(so.gameObject))
                    {
                        m_HaveHit.Add(so.gameObject);
                    }
                    LevelManager.Instance.CallUpdateObjectsPRC(so.id, "", 10 + stats.magicAttack, ToolType.Arrow, transform.position, m_OwnerObject.GetComponent<PhotonView>());
                }
                else if (hm != null)
                {
                    if (!m_HaveHit.Contains(hm.gameObject))
                    {
                        m_HaveHit.Add(hm.gameObject);
                    }

                    hm.Hit(10 + stats.magicAttack, ToolType.Arrow, transform.position, m_OwnerObject, 50);
                }
            }
            catch (System.Exception ex)
            {
                //Debug.Log(ex);
            }
        }

        if (other.transform.GetParentComponent<EnemyManager>() != null)
        {
            HealthManager hm = other.gameObject.GetComponentInParent<HealthManager>();
            if (hm != null)
            {
                hm.Hit(10, ToolType.Default, transform.position, m_OwnerObject, 50);
            }
            return;
        }

    }
}
