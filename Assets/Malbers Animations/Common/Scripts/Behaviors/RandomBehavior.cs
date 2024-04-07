using UnityEngine;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations
{
    /// <summary>Is Used to execute random animations in a State Machine</summary>
    public class RandomBehavior : StateMachineBehaviour
    {
        [Tooltip("Max Random Value start from 1 to <Range>")]
        public IntReference Range = new IntReference(15);
        [Tooltip("Priority of the Random Behaviour, Other Layers may be using the Random Behaviour too")]
        public IntReference Priority = new IntReference(0);

        IRandomizer randomizer;

        static int RandomHash = Animator.StringToHash("Random");

        private bool HasRandomParam = false;
        private bool checkfirstTime = false;



        override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
        {
            if (randomizer == null)
                randomizer = animator.GetComponent<IRandomizer>();

            var value = Random.Range(1, Range + 1);

            if (randomizer != null)
            {
                randomizer.SetRandom(value, Priority.Value);
            }
            else
            {
                FindRandomParam(animator); //Check if the Animator has the Random Param
                if (HasRandomParam)
                    animator.SetInteger(RandomHash, value);
            }
        }

        private void FindRandomParam(Animator animator)
        {
            if (!checkfirstTime)
            {
                HasRandomParam = false;
                foreach (var p in animator.parameters)
                {
                    if (p.nameHash == RandomHash)
                    {
                        HasRandomParam = true;
                        break;
                    }
                }
                checkfirstTime = true;
            }
        }

        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
        {
            randomizer?.ResetRandomPriority(Priority);
        }
    }

    public interface IRandomizer
    {
        void SetRandom(int value, int priority);
        void ResetRandomPriority(int priority);
    }
}