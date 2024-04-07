using MalbersAnimations.Utilities;
using UnityEngine;
using static MalbersAnimations.ColliderReaction;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Utilities/Tools/Use Transform")]

    public class UseTransform : MonoBehaviour
    {
        public enum UpdateMode                                          // The available methods of updating are:
        {
            Update = 1,
            FixedUpdate = 2,                                            // Update in FixedUpdate (for tracking rigidbodies).
            LateUpdate = 4,                                             // Update in LateUpdate. (for tracking objects that are moved in Update)
        }


        public enum XYZEnum { X = 1, Y = 2, Z = 4 }


        [Tooltip("Transform to use the Position as Reference")]
        public Transform Reference;
      
        [Tooltip("Use the Reference's Position")]
        public bool position = true;
        [Hide(nameof(position))]
        public UpdateMode PositionUpdate = UpdateMode.FixedUpdate;

        [Hide(nameof(position))]
        [Flag]
        public XYZEnum posAxis = XYZEnum.X | XYZEnum.Y | XYZEnum.Z;
        [Hide(nameof(position))]
        [Min(0)] public float lerpPos = 0f;

        [Tooltip("Use the Reference's Rotation")]
        public bool rotation = true;
        [Hide(nameof(rotation))]
        public UpdateMode RotationUpdate = UpdateMode.LateUpdate;


        [Hide(nameof(rotation))]
        [Min(0)] public float lerpRot = 0f;

        // Update is called once per frame
        void Update()
        {
            if (Reference == null) return; 

            if (PositionUpdate == UpdateMode.Update) SetPositionReference(Time.deltaTime);
            if (RotationUpdate == UpdateMode.Update) SetRotationReference(Time.deltaTime);
        }

        void LateUpdate()
        {
            if (Reference == null) return;

            if (PositionUpdate == UpdateMode.LateUpdate) SetPositionReference(Time.deltaTime);
            if (RotationUpdate == UpdateMode.LateUpdate) SetRotationReference(Time.deltaTime);
        }

        void FixedUpdate()
        {
            if (Reference == null) return;

            if (PositionUpdate == UpdateMode.FixedUpdate) SetPositionReference(Time.fixedDeltaTime);
            if (RotationUpdate == UpdateMode.FixedUpdate) SetRotationReference(Time.fixedDeltaTime);
        }

        private void SetPositionReference(float delta)
        { 
            if (position)
            {
                var newPos = transform.position;

                if ((posAxis & XYZEnum.X) == XYZEnum.X) newPos.x = Reference.position.x;
                if ((posAxis & XYZEnum.Y) == XYZEnum.Y) newPos.y = Reference.position.y;
                if ((posAxis & XYZEnum.Z) == XYZEnum.Z) newPos.z = Reference.position.z;


                transform.position = Vector3.Lerp(transform.position, newPos, lerpPos == 0 ? 1 : delta * lerpPos);
            }
        }


        private void SetRotationReference(float delta)
        { 
            if (rotation)
                transform.rotation = Quaternion.Lerp(transform.rotation, Reference.rotation, lerpRot == 0 ? 1 : delta * lerpRot);
        }
    }
}