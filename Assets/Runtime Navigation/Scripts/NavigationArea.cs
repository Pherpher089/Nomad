using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(NavigationArea)), CanEditMultipleObjects]
class NavigationAreaEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var t = target as NavigationArea;
        t.area = EditorGUILayout.Popup("Area:", t.area, GameObjectUtility.GetNavMeshAreaNames());
    }
}
#endif
[ExecuteInEditMode]
public class NavigationArea : MonoBehaviour
{
    private void Start()
    {
        if (!NavigationBuilder.instance) NavigationBuilder.Spawn();
        NavigationBuilder.instance.AddArea(this);
    }
    private void OnDestroy()
    {
        if (NavigationBuilder.instance)
            NavigationBuilder.instance.RemoveArea(this);
    }
    public int area;
}
