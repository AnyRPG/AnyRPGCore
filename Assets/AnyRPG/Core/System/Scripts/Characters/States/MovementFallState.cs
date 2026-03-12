using UnityEngine;

namespace AnyRPG {
    public class MovementFallState : IMovementState {

        private UnitController unitController;
        private UnitMovementController unitMovementController;

        public MovementFallState(UnitMovementController unitMovementController, UnitController unitController) {
            this.unitController = unitController;
            this.unitMovementController = unitMovementController;
        }

        public void Enter(bool isReplay, bool isSilent) {
            Debug.Log($"{unitController.gameObject.name}.MovementFallState.Enter(isReplay: {isReplay}, isSilent: {isSilent}) frame: {Time.frameCount} tick: {(isSilent ? "N/A" : unitMovementController.CurrentMovementData.SimulatedTick)} rposition: {unitController.UnitMotor.MovementBody.GetPosition()} mposition: {unitController.UnitModelController.UnitModel.transform.position} velocity: {unitController.UnitMotor.MovementBody.GetLinearVelocity()}");
            
            if (isSilent) return;

            if (isReplay == false) {
                unitMovementController.currentFallDistance = 0f;
                unitMovementController.fallStartHeight = unitController.transform.position.y;
                if (unitController.UnitAnimator.GetInt("Jumping") != 2) {
                    unitController.UnitAnimator.SetTrigger("FallTrigger");
                    unitController.UnitAnimator.SetJumping(2);
                }
            }

            // clamp y velocity to prevent launching off ramps
            unitController.UnitMotor?.Move(new Vector3(unitController.RigidBody.linearVelocity.x, Mathf.Clamp(unitController.RigidBody.linearVelocity.y, -53, 0), unitController.RigidBody.linearVelocity.z));
        }

        public void Exit(bool isReplay, bool isSilent) {
            Debug.Log($"{unitController.gameObject.name}.MovementFallState.Exit(isReplay: {isReplay}, isSilent: {isSilent}) frame: {Time.frameCount} tick: {(isSilent ? "N/A" : unitMovementController.CurrentMovementData.SimulatedTick)} rposition: {unitController.UnitMotor.MovementBody.GetPosition()} mposition: {unitController.UnitModelController.UnitModel.transform.position} velocity: {unitController.UnitMotor.MovementBody.GetLinearVelocity()}");
            
            if (isSilent) return;
            unitMovementController.currentFallDistance = unitMovementController.fallStartHeight - unitController.transform.position.y;

            if (isReplay == false) {
                unitController.UnitAnimator.SetJumping(0);
            }
        }

        public void Update(bool isReplay, double timeInterval) {
            Debug.Log($"{unitController.gameObject.name}.MovementFallState.Update(isReplay: {isReplay}) frame: {Time.frameCount} tick: {unitMovementController.CurrentMovementData.SimulatedTick} rposition: {unitController.UnitMotor.MovementBody.GetPosition()} mposition: {unitController.UnitModelController.UnitModel.transform.position} velocity: {unitController.UnitMotor.MovementBody.GetLinearVelocity()}");

            if (unitController.InWater == true) {
                Debug.Log($"{unitController.gameObject.name}.MovementFallState.Update() IN WATER = TRUE; CheckForSwimming() == {unitMovementController.CheckForSwimming()}");
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