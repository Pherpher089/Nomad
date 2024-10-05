using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "HeadArmorCharacterIndexMap", menuName = "ArmorMap/HeadArmor", order = 0)]
public class HeadArmorCharacterIndexMap : ScriptableObject
{
    public int headCoveringsBaseHairIndex = -1;
    public int headCoveringsNoFacialHairIndex = -1;
    public int headCoveringsNoHairIndex = -1;
    public int hairIndex = -1;
    public int headAttachmentsHelmetIndex = -1;
    public int headAttachments2HelmetIndex = -1;
}
