using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FellingAxe : Tool
{

    void Start()
    {
        icon = Resources.Load<Sprite>("Sprites/FellingAxeIcon");
        name = "Felling Axe";
    }
}