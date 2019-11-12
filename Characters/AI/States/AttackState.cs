using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class AttackState : IState {
        private AIController aiController;

        public void Enter(AIController enemyController) {
            //Debug.Log("Entering Attack State");
            this.aiController = enemyController;
            this.aiController.MyBaseCharacter.MyCharacterUnit.MyCharacterMotor.StopFollowingTarget();
        }

        public void Exit() {

        }

        public void Update() {
            //Debug.Log(aiController.gameObject.name + ": AttackState.Update()");

            aiController.UpdateTarget();

            if (aiController.MyTarget != null) {

                if (aiController.MyBaseCharacter.MyCharacterAbilityManager.MyCurrentCast != null) {
                    Debug.Log(aiController.gameObject.name + ".AttackState.Update() MyCurrentCast != null");
                    // nothing to do, cast in progress
                    return;
                }

                aiController.MyBaseCharacter.MyCharacterUnit.MyCharacterMotor.FaceTarget(aiController.MyTarget);

                BaseAbility validAttackAbility = (aiController.MyBaseCharacter.MyCharacterCombat as AICombat).GetValidAttackAbility();
                if (validAttackAbility != null) {
                    Debug.Log(aiController.gameObject.name + ".AttackState.Update() Got Valid Attack Ability: " + validAttackAbility.MyName);
                    // perform ability attack
                    aiController.MyBaseCharacter.MyCharacterAbilityManager.BeginAbility(validAttackAbility);
                } else {
                    // auto attack should only be run if we couldn't find a valid ability
                    if (!aiController.IsTargetInHitBox(aiController.MyTarget)) {
                        // target is out of range, follow it
                        aiController.ChangeState(new FollowState());
                    } else {
                        // target is in range, attack it
                        if (aiController.MyBaseCharacter.MyCharacterCombat.MyWaitingForAutoAttack == false) {
                            // we were getting too much spam in logs from just passing this through rather than waiting until it was a valid action
                            aiController.AttackCombatTarget();
                        }
                    }
                }
            } else {
                //Debug.Log(aiController.gameObject.name + ": about to change to returnstate");
                aiController.ChangeState(new ReturnState());
            }
        }

    }

}