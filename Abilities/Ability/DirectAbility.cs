using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Direct Ability", menuName = "AnyRPG/Abilities/DirectAbility")]
    public class DirectAbility : InstantEffectAbility {

        public override bool PerformAbilityEffects(IAbilityCaster source, GameObject target, AbilityEffectContext abilityEffectContext) {

            if (MyAbilityCastingTime > 1) {
                castTimeMultiplier = MyAbilityCastingTime;
            }
            return base.PerformAbilityEffects(source, target, abilityEffectContext);
        }

    }
}