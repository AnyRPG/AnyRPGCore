using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ReturnState : IState {

        private UnitController unitController;

        public void Enter(UnitController baseController) {
            //Debug.Log(baseController.gameObject.name + ": Enter return state at position: " + baseController.transform.position);
            this.unitController = baseController;
            if (baseController.UnitProfile.IsMobile == false) {
                baseController.ChangeState(new IdleState());
                return;
            }
            this.unitController.SetDestination(baseController.LeashPosition);
            this.unitController.UnitMotor.MovementSpeed = this.unitController.MovementSpeed;
            if (this.unitController.CombatStrategy != null) {
                this.unitController.ResetCombat();
            }

        }

        public void Exit() {
            //Debug.Log(baseController.gameObject.name + ": Exit return state at position: " + baseController.transform.position);
        }

        public void Update() {
            //Debug.Log(aiController.gameObject.name + ": ReturnState.Update()");

            unitController.UpdateTarget();

            if (unitController.Target != null && unitController.AggroEnabled == true) {
                unitController.ChangeState(new FollowState());
                return;
            }

            float distanceToLeashPosition = Vector3.Distance(unitController.LeashPosition, unitController.transform.position);
            //Debug.Log(aiController.gameObject.name + ": ReturnState: Distance from spawn point: " + distance.ToString());
            if (distanceToLeashPosition <= 1) {
                unitController.ChangeState(new IdleState());
                return;
            } else {
                float agentDestinationDrift = Vector3.Distance(unitController.LeashPosition, unitController.NavMeshAgent.destination);
                if (agentDestinationDrift >= unitController.NavMeshAgent.stoppingDistance + unitController.UnitMotor.NavMeshDistancePadding) {
                    //Debug.Log("ReturnState.Update(). agent destination is: " + aiController.BaseCharacter.MyAnimatedUnit.MyAgent.destination + "; resetting to: " + aiController.MyStartPosition);
                    this.unitController.SetDestination(unitController.LeashPosition);
                }
            }
        }
    }

}