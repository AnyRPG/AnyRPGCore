using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class FollowState : IState {
        private UnitController baseController;

        public void Enter(UnitController baseController) {
            //Debug.Log(baseController.gameObject.name + ".FollowState.Enter()");
            this.baseController = baseController;
            this.baseController.UnitMotor.MovementSpeed = baseController.MovementSpeed;
            MakeFollowDecision();
        }

        public void Exit() {
            baseController.UnitMotor.StopFollowingTarget();
            // stop following target code goes here
        }

        public void Update() {
            //Debug.Log(aiController.gameObject.name + ": FollowState.Update()");
            MakeFollowDecision();
        }

        private void MakeFollowDecision() {
            baseController.UpdateTarget();

            if (baseController.Target != null) {
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

                if (baseController.CanGetValidAttack()) {
                    baseController.ChangeState(new AttackState());
                    return;
                }
                if (baseController.IsTargetInHitBox(baseController.Target)) {
                    // they are in the hitbox and we can attack them
                    baseController.ChangeState(new AttackState());
                } else {
                    //Debug.Log(aiController.gameObject.name + ": FollowTarget: " + aiController.MyTarget.name);
                    // if within agro distance but out of hitbox range, move toward target
                    if (baseController.HasMeleeAttack() || (baseController.GetMinAttackRange() > 0f && (baseController.GetMinAttackRange() < baseController.DistanceToTarget))) {
                        baseController.FollowTarget(baseController.Target, baseController.GetMinAttackRange());
                    } else {
                        baseController.UnitMotor.StopFollowingTarget();
                    }
                }
            } else {
                // there is no target so start idling.  should we return to our start position instead?
                baseController.ChangeState(new ReturnState());
            }
        }
    }

}