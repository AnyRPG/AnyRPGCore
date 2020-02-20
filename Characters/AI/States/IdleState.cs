using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class IdleState : IState {

        private AIController aiController;

        public void Enter(AIController aiController) {
            Debug.Log(aiController.gameObject.name + " entering Idle state");
            this.aiController = aiController;
            this.aiController.Reset();
            if (aiController.MyAiPatrol != null && aiController.MyAiPatrol.enabled == true && aiController.MyAiPatrol.MyAutomaticPatrol != null && aiController.MyAiPatrol.MyAutomaticPatrol.PatrolComplete() == false) {
                aiController.ChangeState(new PatrolState());
                return;
            }
        }

        public void Exit() {

        }

        public void Update() {
            //Debug.Log(aiController.gameObject.name + ": IdleState.Update()");
            aiController.UpdateTarget();

            // change into follow state if the player is close
            if (aiController.MyTarget != null) {
                //Debug.Log(aiController.gameObject.name + ": IdleState.Update(): setting follow state");
                aiController.ChangeState(new FollowState());
                return;
            }
        }
    }

}