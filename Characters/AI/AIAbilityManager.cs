using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class AIAbilityManager : CharacterAbilityManager {
        protected override void BeginAbilityCommon(IAbility ability, GameObject target) {

            base.BeginAbilityCommon(ability, target);
            if (currentCastAbility != null && currentCastAbility.RequiresGroundTarget == true) {
                Vector3 groundTarget = Vector3.zero;
                if (baseCharacter != null && baseCharacter.CharacterController != null && baseCharacter.CharacterController.MyTarget != null) {
                    groundTarget = baseCharacter.CharacterController.MyTarget.transform.position;
                }
                SetGroundTarget(groundTarget);
            }
        }

        public override void ActivateTargettingMode(BaseAbility baseAbility, GameObject target) {
            base.ActivateTargettingMode(baseAbility, target);

            groundTarget = target.transform.position;
        }
    }

}