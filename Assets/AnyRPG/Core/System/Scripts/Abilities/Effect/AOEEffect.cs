using UnityEngine;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New AOEEffect", menuName = "AnyRPG/Abilities/Effects/AOEEffect")]
    public class AOEEffect : AbilityEffect {

        [SerializeField]
        public AOEEffectProperties aoeEffectProperties = new AOEEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => aoeEffectProperties; }

    }

}