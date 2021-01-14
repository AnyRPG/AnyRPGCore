using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class EvadeState : IState {

        private UnitController baseController;

        public void Enter(UnitController baseController) {
            //Debug.Log("Enter evade state");
            this.baseController = baseController;
            this.baseController.ClearTarget();
            this.baseController.SetDestination(baseController.LeashPosition);
            this.baseController.UnitMotor.MovementSpeed = baseController.EvadeRunSpeed;
            this.baseController.CharacterUnit.BaseCharacter.CharacterCombat.AggroTable.ClearAndBroadcast();
        }

        public void Exit() {
        }

        public void Update() {
            //Debug.Log(aiController.gameObject.name + ": EvadeState.Update()");

            float distance = Vector3.Distance(baseController.LeashPosition, baseController.transform.position);
            //Debug.Log(aiController.gameObject.name + ": EvadeState.Update(): Distance from spawn point: " + distance.ToString());
            if (distance <= baseController.NavMeshAgent.stoppingDistance + baseController.UnitMotor.NavMeshDistancePadding) {
                baseController.ChangeState(new IdleState());
                return;
            }
        }
    }

}