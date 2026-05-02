using UnityEngine;

namespace AnyRPG {
    public class MovementFlyState : IMovementState {

        private UnitController unitController;
        private UnitMovementController unitMovementController;

        public MovementFlyState(UnitMovementController unitMovementController, UnitController unitController) {
            this.unitController = unitController;
            this.unitMovementController = unitMovementController;
        }

        public void Enter(bool isReplay, bool isSilent) {
            //Debug.Log($"{unitController.gameObject.name}.MovementFlyState.Enter(isReplay: {isReplay}) tick:  {unitMovementController.CurrentMovementData.SimulatedTick}");

            // 1. PERSISTENT PHYSICS & STATE (Always run during replays)
            unitMovementController.currentFallDistance = 0f;

            // Ensure the motor knows we are in 'Flying' mode for this tick's simulation
            unitController.StartFlying(isReplay);

            // Rigidbody state must be forced every replay to match the Server's simulation
            unitController.RigidBody.useGravity = false;

            // 2. VISUALS & ONE-SHOT TRIGGERS (Guard with !isReplay)
            if (!isReplay) {
                // These only need to happen once on the initial "Prediction"
                unitController.UnitAnimator.SetBool("Flying", true);
                unitController.UnitAnimator.SetTrigger("FlyTrigger");

                // If there's a "Takeoff" sound or effect, trigger it here:
                // unitController.UnitEventController.NotifyOnFlyStart();
            }
        }


        public void Exit(bool isReplay, bool isSilent) {
            //Debug.Log($"{unitController.gameObject.name}.MovementFlyState.Exit(isReplay: {isReplay}) frame: {Time.frameCount} tick: {(isSilent ? "N/A" : unitMovementController.CurrentMovementData.SimulatedTick)}");

            unitController.StopFlying(isReplay);
            unitController.RigidBody.useGravity = true;
            unitController.RigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            if (isReplay == false) {
                unitController.UnitAnimator.SetBool("Flying", false);
            }
        }

        public void Update(bool isReplay, double timeInterval) {
            //Debug.Log($"{unitController.gameObject.name}.MovementFlyState.Update()");

            if (unitMovementController.touchingGround == true && unitMovementController.CurrentMovementData.InputFly == false) {
                if (unitMovementController.CurrentMovementData.HasMoveInput()) {
                    unitMovementController.ChangeState(CharacterMovementState.Move, isReplay);
                    return;
                } else {
                    unitMovementController.ChangeState(CharacterMovementState.Idle, isReplay);
                    return;
                }
            }
            if (unitController.InWater == true && unitController.IsEncumbered == false && unitMovementController.CheckForSwimming() == true) {
                unitMovementController.ChangeState(CharacterMovementState.Swim, isReplay);
                return;
            }
            if (unitController.CanFly == false || unitController.IsEncumbered == true) {
                unitMovementController.ChangeState(CharacterMovementState.Fall, isReplay);
                return;
            }


            if (unitMovementController.CurrentMovementData.HasFlyMoveInput() || unitMovementController.CurrentMovementData.HasTurnInput()) {
                // ============ RIGIDBODY CONSTRAINTS ============
                unitController.RigidBody.constraints = RigidbodyConstraints.FreezeRotation;

                // ============ VELOCITY CALCULATIONS ============
                float clampValue = unitMovementController.MaxMovementSpeed;
                float calculatedSpeed = Mathf.Clamp(unitController.FlySpeed, 0, clampValue);

                if (unitMovementController.CurrentMovementData.HasFlyMoveInput()) {
                    // 2. CALCULATE WORLD VELOCITY (The Physics Truth)
                    unitMovementController.intendedWorldMoveVelocity = unitMovementController.WorldNormalizedFlyMovement() * calculatedSpeed;
                    unitMovementController.adjustedWorldMoveVelocity = unitMovementController.intendedWorldMoveVelocity;

                    Vector3 horizontalDir = new Vector3(unitMovementController.intendedWorldMoveVelocity.x, 0, unitMovementController.intendedWorldMoveVelocity.z);

                    // 3. ROTATE CHARACTER (Apply via Motor)
                    if (unitMovementController.CurrentMovementData.RotateModelMode) {
                        // If moving, face the world direction (ignoring Y for upright rotation)
                        if (horizontalDir.sqrMagnitude > 0.001f) {
                            unitController.UnitMotor.FaceDirection(horizontalDir);
                        }
                    }

                    // 4. DERIVE LOCAL VELOCITY (For the Animator)
                    // This is calculated AFTER FaceDirection so it reflects the correct strafe angles
                    // Use the "Truth" (the Rigidbody's current rotation) to localize the velocity
                    Quaternion physicsRot = unitController.UnitMotor.MovementBody.GetRotation();
                    unitMovementController.intendedLocalMoveVelocity = Quaternion.Inverse(physicsRot) * unitMovementController.intendedWorldMoveVelocity;

                    unitMovementController.adjustedLocalMoveVelocity = unitMovementController.intendedLocalMoveVelocity;
                }

                unitMovementController.CalculateTurnVelocity();

                if (isReplay == false) {
                    unitController.UnitAnimator.SetMoving(true);
                    unitController.UnitAnimator.SetTurnVelocity(unitMovementController.currentTurnVelocity.x);
                }
            } else {
                // ============ STOPPING LOGIC ============
                unitController.FreezeAll();
                unitMovementController.intendedWorldMoveVelocity = Vector3.zero;
                unitMovementController.adjustedWorldMoveVelocity = Vector3.zero;
                unitMovementController.intendedLocalMoveVelocity = Vector3.zero;
                unitMovementController.adjustedLocalMoveVelocity = Vector3.zero;

                if (isReplay == false) {
                    unitController.UnitAnimator.SetMoving(false);
                    unitController.UnitAnimator.SetTurnVelocity(0f);
                }
            }

            // 5. APPLY PHYSICS
            // This now uses the stable world velocity
            unitMovementController.MoveWorld();

            // 6. UPDATE VISUALS
            if (isReplay == false) {
                unitController.UnitAnimator.SetVelocityFromLocal(unitMovementController.intendedLocalMoveVelocity, unitMovementController.CurrentMovementData.RotateModelMode);
            }
        }

    }

}