using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class MovementSwimState : IMovementState {

        private UnitController unitController;
        private UnitMovementController unitMovementController;

        public MovementSwimState(UnitMovementController unitMovementController, UnitController unitController) {
            this.unitController = unitController;
            this.unitMovementController = unitMovementController;
        }

        public void Enter(bool isReplay, bool isSilent) {
            Debug.Log($"{unitController.gameObject.name}.MovementSwimState.Enter(isReplay: {isReplay}) tick:  {unitMovementController.CurrentMovementData.SimulatedTick}");

            unitMovementController.currentFallDistance = 0f;
            unitMovementController.EnterGroundStateCommon(isReplay);
            unitController.StartSwimming(isReplay);
            unitController.RigidBody.useGravity = false;
            if (isReplay == false) {
                unitController.UnitAnimator.SetTrigger("SwimTrigger");
                unitController.UnitAnimator.SetBool("Swimming", true);
            }
        }

        public void Exit(bool isReplay, bool isSilent) {
            Debug.Log($"{unitController.gameObject.name}.MovementSwimState.Exit(isReplay: {isReplay}) tick: {unitMovementController.CurrentMovementData.SimulatedTick}");

            unitController.StopSwimming();
            unitController.RigidBody.useGravity = true;
            unitController.RigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            if (isReplay == false) {
                unitController.UnitAnimator.SetBool("Swimming", false);
            }
        }

        /*
        public void Update(bool isReplay, double timeInterval) {
            //Debug.Log($"{unitController.gameObject.name}.MovementSwimState.Update()");

            if (unitController.InWater == true) {
                if (unitController.CanFly
                    && unitMovementController.CurrentMovementData.InputFly
                    && unitMovementController.CheckForSwimming() == false) {
                    unitMovementController.ChangeState(CharacterMovementState.Fly, isReplay);
                    return;
                }
                if (unitMovementController.CheckForSwimming() == false) {
                    unitMovementController.ChangeState(CharacterMovementState.Move, isReplay);
                    return;
                }

            } else {
                unitMovementController.ChangeState(CharacterMovementState.Move, isReplay);
                return;
            }

            if (unitMovementController.CurrentMovementData.HasWaterMoveInput() || unitMovementController.CurrentMovementData.HasTurnInput()) {

                // ============ RIGIDBODY CONSTRAINTS ============
                unitController.RigidBody.constraints = RigidbodyConstraints.FreezeRotation;

                // ============ VELOCITY CALCULATIONS ============

                // set clampValue to default of max movement speed
                float clampValue = unitMovementController.MaxMovementSpeed;

                // set a clamp value to limit movement speed to walking if going backward
                
                //if (currentMoveVelocity.z < 0) {
                //    clampValue = 1;
                //}

                // get current movement speed and clamp it to current clamp value
                float calculatedSpeed = unitController.SwimSpeed;
                calculatedSpeed = Mathf.Clamp(calculatedSpeed, 0, clampValue);

                if (unitMovementController.CurrentMovementData.HasWaterMoveInput()) {
                    // multiply normalized movement by calculated speed to get actual movement
                    unitMovementController.intendedLocalMoveVelocity = unitMovementController.LocalNormalizedSwimMovement(timeInterval) * calculatedSpeed;
                    unitMovementController.adjustedlocalMoveVelocity = unitMovementController.intendedLocalMoveVelocity;
                    //Debug.Log($"{unitController.gameObject.name}.PlayerUnitMovementController.Swim_StateUpdate() currentMoveVelocity: " + currentMoveVelocity);
                }
                unitMovementController.CalculateTurnVelocity();

                if (isReplay == false ) {
                    // ============ ANIMATOR PARAMETERS ============
                    unitController.UnitAnimator.SetMoving(true);
                    unitController.UnitAnimator.SetTurnVelocity(unitMovementController.currentTurnVelocity.x);
                }
            } else {
                // ============ RIGIDBODY CONSTRAINTS ============
                // prevent constant drifting through water after stop moving
                unitController.FreezeAll();

                // ============ VELOCITY CALCULATIONS ============
                unitMovementController.intendedLocalMoveVelocity = Vector3.zero;
                unitMovementController.adjustedlocalMoveVelocity = unitMovementController.intendedLocalMoveVelocity;
                if (isReplay == false) {
                    // ============ ANIMATOR PARAMETERS ============
                    unitController.UnitAnimator.SetMoving(false);
                    unitController.UnitAnimator.SetTurnVelocity(0f);
                }

            }
            if (isReplay == false) {
                unitController.UnitAnimator.SetVelocity(unitMovementController.intendedLocalMoveVelocity);
            }

            unitMovementController.MoveRelative();
        }
        */

        public void Update(bool isReplay, double timeInterval) {
            //Debug.Log($"{unitController.gameObject.name}.MovementSwimState.Update()");

            if (unitController.InWater == true) {
                if (unitController.CanFly
                    && unitMovementController.CurrentMovementData.InputFly
                    && unitMovementController.CheckForSwimming() == false) {
                    unitMovementController.ChangeState(CharacterMovementState.Fly, isReplay);
                    return;
                }
                if (unitMovementController.CheckForSwimming() == false) {
                    unitMovementController.ChangeState(CharacterMovementState.Move, isReplay);
                    return;
                }

            } else {
                unitMovementController.ChangeState(CharacterMovementState.Move, isReplay);
                return;
            }

            if (unitMovementController.CurrentMovementData.HasWaterMoveInput() || unitMovementController.CurrentMovementData.HasTurnInput()) {
                unitController.RigidBody.constraints = RigidbodyConstraints.FreezeRotation;

                float calculatedSpeed = Mathf.Clamp(unitController.SwimSpeed, 0, unitMovementController.MaxMovementSpeed);

                if (unitMovementController.CurrentMovementData.HasWaterMoveInput()) {
                    // 1. Calculate the World-Space "Truth"
                    unitMovementController.intendedWorldMoveVelocity = unitMovementController.WorldNormalizedSwimMovement(timeInterval) * calculatedSpeed;
                    unitMovementController.adjustedWorldMoveVelocity = unitMovementController.intendedWorldMoveVelocity;

                    Vector3 horizontalDir = new Vector3(unitMovementController.intendedWorldMoveVelocity.x, 0, unitMovementController.intendedWorldMoveVelocity.z);

                    // 2. Handle Character Rotation via UnitMotor
                    if (unitController.UnitProfile.UnitPrefabProps.RotateModel || unitMovementController.CurrentMovementData.GamepadModeActive) {
                        // Face the direction of world travel
                        if (horizontalDir.sqrMagnitude > 0.001f) {
                            unitController.UnitMotor.FaceDirection(horizontalDir);
                        }
                    }

                    // 3. Derive Local Velocity ONLY for Animator blend trees
                    // This is now safe because FaceDirection updated the Rigidbody rotation for this tick
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
                unitController.FreezeAll();
                unitMovementController.intendedWorldMoveVelocity = Vector3.zero;
                unitMovementController.adjustedWorldMoveVelocity = Vector3.zero;
                unitMovementController.intendedLocalMoveVelocity = Vector3.zero;
                unitMovementController.adjustedLocalMoveVelocity = Vector3.zero;
                if (isReplay == false) {
                    // ============ ANIMATOR PARAMETERS ============
                    unitController.UnitAnimator.SetMoving(false);
                    unitController.UnitAnimator.SetTurnVelocity(0f);
                }
            }

            // 4. Apply Physics Movement
            // Make sure MoveRelative is updated to pass adjustedWorldMoveVelocity to the motor!
            unitMovementController.MoveWorld();

            if (!isReplay) {
                unitController.UnitAnimator.SetVelocityFromLocal(unitMovementController.intendedLocalMoveVelocity);
            }
        }


    }

}