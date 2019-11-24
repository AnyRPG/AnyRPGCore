using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    // base class to hold amounts and spellpower calculations for heal and damage effects
    public abstract class AmountEffect : InstantEffect {

        public int healthMinAmount = 0;
        public int healthBaseAmount = 0;
        public int healthMaxAmount = 0;
        public int manaMinAmount = 0;
        public int manaBaseAmount = 0;
        public int manaMaxAmount = 0;

        [SerializeField]
        protected DamageType damageType;

        public DamageType MyDamageType { get => damageType; set => damageType = value; }


        protected KeyValuePair<float, CombatMagnitude> CalculateAbilityAmount(int abilityBaseAmount, BaseCharacter source, CharacterUnit target) {
            float amountModifer = 0f;
            //float spellPowerModifier = 0f;
            //float physicalDamageModifier = 0f;
            float critChanceModifier = 0f;
            float critDamageModifier = 1f;
            if (source.MyCharacterClassName != null && source.MyCharacterClassName != string.Empty) {
                CharacterClass characterClass = SystemCharacterClassManager.MyInstance.GetResource(source.MyCharacterClassName);
                if (characterClass != null) {
                    foreach (PowerEnhancerNode powerEnhancerNode in characterClass.MyPowerEnhancerStats) {

                        // base damage modifer
                        float totalDamageModifier = 0f;
                        totalDamageModifier += powerEnhancerNode.MyStaminaToPowerRatio * source.MyCharacterStats.MyStamina;
                        totalDamageModifier += powerEnhancerNode.MyStrengthToPowerRatio * source.MyCharacterStats.MyStrength;
                        totalDamageModifier += powerEnhancerNode.MyIntellectToPowerRatio * source.MyCharacterStats.MyIntellect;
                        totalDamageModifier += powerEnhancerNode.MyAgilityToPowerRatio * source.MyCharacterStats.MyAgility;
                        if (powerEnhancerNode.MyPowerToPhysicalDamage == true && damageType == DamageType.physical) {
                            amountModifer += totalDamageModifier;
                        }
                        if (powerEnhancerNode.MyPowerToSpellDamage == true && damageType == DamageType.physical) {
                            amountModifer += totalDamageModifier;
                        }

                        // critical hit modifer
                        // porbably supposed to be 0.2, not 0.5 -- or not - test it
                        critChanceModifier += powerEnhancerNode.MyStaminaToCritPerLevel * (source.MyCharacterStats.MyStamina / source.MyCharacterStats.MyLevel);
                        critChanceModifier += powerEnhancerNode.MyIntellectToCritPerLevel * (source.MyCharacterStats.MyIntellect / source.MyCharacterStats.MyLevel);
                        critChanceModifier += powerEnhancerNode.MyStrengthToCritPerLevel * (source.MyCharacterStats.MyStrength / source.MyCharacterStats.MyLevel);
                        critChanceModifier += powerEnhancerNode.MyAgilityToCritPerLevel * (source.MyCharacterStats.MyAgility / source.MyCharacterStats.MyLevel);
                    }
                    int randomInt = Random.Range(0, 100);
                    if (randomInt <= critChanceModifier) {
                        critDamageModifier = 2f;
                    }
                }
            }

            float amountModifier = source.MyCharacterStats.MySpellPower;
            return new KeyValuePair<float, CombatMagnitude>((abilityBaseAmount == 0 ? abilityBaseAmount : (abilityBaseAmount + amountModifier) * critDamageModifier), (critDamageModifier == 1f ? CombatMagnitude.normal : CombatMagnitude.critical));
        }
    }
}