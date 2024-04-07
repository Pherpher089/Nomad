using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations.Scriptables;
using UnityEngine.Serialization;
using MalbersAnimations.Reactions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Controller
{
    /// <summary>When an animal Enter a Zone this will activate a new State or a new Mode </summary>
    [AddComponentMenu("Malbers/Animal Controller/Zone")]
    public class Zone : MonoBehaviour, IZone
    {
        public bool debug;

        [Tooltip("As soon as the animal enters the zone it will execute the logic. If False then Call the Method Zone.Activate()")]
        public BoolReference automatic = new();

        [Tooltip("How many characters can use this zone at the same time.\nNegative values: The zone has no character limit")]
        public IntReference Limit = new(-1);



        [Tooltip("Disable the Zone after it was used")]
        /// <summary>Disable the Zone after it was used</summary>
        public BoolReference DisableAfterUsed = new();


        //[Tooltip("Use it to re-eneable the zone after finishing a mode")]
        ///// <summary>Disable the Zone after it was used</summary>
        //public BoolReference reEnableOnModeEnd = new BoolReference();

        [FormerlySerializedAs("HeadOnly")]
        /// <summary>Use the Trigger for Heads only</summary>
        public bool BoneOnly;



        /// <summary>Limit the Activation of the Zone of an angle from the Animal</summary>
        [Range(0, 360), Tooltip("Limit the Activation of the Zone of an angle from the Animal")]
        public float Angle = 360;


        [Range(0, 1), Tooltip("Probability to Activate the Zone")]
        public float Weight = 1;

        [Tooltip("The Zone can be used in both sides")]
        public bool DoubleSide = false;
        [Tooltip("Flip the Angle")]
        public bool Flip = false;

        public bool ShowActionID = true;

        [FormerlySerializedAs("HeadName")]
        public string BoneName = "Head";


        [Tooltip("Choose between a Mode, State or Stance for the Zone")]
        public ZoneType zoneType = ZoneType.Mode;

        [Tooltip("Actions to do on a state when entering a zone")]
        public StateAction stateAction = StateAction.Activate;
        [Tooltip("Actions to do on a state when exiting a zone")]
        public StateAction stateActionExit = StateAction.None;

        public StanceAction stanceActionEnter = StanceAction.Activate;
        public StanceAction stanceActionExit = StanceAction.Exit;


        [Tooltip("Layer to detect the Animal")]
        public LayerReference Layer = new(1048576); //Animal Layer

        [Tooltip("State Status. If is set to [-1] the Status will be ignored")]
        public IntReference stateStatus = new(0);

        [SerializeField] private List<Tag> tags;

        public ModeID modeID;
        public StateID stateID;

        public StanceID stanceID;
        public MAction ActionID;
        /// <summary> Mode Index Value</summary>
        [SerializeField] private IntReference modeIndex = new(-99);

        /// <summary>Mode Ability Index</summary> 
        public int ModeAbilityIndex => modeID.ID == 4 ? ActionID.ID : modeIndex.Value;
        //public int ModeAbilityIndex =>/* modeID.ID == 4 ? ActionID.ID : */modeIndex.Value;

        /// <summary>ID of the Zone regarding the Type of Zone(State,Stance,Mode) </summary> 
        public int ZoneID { get;private set; }

        [Tooltip("Value of the Ability Status")]
        public AbilityStatus m_abilityStatus = AbilityStatus.PlayOneTime;
        [Tooltip("Time of Ability Activation")]
        public float AbilityTime = 3f;


        [Tooltip("Amount of Force that will be applied to the Animal")]
        public FloatReference Force = new(10);
        [Tooltip("Aceleration to applied the Force when the Animal enters the zone")]
        [FormerlySerializedAs("EnterDrag")]
        public FloatReference EnterAceleration = new(2);
        [Tooltip("Exit Drag to decrease the Force when the Animal exits the zone")]
        public FloatReference ExitDrag = new(4);

        [Tooltip("Limit the Current Force the animal may have")]
        [FormerlySerializedAs("Bounce")]
        public FloatReference LimitForce = new(8);

        [Tooltip("Change if the Animal is Grounded when entering the Force Zone")]
        public BoolReference ForceGrounded = new();

        [Tooltip("Can the Animal be controller while on the Air?")]
        public BoolReference ForceAirControl = new(true);

        [Tooltip("Plays a mode no matter if another mode is already playing")]
        public bool ForceMode = false;

        [Tooltip("Extra conditions to check in case you want to activate the Zone")]
        public Conditions.MConditions CheckConditions;

        /// <summary>Currents Animals inside the zone</summary>
        public HashSet<MAnimal> AnimalsInZone { get; internal set; }

        /// <summary>Currents Animals using the zone</summary>
        public HashSet<MAnimal> AnimalsUsingZone { get; internal set; }

        public MAnimal JustExitAnimal;

        /// <summary>List of all collliders entering the Zone</summary>
        internal List<Collider> m_Colliders = new();

        [Tooltip("Value Assigned to the Mode Float Value when using the Mode Zone")]
        [Min(0)] public float ModeFloat = 0;

        public bool RemoveAnimalOnActive = false;


        public AnimalEvent OnEnter = new();
        public AnimalEvent OnExit = new();
        public AnimalEvent OnZoneActivation = new();
        public AnimalEvent OnZoneFailed = new();


        [SubclassSelector, SerializeReference]
        public Reaction EnterReaction;
        [SubclassSelector, SerializeReference]
        public Reaction ExitReaction;
        [SubclassSelector, SerializeReference]
        public Reaction ActivationReaction;


        //public UnityEvent OnZoneStarted = new UnityEvent();
        //public UnityEvent OnZoneEnded = new UnityEvent();

        [Tooltip("Collider for the Zone. If is not set, it will find the first collider attached to this gameobject")]
        [RequiredField] public Collider ZoneCollider;


        /// <summary>Keep a Track of all the Zones on the Scene </summary>
        public static List<Zone> Zones;

        private int GetID
        {
            get
            {
                switch (zoneType)
                {
                    case ZoneType.Mode:
                        return modeID;
                    case ZoneType.State:
                        return stateID;
                    case ZoneType.Stance:
                        return stanceID;
                    case ZoneType.Force:
                        return 100;
                    default:
                        return 0;
                }
            }
        }

        /// <summary>Is the zone a Mode Zone</summary>
        public bool IsMode => zoneType == ZoneType.Mode;

        /// <summary>Is the zone a Mode Zone</summary>
        public bool IsState => zoneType == ZoneType.State;

        /// <summary>Is the zone a Mode Zone</summary>
        public bool IsStance => zoneType == ZoneType.Stance;

        public List<Tag> Tags { get => tags; set => tags = value; }


        public virtual void RemoveAnimal(MAnimal animal) => AnimalsUsingZone.Remove(animal);

        private void Awake()
        {
            Zones ??= new List<Zone>();
        }

        void OnEnable()
        {
            if (ZoneCollider == null)
                ZoneCollider = GetComponent<Collider>();                  //Get the reference for the collider


            if (ZoneCollider)
            {
                ZoneCollider.isTrigger = true;                                //Force Trigger
                ZoneCollider.enabled = true;
            }

            Zones.Add(this);                                              //Save the the Action Zones on the global Action Zone list

            if (ZoneID == 0) ZoneID = GetID;

            AnimalsInZone = new();                          //Get the reference for the collider
            AnimalsUsingZone = new();                          //Get the reference for the collider



            if (zoneType == ZoneType.Mode && modeID.ID == 4 && ShowActionID)
            {
                if (ActionID != null)
                {
                    modeIndex.Value = ActionID.ID; //Use Action ID IMPORTANT!
                }
                else
                {
                    Debug.LogError("The zone does not have an Action ID. Please add an ID", this);
                    enabled = false;
                }
            }
        }

        void OnDisable()
        {
            Zones.Remove(this);                                              //Remove the the Action Zones on the global Action Zone list

            foreach (var animal in AnimalsInZone)
            {
                ResetStoredAnimal(animal);
                OnExit.Invoke(animal);
                ExitReaction?.React(animal);
            }
            if (ZoneCollider) ZoneCollider.enabled = false;

            AnimalsInZone = new();      //Clear the Animals in Zone
            AnimalsUsingZone = new();   //Clear the Animals using in Zone
            m_Colliders = new();         //Clear the colliders
            JustExitAnimal = null;
        }


        public bool TrueConditions(Collider other)
        {
            if (!enabled) return false;

            if (Tags != null && Tags.Count > 0)
            {
                if (!other.gameObject.HasMalbersTagInParent(Tags.ToArray())) return false;
            }

            if (ZoneCollider == null) return false; // you are 
            if (other == null) return false; // you are CALLING A ELIMINATED ONE

            if (BoneOnly && !other.name.ToLower().Contains(BoneName.ToLower())) return false;  //If is Head Only and no head was found Skip
            if (!MTools.Layer_in_LayerMask(other.gameObject.layer, Layer)) return false;
            if (transform.IsChildOf(other.transform)) return false;                 // Do not Interact with yourself


            return true;
        }
        void OnTriggerEnter(Collider other)
        {
            if (TrueConditions(other))
            {
                //if (debug) Debug.Log($"ENTER: {name}");

                MAnimal animal = other.FindComponent<MAnimal>();             //Get the animal on the entering collider

                if (!animal || animal.Sleep || !animal.enabled) return;       //If there's no animal, or is Sleep or disabled do nothing

                if (animal.RB.isKinematic) return; //Do not Activate while the animal is kinematic

                if (automatic && animal == JustExitAnimal) return; //Do not activate the animal that just exit

                if (!m_Colliders.Contains(other))
                {
                    // Debugging($"[Enter Collider] -> [{other.name}]","white");
                    m_Colliders.Add(other);            //if the entering collider is not already on the list add it
                }
                else return; //The Collider was already there //Kinematic Activation.

                if (AnimalsInZone.Contains(animal)) return;                        //if the animal is already on the list do nothing
                else
                {
                    // animal.IsOnZone = true; //Let know the animal is on a zone
                    animal.Zone = this; //Let know the animal is on a zone

                    AnimalsInZone.Add(animal);                                     //Set a new Animal
                    OnEnter.Invoke(animal);
                    EnterReaction?.React(animal);


                    Debugging($"[Enter Animal] -> [{animal.name}]", "yellow");

                    if (automatic)
                    {
                        ActivateZone(animal);
                    }
                    else
                    {
                        PrepareZone(animal);
                    }
                }
            }
        }


        void OnTriggerExit(Collider other)
        {
            if (TrueConditions(other))
            {
                MAnimal animal = other.GetComponentInParent<MAnimal>();
                if (!animal || animal.Sleep || !animal.enabled) return;       //If there's no animal, or is Sleep or disabled do nothing


                if (m_Colliders != null && m_Colliders.Contains(other))
                {
                    //Debugging($"[Exit Collider] -> [{other.name}]", "white");
                    m_Colliders.Remove(other);                              //Remove the collider from the list that is exiting the zone.
                }

                CheckMissingColliders();

                if (AnimalsInZone.Contains(animal))    //Means that the Entering animal still exist on the zone
                {
                    if (!m_Colliders.Exists(col => col != null && col.transform.SameHierarchy(animal.transform)))  //Check if the Collider was removed
                    {
                        OnExit.Invoke(animal);                //Invoke On Exit when all animal's colliders has exited the Zone
                        ExitReaction?.React(animal);        //React Exit when all animal's colliders has exited the Zone

                        ResetStoredAnimal(animal);

                        AnimalsInZone.Remove(animal);
                        AnimalsUsingZone.Remove(animal);

                        Debugging($"[Exit Animal] -> [{animal.name}]", "yellow");

                        if (automatic)
                        {
                            JustExitAnimal = animal;
                            this.Delay_Action(() => JustExitAnimal = null);
                        }

                        animal.Zone = null;     //Let know the animal  Not On the Zone anymore
                    }
                }
            }
        }



        private void CheckMissingColliders()
        {
            m_Colliders.RemoveAll(x => x == null || x.gameObject.IsDestroyed());
        }


        public void Debugging(string value, string color = "green")
        {
#if UNITY_EDITOR
            if (debug)
                Debug.Log($"<B>[{name}]</B> → <color={color}><B>{value}</B></color>", this);
#endif
        }

        /// <summary>Activate the Zone depending the Zone Type</summary>
        /// <param name="forced"></param>
        public virtual bool ActivateZone(MAnimal animal)
        {
            if (Limit > 0)
            {
                Debug.Log($"AnimalsUsingZone.Count {AnimalsUsingZone.Count}");

                if (AnimalsUsingZone.Count >= Limit)
                {
                    if (debug) Debug.Log($"<b>{name}</b> [Zone Failed to activate Due to limits] -> <b>[{Limit.Value}]</b>", this);
                    OnZoneFailed.Invoke(animal);
                    return false;
                }

                AnimalsUsingZone.Add(animal);
            }

            if (CheckConditions != null)
            {
                CheckConditions.SetTarget(animal);

                //foreach (var cond in CheckConditions.conditions)
                //{
                //    cond.SetTarget(animal);
                //    Debug.Log("lkjahsdlfkajhsdflkjh");
                //}
                if (!CheckConditions.TryEvaluate())
                    return false; //If the conditions are not fullfilled
            }

            if (Weight != 1)
            {
                var prob = Random.Range(0, 1);
                if (Weight < prob)
                {
                    if (debug) Debug.Log($"<b>{name}</b> [Zone Failed to activate] -> <b>[{prob:F2}]</b>", this);
                    return false; //Do not Activate the Zone with low Probability.
                }
            }

            if (CheckAngle(animal))
            {
                var isZoneActive = false;

                //animal.IsOnZone = true; //Let know the animal is on a zone
                animal.Zone = this; //Let know the animal is on a zone

                switch (zoneType)
                {
                    case ZoneType.Mode:
                        isZoneActive = ActivateModeZone(animal);
                        break;
                    case ZoneType.State:
                        isZoneActive = StateZone(animal, stateAction); //State Zones does not require to be delay or prepared to be activated Check if it can be activated
                        break;
                    case ZoneType.Stance:
                        isZoneActive = StanceZone(animal, stanceActionEnter); //State Zones does not require to be delay or prepared to be activated
                        break;
                    case ZoneType.Force:
                        isZoneActive = SetForceZone(animal, true); //State Zones does not require to be delay or prepared to be activated
                        break;
                }

                if (isZoneActive)
                {
                    Debugging($"[Zone Activate] <b>[{animal.name}]</b>");
                    OnZoneActive(animal);
                    return true;
                }
            }

            return false;
        }

        public virtual void ActivateZone()
        {
            //Error when the zone is disabled after is used .. weird bug
            try
            {
                foreach (var animal in AnimalsInZone)
                    ActivateZone(animal);
            }
            catch { }

        }

        /// <summary> Check if the Animal is in the right angle to activate the zone </summary>
        protected bool CheckAngle(MAnimal animal)
        {
            var flip = (Flip ? 1 : -1);

            var EntrySideAngle = Vector3.Angle(transform.forward * flip, animal.Forward) * 2;
            var OtherSideAngle = EntrySideAngle;

            if (DoubleSide) OtherSideAngle = Vector3.Angle(-transform.forward * flip, animal.Forward) * 2;

            var side = Vector3.Dot((animal.transform.position - transform.position).normalized, transform.forward) * -1; //Calculate the correct side
            return (Angle == 360 || (EntrySideAngle < Angle && side < 0) || (OtherSideAngle < Angle && side > 0));
        }

        protected virtual void PrepareZone(MAnimal animal)
        {
            switch (zoneType)
            {
                case ZoneType.Mode:
                    var PreMode = animal.Mode_Get(ZoneID);

                    if (PreMode == null || !PreMode.HasAbilityIndex(ModeAbilityIndex)) //If the Animal does not have that mode or that Ability Index exti
                    {
                        OnZoneFailed.Invoke(animal);

                        Debug.LogWarning($"<B>[{name}]</B> cannot be activated by <B>[{animal.name}]</B>." +
                            $" It does not have The <B>[Mode {modeID.name}]</B> with <B>[Ability {ModeAbilityIndex}]</B>", this);
                        return;
                    }
                    PreMode.SetAbilityIndex(ModeAbilityIndex); //Set the Current Ability
                    break;
                case ZoneType.State:
                    var PreState = animal.State_Get(ZoneID);
                    if (!PreState) OnZoneFailed.Invoke(animal);
                    break;
                case ZoneType.Stance:
                    break;
                case ZoneType.Force:
                    break;
            }
        }

        /// <summary>Enables the Zone using the State</summary>
        private bool StateZone(MAnimal animal, StateAction action)
        {
            var Succesful = false;
            switch (action)
            {
                case StateAction.Activate:
                    if (animal.ActiveStateID != ZoneID)
                    {
                        animal.State_Activate(ZoneID, stateStatus);
                        Succesful = true;
                    }
                    break;
                case StateAction.AllowExit:
                    if (animal.ActiveStateID == ZoneID)
                    {
                        animal.ActiveState.AllowExit();
                        Succesful = true;
                    }
                    break;
                case StateAction.ForceActivate:
                    animal.State_Force(ZoneID, stateStatus);
                    Succesful = true;
                    break;
                case StateAction.Enable:
                    animal.State_Enable(ZoneID);
                    Succesful = true;
                    break;
                case StateAction.Disable:
                    animal.State_Disable(ZoneID);
                    Succesful = true;
                    break;
                case StateAction.SetExitStatus:
                    if (animal.ActiveStateID == stateID)
                    {
                        animal.State_SetExitStatus(stateStatus);
                        Succesful = true;
                    }
                    break;
                default:
                    break;
            }
            return Succesful;
        }

        /// <summary>Enables the Zone using the Modes</summary>
        private bool ActivateModeZone(MAnimal animal)
        {
            if (ForceMode)
            {
                animal.Mode_ForceActivate(ZoneID, ModeAbilityIndex, m_abilityStatus, AbilityTime);
                animal.Mode_SetPower(ModeFloat); //Set the correct height for the Animal Animation
                return true;
            }
            else
            {
                if (animal.Mode_TryActivate(ZoneID, ModeAbilityIndex, m_abilityStatus, AbilityTime))
                {
                    animal.Mode_SetPower(ModeFloat); //Set the correct height for the Animal Animation 
                    return true;
                }
            }
            return false;
        }

        /// <summary>Enables the Zone using the Stance</summary>
        private bool StanceZone(MAnimal animal, StanceAction action)
        {
            switch (action)
            {
                case StanceAction.Activate:
                    animal.Stance_Set(stanceID);
                    break;
                case StanceAction.Exit:
                    animal.Stance_Reset();
                    break;
                case StanceAction.SetDefault:
                    animal.DefaultStanceID = stanceID;
                    break;
                default:
                    break;
            }
            return true;
        }

        /// <summary>Enables the Zone using External Forces</summary>
        private bool SetForceZone(MAnimal animal, bool ON)
        {
            if (ON) //ENTERING THE FORCE ZONE!!!
            {
                var StartExtForce = animal.CurrentExternalForce + animal.GravityStoredVelocity; //Calculate the Starting force


                if (StartExtForce.magnitude > LimitForce)
                {
                    StartExtForce = StartExtForce.normalized * LimitForce; //Add the Bounce
                }


                animal.CurrentExternalForce = StartExtForce;
                animal.ExternalForce = transform.up * Force;
                animal.ExternalForceAcel = EnterAceleration;

                if (animal.ActiveState.ID == StateEnum.Fall) //If we enter to a zone from the Fall state.. Reset the Fall Current Distance
                {
                    var fall = animal.ActiveState as Fall;
                    fall.FallCurrentDistance = 0;

                    //  fall.UpImpulse = Vector3.Project(animal.DeltaPos, animal.UpVector);   //Clean the Vector from Forward and Horizontal Influence    
                    //  animal.CalculateTargetSpeed(); //Important needs to calculate the Target Speed again
                    //  animal.InertiaPositionSpeed = animal.TargetSpeed; //Set the Target speed to the Fall Speed so there's no Lerping when the speed changes
                }

                animal.GravityTime = 0;
                animal.Grounded = ForceGrounded.Value;
                animal.ExternalForceAirControl = ForceAirControl.Value;
            }
            else
            {
                if (animal.ActiveState.ID == StateEnum.Fall) animal.UseGravity = true;  //If we are on the Fall State -- Reactivate the Gravity

                if (ExitDrag > 0)
                {
                    animal.ExternalForceAcel = ExitDrag;
                    animal.ExternalForce = Vector3.zero;
                }
            }
            return ON;
        }

        internal void OnZoneActive(MAnimal animal)
        {
            OnZoneActivation.Invoke(animal);
            ActivationReaction?.React(animal);

            if (RemoveAnimalOnActive)
            {
                ResetStoredAnimal(animal);
                AnimalsInZone.Remove(animal);
                AnimalsUsingZone.Remove(animal);
            }

            if (DisableAfterUsed.Value) enabled = false;
        }

        public void TargetArrived(GameObject go)
        {
            var animal = go.FindComponent<MAnimal>();
            ActivateZone(animal);
        }

        public virtual void ResetStoredAnimal(MAnimal animal)
        {
            if (animal)
            {
                animal.Zone = null; //Tell the Animal is no longer on a Zone

                switch (zoneType)
                {
                    case ZoneType.Mode:

                        var mode = animal.Mode_Get(ZoneID);

                        if (mode != null) //Means we found the current Active mode
                        {
                            //Only reset when it has the same Index... works for zones near eachother 
                            if (mode.AbilityIndex == ModeAbilityIndex) mode.ResetAbilityIndex();
                        }

                        break;
                    case ZoneType.State:
                        StateZone(animal, stateActionExit);
                        break;
                    case ZoneType.Stance:
                        StanceZone(animal, stanceActionExit);
                        break;
                    case ZoneType.Force:
                        SetForceZone(animal, false);
                        break;
                    default:
                        break;
                }
            }
        }

        [HideInInspector] public int Editor_Tabs1 = 0;

#if UNITY_EDITOR
        [ContextMenu("Connect to Align")]
        void TryAlign()
        {
            var method = this.GetUnityAction<MAnimal>("Aligner", "Align");
            if (method != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(OnZoneActivation, method);
            MTools.SetDirty(this);
        }



        private void OnDrawGizmos()
        {
            if (Application.isPlaying && AnimalsInZone != null)
            {
                foreach (var animal in AnimalsInZone)
                {
                    if (animal == null) continue;

                    var flip = (Flip ? 1 : -1);

                    var EntrySideAngle = Vector3.Angle(transform.forward * flip, animal.Forward) * 2;
                    var OtherSideAngle = EntrySideAngle;

                    if (DoubleSide) OtherSideAngle = Vector3.Angle(-transform.forward * flip, animal.Forward) * 2;

                    var DColor = Color.red;

                    var side = Vector3.Dot((animal.transform.position - transform.position).normalized, transform.forward) * -1;


                    if (Angle == 360 || (EntrySideAngle < Angle && side < 0) || (OtherSideAngle < Angle && side > 0))
                    {
                        DColor = Color.green;
                    }



                    if (debug)
                        MDebug.Draw_Arrow(animal.transform.position + Vector3.up * 0.05f, animal.Forward, DColor);
                }
            }
        }
#endif
    }
    public enum StateAction
    {
        /// <summary>Tries to Activate the State of the Zone</summary>
        Activate,
        /// <summary>If the Animal is already on the state of the zone it will allow to exit and activate states below the Active one</summary>
        AllowExit,
        /// <summary>Force the State of the Zone to be enable even if it cannot be activate at the moment</summary>
        ForceActivate,
        /// <summary>Enable a  Disabled State </summary>
        Enable,
        /// <summary>Disable State </summary>
        Disable,
        SetExitStatus,
        /// <summary>Disable State </summary>
        None,
    }
    public enum StanceAction
    {
        /// <summary>Enters a Stance</summary>
        Activate,
        /// <summary>Exits a Stance</summary>
        Exit,
        /// <summary>Set the Default Stance</summary>
        SetDefault,
        /// <summary>Do nothing</summary>
        None,

    }
    public enum ZoneType
    {
        Mode,
        State,
        Stance,
        Force,
        ReactionsOnly,
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(Zone))/*, CanEditMultipleObjects*/]
    public class ZoneEditor : Editor
    {
        private Zone m;


        protected string[] Tabs1 = new string[] { "General", "Events", "Reactions" };


        SerializedProperty
            HeadOnly, stateAction, stateActionExit,

            EnterReaction, ExitReaction, ActivationReaction,

            HeadName, zoneType, stateID, modeID, modeIndex, ActionID, auto, DisableAfterUsed, //reEnableOnModeEnd,
            Limit, ShowActionID, debug, m_abilityStatus, AbilityTime, Editor_Tabs1, ForceMode,
            OnZoneActivation, OnExit, OnEnter, ForceGrounded, OnZoneFailed, Angle, CheckConditions,
            DoubleSide, Weight, Flip, ForceAirControl, ZoneCollider,
            stanceActionEnter, stanceActionExit, layer, stanceID, RemoveAnimalOnActive, m_tag, ModeFloat, Force, EnterAceleration, ExitAceleration, stateStatus, Bounce;

        //MonoScript script;
        private void OnEnable()
        {
            m = ((Zone)target);
            //script = MonoScript.FromMonoBehaviour((MonoBehaviour)target);

            EnterReaction = serializedObject.FindProperty("EnterReaction");
            ExitReaction = serializedObject.FindProperty("ExitReaction");
            ActivationReaction = serializedObject.FindProperty("ActivationReaction");
            CheckConditions = serializedObject.FindProperty("CheckConditions");


            HeadOnly = serializedObject.FindProperty("BoneOnly");
            HeadName = serializedObject.FindProperty("BoneName");
            Limit = serializedObject.FindProperty("Limit");

            RemoveAnimalOnActive = serializedObject.FindProperty("RemoveAnimalOnActive");
            layer = serializedObject.FindProperty("Layer");
            Flip = serializedObject.FindProperty("Flip");

            stateStatus = serializedObject.FindProperty("stateStatus");
            OnZoneFailed = serializedObject.FindProperty("OnZoneFailed");
            DisableAfterUsed = serializedObject.FindProperty("DisableAfterUsed");
            //reEnableOnModeEnd = serializedObject.FindProperty("reEnableOnModeEnd");


            Force = serializedObject.FindProperty("Force");
            EnterAceleration = serializedObject.FindProperty("EnterAceleration");
            ExitAceleration = serializedObject.FindProperty("ExitDrag");
            Bounce = serializedObject.FindProperty("LimitForce");
            ForceGrounded = serializedObject.FindProperty("ForceGrounded");
            ForceAirControl = serializedObject.FindProperty("ForceAirControl");

            m_abilityStatus = serializedObject.FindProperty("m_abilityStatus");
            AbilityTime = serializedObject.FindProperty("AbilityTime");

            Angle = serializedObject.FindProperty("Angle");
            Weight = serializedObject.FindProperty("Weight");
            DoubleSide = serializedObject.FindProperty("DoubleSide");


            m_tag = serializedObject.FindProperty("tags");
            ModeFloat = serializedObject.FindProperty("ModeFloat");
            zoneType = serializedObject.FindProperty("zoneType");
            stateID = serializedObject.FindProperty("stateID");
            stateAction = serializedObject.FindProperty("stateAction");
            stateActionExit = serializedObject.FindProperty("stateActionExit");

            stanceActionEnter = serializedObject.FindProperty("stanceActionEnter");
            stanceActionExit = serializedObject.FindProperty("stanceActionExit");

            modeID = serializedObject.FindProperty("modeID");
            stanceID = serializedObject.FindProperty("stanceID");
            modeIndex = serializedObject.FindProperty("modeIndex");
            ActionID = serializedObject.FindProperty("ActionID");
            auto = serializedObject.FindProperty("automatic");
            debug = serializedObject.FindProperty("debug");
            ZoneCollider = serializedObject.FindProperty("ZoneCollider");
            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");
            ShowActionID = serializedObject.FindProperty("ShowActionID");
            ForceMode = serializedObject.FindProperty("ForceMode");


            OnEnter = serializedObject.FindProperty("OnEnter");
            OnExit = serializedObject.FindProperty("OnExit");
            OnZoneActivation = serializedObject.FindProperty("OnZoneActivation");


            if (ZoneCollider.objectReferenceValue == null)
            {
                ZoneCollider.objectReferenceValue = m.GetComponent<Collider>();
                serializedObject.ApplyModifiedProperties();
            }
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Area to modify States, Stances or Modes on an Animal");


            using (new GUILayout.HorizontalScope())
            {
                Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, Tabs1);
                MalbersEditor.DrawDebugIcon(debug);
            }



            EditorGUI.BeginChangeCheck();

            switch (Editor_Tabs1.intValue)
            {
                case 0: DrawGeneral(); break;
                case 1: DrawEvents(); break;
                case 2: DrawReactions(); break;
                default: break;
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Zone Changed");
                EditorUtility.SetDirty(target);
            }


            if (Application.isPlaying && debug.boolValue)
            {

                using (new EditorGUI.DisabledGroupScope(true))
                {
                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))

                    {
                        EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);

                        EditorGUILayout.LabelField("Animals in Zone (" + m.AnimalsInZone.Count + ")", EditorStyles.boldLabel);
                        foreach (var item in m.AnimalsInZone)
                        {
                            EditorGUILayout.ObjectField(item.name, item, typeof(MAnimal), false);
                        }


                        EditorGUILayout.LabelField("Animals Using Zone (" + m.AnimalsUsingZone.Count + ")", EditorStyles.boldLabel);
                        foreach (var item in m.AnimalsUsingZone)
                        {
                            EditorGUILayout.ObjectField(item.name, item, typeof(MAnimal), false);
                        }

                        EditorGUILayout.LabelField("Colliders in Zone(" + m.m_Colliders.Count + ")", EditorStyles.boldLabel);
                        foreach (var item in m.m_Colliders)
                        {
                            EditorGUILayout.ObjectField(item.name, item, typeof(Collider), false);
                        }

                    }
                    Repaint();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawReactions()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(ActivationReaction);
                EditorGUILayout.PropertyField(EnterReaction);
                EditorGUILayout.PropertyField(ExitReaction);
                EditorGUI.indentLevel--;
            }
        }

        private void DrawGeneral()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(auto, new GUIContent("Automatic"));

                EditorGUILayout.PropertyField(DisableAfterUsed);
                EditorGUILayout.PropertyField(RemoveAnimalOnActive,
                        new GUIContent("Reset on Active", "Remove the stored Animal on the Zone when the Zones gets Active, Reseting it to its default state"));


                //if (zone == ZoneType.Mode)
                //    EditorGUILayout.PropertyField(reEnableOnModeEnd);

                EditorGUILayout.PropertyField(Limit);

                EditorGUILayout.PropertyField(ZoneCollider, new GUIContent("Trigger"));


            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))

            {
                EditorGUILayout.PropertyField(zoneType);
                ZoneType zone = (ZoneType)zoneType.intValue;


                switch (zone)
                {
                    case ZoneType.Mode:

                        var usingAction = m.modeID != null && m.modeID == 4;

                        using (new GUILayout.HorizontalScope())
                        {
                            EditorGUILayout.PropertyField(modeID,
                            new GUIContent("Mode ID: [" + (m.modeID ? m.modeID.ID.ToString() : "") + "]", "Which Mode to Set when entering the Zone"));

                            serializedObject.ApplyModifiedProperties();


                            if (usingAction)
                                ShowActionID.boolValue =
                                    GUILayout.Toggle(ShowActionID.boolValue, new GUIContent("•", "Show/Hide Action ID"),
                                    EditorStyles.miniButton, GUILayout.Width(22));
                        }

                        if (usingAction && ShowActionID.boolValue)
                        {
                            EditorGUILayout.PropertyField(ActionID,
                                new GUIContent("Action Index: [" + (m.ActionID ? m.ActionID.ID.ToString() : "") + "]", "Which Action to Set when entering the Zone"));

                            if (ActionID.objectReferenceValue == null)
                            {
                                EditorGUILayout.HelpBox("Please Select an Action ID", MessageType.Error);
                            }
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(modeIndex, new GUIContent("Ability Index", "Which Ability to Set when entering the Zone"));
                        }

                        EditorGUILayout.PropertyField(m_abilityStatus, new GUIContent("Status", "Mode Ability Status"));

                        if (m_abilityStatus.intValue == (int)AbilityStatus.ActiveByTime)
                        {
                            EditorGUILayout.PropertyField(AbilityTime);
                        }
                        EditorGUILayout.PropertyField(ModeFloat, new GUIContent("Mode Power"));
                        EditorGUILayout.PropertyField(ForceMode);

                        break;
                    case ZoneType.State:
                        EditorGUILayout.PropertyField(stateID, new GUIContent("State ID", "Which State will Activate when entering the Zone"));
                        EditorGUILayout.PropertyField(stateAction, new GUIContent("On Enter", "Execute a State logic when the animal enters the zone"));
                        EditorGUILayout.PropertyField(stateActionExit, new GUIContent("On Exit", "Execute a State logic when the animal Exits the zone"));

                        int stateaction = stateAction.intValue;
                        if (MTools.CompareOR(stateaction, (int)StateAction.Activate, (int)StateAction.ForceActivate, (int)StateAction.SetExitStatus))
                        {
                            EditorGUILayout.PropertyField(stateStatus, new GUIContent("State Status"));
                        }

                        if (stateID.objectReferenceValue == null)
                        {
                            EditorGUILayout.HelpBox("Please Select an State ID", MessageType.Error);
                        }
                        break;
                    case ZoneType.Stance:
                        EditorGUILayout.PropertyField(stanceID, new GUIContent("Stance ID", "Which Stance will Activate when entering the Zone"));
                        EditorGUILayout.PropertyField(stanceActionEnter, new GUIContent("On Enter", "Execute a Stance logic when the animal Enters the zone"));
                        EditorGUILayout.PropertyField(stanceActionExit, new GUIContent("On Exit", "Execute a Stance logic when the animal Exits the zone"));
                        if (stanceID.objectReferenceValue == null)
                        {
                            EditorGUILayout.HelpBox("Please Select an Stance ID", MessageType.Error);
                        }
                        break;
                    case ZoneType.Force:
                        EditorGUILayout.PropertyField(Force);
                        EditorGUILayout.PropertyField(EnterAceleration);
                        EditorGUILayout.PropertyField(ExitAceleration);
                        EditorGUILayout.PropertyField(Bounce);
                        EditorGUILayout.PropertyField(ForceAirControl, new GUIContent("Air Control"));
                        EditorGUILayout.PropertyField(ForceGrounded, new GUIContent("Grounded? "));
                        break;
                    default:
                        break;
                }
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                Weight.isExpanded = MalbersEditor.Foldout(Weight.isExpanded, "Conditions");

                if (Weight.isExpanded)
                {
                    EditorGUILayout.PropertyField(layer);
                    EditorGUILayout.PropertyField(Weight);
                    EditorGUILayout.PropertyField(Angle);
                    EditorGUILayout.PropertyField(CheckConditions);

                    if (Angle.floatValue != 360)
                    {
                        EditorGUILayout.PropertyField(DoubleSide);
                        EditorGUILayout.PropertyField(Flip);
                    }



                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_tag,
                        new GUIContent("Tags", "Set this parameter if you want the zone to Interact only with gameObject with that tag"));
                    EditorGUI.indentLevel--;

                    EditorGUILayout.PropertyField(HeadOnly,
                        new GUIContent("Bone Only", "Activate when a bone enter the Zone.\nThat Bone needs a collider!!"));

                    if (HeadOnly.boolValue)
                        EditorGUILayout.PropertyField(HeadName,
                            new GUIContent("Bone Name", "Name for the Bone you need to check if it has enter the zone"));
                }
            }
        }

        private void DrawEvents()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(OnEnter, new GUIContent("On Animal Enter Zone"));
                EditorGUILayout.PropertyField(OnExit, new GUIContent("On Animal Exit Zone"));
                EditorGUILayout.PropertyField(OnZoneActivation, new GUIContent("On Zone Active"));
                EditorGUILayout.PropertyField(OnZoneFailed, new GUIContent("On Zone Failed"));
            }

        }

        private void OnSceneGUI()
        {
            var angle = Angle.floatValue;
            if (angle != 360)
            {
                angle /= 2;

                var Direction = m.transform.forward * (Flip.boolValue ? -1 : 1);

                Handles.color = new Color(0, 1, 0, 0.1f);
                Handles.DrawSolidArc(m.transform.position, m.transform.up, Quaternion.Euler(0, -angle, 0) * Direction, angle * 2, m.transform.localScale.y);
                Handles.color = Color.green;
                Handles.DrawWireArc(m.transform.position, m.transform.up, Quaternion.Euler(0, -angle, 0) * Direction, angle * 2, m.transform.localScale.y);

                if (DoubleSide.boolValue)
                {
                    Handles.color = new Color(0, 1, 0, 0.1f);
                    Handles.DrawSolidArc(m.transform.position, m.transform.up, Quaternion.Euler(0, -angle, 0) * -Direction, angle * 2, m.transform.localScale.y);
                    Handles.color = Color.green;
                    Handles.DrawWireArc(m.transform.position, m.transform.up, Quaternion.Euler(0, -angle, 0) * -Direction, angle * 2, m.transform.localScale.y);
                }
            }
        }
    }
#endif
}
