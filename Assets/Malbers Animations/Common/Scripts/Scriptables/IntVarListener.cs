using MalbersAnimations.Scriptables;
using MalbersAnimations.Events;
using UnityEngine;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Variables/Int Listener (Local Int)")]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/secondary-components/variable-listeners-and-comparers")]
    public class IntVarListener : VarListener
    {
        public IntReference value;
        public IntEvent Raise = new IntEvent();

        public virtual int Value
        {
            get => value;
            set
            {
                if (Auto) this.value.Value = value;
                Invoke(value);
            }
        }

        void OnEnable()
        {
            if (value.Variable != null && Auto) value.Variable.OnValueChanged += Invoke;
            if (InvokeOnEnable) Invoke();
        }

        void OnDisable()
        {
            if (value.Variable != null && Auto) value.Variable.OnValueChanged -= Invoke;
        }

        public virtual void Invoke(int value) { if (Enable) Raise.Invoke(value); }
        public virtual void Invoke(float value) => Invoke((int)value);  
        public virtual void Invoke(IDs value) => Invoke(value.ID);
        public virtual void Invoke(bool value) => Invoke(value ? 1 : 0);
        public virtual void SetValue(int value) => Value = value;
        public virtual void SetValue(float value) => Value = (int)value;
        public virtual void SetValue(IDs value) => Value = value.ID;
        public virtual void SetValue(bool value) => Value = value ? 1 : 0;

        //  public virtual void InvokeInt(int value) => Invoke(value);

        public virtual void Invoke() => Invoke(Value);
    }



    //INSPECTOR
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(IntVarListener)), UnityEditor.CanEditMultipleObjects]
    public class IntVarListenerEditor : VarListenerEditor
    {
        private UnityEditor.SerializedProperty Raise;

        void OnEnable()
        {
            base.SetEnable();
            Raise = serializedObject.FindProperty("Raise");
        }

        protected override void DrawEvents() => UnityEditor.EditorGUILayout.PropertyField(Raise);
    }
#endif
}