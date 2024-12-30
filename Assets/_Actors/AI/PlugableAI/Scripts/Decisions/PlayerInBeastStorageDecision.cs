using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Decisions/PlayersInBeastStorage")]

public class PlayerInBeastStorageDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        if (LevelManager.Instance.beastLevel == 2 && (BeastManager.Instance.m_BeastChests[0].m_IsOpen || BeastManager.Instance.m_BeastChests[1].m_IsOpen))
        {
            controller.target = null;
            controller.aiPath.destination = controller.transform.position;
            return true;
        }
        return false;
    }

}
