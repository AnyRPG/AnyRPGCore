using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class AttackEffectProperties : AmountEffectProperties {

        /*
        public void GetAttackEffectProperties(AttackEffect effect) {


            GetAmountEffectProperties(effect);
        }
        */

        [Header("Attack")]

        [Tooltip("Weapon attacks are considered to have been landed by the equipped weapon, and will play weapon sounds, as well as trigger weapon or status-based on-hit effects")]
        [SerializeField]
        private bool weaponAttack = false;

        [Tooltip("Physical damage will add weapon damage and physical power.  Ability damage will add spell power.")]
        [SerializeField]
        protected DamageType damageType = DamageType.ability;

        [Tooltip("The percentage of the target armor to ignore when dealing damage.  This only applies to physical attacks.")]
        [SerializeField]
        private float ignoreArmorPercent = 0f;

        public float IgnoreArmorPercent { get => ignoreArmorPercent; set => ignoreArmorPercent = value; }
        public bool WeaponAttack { get => weaponAttack; set => weaponAttack = value; }
        public DamageType DamageType { get => damageType; set => damageType = value; }

        /// <summary>
        /// Does the actual work of hitting the target with an ability
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public override void PerformAbilityHit(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(DisplayName + ".AttackAbility.PerformAbilityEffect(" + source.AbilityManager.Name + ", " + target.name + ")");

            // handle regular effects
            base.PerformAbilityHit(source, target, abilityEffectContext);
        }

        public override float GetAmountMultiplyModifier(IAbilityCaster sourceCharacter) {
            return sourceCharacter.AbilityManager.GetOutgoingDamageModifiers();
        }

        public override AbilityEffectContext ProcessAbilityEffectContext(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {

            if (abilityEffectContext.weaponHitHasCast == true) {
                // can't cast weapon hit effects more than once
                return base.ProcessAbilityEffectContext(source, target, abilityEffectContext);
            }
            // since this is called from any attackeffect (even stuff like fireballs) and sets the weapon cast to true,
            // this will also limit cast time multipliers on future effects.
            // This should be ok because the primary hit of a fireball would be regular damage, and any hit after that is likely
            // on hit, burn effect, etc, which should not be multiplied by cast time
            abilityEffectContext.weaponHitHasCast = true;

            if (weaponAttack == true) {
                source.AbilityManager.ProcessWeaponHitEffects(this, target, abilityEffectContext);
            }

            return abilityEffectContext;
        }

        public override bool ProcessAbilityHit(Interactable target, int finalAmount, IAbilityCaster source, CombatMagnitude combatMagnitude, AbilityEffectContext abilityEffectContext, PowerResource powerResource) {
            //public override bool ProcessAbilityHit(Interactable target, int finalAmount, IAbilityCaster source, CombatMagnitude combatMagnitude, AbilityEffectProperties abilityEffect, AbilityEffectContext abilityEffectContext, PowerResource powerResource) {
            bool returnValue = CharacterUnit.GetCharacterUnit(target).BaseCharacter.CharacterCombat.TakeDamage(abilityEffectContext, powerResource, finalAmount, source, combatMagnitude, this);
            if (returnValue == false) {
                return false;
            }

            //return base.ProcessAbilityHit(target, finalAmount, source, combatMagnitude, abilityEffect, abilityEffectContext, powerResource);
            return base.ProcessAbilityHit(target, finalAmount, source, combatMagnitude, abilityEffectContext, powerResource);
        }

        protected override float GetPower(IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext) {
            if (damageType == DamageType.physical) {
                return sourceCharacter.AbilityManager.GetPhysicalPower();
            } else if (damageType == DamageType.ability) {
                //spells can tick so a spell damage multiplier is additionally calculated for the tick share of damage based on tick rate
                return GetAbilityPower(sourceCharacter, abilityEffectContext);
            }

            // this should never be reached, since ability and physical are the only 2 options
            return 0f;
        }

        protected override float GetBaseAmount(IAbilityCaster sourceCharacter) {
            if (damageType == DamageType.physical) {

                // additive damage from weapons
                return sourceCharacter.AbilityManager.GetPhysicalDamage();

            }
            return base.GetBaseAmount(sourceCharacter);
        }

        /*
        public override bool CanUseOn(Interactable target, IAbilityCaster source, AbilityEffectContext abilityEffectContext = null) {
            //Debug.Log("AttackEffect.CanUseOn(" + (target == null ? " null" : target.name) + ", " + source.gameObject.name + ")");
            return base.CanUseOn(target, source, abilityEffectContext);
        }
        */
    }
}
