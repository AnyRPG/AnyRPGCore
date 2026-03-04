using UnityEngine;

namespace AnyRPG {
    public class MovementNavMeshState : ConfiguredClass, IMovementState {

        private UnitController unitController;
        private UnitMovementController unitMovementController;

        public MovementNavMeshState(UnitMovementController unitMovementController, UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            this.unitMovementController = unitMovementController;
            Configure(systemGameManager);
        }

        public void Enter(bool isReplay, bool isSilent) {
            Debug.Log($"{unitController.gameObject.name}.MovementNavMeshState.Enter(isReplay: {isReplay}) tick: {unitMovementController.CurrentMovementData.SimulatedTick}");

        }

        public void Exit(bool isReplay, bool isSilent) {
            Debug.Log($"{unitController.gameObject.name}.MovementNavMeshState.Exit(isReplay: {isReplay}) tick: {unitMovementController.CurrentMovementData.SimulatedTick}");
        }

        public void Update(bool isReplay, double timeInterval) {
            //Debug.Log($"{unitController.gameObject.name}.MovementNavMeshState.Update()");

            if (unitMovementController.CurrentMovementData.InputJump) {
                unitMovementController.ChangeState(CharacterMovementState.Jump, isReplay);
                return;
            }

            if (unitController.CanFly && unitMovementController.CurrentMovementData.InputFly) {
                unitMovementController.ChangeState(CharacterMovementState.Jump, isReplay);
                return;
            }

            if (unitMovementController.CurrentMovementData.HasMoveInput() || unitMovementController.CurrentMovementData.HasTurnInput()) {
                unitMovementController.ChangeState(CharacterMovementState.Move, isReplay);
                return;
            }

            // this logic is handled in UnitMotor on the server, or single player mode
            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true) {
                return;
            }

            if (unitMovementController.ReconciledNavMeshAgentVelocity.sqrMagnitude > 0) {
                if (unitController.UnitAnimator != null) {
                    //Debug.Log($"{unitController.gameObject.name}.MovementNavMeshState.Update() setting moving true because velocity is {unitMovementController.ReconciledNavMeshAgentVelocity}");
                    unitController.UnitAnimator.SetMoving(true);
                    unitController.UnitAnimator.SetVelocity(unitController.transform.InverseTransformDirection(unitMovementController.ReconciledNavMeshAgentVelocity));
                }
            } else {
                //Debug.Log($"{unitController.gameObject.name}.MovementNavMeshState.Update() setting moving false because velocity is zero");
                if (unitController.UnitAnimator != null) {
                    unitController.UnitAnimator.SetMoving(false);
                    unitController.UnitAnimator.SetVelocity(Vector3.zero);
                }
            }

        }
    }

}