using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class StatusEffectController : MonoBehaviour
{
    PhotonView pv;
    GameObject fireStatusEffect;
    GameObject frozenEffect;
    HealthManager healthManager;
    SourceObject sourceObject;
    ThirdPersonCharacter character;
    public bool canCatchOnFire;
    public bool canBeFrozen;
    bool currentlyHasEffect = false;
    float previousSpeed = 0;
    StateController stateController;

    void Awake()
    {
        sourceObject = GetComponentInParent<SourceObject>();
        healthManager = GetComponentInParent<HealthManager>();
        character = GetComponentInParent<ThirdPersonCharacter>();
        stateController = GetComponentInParent<StateController>();
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
        if (currentlyHasEffect) return;
        currentlyHasEffect = true;
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
            yield return new WaitForSeconds(2);
            counter += 2;
        }
        fireStatusEffect.SetActive(false);
        currentlyHasEffect = false;
        yield return null;
    }

    public void CallFreeze(float damagePerSecond, float duration)
    {
        pv.RPC("Freeze", RpcTarget.All, damagePerSecond, duration);
    }


    [PunRPC]
    public void Freeze(float damagePerSecond, float duration)
    {
        if (!canCatchOnFire) return;
        if (currentlyHasEffect) return;
        currentlyHasEffect = true;
        if (transform.parent.tag == "Player")
        {
            previousSpeed = character.m_MoveSpeedMultiplier;
            character.m_MoveSpeedMultiplier = previousSpeed / 2;
        }
        else if (transform.parent.tag == "Enemy")
        {
            previousSpeed = stateController.moveSpeed;
            stateController.moveSpeed = previousSpeed / 2;
        }
        frozenEffect.SetActive(true);
        StartCoroutine(FreezeStatus(damagePerSecond, duration));

    }

    IEnumerator FreezeStatus(float damagePerSecond, float duration)
    {

        float counter = 0;
        while (counter < duration && healthManager.health > 0)
        {
            // if (pv.IsMine)
            // {
            //     if (healthManager != null)
            //     {
            //         healthManager.Hit((int)damagePerSecond, ToolType.Default, transform.position + Vector3.up * 2, this.gameObject, 0);
            //     }
            //     else if (sourceObject != null)
            //     {
            //         sourceObject.Hit((int)damagePerSecond, ToolType.Default, transform.position + Vector3.up * 2, this.gameObject);
            //     }
            // }
            yield return new WaitForSeconds(1);
            counter += 1;
        }
        frozenEffect.SetActive(false);
        if (transform.parent.tag == "Player")
        {
            character.m_MoveSpeedMultiplier = previousSpeed;
        }
        else if (transform.parent.tag == "Enemy")
        {
            stateController.moveSpeed = previousSpeed;
        }
        currentlyHasEffect = false;
        yield return null;
    }
}
