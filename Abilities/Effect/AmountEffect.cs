using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    // base class to hold amounts and spellpower calculations for heal and damage effects
    public abstract class AmountEffect : InstantEffect {

        [Header("Amounts")]

        [Tooltip("If true, this effect can do critical amounts")]
        [SerializeField]
        private bool allowCriticalStrike = true;

        [Tooltip("The resources to affect, and the amounts of the effects")]
        [SerializeField]
        private List<ResourceAmountNode> resourceAmounts = new List<ResourceAmountNode>();

        [SerializeField]
        protected DamageType damageType;

        public DamageType DamageType { get => damageType; set => damageType = value; }

        protected KeyValuePair<float, CombatMagnitude> CalculateAbilityAmount(float abilityBaseAmount, IAbilityCaster sourceCharacter, CharacterUnit target, AbilityEffectContext abilityEffectInput, ResourceAmountNode resourceAmountNode) {
            //Debug.Log(MyName + ".AmountEffect.CalculateAbilityAmount(" + abilityBaseAmount + ")");

            float amountAddModifier = 0f;
            float amountMultiplyModifier = 1f;
            //float spellPowerModifier = 0f;
            //float physicalDamageModifier = 0f;
            float critChanceModifier = 0f;
            float critDamageModifier = 1f;

            // physical / spell power
            if (resourceAmountNode.AddPower) {
                if (damageType == DamageType.physical) {
                    amountAddModifier = sourceCharacter.GetPhysicalPower();
                } else if (damageType == DamageType.ability) {
                    amountAddModifier = sourceCharacter.GetSpellPower() * abilityEffectInput.spellDamageMultiplier;
                }
            }

            if (allowCriticalStrike == true) {
                // critical hit modifer
                critChanceModifier = sourceCharacter.GetCritChance();

                int randomInt = Random.Range(0, 100);
                if (randomInt <= critChanceModifier) {
                    critDamageModifier = 2f;
                }
            }

            if (damageType == DamageType.physical) {
                
                // additive damage modifiers from gear +damage stat and weapons
                amountAddModifier += sourceCharacter.GetPhysicalDamage();

                // since all damage so far is DPS, we need to multiply it by the attack length.
                // Since global cooldown is 1 second, all abilities less than one second should have their damage increased to one second worth of damage to prevent dps loss
                amountMultiplyModifier *= Mathf.Clamp(sourceCharacter.GetAnimationLengthMultiplier(), 1, Mathf.Infinity);

            } else if (damageType == DamageType.ability) {

                amountMultiplyModifier *= Mathf.Clamp(abilityEffectInput.castTimeMultiplier, 1, Mathf.Infinity);
            }
            // multiplicative damage modifiers
            amountMultiplyModifier *= sourceCharacter.GetOutgoingDamageModifiers();

            return new KeyValuePair<float, CombatMagnitude>(((abilityBaseAmount + amountAddModifier) * amountMultiplyModifier * critDamageModifier), (critDamageModifier == 1f ? CombatMagnitude.normal : CombatMagnitude.critical));
        }

        public override void PerformAbilityHit(IAbilityCaster source, GameObject target, AbilityEffectContext abilityEffectInput) {
            if (abilityEffectInput == null) {
                //Debug.Log("AttackEffect.PerformAbilityHit() abilityEffectInput is null!");
            }
            if (source == null || target == null) {
                // something died or despawned mid cast
                return;
            }

            // check ability context ?  if base ability was animated, then no need to check because we already checked
            if (!((abilityEffectInput.baseAbility as AnimatedAbility) is AnimatedAbility)) {
                if (!source.AbilityHit(target, abilityEffectInput)) {
                    return;
                }

            }
            AbilityEffectContext abilityEffectOutput = new AbilityEffectContext();

            // put this in a for loop
            foreach (ResourceAmountNode resourceAmountNode in resourceAmounts) {
                int finalAmount = 0;
                CombatMagnitude combatMagnitude = CombatMagnitude.normal;
                float effectTotalAmount = resourceAmountNode.BaseAmount + (resourceAmountNode.AmountPerLevel * source.Level);
                KeyValuePair<float, CombatMagnitude> abilityKeyValuePair = CalculateAbilityAmount(effectTotalAmount, source, target.GetComponent<CharacterUnit>(), abilityEffectInput, resourceAmountNode);
                finalAmount = (int)abilityKeyValuePair.Key;
                combatMagnitude = abilityKeyValuePair.Value;
                float inputAmount = 0f;
                foreach (ResourceInputAmountNode _resourceAmountNode in abilityEffectInput.resourceAmounts) {
                    string matchName = resourceAmountNode.ResourceName;
                    if (resourceAmountNode.InputRemap != null && resourceAmountNode.InputRemap != string.Empty) {
                        matchName = resourceAmountNode.InputRemap;
                    }
                    if (_resourceAmountNode.resourceName  == matchName) {
                        inputAmount += _resourceAmountNode.amount;
                    }
                }
                finalAmount += (int)(inputAmount * inputMultiplier);
                //Debug.Log(DisplayName + ".AmountEffect.PerformAbilityHit() input: " + inputAmount + "; multiplier: " + inputMultiplier + "; final: " + finalAmount);

                abilityEffectOutput.AddResourceAmount(resourceAmountNode.ResourceName, finalAmount);

                if (finalAmount > 0) {
                    // this effect may not have any damage and only be here for spawning a prefab or making a sound
                    if (!ProcessAbilityHit(target, finalAmount, source, combatMagnitude, this, abilityEffectInput, resourceAmountNode.PowerResource)) {
                        // if we didn't successfully hit, we can't continue on 
                        return;
                    }
                }
            }

            abilityEffectOutput.groundTargetLocation = abilityEffectInput.groundTargetLocation;

            abilityEffectInput.castTimeMultiplier = 1f;
            base.PerformAbilityHit(source, target, abilityEffectInput);
        }

        public virtual bool ProcessAbilityHit(GameObject target, int finalAmount, IAbilityCaster source, CombatMagnitude combatMagnitude, AbilityEffect abilityEffect, AbilityEffectContext abilityEffectInput, PowerResource powerResource) {
            // nothing here for now, override by heal or attack
            return true;
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            foreach (ResourceAmountNode resourceAmountNode in resourceAmounts) {
                resourceAmountNode.SetupScriptableObjects();
            }
        }
    }

    [System.Serializable]
    public class ResourceAmountNode {

        [Tooltip("The resource to add to or remove from")]
        [SerializeField]
        private string resourceName = string.Empty;

        private PowerResource powerResource = null;

        [Tooltip("If this is not empty, the resource amount will receive input from the following resource, instead of the resource with the same name in the ability effect context.")]
        [SerializeField]
        private string inputRemap = string.Empty;

        [Tooltip("If true, add the appropriate power (spell/physical) to this ability amount")]
        [SerializeField]
        private bool addPower = true;

        [Tooltip("If the amount is lower than this value, it will be rasied to this value.")]
        [SerializeField]
        private int minAmount = 0;

        [Tooltip("Amount not scaled by level.  This will be added to any scaled value.")]
        [SerializeField]
        private int baseAmount = 0;

        [Tooltip("This amount will be multipled by the caster level")]
        [SerializeField]
        private float amountPerLevel = 0f;

        [Tooltip("If the amount is higher than this value, it will be lowered to this value.  0 is unlimited.")]
        [SerializeField]
        private int maxAmount = 0;

        public string ResourceName { get => resourceName; set => resourceName = value; }
        public int MinAmount { get => minAmount; set => minAmount = value; }
        public int BaseAmount { get => baseAmount; set => baseAmount = value; }
        public float AmountPerLevel { get => amountPerLevel; set => amountPerLevel = value; }
        public int MaxAmount { get => maxAmount; set => maxAmount = value; }
        public PowerResource PowerResource { get => powerResource; set => powerResource = value; }
        public bool AddPower { get => addPower; set => addPower = value; }
        public string InputRemap { get => inputRemap; set => inputRemap = value; }

        public void SetupScriptableObjects() {

            if (resourceName != null && resourceName != string.Empty) {
                PowerResource tmpPowerResource = SystemPowerResourceManager.MyInstance.GetResource(resourceName);
                if (tmpPowerResource != null) {
                    powerResource = tmpPowerResource;
                } else {
                    Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find power resource : " + resourceName + " while inititalizing statresourceNode.  CHECK INSPECTOR");
                }
            }

        }
    }

    public class ResourceInputAmountNode {

        public ResourceInputAmountNode(string resourceName, float resourceAmount) {
            this.resourceName = resourceName;
            amount = resourceAmount;
        }

        public string resourceName = string.Empty;

        public float amount = 0f;

    }
}