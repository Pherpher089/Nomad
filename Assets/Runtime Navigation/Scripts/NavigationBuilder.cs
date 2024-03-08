using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AI;
[CustomEditor(typeof(NavigationBuilder))]
class NavigationBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var t = target as NavigationBuilder;
        serializedObject.Update();
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        t.autoRefresh = EditorGUILayout.Toggle("Auto Refresh", t.autoRefresh);
        if (!t.autoRefresh) if (GUILayout.Button("Refresh")) t.Refresh();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        string[] sNames = new string[NavMesh.GetSettingsCount()];
        for (int i = 0; i < NavMesh.GetSettingsCount(); i++)
        {
            sNames[i] = NavMesh.GetSettingsNameFromID(NavMesh.GetSettingsByIndex(i).agentTypeID);
        }
        t.targetSettings = EditorGUILayout.Popup("Target Agent Type:", t.targetSettings, sNames);
        var s = NavMesh.GetSettingsByIndex(t.targetSettings);
        NavMeshEditorHelpers.DrawAgentDiagram(EditorGUILayout.GetControlRect(true, 150), s.agentRadius, s.agentHeight, s.agentClimb, s.agentSlope);
        serializedObject.ApplyModifiedProperties();
        EditorGUILayout.EndVertical();
        EditorApplication.update = () =>
        {
            if (!Application.isPlaying && t && t.autoRefresh) t.Refresh();
           if (t) EditorUtility.SetDirty(t);
            var g = Selection.activeGameObject;
            if (g)
            {
                if (g.GetComponent<NavigationBuilder>() || g.GetComponent<NavigationArea>())
                    NavMeshVisualizationSettings.showNavigation = 1;
                else
                    NavMeshVisualizationSettings.showNavigation = 0;
            }
            else NavMeshVisualizationSettings.showNavigation = 0;
        };
    }
}
#endif

[ExecuteInEditMode]
public class NavigationBuilder : MonoBehaviour
{
    public static NavigationBuilder instance;
    public NavigationBuilder()
    {
        instance = this;
    }

    public static void Spawn()
    {
        if (!FindObjectOfType<NavigationBuilder>()) new GameObject("Navigation Builder").AddComponent<NavigationBuilder>();
    }

    public int targetSettings = 0;
    public bool autoRefresh = true;
    bool isAdded = false;
    Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 10000);
    NavMeshData meshData;
    NavMeshDataInstance dataInstance;
    List<NavMeshBuildSource> data = new List<NavMeshBuildSource>();
    List<NavigationArea> areas = new List<NavigationArea>();
    
    void Start()
    {
        meshData = new NavMeshData();
        dataInstance = NavMesh.AddNavMeshData(meshData);
    }

    private void OnEnable()
    {
        if (!isAdded)
        {
            foreach (var p in FindObjectsOfType<NavigationArea>()) AddArea(p);
            isAdded = true;
        }
    }
    

    private void Update()
    {
      if(autoRefresh) Refresh();
    }

    private void OnDestroy()
    {
        dataInstance.Remove();
    }

    public void AddArea(NavigationArea area)
    {
        if (!areas.Contains(area)) areas.Add(area);
    }

    public void RemoveArea(NavigationArea area)
    {
        if (areas.Contains(area)) areas.Remove(area);
    }
    public void Refresh()
    {
        data.Clear();
        foreach (var area in areas)
        {
            if (!area) continue;
            if (!area.enabled || !area.gameObject.activeInHierarchy) continue;

            var s = new NavMeshBuildSource();
            s.area = area.area;

            if (area.GetComponent<MeshFilter>())
            {
                if (!area.GetComponent<MeshFilter>().sharedMesh) continue;
                var m = area.GetComponent<MeshFilter>().sharedMesh;
                if (!m) continue;
                s.shape = NavMeshBuildSourceShape.Mesh;
                s.sourceObject = m;
                s.transform = area.transform.localToWorldMatrix;
                data.Add(s);
            }

            if (area.GetComponent<Terrain>())
            {
                if (!area.GetComponent<Terrain>().terrainData) continue;
                var m = area.GetComponent<Terrain>().terrainData;
                if (!m) continue;
                s.shape = NavMeshBuildSourceShape.Terrain;
                s.sourceObject = m;
                s.transform = Matrix4x4.TRS(area.transform.position, Quaternion.identity, Vector3.one);
                data.Add(s);
            }
        }
        UnityEngine.AI.NavMeshBuilder.UpdateNavMeshData(meshData, NavMesh.GetSettingsByIndex(targetSettings), data, bounds);
    }
}
