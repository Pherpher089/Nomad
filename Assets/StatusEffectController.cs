using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class StatusEffectController : MonoBehaviour
{
    PhotonView pv;
    GameObject fireStatusEffect;
    GameObject frozenEffect;
    HealthManager healthManager;
    SourceObject sourceObject;
    public bool canCatchOnFire;
    public bool canBeFrozen;

    void Awake()
    {
        sourceObject = GetComponentInParent<SourceObject>();
        healthManager = GetComponentInParent<HealthManager>();
        pv = GetComponent<PhotonView>();
        fireStatusEffect = transform.GetChild(0).gameObject;
        fireStatusEffect.SetActive(false);
        frozenEffect = transform.GetChild(1).gameObject;
        frozenEffect.SetActive(false);
    }

    public void CallCatchFire(float damagePerSecond, float duration)
    {
        pv.RPC("CatchFire", RpcTarget.All, damagePerSecond, duration);
    }


    [PunRPC]
    public void CatchFire(float damagePerSecond, float duration)
    {
        if (!canCatchOnFire) return;
        fireStatusEffect.SetActive(true);
        StartCoroutine(FireStatus(damagePerSecond, duration));

    }

    IEnumerator FireStatus(float damagePerSecond, float duration)
    {
        float counter = 0;
        while (counter < duration && healthManager.health > 0)
        {
            if (pv.IsMine)
            {
                if (healthManager != null)
                {
                    healthManager.Hit((int)damagePerSecond, ToolType.Default, transform.position + Vector3.up * 2, this.gameObject, 0);
                }
                else if (sourceObject != null)
                {
                    sourceObject.Hit((int)damagePerSecond, ToolType.Default, transform.position + Vector3.up * 2, this.gameObject);
                }
            }
            yield return new WaitForSeconds(1);
            counter += 1;
        }
        fireStatusEffect.SetActive(false);
        yield return null;
    }


}
