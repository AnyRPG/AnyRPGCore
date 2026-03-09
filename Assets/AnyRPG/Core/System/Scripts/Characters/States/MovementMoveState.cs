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
            //Debug.Log($"{unitController.gameObject.name}.MovementMoveState.Enter(isReplay: {isReplay}) tick: {unitMovementController.CurrentMovementData.SimulatedTick}");

            unitMovementController.EnterGroundStateCommon(isReplay);
            unitMovementController.CalculateFallDamage(isReplay);
        }

        public void Exit(bool isReplay, bool isSilent) {
            //Debug.Log($"{unitController.gameObject.name}.MovementMoveState.Exit(isReplay: {isReplay}) tick: {unitMovementController.CurrentMovementData.SimulatedTick}");

            if (isReplay == false) {
                unitController.UnitAnimator.SetMoving(false);
            }
        }

        /*
        public void Update(bool isReplay, double timeInterval) {
            //Debug.Log($"{unitController.gameObject.name}.MovementMoveState.Update()");

            float calculatedSpeed = 0f;

            if (unitController.InWater == true) {
                if (unitMovementController.CheckForSwimming() == true) {
                    unitMovementController.ChangeState(CharacterMovementState.Swim, isReplay);
                    return;
                }
            }

            if (unitMovementController.CurrentMovementData.InputJump) {
                unitMovementController.ChangeState(CharacterMovementState.Jump, isReplay);
                return;
            }

            if (unitController.CanFly && unitMovementController.CurrentMovementData.InputFly) {
                unitMovementController.ChangeState(CharacterMovementState.Jump, isReplay);
                return;
            }

            // since we are in the move state, reset velocity to zero so we can pick up the new values
            // allow falling while moving by clamping existing y velocity
            unitMovementController.intendedLocalMoveVelocity = new Vector3(0, Mathf.Clamp(unitController.RigidBody.linearVelocity.y, -53, 0), 0);
            unitMovementController.adjustedlocalMoveVelocity = unitMovementController.intendedLocalMoveVelocity;

            // determine direction of travel in world space
            Vector3 directionOfTravel = unitController.transform.forward;

            if (unitMovementController.CurrentMovementData.HasMoveInput()) {

                // set clampValue to default of max movement speed
                float clampValue = unitMovementController.MaxMovementSpeed;

                // set a clamp value to limit movement speed to walking if going backward
                //if (currentMoveVelocity.z < 0) {
                //    clampValue = 1;
                //}

                // get current movement speed and clamp it to current clamp value
                calculatedSpeed = Mathf.Clamp(unitController.MovementSpeed, 0, clampValue);

                // multiply normalized movement by calculated speed to get actual local movement
                unitMovementController.intendedLocalMoveVelocity = unitMovementController.CurrentMovementData.IntendedLocalDirection * calculatedSpeed;

                if (unitMovementController.intendedLocalMoveVelocity.x != 0 || unitMovementController.intendedLocalMoveVelocity.z != 0) {
                    directionOfTravel = unitController.transform.TransformDirection(new Vector3(unitMovementController.intendedLocalMoveVelocity.x, 0, unitMovementController.intendedLocalMoveVelocity.z)).normalized;
                }

                // determine if there is an obstacle in front, and if it is stairs
                unitMovementController.CheckFrontObstacle(calculatedSpeed, directionOfTravel, timeInterval);

            }

            if (
                !unitMovementController.MaintainingGround() ||
                (unitMovementController.groundAngle > unitMovementController.slopeLimit && unitMovementController.nearBottomFrontObstacle == true && unitMovementController.nearLowObstacle == false) ||
                (unitMovementController.groundAngle > unitMovementController.slopeLimit && unitMovementController.nearBottomFrontObstacle == false && unitMovementController.nearLowObstacle == false && unitMovementController.closestWalkableGroundDistance < -unitMovementController.stepHeight)
                ) { // closetoGround check for running backward off low obstacle
                if (unitController.CanFly) {
                    unitMovementController.ChangeState(CharacterMovementState.Fly, isReplay);
                    return;
                } else {
                    if (unitController.CanGlide) {
                        unitMovementController.ChangeState(CharacterMovementState.Glide, isReplay);
                        return;
                    }
                    unitMovementController.ChangeState(CharacterMovementState.Fall, isReplay);
                    return;
                }
            }

            if (unitMovementController.CurrentMovementData.HasMoveInput() || unitMovementController.CurrentMovementData.HasTurnInput()) {

                if (unitMovementController.CurrentMovementData.HasMoveInput()) {
                    unitMovementController.adjustedlocalMoveVelocity = unitMovementController.NormalizedLocalMovement(calculatedSpeed, directionOfTravel, timeInterval) * calculatedSpeed;
                }
                unitMovementController.CalculateTurnVelocity();
            } else {
                unitMovementController.currentTurnVelocity = Vector3.zero;
                unitMovementController.ChangeState(CharacterMovementState.Idle, isReplay);
                return;
            }

            unitMovementController.MoveRelative();
            
            if (isReplay == false) {
                unitMovementController.AnimatorMoveUpdate();
            }
        }
        */

        public void Update(bool isReplay, double timeInterval) {
            //Debug.Log($"{unitController.gameObject.name}.MovementMoveState.Update()");

            float calculatedSpeed = 0f;

            if (unitController.InWater == true) {
                if (unitMovementController.CheckForSwimming() == true) {
                    unitMovementController.ChangeState(CharacterMovementState.Swim, isReplay);
                    return;
                }
            }

            if (unitMovementController.CurrentMovementData.InputJump) {
                unitMovementController.ChangeState(CharacterMovementState.Jump, isReplay);
                return;
            }

            if (unitController.CanFly && unitMovementController.CurrentMovementData.InputFly) {
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

                if (unitController.CanFly) { unitMovementController.ChangeState(CharacterMovementState.Fly, isReplay); return; }
                if (unitController.CanGlide) { unitMovementController.ChangeState(CharacterMovementState.Glide, isReplay); return; }
                unitMovementController.ChangeState(CharacterMovementState.Fall, isReplay);
                return;
            }

            // 6. APPLY MOVEMENT LOGIC
            if (unitMovementController.CurrentMovementData.HasMoveInput() || unitMovementController.CurrentMovementData.HasTurnInput()) {

                if (unitMovementController.CurrentMovementData.HasMoveInput()) {
                    // Use the new World-Space slope alignment
                    unitMovementController.adjustedWorldMoveVelocity = unitMovementController.NormalizedWorldMovement(calculatedSpeed, timeInterval) * calculatedSpeed;

                    // 7. ROTATE CHARACTER (Apply via Motor for CSP safety)
                    if (unitController.UnitProfile.UnitPrefabProps.RotateModel || unitMovementController.CurrentMovementData.GamepadModeActive) {
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