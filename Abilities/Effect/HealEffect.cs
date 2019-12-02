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
        public override void PerformAbilityHit(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log(abilityEffectName + ".HealEffect.PerformAbilityEffect(" + source.name + ", " + (target == null ? "null" : target.name) + ") effect: " + abilityEffectName);
            int healthFinalAmount = 0;
            CombatMagnitude combatMagnitude = CombatMagnitude.normal;
            if (useHealthAmount == true) {
                float healthTotalAmount = healthBaseAmount + (healthAmountPerLevel * source.MyCharacterStats.MyLevel);
                KeyValuePair<float, CombatMagnitude> abilityKeyValuePair = CalculateAbilityAmount(healthTotalAmount, source, target.GetComponent<CharacterUnit>(), abilityEffectInput);
                healthFinalAmount = (int)abilityKeyValuePair.Key;
                combatMagnitude = abilityKeyValuePair.Value;
                //healthFinalAmount = (int)CalculateAbilityAmount(healthTotalAmount, source, target.GetComponent<CharacterUnit>(), abilityEffectInput).Key;
            }
            healthFinalAmount += (int)(abilityEffectInput.healthAmount * inputMultiplier);

            AbilityEffectOutput abilityEffectOutput = new AbilityEffectOutput();
            abilityEffectOutput.healthAmount = healthFinalAmount;
            if (healthFinalAmount > 0) {
                target.GetComponent<CharacterUnit>().MyCharacter.MyCharacterStats.RecoverHealth(healthFinalAmount, source, true, combatMagnitude);
            }
            int manaFinalAmount = 0;
            combatMagnitude = CombatMagnitude.normal;
            if (useManaAmount == true) {
                float manaTotalAmount = manaBaseAmount + (manaAmountPerLevel * source.MyCharacterStats.MyLevel);
                KeyValuePair<float, CombatMagnitude> abilityKeyValuePair = CalculateAbilityAmount(manaBaseAmount, source, target.GetComponent<CharacterUnit>(), abilityEffectInput);
                manaFinalAmount = (int)abilityKeyValuePair.Key;
                combatMagnitude = abilityKeyValuePair.Value;
                //manaFinalAmount = (int)CalculateAbilityAmount(manaBaseAmount, source, target.GetComponent<CharacterUnit>(), abilityEffectInput).Key;
            }
            manaFinalAmount += (int)(abilityEffectInput.manaAmount * inputMultiplier);
            abilityEffectOutput.manaAmount = manaFinalAmount;
            if (manaFinalAmount > 0) {
                target.GetComponent<CharacterUnit>().MyCharacter.MyCharacterStats.RecoverMana(manaFinalAmount, source, true, combatMagnitude);
            }
            abilityEffectOutput.prefabLocation = abilityEffectInput.prefabLocation;
            base.PerformAbilityHit(source, target, abilityEffectOutput);
        }

        public override CharacterUnit ReturnTarget(CharacterUnit source, CharacterUnit target) {
            //Debug.Log("HealEffect.ReturnTarget(" + (source == null ? "null" : source.MyName) + ", " + (target == null ? "null" : target.MyName) + ")");
            if (target == null) {
                //Debug.Log("Heal spell cast, but there was no target");
                return source;
            }
            if (Faction.RelationWith(target.MyCharacter, source.MyCharacter) <= -1) {
                //if (Faction.RelationWith(source.MyCharacter, target.MyCharacter) < 0) {
                //Debug.Log("The target is an enemy.  Casting on self instead.");
                return source;
            }
            return target;
        }


    }
}