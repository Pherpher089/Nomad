using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Decisions/AchievedPillarPositionDecision")]

public class AchievedPillarPositionDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        return controller.GetComponent<FireHeadBoss>().m_TargetPillar.GetChild(0).position == controller.transform.position;
    }
}
