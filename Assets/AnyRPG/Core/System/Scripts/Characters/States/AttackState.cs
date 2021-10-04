using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class AttackState : IState {
        private UnitController baseController;

        public void Enter(UnitController baseController) {
            //Debug.Log(baseController.gameObject.name + ".AttackState.Enter()");
            this.baseController = baseController;
            this.baseController.UnitMotor.StopFollowingTarget();
        }

        public void Exit() {
            //Debug.Log(baseController.gameObject.name + ".AttackState.Exit()");
        }

        public void Update() {
            //Debug.Log(aiController.gameObject.name + ": AttackState.Update()");

            baseController.UpdateTarget();

            if (baseController.Target == null) {
                //Debug.Log(aiController.gameObject.name + ": about to change to returnstate");
                baseController.ChangeState(new ReturnState());
                return;
            }

            if (baseController.CharacterUnit.BaseCharacter.CharacterAbilityManager.WaitingForAnimatedAbility == true) {
                //Debug.Log(baseController.gameObject.name + ".AttackState.Update() WaitingForAnimatedAbility is true");
                // nothing to do, animated ability in progress
                return;
            }

            if (baseController.CharacterUnit.BaseCharacter.CharacterCombat.WaitingForAutoAttack == true) {
                //Debug.Log(baseController.gameObject.name + ".AttackState.Update() WaitingForAutoAttack == true");
                // nothing to do, auto-attack in progress
                return;
            }

            if (baseController.CharacterUnit.BaseCharacter.CharacterAbilityManager.IsCasting == true || baseController.CharacterUnit.BaseCharacter.CharacterAbilityManager.MyCurrentCastCoroutine != null) {
                //Debug.Log(baseController.gameObject.name + ".AttackState.Update() CurrentCast != null || IsCasting == true");
                // nothing to do, cast in progress
                return;
            }

            // face target before attack to ensure they are in the hitbox
            baseController.UnitMotor.FaceTarget(baseController.Target);

            if (baseController.CanGetValidAttack(true)) {
                //Debug.Log(baseController.gameObject.name + ".AttackState.Update(): got valid ability");
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