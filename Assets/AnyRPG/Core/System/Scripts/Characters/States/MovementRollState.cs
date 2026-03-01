using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class MovementRollState : IMovementState {

        private UnitController unitController;
        private UnitMovementController unitMovementController;

        public MovementRollState(UnitMovementController unitMovementController, UnitController unitController) {
            this.unitController = unitController;
            this.unitMovementController = unitMovementController;
        }

        public void Enter(bool isReplay) {
            //Debug.Log($"{unitController.gameObject.name}.MovementRollState.Enter()");
        }

        public void Exit(bool isReplay) {
            //Debug.Log($"{unitController.gameObject.name}.MovementRollState.Exit()");
        }

        public void Update(bool isReplay, double timeInterval) {
            //Debug.Log($"{unitController.gameObject.name}.MovementRollState.Update()");
        }
    }

}