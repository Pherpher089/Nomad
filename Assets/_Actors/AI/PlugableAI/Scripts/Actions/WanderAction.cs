using Pathfinding;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/Wander")]
public class WanderAction : Action
{
    private Vector3 destination = Vector3.zero;
    private Vector3 startingPos;
    private float waitTimer = 0.0f;
    private bool isWaiting = false;
    private float maxDistance = 30f;

    public override void Act(StateController controller)
    {
        Wander(controller);
    }

    private void Wander(StateController controller)
    {
        controller.focusOnTarget = false;
        AIPath mover = controller.GetComponent<AIPath>();

        // If no destination has been set, pick a new random destination
        if (destination == Vector3.zero)
        {
            startingPos = controller.transform.position;
            destination = PickAPoint(controller, maxDistance);
            mover.destination = destination;
            return;
        }

        // Check if the AI has reached its destination and isn't waiting
        if (!mover.pathPending && mover.reachedDestination && !isWaiting)
        {
            isWaiting = true;
            waitTimer = UnityEngine.Random.Range(1, 10); // Set a random wait time
            return;
        }

        // Handle waiting at the destination
        if (isWaiting)
        {
            if (waitTimer > 0)
            {
                waitTimer -= Time.deltaTime;
            }
            else
            {
                isWaiting = false;
                destination = ActorUtils.GetRandomValidSpawnPoint(maxDistance, startingPos);
                mover.destination = destination;
            }
        }
    }

    public static Vector3 PickAPoint(StateController controller, float maxDistance)
    {
        // Pick a random point within the specified distance
        var point = Random.insideUnitSphere * maxDistance;
        point += controller.transform.position;
        point.y = GetTerrainHeightAtPoint(point); // Adjust height based on terrain
        return point;
    }

    public static float GetTerrainHeightAtPoint(Vector3 point)
    {
        // Cast a ray downward to find the terrain height
        Vector3 origin = point + (Vector3.up * 300);
        Vector3 direction = Vector3.down;
        RaycastHit[] hits = Physics.RaycastAll(origin, direction, 1000);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("WorldTerrain"))
            {
                return hit.point.y;
            }
        }

        return -10000; // Return a default value if no terrain is found
    }
}
