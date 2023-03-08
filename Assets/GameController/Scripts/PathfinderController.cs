using Pathfinding;
using UnityEngine;
using System.Collections.Generic;

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
    public void GenerateTerrainNavMeshGraph(Mesh mesh)
    {
        // NavMeshGraph navMeshGraph = pathfinder.data.navmesh;
        // navMeshGraph.sourceMesh = Mesh.CombineMeshes(pathfinder.data.navmesh)
        // navMeshGraph.Scan();
    }
    public void GenerateRaycastGraph()
    {
        pathfinder.data.recastGraph.SnapForceBoundsToScene();
        pathfinder.data.recastGraph.Scan();
    }

    public static Mesh CombineNavMesh(Mesh mesh1, Mesh mesh2, Transform trans1, Transform trans2)
    {

        CombineInstance[] combine = new CombineInstance[2];


        combine[0].mesh = mesh1;
        combine[0].transform = trans1.localToWorldMatrix;
        combine[1].mesh = mesh2;
        combine[1].transform = trans2.localToWorldMatrix;

        Mesh newMesh = new Mesh();
        newMesh.CombineMeshes(combine);
        return newMesh;
    }

}
