using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
public class FollowState : IState
{
    private AIController aiController;

    public void Enter(AIController aiController) {
        //Debug.Log(aiController.gameObject.name + ".FollowState.Enter()");
        this.aiController = aiController;
        this.aiController.MyBaseCharacter.MyCharacterUnit.MyCharacterMotor.MyMovementSpeed = aiController.MyBaseCharacter.MyCharacterController.MyMovementSpeed;
    }

    public void Exit() {
        // stop following target code goes here
    }

    public void Update() {
        //Debug.Log(aiController.gameObject.name + ": FollowState.Update()");
        aiController.UpdateTarget();

        if (aiController.MyTarget != null) {
            //Debug.Log("current agro range is " + parent.MyAggroRange.ToString() + " and current distance to target is " + parent.MyDistanceToTarget);
            // evade if the target is out of aggro range.  In the future this could also be calculated as distance from start point if we would rather use a leash approach
            // temporarily disable leashing.
            /*
            if (Vector3.Distance(aiController.transform.position, aiController.MyStartPosition) > aiController.MyLeashDistance ) {
                aiController.ChangeState(new EvadeState());
                return;
            }
            */
            /*
            if (aiController.MyDistanceToTarget > aiController.MyAggroRange) {
                aiController.ChangeState(new EvadeState());
                return;
            }
            */
            if (aiController.IsTargetInHitBox(aiController.MyTarget)) {
                // they are in the hitbox and we can attack them
                aiController.ChangeState(new AttackState());
            } else {
                //Debug.Log(aiController.gameObject.name + ": FollowTarget: " + aiController.MyTarget.name);
                // if within agro distance but out of hitbox range, move toward target
                aiController.FollowTarget(aiController.MyTarget);
            }
        } else {
            // there is no target so start idling.  should we return to our start position instead?
            aiController.ChangeState(new ReturnState());
        }
    }
}

}