using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif
namespace MalbersAnimations.Utilities
{
    /// <summary>
    /// Simple script to reparent a bone on enable
    /// </summary>
    [AddComponentMenu("Malbers/Utilities/Tools/Parent")]
    public class ReParent : MonoBehaviour
    {
        [RequiredField]
        [Tooltip("Reparent this gameObject to a new Transform. Use this to have more organized GameObjects on the hierarchy")]
        public Transform newParent;

        private void OnEnable()
        {
            transform.SetParent(newParent, true);
        }
        private void Reset()
        {
            newParent = transform.parent;
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(ReParent)), CanEditMultipleObjects]
    public class ReParentEditor : Editor
    {
        SerializedProperty newParent;
        private void OnEnable()
        {
            newParent = serializedObject.FindProperty("newParent");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(newParent);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}