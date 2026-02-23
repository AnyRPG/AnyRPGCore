using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class MovementJumpState : IMovementState {

        private UnitController unitController;
        private UnitMovementController unitMovementController;

        public MovementJumpState(UnitMovementController unitMovementController, UnitController unitController) {
            this.unitController = unitController;
            this.unitMovementController = unitMovementController;
        }

        public void Enter() {
            //Debug.Log($"{unitController.gameObject.name}.MovementJumpState.Enter()");
            unitMovementController.localMoveVelocity.y = (Vector3.up * unitMovementController.jumpAcceleration).y;
            unitMovementController.adjustedlocalMoveVelocity = unitMovementController.localMoveVelocity;
            unitMovementController.canJump = false;
            unitController.UnitAnimator.SetJumping(1);
            unitController.UnitAnimator.SetTrigger("JumpTrigger");
            unitMovementController.lastJumpFrame = Time.frameCount;
            unitController.UnitEventController.NotifyOnJump();
            unitMovementController.MoveRelative();
        }

        public void Exit() {
            //Debug.Log($"{unitController.gameObject.name}.MovementJumpState.Exit()");
        }

        public void Update() {
            //Debug.Log($"{unitController.gameObject.name}.MovementJumpState.Update()");
            if (unitController.InWater == true) {
                if (unitMovementController.CheckForSwimming() == true) {
                    unitMovementController.ChangeState(CharacterMovementState.Swim);
                    return;
                }
            }

            if (unitController.CanFly
                && unitMovementController.CurrentMovementData.inputFly) {
                unitMovementController.ChangeState(CharacterMovementState.Fly);
                return;
            }


            if (unitController.UnitMotor.MovementBody.GetLinearVelocity().y <= 0f && Time.frameCount > (unitMovementController.lastJumpFrame + 2)) {
                if (unitController.CanGlide) {
                    unitMovementController.ChangeState(CharacterMovementState.Glide);
                    return;
                }
                unitMovementController.ChangeState(CharacterMovementState.Fall);
                return;
            }
        }
    }

}