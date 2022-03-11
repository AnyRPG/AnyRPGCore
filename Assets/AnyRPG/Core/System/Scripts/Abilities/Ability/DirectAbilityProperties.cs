using AnyRPG;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class DirectAbilityProperties : InstantEffectAbilityProperties {

        public override bool PerformAbilityEffects(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {

            //float abilityCastingTime = GetAbilityCastingTime(source);
            float abilityCastingTime = GetBaseAbilityCastingTime(source);
            //if (abilityCastingTime > 1) {
                abilityEffectContext.castTimeMultiplier = abilityCastingTime;
            //}
            return base.PerformAbilityEffects(source, target, abilityEffectContext);
        }

    }
}