using AnyRPG;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class HealEffectProperties : AmountEffectProperties {

        /*
        public void GetHealEffectProperties(HealEffect effect) {

            GetAmountEffectProperties(effect);
        }
        */

        protected override float GetPower(IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext) {
            return GetAbilityPower(sourceCharacter, abilityEffectContext);
        }

        /// <summary>
        /// Does the actual work of hitting the target with an ability
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public override void PerformAbilityHit(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(DisplayName + ".HealEffect.PerformAbilityHit(" + source.AbilityManager.UnitGameObject.name + ", " + (target == null ? "null" : target.gameObject.name) + ")");

            base.PerformAbilityHit(source, target, abilityEffectInput);
        }

        public override float GetAmountMultiplyModifier(IAbilityCaster sourceCharacter) {
            return 1f;
        }

        public override bool ProcessAbilityHit(Interactable target, int finalAmount, IAbilityCaster source, CombatMagnitude combatMagnitude, AbilityEffectContext abilityEffectContext, PowerResource powerResource) {
            //public override bool ProcessAbilityHit(Interactable target, int finalAmount, IAbilityCaster source, CombatMagnitude combatMagnitude, AbilityEffectProperties abilityEffect, AbilityEffectContext abilityEffectContext, PowerResource powerResource) {
            //Debug.Log(DisplayName + ".HealEffect.ProcessAbilityHit(" + (target == null ? "null" : target.gameObject.name) + ", " + finalAmount + ", " + source.AbilityManager.UnitGameObject.name + ")");

            abilityEffectContext.powerResource = powerResource;
            bool returnValue = CharacterUnit.GetCharacterUnit(target).BaseCharacter.CharacterStats.RecoverResource(abilityEffectContext, powerResource, finalAmount, source, true, combatMagnitude);
            if (returnValue == false) {
                return false;
            }

            return base.ProcessAbilityHit(target, finalAmount, source, combatMagnitude, abilityEffectContext, powerResource);
        }


    }
}