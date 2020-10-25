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
            //Debug.Log(aiController.gameObject.name + ".PatrolState.Enter() position: " + aiController.transform.position);
            this.baseController = baseController;
            if (!baseController.PatrolController.MyCurrentPatrol.PatrolComplete()) {
                originalMovementSpeed = this.baseController.BaseCharacter.UnitController.UnitMotor.MyMovementSpeed;
                SetMovementSpeed();

                // set destination
                Vector3 tmpDestination = baseController.PatrolController.MyCurrentPatrol.GetDestination(false);
                if (tmpDestination != Vector3.zero) {
                    currentDestination = this.baseController.SetDestination(tmpDestination);
                }
            } else {
                baseController.ChangeState(new IdleState());
            }
            //Debug.Log(aiController.gameObject.name + ".PatrolState.Enter() setting new Destination: " + newDestination);
        }

        public void SetMovementSpeed() {
            //Debug.Log(aiController.gameObject.name + ".PatrolState.SetMovementSpeed() patrol: " + aiController.MyAiPatrol.MyCurrentPatrol.MyMovementSpeed + " motor: " + this.aiController.MyBaseCharacter.UnitController.MyCharacterMotor.MyMovementSpeed + "; " + "; aicontroller: " + this.aiController.MyMovementSpeed);
            if (baseController.PatrolController.MyCurrentPatrol.MovementSpeed == 0) {
                this.baseController.BaseCharacter.UnitController.UnitMotor.MyMovementSpeed = this.baseController.MovementSpeed;
            } else {
                this.baseController.BaseCharacter.UnitController.UnitMotor.MyMovementSpeed = baseController.PatrolController.MyCurrentPatrol.MovementSpeed;
            }
        }

        public void Exit() {
            //Debug.Log(aiController.gameObject.name + ".PatrolState.Exit()");
            if (coroutine != null) {
                baseController.StopCoroutine(coroutine);
            }
            this.baseController.BaseCharacter.UnitController.UnitMotor.MyMovementSpeed = originalMovementSpeed;
        }

        public void Update() {
            //Debug.Log(aiController.gameObject.name + ": PatrolState.Update() at location: " + aiController.transform.position);
            if (baseController.UnitControllerMode != UnitControllerMode.AI) {
                baseController.ChangeState(new IdleState());
                return;
            }

            baseController.UpdateTarget();

            if (baseController.Target != null && baseController.AggroEnabled() == true) {
                baseController.LeashPosition = baseController.BaseCharacter.CharacterUnit.transform.position;
                baseController.ChangeState(new FollowState());
            }

            bool getNewDestination = false;

            if (currentDestination == Vector3.zero && coroutine == null) {
                getNewDestination = true;
            } else if (Vector3.Distance(baseController.BaseCharacter.CharacterUnit.transform.position, currentDestination) <= baseController.BaseCharacter.UnitController.NavMeshAgent.stoppingDistance + baseController.BaseCharacter.UnitController.UnitMotor.MyNavMeshDistancePadding) {
                //Debug.Log(aiController.gameObject.name + ".PatrolState.Update(): Destination Reached!");

                // destination reached
                if (baseController.PatrolController.MyCurrentPatrol.PatrolComplete()) {
                    if (baseController.PatrolController.MyCurrentPatrol.DespawnOnCompletion) {
                        if (baseController.BaseCharacter.CharacterUnit != null) {
                            baseController.BaseCharacter.CharacterUnit.Despawn(0, false, true);
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
            if (baseController.BaseCharacter.UnitController.NavMeshAgent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid) {
                // this message means things are working properly and the unit just prevented itself from getting stuck or stalling
                //Debug.Log(aiController.gameObject.name + ".PatrolState.Update(): DESTINATION WAS INVALID, GETTING NEW DESTINATION");
                getNewDestination = true;
            }

            if (baseController.BaseCharacter.UnitController.NavMeshAgent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathPartial) {
                // this message means things are working properly and the unit just prevented itself from getting stuck or stalling
                //Debug.Log(aiController.gameObject.name + ".PatrolState.Update(): DESTINATION WAS PARTIAL, GETTING NEW DESTINATION");
                getNewDestination = true;
            }

            if (getNewDestination == true) {
                TrySavePersistentData();
                Vector3 tmpDestination = baseController.PatrolController.MyCurrentPatrol.GetDestination(true);
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
            if (baseController != null && baseController.PatrolController != null && baseController.PatrolController.MyCurrentPatrol != null && baseController.PatrolController.MyCurrentPatrol.SavePositionAtDestination) {
                if (baseController.PersistentObjectComponent != null) {
                    baseController.PersistentObjectComponent.SaveProperties();
                }
            }
        }

        public IEnumerator PauseForNextDestination(Vector3 nextDestination) {
            //Debug.Log(aiController.gameObject.name + ".PatrolState.PauseForNextDestination(" + nextDestination + ")");

            float remainingPauseTime = baseController.PatrolController.MyCurrentPatrol.DestinationPauseTime;
            while (remainingPauseTime > 0f) {
                yield return null;
                remainingPauseTime -= Time.deltaTime;
                //Debug.Log(aiController.gameObject.name + ".PatrolState.PauseForNextDestination(" + nextDestination + "): remainingPauseTime: " + remainingPauseTime);
            }
            currentDestination = this.baseController.SetDestination(nextDestination);
        }
    }

}