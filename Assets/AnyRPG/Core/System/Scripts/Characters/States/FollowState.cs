using UnityEngine;

namespace AnyRPG {
    public class FollowState : IState {
        private UnitController unitController;

        public void Enter(UnitController unitController) {
            //Debug.Log($"FollowState.Enter({unitController.gameObject.name})");

            this.unitController = unitController;
            this.unitController.UnitMotor.MovementSpeed = unitController.MovementSpeed;
            MakeFollowDecision();
        }

        public void Exit() {
            //Debug.Log($"{unitController.gameObject.name}.FollowState.Exit()");

            unitController.UnitMotor.StopFollowingTarget();
        }

        public void Update() {
            //Debug.Log($"{unitController.gameObject.name}: FollowState.Update()");

            MakeFollowDecision();
        }

        private void MakeFollowDecision() {
            //Debug.Log($"{unitController.gameObject.name}: FollowState.MakeFollowDecision()");

            unitController.UpdateTarget();

            if (unitController.Target != null) {
                //Debug.Log($"{unitController.gameObject.name}: FollowState: Target found: {unitController.Target.gameObject.name}");
                // evade if the target is out of aggro range.  In the future this could also be calculated as distance from start point if we would rather use a leash approach

                if (unitController.EnableLeashing == true && unitController.UnderControl == false) {
                    //Debug.Log($"{unitController.gameObject.name}: FollowState: Leash check: distance from start position is {Vector3.Distance(unitController.transform.position, unitController.StartPosition)} and leash distance is {unitController.LeashDistance}");
                    if (Vector3.Distance(unitController.transform.position, unitController.StartPosition) > unitController.LeashDistance) {
                        unitController.ChangeState(new EvadeState());
                        return;
                    }
                }

                if (unitController.CanGetValidAttack()) {
                    unitController.ChangeState(new AttackState());
                    return;
                }
                if (unitController.IsTargetInHitBox(unitController.Target)) {
                    // they are in the hitbox and we can attack them
                    unitController.ChangeState(new AttackState());
                    return;
                } else {
                    // if within agro distance but out of hitbox range, move toward target
                    // do not re-issue the command if we are already moving toward the target
                    // the unit motor will handle the case where the target moves and we need to update the follow position
                    if (unitController.UnitMotor.AttackTarget != unitController.Target) {
                        //Debug.Log($"{unitController.gameObject.name}: AttackTarget: {unitController.UnitMotor.AttackTarget?.gameObject.name} -> {unitController.Target.gameObject.name}");
                        float minAttackRange = unitController.GetMinAttackRange();
                        if (unitController.HasMeleeAttack() || (minAttackRange > 0f && (minAttackRange < unitController.DistanceToTarget))) {
                            unitController.FollowAttackTarget(unitController.Target, minAttackRange);
                        } else {
                            unitController.UnitMotor.StopFollowingTarget();
                        }
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