using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class RockWallParticleController : MonoBehaviour
{
    GameObject m_OwnerObject;

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
                SourceObject so = other.gameObject.GetComponent<SourceObject>();
                BuildingMaterial bm = other.gameObject.GetComponent<BuildingMaterial>();
                if (bm != null)
                {
                    LevelManager.Instance.CallUpdateObjectsPRC(bm.spawnId, 10, ToolType.Default, transform.position, m_OwnerObject.GetComponent<PhotonView>());
                }
                else if (so != null)
                {
                    LevelManager.Instance.CallUpdateObjectsPRC(so.id, 10, ToolType.Default, transform.position, m_OwnerObject.GetComponent<PhotonView>());
                }
                else if (hm != null)
                {
                    hm.Hit(10, ToolType.Default, transform.position, m_OwnerObject, 50);
                }
                return;
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
