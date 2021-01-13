using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PatrolState : IState {

        private UnitController baseController;

        private Vector3 currentDestination = Vector3.zero;

        private Coroutine coroutine;

        private float originalMovementSpeed = 0f;

        public void Enter(UnitController baseController) {
            //Debug.Log(baseController.gameObject.name + ".PatrolState.Enter() position: " + baseController.transform.position);
            this.baseController = baseController;
            if (!baseController.PatrolController.CurrentPatrol.PatrolComplete()) {
                originalMovementSpeed = this.baseController.UnitMotor.MovementSpeed;
                SetMovementSpeed();

                // set destination
                Vector3 tmpDestination = baseController.PatrolController.CurrentPatrol.GetDestination(false);
                if (tmpDestination != Vector3.zero) {
                    currentDestination = this.baseController.SetDestination(tmpDestination);
                }
            } else {
                baseController.ChangeState(new IdleState());
                return;
            }
            //Debug.Log(aiController.gameObject.name + ".PatrolState.Enter() setting new Destination: " + newDestination);
        }

        public void Exit() {
            //Debug.Log(baseController.gameObject.name + ".PatrolState.Exit()");
            if (coroutine != null) {
                baseController.StopCoroutine(coroutine);
            }
            this.baseController.UnitMotor.MovementSpeed = originalMovementSpeed;
        }

        public void SetMovementSpeed() {
            //Debug.Log(baseController.gameObject.name + ".PatrolState.SetMovementSpeed() patrol: " + baseController.PatrolController.CurrentPatrol.MovementSpeed + " motor: " + this.baseController.UnitMotor.MovementSpeed + "; " + "; aicontroller: " + this.baseController.MovementSpeed);
            if (baseController.PatrolController.CurrentPatrol.PatrolProperties.MovementSpeed == 0) {
                this.baseController.UnitMotor.MovementSpeed = this.baseController.MovementSpeed;
            } else {
                this.baseController.UnitMotor.MovementSpeed = baseController.PatrolController.CurrentPatrol.PatrolProperties.MovementSpeed;
            }
        }

        public void Update() {
            //Debug.Log(baseController.gameObject.name + ": PatrolState.Update() at location: " + baseController.transform.position);

            // if this was an AI and was captured as a pet, stop any in progress patrol
            if (baseController.UnitControllerMode != UnitControllerMode.AI) {
                baseController.ChangeState(new IdleState());
                return;
            }

            // give a chance to switch to attack mode
            baseController.UpdateTarget();
            if (baseController.Target != null && baseController.AggroEnabled() == true) {
                baseController.LeashPosition = baseController.transform.position;
                baseController.ChangeState(new FollowState());
                return;
            }

            // if no combat was needed, check if current wait is in progress for a new stage of the patrol
            if (coroutine != null) {
                return;
            }

            // check if a new patrol destination is needed
            if (currentDestination == Vector3.zero && coroutine == null) {
                //Debug.Log(baseController.gameObject.name + ".PatrolState.Update(): getNewDestination: destination was vector3.zero");
                GetNewDestination();
                return;
            } else if (Vector3.Distance(baseController.transform.position, currentDestination) <= baseController.NavMeshAgent.stoppingDistance + baseController.UnitMotor.NavMeshDistancePadding) {
                //Debug.Log(aiController.gameObject.name + ".PatrolState.Update(): Destination Reached!");

                // destination reached
                if (baseController.PatrolController.CurrentPatrol.PatrolComplete()) {
                    if (baseController.PatrolController.CurrentPatrol.PatrolProperties.DespawnOnCompletion) {
                        if (baseController.CharacterUnit != null) {
                            baseController.CharacterUnit.Despawn(0, false, true);
                        }
                    } else {
                        TrySavePersistentData();
                        baseController.ChangeState(new IdleState());
                        return;
                    }
                } else {
                    //Debug.Log(baseController.gameObject.name + ".PatrolState.Update(): getNewDestination: current destination reached and patrol was not complete");
                    GetNewDestination();
                    return;
                }
            }

            //pathstatus: " + animatedUnit.MyAgent.pathStatus
            if (baseController.NavMeshAgent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid) {
                // this message means things are working properly and the unit just prevented itself from getting stuck or stalling
                //Debug.Log(baseController.gameObject.name + ".PatrolState.Update(): getNewDestination: invalid path");
                GetNewDestination();
                return;
            }

            if (baseController.NavMeshAgent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathPartial) {
                // this message means things are working properly and the unit just prevented itself from getting stuck or stalling
                //Debug.Log(baseController.gameObject.name + ".PatrolState.Update(): getNewDestination: partial path");
                GetNewDestination();
                return;
            }
        }

        private void GetNewDestination() {
            //Debug.Log(baseController.gameObject.name + ".PatrolState.GetNewDestination() patrol: " + (baseController?.PatrolController?.CurrentPatrol == null ? "null" : baseController.PatrolController.CurrentPatrol.DisplayName));
            TrySavePersistentData();
            Vector3 tmpDestination = baseController.PatrolController.CurrentPatrol.GetDestination(true);
            if (tmpDestination == Vector3.zero) {
                //Debug.Log(baseController.gameObject.name + ".PatrolState.Update(): GOT ZERO DESTINATION, SKIPPING TO NEXT UPDATE");
                return;
            }
            // destination is safe, set it
            currentDestination = tmpDestination;

            SetMovementSpeed();
            coroutine = baseController.StartCoroutine(PauseForNextDestination(currentDestination));
        }

        public void TrySavePersistentData() {
            //Debug.Log(aiController.gameObject.name + ".PatrolState.TrySavePersistentData()");
            if (baseController != null && baseController.PatrolController != null && baseController.PatrolController.CurrentPatrol != null && baseController.PatrolController.CurrentPatrol.PatrolProperties.SavePositionAtDestination) {
                if (baseController.PersistentObjectComponent != null) {
                    baseController.PersistentObjectComponent.SaveProperties();
                }
            }
        }

        public IEnumerator PauseForNextDestination(Vector3 nextDestination) {
            //Debug.Log(baseController.gameObject.name + ".PatrolState.PauseForNextDestination(" + nextDestination + ")");

            float remainingPauseTime = baseController.PatrolController.CurrentPatrol.PatrolProperties.DestinationPauseTime;
            while (remainingPauseTime > 0f) {
                yield return null;
                remainingPauseTime -= Time.deltaTime;
                //Debug.Log(aiController.gameObject.name + ".PatrolState.PauseForNextDestination(" + nextDestination + "): remainingPauseTime: " + remainingPauseTime);
            }
            currentDestination = this.baseController.SetDestination(nextDestination);
        }
    }

}