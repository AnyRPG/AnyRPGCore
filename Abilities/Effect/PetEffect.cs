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

        public override bool CanUseOn(GameObject target, BaseCharacter sourceCharacter) {
            if (target == null) {
                return false;
            }
            CharacterUnit characterUnit = target.GetComponent<CharacterUnit>();
            if (characterUnit == null) {
                return false;
            }
            if (characterUnit.MyBaseCharacter == null || characterUnit.MyBaseCharacter.MyUnitProfile == null) {
                return false;
            }
            if (!characterUnit.MyBaseCharacter.MyUnitProfile.MyIsPet) {
                return false;
            }

            return base.CanUseOn(target, sourceCharacter);
        }




    }
}
