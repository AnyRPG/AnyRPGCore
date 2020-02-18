using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class AIAbilityManager : CharacterAbilityManager {
        protected override void BeginAbilityCommon(IAbility ability, GameObject target) {

            base.BeginAbilityCommon(ability, target);
            if (currentCastAbility != null && currentCastAbility.MyRequiresGroundTarget == true) {
                Vector3 groundTarget = Vector3.zero;
                if (baseCharacter != null && baseCharacter.MyCharacterController != null && baseCharacter.MyCharacterController.MyTarget != null) {
                    groundTarget = baseCharacter.MyCharacterController.MyTarget.transform.position;
                }
                SetGroundTarget(groundTarget);
            }
        }
    }

}