using Pathfinding;
using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    public Transform playerTransform;
    public GameObject target;
    AIPath aiPath;
    public float siteDistance = 100;

    // Start is called before the first frame update
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        aiPath = GetComponent<AIPath>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 enemyToPlayer = playerTransform.position - transform.position;
        float angle = Vector3.Angle(transform.forward, enemyToPlayer);

        // Check if the player is within the enemy's field of view (160 degrees in front of the enemy).

        // Shoot a ray from the enemy to the player.
        Ray ray = new Ray(transform.position, enemyToPlayer);

        if (angle > 80 && angle < 100) ;
        {
            Debug.DrawLine(transform.position, playerTransform.position, Color.red, 0.01f);
            if (Physics.Raycast(ray, out RaycastHit hit, siteDistance))
            {

                // Return true if the ray hits the player.
                if (hit.collider.CompareTag("Player"))
                {
                    Debug.Log("### sees player");
                    target = hit.collider.gameObject;
                }
            }
        }

        if (target != null)
        {
            aiPath.destination = target.transform.position;
        }

    }
}
