using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class MovementFlyState : IMovementState {

        private UnitController unitController;
        private UnitMovementController unitMovementController;

        public MovementFlyState(UnitMovementController unitMovementController, UnitController unitController) {
            this.unitController = unitController;
            this.unitMovementController = unitMovementController;
        }

        public void Enter() {
            //Debug.Log($"{unitController.gameObject.name}.MovementFlyState.Enter()");
            unitMovementController.currentFallDistance = 0f;
            unitController.StartFlying();
            unitController.RigidBody.useGravity = false;
            unitController.UnitAnimator.SetBool("Flying", true);
            unitController.UnitAnimator.SetTrigger("FlyTrigger");
        }

        public void Exit() {
            //Debug.Log($"{unitController.gameObject.name}.MovementFlyState.Exit()");
            unitController.StopFlying();
            unitController.RigidBody.useGravity = true;
            unitController.UnitAnimator.SetBool("Flying", false);
            unitController.RigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        public void Update() {
            //Debug.Log($"{unitController.gameObject.name}.MovementFlyState.Update()");
            unitMovementController.airForwardDirection = unitController.transform.forward;

            if (unitMovementController.touchingGround == true && unitMovementController.CurrentMovementData.inputFly == false) {
                if (unitMovementController.CurrentMovementData.HasMoveInput()) {
                    unitMovementController.ChangeState(CharacterMovementState.Move);
                    return;
                } else {
                    unitMovementController.ChangeState(CharacterMovementState.Idle);
                    return;
                }
            }
            if (unitController.InWater == true && unitMovementController.CheckForSwimming() == true) {
                unitMovementController.ChangeState(CharacterMovementState.Swim);
                return;
            }
            if (unitController.CanFly == false) {
                unitMovementController.ChangeState(CharacterMovementState.Fall);
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


                // ============ ANIMATOR PARAMETERS ============
                unitController.UnitAnimator.SetMoving(true);
                unitController.UnitAnimator.SetTurnVelocity(unitMovementController.currentTurnVelocity.x);

            } else {
                // ============ RIGIDBODY CONSTRAINTS ============
                // prevent constant drifting through air after stop moving
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