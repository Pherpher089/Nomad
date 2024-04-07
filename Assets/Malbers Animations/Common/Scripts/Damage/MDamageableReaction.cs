using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Reactions
{
    [System.Serializable]
    [AddTypeMenu("Malbers/Damageable/Damageable Set Profile")]

    public class MDamageableReaction : Reaction
    {
        [Tooltip("Changes the Profile of the Main Damageable Component of a Character. Leave it null to Restore to the Defaul Profile")]
        public StringReference Profile = new();

        public override System.Type ReactionType => typeof(MDamageable);

        protected override bool _TryReact(Component reactor)
        {
            var damageable = reactor as MDamageable;

            if (string.IsNullOrEmpty(Profile.Value))
                damageable.Profile_Restore();
            else
                damageable.Profile_Set(Profile);

            return true;
        }
    }
}



 