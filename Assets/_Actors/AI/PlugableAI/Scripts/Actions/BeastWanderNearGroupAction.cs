using UnityEngine;
using Pathfinding;
using MalbersAnimations.Reactions;

[CreateAssetMenu(menuName = "PluggableAI/Actions/BeastWanderNearGroup")]
public class BeastWanderNearGroupAction : Action
{
    private Vector3 destination = Vector3.zero;
    private float waitTimer = 0.0f;
    private bool isWaiting = false;
    private float maxDistance = 20f;

    public override void Act(StateController controller)
    {
        Wander(controller);
    }

    private void Wander(StateController controller)
    {
        if (BeastManager.Instance.m_IsInStable) return;
        controller.focusOnTarget = false;
        AIPath mover = controller.aiPath;
        Animator animator = controller.m_Animator;

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

        if (IsPlayerTooFar(controller))
        {
            destination = PickPointNearPlayer(controller, maxDistance);
            mover.destination = destination;
            animator.SetBool("Idle", false);
            return;
        }

        // Check if destination is unset or if the destination is reached
        if (!mover.hasPath || (mover.reachedDestination && !isWaiting))
        {
            if (mover.reachedDestination && !isWaiting)
            {
                isWaiting = true;
                waitTimer = UnityEngine.Random.Range(1, 3);
                destination = Vector3.zero; // Reset destination
                animator.SetBool("Idle", true);
                return;
            }

            if (!isWaiting)
            {
                destination = PickPointNearPlayer(controller, maxDistance);
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
                destination = PickPointNearPlayer(controller, maxDistance);
                mover.destination = destination;
                animator.SetBool("Idle", false);
            }
        }
    }

    private bool IsPlayerTooFar(StateController controller)
    {
        GameObject nearestPlayer = FindNearestPlayer(controller);
        if (nearestPlayer != null)
        {
            float distance;
            if (controller.aiPath.hasPath)
            {
                distance = Vector3.Distance(controller.aiPath.destination, nearestPlayer.transform.position);
            }
            else
            {
                distance = Vector3.Distance(controller.transform.position, nearestPlayer.transform.position);
            }
            return distance > maxDistance;
        }
        return false;
    }

    private Vector3 PickPointNearPlayer(StateController controller, float maxDistance)
    {
        GameObject nearestPlayer = FindNearestPlayer(controller);
        if (nearestPlayer != null)
        {
            Vector3 randomPoint = nearestPlayer.transform.position + Random.insideUnitSphere * maxDistance;
            randomPoint.y = controller.transform.position.y; // Keep Y consistent
            return randomPoint;
        }
        return controller.transform.position; // Fallback to the current position
    }

    private GameObject FindNearestPlayer(StateController controller)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject nearestPlayer = null;
        float minDistance = float.MaxValue;

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(controller.transform.position, player.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPlayer = player;
            }
        }
        return nearestPlayer;
    }
}
