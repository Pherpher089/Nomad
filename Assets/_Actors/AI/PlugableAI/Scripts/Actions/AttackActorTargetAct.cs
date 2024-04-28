using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAI/Actions/AttackActor")]
public class AttackActorTargetAct : Action
{
    float coolDown = 0;
    ActorEquipment m_ActorEquipment;
    public override void Act(StateController controller)
    {
        m_ActorEquipment = controller.GetComponent<ActorEquipment>();
        AttackActor(controller);
    }
    private void AttackActor(StateController controller)
    {
        controller.navMeshAgent.stoppingDistance = controller.enemyStats.attackRange;
        controller.focusOnTarget = true;
        // if (controller.transform.GetChild(0).gameObject.GetComponent<Animator>().GetBool("TakeHit"))
        // {
        //     coolDown = controller.enemyStats.attackRate;
        // }
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
                bool ranged = m_ActorEquipment.hasItem && (m_ActorEquipment.equippedItem.GetComponent<Item>().itemListIndex == 18 || m_ActorEquipment.equippedItem.GetComponent<Item>().itemListIndex == 13);
                controller.aiMover.Attack(true, false, ranged);
            }
            coolDown = controller.enemyStats.attackRate;
        }
    }
}

