using UnityEngine;

namespace AnyRPG {
    public class MovementGlideState : IMovementState {

        private UnitController unitController;
        private UnitMovementController unitMovementController;

        public MovementGlideState(UnitMovementController unitMovementController, UnitController unitController) {
            this.unitController = unitController;
            this.unitMovementController = unitMovementController;
        }

        public void Enter(bool isReplay, bool isSilent) {
            //Debug.Log($"{unitController.gameObject.name}.MovementGlideState.Enter(isReplay: {isReplay}) frame: {Time.frameCount} tick: {(isSilent ? "N/A" : unitMovementController.CurrentMovementData.SimulatedTick)}");

            if (isSilent) return;

            // 1. PERSISTENT PHYSICS & DATA (Always run these during replays)
            // This ensures the server and client agree on the starting physics state
            unitMovementController.currentFallDistance = 0f;
            //unitMovementController.canJump = false;

            // Rigidbody settings must be reapplied during replays because 
            // a Reconcile might have reset them to default values.
            unitController.RigidBody.useGravity = false;
            unitController.RigidBody.constraints = RigidbodyConstraints.FreezeRotation;

            // clamp y velocity to prevent launching off ramps
            // Clamp velocity - critical for determinism
            unitController.UnitMotor?.Move(new Vector3(
                unitController.RigidBody.linearVelocity.x,
                Mathf.Clamp(unitController.RigidBody.linearVelocity.y, -53, 0),
                unitController.RigidBody.linearVelocity.z
            ));

            // 2. VISUALS & ONE-SHOT TRIGGERS (Guard with !isReplay)
            if (!isReplay) {
                // Only set triggers and values on the first "Predicted" frame
                if (unitController.UnitAnimator.GetInt("Jumping") != 2) {
                    unitController.UnitAnimator.SetTrigger("FallTrigger");
                    unitController.UnitAnimator.SetJumping(2);
                }
                unitController.UnitAnimator.SetTurnVelocity(0f);
            }
        }


        public void Exit(bool isReplay, bool isSilent) {
            //Debug.Log($"{unitController.gameObject.name}.MovementGlideState.Exit() frame: {Time.frameCount} tick: {(isSilent ? "N/A" : unitMovementController.CurrentMovementData.SimulatedTick)}");

            unitController.RigidBody.useGravity = true;
            if (isReplay == false) {
                unitController.UnitAnimator.SetJumping(0);
            }
        }

        public void Update(bool isReplay, double timeInterval) {
            //Debug.Log($"{unitController.gameObject.name}.MovementGlideState.Update()");

            if (unitController.InWater == true) {
                if (unitController.IsEncumbered == false && unitMovementController.CheckForSwimming() == true) {
                    //Debug.Log("PlayerUnitMovementController.Glide_StateUpdate() swimming");
                    unitMovementController.ChangeState(CharacterMovementState.Swim, isReplay);
                    return;
                }
            }

            if (unitController.CanFly
                && unitController.IsEncumbered == false
                && unitMovementController.CurrentMovementData.InputFly) {
                //Debug.Log("PlayerUnitMovementController.Glide_StateUpdate() flying");
                unitMovementController.ChangeState(CharacterMovementState.Fly, isReplay);
                return;
            }

            if (unitMovementController.touchingGround) {
                if (unitMovementController.groundAngle <= unitMovementController.slopeLimit) {
                    if (unitMovementController.CurrentMovementData.HasMoveInput() || unitMovementController.CurrentMovementData.HasTurnInput()) {
                        //Debug.Log("PlayerUnitMovementController.Glide_StateUpdate() moving");
                        unitMovementController.ChangeState(CharacterMovementState.Move, isReplay);
                        return;
                    }
                    //Debug.Log("PlayerUnitMovementController.Glide_StateUpdate() idling");
                    unitMovementController.ChangeState(CharacterMovementState.Idle, isReplay);
                    return;
                }
            }

            if (unitController.CanGlide == false || unitController.IsEncumbered == true) {
                //Debug.Log("PlayerUnitMovementController.Glide_StateUpdate() falling");
                unitMovementController.ChangeState(CharacterMovementState.Fall, isReplay);
                return;
            }

            // 2. Velocity Calculations
            float clampValue = unitMovementController.MaxMovementSpeed;
            float calculatedSpeed = Mathf.Clamp(unitController.GlideSpeed, 0, clampValue);

            // 3. World-Space Direction (The Truth)
            // You should create a 'WorldNormalizedGlideMovement' similar to the Fly/Swim ones
            unitMovementController.intendedWorldMoveVelocity = unitMovementController.WorldNormalizedGlideMovement(calculatedSpeed) * calculatedSpeed;
            unitMovementController.adjustedWorldMoveVelocity = unitMovementController.intendedWorldMoveVelocity;

            // 4. Character Rotation (Face where we glide)
            if (unitMovementController.CurrentMovementData.RotateModelMode) {
                Vector3 horizontalDir = new Vector3(unitMovementController.intendedWorldMoveVelocity.x, 0, unitMovementController.intendedWorldMoveVelocity.z);
                if (horizontalDir.sqrMagnitude > 0.001f) {
                    unitController.UnitMotor.FaceDirection(horizontalDir);
                }
            }

            unitMovementController.CalculateTurnVelocity();

            // 5. Derive Local Velocity (Physics-Safe for Animator)
            Quaternion physicsRot = unitController.UnitMotor.MovementBody.GetRotation();
            unitMovementController.intendedLocalMoveVelocity = Quaternion.Inverse(physicsRot) * unitMovementController.intendedWorldMoveVelocity;

            // 6. Execute Physics
            unitMovementController.MoveWorld();

        }



    }

}