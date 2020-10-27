using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ReturnState : IState {

        private UnitController baseController;

        public void Enter(UnitController baseController) {
            //Debug.Log(aiController.gameObject.name + ": Enter return state at position: " + aiController.transform.position);
            this.baseController = baseController;
            this.baseController.SetDestination(baseController.LeashPosition);
            this.baseController.UnitMotor.MyMovementSpeed = this.baseController.MovementSpeed;
            if (this.baseController.MyCombatStrategy != null) {
                this.baseController.ResetCombat();
            }

        }

        public void Exit() {
        }

        public void Update() {
            //Debug.Log(aiController.gameObject.name + ": ReturnState.Update()");

            baseController.UpdateTarget();

            if (baseController.Target != null && baseController.AggroEnabled() == true) {
                baseController.ChangeState(new FollowState());
            }

            float distanceToLeashPosition = Vector3.Distance(baseController.LeashPosition, baseController.transform.position);
            //Debug.Log(aiController.gameObject.name + ": ReturnState: Distance from spawn point: " + distance.ToString());
            if (distanceToLeashPosition <= 1) {
                baseController.ChangeState(new IdleState());
            } else {
                float agentDestinationDrift = Vector3.Distance(baseController.LeashPosition, baseController.NavMeshAgent.destination);
                if (agentDestinationDrift >= baseController.NavMeshAgent.stoppingDistance + baseController.UnitMotor.MyNavMeshDistancePadding) {
                    //Debug.Log("ReturnState.Update(). agent destination is: " + aiController.MyBaseCharacter.MyAnimatedUnit.MyAgent.destination + "; resetting to: " + aiController.MyStartPosition);
                    this.baseController.SetDestination(baseController.LeashPosition);
                }
            }
        }
    }

}