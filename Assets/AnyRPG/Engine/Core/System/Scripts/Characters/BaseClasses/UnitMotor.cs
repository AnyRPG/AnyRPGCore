using AnyRPG;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class UnitMotor : ConfiguredClass {

        public event System.Action OnMovement = delegate { };

        protected Interactable target;

        protected bool moveToDestination = false;
        protected Vector3 destinationPosition = Vector3.zero;

        // when checking if destinationPosition matches NavMeshAgent destination, add a padding amount to account for destinations that are not precisely on the NavMesh
        protected float navMeshDistancePadding = 0.1f;

        private UnitController unitController;

        // default value meant to be overwritten by a controller (AI/player)
        protected float movementSpeed = 0f;

        private bool frozen = false;

        // the maximum radius from an arbitrary point which the NavMeshAgent will search to find a valid NavMesh location to move to
        private float maxNavMeshSampleRadius = 3f;

        // this current maximum distance to expand a navmesh search outward to
        private float currentMaxSampleRadius = 3f;

        // the amount to expand the search radius each iteration as the navMesh Agent searches for a valid location on a navMesh
        private float navMeshSampleStepSize = 0.5f;

        private bool setMoveDestination = false;

        // last frame number that a navmeshagent destination reset was performed
        private int lastResetFrame = -1;
        // last frame number that a navmeshagent setdestination command was performed
        private int lastCommandFrame = -1;

        // debugging variable to see how long paths are remaining pending for
        private int pathPendingCount = 0;

        private bool useRootMotion = false;

        // game manager references
        private PlayerManager playerManager = null;

        // properties
        public float MovementSpeed { get => movementSpeed; set => movementSpeed = value; }
        public Interactable Target { get => target; }
        public bool Frozen { get => frozen; set => frozen = value; }
        public float NavMeshDistancePadding { get => navMeshDistancePadding; }
        public bool UseRootMotion { get => useRootMotion; set => useRootMotion = value; }

        public UnitMotor(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
        }

        protected void SetMovementSpeed() {
            //Debug.Log(gameObject.name + ".UnitMotor.SetMovementSpeed(): movementSpeed: " + movementSpeed);
            if (movementSpeed == 0) {
                unitController.NavMeshAgent.speed = unitController.MovementSpeed;
            } else {
                //Debug.Log(gameObject.name + ".UnitMotor.Update(): movementSpeed: " + movementSpeed);
                unitController.NavMeshAgent.speed = movementSpeed;
            }

        }

        public void Update() {
            //Debug.Log(gameObject.name + ".UnitMotor.Update(): navhaspath: " + unitController.MyAgent.hasPath + "; isOnNavMesh: " + unitController.MyAgent.isOnNavMesh + "; pathpending: " + unitController.MyAgent.pathPending);
            if (frozen) {
                return;
            }
            //if (unitController?.NavMeshAgent != null && unitController.NavMeshAgent.isActiveAndEnabled) {
            if (unitController?.NavMeshAgent != null && unitController.UseAgent == true) {
                SetMovementSpeed();
            } else {
                //Debug.Log(gameObject.name + ": motor.FixedUpdate(): agent is disabled. Motor will do nothing");
                // unused gravity stuff
                //unitController.MyRigidBody.velocity = new Vector3(0, unitController.MyRigidBody.velocity.y + (-9.81f * Time.deltaTime), 0);
                /*
                Vector3 newRelativeForce = new Vector3(0, -(9.81f * 9.81f * Time.fixedDeltaTime), 0);
                Debug.Log(gameObject.name + ".UnitMotor.FixedUpdate(): newRelativeForce: " + newRelativeForce);
                unitController.MyRigidBody.AddRelativeForce(newRelativeForce);
                */
                return;
            }
            CheckSetMoveDestination();
        }

        protected void CheckSetMoveDestination() {
            //Debug.Log(unitController.gameObject.name + ": UnitMotor.CheckSetMoveDestination()");
            if (unitController?.CharacterUnit?.BaseCharacter?.CharacterStats.IsReviving == true) {
                // cannot issue move command while revive in progress
                return;
            }

            if (setMoveDestination && unitController.NavMeshAgent.pathPending == false && unitController.NavMeshAgent.hasPath == false) {
                //Debug.Log(unitController.gameObject.name + ".UnitMotor.CheckSetMoveDestination(): setMoveDestination: true.  Set move destination: " + destinationPosition + "; current location: " + unitController.transform.position);
                unitController.EnableAgent();
                if (unitController.NavMeshAgent.enabled == true && unitController.NavMeshAgent.isOnNavMesh == true) {
                    moveToDestination = true;
                    //Debug.Log(unitController.gameObject.name + ".UnitMotor.CheckSetMoveDestination(): ISSUING SETDESTINATION: current location: " + unitController.transform.position + "; MyAgent.SetDestination(" + destinationPosition + ") on frame: " + Time.frameCount + " with last reset: " + lastResetFrame + "; pathpending: " + unitController.NavMeshAgent.pathPending + "; pathstatus: " + unitController.NavMeshAgent.pathStatus + "; hasPath: " + unitController.NavMeshAgent.hasPath);
                    unitController.NavMeshAgent.SetDestination(destinationPosition);
                    //Debug.Log(gameObject.name + ": UnitMotor.CheckSetMoveDestination(): AFTER SETDESTINATION: current location: " + transform.position + "; NavMeshAgentDestination: " + unitController.MyAgent.destination + "; destinationPosition: " + destinationPosition + "; frame: " + Time.frameCount + "; last reset: " + lastResetFrame + "; pathpending: " + unitController.MyAgent.pathPending + "; pathstatus: " + unitController.MyAgent.pathStatus + "; hasPath: " + unitController.MyAgent.hasPath);
                    lastCommandFrame = Time.frameCount;
                    setMoveDestination = false;
                }
            }
            
            if (!setMoveDestination) {
                //Debug.Log(unitController.gameObject.name + ".UnitMotor.FixedUpdate(): setMoveDestination: false.  Set move destination: " + destinationPosition + "; current location: " + unitController.transform.position);
            }
            
            if (unitController.NavMeshAgent.pathPending == true) {
                pathPendingCount++;
                //Debug.Log(unitController.gameObject.name + ".UnitMotor.CheckSetMoveDestination(): setMoveDestination: " + setMoveDestination + "; destinationPosition: " + destinationPosition + "; current location: " + unitController.transform.position + "; PATHPENDING: TRUE!!!; status: " + unitController.NavMeshAgent.pathStatus + "; count: " + pathPendingCount);
            } else {
                pathPendingCount = 0;
            }
            if (unitController.NavMeshAgent.hasPath == true) {
                //Debug.Log(unitController.gameObject.name + ".UnitMotor.CheckSetMoveDestination(): setMoveDestination: " + setMoveDestination + "; destinationPosition: " + destinationPosition + "; current location: " + unitController.transform.position + "; HASPATH: TRUE!!!; status: " + unitController.NavMeshAgent.pathStatus);
            }
        }

        public void FixedUpdate() {
            //Debug.Log(gameObject.name + ".UnitMotor.FixedUpdate(). current location: " + transform.position);
            if (frozen) {
                return;
            }
            if (unitController.UseAgent == false) {
                return;
            }
            //if (unitController?.NavMeshAgent != null && unitController.NavMeshAgent.isActiveAndEnabled) {
            if (unitController?.NavMeshAgent != null && frozen == false) {
                //Debug.Log(unitController.gameObject.name + ".UnitMotor.FixedUpdate(): navhaspath: " + unitController.NavMeshAgent.hasPath + "; isOnNavMesh: " + unitController.NavMeshAgent.isOnNavMesh + "; pathpending: " + unitController.NavMeshAgent.pathPending + "; pathstatus: " + unitController.NavMeshAgent.pathStatus + "; remaining: " + unitController.NavMeshAgent.remainingDistance);
                SetMovementSpeed();
                /*
            } else {
                return;
            }
            */
                CheckSetMoveDestination();


                if (target != null) {
                    //Debug.Log(gameObject.name + ": UnitMotor.FixedUpdate() target = " + target.name);
                    if (unitController.IsTargetInHitBox(target)) {
                        StopFollowingTarget();
                    } else {
                        // YES THESE 2 BLOCKS OF CODE ARE COMPLETELY IDENTICAL.  IT'S LIKE THAT SO I CAN ADJUST THE LONG DISTANCE PATHING DIFFERENT IN THE FUTURE.
                        // EG, ENEMY MORE THAN 10 YARDS AWAY CAN HAVE LESS PRECISE UPDATES TO AVOID A LOT OF PATHING CALCULATIONS FOR SOMETHING THAT ONLY NEEDS TO HEAD IN YOUR APPROXIMATE DIRECTION
                        if (Vector3.Distance(target.transform.position, unitController.transform.position) > (unitController.CharacterUnit.HitBoxSize * 2)) {
                            // we are more than 3x the hitbox size away, and should be trying to move toward the targets fuzzy location to prevent movement stutter
                            // this next line is meant to at long distances, move toward the character even if he is off the navmesh and prevent enemy movement stutter chasing a moving target
                            if (Vector3.Distance(CorrectedNavmeshPosition(target.transform.position), unitController.NavMeshAgent.destination) > (unitController.CharacterUnit.HitBoxSize * 1.5) && unitController.NavMeshAgent.pathPending == false) {
                                // the target has moved more than 1 hitbox from our destination position, re-adjust heading
                                if (Time.frameCount != lastResetFrame && Time.frameCount != lastCommandFrame) {
                                    // prevent anything from resetting movement twice in the same frame
                                    MoveToPoint(target.transform.position);
                                }
                            }
                        } else {
                            // they are not in our hitbox yet, but they are closer than 2 meters, we need to move directly to them.  we are likely 0.5 meters out of hitbox range at this point
                            if (Vector3.Distance(CorrectedNavmeshPosition(target.transform.position), unitController.NavMeshAgent.destination) > (unitController.CharacterUnit.HitBoxSize / 2) && unitController.NavMeshAgent.pathPending == false) {
                                if (Time.frameCount != lastResetFrame && Time.frameCount != lastCommandFrame) {
                                    // prevent anything from resetting movement twice in the same frame
                                    MoveToPoint(target.transform.position);
                                }
                            }
                        }
                        FaceTarget(target);
                    }
                } else {
                    //Debug.Log(gameObject.name + ": UnitMotor.FixedUpdate(): TARGET IS NULL!");
                    if (moveToDestination == true && destinationPosition != unitController.NavMeshAgent.destination) {
                        //Debug.Log(gameObject.name + ": UnitMotor.FixedUpdate(): TARGET IS NULL! moveToDestination: true. current location: " + transform.position + "; destinationPosition: " + destinationPosition + "; unitController.MyAgent.destination: " + unitController.MyAgent.destination + "; pathpending: " + unitController.MyAgent.pathPending);
                        float agentDestinationDrift = Vector3.Distance(destinationPosition, unitController.NavMeshAgent.destination);
                        if (agentDestinationDrift >= (unitController.NavMeshAgent.stoppingDistance + navMeshDistancePadding) && destinationPosition != unitController.NavMeshAgent.destination) {
                            MoveToPoint(destinationPosition);
                        } else {
                            //Debug.Log(gameObject.name + ": FixedUpdate() agent.destination: " + unitController.MyAgent.destination + " matches point (within stopping distance): " + destinationPosition + ". Disable moveToDestination boolean");
                            moveToDestination = false;
                        }
                    }
                }
            }

            if (unitController.NavMeshAgent.velocity.sqrMagnitude > 0) {
                BroadcastMovement();
                if (unitController.UnitAnimator != null) {
                    unitController.UnitAnimator.SetMoving(true);
                    unitController.UnitAnimator.SetVelocity((unitController as MonoBehaviour).transform.InverseTransformDirection(unitController.NavMeshAgent.velocity));
                }
            } else {
                if (unitController.UnitAnimator != null) {
                    unitController.UnitAnimator.SetMoving(false);
                    unitController.UnitAnimator.SetVelocity(Vector3.zero);
                }
            }

        }

        public void BroadcastMovement() {
            OnMovement();
            if (unitController != null
                && unitController.CharacterUnit != null
                && unitController.CharacterUnit.BaseCharacter != null
                && unitController.CharacterUnit.BaseCharacter.CharacterAbilityManager != null) {
                unitController.CharacterUnit.BaseCharacter.CharacterAbilityManager.HandleManualMovement();
            }
        }

        public Vector3 CorrectedNavmeshPosition(Vector3 testPosition, float minAttackRange = -1f) {
            //Debug.Log(unitController.gameObject.name + ".UnitMotor.CorrectedNavmeshPosition(" + testPosition + ", " + minAttackRange + ")");

            if (minAttackRange > 0f) {
                currentMaxSampleRadius = minAttackRange;
            }

            NavMeshHit hit;

            // attempt sample at 0.5f radius using current navmesharea.  if this works, we found a valid point on the current navmesh
            if (NavMesh.SamplePosition(testPosition, out hit, 0.5f, NavMesh.AllAreas)) {
                //Debug.Log(gameObject.name + ".UnitMotor.CorrectedNavmeshPosition(): testPosition " + testPosition + " was on current NavMesh near: " + hit.position + ")");
                return hit.position;
            }

            // we did not find a valid point on the current navmesh
            // raycast downward from 10 above the point in case the point was under a steep hill
            RaycastHit raycastHit;
            Vector3 firstTestPosition = unitController.transform.position;
            bool foundMatch = false;
            if (Physics.Raycast(testPosition + new Vector3(0f, 10f, 0f), Vector3.down, out raycastHit, 10f, playerManager.DefaultGroundMask)) {
                firstTestPosition = raycastHit.point;
                foundMatch = true;
                //Debug.Log(unitController.gameObject.name + ".UnitMotor.CorrectedNavmeshPosition(): testPosition " + testPosition + " got hit above on walkable ground: " + firstTestPosition + "; collider: " + raycastHit.collider.name);
            }

            float sampleRadius = 0.5f;
            if (foundMatch) {
                // our raycast found a walkable area.  is it the same area?  check outward for valid point on same area
                while (sampleRadius <= currentMaxSampleRadius) {
                    //Debug.Log(unitController.gameObject.name + ": UnitMotor.FixedUpdate(): testPosition " + firstTestPosition + "; radius: " + sampleRadius);
                    if (NavMesh.SamplePosition(firstTestPosition, out hit, sampleRadius, NavMesh.AllAreas)) {
                        //Debug.Log(unitController.gameObject.name + ": UnitMotor.FixedUpdate(): testPosition " + firstTestPosition + " got hit above on walkable ground at : " + hit.position);
                        return hit.position;
                    }
                    sampleRadius += navMeshSampleStepSize;
                }
                // if we actually got a hit, but did not detect a navmesh, then don't try raycast downward.  the hit was probably on a steep up hill and trying a downcast from our current
                // level would result in a ray inside the hill shooting downward to a potentially inaccessible navmesh below
                //Debug.Log(unitController.gameObject.name + ".UnitMotor.CorrectedNavmeshPosition(): testPosition " + testPosition + "return vector3.zero");
                return unitController.transform.position;
            }

            // now try raycast downward in case we are at the top of a hill
            firstTestPosition = unitController.transform.position;
            foundMatch = false;
            if (Physics.Raycast(testPosition, Vector3.down, out raycastHit, 10f, playerManager.DefaultGroundMask)) {
                firstTestPosition = raycastHit.point;
                foundMatch = true;
                //Debug.Log(gameObject.name + ".UnitMotor.CorrectedNavmeshPosition(): testPosition " + testPosition + " got hit below on walkable ground: " + firstTestPosition + ")");
            }

            sampleRadius = 0.5f;
            if (foundMatch) {
                // our downward raycast found a walkable area.  is it the same area?  check outward for valid point on same area
                while (sampleRadius <= currentMaxSampleRadius) {
                    if (NavMesh.SamplePosition(firstTestPosition, out hit, sampleRadius, NavMesh.AllAreas)) {
                        //Debug.Log(gameObject.name + ": UnitMotor.FixedUpdate(): testPosition " + testPosition + " got hit below on walkable ground at current mask: " + firstTestPosition + ")");
                        return hit.position;
                    }
                    sampleRadius += navMeshSampleStepSize;
                }
            }

            // we didn't find anything on the same navmesharea above the raycast hit, so it's probably a floor of another area.  try just searching outward for any navmesh instead
            // it's possible we are switching areas between navmesh boundaries

            sampleRadius = 0.5f;
            while (sampleRadius <= currentMaxSampleRadius) {
                if (NavMesh.SamplePosition(testPosition, out hit, sampleRadius, NavMesh.AllAreas)) {
                    //Debug.Log(gameObject.name + ": UnitMotor.FixedUpdate(): testPosition " + testPosition + " on NavMesh found closest point on a different navmesh at : " + hit.position + ")");
                    return hit.position;
                }
                sampleRadius += navMeshSampleStepSize;
            }

            // a fallback to the default radius if nothing was found in the above checks in case the radius was accidentally made too small
            sampleRadius = 0.5f;
            while (sampleRadius <= maxNavMeshSampleRadius) {
                if (NavMesh.SamplePosition(testPosition, out hit, sampleRadius, NavMesh.AllAreas)) {
                    //Debug.Log(gameObject.name + ": UnitMotor.FixedUpdate(): testPosition " + testPosition + " on NavMesh found closest point on a different navmesh at : " + hit.position + ")");
                    return hit.position;
                }
                sampleRadius += navMeshSampleStepSize;
            }

            //Debug.Log(unitController.gameObject.name + ".UnitMotor.CorrectedNavmeshPosition(" + testPosition + "): COULD NOT FIND VALID POSITION WITH RADIUS: " + maxNavMeshSampleRadius + ", " + currentMaxSampleRadius + "; minAttackRange: " + minAttackRange + "; RETURNING VECTOR3.ZERO!!!");
            return unitController.transform.position;
        }

        public void FreezeCharacter() {
            //Debug.Log(gameObject.name + "UnitMotor.FreezeCharacter()");
            unitController.DisableAgent();
            frozen = true;
        }

        public void UnFreezeCharacter() {
            //Debug.Log(gameObject.name + "UnitMotor.UnFreezeCharacter()");
            unitController.EnableAgent();
            frozen = false;
        }

        // move toward the position at a normal speed
        public Vector3 MoveToPoint(Vector3 point, float minAttackRange = -1f) {
            //Debug.Log(unitController.gameObject.name + ".UnitMotor.MoveToPoint(" + point + "). current location: " + unitController.transform.position + "; frame: " + Time.frameCount);
            if (frozen) {
                //Debug.Log(gameObject.name + "UnitMotor.MoveToPoint(" + point + "). current location: " + transform.position + "; frame: " + Time.frameCount + "; FROZEN, DOING NOTHING!!!");
                return unitController.transform.position;
            }

            // testing - don't bother with this check since patrolstate is really the only thing that checks for this
            // and returning vector3.zero would result in it just in an endless loop anyway trying to get a new co-ordinate
            /*
            unitController.EnableAgent();
            if (!unitController.NavMeshAgent.enabled) {
                //Debug.Log(gameObject.name + ".UnitMotor.MoveToPoint(" + point + "): agent is disabled.  Will not give move instruction.");
                return Vector3.zero;
            }
            */
            // moving to a point only happens when we click on the ground.  Since we are not tracking a moving target, we can let the agent update the rotation
            unitController.NavMeshAgent.updateRotation = true;
            //Debug.Log(gameObject.name + ".UnitMotor.MoveToPoint(" + point + "): calling unitController.MyAgent.ResetPath()");
            ResetPath();
            destinationPosition = CorrectedNavmeshPosition(point, minAttackRange);
            // set to false for test
            moveToDestination = false;
            setMoveDestination = true;
            // leaving this unset so it gets picked up in the next fixedupdate because navmeshagent doesn't actually reset path until after current frame.
            //unitController.MyAgent.SetDestination(point);

            //Debug.Log(unitController.gameObject.name + "UnitMotor.MoveToPoint(" + point + "). current location: " + unitController.transform.position + "; frame: " + Time.frameCount + "; return: " + destinationPosition);
            return destinationPosition;
        }

        public void MoveToPosition(Vector3 newPosition) {
            //Debug.Log(gameObject.name + ".UnitMotor.MoveToPosition(" + newPosition + ")");
            if (frozen) {
                return;
            }
            unitController.RigidBody.MovePosition(newPosition);
            BroadcastMovement();
        }

        public Vector3 getVelocity() {
            return unitController.NavMeshAgent.velocity;
        }

        public void Move(Vector3 moveDirection, bool isKnockBack = false) {
            //Debug.Log(unitController.gameObject.name + ".UnitMotor.Move(" + moveDirection + "). current position: " + unitController.transform.position);
            if (isKnockBack
                && unitController != null
                && unitController.UnitControllerMode == UnitControllerMode.Player) {
                if (playerManager.PlayerUnitMovementController != null) {
                    playerManager.PlayerUnitMovementController.KnockBack();
                }
            }
            if (frozen) {
                //Debug.Log(gameObject.name + ".UnitMotor.Move(" + moveDirection + "): frozen and doing nothing!!!");
                return;
            }

            if (unitController?.NavMeshAgent != null && unitController.NavMeshAgent.enabled) {
                //Debug.Log(gameObject.name + ".UnitMotor.Move(" + moveDirection + "): moving via navmeshagent");

                //agent.Move(moveDirection);
                ResetPath();
                // TEST DISABLE TO PREVENT GETTING STUCK SIDEWAYS WALKING AROUND CORNERS
                //unitController.MyAgent.updateRotation = false;
                unitController.NavMeshAgent.velocity = moveDirection;
            } else {
                //float currentYVelocity = moveDirection.y != 0 ? moveDirection.y : unitController.MyRigidBody.velocity.y;
                //Debug.Log("characterUnit.yVelocity is " + currentYVelocity);
                //Vector3 newMoveDirection = new Vector3(moveDirection.x, currentYVelocity, moveDirection.z);
                //Debug.Log(unitController.gameObject.name + ".UnitMotor.Move() newMoveDirection: " + moveDirection + "; unitController.MyRigidBody.constraints: " + unitController.RigidBody.constraints);
                unitController.RigidBody.velocity = moveDirection;
                //unitController.MyRigidBody.MovePosition(transform.position + moveDirection);
                //unitController.MyRigidBody.AddForce(moveDirection, ForceMode.VelocityChange);
            }
            if (moveDirection != Vector3.zero) {
                BroadcastMovement();
            }
        }

        public void Jump(float jumpSpeed) {
            //Debug.Log(gameObject.name + ".UnitMotor.Jump(" + jumpSpeed + "). current position: " + transform.position);
            if (frozen) {
                return;
            }
            unitController.RigidBody.AddRelativeForce(new Vector3(0, jumpSpeed, 0), ForceMode.VelocityChange);
        }

        public void RotateTowardsTarget(Vector3 targetPosition, float rotationSpeed) {
            //Debug.Log("RotateTowardsMovementTarget()");
            if (frozen) {
                return;
            }
            Quaternion targetRotation = Quaternion.LookRotation(targetPosition - new Vector3(unitController.transform.position.x, 0, unitController.transform.position.z));
            unitController.transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(unitController.transform.eulerAngles.y, targetRotation.eulerAngles.y, (rotationSpeed * Time.deltaTime) * rotationSpeed);
        }

        public void BeginFaceSouthEast() {
            //Debug.Log(gameObject.name + ".UnitMotor.BeginFaceSouthEast()");
            //RotateToward((new Vector3(1, 0, -1)).normalized);
            Rotate((new Vector3(1, 0, -1)).normalized);
            //Rotate((new Vector3(-1, 0, 1)).normalized);
        }

        public void RotateToward(Vector3 rotateDirection) {
            //Debug.Log(unitController.gameObject.name + ".UnitMotor.RotateToward(): " + rotateDirection);
            if (frozen) {
                return;
            }
            if (unitController.NavMeshAgent.enabled) {
                //Debug.Log("nav mesh agent is enabled");
                //Debug.Log(gameObject.name + ".UnitMotor.RotateToward(): " + rotateDirection);
                ResetPath();
                unitController.NavMeshAgent.updateRotation = true;
                unitController.NavMeshAgent.velocity = rotateDirection;
            } else {
                //Debug.Log("nav mesh agent is disabled");
                unitController.RigidBody.velocity = rotateDirection;
            }
        }

        public void Rotate(Vector3 rotateDirection) {
            //Debug.Log(unitController.gameObject.name + ".UnitMotor.Rotate(): " + rotateDirection);
            if (frozen) {
                return;
            }
            //(characterUnit as MonoBehaviour).transform.Rotate(rotateDirection);
            unitController.transform.Rotate(rotateDirection);
        }


        public void FollowTarget(Interactable newTarget, float minAttackRange = -1f) {
            //Debug.Log(unitController.gameObject.name + ".UnitMotor.FollowTarget(" + (newTarget == null ? "null" : newTarget.name) + ", " + minAttackRange + ")");
            if (frozen) {
                return;
            }
            unitController.EnableAgent();
            unitController.NavMeshAgent.stoppingDistance = 0.2f;
            //agent.stoppingDistance = myStats.hitBox;
            // moving to a target happens when we click on an interactable.  Since it might be moving, we will manually update the rotation every frame
            // TEST DISABLE THIS TO PREVENT WALKING SIDEWAYS AROUND CORNERS
            //unitController.MyAgent.updateRotation = false;
            Interactable oldTarget = target;
            target = newTarget;
            if (oldTarget == null || (minAttackRange > 0f && currentMaxSampleRadius != minAttackRange)) {
                //Debug.Log(gameObject.name + ".UnitMotor.FollowTarget(" + (target == null ? "null" : target.name) + ", " + minAttackRange + "): issuing movetopoint. currentradius: " + currentMaxSampleRadius + "; minattack: " + minAttackRange);
                MoveToPoint(target.transform.position, minAttackRange);
            } else {
                //Debug.Log(gameObject.name + ".UnitMotor.FollowTarget(" + (target == null ? "null" : target.name) + ", " + minAttackRange + "): doing nothing.  oldtarget is not null");
            }

        }

        public void StopFollowingTarget() {
            //Debug.Log(unitController.gameObject.name + ".UnitMotor.StopFollowingTarget()");
            target = null;
            if (frozen) {
                return;
            }
            if (unitController?.NavMeshAgent == null) {
                return;
            }
            if (unitController.NavMeshAgent.isActiveAndEnabled) {
                //Debug.Log(unitController.gameObject.name + ".UnitMotor.StopFollowingTarget()");
                unitController.NavMeshAgent.stoppingDistance = 0.2f;
                unitController.NavMeshAgent.updateRotation = true;
            }
            ResetPath(true);
        }

        public void FaceTarget(Interactable newTarget) {
            //Debug.Log(unitController.gameObject.name + ".UnitMotor.FaceTarget(" + newTarget.name + ")");
            if (frozen) {
                return;
            }
            Vector3 direction = (newTarget.transform.position - unitController.transform.position).normalized;

            // prevent turning updward
            direction = new Vector3(direction.x, 0f, direction.z);

            //Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
            if (unitController.NavMeshAgent.enabled) {
                unitController.NavMeshAgent.updateRotation = false;
            }
            //Debug.Log(unitController.gameObject.name + ".UnitMotor.FaceTarget(" + newTarget.name + "): direction: " + direction + "; current: " + unitController.transform.forward);
            if (direction != Vector3.zero) {
                unitController.transform.forward = direction;
            }
            if (unitController.NavMeshAgent.enabled) {
                unitController.NavMeshAgent.updateRotation = true;
            }
        }

        /*
        public void StartNavAgent() {
            //Debug.Log(gameObject.name + ".UnitMotor.StartNavAgent()");
            if (!unitController.NavMeshAgent.enabled) {
                unitController.EnableAgent();
                unitController.RigidBody.isKinematic = true;
            }
        }
        */

        public void ResetPath(bool forceStop = false) {
            //Debug.Log(unitController.gameObject.name + ".UnitMotor.ResetPath() in frame: " + Time.frameCount);
            moveToDestination = false;
            setMoveDestination = false;
            if (unitController.NavMeshAgent.enabled == true) {
                //Debug.Log(gameObject.name + ".UnitMotor.FixedUpdate(): navhaspath: " + unitController.MyAgent.hasPath + "; isOnNavMesh: " + unitController.MyAgent.isOnNavMesh + "; pathpending: " + unitController.MyAgent.pathPending);
                if (unitController.NavMeshAgent.isOnNavMesh == true) {
                    //Debug.Log(unitController.gameObject.name + ".UnitMotor.ResetPath() setting isStopped = true in frame: " + Time.frameCount);
                    unitController.NavMeshAgent.ResetPath();
                    if (forceStop) {
                        unitController.NavMeshAgent.isStopped = true;
                        unitController.NavMeshAgent.velocity = Vector3.zero;
                        unitController.ResetApparentVelocity();
                        unitController.DisableAgent();
                    }
                }
                lastResetFrame = Time.frameCount;
                //Debug.Log(gameObject.name + ": UnitMotor.FixedUpdate(): AFTER RESETPATH: current location: " + transform.position + "; NavMeshAgentDestination: " + unitController.MyAgent.destination + "; destinationPosition: " + destinationPosition + "; frame: " + Time.frameCount + "; last reset: " + lastResetFrame + "; pathpending: " + unitController.MyAgent.pathPending + "; pathstatus: " + unitController.MyAgent.pathStatus + "; hasPath: " + unitController.MyAgent.hasPath);
                //Debug.Log(gameObject.name + ".UnitMotor.FixedUpdate(): after reset: navhaspath: " + unitController.MyAgent.hasPath + "; isOnNavMesh: " + unitController.MyAgent.isOnNavMesh + "; pathpending: " + unitController.MyAgent.pathPending);
            }
        }

        public void ReceiveAnimatorMovement() {
            //Debug.Log(unitController.gameObject.name + ".UnitMotor.ReceiveAnimatorMovement(): " + unitController.UnitAnimator.Animator.deltaPosition.x + " " + unitController.UnitAnimator.Animator.deltaPosition.y + " " + unitController.UnitAnimator.Animator.deltaPosition.z);
            if (UseRootMotion) {
                // will this work for navmeshAgents?  do we need to warp them?
                unitController.transform.position += unitController.UnitAnimator.Animator.deltaPosition;
                //Debug.Log(unitController.gameObject.name + ".UnitMotor.ReceiveAnimatorMovement() userootmotion is true, apply position: " + unitController.UnitAnimator.Animator.deltaPosition.x + " " + unitController.UnitAnimator.Animator.deltaPosition.y + " " + unitController.UnitAnimator.Animator.deltaPosition.z);
            }
        }
    }

}