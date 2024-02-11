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
        controller.navMeshAgent.stoppingDistance = controller.enemyStats.attackRange;
        controller.focusOnTarget = true;
        if (controller.transform.GetChild(0).gameObject.GetComponent<Animator>().GetBool("TakeHit"))
        {
            coolDown = controller.enemyStats.attackRate;
        }
        if (coolDown > 0)
        {
            coolDown -= 2 * Time.deltaTime;
        }
        else
        {
            if (controller.aiMover)
            {
                Vector3 dir = new(controller.target.position.x, controller.transform.position.y, controller.target.position.z);
                controller.transform.LookAt(dir, controller.transform.up);
                controller.aiMover.Attack(true, false);
            }
            coolDown = controller.enemyStats.attackRate;
        }
    }
}

