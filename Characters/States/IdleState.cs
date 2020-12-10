using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class IdleState : IState {

        private UnitController unitController;

        public void Enter(UnitController unitController) {
            //Debug.Log(aiController.gameObject.name + " entering Idle state");
            this.unitController = unitController;
            this.unitController.Reset();
            TryToEnterPatrolState();
        }

        public void Exit() {

        }

        public void TryToEnterPatrolState() {
            Debug.Log("IdleState.TryToEnterPatrolState()");
            if (unitController.UnitControllerMode == UnitControllerMode.AI
                && unitController.PatrolController != null
                && unitController.PatrolController.CurrentPatrol != null
                && unitController.PatrolController.CurrentPatrol.PatrolComplete() == false) {
                unitController.ChangeState(new PatrolState());
                return;
            }
        }

        public void Update() {
            //Debug.Log(aiController.gameObject.name + ": IdleState.Update()");
            unitController.UpdateTarget();

            // change into follow state if the player is close
            if (unitController.Target != null && unitController.AggroEnabled() == true) {
                //Debug.Log(aiController.gameObject.name + ": IdleState.Update(): setting follow state");
                unitController.ChangeState(new FollowState());
                return;
            }
            TryToEnterPatrolState();
        }
    }

}