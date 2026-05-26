using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Taunt Effect", menuName = "AnyRPG/Abilities/Effects/TauntEffect")]
    public class TauntEffect : AbilityEffect {

        [SerializeField]
        public TauntEffectProperties tauntEffectProperties = new TauntEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => tauntEffectProperties; }


    }
}
