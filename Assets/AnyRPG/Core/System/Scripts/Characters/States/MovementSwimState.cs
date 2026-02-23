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

        public void Enter() {
            //Debug.Log($"{unitController.gameObject.name}.MovementSwimState.Enter()");
            unitMovementController.currentFallDistance = 0f;
            unitMovementController.EnterGroundStateCommon();
            unitController.StartSwimming();
            unitController.RigidBody.useGravity = false;
            unitController.UnitAnimator.SetTrigger("SwimTrigger");
            unitController.UnitAnimator.SetBool("Swimming", true);
        }

        public void Exit() {
            //Debug.Log($"{unitController.gameObject.name}.MovementSwimState.Exit()");
            unitController.StopSwimming();
            unitController.RigidBody.useGravity = true;
            unitController.UnitAnimator.SetBool("Swimming", false);
        }

        public void Update() {
            //Debug.Log($"{unitController.gameObject.name}.MovementSwimState.Update()");
            unitMovementController.airForwardDirection = unitController.transform.forward;

            if (unitController.InWater == true) {
                if (unitController.CanFly
                    && unitMovementController.CurrentMovementData.inputFly
                    && unitMovementController.CheckForSwimming() == false) {
                    unitMovementController.ChangeState(CharacterMovementState.Fly);
                    return;
                }
                if (unitMovementController.CheckForSwimming() == false) {
                    unitMovementController.ChangeState(CharacterMovementState.Move);
                    return;
                }

            } else {
                unitMovementController.ChangeState(CharacterMovementState.Move);
                return;
            }

            if (unitMovementController.CurrentMovementData.HasWaterMoveInput() || unitMovementController.CurrentMovementData.HasTurnInput()) {

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
                float calculatedSpeed = unitController.SwimSpeed;
                calculatedSpeed = Mathf.Clamp(calculatedSpeed, 0, clampValue);

                if (unitMovementController.CurrentMovementData.HasWaterMoveInput()) {
                    // multiply normalized movement by calculated speed to get actual movement
                    unitMovementController.localMoveVelocity = unitMovementController.NormalizedSwimMovement() * calculatedSpeed;
                    unitMovementController.adjustedlocalMoveVelocity = unitMovementController.localMoveVelocity;
                    //Debug.Log($"{unitController.gameObject.name}.PlayerUnitMovementController.Swim_StateUpdate() currentMoveVelocity: " + currentMoveVelocity);
                }
                unitMovementController.CalculateTurnVelocity();


                // ============ ANIMATOR PARAMETERS ============
                unitController.UnitAnimator.SetMoving(true);
                unitController.UnitAnimator.SetTurnVelocity(unitMovementController.currentTurnVelocity.x);

            } else {
                // ============ RIGIDBODY CONSTRAINTS ============
                // prevent constant drifting through water after stop moving
                unitController.FreezeAll();

                // ============ VELOCITY CALCULATIONS ============
                unitMovementController.localMoveVelocity = Vector3.zero;
                unitMovementController.adjustedlocalMoveVelocity = unitMovementController.localMoveVelocity;

                // ============ ANIMATOR PARAMETERS ============
                unitController.UnitAnimator.SetMoving(false);
                unitController.UnitAnimator.SetTurnVelocity(0f);

            }
            unitController.UnitAnimator.SetVelocity(unitMovementController.localMoveVelocity);

            unitMovementController.MoveRelative();
        }

    }

}