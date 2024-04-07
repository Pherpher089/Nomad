using UnityEngine;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/AI/Follow Target")]

    /// <summary> Simple follow target for the animal </summary>
    public class FollowTarget : MonoBehaviour
    {
        public Transform target;
        [Min(0)] public float stopDistance = 3;
        [Min(0)] public float SlowDistance = 6;
        [Tooltip("Limit for the Slowing Multiplier to be applied to the Speed Modifier")]
        [Range(0, 1)]
        [SerializeField] private float slowingLimit = 0.3f;
        ICharacterMove animal;




        /// <summary>Used to Slow Down the Animal when its close the Destination</summary>
        public float SlowMultiplier
        {
            get
            {
                var result = 1f;
                if (SlowDistance > stopDistance && RemainingDistance < SlowDistance)
                    result = Mathf.Max(RemainingDistance / SlowDistance, slowingLimit);

                return result;
            }
        }

        private float RemainingDistance;

        // Use this for initialization
        void Start()
        {
            animal = GetComponentInParent<ICharacterMove>();
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 Direction = (target.position - transform.position).normalized;          //Calculate the direction from the animal to the target
            RemainingDistance = Vector3.Distance(transform.position, target.position);      //Calculate the distance..


            //Move the Animal if we are not on the Stop Distance Radius
            animal.Move(RemainingDistance > stopDistance ? Direction * SlowMultiplier : Vector3.zero);
        }

        private void OnDisable()
        {
            animal.Move(Vector3.zero);      //In case this script gets disabled stop the movement of the Animal
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            var center = transform.position;

            if (Application.isPlaying && target)
            {

                center = target.position;
            }

            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.DrawWireDisc(center, Vector3.up, stopDistance);

            if (SlowDistance > stopDistance)
            {
                UnityEditor.Handles.color = Color.cyan;
                UnityEditor.Handles.DrawWireDisc(center, Vector3.up, SlowDistance);
            }
        }
#endif
    }
}