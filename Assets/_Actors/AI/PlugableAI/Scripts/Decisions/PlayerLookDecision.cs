using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Decisions/Look")]
public class PlayerLookDecision : Decision
{

    public override bool Decide(StateController controller)
    {
        bool targetVisible = Look(controller);
        return targetVisible;
    }


    private bool Look(StateController controller, Transform player = null)
    {
        Transform targetTransform;
        if (player == null)
        {
            if (controller.target && controller.target.GetComponent<ActorManager>().actorState != ActorState.Dead)
            {
                targetTransform = controller.target.transform;
            }
            else
            {
                // GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                ThirdPersonUserControl[] players = PlayersManager.Instance.playerList.ToArray();

                // Iterate through the array of enemies and perform some action on each enemy.
                foreach (ThirdPersonUserControl _player in players)
                {
                    Look(controller, _player.transform);
                }
                if (BeastManager.Instance != null && BeastManager.Instance.gameObject != null)
                {
                    Look(controller, BeastManager.Instance.transform);
                }
                return false;
            }
        }
        else
        {
            targetTransform = player;
        }

        // Calculate the angle between the enemy's forward direction and the direction to the player.
        Vector3 enemyToPlayer = targetTransform.position - controller.transform.position;
        float angle = Vector3.Angle(controller.transform.forward, enemyToPlayer);

        // Check if the player is within the enemy's field of view (160 degrees in front of the enemy).
        if (Vector3.Distance(controller.transform.position, targetTransform.position) < controller.enemyStats.lookRange)
        {
            Debug.DrawRay(controller.transform.position + (Vector3.up * 2), enemyToPlayer, Color.red);
        }
        // Shoot a ray from the enemy to the player.
        Ray ray = new Ray(controller.transform.position + (Vector3.up * 2), enemyToPlayer);
        if (angle < 120)
        {

            if (Physics.Raycast(ray, out RaycastHit hit, controller.enemyStats.lookRange))
            {
                // Return true if the ray hits the player.
                if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Beast"))
                {

                    controller.target = hit.collider.gameObject.transform;
                    return true;
                }
            }
        }
        // Return false if the player is outside the field of view or the ray did not hit the player.
        return false;
    }
}