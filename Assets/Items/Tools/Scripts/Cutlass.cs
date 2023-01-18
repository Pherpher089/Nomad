using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutlass : Tool
{
    // Start is called before the first frame update
    void Start()
    {
        icon = Resources.Load<Sprite>("Sprites/CutlassIcon");
        name = "Cutlass";
    }
}
