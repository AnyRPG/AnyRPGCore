using AnyRPG;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class ResurrectEffectProperties : InstantEffectProperties {

        /*
        public void GetResurrectEffectProperties(ResurrectEffect effect) {

            GetInstantEffectProperties(effect);
        }
        */

        /// <summary>
        /// Does the actual work of hitting the target with an ability
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public override void PerformAbilityHit(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(resourceName + ".ResurrectEffect.PerformAbilityEffect(" + source.name + ", " + (target == null ? "null" : target.name) + ") effect: " + resourceName);
            // is there a reason why there is no copy here ?
            /*
            AbilityEffectContext abilityEffectOutput = new AbilityEffectContext();
            abilityEffectOutput.groundTargetLocation = abilityEffectInput.groundTargetLocation;
            */
            ResurrectTarget(target);
            base.PerformAbilityHit(source, target, abilityEffectContext);
        }

        private void ResurrectTarget(Interactable target) {
            if (target == null) {
                // our target despawned in the middle of the cast
                return;
            }
            CharacterUnit characterUnit = CharacterUnit.GetCharacterUnit(target);
            if (characterUnit == null) {
                //Debug.Log("CharacterUnit is null? target despawn during cast?");
                return;
            }
            characterUnit.UnitController.CharacterStats.Revive();
        }

        public override bool CanUseOn(Interactable target, IAbilityCaster source, AbilityEffectContext abilityEffectContext = null, bool playerInitiated = false, bool performRangeCheck = true) {
            if (target == null) {
                return false;
            }
            CharacterUnit characterUnit = CharacterUnit.GetCharacterUnit(target);
            if (characterUnit == null) {
                return false;
            }
            if (characterUnit.UnitController.CharacterStats.IsAlive == false && characterUnit.UnitController.CharacterStats.IsReviving == false) {
                return true;
            }
            if (characterUnit.UnitController.CharacterStats.IsAlive == true) {
                if (playerInitiated) {
                    source.AbilityManager.ReceiveCombatMessage("Cannot cast " + DisplayName + ". Target is already alive");
                }
            }
            if (characterUnit.UnitController.CharacterStats.IsReviving == true) {
                if (playerInitiated) {
                    source.AbilityManager.ReceiveCombatMessage("Cannot cast " + DisplayName + ". Target is already reviving");
                }
            }
            return false;
        }

        /*
        public override void CastComplete(CharacterUnit source, GameObject target, AbilityEffectOutput abilityEffectInput) {
            base.CastComplete(source, target, abilityEffectInput);
            ResurrectTarget(target);
        }
        */


    }
}