using UnityEngine;

namespace MalbersAnimations.Controller
{
    [HelpURL("https://docs.google.com/document/d/1QBLQVWcDSyyWBDrrcS2PthhsToWkOayU0HUtiiSWTF8/edit#heading=h.kraxblx9518t")]
    public class Death : State
    {
        public override string StateName => "Death/Death (Animation)";

        public override string StateIDName => "Death";

        [Header("Death Parameters")]

        public bool DisableAllComponents = true;
        public bool DisableMainCollider = true;
        public bool RemoveAllColliders = false;
        public bool RemoveAllTriggers = true;
        public bool IsKinematic = true;
        //public bool DisableModes = true;
        public int DelayFrames = 2;
        public float RigidbodyDrag = 5;
        public float RigidbodyAngularDrag = 15;

        [Space]
        public bool disableAnimal = true;

        [Hide("disableAnimal")]
        public float disableAnimalTime = 5f;



        public override void EnterCoreAnimation()
        {
            animal.Mode_Interrupt();
            if (IsKinematic)
            {
                animal.RB.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative; //For Kinematic!!
                animal.RB.isKinematic = true;
            }
            animal.StopMoving();
            animal.InputSource?.Enable(false);
            animal.Mode_Stop();
            animal.Delay_Action(DelayFrames, () => DisableAll()); //Wait 2 frames
                                                                  //   if (DisableModes) animal.Mode_Disable_All();


        }

        void DisableAll()
        {
            SetEnterStatus(0);

            if (DisableMainCollider && animal.MainCollider != null) { animal.MainCollider.enabled = false; }

            if (DisableAllComponents)
            {
                var AllComponents = animal.GetComponentsInChildren<MonoBehaviour>();
                foreach (var comp in AllComponents)
                {
                    if (comp == animal) continue;
                    if (comp != null) comp.enabled = false;
                }
            }

            var AllTriggers = animal.GetComponentsInChildren<Collider>();

            foreach (var trig in AllTriggers)
            {
                if (RemoveAllColliders || (RemoveAllTriggers && trig.isTrigger))
                {
                    Destroy(trig);
                }
            }

            animal.SetCustomSpeed(new MSpeed("Death"));

            if (animal.RB)
            {
                animal.RB.drag = RigidbodyDrag;
                animal.RB.angularDrag = RigidbodyAngularDrag;
            }

            if (disableAnimal)
                animal.DisableSelf(disableAnimalTime); //Disable the Animal Component after x time
        }


#if UNITY_EDITOR

        public override void SetSpeedSets(MAnimal animal)
        {
            //Do nothing... Death does not need a Speed Set
        }

        internal override void Reset()
        {
            base.Reset();

            ID = MTools.GetInstance<StateID>("Death");

            noModes.Value = true;

            General = new AnimalModifier()
            {
                modify = (modifier)(-1),
                Persistent = true,
                LockInput = true,
                LockMovement = true,
                AdditiveRotation = true,
            };
        }
#endif
    }
}