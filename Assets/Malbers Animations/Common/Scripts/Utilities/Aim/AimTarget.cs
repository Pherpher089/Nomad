using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization; 

namespace MalbersAnimations.Utilities
{
    /// <summary>For when someone with LookAt enters it will set this transform as the target</summary>
    [AddComponentMenu("Malbers/Utilities/Aiming/Aim Target")]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/utilities/aim-target")]
    public class AimTarget : MonoBehaviour, IAimTarget
    {
        /// <summary>All Active AimTargets in the current scene</summary>
        public static List<AimTarget> AimTargets;

        /// <summary>This will set AutoAiming for the Aim Logic</summary>
        [SerializeField, Tooltip("It will center the Aim Ray into this gameObject's collider")]
        private bool aimAssist;

       

        [SerializeField, Tooltip("Transform Point for to center the Aim Ray")]
        [FormerlySerializedAs("m_AimPoint")]
        private Transform m_AimCenter;


        /// <summary>This will set AutoAiming for the Aim Logic</summary>
        [SerializeField, Tooltip("The Aim Assist will use Own Trigers to find Aimers")]
        private bool UseOnTriggerEnter;
        [Tooltip("Layer to check the Aimer")]
        [SerializeField] private LayerReference layer = new(-1);
        public LayerMask Layer { get => layer.Value; set => layer.Value = value; }
        [Tooltip("Search only Tags")]
        public Tag[] Tags;

        //   public Vector3Reference Offset;

        private IAim aim;

        public GameObjectEvent OnAimEnter = new();
        public GameObjectEvent OnAimExit = new();


        public bool debug;

        //public System.Action<AimTarget> OnAddedAimTarget { get; private set; } = delegate { };
        //public System.Action<AimTarget> OnRemovedAimTarget { get; private set; } = delegate { };


        /// <summary>This will set AutoAiming for the Aim Logic</summary>
        public bool AimAssist { get => aimAssist; set => aimAssist = value; }
        public bool IsBeingAimed { get; set; }
        // public bool AimedFocused { get; set; }
        public Transform AimPoint => m_AimCenter;


        protected virtual void OnEnable()
        {
            if (m_AimCenter == null) m_AimCenter = transform;
            if (AimTargets == null) AimTargets = new List<AimTarget>();
            AimTargets.Add(this);
            //  OnAddedAimTarget(this);
        }

        protected virtual void OnDisable()
        {
            AimTargets.Remove(this);
            //if (AimedFocused) OnAimExit.Invoke(null);
            // OnRemovedAimTarget(this);
        }

        private void OnValidate()
        {
            if (m_AimCenter == null) m_AimCenter = transform;
        }

        /// <summary>Is the target been aimed by the Aim Ray of the Aim Script</summary>
        public void IsBeenAimed(bool enter, GameObject AimedBy)
        {
            if (debug) Debug.Log($"[{name}] Is Being Aimed by [{AimedBy.name}]", this);

            IsBeingAimed = enter;

            if (enter)
                OnAimEnter.Invoke(AimedBy);
            else
                OnAimExit.Invoke(AimedBy);
        }


        public bool TrueConditions(Collider other)
        {
            if (!enabled) return false;

            if (Tags != null && Tags.Length > 0)
            {
                if (!other.gameObject.HasMalbersTagInParent(Tags)) return false;
            }

            if (other == null) return false; // you are CALLING A ELIMINATED ONE
            if (other.isTrigger) return false; // Check Trigger Interactions 

            if (!MTools.Layer_in_LayerMask(other.gameObject.layer, Layer)) return false;
            if (transform.IsChildOf(other.transform)) return false;                 // Do not Interact with yourself

            return true;
        }


        /// Aim Targets can be also used as Trigger Enter Exit 
        void OnTriggerEnter(Collider other)
        {
            if (!TrueConditions(other)) return;

            IAim Aimer = other.FindInterface<IAim>();

            // Aimer ??= other.FindInterface<IObjectCore>().transform.FindInterface<IAim>();

            if (Aimer != null && aim != Aimer)
            {
                if (debug) Debug.Log($"OnTrigger Enter [{other.name}]", this);
                Aimer.AimTarget = AimPoint;
                aim = Aimer;
                OnAimEnter.Invoke(other.gameObject);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (!TrueConditions(other)) return;

            IAim Aimer = other.FindInterface<IAim>();

            if (Aimer != null && aim == Aimer)
            {
                Aimer.AimTarget = null;
                aim = null;
                OnAimExit.Invoke(other.gameObject);
                if (debug) Debug.Log($"OnTrigger Exit [{other.name}]", this);
            }
        }
    }
}