using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesManager : MonoBehaviour
{
    public static EnemiesManager Instance;
    public Dictionary<BuildingObject, int> targetedWalls;
    public int numberOfAttackPoints = 5;
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
    public BuildingObject TryGetTarget()
    {
        // Cleaning up the dictionary from destroyed objects
        if (targetedWalls.Count < numberOfAttackPoints) return null;
        Debug.Log("### returning a wall");
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
            Debug.Log("### updating curent choice amount");
            targetedWalls[currentChoice] += 1; // Increase the number of attackers targeting the chosen wall
        }
        return currentChoice;
    }

}
