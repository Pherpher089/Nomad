using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeastStick : MonoBehaviour
{
    public Tool tool;
    void Awake()
    {
        tool = GetComponent<Tool>();
    }
}
