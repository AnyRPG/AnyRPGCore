using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    // base class to hold amounts and spellpower calculations for heal and damage effects
    public abstract class AmountEffect : InstantEffect {

        [Header("Amounts")]

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

        public DamageType DamageType { get => damageType; set => damageType = value; }
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

        protected KeyValuePair<float, CombatMagnitude> CalculateAbilityAmount(float abilityBaseAmount, IAbilityCaster sourceCharacter, CharacterUnit target, AbilityEffectOutput abilityEffectInput) {
            float amountAddModifier = 0f;
            float amountMultiplyModifier = 1f;
            //float spellPowerModifier = 0f;
            //float physicalDamageModifier = 0f;
            float critChanceModifier = 0f;
            float critDamageModifier = 1f;

            // stats
            if (damageType == DamageType.physical) {
                amountAddModifier = sourceCharacter.GetPhysicalPower();
            } else if (damageType == DamageType.ability) {
                amountAddModifier = sourceCharacter.GetSpellPower() * abilityEffectInput.spellDamageMultiplier;
            }

            // critical hit modifer
            critChanceModifier = sourceCharacter.GetCritChance();

            int randomInt = Random.Range(0, 100);
            if (randomInt <= critChanceModifier) {
                critDamageModifier = 2f;
            }

            if (damageType == DamageType.physical) {
                
                // additive damage modifiers from gear +damage stat and weapons
                amountAddModifier += sourceCharacter.GetPhysicalDamage();

                // since all damage so far is DPS, we need to multiply it by the attack length.
                // Since global cooldown is 1 second, all abilities less than one second should have their damage increased to one second worth of damage to prevent dps loss
                amountMultiplyModifier *= Mathf.Clamp(sourceCharacter.GetAnimationLengthMultiplier(), 1, Mathf.Infinity);

            } else if (damageType == DamageType.ability) {

                amountMultiplyModifier *= Mathf.Clamp(abilityEffectInput.castTimeMultipler, 1, Mathf.Infinity);
            }
            // multiplicative damage modifiers
            amountMultiplyModifier *= sourceCharacter.GetOutgoingDamageModifiers();

            return new KeyValuePair<float, CombatMagnitude>(((abilityBaseAmount + amountAddModifier) * amountMultiplyModifier * critDamageModifier), (critDamageModifier == 1f ? CombatMagnitude.normal : CombatMagnitude.critical));
        }

        public override void PerformAbilityHit(IAbilityCaster source, GameObject target, AbilityEffectOutput abilityEffectInput) {
            abilityEffectInput.castTimeMultipler = 1f;
            base.PerformAbilityHit(source, target, abilityEffectInput);
        }
    }
}