using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfAnimationEventReceiver : MonoBehaviour
{
    AttackBox attackBox;
    // Start is called before the first frame update
    void Start()
    {
        attackBox = transform.parent.GetChild(2).GetComponent<AttackBox>();
    }

    public void Bite()
    {
        attackBox.Bite();
    }
}
