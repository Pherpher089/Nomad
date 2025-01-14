using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesManager : MonoBehaviour
{
    public static EnemiesManager Instance;
    public Dictionary<BuildingObject, int> targetedWalls;
    public int numberOfAttackPoints = 5;
    public List<EnemyManager> enemies;
    void Awake()
    {
        Instance = this;
        targetedWalls = new Dictionary<BuildingObject, int>();
    }

    public void AddNewTarget(BuildingObject target)
    {
        if (!targetedWalls.ContainsKey(target))
        {
            targetedWalls.Add(target, 1);
        }
    }
    public bool CheckIfTargetExists(BuildingObject target)
    {
        return targetedWalls.ContainsKey(target);
    }
    public void AddEnemy(EnemyManager enemy)
    {
        enemies.Add(enemy);
    }
    public int GetEnemyIndex(EnemyManager enemy)
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] == enemy)
            {
                return i;
            }
        }
        return -1;
    }
    public bool CheckIfPositionIsTaken(Vector3 position)
    {
        StateController[] enemies = FindObjectsOfType<StateController>();
        foreach (StateController enemy in enemies)
        {
            if (enemy.aiPath.destination == position)
            {
                return true;
            }
        }
        return false;
    }
    public BuildingObject TryGetTarget()
    {
        // Cleaning up the dictionary from destroyed objects
        if (targetedWalls.Count < numberOfAttackPoints) return null;
        var keysToRemove = new List<BuildingObject>();
        foreach (var kvp in targetedWalls)
        {
            if (kvp.Key == null || kvp.Key.Equals(null))
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        foreach (var key in keysToRemove)
        {
            targetedWalls.Remove(key);
        }

        // Proceed to find a suitable target
        int least = 10000;
        BuildingObject currentChoice = null; // Initialized to null to ensure it's always assigned
        foreach (KeyValuePair<BuildingObject, int> kvp in targetedWalls)
        {
            if (kvp.Value < least || currentChoice == null) // Ensures there's always at least one choice
            {
                least = kvp.Value;
                currentChoice = kvp.Key;
            }
        }
        if (currentChoice != null)
        {
            targetedWalls[currentChoice] += 1; // Increase the number of attackers targeting the chosen wall
        }
        return currentChoice;
    }

}
