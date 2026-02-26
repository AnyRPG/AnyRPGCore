using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class MovementKnockbackState : IMovementState {

        private UnitController unitController;
        private UnitMovementController unitMovementController;

        public MovementKnockbackState(UnitMovementController unitMovementController, UnitController unitController) {
            this.unitController = unitController;
            this.unitMovementController = unitMovementController;
        }

        public void Enter(bool isReplay) {
            //Debug.Log($"{unitController.gameObject.name}.MovementKnockbackState.Enter()");

            unitController.RigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            unitMovementController.canJump = false;
            unitMovementController.lastKnockbackFrame = unitMovementController.CurrentMovementData.SimulatedTick;

            if (isReplay == false) {
                unitController.UnitAnimator.SetJumping(1);
                unitController.UnitAnimator.SetTrigger("JumpTrigger");
            }
        }

        public void Exit(bool isReplay) {
            //Debug.Log($"{unitController.gameObject.name}.MovementKnockbackState.Exit()");
        }

        public void Update(bool isReplay) {
            //Debug.Log($"{unitController.gameObject.name}.MovementKnockbackState.Update()");
            if (unitMovementController.CurrentMovementData.SimulatedTick <= (unitMovementController.lastKnockbackFrame + 2)) {
                // rigidbody velocity does not immediately update, so a small delay must be added before checking
                // if a different state should be entered
                return;
            }

            if (unitController.InWater == true) {
                if (unitMovementController.CheckForSwimming() == true) {
                    unitMovementController.ChangeState(CharacterMovementState.Swim, isReplay);
                    return;
                }
            }

            if (unitMovementController.touchingGround && unitController.RigidBody.linearVelocity.y < 0.1) {
                if (unitMovementController.CurrentMovementData.HasMoveInput() || unitMovementController.CurrentMovementData.HasTurnInput()) {
                    // new code to allow not freezing up when landing - fix, should be fall or somehow prevent from getting into move during takeoff
                    unitMovementController.ChangeState(CharacterMovementState.Move, isReplay);
                    return;
                }
                //Debug.Log($"PlayerUnitMovementController.Knockback_StateUpdate() entering Idle state with y velocity: {unitController.RigidBody.velocity.y} on frame {Time.frameCount}");
                unitMovementController.ChangeState(CharacterMovementState.Idle, isReplay);
                return;
            }
        }
    }

}