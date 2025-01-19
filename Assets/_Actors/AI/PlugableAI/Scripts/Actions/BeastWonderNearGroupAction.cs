using UnityEngine;
using Pathfinding;

[CreateAssetMenu(menuName = "PluggableAI/Actions/BeastWanderNearGroup")]
public class BeastWanderNearGroupAction : Action
{
    private Vector3 destination = Vector3.zero;
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
        Animator animator = controller.GetComponent<Animator>();

        // Stop movement if eating
        if (animator.GetBool("Eating"))
        {
            mover.canMove = false;
            mover.destination = controller.transform.position;
            return;
        }
        else
        {
            mover.canMove = true;
        }

        // Check if destination is unset or if the destination is reached
        if (!mover.hasPath || (mover.reachedDestination && !isWaiting))
        {
            if (mover.reachedDestination && !isWaiting)
            {
                isWaiting = true;
                waitTimer = UnityEngine.Random.Range(5, 10);
                destination = Vector3.zero; // Reset destination
                animator.SetBool("Idle", true);
                return;
            }

            if (!isWaiting)
            {
                destination = PickAPointOnNavMesh(controller, maxDistance);
                mover.destination = destination;
                animator.SetBool("Idle", false);
            }
        }

        // Handle waiting
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                isWaiting = false;
                destination = PickAPointOnNavMesh(controller, maxDistance);
                mover.destination = destination;
                animator.SetBool("Idle", false);
            }
        }
    }

    private Vector3 PickAPointOnNavMesh(StateController controller, float maxDistance)
    {
        Vector3 randomPoint;
        GraphNode nearestNode;
        NNInfo nearestInfo;
        int maxAttempts = 100; // Limit to 100 attempts
        int attemptCount = 0;

        do
        {
            attemptCount++;
            if (attemptCount > maxAttempts)
            {
                return controller.transform.position; // Fallback to the current position
            }

            // Generate a random point within the maxDistance range
            randomPoint = Random.insideUnitSphere * maxDistance + PlayersManager.Instance.GetCenterPoint();
            randomPoint.y = controller.transform.position.y; // Keep Y consistent

            // Find the nearest valid node on the graph
            nearestInfo = AstarPath.active.GetNearest(randomPoint, NNConstraint.Default);
            nearestNode = nearestInfo.node;

        } while (nearestNode == null || !nearestNode.Walkable);
        return (Vector3)nearestInfo.position;
    }
}
