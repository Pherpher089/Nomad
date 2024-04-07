﻿using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Utilities/Gizmos/Gizmo Visualizer")]

    public class GizmoVisualizer : MonoBehaviour
    {
#if UNITY_EDITOR
        public enum GizmoType
        {
            Cube,
            Sphere,
        }
        public bool UseColliders;
        public GizmoType gizmoType;

        [Min(0)] public float debugSize = 0.03f;
        public Color DebugColor = Color.blue;
        public bool DrawAxis;
        [Min(0)] public float AxisSize = 0.65f;

        private Collider _collider;



        //public StatModifier modifier;

        Collider _Collider
        {
            get
            {
                if (_collider == null)
                {
                    _collider = GetComponent<Collider>();
                }
                return _collider;
            }
        }

        [ContextMenu("Get Gizmo Color")]
        private void GetGizmoColor()
        {
            Debug.Log($"{name}: GizmoColor: {DebugColor}");
        }

        private void Reset()
        {
            if (_Collider) UseColliders = true;
            DebugColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        }


        private void OnEnable() { }

#if UNITY_EDITOR && MALBERS_DEBUG
        void OnDrawGizmos()
        {


            if (!enabled) return;

            var DebugColorWire = new Color(DebugColor.r, DebugColor.g, DebugColor.b, 1);

            if (DrawAxis)
            {
                UnityEditor.Handles.color = DebugColor;
                UnityEditor.Handles.ArrowHandleCap(0, transform.position, transform.rotation, AxisSize, EventType.Repaint);
            }

            Gizmos.matrix = transform.localToWorldMatrix;

            if (_Collider && UseColliders)
            {
                UsesColliders(false);
                return;
            }

            switch (gizmoType)
            {
                case GizmoType.Cube:
                    Gizmos.color = DebugColorWire;
                    Gizmos.DrawWireCube(Vector3.zero, Vector3.one * debugSize);
                    Gizmos.color = DebugColor;
                    Gizmos.DrawCube(Vector3.zero, Vector3.one * debugSize);
                    break;
                case GizmoType.Sphere:
                    Gizmos.color = DebugColorWire;
                    Gizmos.DrawWireSphere(Vector3.zero, debugSize);
                    Gizmos.color = DebugColor;
                    Gizmos.DrawSphere(Vector3.zero, debugSize);
                    break;
                default:
                    break;
            }
        }

        void OnDrawGizmosSelected()
        {
            if (!enabled) return;
            Gizmos.color = new Color(1, 1, 0, 1);
            Gizmos.matrix = transform.localToWorldMatrix;

            if (UseColliders && _Collider)
            {
                UsesColliders(true);
                return;
            }


            switch (gizmoType)
            {
                case GizmoType.Cube:
                    Gizmos.DrawWireCube(Vector3.zero, Vector3.one * debugSize);
                    break;
                case GizmoType.Sphere:
                    Gizmos.DrawWireSphere(Vector3.zero, debugSize);
                    break;
            }
        }
#endif


        void UsesColliders(bool sel)
        {
            var DebugColorWire = new Color(DebugColor.r, DebugColor.g, DebugColor.b, 1);
            if (sel) DebugColorWire = Color.yellow;

            if (_Collider is BoxCollider)
            {
                BoxCollider _C = _Collider as BoxCollider;
                if (!_C.enabled) return;

                Gizmos.matrix = transform.localToWorldMatrix;

                var pos = _C.center;
                var sca = _C.size;

                Gizmos.color = DebugColorWire;

                Gizmos.DrawWireCube(pos, sca);

                if (!sel)
                {
                    Gizmos.color = DebugColor;
                    Gizmos.DrawCube(pos, sca);
                }

               

            }
            else if (_Collider is SphereCollider)
            {
                SphereCollider _C = _Collider as SphereCollider;

                if (!_C.enabled) return;

                Gizmos.matrix = transform.localToWorldMatrix;

                Gizmos.color = DebugColorWire;
                Gizmos.DrawWireSphere(_C.center, _C.radius);

                if (!sel)
                {
                    Gizmos.color = DebugColor;
                    Gizmos.DrawSphere(_C.center, _C.radius);
                }
            }
        }
#endif
        }


#if UNITY_EDITOR
    [CustomEditor(typeof(GizmoVisualizer)), CanEditMultipleObjects]
    public class GizmoVisualizerEditor : Editor
    {

        SerializedProperty UseColliders, gizmoType, debugSize, DebugColor, DrawAxis, AxisSize;


        private void OnEnable()
        {
            UseColliders = serializedObject.FindProperty("UseColliders");
            gizmoType = serializedObject.FindProperty("gizmoType");
            debugSize = serializedObject.FindProperty("debugSize");
            DebugColor = serializedObject.FindProperty("DebugColor");
            DrawAxis = serializedObject.FindProperty("DrawAxis");
            AxisSize = serializedObject.FindProperty("AxisSize");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(UseColliders);
                EditorGUILayout.PropertyField(DebugColor, GUIContent.none, GUILayout.MaxWidth(100));
            }

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(DrawAxis);
                if (DrawAxis.boolValue)
                {
                    EditorGUIUtility.labelWidth = 30;
                    EditorGUILayout.PropertyField(AxisSize, new GUIContent("Size"), GUILayout.MaxWidth(100), GUILayout.MinWidth(70));
                    EditorGUIUtility.labelWidth = 0;
                }
            }

            if (!UseColliders.boolValue)
            {
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(gizmoType);
                    EditorGUIUtility.labelWidth = 30;
                    EditorGUILayout.PropertyField(debugSize, new GUIContent("Size"), GUILayout.MaxWidth(100), GUILayout.MinWidth(70));
                    EditorGUIUtility.labelWidth = 0;
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}