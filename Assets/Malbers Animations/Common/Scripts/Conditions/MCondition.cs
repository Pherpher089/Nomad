using UnityEngine;
using Object = UnityEngine.Object;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Conditions
{
    /// <summary>Conditions to Run on a Object </summary>
    [System.Serializable]
    //public abstract class MCondition<T>  where T : Object
    public abstract class MCondition : MonoBehaviour
    {
        /// <summary>Path displayed on the creation menu</summary>
        public abstract string DisplayName { get; }

        [HideInInspector, Tooltip("Name-Description of the Condition")]
        public string Name = "Condition";
        [HideInInspector, Tooltip("Inverts the result of the condition")]
        public bool invert;
        [HideInInspector, Tooltip("Or = true . And = False")]
        public bool OrAnd;
        [Tooltip("The Target will be updated when calling Set Target")]
        public bool UpdateTarget = true;

        ///// <summary>Get the Type of the Condition</summary>
        //public abstract System.Type ConditionType { get; }


        /// <summary>Evaluate a condition using the Target</summary>
        public abstract bool _Evaluate();

        /// <summary>Set target on the Conditions</summary>
        protected abstract void _SetTarget(Object target);

        public virtual void SetTarget(Object target)
        {
            if (UpdateTarget) _SetTarget(target);
        }


        /// <summary>  Checks and find the correct component to apply a reaction  </summary>  
        public void VerifyTarget<T>(Object obj, ref T component) where T : Object
        {
            //Do nothing if is the same object or is null
            if (component == obj) return; 
           

            //if the object is null then the reference is also null
            if (obj == null)
            {
                component = null;
                return;
            }

            var TType = typeof(T);

            if (TType.IsAssignableFrom(obj.GetType()))
            {
                component = obj as T;
            }
            else if (obj is GameObject)
            {
                component = (obj as GameObject).GetComponent(TType) as T;

                if (component == null)
                    component = (obj as GameObject).GetComponentInParent(TType) as T;
                if (component == null)
                    component = (obj as GameObject).GetComponentInChildren(TType) as T;
            }
            if (component == null && obj is Component)
            {
                component = (obj as Component).GetComponent(TType) as T;

                if (component == null)
                    component = (obj as Component).GetComponentInParent(TType) as T;
                if (component == null)
                    component = (obj as Component).GetComponentInChildren(TType) as T;
            }
        }

        public bool Evaluate()
        {
            return invert ? !_Evaluate() : _Evaluate();
        }


        protected virtual void OnValidate()
        {
             this.hideFlags = HideFlags.HideInInspector;
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(MCondition))]
    public class MConditionEditor : Editor
    {
        protected SerializedObject so;
        protected SerializedProperty TTarget, Condition, Value, invert;

        protected virtual void OnEnable()
        {
            so = serializedObject;
            TTarget = so.FindProperty("Target");
            Condition = so.FindProperty("Condition");
            Value = so.FindProperty("Value");

            invert = so.FindProperty("invert");
        }

        public virtual void CustomInspector() { }

        protected void Field(SerializedProperty prop)
        {
            if (prop != null) EditorGUILayout.PropertyField(prop);
        }

        protected void Field(SerializedProperty prop, GUIContent cc)
        {
            if (prop != null) EditorGUILayout.PropertyField(prop, cc);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Field(TTarget);
            Field(Condition, new GUIContent($"Condition: {(!invert.boolValue ? "[Is]" : "[Is NOT]")}"));

            CustomInspector();
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}