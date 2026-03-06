using UnityEngine;

namespace AnyRPG {
    public class MovementJumpState : IMovementState {

        private UnitController unitController;
        private UnitMovementController unitMovementController;

        public MovementJumpState(UnitMovementController unitMovementController, UnitController unitController) {
            this.unitController = unitController;
            this.unitMovementController = unitMovementController;
        }

        public void Enter(bool isReplay, bool isSilent) {
            //Debug.Log($"{unitController.gameObject.name}.MovementJumpState.Enter(isReplay: {isReplay}, isSilent: {isSilent}) tick:  {unitMovementController.CurrentMovementData.SimulatedTick} pVelocity: {unitController.UnitMotor.MovementBody.GetLinearVelocity()} transform.forward: {unitController.transform.forward} localmoveVelocity: {unitMovementController.localMoveVelocity}");

            //if (isSilent) return;

            if (isReplay == false) {
                unitMovementController.localMoveVelocity.y = (Vector3.up * unitMovementController.jumpAcceleration).y;
            } else {
                // If we are replaying the tick where the jump started, 
                // we MUST re-apply the jump force because the Reconcile 
                // likely just set the Rigidbody velocity to 0.
                if (unitMovementController.CurrentMovementData.SimulatedTick == unitMovementController.lastJumpFrame) {
                    unitMovementController.localMoveVelocity.y = unitMovementController.jumpAcceleration;
                } else {
                    // For subsequent replay ticks, we use the Rigidbody velocity 
                    // (which will now include gravity applied in previous replay steps)
                    //unitMovementController.localMoveVelocity = unitController.UnitMotor.MovementBody.GetLinearVelocity();
                    Vector3 worldVelocity = unitController.UnitMotor.MovementBody.GetLinearVelocity();
                    unitMovementController.localMoveVelocity = unitController.transform.InverseTransformDirection(worldVelocity);
                }
            }
            unitMovementController.adjustedlocalMoveVelocity = unitMovementController.localMoveVelocity;
            if (isReplay == false) {
                unitMovementController.lastJumpFrame = unitMovementController.CurrentMovementData.SimulatedTick;
                unitController.UnitAnimator.SetJumping(1);
                unitController.UnitAnimator.SetTrigger("JumpTrigger");
                unitController.UnitEventController.NotifyOnJump();
            }
            unitMovementController.MoveRelative();
        }

        public void Exit(bool isReplay, bool isSilent) {
            //Debug.Log($"{unitController.gameObject.name}.MovementJumpState.Exit(isReplay: {isReplay}) tick:  {unitMovementController.CurrentMovementData.SimulatedTick} pVelocity: {unitController.UnitMotor.MovementBody.GetLinearVelocity()}");
        }

        public void Update(bool isReplay, double timeInterval) {
            //Debug.Log($"{unitController.gameObject.name}.MovementJumpState.Update(isReplay: {isReplay}) tick: {unitMovementController.CurrentMovementData.SimulatedTick} pVelocity: {unitController.UnitMotor.MovementBody.GetLinearVelocity()} position: {unitController.RigidBody.position} transform.forward: {unitController.transform.forward}");

            if (unitController.InWater == true) {
                if (unitMovementController.CheckForSwimming() == true) {
                    unitMovementController.ChangeState(CharacterMovementState.Swim, isReplay);
                    return;
                }
            }

            if (unitController.CanFly
                && unitMovementController.CurrentMovementData.InputFly) {
                unitMovementController.ChangeState(CharacterMovementState.Fly, isReplay);
                return;
            }

            //Debug.Log($"{unitController.gameObject.name}.MovementJumpState.Update() tick: {unitMovementController.CurrentMovementData.SimulatedTick}, frame: {unitMovementController.lastJumpFrame}");
            if (unitController.UnitMotor.MovementBody.GetLinearVelocity().y <= 0f && unitMovementController.CurrentMovementData.SimulatedTick > (unitMovementController.lastJumpFrame + 2)) {
                if (unitController.CanGlide) {
                    unitMovementController.ChangeState(CharacterMovementState.Glide, isReplay);
                    return;
                }
                unitMovementController.ChangeState(CharacterMovementState.Fall, isReplay);
                return;
            }
        }
    }

}