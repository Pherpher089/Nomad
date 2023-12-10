using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Decisions/HealthReducedByThird")]

public class HealthReducedByThird : Decision
{
    public override bool Decide(StateController controller)
    {
        FireHeadBoss boss = controller.GetComponent<FireHeadBoss>();
        bool val = boss.m_CurrentHealthThreshold >= boss.m_HealthManager.health;
        Debug.Log("### health decision: " + val);
        if (val)
        {
            boss.m_TargetPillar = GameObject.FindGameObjectWithTag("Pillar").transform;
        }
        return val;
    }
}
