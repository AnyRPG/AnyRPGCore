using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    // base class to hold amounts and spellpower calculations for heal and damage effects
    public abstract class AmountEffectProperties : InstantEffectProperties {

        [Header("Amounts")]

        [Tooltip("If true, this effect can do critical amounts")]
        [SerializeField]
        private bool allowCriticalStrike = true;

        [Tooltip("The resources to affect, and the amounts of the effects")]
        [SerializeField]
        private List<ResourceAmountNode> resourceAmounts = new List<ResourceAmountNode>();

        [SerializeField]
        protected DamageType damageType = DamageType.ability;

        [Header("Accuracy")]

        [Tooltip("If true, this effect will always hit regardless of current accuracy")]
        [SerializeField]
        protected bool ignoreAccuracy = false;

        public DamageType DamageType { get => damageType; set => damageType = value; }
        public List<ResourceAmountNode> ResourceAmounts { get => resourceAmounts; set => resourceAmounts = value; }
        public bool AllowCriticalStrike { get => allowCriticalStrike; set => allowCriticalStrike = value; }

        /*
        public void GetAmountEffectProperties(AmountEffect effect) {

            allowCriticalStrike = effect.AllowCriticalStrike;
            resourceAmounts = effect.ResourceAmounts;
            damageType = effect.DamageType;
            ignoreAccuracy = effect.IgnoreAccuracy;

            GetInstantEffectProperties(effect);
        }
        */

        protected KeyValuePair<float, CombatMagnitude> CalculateAbilityAmount(float abilityBaseAmount, IAbilityCaster sourceCharacter, CharacterUnit target, AbilityEffectContext abilityEffectContext, ResourceAmountNode resourceAmountNode) {
            //Debug.Log(DisplayName + ".AmountEffect.CalculateAbilityAmount(" + abilityBaseAmount + ")");

            float amountAddModifier = 0f;
            float amountMultiplyModifier = 1f;
            //float spellPowerModifier = 0f;
            //float physicalDamageModifier = 0f;
            float critChanceModifier = 0f;
            float critDamageModifier = 1f;

            // physical / spell power
            if (resourceAmountNode.AddPower) {
                //Debug.Log(DisplayName + ".AmountEffect.CalculateAbilityAmount(" + abilityBaseAmount + "): addPower is true");
                if (damageType == DamageType.physical) {
                    amountAddModifier = sourceCharacter.AbilityManager.GetPhysicalPower();
                } else if (damageType == DamageType.ability) {
                    //spells can tick so a spell damage multiplier is additionally calculated for the tick share of damage based on tick rate
                    amountAddModifier = sourceCharacter.AbilityManager.GetSpellPower() * abilityEffectContext.spellDamageMultiplier;
                }
            } else {
                //Debug.Log(DisplayName + ".AmountEffect.CalculateAbilityAmount(" + abilityBaseAmount + "): addPower is false");
            }

            if (allowCriticalStrike == true) {
                // critical hit modifer
                critChanceModifier = sourceCharacter.AbilityManager.GetCritChance();

                int randomInt = Random.Range(0, 100);
                if (randomInt <= critChanceModifier) {
                    critDamageModifier = 2f;
                }
            }

            // reflected damage can only have critical strike and base power added. It should not be getting a boost from weapon damage, status effects, cast time multipliers etc
            if (abilityEffectContext.reflectDamage == false) {
                if (damageType == DamageType.physical) {

                    // additive damage from weapons
                    amountAddModifier += sourceCharacter.AbilityManager.GetPhysicalDamage();

                    // since all damage so far is DPS, we need to multiply it by the attack length.
                    // Since global cooldown is 1 second, all abilities less than one second should have their damage increased to one second worth of damage to prevent dps loss
                    if (abilityEffectContext.weaponHitHasCast == false) {
                        // the first attack effect cast from an ability is considered the primary hit
                        // everything after is an onHit effect and should not be multiplied by animation time
                        amountMultiplyModifier *= Mathf.Clamp(sourceCharacter.AbilityManager.GetAnimationLengthMultiplier(), 1, Mathf.Infinity);
                    }

                } else if (damageType == DamageType.ability) {
                    if (abilityEffectContext.weaponHitHasCast == false) {
                        // the first attack effect cast from an ability is considered the primary hit
                        // everything after is an onHit effect and should not be multiplied by animation time
                        amountMultiplyModifier *= Mathf.Clamp(abilityEffectContext.castTimeMultiplier, 1, Mathf.Infinity);
                        //Debug.Log(DisplayName + ".AmountEffect.CalculateAbilityAmount() weapon hit has not cast yet target: " + target.BaseCharacter.UnitController.gameObject.name + "; cast: " + abilityEffectContext.castTimeMultiplier);
                    } else {
                        //Debug.Log(DisplayName + ".AmountEffect.CalculateAbilityAmount(): weaponHit has cast.  not multiplying ability damage");
                    }
                }
                // multiplicative damage modifiers
                amountMultiplyModifier *= sourceCharacter.AbilityManager.GetOutgoingDamageModifiers();
            }
            
            //Debug.Log(DisplayName + ".AmountEffect.CalculateAbilityAmount(): amountMultiplyModifier: " + amountMultiplyModifier + "; critDamageModifier: " + critDamageModifier + "; amountAddModifier: " + amountAddModifier + "; abilityBaseAmount: " + abilityBaseAmount + "; source: " + sourceCharacter.AbilityManager.UnitGameObject.name + "; target: " + target.BaseCharacter.UnitController.gameObject.name + "; castTimeMultiplier: " + abilityEffectContext.castTimeMultiplier);

            return new KeyValuePair<float, CombatMagnitude>(((abilityBaseAmount + amountAddModifier) * amountMultiplyModifier * critDamageModifier), (critDamageModifier == 1f ? CombatMagnitude.normal : CombatMagnitude.critical));
        }

        public override void PerformAbilityHit(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(DisplayName + ".AmountEffect.PerformAbilityHit(" + (source == null ? "null" : source.AbilityManager.UnitGameObject.name) + ", " + (target == null ? "null" : target.gameObject.name) + ")");
            if (source == null || target == null) {
                // something died or despawned mid cast
                return;
            }

            // check ability context ?  if base ability was animated, then no need to check because we already checked
            if (ignoreAccuracy == false && !((abilityEffectContext.baseAbility as AnimatedAbilityProperties) is AnimatedAbilityProperties)) {
                if (!source.AbilityManager.AbilityHit(target, abilityEffectContext)) {
                    return;
                }
            }

            // this potentially triggered off a status effect.  We don't want to add amounts to the base context so we need to make a new one now
            AbilityEffectContext abilityEffectOutput = abilityEffectContext.GetCopy();

            foreach (ResourceAmountNode resourceAmountNode in resourceAmounts) {
                CombatMagnitude combatMagnitude = CombatMagnitude.normal;
                float effectTotalAmount = 0f;

                // add inputs to total amount, accounting for remaps
                foreach (ResourceInputAmountNode _resourceAmountNode in abilityEffectOutput.resourceAmounts) {
                    string matchName = resourceAmountNode.ResourceName;
                    if (resourceAmountNode.InputRemap != null && resourceAmountNode.InputRemap != string.Empty) {
                        matchName = resourceAmountNode.InputRemap;
                    }
                    if (_resourceAmountNode.resourceName == matchName) {
                        effectTotalAmount += _resourceAmountNode.amount;
                    }
                }
                // multiply inputs by the input multiplier for this effect
                effectTotalAmount *= inputMultiplier;

                // add basic effect amounts to the total inputs
                effectTotalAmount += resourceAmountNode.BaseAmount + (resourceAmountNode.AmountPerLevel * source.AbilityManager.Level);

                // calculate total ability amount based on character power, stats, and modifiers
                KeyValuePair<float, CombatMagnitude> abilityKeyValuePair = CalculateAbilityAmount(effectTotalAmount, source, CharacterUnit.GetCharacterUnit(target), abilityEffectOutput, resourceAmountNode);
                effectTotalAmount = (int)abilityKeyValuePair.Key;
                combatMagnitude = abilityKeyValuePair.Value;
                
                //Debug.Log(DisplayName + ".AmountEffect.PerformAbilityHit(" + (source == null ? "null" : source.AbilityManager.UnitGameObject.name) + ", " + (target == null ? "null" : target.gameObject.name) + ") finalAmount : " + finalAmount + "; input: " + inputAmount + "; multiplier: " + inputMultiplier);

                //abilityEffectOutput.AddResourceAmount(resourceAmountNode.ResourceName, finalAmount);
                abilityEffectOutput.SetResourceAmount(resourceAmountNode.ResourceName, effectTotalAmount);

                if (effectTotalAmount > 0) {
                    // this effect may not have any damage and only be here for spawning a prefab or making a sound
                    // ^ is that comment valid?  spawning a prefab can be done with instantEffect and doesn't require amount effects
                    if (!ProcessAbilityHit(target, (int)effectTotalAmount, source, combatMagnitude, this, abilityEffectOutput, resourceAmountNode.PowerResource)) {
                        // if we didn't successfully hit, we can't continue on 
                        return;
                    }
                }
            }

            abilityEffectOutput.castTimeMultiplier = 1f;

            //abilityEffectOutput.groundTargetLocation = abilityEffectContext.groundTargetLocation;
            abilityEffectOutput = ProcessAbilityEffectContext(source, target, abilityEffectOutput);

            //base.PerformAbilityHit(source, target, abilityEffectContext);
            base.PerformAbilityHit(source, target, abilityEffectOutput);
        }

        /// <summary>
        /// give overrides a chance to operate on this before passing it on
        /// </summary>
        /// <param name="abilityEffectContext"></param>
        /// <returns></returns>
        public virtual AbilityEffectContext ProcessAbilityEffectContext(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            return abilityEffectContext;
        }

        public virtual bool ProcessAbilityHit(Interactable target, int finalAmount, IAbilityCaster source, CombatMagnitude combatMagnitude, AbilityEffectProperties abilityEffect, AbilityEffectContext abilityEffectInput, PowerResource powerResource) {
            // nothing here for now, override by heal or attack
            return true;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager, IDescribable describable) {
            base.SetupScriptableObjects(systemGameManager, describable);

            foreach (ResourceAmountNode resourceAmountNode in resourceAmounts) {
                resourceAmountNode.SetupScriptableObjects(systemDataFactory);
            }
        }
    }

   


}