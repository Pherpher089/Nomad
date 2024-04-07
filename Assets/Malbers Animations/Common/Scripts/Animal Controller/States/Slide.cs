using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    public class Slide : State
    {
        public override string StateName => "Slide";
        public override string StateIDName => "Slide";


        [Tooltip("Lerp value for the Aligment to the surface")]
        public FloatReference OrientLerp = new(10f);


      
        [Tooltip("When Sliding the Animal will be able to orient towards the direction of this given angle")]
        public FloatReference RotationAngle = new(30f);
        [Tooltip("When Sliding the Animal will be able to Move horizontally with this value")]
        public FloatReference SideMovement = new(5f);

        [Tooltip("Exit Speed when there's no Slope")]
        public FloatReference ExitSpeed = new(0.5f);


        [Header("Exit Status")]
        [Tooltip("The Exit Status will be set to 1 if the Exit Condition was the Exit Speed")]
        public IntReference ExitSpeedStatus = new(1);
        [Tooltip("The Exit Status will be set to 2 if the Exit Condition was that there's no longer a Ground Changer")]
        public IntReference NoChangerStatus = new(2);

        public override bool TryActivate()
        {
            return TrySlideGround();
        }

        public override void OnPlataformChanged(Transform newPlatform)
        {
            //Debug.Log($"OnPlataformChanged {(newPlatform ? newPlatform.name : "null")}");

            if (!IsActiveState && TrySlideGround() && CanBeActivated)
            {
                Activate();
            }
            else if (IsActiveState && !animal.InGroundChanger && CanExit)
            {
                Debugging("[Allow Exit] No Ground Changer");
                SetExitStatus(NoChangerStatus);
                AllowExit();
            }
        }


        /// <summary>  The State moves always forward  </summary>
        public override bool KeepForwardMovement => true;

        public override void Activate()
        {
            base.Activate();
            IgnoreRotation = animal.GroundChanger.SlideData.IgnoreRotation;
        }

        private bool TrySlideGround()
        {
            //Debug.Log($"InGroundChanger {animal.InGroundChanger}");

            //if (animal.InGroundChanger)
            //{
            //    Debug.Log($" Slide {animal.GroundChanger.SlideData.Slide}, slideAngle {animal.SlopeDirectionAngle > animal.GroundChanger.SlideData.MinAngle}");
            //}




            if (animal.InGroundChanger
                && animal.GroundChanger.SlideData.Slide                                     //Meaning the terrain is set to slide
                && animal.SlopeDirectionAngle > animal.GroundChanger.SlideData.MinAngle     //The character is looking at the Direction of the slope
               // && animal.HorizontalSpeed > ExitSpeed
                //&& !animal.DeepSlope
                )
            { 
                //CHECK THE DIRECTION OF THE SLIDE
                if (Vector3.Angle(animal.Forward, animal.SlopeDirection) < animal.GroundChanger.ActivationAngle)
                {
                    return true;
                }
            }

            return false;
        }


        public override void InputAxisUpdate()
        {
            var move = animal.RawInputAxis;

            if (AlwaysForward) animal.RawInputAxis.z = 1;

            DeltaAngle = move.x;
            var NewInputDirection = Vector3.ProjectOnPlane( animal.SlopeDirection, animal.UpVector);

            if (animal.MainCamera)
            {
                //Normalize the Camera Forward Depending the Up Vector IMPORTANT!
                var Cam_Forward = Vector3.ProjectOnPlane(animal.MainCamera.forward, UpVector).normalized;
                var Cam_Right = Vector3.ProjectOnPlane(animal.MainCamera.right, UpVector).normalized;

                move = (animal.RawInputAxis.z * Cam_Forward) + (animal.RawInputAxis.x * Cam_Right);
                DeltaAngle = Vector3.Dot(animal.Right, move);
            }

            NewInputDirection = Quaternion.AngleAxis(RotationAngle * DeltaAngle, animal.Up) * NewInputDirection;
            animal.MoveFromDirection(NewInputDirection);  //Move using the slope Direction instead

           // MDebug.Draw_Arrow(transform.position, NewInputDirection, Color.green);

          

            moveSmooth = Vector3.Lerp(moveSmooth,Vector3.Project( move,animal.Right), animal.DeltaTime * CurrentSpeed.lerpPosition);

            if(GizmoDebug)
            MDebug.Draw_Arrow(transform.position, moveSmooth, Color.white);

        }

        Vector3 moveSmooth;

        float DeltaAngle;

        public override Vector3 Speed_Direction()
        {
            var NewInputDirection = animal.SlopeDirection;

            if (!IgnoreRotation)
            {
                NewInputDirection = Quaternion.AngleAxis(RotationAngle * DeltaAngle, animal.Up) * NewInputDirection;
            }

            return NewInputDirection;
        }

        private bool IgnoreRotation;

        public override void OnStateMove(float deltatime)
        {
            if (InCoreAnimation)
            {
                var Right = Vector3.Cross(animal.Up, animal.SlopeDirection);

                Right = Vector3.Project(animal.MovementAxisSmoothed, Right);

                if (GizmoDebug)
                    MDebug.Draw_Arrow(transform.position, Right, Color.red );


                

                animal.AdditivePosition += deltatime * SideMovement * moveSmooth;

                //Orient to the Ground
                animal.AlignRotation(animal.SlopeNormal, deltatime, OrientLerp);


                if (IgnoreRotation)
                {
                    animal.AlignRotation(animal.Forward, animal.SlopeDirection, deltatime, OrientLerp); //Make your own Aligment
                    animal.UseAdditiveRot = false; //Remove Rotations
                }

                if (!animal.Grounded)
                {
                    animal.UseGravity = true;
                }
            }
        }


        public override void TryExitState(float DeltaTime)
        {
            if (animal.HorizontalSpeed <= ExitSpeed && animal.GroundChanger == null)
            {
                Debugging("[Allow Exit] Speed is Slow");
                SetExitStatus(ExitSpeedStatus);
                AllowExit();
                return;
            }
            if (animal.GroundChanger == null || !animal.GroundChanger.SlideData.Slide)
            {
                Debugging("[Allow Exit] No Ground Changer");

                SetExitStatus(NoChangerStatus);
                AllowExit();
                return;
            }
            if (!animal.GroundChanger.SlideData.Slide)
            {
                Debugging("[Allow Exit] Ground Changer Slide Data is False");

                SetExitStatus(NoChangerStatus);
                AllowExit();
                return;
            }
        }


        internal override void Reset()
        {
            base.Reset();

            General = new AnimalModifier()
            {
                RootMotion = true,
                Grounded = true,
                Sprint = true,
                OrientToGround = false,
                CustomRotation = true,
                IgnoreLowerStates = true,
                AdditivePosition = true,
                AdditiveRotation = true,
                Gravity = false,
                modify = (modifier)(-1),
            };
        }
    }
}