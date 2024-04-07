using MalbersAnimations.Controller;
using UnityEngine;

namespace MalbersAnimations.Reactions
{
    [System.Serializable]
    [AddTypeMenu("Malbers/Animal/Mode Enable-Disable")]
    public class ModeEnableReaction : MReaction
    { 
        public IDEnable<ModeID>[] modes;

        protected override bool _TryReact(Component component)
        {
            var animal = component as MAnimal;

            foreach (var id in modes)
            {
                var mode = animal.Mode_Get(id.ID);
                if (mode != null)
                {
                    mode.Active = id.enable;
                }
            }
            return true;
        }
    }
}
