using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;


public class BeastGear : MonoBehaviour
{
    public string description;
    public string gearName;
    public Sprite icon;
    [Description("The indices of the beast gear that are going to be spawned in the corresponding gear socket. For instance, the best lantern should be [1,1]. For the chests it will be [2,3] because the individual items are two separate prefabs. A left and right chest")]
    public int[] gearItemIndices;
    [Description("This is the index of the slot that the gear goes into. The list is as follows: Antler = 0, Back = 1, Head = 2, Shoes = 4, RightSide = 5, Rump = 6, LeftSide = 7")]
    public int[] gearIndex;

}
