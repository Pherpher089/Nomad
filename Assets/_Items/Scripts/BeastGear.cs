using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;


public class BeastGear : MonoBehaviour
{
    public string description;
    public string gearName;
    public Sprite icon;
    public int gearItemIndex;
    [Description("This is the index of the slot that the gear goes into. The list is as follows: Antler = 0, Saddle = 1, Back = 2, Head = 3, Sides = 4, Shoes = 5")]
    public int gearIndex;

}
