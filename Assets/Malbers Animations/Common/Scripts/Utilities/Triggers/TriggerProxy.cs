using UnityEngine;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Utilities
{
    /// <summary>
    /// This is used when the collider is in a different gameObject and you need to check the Collider Events
    /// Create this component at runtime and subscribe to the UnityEvents
    /// </summary>
    [AddComponentMenu("Malbers/Utilities/Colliders/Trigger Proxy")]
    public class TriggerProxy : MonoBehaviour
    {
      
        [Tooltip("Hit Layer for the Trigger Proxy")]
        [SerializeField] private LayerReference hitLayer = new(-1);
        public LayerMask Layer { get => hitLayer.Value; set => hitLayer.Value = value; }


        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;
        [Tooltip("Search only Tags")]
        public Tag[] Tags;

        public ColliderEvent OnTrigger_Enter = new();
        public ColliderEvent OnTrigger_Exit =  new();
        public ColliderEvent OnTrigger_Stay =  new();

        public GameObjectEvent OnGameObjectEnter =new();
        public GameObjectEvent OnGameObjectExit = new();
        public GameObjectEvent OnGameObjectStay = new();
        public UnityEvent OnEmpty = new();

        [SerializeField] private bool m_debug = false;

        public BoolReference useOnTriggerStay = new();

        [Tooltip("Trigger will be disabled the first time")]
        public BoolReference OneTimeUse = new();


        internal List<Collider> m_colliders = new();
        /// <summary>All the Gameobjects using the Proxy</summary>
        internal List<GameObject> EnteringGameObjects = new();

        public Action<GameObject, Collider> EnterTriggerInteraction = delegate { };
        public Action<GameObject, Collider> ExitTriggerInteraction = delegate { };

        /// <summary> Is this component enabled? /summary>
        public bool Active { get => enabled; set => enabled = value; }

        //public int ID { get => m_ID.Value; set => m_ID.Value = value; }
      
        public QueryTriggerInteraction TriggerInteraction { get => triggerInteraction; set => triggerInteraction = value; }

        /// <summary> Collider Component used for the Trigger Proxy </summary>
        [RequiredField] public Collider trigger;
        public Transform Owner { get; set; }

        public bool TrueConditions(Collider other)
        {
            if (!Active) return false;

            if (Tags != null && Tags.Length > 0)
            {
                if (!other.gameObject.HasMalbersTagInParent(Tags)) return false;
            }
           
            if (trigger == null) return false; // You don't have a trigger
            if (other == null) return false; // you are CALLING A ELIMINATED ONE

            if (triggerInteraction == QueryTriggerInteraction.Ignore && other.isTrigger) return false; // Check Trigger Interactions 

            if (!MTools.Layer_in_LayerMask(other.gameObject.layer, Layer)) return false;
            if (transform.IsChildOf(other.transform)) return false;                 // Do not Interact with yourself
            if (Owner != null && other.transform.IsChildOf(Owner)) return false;    // Do not Interact with yourself

            return true;
        }
        

        public void OnTriggerEnter(Collider other)
        {
            if (TrueConditions(other))
            {
                GameObject realRoot = FindRealRoot(other);

                OnTrigger_Enter.Invoke(other); //Invoke when a Collider enters the Trigger

                if (m_debug) Debug.Log($"<b>{name}</b> [Entering Collider] -> [{other.name}]", this);

                ////Check Recently destroyed Colliders (Strange bug)
                //CheckMissingColliders();

                if (m_colliders.Find(coll => coll == other) == null)                               //if the entering collider is not already on the list add it
                {
                    m_colliders.Add(other);
                    AddTarget(other);
                }

                if (EnteringGameObjects.Contains(realRoot))
                {
                    return;
                }
                else
                {
                    EnterTriggerInteraction(realRoot, other);
                    EnteringGameObjects.Add(realRoot); 
                    OnGameObjectEnter.Invoke(realRoot);
                    if (m_debug) Debug.Log($"<b>{name}</b> [Entering GameObject] -> [{realRoot.name}]", this);

                    if (OneTimeUse.Value) enabled = false;
                }
            }
        }
        public void OnTriggerExit(Collider other) => TriggerExit(other, true);

        public void TriggerExit(Collider other, bool remove)
        {
            if (TrueConditions(other))
            {
                RemoveTrigger(other, remove);
            }
        }

        public void RemoveTrigger(Collider other, bool remove)
        {
            GameObject realRoot = FindRealRoot(other);

            OnTrigger_Exit.Invoke(other);

            m_colliders.Remove(other);
            RemoveTarget(other, remove);

            if (m_debug) Debug.Log($"<b>{name}</b> [Exit Collider] -> [{other.name}]", this);



            if (EnteringGameObjects.Contains(realRoot))             //Means that the Entering GameObject still exist
            {
                if (!m_colliders.Exists(c => c != null && c.transform.SameHierarchy(realRoot.transform))) //Means that all that root colliders are out
                {
                    EnteringGameObjects.Remove(realRoot);
                    OnGameObjectExit.Invoke(realRoot);
                    ExitTriggerInteraction(realRoot, other);

                    if (m_debug) Debug.Log($"<b>{name}</b> [Leaving Gameobject] -> [{realRoot.name}]", this);
                }
            }

            if (m_colliders.Count == 0) ResetTrigger();
            //CheckMissingColliders();
        }

        private static GameObject FindRealRoot(Collider other)
        {
            var realRoot = other.transform.root.gameObject;        //Get the animal on the entering collider

            //Find the Right Root if the objets is a Malbers Core Object in Parent
            var coreRoot = other.GetComponentInParent<IObjectCore>(false);

            if (coreRoot != null)
            {
                realRoot = coreRoot.transform.gameObject;
            }
            //Means the Root is not on the real root since its not on the search layer
            else if (realRoot.layer != other.gameObject.layer)
            {
                realRoot = MTools.FindRealParentByLayer(other.transform);
            }

            return realRoot;
        }

        /// <summary>Check Recently destroyed Colliders (Strange bug)</summary>
        private void CheckMissingColliders()
        {
            for (var i = m_colliders.Count - 1; i > -1; i--)
            {
                if (m_colliders[i] == null || !m_colliders[i].enabled) m_colliders.RemoveAt(i);
            }

            if (m_colliders.Count == 0)
            {
                EnteringGameObjects = new();
            }
        }
        /// <summary>Add a Trigger Target to every new Collider found</summary>
        private void AddTarget(Collider other)
        {
            TriggerTarget.set ??= new();

            var TT = TriggerTarget.set.Find(x => x.m_collider == other) ?? other.gameObject.AddComponent<TriggerTarget>();
           
            TT.AddProxy(this, other);
        }

     

        /// <summary>OnTrigger exit Logic</summary>
  

        internal void RemoveTarget(Collider other, bool remove)
        {
            var TT = TriggerTarget.set.Find(x => x.m_collider == other);

            if (TT)
            {
                if (remove)
                    TT.RemoveProxy(this);
            }
        }
  

        public void ResetTrigger()
        {
            m_colliders = new List<Collider>();
            EnteringGameObjects = new List<GameObject>();
            OnEmpty.Invoke();
        }

        private void OnDisable()
        {
            if (m_colliders.Count > 0)
            {
                foreach (var c in m_colliders)
                {
                    if (c)
                    {
                        OnTrigger_Exit.Invoke(c); //the colliders may be destroyed
                        RemoveTarget(c, true);
                    }
                }
            }

            if (EnteringGameObjects.Count > 0)
            {
                foreach (var c in EnteringGameObjects)
                {

                    if (c) OnGameObjectExit.Invoke(c);  //the gameobjects  may be destroyed
                }
            }

            if (m_debug) Debug.Log($"<b>{name}</b> [Exit All Colliders and Triggers] ",this);

            ResetTrigger();
        }

        private void OnEnable() => ResetTrigger();

        private void Awake()
        {
            if (trigger == null) trigger = GetComponent<Collider>();

            if (trigger) trigger.isTrigger = true;
            else
                Debug.LogWarning("This Script requires a Collider, please add any type of collider", this);

            if (Owner == null) Owner = transform;

            ResetTrigger();
        }


        private void Update()
        {
            CheckOntriggerStay();
        }
        void CheckOntriggerStay()
        {
            if (useOnTriggerStay.Value)
            {
                foreach (var gos in EnteringGameObjects)
                {
                    OnGameObjectStay.Invoke(gos);
                }

                foreach (var col in m_colliders)
                {
                    OnTrigger_Stay.Invoke(col);
                }
            }
        }

        public void SetLayer(LayerMask mask, QueryTriggerInteraction triggerInteraction, Transform Owner, Tag[] tags = null)
        {
            TriggerInteraction = triggerInteraction;
            Tags = tags;
            Layer = mask;
            this.Owner = Owner;
        }


        public static TriggerProxy CheckTriggerProxy(Collider trigger, LayerMask Layer, QueryTriggerInteraction TriggerInteraction, Transform Owner)
        {
            TriggerProxy Proxy = null;
            if (trigger != null)
            {
                Proxy = trigger.GetComponent<TriggerProxy>();

                if (Proxy == null)
                {
                    Proxy = trigger.gameObject.AddComponent<TriggerProxy>();

                    Proxy.SetLayer(Layer, TriggerInteraction, Owner);
                   // Proxy.hideFlags = HideFlags.HideInInspector;
                }
                else
                {
                    Proxy.Layer = Proxy.Layer | Layer; //combine both layers
                }
                if (TriggerInteraction != QueryTriggerInteraction.Ignore) Proxy.TriggerInteraction = TriggerInteraction;

                trigger.gameObject.SetLayer(2, false); //Force the Trigger Area to be on the Ignore Raycast Layer
                trigger.isTrigger = true;

                Proxy.Active = true;
            }

            return Proxy;
        }

       

        [HideInInspector] public int Editor_Tabs1;
    }


    #region Inspector


#if UNITY_EDITOR
    [CanEditMultipleObjects, CustomEditor(typeof(TriggerProxy))]
    public class TriggerProxyEditor : Editor
    {
        SerializedProperty debug, OnTrigger_Enter, OnTrigger_Exit, OnEmpty, useOnTriggerStay, OnTrigger_Stay, Editor_Tabs1, OneTimeUse,
            triggerInteraction, hitLayer, OnGameObjectEnter, OnGameObjectExit, OnGameObjectStay, Tags;

        TriggerProxy m;

        protected string[] Tabs1 = new string[] { "General", "Events" };

        private void OnEnable()
        {
            m = (TriggerProxy)target;
            OnEmpty = serializedObject.FindProperty("OnEmpty");
            triggerInteraction = serializedObject.FindProperty("triggerInteraction");
            useOnTriggerStay = serializedObject.FindProperty("useOnTriggerStay");
            hitLayer = serializedObject.FindProperty("hitLayer");
            debug = serializedObject.FindProperty("m_debug");
            OnTrigger_Enter = serializedObject.FindProperty("OnTrigger_Enter");
            OnTrigger_Exit = serializedObject.FindProperty("OnTrigger_Exit");
            OnGameObjectEnter = serializedObject.FindProperty("OnGameObjectEnter");
            OnGameObjectExit = serializedObject.FindProperty("OnGameObjectExit");
            Tags = serializedObject.FindProperty("Tags");
            OnGameObjectStay = serializedObject.FindProperty("OnGameObjectStay");
            OnTrigger_Stay = serializedObject.FindProperty("OnTrigger_Stay");
            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");
            OneTimeUse = serializedObject.FindProperty("OneTimeUse");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription("Use this component to do quick OnTrigger Enter/Exit logics");

            Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, Tabs1);
            if (Editor_Tabs1.intValue == 0) DrawGeneral();
            else DrawEvents();
            if (Application.isPlaying && debug.boolValue)
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);

                        //   EditorGUILayout.ObjectField("Own Collider", m.trigger, typeof(Collider), false);

                        EditorGUILayout.LabelField("GameObjects (" + m.EnteringGameObjects.Count + ")", EditorStyles.boldLabel);
                        foreach (var item in m.EnteringGameObjects)
                        {
                            if (item != null) EditorGUILayout.ObjectField(item.name, item, typeof(GameObject), false);
                        }

                        EditorGUILayout.LabelField("Colliders (" + m.m_colliders.Count + ")", EditorStyles.boldLabel);

                        foreach (var item in m.m_colliders)
                        {
                            if (item != null) EditorGUILayout.ObjectField(item.name, item, typeof(Collider), false);
                        }

                        //EditorGUILayout.LabelField("Targets (" + m.TriggerTargets.Count + ")", EditorStyles.boldLabel);

                        //foreach (var item in m.TriggerTargets)
                        //{
                        //    if (item != null) EditorGUILayout.ObjectField(item.name, item, typeof(Collider), false);
                        //}
                    }
                    Repaint();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGeneral()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(hitLayer, new GUIContent("Layer"));
                    MalbersEditor.DrawDebugIcon(debug);
                }

                EditorGUILayout.PropertyField(triggerInteraction);
                EditorGUILayout.PropertyField(useOnTriggerStay);
                EditorGUILayout.PropertyField(OneTimeUse);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(Tags, true);
                EditorGUI.indentLevel--;
            }
        }



        private void DrawEvents()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(OnTrigger_Enter, new GUIContent("On Trigger Enter"));
                EditorGUILayout.PropertyField(OnTrigger_Exit, new GUIContent("On Trigger Exit"));
                EditorGUILayout.PropertyField(OnEmpty);
                if (m.useOnTriggerStay.Value)
                    EditorGUILayout.PropertyField(OnTrigger_Stay, new GUIContent("On Trigger Stay"));


                EditorGUILayout.PropertyField(OnGameObjectEnter, new GUIContent("On GameObject Enter "));
                EditorGUILayout.PropertyField(OnGameObjectExit, new GUIContent("On GameObject Exit"));
                if (m.useOnTriggerStay.Value)
                    EditorGUILayout.PropertyField(OnGameObjectStay, new GUIContent("On GameObject Stay"));
            }
        }
    }
#endif
    #endregion
}