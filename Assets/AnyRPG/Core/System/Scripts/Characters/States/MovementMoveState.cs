using UnityEngine;

namespace AnyRPG {
    public class MovementMoveState : IMovementState {

        private UnitController unitController;
        private UnitMovementController unitMovementController;

        public MovementMoveState(UnitMovementController unitMovementController, UnitController unitController) {
            this.unitController = unitController;
            this.unitMovementController = unitMovementController;
        }

        public void Enter(bool isReplay, bool isSilent) {
            //Debug.Log($"{unitController.gameObject.name}.MovementMoveState.Enter(isReplay: {isReplay}, isSilent: {isSilent}) frame: {Time.frameCount} tick: {(isSilent ? "N/A" : unitMovementController.CurrentMovementData.SimulatedTick)} rposition: {unitController.UnitMotor.MovementBody.GetPosition()} mposition: {unitController.UnitModelController.UnitModel.transform.position} velocity: {unitController.UnitMotor.MovementBody.GetLinearVelocity()}");

            unitMovementController.EnterGroundStateCommon(isReplay);
            unitMovementController.CalculateFallDamage(isReplay);
        }

        public void Exit(bool isReplay, bool isSilent) {
            //Debug.Log($"{unitController.gameObject.name}.MovementMoveState.Exit(isReplay: {isReplay}, isSilent: {isSilent}) frame: {Time.frameCount} tick: {(isSilent ? "N/A" : unitMovementController.CurrentMovementData.SimulatedTick)} rposition: {unitController.UnitMotor.MovementBody.GetPosition()} mposition: {unitController.UnitModelController.UnitModel.transform.position} velocity: {unitController.UnitMotor.MovementBody.GetLinearVelocity()}");

            if (isReplay == false) {
                unitController.UnitAnimator.SetMoving(false);
            }
        }

        public void Update(bool isReplay, double timeInterval) {
            //Debug.Log($"{unitController.gameObject.name}.MovementMoveState.Update(isreplay: {isReplay}) frame: {Time.frameCount} tick: {unitMovementController.CurrentMovementData.SimulatedTick}");

            float calculatedSpeed = 0f;

            if (unitController.InWater == true && unitController.IsEncumbered == false) {
                if (unitMovementController.CheckForSwimming() == true) {
                    unitMovementController.ChangeState(CharacterMovementState.Swim, isReplay);
                    return;
                }
            }

            if (unitMovementController.CurrentMovementData.InputJump && unitController.IsEncumbered == false) {
                unitMovementController.ChangeState(CharacterMovementState.Jump, isReplay);
                return;
            }

            if (unitController.CanFly && unitController.IsEncumbered == false && unitMovementController.CurrentMovementData.InputFly) {
                unitMovementController.ChangeState(CharacterMovementState.Jump, isReplay);
                return;
            }

            // 2. INITIALIZE VELOCITIES
            // Preserve existing vertical velocity (gravity)
            float currentYVel = Mathf.Clamp(unitController.RigidBody.linearVelocity.y, -53, 0);
            unitMovementController.intendedWorldMoveVelocity = new Vector3(0, currentYVel, 0);
            unitMovementController.adjustedWorldMoveVelocity = unitMovementController.intendedWorldMoveVelocity;

            if (unitMovementController.CurrentMovementData.HasMoveInput()) {
                float clampValue = unitMovementController.MaxMovementSpeed;
                calculatedSpeed = Mathf.Clamp(unitController.MovementSpeed, 0, clampValue);

                // 3. CALCULATE WORLD INTENT
                // We use the raw World Direction we gathered from the camera
                unitMovementController.intendedWorldMoveVelocity = unitMovementController.CurrentMovementData.IntendedWorldDirection * calculatedSpeed;
                unitMovementController.intendedWorldMoveVelocity.y = currentYVel;

                // 4. CHECK FOR OBSTACLES (Uses world direction)
                unitMovementController.CheckFrontObstacle(calculatedSpeed, unitMovementController.CurrentMovementData.IntendedWorldDirection, timeInterval);
            }

            // 5. GROUNDING / SLOPE CHECKS
            if (!unitMovementController.MaintainingGround() ||
                (unitMovementController.groundAngle > unitMovementController.slopeLimit && unitMovementController.nearBottomFrontObstacle && !unitMovementController.nearLowObstacle) ||
                (unitMovementController.groundAngle > unitMovementController.slopeLimit && !unitMovementController.nearBottomFrontObstacle && !unitMovementController.nearLowObstacle && unitMovementController.closestWalkableGroundDistance < -unitMovementController.stepHeight)) {

                if (unitController.CanFly && unitController.IsEncumbered == false) {
                    unitMovementController.ChangeState(CharacterMovementState.Fly, isReplay);
                    return;
                }
                if (unitController.CanGlide && unitController.IsEncumbered == false) {
                    unitMovementController.ChangeState(CharacterMovementState.Glide, isReplay);
                    return;
                }
                unitMovementController.ChangeState(CharacterMovementState.Fall, isReplay);
                return;
            }

            // 6. APPLY MOVEMENT LOGIC
            if (unitMovementController.CurrentMovementData.HasMoveInput() || unitMovementController.CurrentMovementData.HasTurnInput()) {

                if (unitMovementController.CurrentMovementData.HasMoveInput()) {
                    // Use the new World-Space slope alignment
                    unitMovementController.adjustedWorldMoveVelocity = unitMovementController.NormalizedWorldMovement(calculatedSpeed, timeInterval) * calculatedSpeed;

                    // 7. ROTATE CHARACTER (Apply via Motor for CSP safety)
                    if (unitMovementController.CurrentMovementData.RotateModelMode) {
                        //Debug.Log($"{unitController.gameObject.name}.MovementMoveState.Update() RotateModelMode is true, so facing intendedWorldMoveVelocity: {unitMovementController.intendedWorldMoveVelocity}");
                        if (unitMovementController.intendedWorldMoveVelocity.sqrMagnitude > 0.001f) {
                            unitController.UnitMotor.FaceDirection(unitMovementController.intendedWorldMoveVelocity);
                        }
                    }
                }

                unitMovementController.CalculateTurnVelocity();

                // 8. DERIVE LOCAL VALUES FOR ANIMATOR
                // This is safe because FaceDirection updated the rotation first
                //unitMovementController.intendedLocalMoveVelocity = unitController.transform.InverseTransformDirection(unitMovementController.intendedWorldMoveVelocity);
                //unitMovementController.adjustedLocalMoveVelocity = unitController.transform.InverseTransformDirection(unitMovementController.adjustedWorldMoveVelocity);

                // Use the "Truth" (the Rigidbody's current rotation) to localize the velocity
                Quaternion physicsRot = unitController.UnitMotor.MovementBody.GetRotation();
                unitMovementController.intendedLocalMoveVelocity = Quaternion.Inverse(physicsRot) * unitMovementController.intendedWorldMoveVelocity;
                unitMovementController.adjustedLocalMoveVelocity = Quaternion.Inverse(physicsRot) * unitMovementController.adjustedWorldMoveVelocity;

            } else {
                unitMovementController.currentTurnVelocity = Vector3.zero;
                unitMovementController.ChangeState(CharacterMovementState.Idle, isReplay);
                return;
            }

            // 9. EXECUTE PHYSICS & VISUALS
            unitMovementController.MoveWorld();

            if (!isReplay) {
                unitMovementController.AnimatorMoveUpdate();
            }
        }

    }

}