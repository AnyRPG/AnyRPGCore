using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New InstantEffect", menuName = "AnyRPG/Abilities/Effects/InstantEffect")]
    public class InstantEffect : AbilityEffect {

        [SerializeField]
        public InstantEffectProperties instantEffectProperties = new InstantEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => instantEffectProperties; }

    }
}