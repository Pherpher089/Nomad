using MalbersAnimations.Scriptables;
using MalbersAnimations.Controller;
using System.Collections;
using UnityEngine;

namespace MalbersAnimations.Reactions
{
    [System.Serializable]
    [AddTypeMenu("Malbers/Animal/Add Force")]

    public class ForceReaction : MReaction
    {
        public enum DirectionType { Local, World }

        [Tooltip("Relative Direction of the Force to apply")]
        public Vector3Reference Direction =  new Vector3Reference( Vector3.forward);
        [Tooltip("Direction mode to be applied the force on the Animal. World, or Local")]
        public DirectionType Mode = DirectionType.Local;

        [Tooltip("Time to Apply the force")]
        public FloatReference time = new FloatReference(1f);
        [Tooltip("Amount of force to apply")]
        public FloatReference force = new FloatReference( 10f);
        [Tooltip("Aceleration to apply to the force")]
        public FloatReference Aceleration = new FloatReference( 2f);
        [Tooltip("Drag to Decrease the Force after the Force time has pass")]
        public FloatReference ExitDrag = new FloatReference(2f);
        [Tooltip("Set if the Animal is grounded when adding a force")]
        public BoolReference ResetGravity = new BoolReference(false);

        
        protected override bool _TryReact(Component component)
        {
            var animal = component as MAnimal;

            if (animal.enabled && animal.gameObject.activeInHierarchy)
            {
                animal.StartCoroutine(IForceC(animal));
                
                return true;
            }
           
            return false;
        }

        IEnumerator IForceC(MAnimal animal)
        {
            var dir = animal.transform.InverseTransformDirection(Direction);

            if (Mode == DirectionType.World) dir = Direction;

            animal.Force_Add(dir, force, Aceleration, ResetGravity);

            yield return new WaitForSeconds(time);

            animal.Force_Remove(ExitDrag);
        }
    }
}
