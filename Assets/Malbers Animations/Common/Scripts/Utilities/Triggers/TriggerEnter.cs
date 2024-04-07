using UnityEngine;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations.Utilities
{
    /// <summary> This is used when the collider is in a different gameObject and you need to check the Trigger Events
    /// Create this component at runtime and subscribe to the UnityEvents </summary>
    [AddComponentMenu("Malbers/Utilities/Colliders/Trigger Enter")]
    [SelectionBase]
    public class TriggerEnter : MonoBehaviour
    {
        public LayerReference Layer = new LayerReference(-1);
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        [Tooltip("On Trigger Enter only works with the first colliders that enters")]
        public bool UseOnce;

        [Tooltip("Search only Tags")]
        public Tag[] Tags;

        public ColliderEvent onTriggerEnter = new ColliderEvent();

        /// <summary> Collider Component used for the Trigger Proxy </summary>
        public Collider OwnCollider { get; private set; }
        public bool Active { get => enabled; set => enabled = value; }

        public QueryTriggerInteraction TriggerInteraction { get => triggerInteraction; set => triggerInteraction = value; }

        private void OnEnable()
        {
            OwnCollider = GetComponent<Collider>();
           
            Active = true;

            if (OwnCollider)
            {
                OwnCollider.isTrigger = true;
            }
            else
            {
                Active = false;
                Debug.LogError("This Script requires a Collider, please add any type of collider",this);
            }
        }
        public bool TrueConditions(Collider other)
        {
            if (!Active) return false;
            if (Tags != null && Tags.Length > 0)
            {
                if (!other.gameObject.HasMalbersTagInParent(Tags)) return false;
            }

            if (OwnCollider == null) return false; // you are 
            if (other == null) return false; // you are CALLING A ELIMINATED ONE
            if (triggerInteraction == QueryTriggerInteraction.Ignore && other.isTrigger) return false; // Check Trigger Interactions 
            if (!MTools.Layer_in_LayerMask(other.gameObject.layer, Layer)) return false;
            if (transform.IsChildOf(other.transform)) return false;                 // Do not Interact with yourself
            if ( other.transform.SameHierarchy(transform)) return false;    // Do not Interact with yourself

            return true;
        }
        void OnTriggerEnter(Collider other)
        {
            if (TrueConditions(other))
            {
                onTriggerEnter.Invoke(other);
                if (UseOnce)
                {
                    Active = false;
                    OwnCollider.enabled = false;
                }
            }
        }
    }
}