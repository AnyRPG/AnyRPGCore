using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathState : IState
{
    private AIController aiController;

    public void Enter(AIController aiController) {
        //Debug.Log(enemyController.gameObject.name + " entered death state");
        this.aiController = aiController;
        //this.aiController.MyBaseCharacter.MyCharacterUnit.GetComponentInChildren<Animator>().enabled = false;
        this.aiController.MyBaseCharacter.MyCharacterUnit.MyCharacterMotor.StopNavAgent();
        this.aiController.MyBaseCharacter.MyCharacterUnit.MyCharacterMotor.enabled = false;
        this.aiController.DisableAggro();
        this.aiController.ClearTarget();
    }

    public void Exit() {
        this.aiController.MyBaseCharacter.MyCharacterUnit.MyCharacterMotor.StartNavAgent();
        this.aiController.MyBaseCharacter.MyCharacterUnit.MyCharacterMotor.enabled = true;
        aiController.EnableAggro();
    }

    public void Update() {
        if (aiController.MyBaseCharacter.MyCharacterStats.IsAlive) {
            Debug.Log("No Longer Dead!");
            aiController.ChangeState(new ReturnState());
        }
        return;
        /*
        float distance = Vector3.Distance(parent.MyStartPosition, parent.transform.position);
        Debug.Log("Distance from spawn point: " + distance.ToString());
        if (distance <= 1) {
            parent.ChangeState(new IdleState());
        }
        */
    }
}
