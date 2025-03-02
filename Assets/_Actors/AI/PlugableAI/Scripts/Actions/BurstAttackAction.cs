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
        if (controller.transform.GetChild(0).gameObject.GetComponent<Animator>().GetBool("TakeHit"))
        {
            burstCooldown = controller.enemyStats.attackRate;
        }
        if (burstCooldown > 0)
        {
            burstCooldown -= 2 * Time.deltaTime;
        }
        else
        {

            if (controller.aiMover && burstCounter < 5)
            {
                Vector3 dir = new(controller.target.position.x, controller.transform.position.y, controller.target.position.z);
                controller.transform.LookAt(dir, controller.transform.up);
                // bool ranged = controller.m_ActorEquipment.hasItem && (controller.m_ActorEquipment.equippedItem.GetComponent<Item>().itemListIndex == 18 || controller.m_ActorEquipment.equippedItem.GetComponent<Item>().itemListIndex == 13);
                controller.aiMover.CallAttack_RPC(true, false);
                burstCooldown = 2f;
                burstCounter++;

            }
            else
            {
                burstCooldown = controller.enemyStats.attackRate;
                burstCounter = 0;
            }
        }
    }
}
