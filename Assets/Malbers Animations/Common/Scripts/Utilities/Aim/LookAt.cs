using MalbersAnimations.Scriptables;
using UnityEngine;
using MalbersAnimations.Events;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Utilities
{
    /// <summary>Used for Animal that have Animated Physics enabled </summary>
    [DefaultExecutionOrder(500)/*,[RequireComponent(typeof(Aim))*/]
    [AddComponentMenu("Malbers/Utilities/Aiming/Look At")]

    public class LookAt : MonoBehaviour, IAnimatorListener, ILookAtActivation
    {
        [System.Serializable]
        public class BoneRotation
        {
            /// <summary> Transform reference for the Bone </summary>
           [RequiredField] public Transform bone;                                          //The bone
            public Vector3 offset = new(0, -90, -90);               //The offset for the look At
            [Range(0, 1)] public float weight = 1;                          //the Weight of the look a
            internal Quaternion nextRotation;
            internal Quaternion defaultRotation;

            [Tooltip("Is not a bone driven by the Animator")]
            public bool external;


        }

        public BoolReference active = new(true);     //For Activating and Deactivating the HeadTrack

        private IGravity a_UpVector;

        [Tooltip("Reference for the Aim Component")]
        [RequiredField] public Aim aimer;

        /// <summary>Max Angle to LookAt</summary>
        [Space, Tooltip("Max Angle to LookAt")]
        public FloatReference LimitAngle = new(80f);                              
        /// <summary>Smoothness between Enabled and Disable</summary>
        [Tooltip("Smoothness between Enabled and Disable")]
        public FloatReference Smoothness = new(5f);

        /// <summary>Smoothness between Enabled and Disable</summary>
        [Tooltip("Use the LookAt only when there's a Force Target on the Aim... use this when the Animal is AI Controlled")]
        [SerializeField] private BoolReference onlyTargets = new(false);

        [Space]
        public BoneRotation[] Bones;      //Bone Chain    
       // private  Quaternion[] LocalRot;      //Bone Chain    

        public BoolEvent OnLookAtActive = new();


        public bool debug = true;
        public float LookAtWeight { get; private set; }
        /// <summary>Angle created between the transform.Forward and the LookAt Point   </summary>
        protected float angle;

        /// <summary>Means there's a camera or a Target to look At</summary>
        public bool HasTarget { get; set; }
        public Vector3 UpVector => a_UpVector != null ? a_UpVector.UpVector : Vector3.up;


        private Transform EndBone;

        /// <summary>Direction Stored on the Aim Script</summary>
        public Vector3 AimDirection => aimer.AimDirection;

        private bool isAiming;
        /// <summary>Check if is on the Angle of Aiming</summary>
        public bool IsAiming
        {
            get
            {
                var check = Active && CameraAndTarget && ActiveByAnimation && (angle < LimitAngle);

                if (check != isAiming)
                {
                    isAiming = check;
                    OnLookAtActive.Invoke(isAiming);

                    if (!isAiming)
                    {
                        ResetBoneLocalRot();
                    }
                    else
                    {
                        for (int i = 0; i < Bones.Length; i++)
                        {
                            Bones[i].nextRotation = Bones[i].bone.rotation; //Save the Local Rotation of the Bone
                        }
                    }
                }
                return isAiming;
            } 
        }

    

        /// <summary> Enable Disable the Look At </summary>
        public bool Active
        {
            get => active;
            set => active.Value = value;
        }


        //bool activebyAnim;
        /// <summary> Enable/Disable the LookAt by the Animator</summary>
        public bool ActiveByAnimation { get; set; }
        //{
        //    get => activebyAnim;
        //    set 
        //    {
        //        activebyAnim = value;
        //        Debug.Log($"activebyAnim {activebyAnim}");
        //    }
            
        //}

        /// <summary>The Character is using a Camera to Look?</summary>
        bool CameraAndTarget { get; set; }

        /// <summary>When set to True the Look At will only function with Targets gameobjects only instead of the Camera forward Direction</summary>
        public bool OnlyTargets { get => onlyTargets.Value; set => onlyTargets.Value = value; }

        void Awake()
        {
            a_UpVector = gameObject.FindInterface<IGravity>(); //Get Up Vector

            if (aimer == null)
                aimer = gameObject.FindInterface<Aim>();  //Get the Aim Component

            aimer.IgnoreTransform = transform;
            ActiveByAnimation = true;
            EnablePriority = 1;
            foreach (var item in Bones)
            {
                if (item.bone == null)
                {
                    Debug.LogError($"LookAt in [{name}] has missing/empty bones. Please fill the reference. Disabling [LookAt]", this);
                    enabled = false;
                    break;
                }
            }
        }

        void Start()
        {
            if (Bones != null && Bones.Length > 0) 
                EndBone = Bones[^1].bone;

            if (aimer.AimOrigin == null || aimer.AimOrigin == EndBone) 
                aimer.AimOrigin = Bones[0].bone.parent;
            
            for (int i = 0; i < Bones.Length; i++)
            {
                 Bones[i].defaultRotation = Bones[i].bone.localRotation; //Save the Local Rotation of the Bone
            }
        }

        void ResetBoneLocalRot()
        {
            for (int i = 0; i < Bones.Length; i++)
            {
                Bones[i].bone.localRotation = Bones[i].defaultRotation; //Save the Local Rotation of the Bone
            }
        }


        void LateUpdate()
        {
            // if (Time.time < float.Epsilon || Time.timeScale <= 0) return; //Do not look when the game is paused

            if (!aimer.UseCamera && aimer.AimTarget == null) { CameraAndTarget = false; }
            else
            {
                //If Only Target is true then Disable it because we do not have any target
                if (OnlyTargets)
                {
                    CameraAndTarget = (aimer.AimTarget != null);
                }
                //If Only Target is false and there's no Camera then Disable it because we do not have any target
                else
                {
                    CameraAndTarget = (aimer.MainCamera != null) || !aimer.UseCamera;
                }
            }


            angle = Vector3.Angle(transform.forward, AimDirection);
            LookAtWeight = Mathf.MoveTowards(LookAtWeight, IsAiming ? 1 : 0, Time.deltaTime * Smoothness / 2);

           if (LookAtWeight == 0) return; //Do nothing on Weight Zero

            LookAtBoneSet_AnimatePhysics();            //Rotate the bones
        }

        /// <summary>Enable Look At from the Animator (Needs Layer)</summary>
        public void EnableLookAt(int layer) => EnableByPriority(layer + 1);

        /// <summary>Disable Look At from the Animator (Needs Layer)</summary>
        public void DisableLookAt(int layer) => DisableByPriority(layer + 1);

        public virtual void SetTargetOnly(bool val) => OnlyTargets = val;

        public virtual void EnableByPriority(int priority)
        {
            if (priority >= DisablePriority)
            {
                EnablePriority = priority;
                if (DisablePriority == EnablePriority) DisablePriority = 0;
            }
            ActiveByAnimation = (EnablePriority > DisablePriority);

             //Debug.Log("ActiveByAnimation: "+ ActiveByAnimation);
        }

        public virtual void ResetByPriority(int priority)
        {
            if (EnablePriority == priority) EnablePriority = 0;
            if (DisablePriority == priority) DisablePriority = 0;

            ActiveByAnimation = (EnablePriority > DisablePriority);
        }


        public virtual void DisableByPriority(int priority)
        {
            if (priority >= EnablePriority)
            {
                DisablePriority = priority;
                if (DisablePriority == EnablePriority)  EnablePriority = 0;
            }

           // Debug.Log("Dis");
            ActiveByAnimation = (EnablePriority > DisablePriority);
        }


        //bool OverridePriority;
        //bool lastActivation;  
        public int EnablePriority { get; private set; }
        public int DisablePriority { get; private set; }

        //private int[] LayersPriority = new int[20];



        /// <summary>Rotates the bones to the Look direction for FIXED UPTADE ANIMALS</summary>
        void LookAtBoneSet_AnimatePhysics()
        {
            // CalculateAiming();

            for (int i = 0; i < Bones.Length; i++)
            {
                var bn = Bones[i];

                if (!bn.bone) continue;

                if (IsAiming)
                {
                    var BoneAim = Vector3.Slerp(transform.forward, AimDirection, bn.weight).normalized;
                    var TargetTotation = Quaternion.LookRotation(BoneAim, UpVector) * Quaternion.Euler(bn.offset);
                    bn.nextRotation = Quaternion.Lerp(bn.nextRotation, TargetTotation, LookAtWeight);
                }
                else
                {
                    if (!bn.external)
                    {
                        bn.nextRotation = Quaternion.Lerp(bn.bone.rotation, bn.nextRotation, LookAtWeight);
                    }
                 // if (LookAtWeight ==0)  bn.nextRotation = bn.bone.rotation;
                }

                if (LookAtWeight != 0)
                {
                    if (bn.external && !IsAiming)
                    {
                        bn.nextRotation = Quaternion.Lerp(bn.nextRotation, bn.defaultRotation, 1-LookAtWeight);
                        bn.bone.localRotation = Quaternion.Lerp(bn.bone.localRotation, bn.nextRotation, LookAtWeight); //LOCAL ROTATION!!!
                      
                    }
                    else
                    {
                        bn.bone.rotation = bn.nextRotation;
                    }
                }
                //else
                //{
                //    bn.nextRotation = bn.bone.rotation;
                //}
            }
        }

        /// <summary>This is used to listen the Animator asociated to this gameObject </summary>
        public virtual bool OnAnimatorBehaviourMessage(string message, object value) => this.InvokeWithParams(message, value);

        void OnValidate()
        {
            if (Bones != null && Bones.Length > 0)
            {
                EndBone = Bones[^1].bone;
            }
        }

        void Reset()
        {
            aimer = gameObject.FindInterface<Aim>();
            if (aimer == null) aimer = gameObject.AddComponent<Aim>();
        }


#if UNITY_EDITOR && MALBERS_DEBUG
        private void OnDrawGizmos()
        {
            bool AppIsPlaying = Application.isPlaying;

            if (debug)
            {
                UnityEditor.Handles.color = IsAiming || !AppIsPlaying ? new Color(0, 1, 0, 0.1f) : new Color(1, 0, 0, 0.1f);
 
                if (EndBone != null)
                {
                    UnityEditor.Handles.DrawSolidArc(EndBone.position, UpVector, Quaternion.Euler(0, -LimitAngle, 0) * transform.forward, LimitAngle * 2, 1);
                    UnityEditor.Handles.color = IsAiming || !AppIsPlaying ? Color.green : Color.red;
                    UnityEditor.Handles.DrawWireArc(EndBone.position, UpVector, Quaternion.Euler(0, -LimitAngle, 0) * transform.forward, LimitAngle * 2, 1);
                }
            }
        }
#endif
    }



#if UNITY_EDITOR
    [CustomEditor(typeof(LookAt))]
    public class LookAtED : Editor
    {
        LookAt M;
        void OnEnable()
        {
            M = (LookAt) target;
        }

        public override void OnInspectorGUI()
        {

            if (M.aimer  && M.Bones != null)
            {
                var origin = M.aimer.AimOrigin;

                if (origin == null)
                {
                    EditorGUILayout.HelpBox($" Please add a Aim Origin to the Aimer Component", MessageType.Error);
                }
                else
                    foreach (var bn in M.Bones)
                    {
                        if (bn.bone != null && origin.SameHierarchy(bn.bone))
                        {
                            EditorGUILayout.HelpBox($"Aimer Origin [{origin.name}] is child of [{bn.bone.name}]." +
                                $" Use a different Aimer Origin that is not child of [{bn.bone.name}]", MessageType.Error);
                            break;
                        }
                    }
            }
            base.OnInspectorGUI();

            if (Application.isPlaying && M.debug)
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.FloatField("LookAtWeight", M.LookAtWeight);
                    EditorGUILayout.Toggle("Active by Animation", M.ActiveByAnimation);
                    EditorGUILayout.IntField("Enable Priority", M.EnablePriority);
                    EditorGUILayout.IntField("Disable Priority", M.DisablePriority);
                    Repaint();
                }
            }
        }
    }
#endif
}