using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ArmorType { Helmet = 0, Chest = 1, Legs = 2 }

public class Armor : Item
{
    public ArmorType m_ArmorType;
    public float m_DefenseValue;
}
