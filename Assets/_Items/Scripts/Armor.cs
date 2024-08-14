using UnityEngine;

public enum ArmorType { Helmet = 0, Chest = 1, Legs = 2 }

public class Armor : Item
{
    public ArmorType m_ArmorType;
    public ChestArmorCharacterIndexMap chestMap;
    public LegsArmorCharacterIndexMap legsMap;
    public HeadArmorCharacterIndexMap headMap;

    [Header("Stat Bonus")]
    public int dexBonus = 0;
    public int strBonus = 0;
    public int intBonus = 0;
    public int conBonus = 0;
    public float m_DefenseValue;
}
