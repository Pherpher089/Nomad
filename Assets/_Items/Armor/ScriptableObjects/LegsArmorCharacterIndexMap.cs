using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "LegsArmorCharacterIndexMap", menuName = "ArmorMap/LegArmor", order = 0)]
public class LegsArmorCharacterIndexMap : ScriptableObject
{
    public int waistIndex;
    public int beltItem1;
    public int beltItem2;
    public int beltItem3;
    public int beltItem4;
    public int rightKneeAttachment;
    public int leftKneeAttachment;
    public int rightLegIndex;
    public int leftLegIndex;
}
