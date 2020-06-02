using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PatrolState : IState {

        private AIController aiController;

        private Vector3 currentDestination = Vector3.zero;

        private Coroutine coroutine;

        private float originalMovementSpeed = 0f;

        public void Enter(AIController aiController) {
            //Debug.Log(aiController.gameObject.name + ".PatrolState.Enter() position: " + aiController.transform.position);
            this.aiController = aiController;
            if (!aiController.MyAiPatrol.MyCurrentPatrol.PatrolComplete()) {
                originalMovementSpeed = this.aiController.MyBaseCharacter.AnimatedUnit.MyCharacterMotor.MyMovementSpeed;
                SetMovementSpeed();

                // set destination
                Vector3 tmpDestination = aiController.MyAiPatrol.MyCurrentPatrol.GetDestination(false);
                if (tmpDestination != Vector3.zero) {
                    currentDestination = this.aiController.SetDestination(tmpDestination);
                }
            } else {
                aiController.ChangeState(new IdleState());
            }
            //Debug.Log(aiController.gameObject.name + ".PatrolState.Enter() setting new Destination: " + newDestination);
        }

        public void SetMovementSpeed() {
            //Debug.Log(aiController.gameObject.name + ".PatrolState.SetMovementSpeed() patrol: " + aiController.MyAiPatrol.MyCurrentPatrol.MyMovementSpeed + " motor: " + this.aiController.MyBaseCharacter.AnimatedUnit.MyCharacterMotor.MyMovementSpeed + "; " + "; aicontroller: " + this.aiController.MyMovementSpeed);
            if (aiController.MyAiPatrol.MyCurrentPatrol.MyMovementSpeed == 0) {
                this.aiController.MyBaseCharacter.AnimatedUnit.MyCharacterMotor.MyMovementSpeed = this.aiController.MyMovementSpeed;
            } else {
                this.aiController.MyBaseCharacter.AnimatedUnit.MyCharacterMotor.MyMovementSpeed = aiController.MyAiPatrol.MyCurrentPatrol.MyMovementSpeed;
            }
        }

        public void Exit() {
            //Debug.Log(aiController.gameObject.name + ".PatrolState.Exit()");
            if (coroutine != null) {
                aiController.StopCoroutine(coroutine);
            }
            this.aiController.MyBaseCharacter.AnimatedUnit.MyCharacterMotor.MyMovementSpeed = originalMovementSpeed;
        }

        public void Update() {
            //Debug.Log(aiController.gameObject.name + ": PatrolState.Update() at location: " + aiController.transform.position);
            if (aiController.MyAiPatrol.enabled == false) {
                aiController.ChangeState(new IdleState());
                return;
            }

            aiController.UpdateTarget();

            if (aiController.MyTarget != null && aiController.AggroEnabled() == true) {
                aiController.MyLeashPosition = aiController.MyBaseCharacter.CharacterUnit.transform.position;
                aiController.ChangeState(new FollowState());
            }

            bool getNewDestination = false;

            if (currentDestination == Vector3.zero && coroutine == null) {
                getNewDestination = true;
            } else if (Vector3.Distance(aiController.MyBaseCharacter.CharacterUnit.transform.position, currentDestination) <= aiController.MyBaseCharacter.AnimatedUnit.MyAgent.stoppingDistance + aiController.MyBaseCharacter.AnimatedUnit.MyCharacterMotor.MyNavMeshDistancePadding) {
                //Debug.Log(aiController.gameObject.name + ".PatrolState.Update(): Destination Reached!");

                // destination reached
                if (aiController.MyAiPatrol.MyCurrentPatrol.PatrolComplete()) {
                    if (aiController.MyAiPatrol.MyCurrentPatrol.MyDespawnOnCompletion) {
                        if (aiController.MyBaseCharacter.CharacterUnit != null) {
                            aiController.MyBaseCharacter.CharacterUnit.Despawn(0, false, true);
                        }
                    } else {
                        TrySavePersistentData();
                        aiController.ChangeState(new IdleState());
                        return;
                    }
                } else {
                    //Debug.Log(aiController.gameObject.name + ".PatrolState.Update(): Destination Reached and patrol not complete yet!");
                    getNewDestination = true;
                }
            }


            //pathstatus: " + animatedUnit.MyAgent.pathStatus
            if (aiController.MyBaseCharacter.AnimatedUnit.MyAgent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid) {
                // this message means things are working properly and the unit just prevented itself from getting stuck or stalling
                //Debug.Log(aiController.gameObject.name + ".PatrolState.Update(): DESTINATION WAS INVALID, GETTING NEW DESTINATION");
                getNewDestination = true;
            }

            if (aiController.MyBaseCharacter.AnimatedUnit.MyAgent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathPartial) {
                // this message means things are working properly and the unit just prevented itself from getting stuck or stalling
                //Debug.Log(aiController.gameObject.name + ".PatrolState.Update(): DESTINATION WAS PARTIAL, GETTING NEW DESTINATION");
                getNewDestination = true;
            }

            if (getNewDestination == true) {
                TrySavePersistentData();
                Vector3 tmpDestination = aiController.MyAiPatrol.MyCurrentPatrol.GetDestination(true);
                if (tmpDestination == Vector3.zero) {
                    //Debug.Log(aiController.gameObject.name + ".PatrolState.Update(): GOT ZERO DESTINATION, SKIPPING TO NEXT UPDATE");
                    return;
                }
                // destination is safe, set it
                currentDestination = tmpDestination;

                SetMovementSpeed();
                coroutine = (aiController as MonoBehaviour).StartCoroutine(PauseForNextDestination(currentDestination));
            }
        }

        public void TrySavePersistentData() {
            //Debug.Log(aiController.gameObject.name + ".PatrolState.TrySavePersistentData()");
            if (aiController != null && aiController.MyAiPatrol != null && aiController.MyAiPatrol.MyCurrentPatrol != null && aiController.MyAiPatrol.MyCurrentPatrol.MySavePositionAtDestination) {
                PersistentObject persistentObject = aiController.gameObject.GetComponent<PersistentObject>();
                if (persistentObject != null) {
                    persistentObject.SaveProperties();
                }
            }
        }

        public IEnumerator PauseForNextDestination(Vector3 nextDestination) {
            //Debug.Log(aiController.gameObject.name + ".PatrolState.PauseForNextDestination(" + nextDestination + ")");

            float remainingPauseTime = aiController.MyAiPatrol.MyCurrentPatrol.MyDestinationPauseTime;
            while (remainingPauseTime > 0f) {
                yield return null;
                remainingPauseTime -= Time.deltaTime;
                //Debug.Log(aiController.gameObject.name + ".PatrolState.PauseForNextDestination(" + nextDestination + "): remainingPauseTime: " + remainingPauseTime);
            }
            currentDestination = this.aiController.SetDestination(nextDestination);
        }
    }

}