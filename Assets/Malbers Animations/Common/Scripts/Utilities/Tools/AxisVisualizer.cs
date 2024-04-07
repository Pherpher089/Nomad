using UnityEngine;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Utilities/Gizmos/Axis Visualizer")]

    public class AxisVisualizer : MonoBehaviour
    {
#if UNITY_EDITOR
        [Min(0)] public float size = 0.5f;
        [Range(0,1)] 
        public float Opacity = 1f;

        private void OnEnable()  {} 
      
        void OnDrawGizmos()
        {
            if (!enabled) return;
            UnityEditor.Handles.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            UnityEditor.Handles.color = new Color(0, 0, 1, Opacity);  //Blue
            UnityEditor.Handles.ArrowHandleCap(0,Vector3.zero, Quaternion.identity, size, EventType.Repaint);
            UnityEditor.Handles.color = new Color(1, 0, 0, Opacity);  //red
            UnityEditor.Handles.ArrowHandleCap(0, Vector3.zero,   Quaternion.Euler(0, 90, 0), size, EventType.Repaint);
            UnityEditor.Handles.color = new Color(0, 1, 0, Opacity);  //Green
            UnityEditor.Handles.ArrowHandleCap(0, Vector3.zero,  Quaternion.Euler(-90,0, 0), size, EventType.Repaint);


            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(1, 1, 0, Opacity);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one * size * 0.1f);
            Gizmos.color = new Color(1, 1, 0, Opacity*0.5f);
            Gizmos.DrawCube(Vector3.zero, Vector3.one * size * 0.1f);


        }
#endif
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(AxisVisualizer)), UnityEditor.CanEditMultipleObjects]
    public class AxisEditor : UnityEditor.Editor
    {
        UnityEditor.SerializedProperty Opacity,  AxisSize;


        private void OnEnable()
        {
            AxisSize = serializedObject.FindProperty("size");
            Opacity = serializedObject.FindProperty("Opacity");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new GUILayout.HorizontalScope())
            {
                UnityEditor.EditorGUIUtility.labelWidth =45;
                UnityEditor.EditorGUILayout.PropertyField(AxisSize);
                UnityEditor.EditorGUILayout.PropertyField(Opacity);
                UnityEditor.EditorGUIUtility.labelWidth = 0;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

}