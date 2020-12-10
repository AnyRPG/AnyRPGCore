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
            Debug.Log(baseController.gameObject.name + ".PatrolState.Enter() position: " + baseController.transform.position);
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
            }
            //Debug.Log(aiController.gameObject.name + ".PatrolState.Enter() setting new Destination: " + newDestination);
        }

        public void SetMovementSpeed() {
            Debug.Log(baseController.gameObject.name + ".PatrolState.SetMovementSpeed() patrol: " + baseController.PatrolController.CurrentPatrol.MovementSpeed + " motor: " + this.baseController.UnitMotor.MovementSpeed + "; " + "; aicontroller: " + this.baseController.MovementSpeed);
            if (baseController.PatrolController.CurrentPatrol.MovementSpeed == 0) {
                this.baseController.UnitMotor.MovementSpeed = this.baseController.MovementSpeed;
            } else {
                this.baseController.UnitMotor.MovementSpeed = baseController.PatrolController.CurrentPatrol.MovementSpeed;
            }
        }

        public void Exit() {
            //Debug.Log(aiController.gameObject.name + ".PatrolState.Exit()");
            if (coroutine != null) {
                baseController.StopCoroutine(coroutine);
            }
            this.baseController.UnitMotor.MovementSpeed = originalMovementSpeed;
        }

        public void Update() {
            Debug.Log(baseController.gameObject.name + ": PatrolState.Update() at location: " + baseController.transform.position);
            if (baseController.UnitControllerMode != UnitControllerMode.AI) {
                baseController.ChangeState(new IdleState());
                return;
            }

            baseController.UpdateTarget();

            if (baseController.Target != null && baseController.AggroEnabled() == true) {
                baseController.LeashPosition = baseController.transform.position;
                baseController.ChangeState(new FollowState());
            }

            bool getNewDestination = false;

            if (currentDestination == Vector3.zero && coroutine == null) {
                getNewDestination = true;
            } else if (Vector3.Distance(baseController.transform.position, currentDestination) <= baseController.NavMeshAgent.stoppingDistance + baseController.UnitMotor.NavMeshDistancePadding) {
                //Debug.Log(aiController.gameObject.name + ".PatrolState.Update(): Destination Reached!");

                // destination reached
                if (baseController.PatrolController.CurrentPatrol.PatrolComplete()) {
                    if (baseController.PatrolController.CurrentPatrol.DespawnOnCompletion) {
                        if (baseController.CharacterUnit != null) {
                            baseController.CharacterUnit.Despawn(0, false, true);
                        }
                    } else {
                        TrySavePersistentData();
                        baseController.ChangeState(new IdleState());
                        return;
                    }
                } else {
                    //Debug.Log(aiController.gameObject.name + ".PatrolState.Update(): Destination Reached and patrol not complete yet!");
                    getNewDestination = true;
                }
            }


            //pathstatus: " + animatedUnit.MyAgent.pathStatus
            if (baseController.NavMeshAgent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid) {
                // this message means things are working properly and the unit just prevented itself from getting stuck or stalling
                //Debug.Log(aiController.gameObject.name + ".PatrolState.Update(): DESTINATION WAS INVALID, GETTING NEW DESTINATION");
                getNewDestination = true;
            }

            if (baseController.NavMeshAgent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathPartial) {
                // this message means things are working properly and the unit just prevented itself from getting stuck or stalling
                //Debug.Log(aiController.gameObject.name + ".PatrolState.Update(): DESTINATION WAS PARTIAL, GETTING NEW DESTINATION");
                getNewDestination = true;
            }

            if (getNewDestination == true) {
                TrySavePersistentData();
                Vector3 tmpDestination = baseController.PatrolController.CurrentPatrol.GetDestination(true);
                if (tmpDestination == Vector3.zero) {
                    //Debug.Log(aiController.gameObject.name + ".PatrolState.Update(): GOT ZERO DESTINATION, SKIPPING TO NEXT UPDATE");
                    return;
                }
                // destination is safe, set it
                currentDestination = tmpDestination;

                SetMovementSpeed();
                coroutine = (baseController as MonoBehaviour).StartCoroutine(PauseForNextDestination(currentDestination));
            }
        }

        public void TrySavePersistentData() {
            //Debug.Log(aiController.gameObject.name + ".PatrolState.TrySavePersistentData()");
            if (baseController != null && baseController.PatrolController != null && baseController.PatrolController.CurrentPatrol != null && baseController.PatrolController.CurrentPatrol.SavePositionAtDestination) {
                if (baseController.PersistentObjectComponent != null) {
                    baseController.PersistentObjectComponent.SaveProperties();
                }
            }
        }

        public IEnumerator PauseForNextDestination(Vector3 nextDestination) {
            //Debug.Log(aiController.gameObject.name + ".PatrolState.PauseForNextDestination(" + nextDestination + ")");

            float remainingPauseTime = baseController.PatrolController.CurrentPatrol.DestinationPauseTime;
            while (remainingPauseTime > 0f) {
                yield return null;
                remainingPauseTime -= Time.deltaTime;
                //Debug.Log(aiController.gameObject.name + ".PatrolState.PauseForNextDestination(" + nextDestination + "): remainingPauseTime: " + remainingPauseTime);
            }
            currentDestination = this.baseController.SetDestination(nextDestination);
        }
    }

}