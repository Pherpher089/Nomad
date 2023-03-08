using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/AttackActor")]
public class AttackActorTargetAct : Action
{
    float coolDown = 0;

    public override void Act(StateController controller)
    {
        AttackActor(controller);
    }

    private void AttackActor(StateController controller)
    {
        controller.aiPath.endReachedDistance = controller.enemyStats.attackRange;
        controller.focusOnTarget = true;

        if (coolDown > 0)
        {
            coolDown -= 2 * Time.deltaTime;
        }
        else
        {

            if (controller.aiMover)
            {
                controller.aiMover.Attack(true, false);
            }
            coolDown = controller.enemyStats.attackRate;
        }
    }
}

