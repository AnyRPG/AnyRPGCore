using UnityEngine;

namespace AnyRPG {
    public class AttackState : IState {
        private UnitController unitController;

        public void Enter(UnitController unitController) {
            //Debug.Log($"{unitController.gameObject.name}.AttackState.Enter()");

            this.unitController = unitController;
            this.unitController.UnitMotor.StopFollowingTarget();
        }

        public void Exit() {
            //Debug.Log($"{unitController.gameObject.name}.AttackState.Exit()");
        }

        public void Update() {
            //Debug.Log($"{unitController.gameObject.name}.AttackState.Update()");

            unitController.UpdateTarget();

            if (unitController.Target == null) {
                //Debug.Log(aiController.gameObject.name + ": about to change to returnstate");
                unitController.ChangeState(new ReturnState());
                return;
            }

            if (unitController.CharacterAbilityManager.PerformingAnyAbility() == true) {
                //Debug.Log($"{unitController.gameObject.name}.AttackState.Update() WaitingForAnimatedAbility is true");
                // nothing to do, other attack or ability in progress
                return;
            }

            // face target before attack to ensure they are in the hitbox
            unitController.UnitMotor.FaceTarget(unitController.Target);

            if (unitController.CanGetValidAttack(true)) {
                //Debug.Log(baseController.gameObject.name + ".AttackState.Update(): got valid ability");
                return;
            }

            // no valid ability found, try auto attack range check
            if (!unitController.IsTargetInHitBox(unitController.Target)) {
                // target is out of range, follow it
                unitController.ChangeState(new FollowState());
                return;
            }

            // target is in range, attack it
            //aiController.AttackCombatTarget();

        }

    }

}