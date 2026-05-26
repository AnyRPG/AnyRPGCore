namespace AnyRPG {

    [System.Serializable]
    public class HealEffectProperties : AmountEffectProperties {

        /*
        public void GetHealEffectProperties(HealEffect effect) {

            GetAmountEffectProperties(effect);
        }
        */

        protected override float GetPower(IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext) {
            return GetAbilityPower(sourceCharacter, abilityEffectContext);
        }

        /// <summary>
        /// Does the actual work of hitting the target with an ability
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public override void PerformAbilityHit(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log($"{ResourceName}.HealEffectProperties.PerformAbilityHit({source.AbilityManager.UnitGameObject.name}, {(target == null ? "null" : target.gameObject.name)}");

            base.PerformAbilityHit(source, target, abilityEffectInput);
        }

        public override float GetAmountMultiplyModifier(IAbilityCaster sourceCharacter) {
            return 1f;
        }

        public override bool ProcessAbilityHit(Interactable target, int finalAmount, IAbilityCaster source, CombatMagnitude combatMagnitude, AbilityEffectContext abilityEffectContext, PowerResource powerResource) {
            //Debug.Log($"{ResourceName}.HealEffectProperties.ProcessAbilityHit({(target == null ? "null" : target.gameObject.name)}, {finalAmount}, {source.AbilityManager.UnitGameObject.name}, {powerResource.ResourceName})");

            abilityEffectContext.PowerResource = powerResource;
            bool returnValue = CharacterUnit.GetCharacterUnit(target).UnitController.CharacterStats.RecoverResource(abilityEffectContext, powerResource, finalAmount, source, combatMagnitude);
            if (returnValue == false) {
                return false;
            }

            return base.ProcessAbilityHit(target, finalAmount, source, combatMagnitude, abilityEffectContext, powerResource);
        }


    }
}