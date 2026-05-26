using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New StatusEffect", menuName = "AnyRPG/Abilities/Effects/StatusEffect")]
    public class StatusEffect : StatusEffectBase {

        [SerializeField]
        public StatusEffectProperties statusEffectProperties = new StatusEffectProperties();

        public override StatusEffectProperties StatusEffectProperties { get => statusEffectProperties; }

    }


}
