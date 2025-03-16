using Pathfinding;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/Wander")]
public class WanderAction : Action
{
    private bool setDestination = false;
    private Vector3 startingPos;
    private float waitTimer = 0.0f;
    private bool isWaiting = false;
    private float maxDistance = 1f;

    public override void Act(StateController controller)
    {
        Wander(controller);
    }

    private void Wander(StateController controller)
    {

        controller.focusOnTarget = false;
        AIPath mover = controller.GetComponent<AIPath>();

        // If no destination has been set, pick a new random destination
        if (!setDestination && !isWaiting)
        {
            startingPos = controller.transform.position;
            mover.destination = ActorUtils.GetRandomValidSpawnPoint(maxDistance, startingPos);
            setDestination = true;
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
                mover.destination = ActorUtils.GetRandomValidSpawnPoint(maxDistance, startingPos);
            }
        }
    }
}
