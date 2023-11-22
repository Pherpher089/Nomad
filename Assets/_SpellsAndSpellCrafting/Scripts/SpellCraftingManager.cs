using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellCraftingManager : MonoBehaviour
{
    public SpellCirclePedestalInteraction[] m_Pedestals;
    public SpellCircleAlterInteraction m_Alter;
    public void Start()
    {
        m_Pedestals = GetComponentsInChildren<SpellCirclePedestalInteraction>();
        m_Alter = GetComponentInChildren<SpellCircleAlterInteraction>();
    }
}
