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
            if (this.baseController.CharacterUnit.BaseCharacter != null && this.baseController != null) {
                this.baseController.DisableAgent();
                this.baseController.DisableMotor();
            }
            this.baseController.DisableAggro();
            this.baseController.ClearTarget();

            if (this.baseController.MyCombatStrategy != null) {
                this.baseController.ResetCombat();
            }

            // handle despawn
            baseController.CharacterUnit.BaseCharacter.TryToDespawn();

        }

        public void Exit() {
            //Debug.Log(aiController.gameObject.name + ".DeathState.Exit()");
            this.baseController.EnableAgent();
            this.baseController.EnableMotor();
            baseController.EnableAggro();
        }

        public void Update() {
            if (baseController.CharacterUnit.BaseCharacter.CharacterStats.IsAlive) {
                //Debug.Log("No Longer Dead!");
                baseController.ChangeState(new ReturnState());
                return;
            }
        }
    }

}