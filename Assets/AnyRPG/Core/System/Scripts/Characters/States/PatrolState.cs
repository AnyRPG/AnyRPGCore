using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PatrolState : IState {

        private UnitController unitController;

        private Vector3 currentDestination = Vector3.zero;

        private Coroutine pauseCoroutine;

        private float originalMovementSpeed = 0f;

        public void Enter(UnitController unitController) {
            //Debug.Log($"{unitController.gameObject.name}.PatrolState.Enter()");

            this.unitController = unitController;
            if (unitController.PatrolController.CurrentPatrolSaveState.PatrolComplete() == false) {
                originalMovementSpeed = this.unitController.UnitMotor.MovementSpeed;
                SetMovementSpeed();

                // set destination
                Vector3 tmpDestination = unitController.PatrolController.CurrentPatrolSaveState.GetDestination(false);
                if (tmpDestination != Vector3.zero) {
                    currentDestination = this.unitController.SetDestination(tmpDestination);
                }
            } else {
                unitController.ChangeState(new IdleState());
                return;
            }
            //Debug.Log(aiController.gameObject.name + ".PatrolState.Enter() setting new Destination: " + newDestination);
        }

        public void Exit() {
            //Debug.Log(baseController.gameObject.name + ".PatrolState.Exit()");
            if (pauseCoroutine != null) {
                unitController.StopCoroutine(pauseCoroutine);
            }
            this.unitController.UnitMotor.MovementSpeed = originalMovementSpeed;
        }

        public void SetMovementSpeed() {
            //Debug.Log(baseController.gameObject.name + ".PatrolState.SetMovementSpeed() patrol: " + baseController.PatrolController.CurrentPatrol.MovementSpeed + " motor: " + this.baseController.UnitMotor.MovementSpeed + "; " + "; aicontroller: " + this.baseController.MovementSpeed);
            if (unitController.PatrolController.CurrentPatrol.MovementSpeed == 0) {
                this.unitController.UnitMotor.MovementSpeed = this.unitController.MovementSpeed;
            } else {
                this.unitController.UnitMotor.MovementSpeed = unitController.PatrolController.CurrentPatrol.MovementSpeed;
            }
        }

        public void Update() {
            //Debug.Log(baseController.gameObject.name + ": PatrolState.Update() at location: " + baseController.transform.position);

            // if this was an AI and was captured as a pet, stop any in progress patrol
            if (unitController.UnitControllerMode != UnitControllerMode.AI) {
                unitController.ChangeState(new IdleState());
                return;
            }

            // give a chance to switch to attack mode
            unitController.UpdateTarget();
            if (unitController.Target != null && unitController.AggroEnabled == true) {
                unitController.LeashPosition = unitController.transform.position;
                unitController.ChangeState(new FollowState());
                return;
            }

            // if no combat was needed, check if current wait is in progress for a new stage of the patrol
            if (pauseCoroutine != null) {
                return;
            }

            // check if a new patrol destination is needed
            if (currentDestination == Vector3.zero && pauseCoroutine == null) {
                //Debug.Log(baseController.gameObject.name + ".PatrolState.Update(): getNewDestination: destination was vector3.zero");
                GetNewDestination();
                return;
            } else if (Vector3.Distance(unitController.transform.position, currentDestination) <= unitController.NavMeshAgent.stoppingDistance + unitController.UnitMotor.NavMeshDistancePadding) {
                // destination reached
                //Debug.Log($"{unitController.gameObject.name}.PatrolState.Update(): Destination Reached!  setting HasDestinationPosition to false");

                // we need to set this manually, or the next fixedUpdate that runs will clear our pending destination.
                //unitController.UnitMotor.HasDestinationPosition = false;
                
                if (unitController.PatrolController.CurrentPatrolSaveState.PatrolComplete()) {
                    if (unitController.PatrolController.CurrentPatrol.DespawnOnCompletion) {
                        unitController.Despawn(0, false, true);
                        return;
                    } else {
                        TrySavePersistentData();
                        unitController.ChangeState(new IdleState());
                        return;
                    }
                } else {
                    //Debug.Log(baseController.gameObject.name + ".PatrolState.Update(): getNewDestination: current destination reached and patrol was not complete");
                    GetNewDestination();
                    return;
                }
            }
            if (unitController.UnitMotor.SetMoveDestination == true) {
                return;
            }
            if (unitController.NavMeshAgent.pathPending == false) {
                if (unitController.NavMeshAgent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid) {
                    // this message means things are working properly and the unit just prevented itself from getting stuck or stalling
                    //Debug.Log(baseController.gameObject.name + ".PatrolState.Update(): getNewDestination: invalid path");
                    GetNewDestination();
                    return;
                }

                if (unitController.NavMeshAgent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathPartial) {
                    // this message means things are working properly and the unit just prevented itself from getting stuck or stalling
                    //Debug.Log(baseController.gameObject.name + ".PatrolState.Update(): getNewDestination: partial path");
                    GetNewDestination();
                    return;
                }
            }

            //Debug.Log(baseController.gameObject.name + ".PatrolState.Update(): end of update and no action taken");
        }

        private void GetNewDestination() {
            //Debug.Log($"{unitController.gameObject.name}.PatrolState.GetNewDestination()");

            TrySavePersistentData();
            Vector3 tmpDestination = unitController.PatrolController.CurrentPatrolSaveState.GetDestination(true);
            if (tmpDestination == Vector3.zero) {
                //Debug.Log(baseController.gameObject.name + ".PatrolState.Update(): GOT ZERO DESTINATION, SKIPPING TO NEXT UPDATE");
                return;
            }
            // destination is safe, set it
            currentDestination = tmpDestination;

            SetMovementSpeed();
            pauseCoroutine = unitController.StartCoroutine(PauseForNextDestination(currentDestination));
        }

        public void TrySavePersistentData() {
            //Debug.Log(aiController.gameObject.name + ".PatrolState.TrySavePersistentData()");
            if (unitController?.PatrolController?.CurrentPatrol != null && unitController.PatrolController.CurrentPatrol.SavePositionAtDestination) {
                if (unitController.PersistentObjectComponent != null) {
                    unitController.PersistentObjectComponent.SaveProperties();
                }
            }
        }

        public IEnumerator PauseForNextDestination(Vector3 nextDestination) {
            //Debug.Log($"{unitController.gameObject.name}.PatrolState.PauseForNextDestination({nextDestination})");

            float remainingPauseTime = unitController.PatrolController.CurrentPatrol.DestinationPauseTime;
            while (remainingPauseTime > 0f) {
                yield return null;
                remainingPauseTime -= Time.deltaTime;
                //Debug.Log(aiController.gameObject.name + ".PatrolState.PauseForNextDestination(" + nextDestination + "): remainingPauseTime: " + remainingPauseTime);
            }
            currentDestination = this.unitController.SetDestination(nextDestination);
            pauseCoroutine = null;
        }
    }

}