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


        protected KeyValuePair<float, CombatMagnitude> CalculateAbilityAmount(int abilityBaseAmount, BaseCharacter sourceCharacter, CharacterUnit target) {
            float amountModifier = 0f;
            //float spellPowerModifier = 0f;
            //float physicalDamageModifier = 0f;
            float critChanceModifier = 0f;
            float critDamageModifier = 1f;
            if (sourceCharacter.MyCharacterClassName != null && sourceCharacter.MyCharacterClassName != string.Empty) {
                CharacterClass characterClass = SystemCharacterClassManager.MyInstance.GetResource(sourceCharacter.MyCharacterClassName);
                if (characterClass != null) {

                    // stats
                    if (damageType == DamageType.physical) {
                        amountModifier = LevelEquations.GetPhysicalPowerForCharacter(sourceCharacter);
                    } else if (damageType == DamageType.ability) {
                        amountModifier = LevelEquations.GetSpellPowerForCharacter(sourceCharacter);
                    }

                    // critical hit modifer
                    critChanceModifier = LevelEquations.GetCritChanceForCharacter(sourceCharacter);

                    int randomInt = Random.Range(0, 100);
                    if (randomInt <= critChanceModifier) {
                        critDamageModifier = 2f;
                    }
                }
            }
            if (damageType == DamageType.physical) {
                // additive damage modifiers
                amountModifier += sourceCharacter.MyCharacterStats.MyPhysicalDamage;

                // weapon damage
                if (sourceCharacter.MyCharacterEquipmentManager != null) {
                    amountModifier += sourceCharacter.MyCharacterEquipmentManager.GetWeaponDamage();
                }

                // multiplicative damage modifiers
                amountModifier *= sourceCharacter.MyCharacterStats.GetOutGoingDamageModifiers();

                amountModifier *= Mathf.Clamp(sourceCharacter.MyAnimatedUnit.MyCharacterAnimator.MyLastAnimationLength, 1, Mathf.Infinity);
            }

            return new KeyValuePair<float, CombatMagnitude>((abilityBaseAmount == 0 ? abilityBaseAmount : (abilityBaseAmount + amountModifier) * critDamageModifier), (critDamageModifier == 1f ? CombatMagnitude.normal : CombatMagnitude.critical));
        }
    }
}