using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New PetEffect", menuName = "AnyRPG/Abilities/Effects/PetEffect")]
    public class PetEffect : StatusEffect {

        public override bool CanUseOn(Interactable target, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext = null, bool playerInitiated = false) {
            if (target == null) {
                return false;
            }
            UnitController unitController = target as UnitController;
            if (unitController == null || unitController.UnitProfile == null || unitController.UnitProfile.IsPet == false) {
                // has to be the right unit type plus needs to be capturable specifically
                //Debug.Log(DisplayName + ".CapturePetEffect.CanUseOn(): pet was not capturable ");
                if (playerInitiated) {
                    sourceCharacter.AbilityManager.ReceiveCombatMessage("Cannot cast " + resourceName + ". Target must be a capturable pet");
                }
                return false;
            }

            return base.CanUseOn(target, sourceCharacter, abilityEffectContext, playerInitiated);
        }




    }
}
