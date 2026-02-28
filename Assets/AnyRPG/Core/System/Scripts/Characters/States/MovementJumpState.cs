using UnityEngine;

namespace AnyRPG {
    public class MovementJumpState : IMovementState {

        private UnitController unitController;
        private UnitMovementController unitMovementController;

        public MovementJumpState(UnitMovementController unitMovementController, UnitController unitController) {
            this.unitController = unitController;
            this.unitMovementController = unitMovementController;
        }

        public void Enter(bool isReplay) {
            Debug.Log($"{unitController.gameObject.name}.MovementJumpState.Enter(isReplay: {isReplay})");

            unitMovementController.localMoveVelocity.y = (Vector3.up * unitMovementController.jumpAcceleration).y;
            unitMovementController.adjustedlocalMoveVelocity = unitMovementController.localMoveVelocity;
            unitMovementController.canJump = false;
            unitMovementController.lastJumpFrame = unitMovementController.CurrentMovementData.SimulatedTick;
            if (isReplay == false) {
                unitController.UnitAnimator.SetJumping(1);
                unitController.UnitAnimator.SetTrigger("JumpTrigger");
                unitController.UnitEventController.NotifyOnJump();
            }
            unitMovementController.MoveRelative();
        }

        public void Exit(bool isReplay) {
            Debug.Log($"{unitController.gameObject.name}.MovementJumpState.Exit(isReplay: {isReplay})");
        }

        public void Update(bool isReplay) {
            Debug.Log($"{unitController.gameObject.name}.MovementJumpState.Update(isReplay: {isReplay})");

            if (unitController.InWater == true) {
                if (unitMovementController.CheckForSwimming() == true) {
                    unitMovementController.ChangeState(CharacterMovementState.Swim, isReplay);
                    return;
                }
            }

            if (unitController.CanFly
                && unitMovementController.CurrentMovementData.InputFly) {
                unitMovementController.ChangeState(CharacterMovementState.Fly, isReplay);
                return;
            }

            //Debug.Log($"{unitController.gameObject.name}.MovementJumpState.Update() tick: {unitMovementController.CurrentMovementData.SimulatedTick}, frame: {unitMovementController.lastJumpFrame}");
            if (unitController.UnitMotor.MovementBody.GetLinearVelocity().y <= 0f && unitMovementController.CurrentMovementData.SimulatedTick > (unitMovementController.lastJumpFrame + 2)) {
                if (unitController.CanGlide) {
                    unitMovementController.ChangeState(CharacterMovementState.Glide, isReplay);
                    return;
                }
                unitMovementController.ChangeState(CharacterMovementState.Fall, isReplay);
                return;
            }
        }
    }

}