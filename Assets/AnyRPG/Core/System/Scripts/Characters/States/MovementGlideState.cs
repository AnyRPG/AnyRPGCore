using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class MovementGlideState : IMovementState {

        private UnitController unitController;
        private UnitMovementController unitMovementController;

        public MovementGlideState(UnitMovementController unitMovementController, UnitController unitController) {
            this.unitController = unitController;
            this.unitMovementController = unitMovementController;
        }

        public void Enter() {
            //Debug.Log($"{unitController.gameObject.name}.MovementGlideState.Enter()");
            unitMovementController.currentFallDistance = 0f;
            unitMovementController.canJump = false;
            if (unitController.UnitAnimator.GetInt("Jumping") != 2) {
                unitController.UnitAnimator.SetTrigger("FallTrigger");
                unitController.UnitAnimator.SetJumping(2);
            }
            unitController.RigidBody.useGravity = false;
            unitController.UnitAnimator.SetTurnVelocity(0f);
            unitController.RigidBody.constraints = RigidbodyConstraints.FreezeRotation;

            // clamp y velocity to prevent launching off ramps
            unitController.UnitMotor?.Move(new Vector3(unitController.RigidBody.linearVelocity.x, Mathf.Clamp(unitController.RigidBody.linearVelocity.y, -53, 0), unitController.RigidBody.linearVelocity.z));
        }

        public void Exit() {
            //Debug.Log($"{unitController.gameObject.name}.MovementGlideState.Exit()");
            unitController.UnitAnimator.SetJumping(0);
            unitController.RigidBody.useGravity = true;
        }

        public void Update() {
            //Debug.Log($"{unitController.gameObject.name}.MovementGlideState.Update()");
            if (unitController.InWater == true) {
                if (unitMovementController.CheckForSwimming() == true) {
                    //Debug.Log("PlayerUnitMovementController.Glide_StateUpdate() swimming");
                    unitMovementController.ChangeState(CharacterMovementState.Swim);
                    return;
                }
            }

            if (unitController.CanFly
                && unitMovementController.CurrentMovementData.inputFly) {
                //Debug.Log("PlayerUnitMovementController.Glide_StateUpdate() flying");
                unitMovementController.ChangeState(CharacterMovementState.Fly);
                return;
            }

            if (unitMovementController.touchingGround) {
                if (unitMovementController.groundAngle <= unitMovementController.slopeLimit) {
                    if (unitMovementController.CurrentMovementData.HasMoveInput() || unitMovementController.CurrentMovementData.HasTurnInput()) {
                        //Debug.Log("PlayerUnitMovementController.Glide_StateUpdate() moving");
                        unitMovementController.ChangeState(CharacterMovementState.Move);
                        return;
                    }
                    //Debug.Log("PlayerUnitMovementController.Glide_StateUpdate() idling");
                    unitMovementController.ChangeState(CharacterMovementState.Idle);
                    return;
                }
            }

            if (unitController.CanGlide == false) {
                //Debug.Log("PlayerUnitMovementController.Glide_StateUpdate() falling");
                unitMovementController.ChangeState(CharacterMovementState.Fall);
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