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
        public override void PerformAbilityHit(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log(MyAbilityEffectName + ".AttackAbility.PerformAbilityEffect(" + source.name + ", " + target.name + ")");
            if (abilityEffectInput == null) {
                //Debug.Log("AttackEffect.PerformAbilityEffect() abilityEffectInput is null!");
            }
            if (source == null || target == null) {
                // something died or despawned mid cast
                return;
            }
            if (source.MyCharacterCombat.DidAttackMiss() == true) {
                //Debug.Log(MyName + ".AttackEffect.PerformAbilityHit(" + source.name + ", " + target.name + "): attack missed");
                source.MyCharacterCombat.ReceiveCombatMiss(target);
                return;
            }
            int healthFinalAmount = 0;
            CombatMagnitude combatMagnitude = CombatMagnitude.normal;
            if (useHealthAmount == true) {
                float healthTotalAmount = healthBaseAmount + (healthAmountPerLevel * source.MyCharacterStats.MyLevel);
                KeyValuePair<float, CombatMagnitude> abilityKeyValuePair = CalculateAbilityAmount(healthTotalAmount, source, target.GetComponent<CharacterUnit>(), abilityEffectInput);
                healthFinalAmount = (int)abilityKeyValuePair.Key;
                combatMagnitude = abilityKeyValuePair.Value;
            }
            healthFinalAmount += (int)(abilityEffectInput.healthAmount * inputMultiplier);

            AbilityEffectOutput abilityEffectOutput = new AbilityEffectOutput();
            abilityEffectOutput.healthAmount = healthFinalAmount;
            if (healthFinalAmount > 0) {
                // this effect may not have any damage and only be here for spawning a prefab or making a sound
                target.GetComponent<CharacterUnit>().MyCharacter.MyCharacterCombat.TakeDamage(healthFinalAmount, source.MyCharacterUnit.transform.position, source, combatMagnitude, this, abilityEffectInput.refectDamage);
            }
            abilityEffectOutput.prefabLocation = abilityEffectInput.prefabLocation;

            // handle weapon on hit effects
            if (source.MyCharacterCombat != null && source.MyCharacterCombat.MyOnHitEffect != null && damageType == DamageType.physical && source.MyCharacterCombat.MyOnHitEffect.MyName != MyName) {
                List<AbilityEffect> onHitEffectList = new List<AbilityEffect>();
                onHitEffectList.Add(source.MyCharacterCombat.MyOnHitEffect);
                PerformAbilityEffects(source, target, abilityEffectOutput, onHitEffectList);
            } else {
                //Debug.Log(MyName + ".AttackEffect.PerformAbilityHit(" + (source == null ? "null" : source.name) + ", " + (target == null ? "null" : target.name) + "): no on hit effect set");
            }

            // handle regular effects
            base.PerformAbilityHit(source, target, abilityEffectOutput);
        }

        public override bool CanUseOn(GameObject target, BaseCharacter source) {
            //Debug.Log("AttackEffect.CanUseOn(" + (target == null ? " null" : target.name) + ", " + source.gameObject.name + ")");
            return base.CanUseOn(target, source);
        }
    }
}
