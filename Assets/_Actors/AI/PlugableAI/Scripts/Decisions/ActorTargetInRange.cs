using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Decisions/TargetRange")]

public class ActorTargetInRange : Decision
{

    public override bool Decide(StateController controller)
    {
        return TargetInRange(controller);
    }

    private bool TargetInRange(StateController controller)
    {
        float dis = Vector3.Distance(controller.target.transform.position, controller.transform.position);

        if (dis < controller.enemyStats.attackRange)
        {
            return true;
        }

        return false;
    }
}
