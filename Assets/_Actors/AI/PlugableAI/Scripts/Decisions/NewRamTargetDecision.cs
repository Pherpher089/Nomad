using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Decisions/NewRamTargetDecision")]

public class NewRamTargetDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        BeastManager bm = controller.GetComponent<BeastManager>();
        if (bm.m_GearIndices[3] == 0 && bm.m_RamTarget != null)
        {
            // Set the AI's target to the RamTarget's position
            controller.target = bm.m_RamTarget.transform;
            controller.aiPath.destination = bm.m_RamTarget.transform.position;

            // Triple the speed for the ram action
            controller.aiPath.maxSpeed *= 3; // Use 'maxSpeed' instead of 'speed' for AIPath

            // Set stopping distance to 0
            controller.aiPath.endReachedDistance = 0;

            return true;
        }

        return false;
    }
}

