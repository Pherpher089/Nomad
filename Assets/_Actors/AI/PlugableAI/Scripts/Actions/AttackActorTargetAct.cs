using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Actions/AttackActor")]
public class AttackActorTargetAct : Action
{
    public override void Act(StateController controller)
    {
        AttackActor(controller);
    }
    private void AttackActor(StateController controller)
    {
        controller.aiPath.endReachedDistance = controller.enemyStats.attackRange;
        controller.focusOnTarget = true;
        // if (controller.transform.GetChild(0).gameObject.GetComponent<Animator>().GetBool("TakeHit"))
        // {
        //     coolDown = controller.enemyStats.attackRate;
        // }
        if (controller.attackCoolDown > 0)
        {
            controller.attackCoolDown -= 2 * Time.deltaTime;
        }
        else
        {
            if (controller.aiMover)
            {
                Vector3 dir = new(controller.target.position.x, controller.transform.position.y, controller.target.position.z);
                controller.transform.LookAt(dir, controller.transform.up);
                bool ranged = controller.m_ActorEquipment.hasItem && (controller.m_ActorEquipment.equippedItem.GetComponent<Item>().itemListIndex == 18 || controller.m_ActorEquipment.equippedItem.GetComponent<Item>().itemListIndex == 13);
                controller.aiMover.CallAttack_RPC(true, false, ranged);
            }
            controller.attackCoolDown = controller.enemyStats.attackRate;
        }
    }
}

