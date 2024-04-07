using MalbersAnimations.Events;
using MalbersAnimations.Scriptables; 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MalbersAnimations.Reactions;

#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif
namespace MalbersAnimations.Utilities
{
    [AddComponentMenu("Malbers/Utilities/Effects - Audio/Effect Manager")]
    public class EffectManager : MonoBehaviour, IAnimatorListener
    {
        [RequiredField, Tooltip("Root Gameobject of the Hierarchy")]
        public Transform Owner;

        public List<Effect> Effects;

        public int SelectedEffect = -1;
        public bool debug;
        private void Awake()
        {
            foreach (var e in Effects)
            {
                e.Initialize();
            }
        }

        private void OnDisable()
        {
            Stop_Effects(Effects);  //Stop all effects if the Effect Manager is disabled
        }


        /// <summary>Plays an Effect using its ID value</summary>
        public virtual void PlayEffect(int ID)
        {
            List<Effect> effects = Effects.FindAll(effect => effect.ID == ID && effect.active == true);

            if (effects != null)
                foreach (var effect in effects) Play(effect);
        }


        /// <summary>Plays an Effect using its ID value</summary>
        public virtual void PlayEffect(string name)
        {
            List<Effect> effects = Effects.FindAll(effect => effect.Name == name && effect.active == true);

            if (effects != null)
                foreach (var effect in effects) Play(effect);
        }

        /// <summary>Stops an Effect using its ID value</summary>
        public virtual void StopEffect(string name)
        {
            var effects = Effects.FindAll(effect => effect.Name == name && effect.active == true);

            Stop_Effects(effects);
        }

        /// <summary>Stops an Effect using its ID value</summary>
        public virtual void StopEffect(int ID) => Effect_Stop(ID);

        /// <summary>Plays an Effect using its ID value</summary>
        public virtual void Effect_Play(int ID) => PlayEffect(ID);
        public virtual void EffectPlay(int ID) => PlayEffect(ID);
        public virtual void Effect_Play(string name) => PlayEffect(name);
        public virtual void EffectPlay(string name) => PlayEffect(name);

        /// <summary>Stops an Effect using its ID value</summary>
        public virtual void Effect_Stop(int ID)
        {
            var effects = Effects.FindAll(effect => effect.ID == ID && effect.active == true);

            Stop_Effects(effects);
        }

        private void Stop_Effects(List<Effect> effects)
        {
            if (effects != null)
            {
                foreach (var e in effects)
                {
                    StopEffect(e);
                }
            }
        }

        public virtual void StopEffect(Effect e)
        {
            //Stop the Reaction
            e.OnStopReaction?.React(Owner);
            e.OnStop.Invoke();

            e.IsPlaying = false;

            if (e.effect != null)
            {
                if (!e.effect.IsPrefab())
                {
                    if (e.disableOnStop) e.Instance?.SetActive(false);
                }
                else
                    Destroy(e.Instance);

            }

            if (debug)
                Debug.Log($"<B>{Owner.name}</B> Effect Stop: <B>[{e.Name}]</B>", this);
        }


        /// <summary>Stops an Effect using its ID value</summary>
        public virtual void Effect_Stop(string name)
        {
            var effects = Effects.FindAll(effect => effect.Name == name && effect.active == true);
            Stop_Effects(effects);
        }
 

