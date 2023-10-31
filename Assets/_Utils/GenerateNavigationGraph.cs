using Pathfinding;
using UnityEngine;

public class GenerateNavigationGraph : MonoBehaviour
{
    GenerateNavigationGraph Instance;
    // Start is called before the first frame update
    void Awake()
    {
        AstarPath pathFinder = GameObject.FindObjectOfType<AstarPath>();
        // Create a new NavMeshGraph
        NavMeshGraph navMeshGraph = pathFinder.data.AddGraph(typeof(NavMeshGraph)) as NavMeshGraph;
        navMeshGraph.sourceMesh = GetComponent<MeshFilter>().mesh;
        // Set the center of the graph
        navMeshGraph.offset = transform.position;
        navMeshGraph.enableNavmeshCutting = true;
        navMeshGraph.Scan();
    }
}
