using UnityEngine;

namespace AnyRPG {
    public class MovementGlideState : IMovementState {

        private UnitController unitController;
        private UnitMovementController unitMovementController;

        public MovementGlideState(UnitMovementController unitMovementController, UnitController unitController) {
            this.unitController = unitController;
            this.unitMovementController = unitMovementController;
        }

        public void Enter(bool isReplay) {
            Debug.Log($"{unitController.gameObject.name}.MovementGlideState.Enter(isReplay: {isReplay}) tick: {unitMovementController.CurrentMovementData.SimulatedTick}");

            // 1. PERSISTENT PHYSICS & DATA (Always run these during replays)
            // This ensures the server and client agree on the starting physics state
            unitMovementController.currentFallDistance = 0f;
            unitMovementController.canJump = false;

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


        public void Exit(bool isReplay) {
            Debug.Log($"{unitController.gameObject.name}.MovementGlideState.Exit() tick: {unitMovementController.CurrentMovementData.SimulatedTick}");

            unitController.RigidBody.useGravity = true;
            if (isReplay == false) {
                unitController.UnitAnimator.SetJumping(0);
            }
        }

        public void Update(bool isReplay, double timeInterval) {
            //Debug.Log($"{unitController.gameObject.name}.MovementGlideState.Update()");
            if (unitController.InWater == true) {
                if (unitMovementController.CheckForSwimming() == true) {
                    //Debug.Log("PlayerUnitMovementController.Glide_StateUpdate() swimming");
                    unitMovementController.ChangeState(CharacterMovementState.Swim, isReplay);
                    return;
                }
            }

            if (unitController.CanFly
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

            if (unitController.CanGlide == false) {
                //Debug.Log("PlayerUnitMovementController.Glide_StateUpdate() falling");
                unitMovementController.ChangeState(CharacterMovementState.Fall, isReplay);
                return;
            }

            // ============ VELOCITY CALCULATIONS ============

            // set clampValue to default of max movement speed
            float clampValue = unitMovementController.MaxMovementSpeed;

            // get current movement speed and clamp it to current clamp value
            float calculatedSpeed = unitController.GlideSpeed;
            calculatedSpeed = Mathf.Clamp(calculatedSpeed, 0, clampValue);

            // multiply normalized movement by calculated speed to get actual movement
            unitMovementController.localMoveVelocity = unitMovementController.NormalizedGlideMovement(calculatedSpeed) * calculatedSpeed;
            unitMovementController.adjustedlocalMoveVelocity = unitMovementController.localMoveVelocity;
            //Debug.Log($"{unitController.gameObject.name}.PlayerUnitMovementController.Swim_StateUpdate() currentMoveVelocity: " + currentMoveVelocity);

            unitMovementController.CalculateTurnVelocity();

            unitMovementController.MoveRelative();
        }
    }

}