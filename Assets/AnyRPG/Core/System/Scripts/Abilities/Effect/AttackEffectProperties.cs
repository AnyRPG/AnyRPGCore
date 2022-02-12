using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class AttackEffectProperties : AmountEffectProperties {

        /// <summary>
        /// Does the actual work of hitting the target with an ability
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public override void PerformAbilityHit(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(DisplayName + ".AttackAbility.PerformAbilityEffect(" + source.AbilityManager.Name + ", " + target.name + ")");

            // handle regular effects
            base.PerformAbilityHit(source, target, abilityEffectContext);
        }

        public override AbilityEffectContext ProcessAbilityEffectContext(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {

            if (abilityEffectContext.weaponHitHasCast == true) {
                // can't cast weapon hit effects more than once
                return base.ProcessAbilityEffectContext(source, target, abilityEffectContext);
            }
            // since this is called from any attackeffect (even stuff like fireballs) and sets the weapon cast to true,
            // this will also limit cast time multipliers on future effects.
            // This should be ok because the primary hit of a fireball would be regular damage, and any hit after that is likely
            // on hit, burn effect, etc, which should not be multiplied by cast time
            abilityEffectContext.weaponHitHasCast = true;

            source.AbilityManager.ProcessWeaponHitEffects(this, target, abilityEffectContext);

            return abilityEffectContext;
        }


        public override bool ProcessAbilityHit(Interactable target, int finalAmount, IAbilityCaster source, CombatMagnitude combatMagnitude, AbilityEffectProperties abilityEffect, AbilityEffectContext abilityEffectContext, PowerResource powerResource) {
            bool returnValue = CharacterUnit.GetCharacterUnit(target).BaseCharacter.CharacterCombat.TakeDamage(abilityEffectContext, powerResource, finalAmount, source, combatMagnitude, this);
            if (returnValue == false) {
                return false;
            }

            return base.ProcessAbilityHit(target, finalAmount, source, combatMagnitude, abilityEffect, abilityEffectContext, powerResource);
        }

        /*
        public override bool CanUseOn(Interactable target, IAbilityCaster source, AbilityEffectContext abilityEffectContext = null) {
            //Debug.Log("AttackEffect.CanUseOn(" + (target == null ? " null" : target.name) + ", " + source.gameObject.name + ")");
            return base.CanUseOn(target, source, abilityEffectContext);
        }
        */
    }
}
