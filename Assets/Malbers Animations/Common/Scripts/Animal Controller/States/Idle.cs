﻿using UnityEngine;

namespace MalbersAnimations.Controller
{
    /// <summary>Idle Should be the Last State on the Queue, when nothing is moving Happening </summary>
    public class Idle : State
    {
        public override string StateName => "Idle";
        public override string StateIDName => "Idle";

        public bool HasLocomotion { get; private set; }

        public override void InitializeState()
        {
            HasLocomotion = animal.HasState(StateEnum.Locomotion); //Check if the animal has Idle State if it does not have then Locomotion is IDLE TOO
        }

        public override void Activate()
        {
            base.Activate();
            CanExit = true; //This allow the Locomotion state to enable any time he want! without waiting the transition to be finished
        }

        public override bool TryActivate()
        {
            //Activate when the animal is not moving and is grounded
            if (HasLocomotion) //Default IDLE!!!! IMPORTANT
            {
                return (
                    animal.MovementAxisSmoothed == Vector3.zero &&
                    //animal.MovementAxis == Vector3.zero && 
                    !animal.MovementDetected &&
                    General.Grounded == animal.Grounded
                    );
            }
            else  //Meaning the Idle works as locomotino too
            {
                return (General.Grounded == animal.Grounded); //This enables that you can be on idle if you are not grounded too
            }
        }


#if UNITY_EDITOR
        internal override void Reset()
        {
            base.Reset();

            ResetLastState = true; //Important por Idle

            General = new AnimalModifier()
            {
                RootMotion = true,
                Grounded = true,
                Sprint = false,
                OrientToGround = true,
                CustomRotation = false,
                FreeMovement = false,
                AdditivePosition = true,
                AdditiveRotation = true,
                Gravity = false,
                modify = (modifier)(-1),
            };
        }

        public override void SetSpeedSets(MAnimal animal)
        {
            //Do nothing... the Animal Controller already does it on Start
        }
#endif
    }
}