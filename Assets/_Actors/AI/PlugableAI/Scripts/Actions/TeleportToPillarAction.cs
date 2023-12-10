using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Actions/TeleportToPillar")]

public class TeleportToPillarAction : Action
{
    public override void Act(StateController controller)
    {
        FireHeadBoss boss = controller.GetComponent<FireHeadBoss>();
        controller.transform.position = boss.m_TargetPillar.GetChild(0).position;
        boss.m_CurrentHealthThreshold = boss.m_HealthManager.health / 2;
    }
}
