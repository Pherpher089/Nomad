using MalbersAnimations.Reactions;
using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    /// <summary>This will be in charge of the Movement While is on the Ground </summary>
    public class Locomotion : State
    {
        [System.Serializable]
        public class WallStopProfiles
        {
            [Tooltip("Speed Index to Identify the Profile (Walk = 1, Trot = 2, etc)")]
            public float SpeedIndex;
            [Tooltip("Speed Index to Identify the Profile (Walk = 1, Trot = 2, etc)")]
            public float RayLength;
            [Tooltip("Reaction to do if the Animal touches a Wall")]
            [SerializeReference, SubclassSelector]
            public Reaction reaction;

            [Tooltip("Reaction if there's no wall detected in front of the animal")]
            [SerializeReference, SubclassSelector]
            public Reaction NoWallDetected;
        }

        public override string StateName => "Locomotion";
        public override string StateIDName => "Locomotion";
        [Header("Locomotion Parameters")]

        [Tooltip("Backward Offset Position of the BackFall Ray")]
        public FloatReference FallRayBackwards = new(0.3f);

        [Tooltip("Reset Inertia On Enter")]
        public BoolReference ResetIntertia = new(false);

        [Space(10), Tooltip("Makes the Animal Stop Moving when is near a Wall")]
        public bool WallStop = false;
        [Hide("WallStop")]
        public LayerMask StopLayer = 1;
        /// <summary>  Store what Wall was the last wall hit </summary>
        private Transform WallHit;

        private WallStopProfiles currentProfile;

        [Tooltip("Profiles to increase or decrease the WallRayLength depending the current Speed (Walk,Trot,Run)." +
            "\nX:Speed Index (Walk = 1, Trot = 2, etc)" +
            "\nY:Additional Value for the Ray when the Character is on that speed.")]
        [Hide("WallStop", false, false)]
        public List<WallStopProfiles> wallStopProfiles = new();




        [Space(10), Tooltip("Makes the Animal avoid ledges, Useful when the Animal without a Fall State, like the Elephant")]
        public bool AntiFall = false;

        [Hide("AntiFall")] public float frontDistance = 0.5f;
        [Hide("AntiFall")] public float frontSpace = 0.2f;
        [Space]
        [Hide("AntiFall")] public float BackDistance = 0.5f;
        [Hide("AntiFall")] public float BackSpace = 0.2f;
        [Space]
        [Hide("AntiFall")] public float FallMultiplier = 1f;
        [Hide("AntiFall")] public Color DebugColor = Color.yellow;

        /// <summary> The Locomotion also works as the Idle Animation </summary>
        public bool HasIdle { get; private set; }


        public override void InitializeState()
        {
            HasIdle = animal.HasState(StateEnum.Idle); //Check if the animal has Idle State if it does not have then Locomotion is IDLE TOO
        }


        /// <summary>This try to enable the Locomotion Logic</summary>
        public override bool TryActivate()
        {
            if (animal.Grounded)
            {
                if (!HasIdle) return true; //Return true if is grounded (Meaning Locomotion is also the IDLE STATE

                if (animal.MovementAxisSmoothed != Vector3.zero || animal.MovementDetected) //If is moving? 
                {
                    return true;
                }
            }
            return false;
        }

        public override void Activate()
        {
            animal.UseSprintState = true; //Update that the state can use sprint

            base.Activate();
            
            //When entering Locomotion the State set the Status the current Speed Modifier. but only when the smooth vertical is off (Weird bug)
            if (!animal.UseSmoothVertical)
            SetEnterStatus((int)animal.CurrentSpeedModifier.Vertical.Value); 
            
            CheckCurrentWallProfile(animal.CurrentSpeedIndex);

            animal.OnMovementDetected.AddListener(OnMovementDetected);

            OnMovementDetected(true); //REcord that the movement has started
        }

        public override void ExitState()
        {
            base.ExitState();
            animal.OnMovementDetected.RemoveListener(OnMovementDetected);
        }

        private void OnMovementDetected(bool movementDetected)
        {
            //Means the input has been released
            if (!movementDetected)
            {
                SetExitStatus((int)animal.CurrentSpeedModifier.Vertical.Value); //Use the Enter Status to check the speed

                //Add an extra movement Detected when the Input is released so the Animal Can calculate a Exit Animations well,
                //but do not do it if the animal is rotatin at direction
                if (animal.Rotate_at_Direction)
                {
                    // SetExitStatus(animal.CurrentSpeedIndex);
                    animal.MovementAxis.z = 1;
                    // animal.movementAxisRaw.z = 1;
                    animal.MovementAxisRaw.z = 1;
                }
                // Debug.Log($"Movement REleased!!! -> {animal.CurrentSpeedModifier.Vertical.Value} - {animal.Sprint}");
            }
        }
        public override void EnterCoreAnimation()
        {
            SetExitStatus(0);

            if (animal.LastState.ID == StateEnum.Climb) animal.ResetCameraInput(); //HACK
            //Keep the Enter Speed on the State Enter Parameter.
            SetEnterStatus((int)animal.CurrentSpeedModifier.Vertical.Value);

            if (ResetIntertia.Value) animal.ResetInertiaSpeed();  //BUG THAT IT WAS MAKING GO FASTER WHEN ENTERING LOCOMOTION

        }

        public override void EnterTagAnimation()
        {
            if (CurrentAnimTag == EnterTagHash) //Using Enter Animation Tag, set the vertical smooth to the velocity 
            {
                animal.VerticalSmooth = animal.CurrentSpeedModifier.Vertical;
            }
        }

        public override void OnStatePreMove(float deltatime)
        {
            Wall_Stop();
            Anti_Fall();
        }


        public override void OnStateMove(float deltatime)
        {
            SetFloatSmooth(0, deltatime * CurrentSpeed.lerpPosition);

            //Hack to use gravity with no Fall State
            if (General.Gravity)
            {
                if (!animal.Grounded)
                {
                    animal.CheckIfGrounded_Height();
                }
                else if (!animal.FrontRay && !animal.MainRay)
                    animal.Grounded = false;
            }

            if (InExitAnimation)
            {
                //Keep Vertical speed here!!!!!!
                if (Anim.IsInTransition(0))
                {
                    animal.MovementAxis.z = 1;
                    animal.MovementAxisRaw.z = 1;
                   // animal.movementAxisRaw.z = 1;
                }
                else
                {
                    SetExitStatus(0);
                    animal.VerticalSmooth = 0; //This makes the Idle State ready to be played??
                }
            }
        }

        public override void SpeedModifierChanged(MSpeed speed, int SpeedIndex)
        {
            SetEnterStatus((int)speed.Vertical.Value); //Use the Enter Status to check the speed
            CheckCurrentWallProfile(SpeedIndex);
        }

        //───────────────────────────────────────── Wall Stop ──────────────────────────────────────────────────────────────────
        private void Wall_Stop()
        {
            if (WallStop && currentProfile != null && MovementRaw.z > 0)
            {
                var Length = (currentProfile.RayLength) * ScaleFactor;
                var MainPivotPoint = animal.Main_Pivot_Point;

                Debug.DrawRay(MainPivotPoint, animal.Forward * Length, Color.yellow);
                MDebug.DrawWireSphere(MainPivotPoint + animal.Forward * Length, Color.yellow, 0.02f);

                if (Physics.Raycast(MainPivotPoint, animal.Forward, out var hit, Length, StopLayer, IgnoreTrigger))
                {
                    animal.MovementAxis.z = 0; //Remove all ForwardMovement
                    if (hit.transform && WallHit != hit.transform)
                    {
                        currentProfile.reaction?.React(animal);
                        WallHit = hit.transform;
                    }
                }
                else
                {
                    Debug.DrawRay(MainPivotPoint, animal.Forward * Length, DebugColor);
                    if (WallHit)
                    {
                        WallHit = null;
                        currentProfile.NoWallDetected?.React(animal);
                    }
                }
            }
        }

        /// <summary>  Wall Stop Profiles  </summary>
        /// <param name="SpeedIndex"></param>
        private void CheckCurrentWallProfile(int SpeedIndex)
        {
            if (WallStop)
            {
                foreach (var prof in wallStopProfiles)
                {
                    if (prof.SpeedIndex <= SpeedIndex)
                        currentProfile = prof;
                }

                //   Debug.Log($"Current Wall Stop Index: {currentProfile.SpeedIndex}");
            }
        }

        public override void ResetStateValues()
        {
            currentProfile = null; //Reset the current ProFile
            WallHit = null;
        }

        //───────────────────────────────────────── ANTI FALL CODE ──────────────────────────────────────────────────────────────────

        private void Anti_Fall()
        {
            if (AntiFall)
            {
                bool BlockForward = false;
                MovementAxisMult = Vector3.one;

                var ForwardMov = MovementRaw.z; // Get the Raw movement that enters on the animal witouth any modifications
                var Dir = animal.TerrainSlope > 0 ? Gravity : -animal.Up;

                float SprintMultiplier = (animal.CurrentSpeedModifier.Vertical).Value;
                SprintMultiplier += animal.Sprint ? 1f : 0f; //Check if the animal is sprinting


                var RayMultiplier = animal.Pivot_Multiplier * FallMultiplier * ScaleFactor; //Get the Multiplier

                var MainPivotPoint = animal.Pivot_Chest.World(animal.transform);

                RaycastHit[] hits = new RaycastHit[1];

                Vector3 Center;
                Vector3 Left;
                Vector3 Right;


                if (ForwardMov > 0)              //Means we are going forward
                {
                    Center = MainPivotPoint + (frontDistance * ScaleFactor * SprintMultiplier * animal.Forward); //Calculate ahead the falling ray
                    Left = Center + (frontSpace * ScaleFactor * animal.Right);
                    Right = Center + (frontSpace * ScaleFactor * -animal.Right);
                }
                else if (ForwardMov < 0)  //Means we are going backwards
                {
                    Center = MainPivotPoint - (BackDistance * ScaleFactor * SprintMultiplier * animal.Forward); //Calculate ahead the falling ray
                    Left = Center + (BackSpace * ScaleFactor * animal.Right);
                    Right = Center + (BackSpace * ScaleFactor * -animal.Right);
                }
                else
                { return; }

                Debug.DrawRay(Center, Dir * RayMultiplier, DebugColor);
                Debug.DrawRay(Left, Dir * RayMultiplier, DebugColor);
                Debug.DrawRay(Right, Dir * RayMultiplier, DebugColor);

                var fallHits = Physics.RaycastNonAlloc(Center, Dir, hits, RayMultiplier, GroundLayer, IgnoreTrigger);

                if (fallHits == 0)
                {
                    BlockForward = true; //Means there's 2 rays that are falling
                }
                else
                    fallHits = Physics.RaycastNonAlloc(Left, Dir, hits, RayMultiplier, GroundLayer, IgnoreTrigger);
                if (fallHits == 0)
                {
                    BlockForward = true; //Means there's 2 rays that are falling
                }
                else
                {
                    fallHits = Physics.RaycastNonAlloc(Right, Dir, hits, RayMultiplier, GroundLayer, IgnoreTrigger);
                    if (fallHits == 0)
                    {
                        BlockForward = true; //Means there's 2 rays that are falling
                    }
                }

                if (BlockForward) MovementAxisMult.z = 0;
                //animal.Remove_HMovement = BlockForward;
            }
            else if (!animal.UseCameraInput && MovementRaw.z < 0) //Meaning is going backwards so AntiFall B
            {
                var MainPivotPoint = animal.Has_Pivot_Hip ? animal.Pivot_Hip.World(transform) : animal.Pivot_Chest.World(transform);
                MainPivotPoint += Forward * -(FallRayBackwards * ScaleFactor);
                RaycastHit[] hits = new RaycastHit[1];

                var RayMultiplier = animal.Pivot_Multiplier; //Get the Multiplier
                Debug.DrawRay(MainPivotPoint, -Up * RayMultiplier, Color.white);

                var fallHits = Physics.RaycastNonAlloc(MainPivotPoint, -Up, hits, RayMultiplier, GroundLayer, IgnoreTrigger);

                if (fallHits == 0)
                {
                    MovementAxisMult.z = 0;
                    //animal.Remove_HMovement = true;
                }
            }
        }


#if UNITY_EDITOR
        public override void StateGizmos(MAnimal animal)
        {
            if (AntiFall) PaintRays(animal);

            if (WallStop && !Application.isPlaying)
            {
                Gizmos.color = Color.yellow;
                var MainPivotPoint = animal.transform.position + new Vector3(0, animal.Height, 0);

                foreach (var item in wallStopProfiles)
                {
                    Gizmos.DrawRay(MainPivotPoint, animal.Forward * item.RayLength);
                    Gizmos.DrawSphere(MainPivotPoint + animal.Forward * item.RayLength, 0.02f);
                }
            }
        }



        void PaintRays(MAnimal animal)
        {
            float scale = animal.ScaleFactor;
            var Dir = animal.TerrainSlope > 0 ? animal.Gravity : -animal.Up;
            var RayMultiplier = animal.Pivot_Multiplier * FallMultiplier; //Get the Multiplier
            var MainPivotPoint = animal.Pivot_Chest.World(animal.transform);

            var FrontCenter = MainPivotPoint + (animal.Forward * frontDistance * scale); //Calculate ahead the falling ray
            var FrontLeft = FrontCenter + (animal.Right * frontSpace * scale);
            var FrontRight = FrontCenter + (-animal.Right * frontSpace * scale);
            var BackCenter = MainPivotPoint - (animal.Forward * BackDistance * scale); //Calculate ahead the falling ray
            var BackLeft = BackCenter + (animal.Right * BackSpace * scale);
            var BackRight = BackCenter + (-animal.Right * BackSpace * scale);

            Debug.DrawRay(FrontCenter, Dir * RayMultiplier, DebugColor);
            Debug.DrawRay(FrontLeft, Dir * RayMultiplier, DebugColor);
            Debug.DrawRay(FrontRight, Dir * RayMultiplier, DebugColor);
            Debug.DrawRay(BackCenter, Dir * RayMultiplier, DebugColor);
            Debug.DrawRay(BackLeft, Dir * RayMultiplier, DebugColor);
            Debug.DrawRay(BackRight, Dir * RayMultiplier, DebugColor);
        }


        public override void SetSpeedSets(MAnimal animal)
        {
            //Do nothing... the Animal Controller already does it on Start
        }


        internal override void Reset()
        {
            base.Reset();

            General = new AnimalModifier()
            {
                RootMotion = true,
                Grounded = true,
                Sprint = true,
                OrientToGround = true,
                CustomRotation = false,
                IgnoreLowerStates = false,
                AdditivePosition = true,
                AdditiveRotation = true,
                Gravity = false,
                modify = (modifier)(-1),
            };

            EnterTag.Value = "StartLocomotion";
        }
#endif
    }


}