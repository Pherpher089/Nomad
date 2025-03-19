using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Pathfinding;

public static class ActorUtils
{
    public static Vector3 GetRandomValidSpawnPoint(float radius, Vector3 position)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += position;
        GraphNode node = AstarPath.active.GetNearest(randomDirection).node;

        if (node != null && node.Walkable)
        {
            return position;
        }
        return position;
    }
}
