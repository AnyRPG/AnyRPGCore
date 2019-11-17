using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ReturnState : IState {

        private AIController aiController;

        public void Enter(AIController aiController) {
            //Debug.Log(aiController.gameObject.name + ": Enter return state at position: " + aiController.transform.position);
            this.aiController = aiController;
            this.aiController.SetDestination(aiController.MyLeashPosition);
            this.aiController.MyBaseCharacter.MyAnimatedUnit.MyCharacterMotor.MyMovementSpeed = this.aiController.MyMovementSpeed;
        }

        public void Exit() {
        }

        public void Update() {
            //Debug.Log(aiController.gameObject.name + ": ReturnState.Update()");

            aiController.UpdateTarget();

            if (aiController.MyTarget != null) {
                aiController.ChangeState(new FollowState());
            }

            float distanceToLeashPosition = Vector3.Distance(aiController.MyLeashPosition, aiController.MyBaseCharacter.MyCharacterUnit.transform.position);
            //Debug.Log(aiController.gameObject.name + ": ReturnState: Distance from spawn point: " + distance.ToString());
            if (distanceToLeashPosition <= 1) {
                aiController.ChangeState(new IdleState());
            } else {
                float agentDestinationDrift = Vector3.Distance(aiController.MyLeashPosition, aiController.MyBaseCharacter.MyAnimatedUnit.MyAgent.destination);
                if (agentDestinationDrift >= aiController.MyBaseCharacter.MyAnimatedUnit.MyAgent.stoppingDistance + aiController.MyBaseCharacter.MyAnimatedUnit.MyCharacterMotor.MyNavMeshDistancePadding) {
                    //Debug.Log("ReturnState.Update(). agent destination is: " + aiController.MyBaseCharacter.MyAnimatedUnit.MyAgent.destination + "; resetting to: " + aiController.MyStartPosition);
                    this.aiController.SetDestination(aiController.MyLeashPosition);
                }
            }
        }
    }

}