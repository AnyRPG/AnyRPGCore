using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Direct Ability", menuName = "AnyRPG/Abilities/DirectAbility")]
    public class DirectAbility : InstantEffectAbility {

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