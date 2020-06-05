using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New AttackEffect", menuName = "AnyRPG/Abilities/Effects/AttackEffect")]
    public class AttackEffect : AmountEffect {

        /// <summary>
        /// Does the actual work of hitting the target with an ability
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public override void PerformAbilityHit(IAbilityCaster source, GameObject target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(MyAbilityEffectName + ".AttackAbility.PerformAbilityEffect(" + source.name + ", " + target.name + ")");

            // handle regular effects
            base.PerformAbilityHit(source, target, abilityEffectInput);

            source.ProcessWeaponHitEffects(this, target, abilityEffectInput);
        }

        public override void ProcessAbilityHit(GameObject target, int finalAmount, IAbilityCaster source, CombatMagnitude combatMagnitude, AbilityEffect abilityEffect, AbilityEffectContext abilityEffectInput, PowerResource powerResource) {
            target.GetComponent<CharacterUnit>().MyCharacter.CharacterCombat.TakeDamage(abilityEffectInput, powerResource, finalAmount, source, combatMagnitude, this, abilityEffectInput.refectDamage);

            base.ProcessAbilityHit(target, finalAmount, source, combatMagnitude, abilityEffect, abilityEffectInput, powerResource);
        }

        public override bool CanUseOn(GameObject target, IAbilityCaster source, AbilityEffectContext abilityEffectContext = null) {
            //Debug.Log("AttackEffect.CanUseOn(" + (target == null ? " null" : target.name) + ", " + source.gameObject.name + ")");
            return base.CanUseOn(target, source, abilityEffectContext);
        }
    }
}
