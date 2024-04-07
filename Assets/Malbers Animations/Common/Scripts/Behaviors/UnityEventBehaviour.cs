using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations.Events;

namespace MalbersAnimations.Utilities
{
    public class UnityEventBehaviour : StateMachineBehaviour
    {
        [Range(0, 1)]
        [SerializeField] private float _time = 0;

        [SerializeField] private AnimatorEvent Invoke;

        private bool MessageSent = false;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            MessageSent = false;

            if (_time == 0)
            {
                Invoke.Invoke(animator);
                MessageSent = true;
            }
        }


        public override void OnStateUpdate(Animator animator, AnimatorStateInfo state, int layer)
        {
            var time = state.normalizedTime % 1;

            if (!MessageSent && time >= _time)
            {
                Invoke.Invoke(animator);
                MessageSent = true;
            }
        }
    }
}