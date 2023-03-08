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
            if (controller.actorTarget)
            {
                Debug.Log("### Here 2");
                targetTransform = controller.actorTarget.transform;
            }
            else
            {
                Debug.Log("### Here 3");
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                // Iterate through the array of enemies and perform some action on each enemy.
                foreach (GameObject _player in players)
                {
                    Debug.Log("### Here 4");
                    Look(controller, _player.transform);

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

        // Shoot a ray from the enemy to the player.
        Ray ray = new Ray(controller.transform.position, enemyToPlayer);

        if (angle > 80 && angle < 100) ;
        {
            Debug.DrawLine(controller.transform.position, targetTransform.position, Color.red, 0.01f);
            if (Physics.Raycast(ray, out RaycastHit hit, controller.enemyStats.lookRange))
            {

                // Return true if the ray hits the player.
                if (hit.collider.CompareTag("Player"))
                {
                    Debug.Log("### sees player");
                    controller.actorTarget = hit.collider.gameObject;
                    controller.chaseTarget = hit.collider.gameObject.transform.position;
                    return true;
                }
            }
        }
        Debug.Log("### Did Not see the player");
        // Return false if the player is outside the field of view or the ray did not hit the player.
        return false;
    }
}