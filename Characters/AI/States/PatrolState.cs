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

        public void Enter(AIController aiController) {
            //Debug.Log(aiController.gameObject.name + ".PatrolState.Enter() position: " + aiController.transform.position);
            this.aiController = aiController;
            if (!aiController.MyAiPatrol.PatrolComplete()) {
                Vector3 tmpDestination = aiController.MyAiPatrol.GetDestination(false);
                if (tmpDestination != Vector3.zero) {
                    currentDestination = this.aiController.SetDestination(tmpDestination);
                }
                this.aiController.MyBaseCharacter.MyAnimatedUnit.MyCharacterMotor.MyMovementSpeed = this.aiController.MyMovementSpeed;
            } else {
                aiController.ChangeState(new IdleState());
            }
            //Debug.Log(aiController.gameObject.name + ".PatrolState.Enter() setting new Destination: " + newDestination);
        }

        public void Exit() {
            if (coroutine != null) {
                aiController.StopCoroutine(coroutine);
            }
        }

        public void Update() {
            //Debug.Log(aiController.gameObject.name + ": PatrolState.Update() at location: " + aiController.transform.position);
            if (aiController.MyAiPatrol.enabled == false) {
                aiController.ChangeState(new IdleState());
                return;
            }

            aiController.UpdateTarget();

            if (aiController.MyTarget != null) {
                aiController.MyLeashPosition = aiController.MyBaseCharacter.MyCharacterUnit.transform.position;
                aiController.ChangeState(new FollowState());
            }

            bool getNewDestination = false;

            if (currentDestination == Vector3.zero && coroutine == null) {
                getNewDestination = true;
            } else if (Vector3.Distance(aiController.MyBaseCharacter.MyCharacterUnit.transform.position, currentDestination) <= aiController.MyBaseCharacter.MyAnimatedUnit.MyAgent.stoppingDistance + aiController.MyBaseCharacter.MyAnimatedUnit.MyCharacterMotor.MyNavMeshDistancePadding) {
                //Debug.Log(aiController.gameObject.name + ".PatrolState.Update(): Destination Reached!");

                // destination reached
                if (aiController.MyAiPatrol.PatrolComplete()) {
                    if (aiController.MyAiPatrol.MyDespawnOnCompletion) {
                        if (aiController.MyBaseCharacter.MyCharacterUnit != null) {
                            aiController.MyBaseCharacter.MyCharacterUnit.Despawn(0, false, true);
                        }
                    } else {
                        aiController.ChangeState(new IdleState());
                        return;
                    }
                } else {
                    //Debug.Log(aiController.gameObject.name + ".PatrolState.Update(): Destination Reached and patrol not complete yet!");
                    getNewDestination = true;
                }
            }

            if (getNewDestination == true) {
                Vector3 tmpDestination = aiController.MyAiPatrol.GetDestination(true);
                if (tmpDestination == Vector3.zero) {
                    Debug.Log(aiController.gameObject.name + ".PatrolState.Update(): GOT ZERO DESTINATION, SKIPPING TO NEXT UPDATE");
                    return;
                }
                // destination is safe, set it
                currentDestination = tmpDestination;

                this.aiController.MyBaseCharacter.MyAnimatedUnit.MyCharacterMotor.MyMovementSpeed = this.aiController.MyMovementSpeed;
                coroutine = (aiController as MonoBehaviour).StartCoroutine(PauseForNextDestination(currentDestination));
            }
        }

        public IEnumerator PauseForNextDestination(Vector3 nextDestination) {

            float remainingPauseTime = aiController.MyAiPatrol.MyDestinationPauseTime;
            while (remainingPauseTime > 0f) {
                remainingPauseTime -= Time.deltaTime;
                Debug.Log(aiController.gameObject.name + ".PatrolState.PauseForNextDestination(" + nextDestination + "): remainingPauseTime: " + remainingPauseTime);
                yield return null;
            }
            currentDestination = this.aiController.SetDestination(nextDestination);
        }
    }

}