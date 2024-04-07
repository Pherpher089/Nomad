﻿using UnityEngine;
using MalbersAnimations.Scriptables;
using MalbersAnimations.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Controller
{
    [AddComponentMenu("Malbers/Interaction/Pickable")]
    [SelectionBase]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/secondary-components/pickable")]
    public class Pickable : MonoBehaviour, ICollectable
    {
        //  public enum CollectType { Collectable, Hold, OneUse } //For different types of collectable items? FOR ANOTHER UPDATE

        public bool Align = true;
        public bool AlignPos = true;
        [Min(0)] public float AlignTime = 0.15f;
        [Min(0)] public float AlignDistance = 1f;

        [Tooltip("Delay time after calling the Pick() method. the item will be parented to the PickUp component after this time has passed")]
        public FloatReference PickDelay = new (0);
        [Tooltip("Delay time after calling the Drop() method. the item will be unparented from the PickUp component after this time has passed")]
        public FloatReference DropDelay = new(0);
        [Tooltip("Cooldown needed to pick or drop again the collectable")]
        public FloatReference coolDown = new(0f);
        [Tooltip("When an Object is Collectable it means that the Picker can still pick objects, the item was collected by another component (E.g. Weapons or Inventory)")]

        public BoolReference m_Collectable = new(false);
        [Tooltip("The Pick Up Drop Logic will be called via animator events/messages. Use These methods on the Animator: TryPick(), TryDrop(), TryPickUpDrop()")]
        public BoolReference m_ByAnimation = new(false);
        [Tooltip("The Pick Up Drop Logic will be called via animator events/messages")]
        public BoolReference m_DestroyOnPick = new(false);
        //[Tooltip("Unparent the Pickable, so it does not have any Transform parents.")]
        //public BoolReference SceneRoot = new BoolReference(true);

        [Tooltip(" Amount Pickable Item can store.. that it can be use for anything")]
        public IntReference m_Amount = new(1); //Not done yet
        [Tooltip("The Pick Up Drop Logic will be called via animator events/messages")]

        public BoolReference m_AutoPick = new(false); //Not done yet
        public IntReference m_ID = new ();         //Not done yet

        /// <summary>Who Did the Picking </summary>
        public MPickUp Picker { get; set; }


        public BoolEvent OnFocused = new();
        public GameObjectEvent OnFocusedBy = new();
        public GameObjectEvent OnPicked = new();
        public GameObjectEvent OnPrePicked = new();
        public GameObjectEvent OnDropped = new();
        public GameObjectEvent OnPreDropped = new();

        [SerializeField] private Rigidbody rb;
        [RequiredField] public Collider[] m_colliders;

        private float currentPickTime;

        /// <summary>Is this Object being picked </summary>
        public bool IsPicked { get; set; }

        /// <summary>Current value of the Item</summary>
        public int Amount { get => m_Amount.Value; set => m_Amount.Value = value; }

        /// <summary>The Item will be autopicked if the Picker is focusing it</summary>
        public bool AutoPick { get => m_AutoPick.Value; set => m_AutoPick.Value = value; }
        public bool Collectable { get => m_Collectable.Value; set => m_Collectable.Value = value; }
        public Rigidbody RigidBody => rb;

        /// <summary>The Pick Up Drop Logic will be called via animator events/messages</summary>
        public bool ByAnimation { get => m_ByAnimation.Value; set => m_ByAnimation.Value = value; }
        public bool DestroyOnPick { get => m_DestroyOnPick.Value; set => m_DestroyOnPick.Value = value; }
        public bool InCoolDown => !MTools.ElapsedTime(CurrentPickTime, coolDown);
        public int ID { get => m_ID.Value; set => m_ID.Value = value; }

        private Vector3 DefaultScale;


        private bool focused;
        public bool Focused
        {
            get => focused;
            private set
            {
                focused = value;
                OnFocused.Invoke(focused);
            }
        }

        public void SetFocused(GameObject FocusBy)
        {
            if (FocusBy)
            {
                Focused = true;
                OnFocusedBy.Invoke(FocusBy);
            }
            else
            {
                Focused = false;
                OnFocusedBy.Invoke(null);
            }
        }



        /// <summary>Game Time the Pickable was Picked</summary>
        public float CurrentPickTime { get => currentPickTime; set => currentPickTime = value; }

        private void OnDisable()
        {
            Focused = false;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            if (m_colliders == null || m_colliders.Length == 0) m_colliders = GetComponents<Collider>();

            CurrentPickTime = -coolDown;

            DefaultScale = transform.localScale;
        }

        public virtual void Pick()
        {
            DisablePhysics();                       //Disable all physics when the item is picked
            IsPicked = !Collectable;                //Check if the Item is collectable 
            Focused = false;                        //Unfocus the Item
            OnFocusedBy.Invoke(null);

            //Weapons can be picked witout having a picker
            OnPicked.Invoke(Picker ? Picker.Root.gameObject : null);     //Call the Event
            CurrentPickTime = Time.time;            //Store the time it was picked
            if (Collectable) enabled = false;
        }

        public virtual void Drop()
        {
            EnablePhysics();
            IsPicked = false;
            enabled = true;

            transform.parent = null;                                //UnParent
            transform.localScale = DefaultScale;                    //Restore the Scale
            OnDropped.Invoke(Picker ? Picker.Root.gameObject : null);
            Picker = null;                                          //Reset who did the picking
            CurrentPickTime = Time.time;
        }

        /// <summary> Call this in case a picker has still the item </summary>
        public virtual void ForceDrop()
        {
            Picker?.DropItem();
        }

        public void DisablePhysics()
        {
            if (RigidBody)
            {
                RigidBody.useGravity = false;

#if !UNITY_2022_3_OR_NEWER
                RigidBody.velocity = Vector3.zero;
#endif
                RigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                RigidBody.isKinematic = true;
            }

            foreach (var c in m_colliders)
            {
                if (c) c.enabled = false; //Disable all colliders
            }
        }

        public void EnablePhysics()
        {
            if (RigidBody)
            {
                RigidBody.useGravity = true;
                RigidBody.isKinematic = false;
                RigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                //THIS CAUSES ISSUES WITH THROWNING OBJECTS ... (CHECK THE WEAPON PROBLEM)
                //this.Delay_Action(() =>
                //{
                //    RigidBody.angularVelocity = Vector3.zero;
                //    RigidBody.velocity = Vector3.zero;
                //}
                //);
            }

            foreach (var c in m_colliders) c.enabled = true; //Enable all colliders
        }

        [HideInInspector] public int EditorTabs = 0;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (Align)
            {
                UnityEditor.Handles.color = Color.yellow;
                UnityEditor.Handles.DrawWireDisc(transform.position, transform.up, AlignDistance);
            }
        }

        public void SetEnable(bool enable)
        {
            this.enabled = enable;
        }
#endif
    }


    //INSPECTOR
