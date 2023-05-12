using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPointVisulizer : MonoBehaviour {

    public Color thisColor;
    public float gizmoSize;
    public void OnDrawGizmos()
    {
        Gizmos.color = thisColor;
        Gizmos.DrawWireSphere(transform.position, gizmoSize );
    }
}
