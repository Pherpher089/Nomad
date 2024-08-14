using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cape : Item
{
    public int m_CapeIndex;
    [Header("Stat Bonus")]
    public int dexBonus = 0;
    public int strBonus = 0;
    public int intBonus = 0;
    public int conBonus = 0;
    public float m_DefenseValue = 0;
}
