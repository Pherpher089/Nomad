using UnityEngine;

namespace MalbersAnimations.Controller
{
    [AddComponentMenu("Malbers/Animal Controller/Rotate in Place by Camera")]

    public class RotateInPlaceByCamera : MonoBehaviour
    {
        [RequiredField]  public MAnimal animal;
        [Min(15)] public float LimitAngle = 90f;
        [Min(1)] public float AngleThreshold = 2f;
        [Min(0)] public float Wait = 1f;
        public bool RootMotionOnly = true;

        public bool debug = true;
        
        private void OnEnable()
        {
            animal.PreInput += PreInput;
            animal.PostStateMovement += PostStateMovement;
        }


        private void OnDisable()
        {
            animal.PreInput -= PreInput;
            animal.PostStateMovement -= PostStateMovement;
        }

      
        //
        private bool RotateInPlace;
        private Vector3 TargetRotation;
        private float angle;
        private float releaseTime;
        private bool waitTime;

        private void PostStateMovement(MAnimal animal)
        {
            //Only do RootMotion, Remove all additive position.
            if (RotateInPlace && RootMotionOnly)
            {
                animal.AdditiveRotation = animal.Anim.deltaRotation; 
            }
        }

        private void PreInput(MAnimal animal)
        {
            //Do nothing if movement is detected, locomotion Idle is NOT playing or Strafe is true
            if (animal.RawInputAxis != Vector3.zero || animal.ActiveStateID.ID > 1 || animal.Strafe)
            {
                RotateInPlace = false;
                animal.Rotate_at_Direction = false;
                releaseTime = Time.time;
                waitTime = false;
                return;
            }

            if (!waitTime && MTools.ElapsedTime(releaseTime, Wait))
            {
                waitTime = true;
            }

            if (waitTime)
            {
                TargetRotation = Vector3.ProjectOnPlane(animal.MainCamera.transform.forward, Vector3.up).normalized;
                angle = Vector3.Angle(animal.transform.forward, TargetRotation);


                if (debug)
                {
                    MDebug.DrawRay(transform.position + Vector3.up * 0.01f, TargetRotation, Color.yellow);
                    MDebug.DrawRay(transform.position + Vector3.up * 0.01f, transform.forward, Color.yellow);
                }


                if (RotateInPlace)
                {
                    animal.RotateAtDirection(TargetRotation);

                    if (angle <= AngleThreshold)
                    {
                        if (debug) Debug.Log($"Stoping Rotate In Place ");

                        RotateInPlace = false;
                        animal.Rotate_at_Direction = false;
                    }
                }
                else
                {
                    if (angle >= LimitAngle)
                    {
                        RotateInPlace = true;
                    }
                }
            }
        }
 
        private void Reset()
        {
            animal = GetComponent<MAnimal>();
        }



#if UNITY_EDITOR && MALBERS_DEBUG
        private void OnDrawGizmos()
        {
            if (debug && animal)
            {
                UnityEditor.Handles.color = new Color(1, 1, 0, 0.01f);
                UnityEditor.Handles.DrawSolidArc(transform.position, animal.UpVector, Quaternion.Euler(0, -LimitAngle, 0) * transform.forward, LimitAngle * 2, 1);
                UnityEditor.Handles.color = Color.yellow;
                UnityEditor.Handles.DrawWireArc(transform.position, animal.UpVector, Quaternion.Euler(0, -LimitAngle, 0) * transform.forward, LimitAngle * 2, 1);
            }
        }
#endif
    }
}
