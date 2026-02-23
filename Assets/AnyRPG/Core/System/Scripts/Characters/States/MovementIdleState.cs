using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class MovementIdleState : IMovementState {

        private UnitController unitController;
        private UnitMovementController unitMovementController;

        public MovementIdleState(UnitMovementController unitMovementController, UnitController unitController) {
            this.unitController = unitController;
            this.unitMovementController = unitMovementController;
        }

        public void Enter() {
            //Debug.Log($"{unitController.gameObject.name}.MovementIdleState.Enter()");
            // allow the character to fall until they reach the ground
            unitController.FreezePositionXZ();

            // reset velocity from any falling movement that was happening
            unitMovementController.localMoveVelocity = Vector3.zero;
            unitMovementController.EnterGroundStateCommon();

            unitController.UnitAnimator.SetMoving(false);
            unitController.UnitAnimator.SetStrafing(false);
            unitController.UnitAnimator.SetTurnVelocity(0f);

            unitController.UnitAnimator.SetVelocity(unitMovementController.localMoveVelocity);

            unitController.UnitMotor?.Move(new Vector3(0, Mathf.Clamp(unitController.RigidBody.linearVelocity.y, -53, 0), 0));
            unitMovementController.CalculateFallDamage();

        }

        public void Exit() {
            //Debug.Log($"{unitController.gameObject.name}.MovementIdleState.Exit()");
            unitController.RigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        public void Update() {
            //Debug.Log($"{unitController.gameObject.name}.MovementIdleState.Update()");
            if (unitController.InWater == true) {
                if (unitMovementController.CheckForSwimming() == true) {
                    unitMovementController.ChangeState(CharacterMovementState.Swim);
                    return;
                }
            }

            if (unitMovementController.CurrentMovementData.inputJump) {
                unitMovementController.ChangeState(CharacterMovementState.Jump);
                return;
            }

            if (unitController.CanFly && unitMovementController.CurrentMovementData.inputFly) {
                unitMovementController.ChangeState(CharacterMovementState.Jump);
                return;
            }

            if (!unitMovementController.MaintainingGround() && unitMovementController.groundAngle > unitMovementController.slopeLimit) {
                unitMovementController.ChangeState(CharacterMovementState.Fall);
                return;
            }
            if (unitMovementController.CurrentMovementData.HasMoveInput() || unitMovementController.CurrentMovementData.HasTurnInput()) {
                //Debug.Log($"{unitController.gameObject.name}.PlayerUnitMovementController.Idle_StateUpdate(): entering move state");
                unitMovementController.ChangeState(CharacterMovementState.Move);
                return;
            }
        }
    }

}