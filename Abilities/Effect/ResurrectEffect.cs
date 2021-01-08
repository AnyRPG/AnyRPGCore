using AnyRPG;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New ResurrectEffect", menuName = "AnyRPG/Abilities/Effects/ResurrectEffect")]
    public class ResurrectEffect : InstantEffect {

        /// <summary>
        /// Does the actual work of hitting the target with an ability
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public override void PerformAbilityHit(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(resourceName + ".ResurrectEffect.PerformAbilityEffect(" + source.name + ", " + (target == null ? "null" : target.name) + ") effect: " + resourceName);
            AbilityEffectContext abilityEffectOutput = new AbilityEffectContext();
            abilityEffectOutput.groundTargetLocation = abilityEffectInput.groundTargetLocation;
            ResurrectTarget(target);
            base.PerformAbilityHit(source, target, abilityEffectOutput);
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
            characterUnit.BaseCharacter.CharacterStats.Revive();
        }

        public override bool CanUseOn(Interactable target, IAbilityCaster source, AbilityEffectContext abilityEffectContext = null, bool playerInitiated = false, bool performRangeCheck = true) {
            if (target == null) {
                return false;
            }
            CharacterUnit characterUnit = CharacterUnit.GetCharacterUnit(target);
            if (characterUnit == null) {
                return false;
            }
            if (characterUnit.BaseCharacter.CharacterStats.IsAlive == false && characterUnit.BaseCharacter.CharacterStats.IsReviving == false) {
                return true;
            }
            if (characterUnit.BaseCharacter.CharacterStats.IsAlive == true) {
                if (playerInitiated) {
                    sourceCharacter.AbilityManager.ReceiveCombatMessage("Cannot cast " + resourceName + ". Target is already alive");
                }
            }
            if (characterUnit.BaseCharacter.CharacterStats.IsReviving == true) {
                if (playerInitiated) {
                    sourceCharacter.AbilityManager.ReceiveCombatMessage("Cannot cast " + resourceName + ". Target is already reviving");
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