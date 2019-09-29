using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : IState {

    private AIController aiController;

    public void Enter(AIController aiController) {
        //Debug.Log(aiController.gameObject.name + " entering Idle state");
        this.aiController = aiController;
        this.aiController.Reset();
        if (aiController.MyAiPatrol != null && aiController.MyAiPatrol.PatrolComplete() == false) {
            aiController.ChangeState(new PatrolState());
        }
    }

    public void Exit() {
        
    }

    public void Update() {
        //Debug.Log(aiController.gameObject.name + ": IdleState.Update()");
        aiController.UpdateTarget();

        // change into follow state if the player is close
        if (aiController.MyTarget != null) {
            aiController.ChangeState(new FollowState());
        }
    }
}