#if UNITY_EDITOR
    [CustomEditor(typeof(Pickable)), CanEditMultipleObjects]
    public class PickableEditor : Editor
    {
        private SerializedProperty //   PickAnimations, PickUpMode, PickUpAbility, DropMode, DropAbility,DropAnimations, 
            Align, AlignTime, AlignDistance, AlignPos, EditorTabs,
            m_AutoPick, DropDelay, PickDelay, rb, CoolDown,// SceneRoot,
            OnFocused, OnFocusedBy,
            OnPrePicked, OnPicked, OnDropped, OnPreDropped, /*ShowEvents, */Amount, IntID, m_collider, m_Collectable, m_ByAnimation, m_DestroyOnPick;

        private Pickable m;

        protected string[] Tabs1 = new string[] { "General", "Events" };

        private void OnEnable()
        {
            m = (Pickable)target;

            EditorTabs = serializedObject.FindProperty("EditorTabs");
            //SceneRoot = serializedObject.FindProperty("SceneRoot");
            rb = serializedObject.FindProperty("rb");
            PickDelay = serializedObject.FindProperty("PickDelay");
            DropDelay = serializedObject.FindProperty("DropDelay");
            m_Collectable = serializedObject.FindProperty("m_Collectable");
            m_ByAnimation = serializedObject.FindProperty("m_ByAnimation");
            m_DestroyOnPick = serializedObject.FindProperty("m_DestroyOnPick");


            Align = serializedObject.FindProperty("Align");
            AlignTime = serializedObject.FindProperty("AlignTime");
            AlignDistance = serializedObject.FindProperty("AlignDistance");
            OnFocused = serializedObject.FindProperty("OnFocused");
            OnFocusedBy = serializedObject.FindProperty("OnFocusedBy");
            OnPicked = serializedObject.FindProperty("OnPicked");
            OnPrePicked = serializedObject.FindProperty("OnPrePicked");
            OnDropped = serializedObject.FindProperty("OnDropped");
            OnPreDropped = serializedObject.FindProperty("OnPreDropped");
            //ShowEvents = serializedObject.FindProperty("ShowEvents");
            Amount = serializedObject.FindProperty("m_Amount");
            IntID = serializedObject.FindProperty("m_ID");
            m_collider = serializedObject.FindProperty("m_colliders");
            AlignPos = serializedObject.FindProperty("AlignPos");
            //Collectable = serializedObject.FindProperty("Collectable");
            m_AutoPick = serializedObject.FindProperty("m_AutoPick");
            CoolDown = serializedObject.FindProperty("coolDown");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription("Pickable - Collectable object");
            EditorTabs.intValue = GUILayout.Toolbar(EditorTabs.intValue, Tabs1);
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (Application.isPlaying)
                {
                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        EditorGUILayout.ToggleLeft("Is Picked", m.IsPicked);
                        EditorGUILayout.ToggleLeft("Is Focused", m.Focused);
                    }
                }

                if (EditorTabs.intValue == 0) DrawGeneral();
                else DrawEvents();
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawEvents()
        {
            EditorGUILayout.PropertyField(OnFocused);
            EditorGUILayout.PropertyField(OnFocusedBy);
            if (m.PickDelay > 0 || m.m_ByAnimation.Value)
                EditorGUILayout.PropertyField(OnPrePicked, new GUIContent("On Pre-Picked By"));
            EditorGUILayout.PropertyField(OnPicked, new GUIContent("On Picked By"));
            if (m.DropDelay > 0 || m.m_ByAnimation.Value)
                EditorGUILayout.PropertyField(OnPreDropped, new GUIContent("On Pre-Dropped By"));
            EditorGUILayout.PropertyField(OnDropped, new GUIContent("On Dropped By"));

        }

        private void DrawGeneral()
        {
            m_AutoPick.isExpanded = MalbersEditor.Foldout(m_AutoPick.isExpanded, "Pickable Item");

            if (m_AutoPick.isExpanded)
            {
                EditorGUILayout.PropertyField(IntID, new GUIContent("ID", "Int value the Pickable Item can store. This ID is used by the Picker component to Identify each Pickable Object"));
                EditorGUILayout.PropertyField(Amount);

                EditorGUILayout.PropertyField(m_AutoPick, new GUIContent("Auto", "The Item will be Picked Automatically"));
                EditorGUILayout.PropertyField(m_ByAnimation,
                    new GUIContent("Use Animation", "The Item will Pre-Picked/Dropped by the Picker Animator." +
                    " Pick-Drop Logic is called by Animation Event or Animator Message Behaviour.\nUse the Methods: TryPickUpDrop(); TryPickUp(); TryDrop();"));

             //   EditorGUILayout.PropertyField(SceneRoot);


                EditorGUILayout.PropertyField(m_Collectable, new GUIContent("Collectable", "The Item will Picked by the Pickable and it will be stored"));
                if (m.Collectable)
                    EditorGUILayout.PropertyField(m_DestroyOnPick, new GUIContent("Destroy Collectable", "The Item will be destroyed after is picked"));
            }

            rb.isExpanded = MalbersEditor.Foldout(rb.isExpanded, "References");
            if (rb.isExpanded)
            {
                EditorGUILayout.PropertyField(rb, new GUIContent("Rigid Body"));
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_collider);
                EditorGUI.indentLevel--;
            }





            CoolDown.isExpanded = MalbersEditor.Foldout(CoolDown.isExpanded, "Delays");

            if (CoolDown.isExpanded)
            {
                EditorGUILayout.PropertyField(CoolDown);
                EditorGUILayout.PropertyField(PickDelay);
                EditorGUILayout.PropertyField(DropDelay);
            }


            Align.isExpanded = MalbersEditor.Foldout(Align.isExpanded, "Alignment");

            if (Align.isExpanded)
            {
                EditorGUILayout.PropertyField(Align, new GUIContent("Align On Pick", "Align the character to the Item"));

                if (Align.boolValue)
                {

                    using (new GUILayout.HorizontalScope())
                    {

                        EditorGUILayout.PropertyField(AlignPos, new GUIContent("Align Pos", "align the Position"));

                        EditorGUIUtility.labelWidth = 60;
                        EditorGUILayout.PropertyField
                            (AlignDistance, new GUIContent("Distance", "Distance to move the Animal towards the Item"), GUILayout.MinWidth(50));
                        EditorGUIUtility.labelWidth = 0;
                    }
                    EditorGUILayout.PropertyField(AlignTime, new GUIContent("Time", "Time required to do the alignment"));
                }
            }


        }
    }
#endif
}