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

        public override bool CanUseOn(GameObject target, IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext = null) {
            if (target == null) {
                return false;
            }
            CharacterUnit characterUnit = target.GetComponent<CharacterUnit>();
            if (characterUnit == null) {
                return false;
            }
            if (characterUnit.BaseCharacter == null || characterUnit.BaseCharacter.UnitProfile == null) {
                return false;
            }
            if (!characterUnit.BaseCharacter.UnitProfile.MyIsPet) {
                return false;
            }

            return base.CanUseOn(target, sourceCharacter, abilityEffectContext);
        }




    }
}