        protected virtual void Play(Effect e)
        {
            //e.Modifier?.PreStart(e);        //Execute the Method PreStart Effect if it has a modifier

            if (e.IsPlaying) return; //Do not play a effect that is already playing

            //Delay an action
            this.Delay_Action(e.delay,
                () =>
                { 
                    e.IsPlaying = true;

                    //Play Audio
                    if (!e.Clip.NullOrEmpty() && e.audioSource != null)
                    {
                        if (e.audioSource.isPlaying) e.audioSource.Stop();

                        e.Clip.Play(e.audioSource);
                    }

                    if (e.effect != null)
                    {
                        if (e.effect.IsPrefab())                        //If instantiate is active (meaning is a prefab)
                        {
                            e.Instance = Instantiate(e.effect);         //Instantiate!
                            e.Instance.SetActive(false);

                            e.Instance.transform.localScale *= e.scale;
                        }
                        else
                        {
                            e.Instance = e.effect;                     //Use the effect as the gameobject
                        }

                        if (Owner == null) Owner = transform.root;
                        if (e.Owner == null) e.Owner = Owner;  //Save in all effects that the owner of the effects is this transform


                        if (e.Instance)
                        {
                            //Apply Offsets
                            if (e.root)
                            {
                                e.Instance.transform.position = e.root.position;

                                if (e.isChild)
                                {
                                    e.Instance.transform.parent = e.root;
                                    e.Instance.transform.localPosition = e.Offset.Position;
                                    e.Instance.transform.localEulerAngles = e.Offset.Rotation;
                                    e.Instance.transform.localScale = e.Offset.Scale; //Scale the Effect
                                }
                                else
                                {
                                    e.Instance.transform.position = e.root.TransformPoint(e.Offset.Position);
                                }

                                if (e.useRootRotation)
                                {
                                    e.Instance.transform.rotation = e.root.rotation * Quaternion.Euler(e.Offset.Rotation);     //Orient to the root rotation
                                }
                            }

                            e.Instance.SetActive(true);


                            if (e.effect.IsPrefab()) //get the trailrenderer and particle system from the Instance instead of the prefab
                            {
                                e.IsTrailRenderer = e.Instance.FindComponent<TrailRenderer>();
                                e.IsParticleSystem = e.Instance.FindComponent<ParticleSystem>();
                            }

                            if (e.IsTrailRenderer) e.IsTrailRenderer.Clear();
                            if (e.IsParticleSystem) e.IsParticleSystem.Play();
                        }
                      

                        if (e.life > 0)
                        {
                            this.Delay_Action(e.life, () => StopEffect(e));
                        }
                    }

                    if (debug)
                        Debug.Log($"<B>{Owner.name}</B> Effect Play: <B>[{e.Name}]</B>", this);

                    e.OnPlay.Invoke();                 //Invoke the Play Event
                    e.OnPlayReaction?.React(Owner);    //Play the Reaction
                }
            ); 
        }


        /// <summary>IAnimatorListener function </summary>
        public virtual bool OnAnimatorBehaviourMessage(string message, object value) => this.InvokeWithParams(message, value);

        //─────────────────────────────────CALLBACKS METHODS───────────────────────────────────────────────────────────────────

        /// <summary>Disables all effects using their name </summary>
        public virtual void Effect_Disable(string name)
        {
            List<Effect> effects = Effects.FindAll(effect => effect.Name.ToUpper() == name.ToUpper());

            if (effects != null)
            {
                foreach (var e in effects) e.active = false;
            }
            else
            {
                Debug.LogWarning("No effect with the name: " + name + " was found");
            }
        }

        /// <summary> Disables all effects using their ID</summary>
        public virtual void Effect_Disable(int ID)
        {
            List<Effect> effects = Effects.FindAll(effect => effect.ID == ID);

            if (effects != null)
            {
                foreach (var e in effects) e.active = false;
            }
            else
            {
                Debug.LogWarning("No effect with the ID: " + ID + " was found");
            }
        }

        /// <summary>Enable all effects using their name</summary>
        public virtual void Effect_Enable(string name)
        {
            List<Effect> effects = Effects.FindAll(effect => effect.Name.ToUpper() == name.ToUpper());

            if (effects != null)
            {
                foreach (var e in effects) e.active = true;
            }
            else
            {
                Debug.LogWarning("No effect with the name: " + name + " was found");
            }
        }


        /// <summary> Enable all effects using their ID</summary>
        public virtual void Effect_Enable(int ID)
        {
            List<Effect> effects = Effects.FindAll(effect => effect.ID == ID);

            if (effects != null)
            {
                foreach (var e in effects) e.active = true;
            }
            else
            {
                Debug.LogWarning("No effect with the ID: " + ID + " was found");
            }
        }

        private void Reset()
        {
            Owner = transform.root;
        }

#if UNITY_EDITOR
        [ContextMenu("Create Event Listeners")]
        void CreateListeners()
        {
            MEventListener listener = gameObject.FindComponent<MEventListener>();

            if (listener == null) listener = gameObject.AddComponent<MEventListener>();
            if (listener.Events == null) listener.Events = new List<MEventItemListener>();

            MEvent effectEnable = MTools.GetInstance<MEvent>("Effect Enable");
            MEvent effectDisable = MTools.GetInstance<MEvent>("Effect Disable");

            if (listener.Events.Find(item => item.Event == effectEnable) == null)
            {
                var item = new MEventItemListener()
                {
                    Event = effectEnable,
                    useVoid = false,
                    useString = true,
                    useInt = true
                };

                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseInt, Effect_Enable);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseString, Effect_Enable);
                listener.Events.Add(item);

                Debug.Log("<B>Effect Enable</B> Added to the Event Listeners");
            }

            if (listener.Events.Find(item => item.Event == effectDisable) == null)
            {
                var item = new MEventItemListener()
                {
                    Event = effectDisable,
                    useVoid = false,
                    useString = true,
                    useInt = true
                };

                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseInt, Effect_Disable);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseString, Effect_Disable);
                listener.Events.Add(item);

                Debug.Log("<B>Effect Disable</B> Added to the Event Listeners");
            }

            UnityEditor.EditorUtility.SetDirty(listener);
        }
