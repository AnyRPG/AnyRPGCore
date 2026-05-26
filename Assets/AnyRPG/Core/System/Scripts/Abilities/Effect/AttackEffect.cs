using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New AttackEffect", menuName = "AnyRPG/Abilities/Effects/AttackEffect")]
    public class AttackEffect : AbilityEffect {

        [SerializeField]
        public AttackEffectProperties attackEffectProperties = new AttackEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => attackEffectProperties; }

    }
}
