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
        public override void PerformAbilityHit(IAbilityCaster source, GameObject target, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log(MyAbilityEffectName + ".AttackAbility.PerformAbilityEffect(" + source.name + ", " + target.name + ")");
            if (abilityEffectInput == null) {
                //Debug.Log("AttackEffect.PerformAbilityEffect() abilityEffectInput is null!");
            }
            if (source == null || target == null) {
                // something died or despawned mid cast
                return;
            }
            if (!source.AbilityHit(target)) {
                return;
            }
            int healthFinalAmount = 0;
            CombatMagnitude combatMagnitude = CombatMagnitude.normal;
            if (useHealthAmount == true) {
                //Debug.Log(MyName + ".AttackEffect.PerformAbilityHit(): source.level: " + source.Level + "; healthperlevel: " + healthAmountPerLevel);
                float healthTotalAmount = healthBaseAmount + (healthAmountPerLevel * source.Level);
                KeyValuePair<float, CombatMagnitude> abilityKeyValuePair = CalculateAbilityAmount(healthTotalAmount, source, target.GetComponent<CharacterUnit>(), abilityEffectInput);
                healthFinalAmount = (int)abilityKeyValuePair.Key;
                combatMagnitude = abilityKeyValuePair.Value;
            }
            healthFinalAmount += (int)(abilityEffectInput.healthAmount * inputMultiplier);

            AbilityEffectOutput abilityEffectOutput = new AbilityEffectOutput();
            abilityEffectOutput.healthAmount = healthFinalAmount;
            if (healthFinalAmount > 0) {
                // this effect may not have any damage and only be here for spawning a prefab or making a sound

                target.GetComponent<CharacterUnit>().MyCharacter.CharacterCombat.TakeDamage(healthFinalAmount, source, combatMagnitude, this, abilityEffectInput.refectDamage);
            }
            abilityEffectOutput.prefabLocation = abilityEffectInput.prefabLocation;

            source.ProcessWeaponHitEffects(this, target, abilityEffectInput);

            // handle regular effects
            base.PerformAbilityHit(source, target, abilityEffectOutput);
        }

        public override bool CanUseOn(GameObject target, IAbilityCaster source) {
            //Debug.Log("AttackEffect.CanUseOn(" + (target == null ? " null" : target.name) + ", " + source.gameObject.name + ")");
            return base.CanUseOn(target, source);
        }
    }
}
