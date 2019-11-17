using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
public class DeathState : IState
{
    private AIController aiController;

    public void Enter(AIController aiController) {
        //Debug.Log(enemyController.gameObject.name + " entered death state");
        this.aiController = aiController;
        //this.aiController.MyBaseCharacter.MyCharacterUnit.GetComponentInChildren<Animator>().enabled = false;
        this.aiController.MyBaseCharacter.MyAnimatedUnit.MyCharacterMotor.StopNavAgent();
        this.aiController.MyBaseCharacter.MyAnimatedUnit.MyCharacterMotor.enabled = false;
        this.aiController.DisableAggro();
        this.aiController.ClearTarget();
    }

    public void Exit() {
        this.aiController.MyBaseCharacter.MyAnimatedUnit.MyCharacterMotor.StartNavAgent();
        this.aiController.MyBaseCharacter.MyAnimatedUnit.MyCharacterMotor.enabled = true;
        aiController.EnableAggro();
    }

    public void Update() {
        if (aiController.MyBaseCharacter.MyCharacterStats.IsAlive) {
            //Debug.Log("No Longer Dead!");
            aiController.ChangeState(new ReturnState());
        }
        return;
    }
}

}