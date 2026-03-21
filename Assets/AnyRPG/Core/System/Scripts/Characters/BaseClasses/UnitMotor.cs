using AnyRPG;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class UnitMotor : ConfiguredClass {

        public event System.Action OnMovement = delegate { };

        protected Interactable target = null;
        protected Interactable attackTarget = null;
        protected Interactable interactionTarget = null;
        protected float attackRange = 0f;

        // if true, the navmeshAgent will be directed to this destination on the next update
        private bool setMoveDestination = false;

        // if true, the agent was directed to set this destination
        protected bool moveToDestination = false;
        
        // the spot on the ground the agent is moving toward
        protected Vector3 destinationPosition = Vector3.zero;

        // if true, the agent is currently moving toward a spot on the ground
        protected bool hasDestinationPosition = false;

        protected Transform interactionTransform = null;

        protected Vector3 lastTargetPosition = Vector3.zero;

        protected RaycastHit centerDownHitInfo;

        // when checking if destinationPosition matches NavMeshAgent destination, add a padding amount to account for destinations that are not precisely on the NavMesh
        protected float navMeshDistancePadding = 0.1f;

        private UnitController unitController;
        private IMovementBody movementBody;

        // default value meant to be overwritten by a controller (AI/player)
        protected float movementSpeed = 0f;

        private bool frozen = false;

        // the maximum radius from an arbitrary point which the NavMeshAgent will search to find a valid NavMesh location to move to
        private float maxNavMeshSampleRadius = 3f;

        // this current maximum distance to expand a navmesh search outward to
        private float currentMaxSampleRadius = 3f;

        // the amount to expand the search radius each iteration as the navMesh Agent searches for a valid location on a navMesh
        private float navMeshSampleStepSize = 0.5f;

        // last frame number that a navmeshagent destination reset was performed
        private int lastResetFrame = -1;
        // last frame number that a navmeshagent setdestination command was performed
        private int lastCommandFrame = -1;

        // debugging variable to see how long paths are remaining pending for
        private int pathPendingCount = 0;

        private bool useRootMotion = false;

        LayerMask defaultLayerMask;
        float capsuleRadius;

        // game manager references
        private InteractionManagerServer interactionManagerServer = null;

        // properties
        public float MovementSpeed { get => movementSpeed; set => movementSpeed = value; }
        public Interactable Target { get => target; }
        public bool Frozen { get => frozen; set => frozen = value; }
        public float NavMeshDistancePadding { get => navMeshDistancePadding; }
        public bool UseRootMotion { get => useRootMotion; set => useRootMotion = value; }
        public Interactable InteractionTarget { get => interactionTarget; }
        public Interactable AttackTarget { get => attackTarget; }
        public Transform InteractionTransform { get => interactionTransform; set => interactionTransform = value; }
        public IMovementBody MovementBody { get => movementBody; }
        public bool SetMoveDestination { get => setMoveDestination; }
        public bool HasDestinationPosition { get => hasDestinationPosition; set => hasDestinationPosition = value; }

        public UnitMotor(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
            defaultLayerMask = 1 << LayerMask.NameToLayer("Default");
            capsuleRadius = unitController.GetComponent<CapsuleCollider>().radius;
            // setting the standard movement body for now so there is something to refer to during startup
            // this can be replaced by network units that use prediction
            SetMovementBody(new StandardMovementBody(unitController.RigidBody));
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            interactionManagerServer = systemGameManager.InteractionManagerServer;
        }

        public void SetMovementBody(IMovementBody movementBody) {
            this.movementBody = movementBody;
        }

        protected void SetMovementSpeed() {
            //Debug.Log($"{gameObject.name}.UnitMotor.SetMovementSpeed(): movementSpeed: " + movementSpeed);
            if (movementSpeed == 0) {
                unitController.NavMeshAgent.speed = unitController.MovementSpeed;
            } else {
                //Debug.Log($"{gameObject.name}.UnitMotor.Update(): movementSpeed: " + movementSpeed);
                unitController.NavMeshAgent.speed = movementSpeed;
            }

        }

        public void Tick() {
            //Debug.Log($"{gameObject.name}.UnitMotor.Update(): navhaspath: " + unitController.MyAgent.hasPath + "; isOnNavMesh: " + unitController.MyAgent.isOnNavMesh + "; pathpending: " + unitController.MyAgent.pathPending);
            if (frozen) {
                return;
            }
            //if (unitController?.NavMeshAgent != null && unitController.NavMeshAgent.isActiveAndEnabled) {
            if (unitController?.NavMeshAgent != null && unitController.UseAgent == true) {
                SetMovementSpeed();
            } else {
                return;
            }
            CheckSetMoveDestination();
        }

        protected void CheckSetMoveDestination() {
            //Debug.Log($"{unitController.gameObject.name}: UnitMotor.CheckSetMoveDestination()");
            if (unitController.CharacterStats.IsReviving == true) {
                // cannot issue move command while revive in progress
                return;
            }

            if (setMoveDestination && unitController.NavMeshAgent.pathPending == false && unitController.NavMeshAgent.hasPath == false) {
                unitController.EnableAgent();
                if (unitController.NavMeshAgent.enabled == true && unitController.NavMeshAgent.isOnNavMesh == true) {
                    moveToDestination = true;
                    //Debug.Log($"{unitController.gameObject.name}.UnitMotor.CheckSetMoveDestination(): ISSUING SETDESTINATION: current location: {unitController.transform.position}; MyAgent.SetDestination({destinationPosition}) on frame: {Time.frameCount} with last reset: {lastResetFrame}; pathpending: {unitController.NavMeshAgent.pathPending}; pathstatus: {unitController.NavMeshAgent.pathStatus}; hasPath: {unitController.NavMeshAgent.hasPath}");
                    unitController.NavMeshAgent.SetDestination(destinationPosition);
                    //Debug.Log($"{gameObject.name}: UnitMotor.CheckSetMoveDestination(): AFTER SETDESTINATION: current location: {transform.position}; NavMeshAgentDestination: " + unitController.MyAgent.destination + "; destinationPosition: " + destinationPosition + "; frame: " + Time.frameCount + "; last reset: " + lastResetFrame + "; pathpending: {unitController.MyAgent.pathPending}; pathstatus: {unitController.MyAgent.pathStatus}; hasPath: {unitController.MyAgent.hasPath}");
                    lastCommandFrame = Time.frameCount;
                    setMoveDestination = false;
                }
            }
            
            if (!setMoveDestination) {
                //Debug.Log($"{unitController.gameObject.name}.UnitMotor.FixedUpdate(): setMoveDestination: false.  Set move destination: {destinationPosition}; current location: {unitController.transform.position}");
            }
            
            if (unitController.NavMeshAgent.pathPending == true) {
                pathPendingCount++;
                //Debug.Log($"{unitController.gameObject.name}.UnitMotor.CheckSetMoveDestination(): setMoveDestination: {setMoveDestination}; destinationPosition: {destinationPosition}; current location: {unitController.transform.position}; PATHPENDING: TRUE!!!; status: {unitController.NavMeshAgent.pathStatus}; count: {pathPendingCount}");
            } else {
                pathPendingCount = 0;
            }
            if (unitController.NavMeshAgent.hasPath == true) {
                //Debug.Log($"{unitController.gameObject.name}.UnitMotor.CheckSetMoveDestination(): setMoveDestination: {setMoveDestination}; destinationPosition: {destinationPosition}; current location: {unitController.transform.position}; HASPATH: TRUE!!!; status: {unitController.NavMeshAgent.pathStatus}");
            }
        }

        public void FixedTick() {
            //Debug.Log($"{gameObject.name}.UnitMotor.FixedUpdate(). current location: {transform.position}");
            if (frozen) {
                return;
            }
            if (unitController.UseAgent == false) {
                return;
            }

            if (unitController.NavMeshAgent.enabled == true
                && unitController.NavMeshAgent.isOnNavMesh == true
                && hasDestinationPosition
                && setMoveDestination == false
                && !unitController.NavMeshAgent.pathPending) {
                if (unitController.NavMeshAgent.remainingDistance <= unitController.NavMeshAgent.stoppingDistance) {
                    if (!unitController.NavMeshAgent.hasPath || unitController.NavMeshAgent.velocity.sqrMagnitude == 0f) {
                        //Debug.Log($"{unitController.gameObject.name}.UnitMotor.FixedUpdate(): REACHED DESTINATION: {destinationPosition}; current location: {unitController.transform.position}; frame: {Time.frameCount}; last reset: {lastResetFrame}; ");
                        hasDestinationPosition = false;

                        // cache the interaction variables because they will be reset in StopFollowingTarget()
                        Transform cachedInteractionTransform = interactionTransform;
                        Interactable cachedInteractionTarget = interactionTarget;
                        // face the prefered direction of the interaction
                        if (interactionTransform != null) {
                            FaceDirection(interactionTransform.forward);
                        }
                        
                        unitController.UnitEventController.NotifyOnReachDestination();

                        // if this is a mount, we need to let the rider know we reached the destination so they can turn off the movement target
                        if (unitController.UnitControllerMode == UnitControllerMode.Mount) {
                            unitController.RiderUnitController.UnitEventController.NotifyOnReachDestination();
                        }
                        if (unitController.UnitControllerMode == UnitControllerMode.Player || unitController.UnitControllerMode == UnitControllerMode.Mount) {
                            unitController.UnitMovementController.ChangeState(CharacterMovementState.Idle, false);
                        }

                        // clear variables related to following an interaction target since we have reached the destination and are now interacting
                        StopFollowingTarget();

                        if (cachedInteractionTransform != null) {
                            if (unitController.UnitControllerMode == UnitControllerMode.Mount) {
                                UnitController riderUnitController = unitController.RiderUnitController;
                                riderUnitController.CancelMountEffects();
                                cachedInteractionTarget.InteractableTriggerEnter(riderUnitController.Collider);
                                interactionManagerServer.InteractWithInteractable(riderUnitController, cachedInteractionTarget);
                                return;
                            } else {
                                interactionManagerServer.InteractWithInteractable(unitController, cachedInteractionTarget);
                            }
                        }

                    }
                }
            }
            //if (unitController?.NavMeshAgent != null && unitController.NavMeshAgent.isActiveAndEnabled) {
            if (unitController.NavMeshAgent != null && frozen == false) {
                //Debug.Log($"{unitController.gameObject.name}.UnitMotor.FixedUpdate(): navhaspath: " + unitController.NavMeshAgent.hasPath + "; isOnNavMesh: " + unitController.NavMeshAgent.isOnNavMesh + "; pathpending: " + unitController.NavMeshAgent.pathPending + "; pathstatus: " + unitController.NavMeshAgent.pathStatus + "; remaining: " + unitController.NavMeshAgent.remainingDistance);
                SetMovementSpeed();
                CheckSetMoveDestination();

                if (target != null) {
                    FollowTargetTick();
                } else {
                    if (moveToDestination == true) {
                        FollowNullTargetTick();
                    }
                }
            }

            if ((unitController.UnitControllerMode == UnitControllerMode.Player || unitController.UnitControllerMode == UnitControllerMode.Mount)
                && (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true) == false) {
                // players and mounts handle animation updates in the UnitMovementController, so skip the rest of this method which is meant for AI units
                // unless this is a local game or happening on the server, because in that case, we have access to NavmeshAgent.Velocity instead of the reconciled version
                return;
            }

            if (unitController.NavMeshAgent.velocity.sqrMagnitude > 0) {
                BroadcastMovement();
                if (unitController.UnitAnimator != null) {
                    unitController.UnitAnimator.SetMoving(true);
                    unitController.UnitAnimator.SetVelocityFromLocal(unitController.transform.InverseTransformDirection(unitController.NavMeshAgent.velocity), unitController.UnitProfile.UnitPrefabProps.ForceRotateModelMode);
                }
            } else {
                if (unitController.UnitAnimator != null) {
                    unitController.UnitAnimator.SetMoving(false);
                    unitController.UnitAnimator.SetVelocityFromLocal(Vector3.zero, unitController.UnitProfile.UnitPrefabProps.ForceRotateModelMode);
                }
            }
        }

        private void FollowTargetTick() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMotor.FollowTargetTick() targetPosition: {target.transform.position} lastTargetPosition: {lastTargetPosition}");

            //if (attackTarget != null && unitController.IsTargetInHitBox(target)) {
            if (attackTarget != null && Vector3.Distance(movementBody.GetPosition(), attackTarget.transform.position) <= attackRange) {
                StopFollowingTarget();
                return;
            }

            if (target.transform.position != lastTargetPosition) {
                // YES THESE 2 BLOCKS OF CODE ARE COMPLETELY IDENTICAL.  IT'S LIKE THAT SO I CAN ADJUST THE LONG DISTANCE PATHING DIFFERENT IN THE FUTURE.
                // EG, ENEMY MORE THAN 10 YARDS AWAY CAN HAVE LESS PRECISE UPDATES TO AVOID A LOT OF PATHING CALCULATIONS FOR SOMETHING THAT ONLY NEEDS TO HEAD IN YOUR APPROXIMATE DIRECTION
                //Debug.Log($"{unitController.gameObject.name}.UnitMotor.FollowTargetTick(): target has moved.  current location: {unitController.transform.position}; target position: {target.transform.position}; last target position: {lastTargetPosition}");
                if (Vector3.Distance(target.transform.position, movementBody.GetPosition()) > (unitController.CharacterUnit.HitBoxSize * 2)) {
                    // we are more than 3x the hitbox size away, and should be trying to move toward the targets fuzzy location to prevent movement stutter
                    // this next line is meant to at long distances, move toward the character even if he is off the navmesh and prevent enemy movement stutter chasing a moving target
                    //Debug.Log($"{unitController.gameObject.name}.UnitMotor.FollowTargetTick(): target is far away.  Using corrected navmesh position to prevent stutter.  current location: {unitController.transform.position}; target position: {target.transform.position}; last target position: {lastTargetPosition}");
                    if (Vector3.Distance(CorrectedNavmeshPosition(target.transform.position), unitController.NavMeshAgent.destination) > (unitController.CharacterUnit.HitBoxSize * 1.5) && unitController.NavMeshAgent.pathPending == false) {
                        // the target has moved more than 1 hitbox from our destination position, re-adjust heading
                        //Debug.Log($"{unitController.gameObject.name}.UnitMotor.FollowTargetTick(): target is far away and has moved more than 1.5 hitbox from current destination.  Re-issuing move command.  current location: {unitController.transform.position}; target position: {target.transform.position}; last target position: {lastTargetPosition}; current destination: {unitController.NavMeshAgent.destination}");
                        if (Time.frameCount != lastResetFrame && Time.frameCount != lastCommandFrame) {
                            // prevent anything from resetting movement twice in the same frame
                            MoveToPoint(target.transform.position);
                        }/* else {
                            //Debug.Log($"{unitController.gameObject.name}.UnitMotor.FollowTargetTick(): target is far away and has moved more than 1.5 hitbox from current destination, but we already issued a move command this frame or reset the path, so skipping issuing another command.  current location: {unitController.transform.position}; target position: {target.transform.position}; last target position: {lastTargetPosition}; current destination: {unitController.NavMeshAgent.destination}; frame: {Time.frameCount}; last reset: {lastResetFrame}; last command: {lastCommandFrame}");
                        }*/
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
                lastTargetPosition = target.transform.position;
            }
            //FaceTarget(target);

        }

        private void FollowNullTargetTick() {
            //Debug.Log($"{gameObject.name}: UnitMotor.FixedUpdate(): TARGET IS NULL!");
            if (moveToDestination == false) {
                return;
            }
            if (destinationPosition != unitController.NavMeshAgent.destination) {
                //Debug.Log($"{gameObject.name}: UnitMotor.FixedUpdate(): TARGET IS NULL! moveToDestination: true. current location: " + transform.position + "; destinationPosition: " + destinationPosition + "; unitController.MyAgent.destination: " + unitController.MyAgent.destination + "; pathpending: " + unitController.MyAgent.pathPending);
                float agentDestinationDrift = Vector3.Distance(destinationPosition, unitController.NavMeshAgent.destination);
                if (agentDestinationDrift >= (unitController.NavMeshAgent.stoppingDistance + navMeshDistancePadding) && destinationPosition != unitController.NavMeshAgent.destination) {
                    MoveToPoint(destinationPosition);
                } else {
                    //Debug.Log($"{gameObject.name}: FixedUpdate() agent.destination: " + unitController.MyAgent.destination + " matches point (within stopping distance): " + destinationPosition + ". Disable moveToDestination boolean");
                    moveToDestination = false;
                }
            }
        }

        public void BroadcastMovement() {
            OnMovement();
            if (unitController != null
                && unitController.CharacterUnit != null
                && unitController.BaseCharacter != null
                && unitController.CharacterAbilityManager != null) {
                unitController.CharacterAbilityManager.HandleManualMovement();
                unitController.UnitActionManager.HandleManualMovement();
            }
        }

        public Vector3 CorrectedNavmeshPosition(Vector3 testPosition, float minAttackRange = -1f) {
            //Debug.Log($"{unitController.gameObject.name}.UnitMotor.CorrectedNavmeshPosition(testPosition: {testPosition}, minAttackRange: {minAttackRange}) currentMaxSampleRadius: {currentMaxSampleRadius}");

            if (minAttackRange > 0f) {
                currentMaxSampleRadius = minAttackRange;
            }

            NavMeshHit hit;

            // attempt sample at 0.5f radius using current navmesharea.  if this works, we found a valid point on the current navmesh
            if (NavMesh.SamplePosition(testPosition, out hit, 0.1f, NavMesh.AllAreas)) {
                //Debug.Log($"{unitController.gameObject.name}.UnitMotor.CorrectedNavmeshPosition(): testPosition {testPosition} was within 0.1f of NavMesh near: {hit.position})");
                return hit.position;
            }

            // repeat the above sample in steps of 0.1f to current max sample radius
            for (float positionOffset = 0.1f; positionOffset <= currentMaxSampleRadius; positionOffset += navMeshSampleStepSize) {
                //Debug.Log($"{unitController.gameObject.name}.UnitMotor.CorrectedNavmeshPosition(): trying again with position offset: {positionOffset}");
                if (NavMesh.SamplePosition(testPosition, out hit, positionOffset, NavMesh.AllAreas)) {
                    //Debug.Log($"{unitController.gameObject.name}.UnitMotor.CorrectedNavmeshPosition(): testPosition {testPosition} was not within 0.1f of NavMesh, but found a point on the NavMesh near: {hit.position})");
                    return hit.position;
                }
            }

            // repeat the above sample in steps of 0.1f to current max sample radius, with the testposition moving closer to the character by the step size each time
            for (float positionOffset = 0.1f; positionOffset <= currentMaxSampleRadius; positionOffset += navMeshSampleStepSize) {
                Vector3 directionToCharacter = (movementBody.GetPosition() - testPosition).normalized;
                Vector3 samplePosition = testPosition + (directionToCharacter * positionOffset);
                if (NavMesh.SamplePosition(samplePosition, out hit, 0.1f, NavMesh.AllAreas)) {
                    //Debug.Log($"{unitController.gameObject.name}.UnitMotor.CorrectedNavmeshPosition(): testPosition {testPosition} was not on current NavMesh, but found a point on the NavMesh near: {hit.position})");
                    return hit.position;
                }
            }

            // we did not find a valid point on the current navmesh
            // use just the default layer when doing the raycast to prevent hitting things that are not actually the ground and getting stuck trying to find a navmesh on something we can't actually walk on
            // get a layermask for the default layer
            RaycastHit raycastHit;
            Vector3 firstTestPosition = movementBody.GetPosition();
            bool foundMatch = false;
            float sampleRadius = 0.5f;

            // try raycast downward in case we are at the top of a hill or object is floating or on a wall
            // doing this first because the raycast later that starts from above can find a walkable roof that cannot be navigated to
            firstTestPosition = movementBody.GetPosition();
            foundMatch = false;
            if (unitController.PhysicsScene.Raycast(testPosition, Vector3.down, out raycastHit, 10f, defaultLayerMask)) {
                firstTestPosition = raycastHit.point;
                foundMatch = true;
                //Debug.Log($"{unitController.gameObject.name}.UnitMotor.CorrectedNavmeshPosition(): testPosition {testPosition} got hit below on walkable ground: {firstTestPosition})");
            }

            sampleRadius = 0.5f;
            if (foundMatch) {
                // our downward raycast found a walkable area.  is it the same area?  check outward for valid point on same area
                while (sampleRadius <= currentMaxSampleRadius) {
                    if (NavMesh.SamplePosition(firstTestPosition, out hit, sampleRadius, NavMesh.AllAreas)) {
                        //Debug.Log($"{unitController.gameObject.name}UnitMotor.CorrectedNavmeshPosition(): testPosition {testPosition} got hit below on walkable ground at current mask: {firstTestPosition} hit: {hit.position})");
                        return hit.position;
                    }
                    sampleRadius += navMeshSampleStepSize;
                }
            }

            // raycast downward from 10 above the point in case the point was under a steep hill
            if (unitController.PhysicsScene.Raycast(testPosition + new Vector3(0f, 10f, 0f), Vector3.down, out raycastHit, 10f, defaultLayerMask)) {
                firstTestPosition = raycastHit.point;
                foundMatch = true;
                //Debug.Log($"{unitController.gameObject.name}.UnitMotor.CorrectedNavmeshPosition(): testPosition {testPosition} got hit above on walkable ground: {firstTestPosition}; collider: {raycastHit.collider.name}");
            }

            if (foundMatch) {
                // our raycast found a walkable area.  is it the same area?  check outward for valid point on same area
                while (sampleRadius <= currentMaxSampleRadius) {
                    //Debug.Log($"{unitController.gameObject.name}: UnitMotor.FixedUpdate(): testPosition " + firstTestPosition + "; radius: " + sampleRadius);
                    if (NavMesh.SamplePosition(firstTestPosition, out hit, sampleRadius, NavMesh.AllAreas)) {
                        //Debug.Log($"{unitController.gameObject.name}.UnitMotor.CorrectedNavmeshPosition() testPosition {firstTestPosition} got hit above on walkable ground at {hit.position}");
                        return hit.position;
                    }
                    sampleRadius += navMeshSampleStepSize;
                }
                // if we actually got a hit, but did not detect a navmesh, then don't try raycast downward.  the hit was probably on a steep up hill and trying a downcast from our current
                // level would result in a ray inside the hill shooting downward to a potentially inaccessible navmesh below
                //Debug.Log($"{unitController.gameObject.name}.UnitMotor.CorrectedNavmeshPosition(): testPosition " + testPosition + "return vector3.zero");
                return movementBody.GetPosition();
            }

            // we didn't find anything on the same navmesharea above the raycast hit  try just searching outward for any navmesh instead
            // it's possible we are switching areas between navmesh boundaries

            sampleRadius = 0.5f;
            while (sampleRadius <= currentMaxSampleRadius) {
                if (NavMesh.SamplePosition(testPosition, out hit, sampleRadius, NavMesh.AllAreas)) {
                    //Debug.Log($"{unitController.gameObject.name}.UnitMotor.FixedUpdate(): testPosition {testPosition} on NavMesh found closest point at {hit.position}");
                    return hit.position;
                }
                sampleRadius += navMeshSampleStepSize;
            }

            // a fallback to the default radius if nothing was found in the above checks in case the radius was accidentally made too small
            sampleRadius = 0.5f;
            while (sampleRadius <= maxNavMeshSampleRadius) {
                if (NavMesh.SamplePosition(testPosition, out hit, sampleRadius, NavMesh.AllAreas)) {
                    //Debug.Log($"{unitController.gameObject.name}: UnitMotor.FixedUpdate(): testPosition {testPosition} on NavMesh found closest point on a different navmesh at : {hit.position}");
                    return hit.position;
                }
                sampleRadius += navMeshSampleStepSize;
            }

            //Debug.Log($"{unitController.gameObject.name}.UnitMotor.CorrectedNavmeshPosition(" + testPosition + "): COULD NOT FIND VALID POSITION WITH RADIUS: " + maxNavMeshSampleRadius + ", " + currentMaxSampleRadius + "; minAttackRange: " + minAttackRange + "; RETURNING VECTOR3.ZERO!!!");
            return movementBody.GetPosition();
        }

        public void FreezeCharacter() {
            //Debug.Log($"{gameObject.name}UnitMotor.FreezeCharacter()");
            unitController.DisableAgent();
            frozen = true;
        }

        public void UnFreezeCharacter() {
            //Debug.Log($"{gameObject.name}UnitMotor.UnFreezeCharacter()");
            unitController.EnableAgent();
            frozen = false;
        }

        public void ClickToMove(Vector3 point) {
            //Debug.Log($"{gameObject.name}UnitMotor.ClickToMove(" + point + ")");
            unitController.EnableAgent();
            unitController.RigidBody.isKinematic = true;
            unitController.RigidBody.useGravity = false;
            unitController.RigidBody.interpolation = RigidbodyInterpolation.None;
            unitController.UnitMovementController.ChangeState(CharacterMovementState.NavMesh, false);
            MoveToPoint(point);
        }

        // move toward the position at a normal speed
        public Vector3 MoveToPoint(Vector3 point, float minAttackRange = -1f) {
            //Debug.Log($"{unitController.gameObject.name}.UnitMotor.MoveToPoint(point: {point}, minAttackRange: {minAttackRange}). current location: {unitController.transform.position}; frame: {Time.frameCount}");

            if (frozen) {
                //Debug.Log($"{gameObject.name}UnitMotor.MoveToPoint(" + point + "). current location: " + transform.position + "; frame: " + Time.frameCount + "; FROZEN, DOING NOTHING!!!");
                return movementBody.GetPosition();
            }

            // testing - don't bother with this check since patrolstate is really the only thing that checks for this
            // and returning vector3.zero would result in it just in an endless loop anyway trying to get a new co-ordinate
            /*
            unitController.EnableAgent();
            if (!unitController.NavMeshAgent.enabled) {
                //Debug.Log($"{gameObject.name}.UnitMotor.MoveToPoint(" + point + "): agent is disabled.  Will not give move instruction.");
                return Vector3.zero;
            }
            */
            // moving to a point only happens when we click on the ground.  Since we are not tracking a moving target, we can let the agent update the rotation
            unitController.NavMeshAgent.updateRotation = true;
            //Debug.Log($"{gameObject.name}.UnitMotor.MoveToPoint(" + point + "): calling unitController.MyAgent.ResetPath()");
            ResetPath();
            destinationPosition = CorrectedNavmeshPosition(point, minAttackRange);
            hasDestinationPosition = true;
            // set to false for test
            moveToDestination = false;
            setMoveDestination = true;

            // leaving this unset so it gets picked up in the next fixedupdate because navmeshagent doesn't actually reset path until after current frame.
            //unitController.MyAgent.SetDestination(point);

            //Debug.Log($"{unitController.gameObject.name}.UnitMotor.MoveToPoint({point}). current location: {unitController.transform.position}; frame: {Time.frameCount}; return: {destinationPosition}");
            return destinationPosition;
        }

        public Vector3 GetVelocity() {
            return unitController.NavMeshAgent.velocity;
        }

        public void Move(Vector3 moveDirection) {
            //Debug.Log($"{unitController.gameObject.name}.UnitMotor.Move({moveDirection.x}, {moveDirection.y}, {moveDirection.z}). current position: {unitController.transform.position}; Rigidbody velocity: {unitController.RigidBody.linearVelocity}");
            
            if (frozen) {
                //Debug.Log($"{unitController.gameObject.name}.UnitMotor.Move({moveDirection}: frozen and doing nothing!!!");
                return;
            }

            if (unitController?.NavMeshAgent != null && unitController.NavMeshAgent.enabled) {
                ResetPath();
                unitController.NavMeshAgent.velocity = moveDirection;
            } else {
                movementBody.SetLinearVelocity(moveDirection);
            }
            if (moveDirection != Vector3.zero) {
                BroadcastMovement();
            }
        }

        public void Jump(float jumpSpeed) {
            //Debug.Log($"{unitController.gameObject.name}.UnitMotor.Jump(" + jumpSpeed + "). current position: " + unitController.transform.position);
            if (frozen) {
                return;
            }
            movementBody.AddForce(new Vector3(0, jumpSpeed, 0));
        }

        /*
        public void RotateTowardsTarget(Vector3 targetPosition, float rotationSpeed) {
            //Debug.Log("UnitMotor.RotateTowardsTarget(" + targetPosition + ", " + rotationSpeed + ")");
            if (frozen) {
                return;
            }
            Quaternion targetRotation = Quaternion.LookRotation(targetPosition - new Vector3(unitController.transform.position.x, 0, unitController.transform.position.z));
            unitController.transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(unitController.transform.eulerAngles.y, targetRotation.eulerAngles.y, (rotationSpeed * Time.deltaTime) * rotationSpeed);
        }
        */

        public void BeginFaceSouthEast() {
            //Debug.Log($"{gameObject.name}.UnitMotor.BeginFaceSouthEast()");
            Rotate((new Vector3(1, 0, -1)).normalized);
        }

        public void RotateToward(Vector3 rotateDirection) {
            //Debug.Log($"{unitController.gameObject.name}.UnitMotor.RotateToward({rotateDirection})");

            if (frozen) {
                return;
            }
            if (unitController.NavMeshAgent.enabled) {
                //Debug.Log("nav mesh agent is enabled");
                //Debug.Log($"{gameObject.name}.UnitMotor.RotateToward(): " + rotateDirection);
                ResetPath();
                unitController.NavMeshAgent.updateRotation = true;
                unitController.NavMeshAgent.velocity = rotateDirection;
            } else {
                //Debug.Log("nav mesh agent is disabled");
                movementBody.SetLinearVelocity(rotateDirection);
            }
        }

        public void Rotate(Vector3 rotateDirection) {
            //Debug.Log($"{unitController.gameObject.name}.UnitMotor.Rotate({rotateDirection})");

            if (frozen) {
                return;
            }
            Quaternion currentRotation = movementBody.GetRotation();

            // 2. Calculate the new rotation by multiplying the delta
            // In Unity, adding euler rotations is done by multiplying Quaternions
            Quaternion deltaRotation = Quaternion.Euler(rotateDirection);
            Quaternion nextRotation = currentRotation * deltaRotation;

            // 3. Apply it back to the body
            movementBody.SetRotation(nextRotation);

        }

        public void FollowAttackTarget(Interactable newTarget, float minAttackRange) {
            //Debug.Log($"{unitController.gameObject.name}.UnitMotor.FollowAttackTarget({newTarget.name}, minAttackRange: {minAttackRange})");

            unitController.RigidBody.isKinematic = true;
            unitController.RigidBody.useGravity = false;
            unitController.RigidBody.interpolation = RigidbodyInterpolation.None;

            attackTarget = newTarget;
            this.attackRange = minAttackRange;
            FollowTarget(newTarget, minAttackRange);
        }

        public void FollowInteractionTarget(Interactable newTarget) {
            //Debug.Log($"{unitController.gameObject.name}.UnitMotor.FollowInteractionTarget({(newTarget == null ? "null" : newTarget.name)})");

            unitController.RigidBody.isKinematic = true;
            unitController.RigidBody.useGravity = false;
            unitController.RigidBody.interpolation = RigidbodyInterpolation.None;

            interactionTarget = newTarget;
            FollowTarget(newTarget, unitController.CharacterUnit.HitBoxSize);
        }

        public void FollowTarget(Interactable newTarget, float minAttackRange) {
            //Debug.Log($"{unitController.gameObject.name}.UnitMotor.FollowTarget({(newTarget == null ? "null" : newTarget.name)}, {minAttackRange})");

            if (frozen) {
                return;
            }
            unitController.EnableAgent();

            if (unitController.UnitControllerMode == UnitControllerMode.Player || unitController.UnitControllerMode == UnitControllerMode.Mount) {
                unitController.UnitMovementController.ChangeState(CharacterMovementState.NavMesh, false);
            }

            unitController.NavMeshAgent.stoppingDistance = 0.2f;
            //agent.stoppingDistance = myStats.hitBox;
            // moving to a target happens when we click on an interactable.  Since it might be moving, we will manually update the rotation every frame
            // TEST DISABLE THIS TO PREVENT WALKING SIDEWAYS AROUND CORNERS
            //unitController.MyAgent.updateRotation = false;
            Interactable oldTarget = target;
            target = newTarget;
            lastTargetPosition = target.transform.position;
            if (oldTarget == null || (minAttackRange > 0f && currentMaxSampleRadius != minAttackRange)) {
                //Debug.Log($"{gameObject.name}.UnitMotor.FollowTarget(" + (target == null ? "null" : target.name) + ", " + minAttackRange + "): issuing movetopoint. currentradius: " + currentMaxSampleRadius + "; minattack: " + minAttackRange);
                MoveToInteractionPoint(target, minAttackRange);
            } else {
                //Debug.Log($"{gameObject.name}.UnitMotor.FollowTarget(" + (target == null ? "null" : target.name) + ", " + minAttackRange + "): doing nothing.  oldtarget is not null");
            }
        }

        private void MoveToInteractionPoint(Interactable newTarget, float minAttackRange) {
            //Debug.Log($"{unitController.gameObject.name}.UnitMotor.MoveToInteractionPoint({newTarget.name}, {minAttackRange})");

            // cycle through newTarget interactaionLocations and determine the shortest path, then move to that one
            if (newTarget.InteractionPoints.Count > 0) { 
                Transform bestTarget = newTarget.InteractionPoints[0].transform;
                float bestDistance = GetPathLength(movementBody.GetPosition(), bestTarget.position);
                for (int i = 1; i < newTarget.InteractionPoints.Count; i++) {
                    Transform testTarget = newTarget.InteractionPoints[i].transform;
                    float testDistance = GetPathLength(movementBody.GetPosition(), testTarget.position);
                    if (testDistance < bestDistance) {
                        bestDistance = testDistance;
                        bestTarget = testTarget;
                    }
                }
                MoveToPoint(bestTarget.position, minAttackRange);

                //Debug.Log($"{unitController.gameObject.name}.UnitMotor.MoveToInteractionPoint(): best target: {bestTarget} with distance: {bestDistance}");

                // MoveToPoint() will reset the interaction transform so we need to set it after
                interactionTransform = bestTarget;

                return;
            }

            MoveToPoint(target.transform.position, minAttackRange);
        }

        private float GetPathLength(Vector3 start, Vector3 target) {
            NavMeshPath path = new NavMeshPath();
            // Calculate the actual walking route
            if (NavMesh.CalculatePath(start, target, NavMesh.AllAreas, path)) {
                // If the path is 'Partial', it means it hit a wall/carved obstacle
                if (path.status != NavMeshPathStatus.PathComplete) return float.MaxValue;

                float distance = 0f;
                for (int i = 0; i < path.corners.Length - 1; i++) {
                    distance += Vector3.Distance(path.corners[i], path.corners[i + 1]);
                }
                return distance;
            }
            return float.MaxValue;
        }

        public void StopFollowingTarget() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMotor.StopFollowingTarget()");

            target = null;
            attackTarget = null;
            interactionTarget = null;
            if (frozen) {
                return;
            }
            if (unitController?.NavMeshAgent == null) {
                return;
            }
            if (unitController.NavMeshAgent.isActiveAndEnabled) {
                //Debug.Log($"{unitController.gameObject.name}.UnitMotor.StopFollowingTarget()");
                unitController.NavMeshAgent.stoppingDistance = 0.2f;
                unitController.NavMeshAgent.updateRotation = true;
            }
            ResetPath(true);
        }

        public void FaceTarget(Interactable newTarget) {
            //Debug.Log($"{unitController.gameObject.name}.UnitMotor.FaceTarget(" + newTarget.name + ")");
            if (frozen) {
                return;
            }
            Vector3 direction = (newTarget.transform.position - movementBody.GetPosition()).normalized;

            FaceDirection(direction);
        }

        public void FaceDirection(Vector3 direction) {
            //Debug.Log($"{unitController.gameObject.name}.UnitMotor.FaceDirection({direction})");

            Vector3 horizontalDirection = new Vector3(direction.x, 0f, direction.z);

            if (frozen || horizontalDirection.sqrMagnitude < 0.0001f) {
                return;
            }

            // 2. Convert direction vector to a Quaternion
            Quaternion targetRotation = Quaternion.LookRotation(horizontalDirection.normalized);

            if (unitController.NavMeshAgent.enabled) {
                unitController.NavMeshAgent.updateRotation = false;
                unitController.transform.rotation = targetRotation;
            } else {
                movementBody.SetRotation(targetRotation);
            }
        }

        public void ResetPath(bool forceStop = false) {
            //Debug.Log($"{unitController.gameObject.name}.UnitMotor.ResetPath(forceStop: {forceStop}) in frame: {Time.frameCount}");

            hasDestinationPosition = false;
            moveToDestination = false;
            setMoveDestination = false;
            interactionTransform = null;
            if (unitController.NavMeshAgent.enabled == true) {
                //Debug.Log($"{unitController.gameObject.name}.UnitMotor.ResetPath(forceStop: {forceStop}): navhaspath: {unitController.NavMeshAgent.hasPath}; isOnNavMesh: {unitController.NavMeshAgent.isOnNavMesh}; pathpending: {unitController.NavMeshAgent.pathPending}");
                if (unitController.NavMeshAgent.isOnNavMesh == true) {
                    //Debug.Log($"{unitController.gameObject.name}.UnitMotor.ResetPath(forceStop: {forceStop}) frame: {Time.frameCount}");
                    unitController.NavMeshAgent.ResetPath();
                    if (forceStop) {
                        unitController.NavMeshAgent.isStopped = true;
                        unitController.NavMeshAgent.velocity = Vector3.zero;
                        unitController.ResetApparentVelocity();
                        unitController.DisableAgent();
                        if (unitController.UnitControllerMode == UnitControllerMode.Player || unitController.UnitControllerMode == UnitControllerMode.Mount) {
                            if (systemGameManager.GameMode == GameMode.Local) {
                                unitController.RigidBody.interpolation = RigidbodyInterpolation.Interpolate;
                            }
                            //Debug.Log($"{unitController.gameObject.name}.UnitMotor.ResetPath(forceStop: {forceStop}) in frame: {Time.frameCount} set kinematic false");
                            unitController.RigidBody.isKinematic = false;
                            unitController.RigidBody.useGravity = true;
                        }
                    }
                }
                lastResetFrame = Time.frameCount;
                //Debug.Log($"{gameObject.name}: UnitMotor.FixedUpdate(): AFTER RESETPATH: current location: " + transform.position + "; NavMeshAgentDestination: " + unitController.MyAgent.destination + "; destinationPosition: " + destinationPosition + "; frame: " + Time.frameCount + "; last reset: " + lastResetFrame + "; pathpending: " + unitController.MyAgent.pathPending + "; pathstatus: " + unitController.MyAgent.pathStatus + "; hasPath: " + unitController.MyAgent.hasPath);
                //Debug.Log($"{gameObject.name}.UnitMotor.FixedUpdate(): after reset: navhaspath: " + unitController.MyAgent.hasPath + "; isOnNavMesh: " + unitController.MyAgent.isOnNavMesh + "; pathpending: " + unitController.MyAgent.pathPending);
            }
        }

        public void ReceiveAnimatorMovement() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMotor.ReceiveAnimatorMovement(): " + unitController.UnitAnimator.Animator.deltaPosition.x + " " + unitController.UnitAnimator.Animator.deltaPosition.y + " " + unitController.UnitAnimator.Animator.deltaPosition.z);
            if (UseRootMotion) {
                // will this work for navmeshAgents?  do we need to warp them?
                movementBody.SetPosition(movementBody.GetPosition() + unitController.UnitAnimator.Animator.deltaPosition);
                //Debug.Log($"{unitController.gameObject.name}.UnitMotor.ReceiveAnimatorMovement() userootmotion is true, apply position: " + unitController.UnitAnimator.Animator.deltaPosition.x + " " + unitController.UnitAnimator.Animator.deltaPosition.y + " " + unitController.UnitAnimator.Animator.deltaPosition.z);
            }
        }

        public void SetPosition(Vector3 newPosition) {
            //Debug.Log($"{unitController.gameObject.name}.UnitMotor.SetPosition({newPosition})");
            movementBody.SetPosition(newPosition);
        }

        public void StickToGround() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMotor.StickToGround()");

            if (unitController.PhysicsScene.Raycast(unitController.transform.position + (Vector3.up * 0.25f), -Vector3.up, out centerDownHitInfo, Mathf.Infinity, defaultLayerMask)) {
                
                // 3. Calculate the slope angle
                float angle = Vector3.Angle(Vector3.up, centerDownHitInfo.normal);
                float angleRad = angle * Mathf.Deg2Rad;

                // 4. Calculate the required offset to prevent clipping/jitter
                //float slopeOffset = capsuleRadius * (1f - Mathf.Cos(angleRad));
                float slopeOffset = (capsuleRadius / Mathf.Cos(angleRad)) - capsuleRadius;

                // 5. Target Y is the hit point PLUS the required offset
                ///float targetY = centerDownHitInfo.point.y + slopeOffset;

                // Use the Normal-based projection (PhysX Standard)
                //float targetY = (centerDownHitInfo.point.y + (centerDownHitInfo.normal.y * capsuleRadius)) - capsuleRadius;

                // Correct Trigonometric Projection
                // targetY = hitPoint.y + (radius / normal.y) - radius
                float targetY = centerDownHitInfo.point.y + (capsuleRadius / centerDownHitInfo.normal.y) - capsuleRadius;

                // Add back the Default Contact Offset (0.0001) to be safe
                targetY += 0.0001f;

                // Only snap if we are actually above the target to avoid "launching"
                
                if (Mathf.Abs(movementBody.GetPosition().y - targetY) > 0.001f) {

                    movementBody.SetPosition(new Vector3(
                        unitController.RigidBody.position.x,
                        targetY,
                        unitController.RigidBody.position.z
                    ));
                }
            }
        }

        public bool HasDestination() {
            if (hasDestinationPosition) {
                return true;
            }
            if (unitController.NavMeshAgent.enabled == false) {
                return false;
            }
            if (unitController.NavMeshAgent.hasPath) {
                return true;
            }
            return false;
        }

        public void Knockback(float explosionForce, Vector3 explosionCenter, float upwardModifier) {
            movementBody.AddExplosionForce(explosionForce, explosionCenter, upwardModifier);
        }
    }

}