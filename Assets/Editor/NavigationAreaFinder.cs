using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using System.Collections.Generic;

public class NavigationAreaFinder : EditorWindow
{
    private List<GameObject> navigationAreaObjects = new List<GameObject>();
    private List<GameObject> navMeshObstacleObjects = new List<GameObject>();
    private Vector2 scrollPosition = Vector2.zero;

    [MenuItem("Tools/Navigation Component Finder")]
    public static void ShowWindow()
    {
        GetWindow<NavigationAreaFinder>("Navigation Component Finder");
    }

    private void OnGUI()
    {
        // Buttons for finding components
        if (GUILayout.Button("Find All NavigationAreas"))
        {
            FindNavigationAreas();
        }

        if (GUILayout.Button("Find All NavMeshObstacles"))
        {
            FindNavMeshObstacles();
        }

        // Global toggle buttons for NavigationAreas
        GUILayout.Label("NavigationAreas:");
        if (GUILayout.Button("Disable All NavigationAreas"))
        {
            ToggleNavigationAreas(false);
        }

        if (GUILayout.Button("Enable All NavigationAreas"))
        {
            ToggleNavigationAreas(true);
        }

        // Global toggle buttons for NavMeshObstacles
        GUILayout.Label("NavMeshObstacles:");
        if (GUILayout.Button("Disable All NavMeshObstacles"))
        {
            ToggleNavMeshObstacles(false);
        }

        if (GUILayout.Button("Enable All NavMeshObstacles"))
        {
            ToggleNavMeshObstacles(true);
        }

        // Display results
        if (navigationAreaObjects.Count > 0 || navMeshObstacleObjects.Count > 0)
        {
            GUILayout.Label($"NavigationAreas: {navigationAreaObjects.Count} | NavMeshObstacles: {navMeshObstacleObjects.Count}");

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.BeginVertical("box");

            // Display NavigationAreas
            if (navigationAreaObjects.Count > 0)
            {
                GUILayout.Label("NavigationAreas:");
                foreach (var obj in navigationAreaObjects)
                {
                    DisplayObjectRow(obj, typeof(NavigationArea));
                }
            }

            // Display NavMeshObstacles
            if (navMeshObstacleObjects.Count > 0)
            {
                GUILayout.Label("NavMeshObstacles:");
                foreach (var obj in navMeshObstacleObjects)
                {
                    DisplayObjectRow(obj, typeof(NavMeshObstacle));
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
        else
        {
            GUILayout.Label("No components found.");
        }
    }

    private void DisplayObjectRow(GameObject obj, System.Type componentType)
    {
        EditorGUILayout.BeginHorizontal();

        // Get the component and its current enabled state
        var component = obj.GetComponent(componentType) as Behaviour; // NavigationArea and NavMeshObstacle derive from Behaviour
        bool isEnabled = component != null && component.enabled;

        // Checkbox to toggle the enabled state of the component
        bool newState = EditorGUILayout.Toggle(isEnabled, GUILayout.Width(20));
        if (newState != isEnabled)
        {
            component.enabled = newState; // Update the component's enabled state
            Debug.Log($"{(newState ? "Enabled" : "Disabled")} {componentType.Name} on {obj.name}");
        }

        // Button to select the object in the hierarchy
        if (GUILayout.Button(obj.name, GUILayout.ExpandWidth(true)))
        {
            Selection.activeObject = obj;
        }

        EditorGUILayout.EndHorizontal();
    }

    private void FindNavigationAreas()
    {
        navigationAreaObjects.Clear();

        // Find all NavigationArea components
        var areas = FindObjectsOfType<NavigationArea>(true); // Include inactive objects
        foreach (var area in areas)
        {
            navigationAreaObjects.Add(area.gameObject);
        }

        Debug.Log($"Found {navigationAreaObjects.Count} NavigationAreas.");
    }

    private void FindNavMeshObstacles()
    {
        navMeshObstacleObjects.Clear();

        // Find all NavMeshObstacle components
        var obstacles = FindObjectsOfType<NavMeshObstacle>(true); // Include inactive objects
        foreach (var obstacle in obstacles)
        {
            navMeshObstacleObjects.Add(obstacle.gameObject);
        }

        Debug.Log($"Found {navMeshObstacleObjects.Count} NavMeshObstacles.");
    }

    private void ToggleNavigationAreas(bool enable)
    {
        foreach (var obj in navigationAreaObjects)
        {
            if (obj.TryGetComponent<NavigationArea>(out var navArea))
            {
                navArea.enabled = enable;
            }
        }

        Debug.Log($"{(enable ? "Enabled" : "Disabled")} all NavigationAreas.");
    }

    private void ToggleNavMeshObstacles(bool enable)
    {
        foreach (var obj in navMeshObstacleObjects)
        {
            if (obj.TryGetComponent<NavMeshObstacle>(out var obstacle))
            {
                obstacle.enabled = enable;
            }
        }

        Debug.Log($"{(enable ? "Enabled" : "Disabled")} all NavMeshObstacles.");
    }
}
