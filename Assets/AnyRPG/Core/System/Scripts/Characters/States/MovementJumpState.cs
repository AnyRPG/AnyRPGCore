using UnityEngine;

namespace AnyRPG {
    public class MovementJumpState : IMovementState {

        private UnitController unitController;
        private UnitMovementController unitMovementController;

        public MovementJumpState(UnitMovementController unitMovementController, UnitController unitController) {
            this.unitController = unitController;
            this.unitMovementController = unitMovementController;
        }

        /*
        public void Enter(bool isReplay, bool isSilent) {
            //Debug.Log($"{unitController.gameObject.name}.MovementJumpState.Enter(isReplay: {isReplay}, isSilent: {isSilent}) tick:  {unitMovementController.CurrentMovementData.SimulatedTick} pVelocity: {unitController.UnitMotor.MovementBody.GetLinearVelocity()} transform.forward: {unitController.transform.forward} localmoveVelocity: {unitMovementController.localMoveVelocity}");

            //if (isSilent) return;

            if (isReplay == false) {
                unitMovementController.intendedLocalMoveVelocity.y = (Vector3.up * unitMovementController.jumpAcceleration).y;
            } else {
                // If we are replaying the tick where the jump started, 
                // we MUST re-apply the jump force because the Reconcile 
                // likely just set the Rigidbody velocity to 0.
                if (unitMovementController.CurrentMovementData.SimulatedTick == unitMovementController.lastJumpFrame) {
                    unitMovementController.intendedLocalMoveVelocity.y = unitMovementController.jumpAcceleration;
                } else {
                    // For subsequent replay ticks, we use the Rigidbody velocity 
                    // (which will now include gravity applied in previous replay steps)
                    //unitMovementController.localMoveVelocity = unitController.UnitMotor.MovementBody.GetLinearVelocity();
                    Vector3 worldVelocity = unitController.UnitMotor.MovementBody.GetLinearVelocity();
                    unitMovementController.intendedLocalMoveVelocity = unitController.transform.InverseTransformDirection(worldVelocity);
                }
            }
            unitMovementController.adjustedLocalMoveVelocity = unitMovementController.intendedLocalMoveVelocity;
            if (isReplay == false) {
                unitMovementController.lastJumpFrame = unitMovementController.CurrentMovementData.SimulatedTick;
                unitController.UnitAnimator.SetJumping(1);
                unitController.UnitAnimator.SetTrigger("JumpTrigger");
                unitController.UnitEventController.NotifyOnJump();
            }
            unitMovementController.MoveRelative();
        }
        */

        public void Enter(bool isReplay, bool isSilent) {
            Debug.Log($"{unitController.gameObject.name}.MovementJumpState.Enter(isReplay: {isReplay}, isSilent: {isSilent}) tick: {unitMovementController.CurrentMovementData.SimulatedTick} pVelocity: {unitController.UnitMotor.MovementBody.GetLinearVelocity()}");
            
            // 1. Determine the "Base" horizontal world velocity we are jumping WITH
            // If we were moving, intendedWorldMoveVelocity should already be set from the previous state's Update
            Vector3 currentWorldVelocity = unitMovementController.intendedWorldMoveVelocity;

            if (isReplay == false) {
                // Initial Jump: Apply the upward burst to the World Vector
                currentWorldVelocity.y = unitMovementController.jumpAcceleration;
                unitMovementController.lastJumpFrame = unitMovementController.CurrentMovementData.SimulatedTick;
            } else {
                // CSP Replay: If we are re-simulating the EXACT tick the jump happened
                if (unitMovementController.CurrentMovementData.SimulatedTick == unitMovementController.lastJumpFrame) {
                    currentWorldVelocity.y = unitMovementController.jumpAcceleration;
                } else {
                    // Replay Mid-Air: Use the Rigidbody's current velocity (which includes gravity from the replay)
                    currentWorldVelocity = unitController.UnitMotor.MovementBody.GetLinearVelocity();
                }
            }

            // 2. Set the stable World Velocity variables
            unitMovementController.intendedWorldMoveVelocity = currentWorldVelocity;
            unitMovementController.adjustedWorldMoveVelocity = currentWorldVelocity;

            // 3. Derive Local Velocity (Physics-Safe for Animator)
            // Using the Rigidbody rotation bypasses interpolation jitter
            Quaternion physicsRot = unitController.UnitMotor.MovementBody.GetRotation();
            unitMovementController.intendedLocalMoveVelocity = Quaternion.Inverse(physicsRot) * unitMovementController.intendedWorldMoveVelocity;
            unitMovementController.adjustedLocalMoveVelocity = unitMovementController.intendedLocalMoveVelocity;

            // 4. Visuals & Events
            if (!isReplay) {
                unitController.UnitAnimator.SetJumping(1);
                unitController.UnitAnimator.SetTrigger("JumpTrigger");
                unitController.UnitEventController.NotifyOnJump();
            }

            // 5. Execute Physics via MoveWorld
            unitMovementController.MoveWorld();
        }


        public void Exit(bool isReplay, bool isSilent) {
            Debug.Log($"{unitController.gameObject.name}.MovementJumpState.Exit(isReplay: {isReplay}) tick:  {unitMovementController.CurrentMovementData.SimulatedTick} pVelocity: {unitController.UnitMotor.MovementBody.GetLinearVelocity()}");
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