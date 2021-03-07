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
        public override void PerformAbilityHit(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(DisplayName + ".HealEffect.PerformAbilityEffect(" + source.AbilityManager.UnitGameObject.name + ", " + (target == null ? "null" : target.gameObject.name) + ")");

            base.PerformAbilityHit(source, target, abilityEffectInput);
        }

        public override bool ProcessAbilityHit(Interactable target, int finalAmount, IAbilityCaster source, CombatMagnitude combatMagnitude, AbilityEffect abilityEffect, AbilityEffectContext abilityEffectInput, PowerResource powerResource) {
            //Debug.Log(DisplayName + ".HealEffect.ProcessAbilityHit(" + (target == null ? "null" : target.gameObject.name) + ", " + finalAmount + ", " + source.AbilityManager.UnitGameObject.name + ")");

            abilityEffectInput.powerResource = powerResource;
            bool returnValue = CharacterUnit.GetCharacterUnit(target).BaseCharacter.CharacterStats.RecoverResource(abilityEffectInput, powerResource, finalAmount, source, true, combatMagnitude);
            if (returnValue == false) {
                return false;
            }

            return base.ProcessAbilityHit(target, finalAmount, source, combatMagnitude, abilityEffect, abilityEffectInput, powerResource);
        }


    }
}