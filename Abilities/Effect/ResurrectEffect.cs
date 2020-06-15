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
        public override void PerformAbilityHit(IAbilityCaster source, GameObject target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(resourceName + ".ResurrectEffect.PerformAbilityEffect(" + source.name + ", " + (target == null ? "null" : target.name) + ") effect: " + resourceName);
            AbilityEffectContext abilityEffectOutput = new AbilityEffectContext();
            abilityEffectOutput.groundTargetLocation = abilityEffectInput.groundTargetLocation;
            ResurrectTarget(target);
            base.PerformAbilityHit(source, target, abilityEffectOutput);
        }

        private void ResurrectTarget(GameObject target) {
            if (target == null) {
                // our target despawned in the middle of the cast
                return;
            }
            CharacterUnit characterUnit = target.GetComponent<CharacterUnit>();
            if (characterUnit == null) {
                //Debug.Log("CharacterUnit is null? target despawn during cast?");
                return;
            }
            characterUnit.MyCharacter.CharacterStats.Revive();
        }

        public override bool CanUseOn(GameObject target, IAbilityCaster source, AbilityEffectContext abilityEffectContext = null) {
            if (target == null) {
                return false;
            }
            CharacterUnit characterUnit = target.GetComponent<CharacterUnit>();
            if (characterUnit == null) {
                return false;
            }
            if (characterUnit.MyCharacter.CharacterStats.IsAlive == false && characterUnit.MyCharacter.CharacterStats.IsReviving == false) {
                return true;
            }
            return false;
        }

        public override GameObject ReturnTarget(GameObject target) {
            if (target == null) {
                //Debug.Log("Ressurect spell cast, but there was no target");
                return null;
            }
            CharacterUnit targetCharacterUnit = target.GetComponent<CharacterUnit>();
            if (targetCharacterUnit == null) {
                return null;
            }
            if (targetCharacterUnit.MyCharacter.CharacterStats.IsAlive == false) {
                return target;
            }
            return null;
        }

        /*
        public override void CastComplete(CharacterUnit source, GameObject target, AbilityEffectOutput abilityEffectInput) {
            base.CastComplete(source, target, abilityEffectInput);
            ResurrectTarget(target);
        }
        */


    }
}