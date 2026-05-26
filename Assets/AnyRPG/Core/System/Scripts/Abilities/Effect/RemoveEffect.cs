using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New RemoveEffect", menuName = "AnyRPG/Abilities/Effects/RemoveEffect")]
    public class RemoveEffect : AbilityEffect {

        [SerializeField]
        public RemoveEffectProperties removeEffectProperties = new RemoveEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => removeEffectProperties; }


    }
}