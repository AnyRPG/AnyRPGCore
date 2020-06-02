using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class DeathState : IState {
        private AIController aiController;

        public void Enter(AIController aiController) {
            //Debug.Log(aiController.gameObject.name + ".DeathState.Enter(): entered death state");
            this.aiController = aiController;
            //this.aiController.MyBaseCharacter.MyCharacterUnit.GetComponentInChildren<Animator>().enabled = false;
            if (this.aiController.MyBaseCharacter != null && this.aiController.MyBaseCharacter.AnimatedUnit != null) {
                this.aiController.MyBaseCharacter.AnimatedUnit.MyCharacterMotor.StopNavAgent();
                this.aiController.MyBaseCharacter.AnimatedUnit.MyCharacterMotor.enabled = false;
            }
            this.aiController.DisableAggro();
            this.aiController.ClearTarget();

            if (this.aiController.MyCombatStrategy != null) {
                this.aiController.ResetCombat();
            }

            // handle despawn
            (aiController.MyBaseCharacter as AICharacter).TryToDespawn();

        }

        public void Exit() {
            //Debug.Log(aiController.gameObject.name + ".DeathState.Exit()");
            this.aiController.MyBaseCharacter.AnimatedUnit.MyCharacterMotor.StartNavAgent();
            this.aiController.MyBaseCharacter.AnimatedUnit.MyCharacterMotor.enabled = true;
            aiController.EnableAggro();
        }

        public void Update() {
            if (aiController.MyBaseCharacter.CharacterStats.IsAlive) {
                //Debug.Log("No Longer Dead!");
                aiController.ChangeState(new ReturnState());
            }
            return;
        }
    }

}