#endif


    }

    [System.Serializable]
    public class Effect
    {
        public string Name = "EffectName";
        public int ID;
        public bool active = true;
        public Transform root;

        public bool isChild;
        public bool disableOnStop = true;
        public bool useRootRotation = true;
        public GameObject effect;
        public TransformOffset Offset = new TransformOffset(1);
        public AudioSource audioSource;
        public AudioClipReference Clip;

        /// <summary>Life of the Effect</summary>
        public float life = 10f;

        /// <summary>Delay Time to execute the effect after is called.</summary>
        public float delay;
        public float scale = 1f;

        ///// <summary>Scriptable Object to Modify anything you want before, during or after the effect is invoked</summary>
        //public EffectModifier Modifier;


        [SerializeReference, SubclassSelector]
        public Reaction OnPlayReaction;
        [SerializeReference, SubclassSelector]
        public Reaction OnStopReaction;

        public UnityEvent OnPlay;
        public UnityEvent OnStop;

        /// <summary>Returns the Owner of the Effect </summary>
        public Transform Owner { get; set; }

        /// <summary>  The Effect is playing. Use this to skip double playing the same effect /summary>
        public bool IsPlaying { get; set; }

        /// <summary>Returns the Instance of the Effect Prefab </summary>
        public GameObject Instance { get => instance; set => instance = value; }

        public TrailRenderer IsTrailRenderer { get; set; }
        public ParticleSystem IsParticleSystem { get; set; }

        [System.NonSerialized]
        private GameObject instance;

        internal void Initialize()
        {
            if (effect != null && !effect.IsPrefab()) //Store if the effect its not a prefab
            {
                effect.gameObject.SetActive(false); //Deactivate at start
                IsTrailRenderer = effect.FindComponent<TrailRenderer>();
                IsParticleSystem = effect.FindComponent<ParticleSystem>();
            }
        }
    }

    /// ---------------------------------------------------

    #region INSPECTOR
