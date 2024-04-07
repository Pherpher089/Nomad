using MalbersAnimations.Controller;
using UnityEngine;

namespace MalbersAnimations.Reactions
{
    [System.Serializable]
    [AddTypeMenu("Malbers/Animal/Movement")]
    public class MovementReaction : MReaction
    {
        public Move_Reaction type = Move_Reaction.Sleep;
        public bool Value;

        protected override bool _TryReact(Component component)
        {
            var animal = component as MAnimal;
            switch (type)
            {
                case Move_Reaction.UseCameraInput:
                    animal.UseCameraInput = Value;
                    break;
                case Move_Reaction.Sleep:
                    animal.Sleep = Value;
                    break;
                case Move_Reaction.LockInput:
                    animal.LockInput = Value;
                    break;
                case Move_Reaction.LockMovement:
                    animal.LockMovement = Value;
                    break;
                case Move_Reaction.AlwaysForward:
                    animal.AlwaysForward = Value;
                    break;
                case Move_Reaction.UseCameraUp:
                    animal.UseCameraUp = Value;
                    break;
                case Move_Reaction.LockForward:
                    animal.LockForwardMovement = Value;
                    break;
                case Move_Reaction.LockHorizontal:
                    animal.LockHorizontalMovement = Value;
                    break;
                case Move_Reaction.LockUpDown:
                    animal.LockUpDownMovement = Value;
                    break;
                default:
                    break;
            }
            return true;
        }

        public enum Move_Reaction
        {
            UseCameraInput,
            Sleep,
            LockInput,
            LockMovement,
            AlwaysForward,
            UseCameraUp,
            LockForward,
            LockHorizontal,
            LockUpDown,
        }
    }
}
