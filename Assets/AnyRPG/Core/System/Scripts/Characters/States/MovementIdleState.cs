using UnityEngine;

namespace AnyRPG {
    public class MovementIdleState : IMovementState {

        private UnitController unitController;
        private UnitMovementController unitMovementController;

        public MovementIdleState(UnitMovementController unitMovementController, UnitController unitController) {
            this.unitController = unitController;
            this.unitMovementController = unitMovementController;
        }

        public void Enter(bool isReplay, bool isSilent) {
            //Debug.Log($"{unitController.gameObject.name}.MovementIdleState.Enter(isReplay: {isReplay}, isSilent: {isSilent}) frame: {Time.frameCount} tick: {(isSilent ? "N/A" : unitMovementController.CurrentMovementData.SimulatedTick)} rposition: {unitController.UnitMotor.MovementBody.GetPosition()} mposition: {unitController.UnitModelController.UnitModel.transform.position} velocity: {unitController.UnitMotor.MovementBody.GetLinearVelocity()}");

            // 1. PERSISTENT PHYSICS & STATE (Always run during replays)
            // We must freeze position and reset velocity every replay to ensure 
            // the character doesn't "drift" during the 7-frame re-simulation.
            unitController.FreezePositionXZ();

            // Reset local move velocity for deterministic simulation
            unitMovementController.intendedLocalMoveVelocity = Vector3.zero;
            unitMovementController.intendedWorldMoveVelocity = Vector3.zero;

            if (isSilent) return;

            // Ensure Grounded flags are set correctly for every replayed tick
            unitMovementController.EnterGroundStateCommon(isReplay);

            // Apply the stop to the motor (Physics simulation)
            // We keep the Y-clamp to ensure we don't "bounce" off the floor during replay
            unitController.UnitMotor?.Move(new Vector3(0, Mathf.Clamp(unitController.RigidBody.linearVelocity.y, -53, 0), 0));

            // 2. VISUALS & ONE-SHOT TRIGGERS (Guard with !isReplay)
            if (isReplay == false) {
                unitController.UnitAnimator.SetMoving(false);
                unitController.UnitAnimator.SetStrafing(false);
                unitController.UnitAnimator.SetTurnVelocity(0f);
                unitController.UnitAnimator.SetVelocityFromLocal(unitMovementController.intendedLocalMoveVelocity);
            }
            unitMovementController.CalculateFallDamage(isReplay);
        }


        public void Exit(bool isReplay, bool isSilent) {
            //Debug.Log($"{unitController.gameObject.name}.MovementIdleState.Exit(isReplay: {isReplay}, isSilent: {isSilent}) frame: {Time.frameCount} tick: {(isSilent ? "N/A" : unitMovementController.CurrentMovementData.SimulatedTick)} rposition: {unitController.UnitMotor.MovementBody.GetPosition()} mposition: {unitController.UnitModelController.UnitModel.transform.position} velocity: {unitController.UnitMotor.MovementBody.GetLinearVelocity()}");

            unitController.RigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        public void Update(bool isReplay, double timeInterval) {
            //Debug.Log($"{unitController.gameObject.name}.MovementIdleState.Update()");

            if (unitController.InWater == true) {
                if (unitMovementController.CheckForSwimming() == true) {
                    unitMovementController.ChangeState(CharacterMovementState.Swim, isReplay);
                    return;
                }
            }

            if (unitMovementController.CurrentMovementData.InputJump) {
                unitMovementController.ChangeState(CharacterMovementState.Jump, isReplay);
                return;
            }

            if (unitController.CanFly && unitMovementController.CurrentMovementData.InputFly) {
                unitMovementController.ChangeState(CharacterMovementState.Jump, isReplay);
                return;
            }

            // this condition was causing the character to stay in idle state even when dismounted high in the air, commenting out to see
            // if it was actually needed for anything. it was supposed to cause a slide if the character was on steep slopes originally.
            //if (!unitMovementController.MaintainingGround() && unitMovementController.groundAngle > unitMovementController.slopeLimit) {
            if (!unitMovementController.MaintainingGround()) {
                unitMovementController.ChangeState(CharacterMovementState.Fall, isReplay);
                return;
            }
            if (unitMovementController.CurrentMovementData.HasMoveInput() || unitMovementController.CurrentMovementData.HasTurnInput()) {
                //Debug.Log($"{unitController.gameObject.name}.PlayerUnitMovementController.Idle_StateUpdate(): entering move state");
                unitMovementController.ChangeState(CharacterMovementState.Move, isReplay);
                return;
            }
        }
    }

}