#if UNITY_EDITOR

    [CustomEditor(typeof(EffectManager))]
    public class EffectManagerEditor : Editor
    {
        private ReorderableList list;
        private SerializedProperty EffectList, Owner, SelectedEffect, debug;
        private EffectManager M;

        private void OnEnable()
        {
            M = ((EffectManager)target);
            Owner = serializedObject.FindProperty("Owner");
            debug = serializedObject.FindProperty("debug");
            EffectList = serializedObject.FindProperty("Effects");
            SelectedEffect = serializedObject.FindProperty("SelectedEffect");

            list = new ReorderableList(serializedObject, EffectList, true, true, true, true)
            {
                drawElementCallback = DrawElementCallback,
                drawHeaderCallback = HeaderCallbackDelegate,
                onAddCallback = OnAddCallBack,
                onSelectCallback = (list) =>
                {
                    SelectedEffect.intValue = list.index;
                }
            };

            list.index = SelectedEffect.intValue;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Manage all the Effects using the function (PlayEffect(int ID))");

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(Owner);
                MalbersEditor.DrawDebugIcon(debug);
            }


            list.DoLayoutList();

            if (list.index != -1)
            {
                Effect effect = M.Effects[list.index];

                EditorGUILayout.Space(-16);
                SerializedProperty Element = EffectList.GetArrayElementAtIndex(list.index);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(Element, new GUIContent($"[{effect.Name}]"), false);
                EditorGUI.indentLevel--;

                if (Element.isExpanded)
                {
                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        var eff = Element.FindPropertyRelative("effect");
                        eff.isExpanded = MalbersEditor.Foldout(eff.isExpanded, "General");
                        if (eff.isExpanded)
                        {

                            string prefabTooltip = "";

                            var is_Prefab = false;
                            
                            if (eff.objectReferenceValue != null) 
                                is_Prefab = (eff.objectReferenceValue as GameObject).IsPrefab();

                            if (eff.objectReferenceValue != null && is_Prefab)
                            {
                                prefabTooltip = "[Prefab]";
                            }
                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("effect"), new GUIContent("Effect " + prefabTooltip, "The Prefab or gameobject which holds the Effect(Particles, transforms)"));

                            if (is_Prefab )
                                EditorGUILayout.PropertyField(Element.FindPropertyRelative("scale"), new GUIContent("Scale", "Scale the Prefab object"));

                            if (effect.effect != null)
                                EditorGUILayout.PropertyField(Element.FindPropertyRelative("life"), new GUIContent("Life", "Duration of the Effect. The Effect will be destroyed after the Life time has passed"));

                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("delay"), new GUIContent("Delay", "Time before playing the Effect"));
                            

                            if (eff.objectReferenceValue != null && !(eff.objectReferenceValue as GameObject).IsPrefab())
                                EditorGUILayout.PropertyField(Element.FindPropertyRelative("disableOnStop"), new GUIContent("Disable On Stop", "if the Effect is not a prefab the gameOBject will be disabled"));

                            if (Element.FindPropertyRelative("life").floatValue <= 0)
                            {
                                EditorGUILayout.HelpBox("Life = 0  the effect will not be destroyed by this Script", MessageType.Info);
                            }
                        }
                    }


                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        var audio = Element.FindPropertyRelative("audioSource");
                        audio.isExpanded = MalbersEditor.Foldout(audio.isExpanded, "Audio");

                        if (audio.isExpanded)
                        {
                            EditorGUILayout.PropertyField(audio,
                                new GUIContent("Source", "Where the audio for the Effect will be player"));
                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("Clip"),
                               new GUIContent("Clip", "What audio will be played"));
                        }
                    }
                    
                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        var root = Element.FindPropertyRelative("root");
                        root.isExpanded = MalbersEditor.Foldout(root.isExpanded, "Parent");

                        if (root.isExpanded)
                        {
                            EditorGUILayout.PropertyField(root, new GUIContent("Root", "Uses this transform to position the Effect"));

                            if (root.objectReferenceValue != null)
                            {
                                var isChild = Element.FindPropertyRelative("isChild");
                                var useRootRotation = Element.FindPropertyRelative("useRootRotation");

                                EditorGUILayout.PropertyField(isChild, new GUIContent("is Child", "Set the Effect as a child of the Root transform"));

                                if (isChild.boolValue)
                                {
                                    var Offset = Element.FindPropertyRelative("Offset");
                                    EditorGUI.indentLevel++;
                                    EditorGUILayout.PropertyField(Offset, true);
                                    EditorGUI.indentLevel--;
                                }

                                EditorGUILayout.PropertyField(useRootRotation, new GUIContent("Use Root Rotation", "Orient the Effect using the root rotation."));
                            }
                        }
                    } 

                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        var OnPlayReaction = Element.FindPropertyRelative("OnPlayReaction");
                        var OnStopReaction = Element.FindPropertyRelative("OnStopReaction");
                        OnPlayReaction.isExpanded = MalbersEditor.Foldout(OnPlayReaction.isExpanded, "Reactions");

                        if (OnPlayReaction.isExpanded)
                        {

                            EditorGUILayout.PropertyField(OnPlayReaction);
                            EditorGUILayout.PropertyField(OnStopReaction);
                        }
                    }


                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        Owner.isExpanded = MalbersEditor.Foldout(Owner.isExpanded, "Events");

                        if (Owner.isExpanded)
                        {
                            var OnStop = Element.FindPropertyRelative("OnStop");
                            var OnPlay = Element.FindPropertyRelative("OnPlay");

                            EditorGUILayout.PropertyField(OnPlay);
                            EditorGUILayout.PropertyField(OnStop);
                        }
                    }
                }
            } 

            serializedObject.ApplyModifiedProperties();
        }

        void HeaderCallbackDelegate(Rect rect)
        {
            Rect R_1 = new Rect(rect.x + 14, rect.y, (rect.width - 10) / 2, EditorGUIUtility.singleLineHeight);
            Rect R_2 = new Rect(rect.x + 14 + ((rect.width - 30) / 2), rect.y, rect.width - ((rect.width) / 2), EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(R_1, "Effect List", EditorStyles.miniLabel);
            EditorGUI.LabelField(R_2, "ID", EditorStyles.centeredGreyMiniLabel);
        }

        void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = EffectList.GetArrayElementAtIndex(index);

            var e_active = element.FindPropertyRelative("active");
            var e_Name = element.FindPropertyRelative("Name");
            var e_ID = element.FindPropertyRelative("ID");

            rect.y += 2;

            Rect R_0 = new Rect(rect.x, rect.y, 15, EditorGUIUtility.singleLineHeight);
            Rect R_1 = new Rect(rect.x + 16, rect.y, (rect.width - 10) / 2, EditorGUIUtility.singleLineHeight);
            Rect R_2 = new Rect(rect.x + 16 + ((rect.width - 30) / 2), rect.y, rect.width - ((rect.width) / 2), EditorGUIUtility.singleLineHeight);

            e_active.boolValue = EditorGUI.Toggle(R_0, e_active.boolValue);
            e_Name.stringValue = EditorGUI.TextField(R_1, e_Name.stringValue, EditorStyles.label);
            e_ID.intValue = EditorGUI.IntField(R_2, e_ID.intValue);
        }

        void OnAddCallBack(ReorderableList list)
        {
            if (M.Effects == null)
            {
                M.Effects = new System.Collections.Generic.List<Effect>();
            }
            M.Effects.Add(new Effect());
        }
    }
#endif
    #endregion
}