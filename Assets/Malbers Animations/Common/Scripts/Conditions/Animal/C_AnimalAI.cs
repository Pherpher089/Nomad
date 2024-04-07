using MalbersAnimations.Controller.AI;
using UnityEngine;

namespace MalbersAnimations.Conditions
{
    [System.Serializable]
    [AddComponentMenu("Malbers/Animal Controller/Conditions/Animal AI")]
    public class C_AnimalAI : MCondition
    {
        public override string DisplayName => "Animal/Animal AI";

       // public override System.Type ConditionType => typeof(MAnimalAIControl);

        [RequiredField] public MAnimalAIControl AI;
        public enum AnimalAICondition { enabled, HasTarget, HasNextTarget, Arrived, Waiting, InOffMesh, CurrentTarget, NextTarget }
        public AnimalAICondition Condition;
        [Hide("Condition", 6, 7)]
        public Transform Target;

        public override bool _Evaluate()
        {
            if (AI)
            {
                switch (Condition)
                {
                    case AnimalAICondition.enabled: return AI.enabled;
                    case AnimalAICondition.HasTarget: return AI.Target != null;
                    case AnimalAICondition.HasNextTarget: return AI.NextTarget != null;
                    case AnimalAICondition.Arrived: return AI.HasArrived;
                    case AnimalAICondition.InOffMesh: return AI.InOffMeshLink;
                    case AnimalAICondition.CurrentTarget: return AI.Target == Target;
                    case AnimalAICondition.Waiting: return AI.IsWaiting;
                    case AnimalAICondition.NextTarget: return AI.NextTarget == Target;
                }
            }
            return false;
        }

        protected override void _SetTarget(Object target) => VerifyTarget(target, ref AI);


        private void Reset() => Name = "New Animal AI Condition";

        [HideInInspector, SerializeField] bool showTarg;
        protected override void OnValidate()
        {
            base.OnValidate();
            showTarg = Condition == AnimalAICondition.CurrentTarget || Condition == AnimalAICondition.NextTarget;
        }
    }
}
