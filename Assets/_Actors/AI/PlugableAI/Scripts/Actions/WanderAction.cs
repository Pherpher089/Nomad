using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "PluggableAI/Actions/Wander")]
public class WanderAction : Action
{
    Vector3 destination = Vector3.zero;
    Vector3 startingPos;
    float waitTimer = 0.0f;
    bool isWaiting = false;
    float maxDistance = 30f;
    public override void Act(StateController controller)
    {
        Wander(controller);
    }

    private void Wander(StateController controller)
    {
        controller.focusOnTarget = false;
        NavMeshAgent mover = controller.GetComponent<NavMeshAgent>();

        if (destination == Vector3.zero)
        {
            startingPos = controller.transform.position;
            destination = PickAPoint(controller, maxDistance);
            mover.destination = destination;
            return;
        }

        if (!mover.pathPending && (mover.pathStatus == NavMeshPathStatus.PathComplete || !mover.hasPath) && !isWaiting)
        {
            isWaiting = true;
            waitTimer = UnityEngine.Random.Range(1, 10);
            return;
        }

        if (!mover.pathPending && (mover.pathStatus == NavMeshPathStatus.PathComplete || !mover.hasPath) && isWaiting)
        {
            if (waitTimer > 0)
            {
                waitTimer -= Time.deltaTime;
            }
            else if (!mover.pathPending && (mover.pathStatus == NavMeshPathStatus.PathComplete || !mover.hasPath))
            {
                isWaiting = false;
                destination = PickAPoint(controller, maxDistance);

                mover.destination = destination;
            }
        }


    }
    public static Vector3 PickAPoint(StateController controller, float maxDistance)
    {
        var point = Random.insideUnitSphere * maxDistance;

        point += controller.transform.position;
        point.y = GetTerrainHeightAtPoint(point);
        return point;


    }
    public static float GetTerrainHeightAtPoint(Vector3 point)
    {
        Vector3 origin = point + (Vector3.up * 300);
        Vector3 direction = Vector3.down;
        RaycastHit[] hits = Physics.RaycastAll(origin, direction, 1000);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.tag == "WorldTerrain")
            {
                return hit.point.y;
            }
        }
        return -10000;
    }

}