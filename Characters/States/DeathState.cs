using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class DeathState : IState {
        private UnitController baseController;

        public void Enter(UnitController baseController) {
            //Debug.Log(aiController.gameObject.name + ".DeathState.Enter(): entered death state");
            this.baseController = baseController;
            //this.aiController.MyBaseCharacter.MyCharacterUnit.GetComponentInChildren<Animator>().enabled = false;
            if (this.baseController.BaseCharacter != null && this.baseController.BaseCharacter.UnitController != null) {
                this.baseController.BaseCharacter.UnitController.UnitMotor.StopNavAgent();
                this.baseController.BaseCharacter.UnitController.DisableMotor();
            }
            this.baseController.DisableAggro();
            this.baseController.ClearTarget();

            if (this.baseController.MyCombatStrategy != null) {
                this.baseController.ResetCombat();
            }

            // handle despawn
            baseController.BaseCharacter.TryToDespawn();

        }

        public void Exit() {
            //Debug.Log(aiController.gameObject.name + ".DeathState.Exit()");
            this.baseController.BaseCharacter.UnitController.UnitMotor.StartNavAgent();
            this.baseController.BaseCharacter.UnitController.EnableMotor();
            baseController.EnableAggro();
        }

        public void Update() {
            if (baseController.BaseCharacter.CharacterStats.IsAlive) {
                //Debug.Log("No Longer Dead!");
                baseController.ChangeState(new ReturnState());
            }
            return;
        }
    }

}