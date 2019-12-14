using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    // base class to hold amounts and spellpower calculations for heal and damage effects
    public abstract class AmountEffect : InstantEffect {

        [SerializeField]
        protected bool useHealthAmount;

        [SerializeField]
        protected int healthMinAmount = 0;
        [SerializeField]
        protected int healthBaseAmount = 0;
        [SerializeField]
        protected float healthAmountPerLevel = 0f;
        [SerializeField]
        protected int healthMaxAmount = 0;


        [SerializeField]
        protected bool useManaAmount;

        [SerializeField]
        protected int manaMinAmount = 0;
        [SerializeField]
        protected int manaBaseAmount = 0;
        [SerializeField]
        protected float manaAmountPerLevel = 0f;
        [SerializeField]
        protected int manaMaxAmount = 0;

        [SerializeField]
        protected DamageType damageType;

        public DamageType MyDamageType { get => damageType; set => damageType = value; }
        public int MyHealthMinAmount { get => healthMinAmount; set => healthMinAmount = value; }
        public int MyHealthBaseAmount { get => healthBaseAmount; set => healthBaseAmount = value; }
        public float MyHealthAmountPerLevel { get => healthAmountPerLevel; set => healthAmountPerLevel = value; }
        public int MyHealthMaxAmount { get => healthMaxAmount; set => healthMaxAmount = value; }
        public int MyManaMinAmount { get => manaMinAmount; set => manaMinAmount = value; }
        public int MyManaBaseAmount { get => manaBaseAmount; set => manaBaseAmount = value; }
        public float MyManaAmountPerLevel { get => manaAmountPerLevel; set => manaAmountPerLevel = value; }
        public int MyManaMaxAmount { get => manaMaxAmount; set => manaMaxAmount = value; }
        protected bool MyUseHealthAmount { get => useHealthAmount; set => useHealthAmount = value; }
        protected bool MyUseManaAmount { get => useManaAmount; set => useManaAmount = value; }

        protected KeyValuePair<float, CombatMagnitude> CalculateAbilityAmount(float abilityBaseAmount, BaseCharacter sourceCharacter, CharacterUnit target, AbilityEffectOutput abilityEffectInput) {
            float amountAddModifier = 0f;
            float amountMultiplyModifier = 1f;
            //float spellPowerModifier = 0f;
            //float physicalDamageModifier = 0f;
            float critChanceModifier = 0f;
            float critDamageModifier = 1f;
            if (sourceCharacter.MyCharacterClass != null) {
                if (sourceCharacter.MyCharacterClass != null) {

                    // stats
                    if (damageType == DamageType.physical) {
                        amountAddModifier = LevelEquations.GetPhysicalPowerForCharacter(sourceCharacter);
                    } else if (damageType == DamageType.ability) {
                        amountAddModifier = LevelEquations.GetSpellPowerForCharacter(sourceCharacter) * abilityEffectInput.spellDamageMultiplier;
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
                
                // additive damage modifiers from gear +damage stat
                amountAddModifier += sourceCharacter.MyCharacterStats.MyPhysicalDamage;

                // weapon damage
                if (sourceCharacter.MyCharacterEquipmentManager != null) {
                    amountAddModifier += sourceCharacter.MyCharacterEquipmentManager.GetWeaponDamage();
                }

                amountMultiplyModifier *= Mathf.Clamp(sourceCharacter.MyAnimatedUnit.MyCharacterAnimator.MyLastAnimationLength, 1, Mathf.Infinity);
            } else if (damageType == DamageType.ability) {

                amountMultiplyModifier *= Mathf.Clamp(abilityEffectInput.castTimeMultipler, 1, Mathf.Infinity);
            }
            // multiplicative damage modifiers
            amountMultiplyModifier *= sourceCharacter.MyCharacterStats.GetOutGoingDamageModifiers();

            return new KeyValuePair<float, CombatMagnitude>(((abilityBaseAmount + amountAddModifier) * amountMultiplyModifier * critDamageModifier), (critDamageModifier == 1f ? CombatMagnitude.normal : CombatMagnitude.critical));
        }

        public override void PerformAbilityHit(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
            abilityEffectInput.castTimeMultipler = 1f;
            base.PerformAbilityHit(source, target, abilityEffectInput);
        }
    }
}