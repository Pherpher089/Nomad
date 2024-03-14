using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Decisions/PlayersInBeastStorage")]

public class PlayerInBeastStorageDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        if (BeastManager.Instance.m_BeastChests[0].m_IsOpen || BeastManager.Instance.m_BeastChests[1].m_IsOpen)
        {
            controller.target = null;
            controller.navMeshAgent.destination = controller.transform.position;
            return true;
        }
        return false;
    }

}
