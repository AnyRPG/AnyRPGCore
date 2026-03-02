using UnityEngine;

namespace AnyRPG {
    public class MovementMoveState : IMovementState {

        private UnitController unitController;
        private UnitMovementController unitMovementController;

        public MovementMoveState(UnitMovementController unitMovementController, UnitController unitController) {
            this.unitController = unitController;
            this.unitMovementController = unitMovementController;
        }

        public void Enter(bool isReplay, bool isSilent) {
            Debug.Log($"{unitController.gameObject.name}.MovementMoveState.Enter(isReplay: {isReplay}) tick: {unitMovementController.CurrentMovementData.SimulatedTick}");

            unitMovementController.EnterGroundStateCommon(isReplay);
            unitMovementController.CalculateFallDamage(isReplay);
        }

        public void Exit(bool isReplay, bool isSilent) {
            Debug.Log($"{unitController.gameObject.name}.MovementMoveState.Exit(isReplay: {isReplay}) tick: {unitMovementController.CurrentMovementData.SimulatedTick}");

            if (isReplay == false) {
                unitController.UnitAnimator.SetMoving(false);
            }
        }

        public void Update(bool isReplay, double timeInterval) {
            //Debug.Log($"{unitController.gameObject.name}.MovementMoveState.Update()");
            //unitMovementController.airForwardDirection = unitController.transform.forward;

            float calculatedSpeed = 0f;


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

            // since we are in the move state, reset velocity to zero so we can pick up the new values
            // allow falling while moving by clamping existing y velocity
            unitMovementController.localMoveVelocity = new Vector3(0, Mathf.Clamp(unitController.RigidBody.linearVelocity.y, -53, 0), 0);
            unitMovementController.adjustedlocalMoveVelocity = unitMovementController.localMoveVelocity;

            // determine direction of travel in world space
            Vector3 directionOfTravel = unitController.transform.forward;

            if (unitMovementController.CurrentMovementData.HasMoveInput()) {

                // set clampValue to default of max movement speed
                float clampValue = unitMovementController.MaxMovementSpeed;

                // set a clamp value to limit movement speed to walking if going backward
                /*
                if (currentMoveVelocity.z < 0) {
                    clampValue = 1;
                }
                */

                // get current movement speed and clamp it to current clamp value
                calculatedSpeed = Mathf.Clamp(unitController.MovementSpeed, 0, clampValue);

                // multiply normalized movement by calculated speed to get actual local movement
                unitMovementController.localMoveVelocity = unitMovementController.CurrentMovementData.LocalInput * calculatedSpeed;

                if (unitMovementController.localMoveVelocity.x != 0 || unitMovementController.localMoveVelocity.z != 0) {
                    //if (controlsManager.GamePadModeActive == true) {
                    //directionOfTravel = CameraRelativeInput(new Vector3(localMoveVelocity.x, 0, localMoveVelocity.z)).normalized;
                    //Debug.Log("directionOfTravel: " + directionOfTravel);
                    //} else {
                    directionOfTravel = unitController.transform.TransformDirection(new Vector3(unitMovementController.localMoveVelocity.x, 0, unitMovementController.localMoveVelocity.z)).normalized;
                    //}
                }

                // determine if there is an obstacle in front, and if it is stairs
                unitMovementController.CheckFrontObstacle(calculatedSpeed, directionOfTravel, timeInterval);

            }

            //if (!MaintainingGround() || (groundAngle > slopeLimit && nearBottomFrontObstacle == true && nearTopFrontObstacle == true && touchingGround == false)) {
            //Debug.Log("groundAngle: " + groundAngle + "; closestWalkablegrounddistance: " + closestWalkableGroundDistance + "; nearLowObstacle: " + nearLowObstacle + "; nearBottomFrontObstacle: " + nearBottomFrontObstacle + "; touchingGround: " + touchingGround);
            //if (!MaintainingGround() || (groundAngle > slopeLimit && touchingGround == false && nearLowObstacle == false)) {
            if (
                !unitMovementController.MaintainingGround() ||
                //(groundAngle > slopeLimit && touchingGround == false && (nearLowObstacle == false || (nearLowObstacle == true && closestWalkableGroundDistance < -stepHeight))) // closestGroundDistance check for running off low obstacle
                (unitMovementController.groundAngle > unitMovementController.slopeLimit && unitMovementController.nearBottomFrontObstacle == true && unitMovementController.nearLowObstacle == false) ||
                (unitMovementController.groundAngle > unitMovementController.slopeLimit && unitMovementController.nearBottomFrontObstacle == false && unitMovementController.nearLowObstacle == false && unitMovementController.closestWalkableGroundDistance < -unitMovementController.stepHeight)
                ) { // closetoGround check for running backward off low obstacle
                //Debug.Log("groundAngle: " + groundAngle + "; closestWalkablegrounddistance: " + closestWalkableGroundDistance + "; nearLowObstacle: " + nearLowObstacle + "; nearBottomFrontObstacle: " + nearBottomFrontObstacle + "; touchingGround: " + touchingGround);
                //Debug.Break();
                if (unitController.CanFly) {
                    unitMovementController.ChangeState(CharacterMovementState.Fly, isReplay);
                    return;
                } else {
                    if (unitController.CanGlide) {
                        unitMovementController.ChangeState(CharacterMovementState.Glide, isReplay);
                        return;
                    }
                    //if (touchingGround == false) {
                    unitMovementController.ChangeState(CharacterMovementState.Fall, isReplay);
                    return;
                    //}
                }
            }

            if (unitMovementController.CurrentMovementData.HasMoveInput() || unitMovementController.CurrentMovementData.HasTurnInput()) {

                if (unitMovementController.CurrentMovementData.HasMoveInput()) {
                    unitMovementController.adjustedlocalMoveVelocity = unitMovementController.NormalizedLocalMovement(calculatedSpeed, directionOfTravel, timeInterval) * calculatedSpeed;
                }
                unitMovementController.CalculateTurnVelocity();
            } else {
                unitMovementController.currentTurnVelocity = Vector3.zero;
                unitMovementController.ChangeState(CharacterMovementState.Idle, isReplay);
                //rpgCharacterState = CharacterMovementState.Idle;
                return;
            }

            unitMovementController.MoveRelative();
            if (isReplay == false) {
                unitMovementController.AnimatorMoveUpdate();
            }
        }
    }

}