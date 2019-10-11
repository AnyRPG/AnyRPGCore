using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : IState {

    private AIController aiController;

    private Vector3 currentDestination = Vector3.zero;

    private Coroutine coroutine;

    public void Enter(AIController aiController) {
        //Debug.Log(aiController.gameObject.name + ".PatrolState.Enter() position: " + aiController.transform.position);
        this.aiController = aiController;
        if (!aiController.MyAiPatrol.PatrolComplete()) {
            currentDestination = aiController.MyAiPatrol.GetDestination(false);
            currentDestination = this.aiController.SetDestination(currentDestination);
            this.aiController.MyBaseCharacter.MyCharacterUnit.MyCharacterMotor.MyMovementSpeed = this.aiController.MyMovementSpeed;
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

        aiController.UpdateTarget();

        if (aiController.MyTarget != null) {
            aiController.MyLeashPosition = aiController.MyBaseCharacter.MyCharacterUnit.transform.position;
            aiController.ChangeState(new FollowState());
        }

        if (Vector3.Distance(aiController.MyBaseCharacter.MyCharacterUnit.transform.position, currentDestination) <= aiController.MyBaseCharacter.MyCharacterUnit.MyAgent.stoppingDistance + aiController.MyBaseCharacter.MyCharacterUnit.MyCharacterMotor.MyNavMeshDistancePadding) {
            //Debug.Log(aiController.gameObject.name + ".PatrolState.Update(): Destination Reached!");
            currentDestination = aiController.MyAiPatrol.GetDestination(true);

            // destination reached
            if (aiController.MyAiPatrol.PatrolComplete()) {
                if (aiController.MyAiPatrol.MyDespawnOnCompletion) {
                    aiController.MyBaseCharacter.MyCharacterUnit.Despawn(0, false, true);
                } else {
                    aiController.ChangeState(new IdleState());
                }
            } else {
                //Debug.Log(aiController.gameObject.name + ".PatrolState.Update(): Destination Reached and patrol not complete yet!");
                coroutine = (aiController as MonoBehaviour).StartCoroutine(PauseForNextDestination(currentDestination));
                this.aiController.MyBaseCharacter.MyCharacterUnit.MyCharacterMotor.MyMovementSpeed = this.aiController.MyMovementSpeed;
            }
        }
    }

    public IEnumerator PauseForNextDestination(Vector3 nextDestination) {
        if (aiController.MyAiPatrol.MyDestinationPauseTime == 0f) {
            yield return null;
        } else {
            float remainingPauseTime = aiController.MyAiPatrol.MyDestinationPauseTime;
            while (remainingPauseTime > 0f) {
                remainingPauseTime -= Time.deltaTime;
                yield return null;
            }
        }
        currentDestination = this.aiController.SetDestination(currentDestination);
    }
}
