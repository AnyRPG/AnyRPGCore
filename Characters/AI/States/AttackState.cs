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
            this.aiController.MyBaseCharacter.AnimatedUnit.MyCharacterMotor.StopFollowingTarget();
        }

        public void Exit() {

        }

        public void Update() {
            //Debug.Log(aiController.gameObject.name + ": AttackState.Update()");

            aiController.UpdateTarget();

            if (aiController.MyTarget == null) {
                //Debug.Log(aiController.gameObject.name + ": about to change to returnstate");
                aiController.ChangeState(new ReturnState());
                return;
            }

            if (aiController.MyBaseCharacter.CharacterAbilityManager.WaitingForAnimatedAbility == true) {
                //Debug.Log(aiController.gameObject.name + ".AttackState.Update() MyWaitingForAnimatedAbility is true");
                // nothing to do, animated ability in progress
                return;
            }

            if (aiController.MyBaseCharacter.CharacterCombat.MyWaitingForAutoAttack == true) {
                //Debug.Log(aiController.gameObject.name + ".AttackState.Update() MyWaitingForAutoAttack == true");
                // nothing to do, auto-attack in progress
                return;
            }

            if (aiController.MyBaseCharacter.CharacterAbilityManager.IsCasting == true || aiController.MyBaseCharacter.CharacterAbilityManager.MyCurrentCastCoroutine != null) {
                //Debug.Log(aiController.gameObject.name + ".AttackState.Update() MyCurrentCast != null || MyIsCasting == true");
                // nothing to do, cast in progress
                return;
            }

            // face target before attack to ensure they are in the hitbox
            aiController.MyBaseCharacter.AnimatedUnit.MyCharacterMotor.FaceTarget(aiController.MyTarget);

            if (aiController.CanGetValidAttack(true)) {
                return;
            }

            // no valid ability found, try auto attack range check
            if (!aiController.IsTargetInHitBox(aiController.MyTarget)) {
                // target is out of range, follow it
                aiController.ChangeState(new FollowState());
                return;
            }

            // target is in range, attack it
            //aiController.AttackCombatTarget();

        }

    }

}