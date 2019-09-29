﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvadeState : IState
{

    private AIController aiController;

    public void Enter(AIController aiController) {
        Debug.Log("Enter evade state");
        this.aiController = aiController;
        this.aiController.ClearTarget();
        this.aiController.SetDestination(aiController.MyLeashPosition);
        this.aiController.MyBaseCharacter.MyCharacterUnit.MyCharacterMotor.MyMovementSpeed = aiController.MyEvadeRunSpeed;
        this.aiController.MyBaseCharacter.MyCharacterCombat.MyAggroTable.ClearAndBroadcast();
    }

    public void Exit() {
    }

    public void Update() {
        //Debug.Log(aiController.gameObject.name + ": EvadeState.Update()");

        float distance = Vector3.Distance(aiController.MyLeashPosition, aiController.MyBaseCharacter.MyCharacterUnit.transform.position);
        Debug.Log(aiController.gameObject.name + ": EvadeState.Update(): Distance from spawn point: " + distance.ToString());
        if (distance <= aiController.MyBaseCharacter.MyCharacterUnit.MyAgent.stoppingDistance + aiController.MyBaseCharacter.MyCharacterUnit.MyCharacterMotor.MyNavMeshDistancePadding) {
            aiController.ChangeState(new IdleState());
        }
    }
}
