using Pathfinding;
using UnityEngine;

public class GenerateNavigationGraph : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        AstarPath pathFinder = GameObject.FindObjectOfType<AstarPath>();

        // Create a new NavMeshGraph
        NavMeshGraph navMeshGraph = pathFinder.data.AddGraph(typeof(NavMeshGraph)) as NavMeshGraph;

        // Use the original mesh without scaling the vertices
        navMeshGraph.sourceMesh = GetComponent<MeshFilter>().mesh;

        // Set the center of the graph
        navMeshGraph.offset = transform.position;
        navMeshGraph.rotation = transform.rotation.eulerAngles; // Consider rotation

        // Directly set the scale for the navMeshGraph
        navMeshGraph.scale = transform.localScale.x;

        navMeshGraph.enableNavmeshCutting = true;
        navMeshGraph.Scan();
    }
}
