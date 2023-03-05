using Pathfinding;
using UnityEngine;

public class PathfinderController : MonoBehaviour
{
    AstarPath pathfinder;
    void Start()
    {
        pathfinder = FindObjectOfType<AstarPath>();
    }

    public void GenerateTerrainGridGraph(int size, Vector3 position)
    {
        GridGraph gridGraph = pathfinder.data.AddGraph(typeof(GridGraph)) as GridGraph;
        gridGraph.SetDimensions(240, 240, 1);
        gridGraph.center = position;
        gridGraph.Scan();
    }

    public void GenerateTerrainNavMeshGraph(Vector3 position, Mesh mesh)
    {
        // Create a new NavMeshGraph
        NavMeshGraph navMeshGraph = pathfinder.data.AddGraph(typeof(NavMeshGraph)) as NavMeshGraph;
        navMeshGraph.sourceMesh = mesh;
        // Set the center of the graph
        navMeshGraph.offset = position;
        navMeshGraph.enableNavmeshCutting = true;
        navMeshGraph.Scan();
    }
}
