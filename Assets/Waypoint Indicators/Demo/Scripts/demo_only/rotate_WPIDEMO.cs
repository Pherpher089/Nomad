using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotate_WPIDEMO : MonoBehaviour
{
    public float XSpeed = 2f;
    public float YSpeed = 2f;
    public float ZSpeed = 2f;

    void Update()
    {
        transform.Rotate(XSpeed, YSpeed, ZSpeed, Space.World);
    }
}
