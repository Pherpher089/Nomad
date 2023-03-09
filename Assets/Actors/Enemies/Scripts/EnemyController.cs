using Pathfinding;
using UnityEngine;

/// Incharge of the character movement
public class EnemyController : MonoBehaviour
{

    public Transform playerTransform;
    public GameObject target;
    AIPath aiPath;
    public float siteDistance = 100;

    // Start is called before the first frame update
    void Start()
    {
        aiPath = GetComponent<AIPath>();
    }

    // Update is called once per frame
    void Update()
    {

        if (aiPath.hasPath == false && target != null)
        {
            GetComponent<AIMover>().Move(target.transform.position - transform.position);
        }
    }
}
