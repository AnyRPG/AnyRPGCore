using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class FollowState : IState {
        private UnitController unitController;

        public void Enter(UnitController unitController) {
            //Debug.Log($"{baseController.gameObject.name}.FollowState.Enter()");

            this.unitController = unitController;
            this.unitController.UnitMotor.MovementSpeed = unitController.MovementSpeed;
            MakeFollowDecision();
        }

        public void Exit() {
            //Debug.Log(baseController.gameObject.name + ".FollowState.Exit()");
            unitController.UnitMotor.StopFollowingTarget();
            // stop following target code goes here
        }

        public void Update() {
            //Debug.Log(aiController.gameObject.name + ": FollowState.Update()");
            MakeFollowDecision();
        }

        private void MakeFollowDecision() {
            unitController.UpdateTarget();

            if (unitController.Target != null) {
                //Debug.Log("current agro range is " + baseController.Target.name + " and current distance to target is " + baseController.DistanceToTarget);
                // evade if the target is out of aggro range.  In the future this could also be calculated as distance from start point if we would rather use a leash approach

                if (unitController.EnableLeashing == true && unitController.UnderControl == false) {
                    if (Vector3.Distance(unitController.transform.position, unitController.StartPosition) > unitController.LeashDistance) {
                        unitController.ChangeState(new EvadeState());
                        return;
                    }
                }

                /*
                if (aiController.MyDistanceToTarget > aiController.MyAggroRange) {
                    aiController.ChangeState(new EvadeState());
                    return;
                }
                */

                if (unitController.CanGetValidAttack()) {
                    unitController.ChangeState(new AttackState());
                    return;
                }
                if (unitController.IsTargetInHitBox(unitController.Target)) {
                    // they are in the hitbox and we can attack them
                    unitController.ChangeState(new AttackState());
                    return;
                } else {
                    //Debug.Log(aiController.gameObject.name + ": FollowTarget: " + aiController.MyTarget.name);
                    // if within agro distance but out of hitbox range, move toward target
                    if (unitController.HasMeleeAttack() || (unitController.GetMinAttackRange() > 0f && (unitController.GetMinAttackRange() < unitController.DistanceToTarget))) {
                        unitController.FollowTarget(unitController.Target, unitController.GetMinAttackRange());
                    } else {
                        unitController.UnitMotor.StopFollowingTarget();
                    }
                }
            } else {
                // there is no target so start idling.  should we return to our start position instead?
                unitController.ChangeState(new ReturnState());
                return;
            }
        }
    }

}