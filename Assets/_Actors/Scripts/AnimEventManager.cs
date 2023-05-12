using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimEventManager : MonoBehaviour {

    public bool isAttacking = false;

    public void AttackBegin()
    {
        isAttacking = true;
    }

    public void AttackEnd()
    {
        isAttacking = false;
    }
}
