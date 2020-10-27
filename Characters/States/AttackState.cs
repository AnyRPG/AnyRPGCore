using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class AttackState : IState {
        private UnitController baseController;

        public void Enter(UnitController baseController) {
            //Debug.Log("Entering Attack State");
            this.baseController = baseController;
            this.baseController.UnitMotor.StopFollowingTarget();
        }

        public void Exit() {

        }

        public void Update() {
            //Debug.Log(aiController.gameObject.name + ": AttackState.Update()");

            baseController.UpdateTarget();

            if (baseController.Target == null) {
                //Debug.Log(aiController.gameObject.name + ": about to change to returnstate");
                baseController.ChangeState(new ReturnState());
                return;
            }

            if (baseController.BaseCharacter.CharacterAbilityManager.WaitingForAnimatedAbility == true) {
                //Debug.Log(aiController.gameObject.name + ".AttackState.Update() MyWaitingForAnimatedAbility is true");
                // nothing to do, animated ability in progress
                return;
            }

            if (baseController.BaseCharacter.CharacterCombat.MyWaitingForAutoAttack == true) {
                //Debug.Log(aiController.gameObject.name + ".AttackState.Update() MyWaitingForAutoAttack == true");
                // nothing to do, auto-attack in progress
                return;
            }

            if (baseController.BaseCharacter.CharacterAbilityManager.IsCasting == true || baseController.BaseCharacter.CharacterAbilityManager.MyCurrentCastCoroutine != null) {
                //Debug.Log(aiController.gameObject.name + ".AttackState.Update() MyCurrentCast != null || MyIsCasting == true");
                // nothing to do, cast in progress
                return;
            }

            // face target before attack to ensure they are in the hitbox
            baseController.UnitMotor.FaceTarget(baseController.Target);

            if (baseController.CanGetValidAttack(true)) {
                return;
            }

            // no valid ability found, try auto attack range check
            if (!baseController.IsTargetInHitBox(baseController.Target)) {
                // target is out of range, follow it
                baseController.ChangeState(new FollowState());
                return;
            }

            // target is in range, attack it
            //aiController.AttackCombatTarget();

        }

    }

}