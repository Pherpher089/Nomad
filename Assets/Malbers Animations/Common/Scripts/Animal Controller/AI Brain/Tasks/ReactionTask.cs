using MalbersAnimations.Reactions;
using UnityEngine;

namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Tasks/Reaction Task")]
    public class ReactionTask : MTask
    {
        public override string DisplayName => "General/Reaction";

        [Space, Tooltip("Apply the Task to the Animal(Self) or the Target(Target)")]
        public Affected affect = Affected.Self;

        [SerializeReference,SubclassSelector]
        [Tooltip("Reaction when the AI Task begin")]
        public Reaction reaction;

        [SerializeReference, SubclassSelector]
        [Tooltip("Reaction when the AI State ends")]
        public Reaction reactionOnExit;

        public override void StartTask(MAnimalBrain brain, int index)
        {
            React(brain, index, reaction);
        }

        private void React(MAnimalBrain brain, int index, Reaction reaction)
        {
            if (affect == Affected.Self)
            {
                reaction?.React(brain.Animal);
            }
            else
            {
                if (brain.Target)
                    reaction?.React(brain.Target);
            }
            brain.TaskDone(index);
        }

        public override void ExitAIState(MAnimalBrain brain, int index)
        {
            React(brain, index, reactionOnExit);
        }

        private void Reset()
        => Description = "Add a Reaction to the Target or the Animal";

    }
}
