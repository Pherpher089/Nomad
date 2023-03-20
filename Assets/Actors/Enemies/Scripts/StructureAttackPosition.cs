using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureAttackPosition : MonoBehaviour {

    public bool occupied;
    public GameObject occupant;

    private void Start()
    {
        occupied = false;
    }
}
