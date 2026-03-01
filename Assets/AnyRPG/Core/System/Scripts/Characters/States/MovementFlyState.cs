using UnityEngine;

namespace AnyRPG {
    public class MovementFlyState : IMovementState {

        private UnitController unitController;
        private UnitMovementController unitMovementController;

        public MovementFlyState(UnitMovementController unitMovementController, UnitController unitController) {
            this.unitController = unitController;
            this.unitMovementController = unitMovementController;
        }

        public void Enter(bool isReplay) {
            Debug.Log($"{unitController.gameObject.name}.MovementFlyState.Enter(isReplay: {isReplay})");

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


        public void Exit(bool isReplay) {
            Debug.Log($"{unitController.gameObject.name}.MovementFlyState.Exit(isReplay: {isReplay})");

            unitController.StopFlying(isReplay);
            unitController.RigidBody.useGravity = true;
            unitController.RigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            if (isReplay == false) {
                unitController.UnitAnimator.SetBool("Flying", false);
            }
        }

        public void Update(bool isReplay, double timeInterval) {
            //Debug.Log($"{unitController.gameObject.name}.MovementFlyState.Update()");

            unitMovementController.airForwardDirection = unitController.transform.forward;
            if (unitMovementController.touchingGround == true && unitMovementController.CurrentMovementData.InputFly == false) {
                if (unitMovementController.CurrentMovementData.HasMoveInput()) {
                    unitMovementController.ChangeState(CharacterMovementState.Move, isReplay);
                    return;
                } else {
                    unitMovementController.ChangeState(CharacterMovementState.Idle, isReplay);
                    return;
                }
            }
            if (unitController.InWater == true && unitMovementController.CheckForSwimming() == true) {
                unitMovementController.ChangeState(CharacterMovementState.Swim, isReplay);
                return;
            }
            if (unitController.CanFly == false) {
                unitMovementController.ChangeState(CharacterMovementState.Fall, isReplay);
                return;
            }


            if (unitMovementController.CurrentMovementData.HasFlyMoveInput() || unitMovementController.CurrentMovementData.HasTurnInput()) {

                // ============ RIGIDBODY CONSTRAINTS ============
                unitController.RigidBody.constraints = RigidbodyConstraints.FreezeRotation;

                // ============ VELOCITY CALCULATIONS ============

                // set clampValue to default of max movement speed
                float clampValue = unitMovementController.MaxMovementSpeed;

                // set a clamp value to limit movement speed to walking if going backward
                /*
                if (currentMoveVelocity.z < 0) {
                    clampValue = 1;
                }
                */

                // get current movement speed and clamp it to current clamp value
                float calculatedSpeed = unitController.FlySpeed;
                calculatedSpeed = Mathf.Clamp(calculatedSpeed, 0, clampValue);

                if (unitMovementController.CurrentMovementData.HasFlyMoveInput()) {
                    // multiply normalized movement by calculated speed to get actual movement
                    unitMovementController.localMoveVelocity = unitMovementController.NormalizedFlyMovement() * calculatedSpeed;
                    unitMovementController.adjustedlocalMoveVelocity = unitMovementController.localMoveVelocity;
                    //Debug.Log($"{unitController.gameObject.name}.PlayerUnitMovementController.Swim_StateUpdate() currentMoveVelocity: " + currentMoveVelocity);
                }
                unitMovementController.CalculateTurnVelocity();


                if (isReplay == false) {
                    // ============ ANIMATOR PARAMETERS ============
                    unitController.UnitAnimator.SetMoving(true);
                    unitController.UnitAnimator.SetTurnVelocity(unitMovementController.currentTurnVelocity.x);
                }

            } else {
                // ============ RIGIDBODY CONSTRAINTS ============
                // prevent constant drifting through air after stop moving
                unitController.FreezeAll();

                // ============ VELOCITY CALCULATIONS ============
                unitMovementController.localMoveVelocity = Vector3.zero;
                unitMovementController.adjustedlocalMoveVelocity = unitMovementController.localMoveVelocity;
                if (isReplay == false) {
                    // ============ ANIMATOR PARAMETERS ============
                    unitController.UnitAnimator.SetMoving(false);
                    unitController.UnitAnimator.SetTurnVelocity(0f);
                }

            }
            if (isReplay == false) {
                unitController.UnitAnimator.SetVelocity(unitMovementController.localMoveVelocity);
            }

            unitMovementController.MoveRelative();
        }
    }

}