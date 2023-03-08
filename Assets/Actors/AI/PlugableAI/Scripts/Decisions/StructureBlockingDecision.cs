using UnityEngine;


[CreateAssetMenu(menuName = "PluggableAI/Decisions/StructureLook")]
public class StructureBlockingDecision : Decision
{

    public override bool Decide(StateController controller)
    {
        //bool targetVisible = Look(controller);
        //return targetVisible;

        bool inStructiure = TargetInBuilding(controller);
        return inStructiure;
    }

    private bool TargetInBuilding(StateController controller)
    {
        bool targetInBuilding = controller.target.gameObject.GetComponent<CharacterManager>().inBuilding;
        GameObject currentTarget = controller.target.gameObject;

        Transform parentStructure = currentTarget.gameObject.GetComponent<CharacterManager>().currentBuildingObj.transform;
        int cldCount = parentStructure.childCount;
        float closest = 1000f;
        Transform closestWall = null;

        for (int i = 0; i < cldCount; i++)
        {
            float dis = Vector3.Distance(parentStructure.GetChild(i).position, controller.transform.position);
            if (dis < closest)
            {
                closestWall = parentStructure.GetChild(i);
                closest = dis;
            }
        }

        controller.target = closestWall.gameObject.transform;
        //controller.chaseTarget = closestWall.GetComponent<BaseWall>().target.position;
        return targetInBuilding;
    }

    //private bool Look(StateController controller)
    //{
    //    RaycastHit hit;

    //    Debug.DrawRay(controller.eyes.position, controller.eyes.forward.normalized * controller.enemyStats.lookRange, Color.green);

    //    if (Physics.SphereCast(controller.eyes.position, controller.enemyStats.lookSphereCastRadius, controller.eyes.forward, out hit, controller.enemyStats.lookRange)
    //        && hit.collider.CompareTag("WallFrame"))
    //    {
    //        controller.chaseTarget = hit.transform;
    //        return true;
    //    }
    //    else
    //    {
    //        return false;
    //    }
    //}
}
