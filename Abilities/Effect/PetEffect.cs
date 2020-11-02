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

        public override bool CanUseOn(Interactable target, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext = null) {
            if (target == null) {
                return false;
            }
            CharacterUnit characterUnit = CharacterUnit.GetCharacterUnit(target);
            if (characterUnit == null) {
                return false;
            }
            if (target.UnitController == null || target.UnitController.UnitProfile == null) {
                return false;
            }
            if (!target.UnitController.UnitProfile.IsPet) {
                return false;
            }

            return base.CanUseOn(target, sourceCharacter, abilityEffectContext);
        }




    }
}
