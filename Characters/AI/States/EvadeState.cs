using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class EvadeState : IState {

        private AIController aiController;

        public void Enter(AIController aiController) {
            //Debug.Log("Enter evade state");
            this.aiController = aiController;
            this.aiController.ClearTarget();
            this.aiController.SetDestination(aiController.MyLeashPosition);
            this.aiController.MyBaseCharacter.AnimatedUnit.MyCharacterMotor.MyMovementSpeed = aiController.MyEvadeRunSpeed;
            this.aiController.MyBaseCharacter.CharacterCombat.MyAggroTable.ClearAndBroadcast();
        }

        public void Exit() {
        }

        public void Update() {
            //Debug.Log(aiController.gameObject.name + ": EvadeState.Update()");

            float distance = Vector3.Distance(aiController.MyLeashPosition, aiController.MyBaseCharacter.CharacterUnit.transform.position);
            //Debug.Log(aiController.gameObject.name + ": EvadeState.Update(): Distance from spawn point: " + distance.ToString());
            if (distance <= aiController.MyBaseCharacter.AnimatedUnit.MyAgent.stoppingDistance + aiController.MyBaseCharacter.AnimatedUnit.MyCharacterMotor.MyNavMeshDistancePadding) {
                aiController.ChangeState(new IdleState());
            }
        }
    }

}