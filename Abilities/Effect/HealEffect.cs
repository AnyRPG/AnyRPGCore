using AnyRPG;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New HealEffect", menuName = "AnyRPG/Abilities/Effects/HealEffect")]
    public class HealEffect : AmountEffect {

        /// <summary>
        /// Does the actual work of hitting the target with an ability
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public override void PerformAbilityHit(IAbilityCaster source, GameObject target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(abilityEffectName + ".HealEffect.PerformAbilityEffect(" + source.name + ", " + (target == null ? "null" : target.name) + ") effect: " + abilityEffectName);

            base.PerformAbilityHit(source, target, abilityEffectInput);
        }

        public override void ProcessAbilityHit(GameObject target, int finalAmount, IAbilityCaster source, CombatMagnitude combatMagnitude, AbilityEffect abilityEffect, AbilityEffectContext abilityEffectInput, PowerResource powerResource) {

            abilityEffectInput.powerResource = powerResource;
            target.GetComponent<CharacterUnit>().MyCharacter.CharacterStats.RecoverResource(abilityEffectInput, powerResource, finalAmount, source, true, combatMagnitude);

            base.ProcessAbilityHit(target, finalAmount, source, combatMagnitude, abilityEffect, abilityEffectInput, powerResource);
        }


    }
}