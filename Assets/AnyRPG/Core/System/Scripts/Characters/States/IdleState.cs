using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class IdleState : IState {

        private UnitController unitController;

        public void Enter(UnitController unitController) {
            //Debug.Log($"{unitController.gameObject.name} entering Idle state");
            this.unitController = unitController;
            this.unitController.Reset();
            TryToEnterPatrolState();
        }

        public void Exit() {

        }

        public void TryToEnterPatrolState() {
            //Debug.Log($"{unitController.gameObject.name}.IdleState.TryToEnterPatrolState()");
            if (unitController.UnitControllerMode == UnitControllerMode.AI
                && unitController.PatrolController != null
                && unitController.PatrolController.CurrentPatrol != null
                && unitController.PatrolController.CurrentPatrolSaveState.PatrolComplete() == false) {
                unitController.ChangeState(new PatrolState());
                return;
            }/*
            else {
                if (unitController.PatrolController == null) {
                    Debug.Log(unitController.gameObject.name + ".IdleState.TryToEnterPatrolState(): patrol controller is null");
                    return;
                }
                if (unitController.PatrolController.CurrentPatrol == null) {
                    Debug.Log(unitController.gameObject.name + ".IdleState.TryToEnterPatrolState(): current patrol is null");
                    return;
                }
                if (unitController.PatrolController.CurrentPatrol.PatrolComplete() == true) {
                    Debug.Log(unitController.gameObject.name + ".IdleState.TryToEnterPatrolState(): current patrol is complete");
                }
            }
            */
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