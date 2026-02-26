using UnityEngine;

namespace AnyRPG {
    public class MovementFallState : IMovementState {

        private UnitController unitController;
        private UnitMovementController unitMovementController;

        public MovementFallState(UnitMovementController unitMovementController, UnitController unitController) {
            this.unitController = unitController;
            this.unitMovementController = unitMovementController;
        }

        public void Enter(bool isReplay) {
            Debug.Log($"{unitController.gameObject.name}.MovementFallState.Enter(isReplay: {isReplay})");

            unitMovementController.canJump = false;
            unitMovementController.currentFallDistance = 0f;
            unitMovementController.fallStartHeight = unitController.transform.position.y;

            if (isReplay == false) {
                if (unitController.UnitAnimator.GetInt("Jumping") != 2) {
                    unitController.UnitAnimator.SetTrigger("FallTrigger");
                    unitController.UnitAnimator.SetJumping(2);
                }
            }

            // clamp y velocity to prevent launching off ramps
            unitController.UnitMotor?.Move(new Vector3(unitController.RigidBody.linearVelocity.x, Mathf.Clamp(unitController.RigidBody.linearVelocity.y, -53, 0), unitController.RigidBody.linearVelocity.z));
        }

        public void Exit(bool isReplay) {
            Debug.Log($"{unitController.gameObject.name}.MovementFallState.Exit(isReplay: {isReplay})");

            unitMovementController.currentFallDistance = unitMovementController.fallStartHeight - unitController.transform.position.y;

            if (isReplay == false) {
                unitController.UnitAnimator.SetJumping(0);
            }
        }

        public void Update(bool isReplay) {
            //Debug.Log($"{unitController.gameObject.name}.MovementFallState.Update()");
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

            if (unitController.CanGlide) {
                unitMovementController.ChangeState(CharacterMovementState.Glide, isReplay);
                return;
            }


            if (unitMovementController.touchingGround && unitMovementController.groundAngle <= unitMovementController.slopeLimit) {
                if (unitMovementController.CurrentMovementData.HasMoveInput() || unitMovementController.CurrentMovementData.HasTurnInput()) {
                    unitMovementController.ChangeState(CharacterMovementState.Move, isReplay);
                    return;
                }
                unitMovementController.ChangeState(CharacterMovementState.Idle, isReplay);
                return;
            }

        }
    }

}