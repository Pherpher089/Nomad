using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/BurstAttackAction")]

public class BurstAttackAction : Action
{
    float burstCooldown = 0;
    int burstCounter = 0;

    public override void Act(StateController controller)
    {
        AttackActor(controller);
    }

    private void AttackActor(StateController controller)
    {
        controller.aiPath.endReachedDistance = controller.enemyStats.attackRange;
        controller.focusOnTarget = true;
        if (burstCooldown > 0)
        {
            burstCooldown -= 2 * Time.deltaTime;
        }
        else
        {
            if (controller.aiMover && burstCounter < 5)
            {
                if (controller.m_Animator.GetBool("Attacking")) return;
                Vector3 dir = new(controller.target.position.x, controller.transform.position.y, controller.target.position.z);
                controller.transform.LookAt(dir, controller.transform.up);
                controller.aiMover.CallAttack_RPC(true, false);
                burstCounter++;
            }
            else
            {
                burstCounter = 0;
                burstCooldown = controller.enemyStats.attackRate;
            }
        }
    }
}
