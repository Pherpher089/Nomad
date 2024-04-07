﻿using UnityEngine;
using MalbersAnimations.Scriptables;
using MalbersAnimations.Events;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace MalbersAnimations.Utilities
{
    [DefaultExecutionOrder(10000)]
    [AddComponentMenu("Malbers/Utilities/Aiming/Aim")]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/utilities/aim")]
    public class Aim : MonoBehaviour, IAim, IAnimatorListener
    {
        #region Vars and Props

        #region Public Variables
        [SerializeField, Tooltip("Is the Aim Active")]
        private BoolReference m_active = new(true);

        [SerializeField, Tooltip("Aim Origin Reference (Required)"), ContextMenuItem("Head as AimOrigin", "HeadAimOrigin")]
        [RequiredField] private Transform m_aimOrigin;
        [SerializeField, Tooltip("Smoothness Lerp value to change from Active to Disable")]
        private float m_Smoothness = 10f;

        [SerializeField, Tooltip("Layers inlcuded on the Aiming Logic")]
        private LayerReference m_aimLayer = new(-1);
        [SerializeField, Tooltip("Does the Aiming Logic ignore Colliders??")]
        private QueryTriggerInteraction m_Triggers = QueryTriggerInteraction.Ignore;

        [SerializeField, Tooltip("Forced a Target on the Aiming Logic. Calculate the Aim from the Aim Origin to a Target")]
        private TransformReference m_AimTarget = new();

        [Tooltip("Transform Helper that stores the position of the Hit")]
        public TransformReference m_AimPosition = new();

        [SerializeField, Tooltip("Set a Transform Hierarchy to Ignore on the Aim Ray")]
        private TransformReference m_Ignore = new();

        [SerializeField, Tooltip("Camera Reference used for calculatin the Aim logic from the Camera Center. By Default will use the Camera.Main Transform")]
        private TransformReference m_camera = new();

        [SerializeField, Tooltip("Cast the Camera Ray a bit forward to avoid colliding with near the camera colliders ")]
        private FloatReference m_forwardCam = new(0.2f);

        private Camera cam;

        [SerializeField, Tooltip("Do the raycast every X Cycles to increase performance")]
        [Min(1)] private int m_cycles = 1;
        private int CurrentCycles;

        [SerializeField, Tooltip("Default screen center")]
        private Vector2Reference m_screenCenter = new(0.5f, 0.5f);

        [Tooltip("Does the Character Requires the Camera to Find Aiming... Disable this for AI Characters")]
        public BoolReference m_UseCamera = new(true);



        [SerializeField]
        [Tooltip("This Parameter is used to Change the Current Camera to the Side of which the Character is relative to Camera or the Target")]
        private AimSide m_AimSide = 0;

        [Tooltip("Update mode for the Aim Logic")]
        public UpdateType updateMode = UpdateType.LateUpdate;

        /// <summary>Maximun Distance for the Camera Ray</summary>
        [Tooltip("Maximun Distance from the Origin to the Possible Target")]
        public float MaxDistance = 100f;

        [SerializeField, Tooltip("Use Raycasting for finding the Hit Point. Disable this if you don't need to know which was the object hitted.")]
        private BoolReference m_UseRaycasting = new(true);

        /// <summary>Radius for the Sphere Casting, if this is set to Zero they I will use a Ray Casting</summary>
        [Tooltip("Radius for the Sphere Casting, if this is set to Zero they I will use a Ray Casting")]
        public FloatReference rayRadius = new(0.0f);

        /// <summary>Ray Counts for the Ray casting</summary>
        [Tooltip("Maximum Ray Hits for the Ray casting")]
        public int RayHits = 5;

        public TransformEvent OnAimRayTarget = new();
        public Vector3Event OnScreenCenter = new();
        public IntEvent OnAimSide = new();
        public BoolEvent OnAiming = new();
        public BoolEvent OnUsingTarget = new();
        public TransformEvent OnHit = new();

        public TransformEvent OnSetTarget = new();
        public UnityEvent OnClearTarget = new();

        public bool debug;
        private string hitName;
        private int hitcount;
        #endregion

        #region Properties

        /// <summary>Store the Target Renderer when a new Target is set</summary>
        public Renderer TargetRenderer { get; private set; }

        /// <summary>Find the Target Center</summary>
        public Vector3 TargetCenter => TargetRenderer != null ? TargetRenderer.bounds.center : AimTarget.position;

        /// <summary>Main Camera</summary>
        public Transform MainCamera { get => m_camera.Value; set => m_camera.Value = value; }

        /// <summary>Check if use Camera is enabled</summary>
        public bool UseCamera { get => m_UseCamera.Value; set => m_UseCamera.Value = value; }

        /// <summary>Cast the Camera Ray a bit forward to avoid colliding with near the camera colliders </summary>
        public float ForwardCam { get => m_forwardCam.Value; set => m_forwardCam.Value = value; }

        #region Animator Values
        internal int hash_AimHorizontal;
        internal int hash_AimVertical;

        public Animator m_Animator;
        public string m_AimHorizontal = "AimHorizontal";
        public string m_AimVertical = "AimVertical";

        public FloatReference AngleLerp = new FloatReference();

        #endregion


        /// <summary>Use Raycasting for finding the Hit Point</summary>
        public bool UseRaycasting { get => m_UseRaycasting.Value; set => m_UseRaycasting.Value = value; }

        /// <summary>Sets the Aim Origin Transform </summary>
        public Transform AimOrigin
        {
            get => m_aimOrigin;
            set
            {
                //Debug.Log("value = " + value);
                if (value)
                    m_aimOrigin = value;
                else
                    m_aimOrigin = defaultOrigin;
            }
        }

        /// <summary>Store the Original Default Origin Transform, in case someone else changed it</summary>
        private Transform defaultOrigin;
        private Transform OwnObjectCore;

        /// <summary>Set a Extra Transform to Ignore it (Used in case of the Mount for the Rider)</summary>
        public Transform IgnoreTransform { get => m_Ignore.Value; set => m_Ignore.Value = value; }

        /// <summary>Direction the GameObject is Aiming (Smoothed) </summary>
        public Vector3 AimDirection => AimOrigin.DirectionTo(AimPoint);

        /// <summary>Raw Direction the GameObject is Aiming</summary>
        public Vector3 RawAimDirection { get; private set; }

        /// <summary>is the Current AimTarget a Target Assist?</summary>
        public bool IsTargetAssist { get; private set; }

        /// <summary>Smooth Aim Point the ray is Aiming</summary>
        public Vector3 AimPoint { get; private set; }

        /// <summary>RAW Aim Point the ray is Aiming</summary>
        public Vector3 RawPoint { get; private set; }

        public float HorizontalAngle_Raw { get; set; }
        public float VerticalAngle_Raw { get; set; }
        public float HorizontalAngle { get; set; }
        public float VerticalAngle { get; set; }

        /// <summary>Default Screen Center</summary>
        public Vector3 ScreenCenter { get; private set; }

        public IAimTarget LastAimTarget;

        /// <summary>Is the Aiming Logic Active?</summary>
        public bool Active
        {
            get => m_active;
            set
            {
                m_active.Value = enabled = value;

                if (value) EnterAim();
                else ExitAim();
            }
        }



        /// <summary> Last Raycast stored for calculating the Aim</summary>
        private RaycastHit aimHit;

        /// <summary> Last Raycast stored for calculating the Aim</summary>
        public RaycastHit AimHit => aimHit;

        private Transform m_AimTargetAssist;

        /// <summary>Transform hitted using Raycast</summary>
        private Transform AimHitTransform;

        /// <summary>Target Transform Stored from the AimRay</summary>
        public Transform AimRayTargetAssist
        {
            get => m_AimTargetAssist;
            set
            {
                if (m_AimTargetAssist != value)
                {
                    m_AimTargetAssist = value;
                    OnAimRayTarget.Invoke(value);
                }
            }
        }

        /// <summary>Check if the camera is in the right:true or Left: False side of the Character </summary>
        public bool AimingSide { get; private set; }

        /// <summary>Forced Target on the Aiming Logic</summary>
        public Transform AimTarget
        {
            get => m_AimTarget.Value;
            set
            {
                if (m_AimTarget.Value != value) //Only execute the logic when the values are different
                {
                    m_AimTarget.Value = value;

                    if (value != null)
                    {
                        var assist = value.GetComponentInChildren<AimTarget>();
                        if (assist != null)
                        {
                            m_AimTarget.Value = assist.AimPoint;
                        }
                        enabled = true; //make sure the is Enabled on Target
                    }
                    else
                    {
                        OnClearTarget.Invoke();
                        //if (!m_UseCamera.Value) { enabled = false; return; } //Do not Use AimState if is only meant to be use on Targets
                    }

                    if (debug) Debug.Log($"<B>[{name}]</B> - New Target Set <B>[{value}]</B>", this);

                    OnSetTarget.Invoke(value);
                    OnUsingTarget.Invoke(value != null);
                    OnAimRayTarget.Invoke(value);
                }
            }
        }

        /// <summary>Tranform Helper use to Ping the Hit Point</summary>
        public Transform AimPosition { get => m_AimPosition.Value; set => m_AimPosition.Value = value; }

        /// <summary>Layer to Aim and Hit</summary>
        public LayerMask Layer { get => m_aimLayer.Value; set => m_aimLayer.Value = value; }

        public QueryTriggerInteraction TriggerInteraction { get => m_Triggers; set => m_Triggers = value; }

        public AimSide AimSide
        {
            get => m_AimSide;
            set
            {
                m_AimSide = value;

                switch (value)
                {
                    case AimSide.None: OnAimSide.Invoke(0); break;
                    case AimSide.Left: OnAimSide.Invoke(-1); break;
                    case AimSide.Right: OnAimSide.Invoke(1); break;
                    default: break;
                }
            }
        }
        public RaycastHit[] ArrayHits { get; private set; }

        #endregion
        #endregion

        public int EditorTab1 = 1;


        void Awake()
        {
            FindCamera();

            m_Animator = GetComponentInParent<Animator>();

            if (m_Animator)
            {
                hash_AimHorizontal = TryOptionalParameter(m_AimHorizontal);
                hash_AimVertical = TryOptionalParameter(m_AimVertical);
            }

            if (AimOrigin)
                defaultOrigin = AimOrigin;
            else
                AimOrigin = defaultOrigin = transform;

            OwnObjectCore = transform.FindObjectCore();

            GetCenterScreen();

            CurrentCycles = UnityEngine.Random.Range(0, 999999);
        }

        private void FindCamera()
        {
            //Find the Main Camera on the Scene
            if (MainCamera == null)
            {
                cam = MTools.FindMainCamera();
                if (cam) MainCamera = cam.transform;
            }
            else
            {
                cam = MainCamera.GetComponent<Camera>();
            }
        }

        private int TryOptionalParameter(string param)
        {
            var AnimHash = Animator.StringToHash(param);

            foreach (var p in m_Animator.parameters)
            {
                if (p.nameHash == AnimHash) return AnimHash;
            }

            return 0;
        }


        void OnEnable()
        {
            CalculateAiming();

            var newT = m_AimTarget.Value;
            m_AimTarget.Value = null;
            AimTarget = newT;     //Call the Events on the Aim Target

            if (!m_camera.UseConstant && m_camera.Variable)
            {
                m_camera.Variable.OnValueChanged += SearchCamera;
            }
        }

        private void OnDisable()
        {
            if (!m_camera.UseConstant && m_camera.Variable)
            {
                m_camera.Variable.OnValueChanged -= SearchCamera;
            }

            LastAimTarget?.IsBeenAimed(false, gameObject);

            LastAimTarget = null;

            //THE AIM TARGET NEEDS TO BE STOPSD
        }

        private void SearchCamera(Transform obj) => FindCamera();

        private void FixedUpdate()
        {
            if (updateMode == UpdateType.FixedUpdate)
                UpdateLogic(Time.fixedDeltaTime);
        }


        private void LateUpdate()
        {
            if (updateMode == UpdateType.LateUpdate)
                UpdateLogic(Time.deltaTime);
        }


        private void UpdateLogic(float time)
        {
            if (!Active) return;


            CurrentCycles++;
            var UseRay = UseRaycasting && (CurrentCycles % m_cycles == 0);
            if (UseRay) CurrentCycles = 0;

            AimLogic(UseRay);
            SmoothValues(time);
            CalculateAngles(time);

            if (m_Animator)
            {
                TryAnimParameter(hash_AimHorizontal, HorizontalAngle);
                TryAnimParameter(hash_AimVertical, VerticalAngle);
            }
        }

        public void EnterAim()
        {
            CalculateAiming();
            OnAiming.Invoke(true);
            if (AimPosition) AimPosition.gameObject.SetActive(true); //Hide the Helper
        }

        public void ExitAim()
        {
            GetCenterScreen();
            OnScreenCenter.Invoke(ScreenCenter);
            OnAimRayTarget.Invoke(null);
            AimSide = AimSide.None;

            OnAiming.Invoke(false);

            if (AimPosition)
                AimPosition.gameObject.SetActive(false); //Hide the Helper
        }

        public virtual void TryAnimParameter(int Hash, float value)
        {
            if (Hash != 0) m_Animator.SetFloat(Hash, value);
        }

        public virtual void AimLogic(bool useRaycasting)
        {
            if (AimTarget)
            {
                aimHit = DirectionFromTarget(useRaycasting);
                RawPoint = UseRaycasting ? aimHit.point : TargetCenter;
            }
            else if (UseCamera && MainCamera && cam != null)
            {
                aimHit = DirectionFromCamera(useRaycasting);
                RawPoint = aimHit.point;
            }
            else //Means we are using Forward Direction
            {
                aimHit = DirectionFromDirection(useRaycasting);
                RawPoint = aimHit.point;
            }

            if (useRaycasting) //Invoke the OnHit Option
            {
                if (AimHitTransform != aimHit.transform)
                {
                    AimHitTransform = aimHit.transform;
                    OnHit.Invoke(AimHitTransform);
                    // if (debug) Debug.Log("AimHitTransform = " + AimHitTransform);
                }
            }
        }

        /// <summary> Calculate the Aiming Direction with no smoothing</summary>
        public void CalculateAiming()
        {
            if (Active)
            {
                AimLogic(UseRaycasting);
                SmoothValues(0);
                CalculateAngles(0);
            }
        }


        public void Active_Set(bool value) => Active = value;
        public void Active_Toggle() => Active ^= true;
        public void SetTarget(Transform target) => AimTarget = target;
        public void SetTarget(TransformVar target) => AimTarget = target.Value;
        public void SetTarget(GameObjectVar target) => AimTarget = target.Value.transform;
        public void SetTarget(Component target) => SetTarget(target.transform);
        public void SetTarget(GameObject target) => SetTarget(target.transform);
        public void ClearTarget() => AimTarget = null;

        /// <summary>Calculates the Camera/Target Horizontal Angle Normalized </summary>
        public void CalculateAngles(float time)
        {
            var AimDir = (AimPoint - OwnObjectCore.position);

            //Calculate the side of the Origin according to the Aiming Position
            AimingSide = Vector3.Dot(AimDir, transform.right) < 0;

            Vector3 HorizontalDir = Vector3.ProjectOnPlane(AimDir, Vector3.up).normalized;
            Vector3 ForwardDir = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

            // Vector3 HorizontalDir = AimDirection;
            //Vector3 ForwardDir = transform.forward;


            MDebug.Draw_Arrow(transform.position, HorizontalDir, Color.red);
            MDebug.Draw_Arrow(transform.position, ForwardDir, Color.red);

            HorizontalAngle_Raw = Vector3.SignedAngle(ForwardDir, HorizontalDir, Vector3.up); //Get the Normalized value for the look direction
            VerticalAngle_Raw = (Vector3.Angle(transform.up, AimDirection) - 90) * -1;                   //Get the Normalized value for the look direction

            HorizontalAngle = HorizontalAngle_Raw;
            VerticalAngle = VerticalAngle_Raw;
        }


        void SmoothValues(float time)
        {
            float Smoothlerp = time * m_Smoothness;
            Smoothlerp = Mathf.Sin(Smoothlerp * Mathf.PI * 0.5f); //don't remember why  Smooth In Out the Time
            var isRaw = m_Smoothness == 0 || time == 0;

            AimPoint = isRaw ? RawPoint : Vector3.Lerp(AimPoint, RawPoint, Smoothlerp);

            if (AimPosition != null) //Helper for the Aim Position
            {
                AimPosition.position = AimPoint;
                AimPosition.up = isRaw ? aimHit.normal : Vector3.Lerp(AimPosition.up, aimHit.normal, Smoothlerp);
            }
        }

        private void GetCenterScreen()
        {
            Vector3 SC;

            if (cam != null)
            {
                SC = new Vector3(Screen.width * m_screenCenter.Value.x * cam.rect.width,
                    Screen.height * m_screenCenter.Value.y * cam.rect.height); //Gets the Center of the Aim Dot Transform

                //Add the Split Screen Offset
                SC += new Vector3(Screen.width * cam.rect.x, Screen.height * cam.rect.y);
            }
            else
            {
                SC = new Vector3(Screen.width * m_screenCenter.Value.x,
                    Screen.height * m_screenCenter.Value.y); //Gets the Center of the Aim Dot Transform No Cam
            }

            if (SC != ScreenCenter)
            {
                ScreenCenter = SC;
                OnScreenCenter.Invoke(ScreenCenter);
            }
        }

        public RaycastHit DirectionFromCamera(bool useray)
        {

            RawAimDirection = cam.transform.forward;

            Ray ray;

            if (ScreenCenter != Vector3.zero)
            {
                GetCenterScreen();
                ray = cam.ScreenPointToRay(ScreenCenter);
            }
            else
                ray = new Ray(cam.transform.position, cam.transform.forward);

            ray.origin += cam.transform.forward * ForwardCam; //Push the ray forward so it does not touch near colliders

            if (debug)
            {
                Debug.DrawRay(ray.origin, cam.transform.forward * MaxDistance, Color.gray);

            }

            var MaxPoint = ray.GetPoint(AimHit.distance);

            var hit = new RaycastHit { distance = MaxDistance, point = MaxPoint };

            return CalculateRayCasting(UseRaycasting, ray, ref hit);
        }

        public RaycastHit DirectionFromDirection(bool UseRaycasting)
        {
            RawAimDirection = AimOrigin.forward;

            Ray ray = new(AimOrigin.position, RawAimDirection);

            var hit = new RaycastHit()
            {
                distance = MaxDistance,
                point = ray.GetPoint(100)
            };

            return CalculateRayCasting(UseRaycasting, ray, ref hit);
        }


        public RaycastHit DirectionFromTarget(bool UseRaycasting)
        {
            var TargetCenter = this.TargetCenter; //Cache the Center

            RawAimDirection = AimOrigin.DirectionTo(TargetCenter);

            Ray ray = new(AimOrigin.position, RawAimDirection);

            var hit = new RaycastHit()
            {
                distance = MaxDistance,
                point = TargetCenter,
            };

            return CalculateRayCasting(UseRaycasting, ray, ref hit);
        }
        private RaycastHit CalculateRayCasting(bool UseRaycasting, Ray ray, ref RaycastHit hit)
        {
            if (UseRaycasting)
            {
                ArrayHits = new RaycastHit[RayHits];

                if (rayRadius > 0)
                    hitcount = Physics.SphereCastNonAlloc(ray, rayRadius, ArrayHits, MaxDistance, Layer, m_Triggers);
                else
                    hitcount = Physics.RaycastNonAlloc(ray, ArrayHits, MaxDistance, Layer, m_Triggers);

                if (hitcount > 0)
                {
                    foreach (RaycastHit rHit in ArrayHits)
                    {
                        if (rHit.point == Vector3.zero) continue;               //QUICK HACK?
                        if (rHit.transform == null) break;                      //Means nothing was found
                        if (SkipConditions(rHit.transform)) continue;

                        if (hit.distance > rHit.distance) hit = rHit;
                    }
                }

                return GetAimAssist(hit);
            }

            return hit;
        }

        private bool SkipConditions(Transform t)
        {
            if (t.SameHierarchy(IgnoreTransform)) return true;        //Dont Hit anything the Ignore
            if (t.SameHierarchy(OwnObjectCore)) return true;          //Dont Hit anything in the Object Core
            if (t == OwnObjectCore) return true;               //Dont Hit yorus
            if (t.SameHierarchy(AimOrigin)) return true;              //Dont Hit anything in this hierarchy

            return false;
        }


       // private IAimTarget IAimTargetAssist;

        /// <summary> Find if the Transform Hit with the RayCast is an AimAssist </summary>
        /// <param name="hit"></param>
        /// <returns></returns>
        private RaycastHit GetAimAssist(RaycastHit hit)
        {

#if UNITY_EDITOR
            hitName = hit.collider ? hit.collider.name : string.Empty; //For debbuging purposes
#endif

            IAimTarget IAimTargetAssist = hit.collider != null ? hit.collider.GetComponent<IAimTarget>() : null;
            IsTargetAssist = false;

            if (IAimTargetAssist != null)
            {
                if (IAimTargetAssist.AimAssist)
                {
                    IsTargetAssist = true;
                    AimRayTargetAssist = IAimTargetAssist.AimPoint;
                    hit.point = IAimTargetAssist.AimPoint.position;
                    //  Debug.Log("IAimTarg.AimAssist");
                }


                if (IAimTargetAssist != LastAimTarget)
                {
                    LastAimTarget?.IsBeenAimed(false, gameObject); //Make sure is no longer being aimed
                    LastAimTarget = IAimTargetAssist;
                    LastAimTarget.IsBeenAimed(true, gameObject);
                }
            }
            else
            {
                LastAimTarget?.IsBeenAimed(false, gameObject);
                LastAimTarget = null;

                AimRayTargetAssist = null;
            }

            return hit;
        }


        /// <summary>This is used to listen the Animator asociated to this gameObject </summary>
        public virtual bool OnAnimatorBehaviourMessage(string message, object value) => this.InvokeWithParams(message, value);

        private void HeadAimOrigin()
        {
            var anim = transform.FindComponent<Animator>();

            if (anim)
            {
                if (anim.isHuman)
                {
                    var head = anim.GetBoneTransform(HumanBodyBones.Head);
                    if (head) AimOrigin = head;
                }
                else
                {
                    AimOrigin = anim.transform.FindGrandChild("Head");
                }
            }
            MTools.SetDirty(this);
        }

#if UNITY_EDITOR
        void Reset()
        {
            if (MainCamera == null)
            {
                cam = MTools.FindMainCamera();
                if (cam)  MainCamera = cam.transform;
            }
            else
            {
                cam = MainCamera.GetComponent<Camera>();
            }

            AimOrigin = defaultOrigin = transform;
        }

        private void OnDrawGizmosSelected()
        {
            if (debug && enabled && !Application.isPlaying)
            {
                Gizmos.color = Color.green;
                if (AimOrigin != null)
                {
                    Gizmos.DrawRay(AimOrigin.position, AimOrigin.forward * MaxDistance);
                    if (rayRadius.Value > 0)
                    {

                        Gizmos.DrawWireSphere(AimOrigin.position + AimOrigin.forward * MaxDistance, rayRadius);
                        Gizmos.DrawWireSphere(AimOrigin.position, rayRadius);
                    }
                }
            }
        }

        void OnDrawGizmos()
        {
            if (debug && enabled)
            {
                if (Application.isPlaying)
                {
                    if (AimOrigin && !AimPoint.CloseToZero())
                    {
                        float radius = 0.05f;
                        Gizmos.color = Color.green;
                        Gizmos.DrawWireSphere(AimPoint, radius);
                        Gizmos.DrawSphere(AimPoint, radius);

                        Gizmos.DrawLine(AimOrigin.position, AimPoint);
                        Gizmos.color = Color.black;
                        Gizmos.DrawLine(AimOrigin.position, RawPoint);

                        if (UseRaycasting)
                        {
                            GUIStyle style = new(EditorStyles.label)
                            {
                                fontStyle = FontStyle.Bold,
                                fontSize = 10
                            };

                            style.normal.textColor = Color.green;

                            Handles.Label(AimPoint, hitName, style);
                        }
                    }
                }
            }
        }
#endif
    }
    #region Inspector


#if UNITY_EDITOR
    [CanEditMultipleObjects, CustomEditor(typeof(Aim))]
    public class AimEditor : Editor
    {
        Aim m;

        SerializedProperty m_active, m_aimOrigin, m_Smoothness, m_Animator, m_AimHorizontal, m_AimVertical,
            m_aimLayer, m_Triggers, m_AimTarget, m_AimPosition,
            m_AimSide, debug, m_UpdateMode, OnAiming, m_cycles, OnHit,
            m_Ignore, m_camera, m_UseCamera, m_forwardCam,
            m_screenCenter, rayRadius, RayHits, OnAimRayTarget, OnSetTarget, OnClearTarget,
            OnUsingTarget, OnScreenCenter, OnAimSide, EditorTab1, MaxDistance, m_UseRaycasting;
        private void OnEnable()
        {
            m_Animator = serializedObject.FindProperty("m_Animator");
            m_AimHorizontal = serializedObject.FindProperty("m_AimHorizontal");
            m_AimVertical = serializedObject.FindProperty("m_AimVertical");


            m = (Aim)target;

            m_active = serializedObject.FindProperty("m_active");
            OnHit = serializedObject.FindProperty("OnHit");
            m_Smoothness = serializedObject.FindProperty("m_Smoothness");
            m_aimOrigin = serializedObject.FindProperty("m_aimOrigin");

            m_aimLayer = serializedObject.FindProperty("m_aimLayer");
            m_Triggers = serializedObject.FindProperty("m_Triggers");
            m_AimTarget = serializedObject.FindProperty("m_AimTarget");
            m_AimPosition = serializedObject.FindProperty("m_AimPosition");
            m_Ignore = serializedObject.FindProperty("m_Ignore");

            m_camera = serializedObject.FindProperty("m_camera");
            m_UseCamera = serializedObject.FindProperty("m_UseCamera");

            m_forwardCam = serializedObject.FindProperty("m_forwardCam");

            m_screenCenter = serializedObject.FindProperty("m_screenCenter");
            m_AimSide = serializedObject.FindProperty("m_AimSide");
            rayRadius = serializedObject.FindProperty("rayRadius");
            MaxDistance = serializedObject.FindProperty("MaxDistance");
            RayHits = serializedObject.FindProperty("RayHits");
            EditorTab1 = serializedObject.FindProperty("EditorTab1");
            debug = serializedObject.FindProperty("debug");
            m_UpdateMode = serializedObject.FindProperty("updateMode");

            OnAimRayTarget = serializedObject.FindProperty("OnAimRayTarget");
            OnUsingTarget = serializedObject.FindProperty("OnUsingTarget");

            OnScreenCenter = serializedObject.FindProperty("OnScreenCenter");
            OnAimSide = serializedObject.FindProperty("OnAimSide");
            OnAiming = serializedObject.FindProperty("OnAiming");
            m_cycles = serializedObject.FindProperty("m_cycles");
            m_UseRaycasting = serializedObject.FindProperty("m_UseRaycasting");



            OnSetTarget = serializedObject.FindProperty("OnSetTarget");
            OnClearTarget = serializedObject.FindProperty("OnClearTarget");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription("Aim Logic. Uses raycasting to find object when the character is aiming with the camera or towards a target");
            EditorTab1.intValue = GUILayout.Toolbar(EditorTab1.intValue, new string[] { "General", "References", "Events" });


            if (Application.isPlaying)
            {
                var desc = m.AimTarget != null ?
                    "Using a Target" : (m.UseCamera ?
                    "Using the Camera" :
                    "Using the Aim Origin Forward Direction");
                MalbersEditor.DrawDescription(desc);
            }

            //First Tabs
            int Selection = EditorTab1.intValue;

            if (Selection == 0) ShowGeneral();
            else if (Selection == 1) ShowReferences();
            else if (Selection == 2) ShowEvents();


            serializedObject.ApplyModifiedProperties();
        }



        private void ShowGeneral()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(m_active);
                    if (EditorGUI.EndChangeCheck())
                    {
                        m.enabled = m.Active;
                        EditorUtility.SetDirty(target);
                    }
                    MalbersEditor.DrawDebugIcon(debug);
                }

                EditorGUILayout.PropertyField(m_UpdateMode);
                EditorGUILayout.PropertyField(m_AimSide);
                EditorGUILayout.PropertyField(m_Smoothness);
                EditorGUILayout.PropertyField(m_aimOrigin);
                if (m_aimOrigin.objectReferenceValue == null)
                    EditorGUILayout.HelpBox("Please Select an Aim Origin Reference", MessageType.Error);
            }


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))

            // EditorGUILayout.LabelField("Ray Casting", EditorStyles.boldLabel);
            {
                EditorGUILayout.PropertyField(m_UseRaycasting);
                {
                    if (m.UseRaycasting)
                    {
                        EditorGUILayout.PropertyField(m_aimLayer, new GUIContent("Layer"));
                        EditorGUILayout.PropertyField(m_Triggers);
                        EditorGUILayout.PropertyField(MaxDistance);
                        EditorGUILayout.PropertyField(rayRadius);
                        EditorGUILayout.PropertyField(RayHits);
                        EditorGUILayout.PropertyField(m_cycles);
                    }
                }
            }
           

            if (Application.isPlaying && debug.boolValue)
            {
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
                        EditorGUILayout.LabelField("Camera Side: " + (m.AimingSide ? "Right" : "Left"));
                        EditorGUILayout.FloatField("Vertical Angle", m.VerticalAngle);
                        EditorGUILayout.FloatField("Horizontal Angle", m.HorizontalAngle);
                        EditorGUILayout.ObjectField("Hit", m.AimHit.transform, typeof(Transform), false);
                    }
                }
            }
        }

        private void ShowReferences()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (m_AimTarget.isExpanded = MalbersEditor.Foldout(m_AimTarget.isExpanded, "Target"))
                {
                    EditorGUILayout.PropertyField(m_AimTarget, new GUIContent("Target"));
                }
            }


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (m_camera.isExpanded = MalbersEditor.Foldout(m_camera.isExpanded, "Camera"))
                {
                    EditorGUILayout.PropertyField(m_UseCamera);

                    if (m.m_UseCamera.Value)
                    {
                        EditorGUILayout.PropertyField(m_camera);
                        EditorGUILayout.PropertyField(m_forwardCam);
                        EditorGUILayout.PropertyField(m_screenCenter);
                    }
                }
            }


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (m_Animator.isExpanded = MalbersEditor.Foldout(m_Animator.isExpanded, "Animator"))
                {
                    EditorGUILayout.PropertyField(m_Animator);

                    MalbersEditor.DisplayParam(m.m_Animator, m_AimHorizontal, AnimatorControllerParameterType.Float);
                    MalbersEditor.DisplayParam(m.m_Animator, m_AimVertical, AnimatorControllerParameterType.Float);
                    //EditorGUILayout.PropertyField(AngleLerp);

                }
            }


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (m_Ignore.isExpanded = MalbersEditor.Foldout(m_Ignore.isExpanded, "Extras"))
                {
                    EditorGUILayout.LabelField("Extras", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(m_Ignore);
                    EditorGUILayout.PropertyField(m_AimPosition);
                }
            }
        }

        private void ShowEvents()
        {
            EditorGUILayout.PropertyField(OnAiming);
            EditorGUILayout.PropertyField(OnHit);
            EditorGUILayout.PropertyField(OnAimRayTarget);
            EditorGUILayout.PropertyField(OnUsingTarget);
            EditorGUILayout.PropertyField(OnClearTarget);
            EditorGUILayout.PropertyField(OnSetTarget);
            EditorGUILayout.PropertyField(OnScreenCenter);
            EditorGUILayout.PropertyField(OnAimSide);
        }
    }
#endif
    #endregion
}