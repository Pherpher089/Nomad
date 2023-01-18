using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stick : Tool
{
    void Start()
    {
        icon = Resources.Load<Sprite>("Sprites/StickIcon");
        name = "Stick";
    }
}