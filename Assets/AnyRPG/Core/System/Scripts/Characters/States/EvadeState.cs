using UnityEngine;

namespace AnyRPG {
    public class EvadeState : IState {

        private UnitController unitController;

        public void Enter(UnitController unitController) {
            //Debug.Log($"EvadeState.Enter({unitController.gameObject.name})");

            this.unitController = unitController;
            this.unitController.ClearTarget();
            this.unitController.SetDestination(unitController.LeashPosition);
            this.unitController.UnitMotor.MovementSpeed = unitController.EvadeRunSpeed;
            this.unitController.CharacterCombat.AggroTable.ClearAndBroadcast();
        }

        public void Exit() {
            // if the unit is leaving evade state, we need to cycle the collider in case there are units in range that should be aggroed.
            unitController.UnitEventController.NotifyOnDisableAggro();
            unitController.UnitEventController.NotifyOnEnableAggro();
        }

        public void Update() {
            //Debug.Log(aiController.gameObject.name + ": EvadeState.Update()");

            float distance = Vector3.Distance(unitController.LeashPosition, unitController.transform.position);
            //Debug.Log(aiController.gameObject.name + ": EvadeState.Update(): Distance from spawn point: " + distance.ToString());
            if (distance <= unitController.NavMeshAgent.stoppingDistance + unitController.UnitMotor.NavMeshDistancePadding) {
                unitController.ChangeState(new IdleState());
                return;
            }
        }
    }

}