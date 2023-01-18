using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/DestroyStructure")]
public class DestroyStructure : Action {

    float coolDown = 0;

    public override void Act(StateController controller)
    {
        Destroy(controller);
    }

    private void Destroy(StateController controller)
    {
       controller.focusOnTarget = true;

       if (coolDown > 0)
       {
           coolDown -= 1 * Time.deltaTime;
            controller.equipment.equipedItem.GetComponent<Item>().PrimaryAction(0);
       }
       else
       {
            if(controller.equipment.hasItem)
            {
                controller.equipment.equipedItem.GetComponent<Item>().PrimaryAction(1);
                coolDown = controller.enemyStats.attackRate;

            }
       }
    }
}
