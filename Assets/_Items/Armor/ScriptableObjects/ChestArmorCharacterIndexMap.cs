using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ChestArmorCharacterIndexMap", menuName = "ArmorMap/ChestArmor", order = 0)]
public class ChestArmorCharacterIndexMap : ScriptableObject
{
    public int chestIndex;
    public int upperRightArmIndex;
    public int upperLeftArmIndex;
    public int lowerRightArmIndex;
    public int lowerLeftArmIndex;
}
