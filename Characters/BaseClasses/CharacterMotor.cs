using AnyRPG;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class CharacterMotor : MonoBehaviour {

        public event System.Action OnMovement = delegate { };

        protected GameObject target;

        protected bool moveToDestination = false;
        protected Vector3 destinationPosition = Vector3.zero;

        // when checking if destinationPosition matches NavMeshAgent destination, add a padding amount to account for destinations that are not precisely on the NavMesh
        protected float navMeshDistancePadding = 0.1f;

        protected CharacterUnit characterUnit;
        protected AnimatedUnit animatedUnit;

        // default value meant to be overwritten by a controller (AI/player)
        protected float movementSpeed = 0f;

        private bool frozen = false;

        // the maximum radius from an arbitrary point which the NavMeshAgent will search to find a valid NavMesh location to move to
        private float maxNavMeshSampleRadius = 3f;

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

        // properties
        public float MyMovementSpeed { get => movementSpeed; set => movementSpeed = value; }
        public GameObject MyTarget { get => target; }
        public bool MyFrozen { get => frozen; set => frozen = value; }
        public float MyNavMeshDistancePadding { get => navMeshDistancePadding; }
        public CharacterUnit MyCharacterUnit { get => characterUnit; set => characterUnit = value; }
        public bool MyUseRootMotion { get => useRootMotion; set => useRootMotion = value; }

        protected virtual void Awake() {
        }

        protected virtual void Start() {
            // meant to be overwritten
        }

        public virtual void OrchestratorStart() {
            GetComponentReferences();
        }

        public virtual void OrchestratorFinish() {

        }

        protected virtual void GetComponentReferences() {
            characterUnit = GetComponent<CharacterUnit>();
            animatedUnit = GetComponent<AnimatedUnit>();
        }

        protected virtual void SetMovementSpeed() {
            if (movementSpeed == 0) {
                animatedUnit.MyAgent.speed = characterUnit.MyCharacter.MyCharacterController.MyMovementSpeed;
            } else {
                //Debug.Log(gameObject.name + ".CharacterMotor.Update(): movementSpeed: " + movementSpeed);
                animatedUnit.MyAgent.speed = movementSpeed;
            }

        }

        protected virtual void Update() {
            //Debug.Log(gameObject.name + ".CharacterMotor.Update(): navhaspath: " + animatedUnit.MyAgent.hasPath + "; isOnNavMesh: " + animatedUnit.MyAgent.isOnNavMesh + "; pathpending: " + animatedUnit.MyAgent.pathPending);
            if (frozen) {
                return;
            }
            if (characterUnit != null && animatedUnit != null && animatedUnit.MyAgent != null && animatedUnit.MyAgent.isActiveAndEnabled) {
                SetMovementSpeed();
            } else {
                //Debug.Log(gameObject.name + ": motor.FixedUpdate(): agent is disabled. Motor will do nothing");
                // unused gravity stuff
                //animatedUnit.MyRigidBody.velocity = new Vector3(0, animatedUnit.MyRigidBody.velocity.y + (-9.81f * Time.deltaTime), 0);
                /*
                Vector3 newRelativeForce = new Vector3(0, -(9.81f * 9.81f * Time.fixedDeltaTime), 0);
                Debug.Log(gameObject.name + ".CharacterMotor.FixedUpdate(): newRelativeForce: " + newRelativeForce);
                animatedUnit.MyRigidBody.AddRelativeForce(newRelativeForce);
                */
                return;
            }
            CheckSetMoveDestination();
        }

        protected virtual void CheckSetMoveDestination() {
            if (setMoveDestination && animatedUnit.MyAgent.pathPending == false && animatedUnit.MyAgent.hasPath == false) {
                //Debug.Log(gameObject.name + ": CharacterMotor.FixedUpdate(): setMoveDestination: true.  Set move destination: " + destinationPosition + "; current location: " + transform.position);
                moveToDestination = true;
                //Debug.Log(gameObject.name + ": CharacterMotor.Update(): ISSUING SETDESTINATION: current location: " + transform.position + "; MyAgent.SetDestination(" + destinationPosition + ") on frame: " + Time.frameCount + " with last reset: " + lastResetFrame + "; pathpending: " + animatedUnit.MyAgent.pathPending + "; pathstatus: " + animatedUnit.MyAgent.pathStatus + "; hasPath: " + animatedUnit.MyAgent.hasPath);
                animatedUnit.MyAgent.SetDestination(destinationPosition);
                //Debug.Log(gameObject.name + ": CharacterMotor.Update(): AFTER SETDESTINATION: current location: " + transform.position + "; NavMeshAgentDestination: " + animatedUnit.MyAgent.destination + "; destinationPosition: " + destinationPosition + "; frame: " + Time.frameCount + "; last reset: " + lastResetFrame + "; pathpending: " + animatedUnit.MyAgent.pathPending + "; pathstatus: " + animatedUnit.MyAgent.pathStatus + "; hasPath: " + animatedUnit.MyAgent.hasPath);
                lastCommandFrame = Time.frameCount;
                setMoveDestination = false;
            }
            /*
            if (!setMoveDestination) {
                Debug.Log(gameObject.name + ": CharacterMotor.FixedUpdate(): setMoveDestination: false.  Set move destination: " + destinationPosition + "; current location: " + transform.position);
            }
            */
            if (animatedUnit.MyAgent.pathPending == true) {
                pathPendingCount++;
                //Debug.Log(gameObject.name + ": CharacterMotor.CheckSetMoveDestination(): setMoveDestination: " + setMoveDestination + "; destinationPosition: " + destinationPosition + "; current location: " + transform.position + "; PATHPENDING: TRUE!!!; status: " + animatedUnit.MyAgent.pathStatus + "; count: " + pathPendingCount);
            } else {
                pathPendingCount = 0;
            }
            if (animatedUnit.MyAgent.hasPath == true) {
                //Debug.Log(gameObject.name + ": CharacterMotor.CheckSetMoveDestination(): setMoveDestination: " + setMoveDestination + "; destinationPosition: " + destinationPosition + "; current location: " + transform.position + "; HASPATH: TRUE!!!; status: " + animatedUnit.MyAgent.pathStatus);
            }
        }

        protected virtual void FixedUpdate() {
            //Debug.Log(gameObject.name + ".CharacterMotor.FixedUpdate(). current location: " + transform.position);
            //Debug.Log(gameObject.name + ".CharacterMotor.FixedUpdate(): navhaspath: " + animatedUnit.MyAgent.hasPath + "; isOnNavMesh: " + animatedUnit.MyAgent.isOnNavMesh + "; pathpending: " + animatedUnit.MyAgent.pathPending);
            if (frozen) {
                //Debug.Log(gameObject.name + ".CharacterMotor.FixedUpdate(): navhaspath: " + animatedUnit.MyAgent.hasPath + "; isOnNavMesh: " + animatedUnit.MyAgent.isOnNavMesh + "; pathpending: " + animatedUnit.MyAgent.pathPending + "; FROZEN: RETURNING!!!");
                return;
            }
            if (characterUnit != null && animatedUnit.MyAgent != null && animatedUnit.MyAgent.isActiveAndEnabled) {
                //Debug.Log(gameObject.name + ".CharacterMotor.FixedUpdate(): navhaspath: " + animatedUnit.MyAgent.hasPath + "; isOnNavMesh: " + animatedUnit.MyAgent.isOnNavMesh + "; pathpending: " + animatedUnit.MyAgent.pathPending + "; ANIMATED UNIT IS NOT NULL, SETTING SPEED");
                SetMovementSpeed();
            } else {
                //Debug.Log(gameObject.name + ": motor.FixedUpdate(): agent is disabled. Motor will do nothing");
                // unused gravity stuff
                //animatedUnit.MyRigidBody.velocity = new Vector3(0, animatedUnit.MyRigidBody.velocity.y + (-9.81f * Time.deltaTime), 0);
                /*
                Vector3 newRelativeForce = new Vector3(0, -(9.81f * 9.81f * Time.fixedDeltaTime), 0);
                Debug.Log(gameObject.name + ".CharacterMotor.FixedUpdate(): newRelativeForce: " + newRelativeForce);
                animatedUnit.MyRigidBody.AddRelativeForce(newRelativeForce);
                */
                return;
            }

            CheckSetMoveDestination();


            if (target != null) {
                //Debug.Log(gameObject.name + ": CharacterMotor.FixedUpdate() target = " + target.name);
                // sometimes objects are above the ground.  if you don't ignore height you may try to move upward.
                // check if you are in the same spot as it and ignore height.
                //float distanceToTarget = Vector3.Distance(new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z), transform.position);
                //Debug.Log("Target is " + target.ToString() + " and target position is " + target.position.ToString() + " and my position is " + transform.position.ToString());
                //if (distanceToTarget > 0.1f) {
                //Debug.Log(gameObject.name + ".CharacterMotor.FixedUpdate(): ABOUT TO CHECK IF TARGET IS IN HITBOX!!");
                if (characterUnit.MyCharacter.MyCharacterController.IsTargetInHitBox(target)) {
                    // this code can tend to get bypassed because followstate for AI runs on update, not fixedupdate, so will null the target quicker than this method usually
                    //Debug.Log(gameObject.name + ".CharacterMotor.FixedUpdate(): Target is in hitbox.  Stop following target.");
                    StopFollowingTarget();
                } else {

                    // YES THESE 2 BLOCKS OF CODE ARE COMPLETELY IDENTICAL.  IT'S LIKE THAT SO I CAN ADJUST THE LONG DISTANCE PATHING DIFFERENT IN THE FUTURE.
                    // EG, ENEMY MORE THAN 10 YARDS AWAY CAN HAVE LESS PRECISE UPDATES TO AVOID A LOT OF PATHING CALCULATIONS FOR SOMETHING THAT ONLY NEEDS TO HEAD IN YOUR APPROXIMATE DIRECTION
                    if (Vector3.Distance(target.transform.position, transform.position) > (characterUnit.MyHitBoxSize * 2)) {
                        // we are more than 3x the hitbox size away, and should be trying to move toward the targets fuzzy location to prevent movement stutter
                        //Debug.Log(gameObject.name + ".CharacterMotor.FixedUpdate(): More than twice the hitbox distance from the target: " + Vector3.Distance(target.transform.position, transform.position));

                        // this next line is meant to at long distances, move toward the character even if he is off the navmesh and prevent enemy movement stutter chasing a moving target
                        if (Vector3.Distance(CorrectedNavmeshPosition(target.transform.position), animatedUnit.MyAgent.destination) > (characterUnit.MyHitBoxSize * 1.5) && animatedUnit.MyAgent.pathPending == false) {
                            // the target has moved more than 1 hitbox from our destination position, re-adjust heading
                            //Debug.Log(gameObject.name + ": FixedUpdate() destinationPosition: " + destinationPosition + " distance: " + Vector3.Distance(target.transform.position, destinationPosition) + ". Issuing MoveToPoint()");
                            if (Time.frameCount != lastResetFrame && Time.frameCount != lastCommandFrame) {
                                // prevent anything from resetting movement twice in the same frame
                                //Debug.Log(gameObject.name + ".CharacterMotor.FixedUpdate(): ABOUT TO ISSUE MOVETOPOINT: current location: " + transform.position + "; NavMeshAgentDestination: " + animatedUnit.MyAgent.destination + "; destinationPosition: " + destinationPosition + "; frame: " + Time.frameCount + "; last reset: " + lastResetFrame + "; pathpending: " + animatedUnit.MyAgent.pathPending + "; pathstatus: " + animatedUnit.MyAgent.pathStatus + "; hasPath: " + animatedUnit.MyAgent.hasPath);

                                MoveToPoint(target.transform.position);
                            } else {
                                //Debug.Log(gameObject.name + ".CharacterMotor.FixedUpdate(): LONGDISTANCE: WE WERE ABOUT TO ISSUE A MOVETOPOINT ON THE SAME FRAME AS A RESET OR COMMAND: frame: " + Time.frameCount + "; last reset: " + lastResetFrame + "; last command: " + lastCommandFrame);
                            }
                        } else {
                            //Debug.Log(gameObject.name + ": FixedUpdate() NOT RECALCULATING! targetPosition: " + target.transform.position + "; destinationPosition: " + destinationPosition + "; navMeshAgentDestination: " + animatedUnit.MyAgent.destination + "; NavMesAgentDestinationToTargetDrift: " + Vector3.Distance(CorrectedNavmeshPosition(target.transform.position), animatedUnit.MyAgent.destination) + "; destinationPositionToTargetDifference: " + Vector3.Distance(CorrectedNavmeshPosition(target.transform.position), destinationPosition) + "; frame: " + Time.frameCount);
                            // we are more than 2 meters from the target, and they are less than 2 meters from their last position, destinations may not match but are close enough that there is no point in re-calculating
                        }
                    } else {
                        // they are not in our hitbox yet, but they are closer than 2 meters, we need to move directly to them.  we are likely 0.5 meters out of hitbox range at this point
                        //Debug.Log(gameObject.name + ".CharacterMotor.FixedUpdate(): Less than twice the hitbox distance from the target: " + Vector3.Distance(target.transform.position, transform.position) + ". Issuing MoveToPoint (maybe)");

                        //Debug.Log(gameObject.name + ".CharacterMotor.FixedUpdate(): Less than twice the hitbox distance from the target: " + target.transform.position + "; destination: " + destinationPosition);
                        if (Vector3.Distance(CorrectedNavmeshPosition(target.transform.position), animatedUnit.MyAgent.destination) > (characterUnit.MyHitBoxSize / 2) && animatedUnit.MyAgent.pathPending == false) {
                            //Debug.Log(gameObject.name + ": CharacterMotor.FixedUpdate(): current location: " + transform.position + "; destinationPosition: " + destinationPosition + "; animatedUnit.MyAgent.destination: " + animatedUnit.MyAgent.destination + "; pathpending: " + animatedUnit.MyAgent.pathPending + " ISSUING MOVETOPOINT!");
                            if (Time.frameCount != lastResetFrame && Time.frameCount != lastCommandFrame) {
                                // prevent anything from resetting movement twice in the same frame
                                //Debug.Log(gameObject.name + ": CharacterMotor.FixedUpdate(): ABOUT TO ISSUE MOVETOPOINT: current location: " + transform.position + "; MyAgent.SetDestination(" + destinationPosition + ") on frame: " + Time.frameCount + " with last reset: " + lastResetFrame + "; pathpending: " + animatedUnit.MyAgent.pathPending + "; pathstatus: " + animatedUnit.MyAgent.pathStatus + "; hasPath: " + animatedUnit.MyAgent.hasPath);
                                MoveToPoint(target.transform.position);
                            } else {
                                //Debug.Log(gameObject.name + ".CharacterMotor.FixedUpdate(): SHORTDISTANCE: WE WERE ABOUT TO ISSUE A MOVETOPOINT ON THE SAME FRAME AS A RESET OR COMMAND: frame: " + Time.frameCount + "; last reset: " + lastResetFrame + "; last command: " + lastCommandFrame);
                            }
                        } else {
                            //Debug.Log(gameObject.name + ": CharacterMotor.FixedUpdate(): DOING NOTHING location: " + transform.position + "; targetLocation: " + target.transform.position + "; destinationPosition: " + destinationPosition + "; animatedUnit.MyAgent.destination: " + animatedUnit.MyAgent.destination + "; pathpending: " + animatedUnit.MyAgent.pathPending + "; destination distance vector: " + Vector3.Distance(CorrectedNavmeshPosition(target.transform.position), destinationPosition) + "; actualdistancevector: " + Vector3.Distance(transform.position, target.transform.position));
                        }

                        //Debug.Log(gameObject.name + "(" + transform.position + "): CharacterMotor.Update(): destination is: " + agent.destination + "; target: " + target.transform.position);
                    }
                    //lastTargetLocation = target.transform.position;
                    FaceTarget(target);
                }
                //} else if (moveToDestination == true) {
                // i think this next statement was the short version to stop agents getting stuck on corners so it continously updated the destination
            } else {
                //Debug.Log(gameObject.name + ": CharacterMotor.FixedUpdate(): TARGET IS NULL!");
                if (moveToDestination == true && destinationPosition != animatedUnit.MyAgent.destination) {
                    //Debug.Log(gameObject.name + ": CharacterMotor.FixedUpdate(): TARGET IS NULL! moveToDestination: true. current location: " + transform.position + "; destinationPosition: " + destinationPosition + "; animatedUnit.MyAgent.destination: " + animatedUnit.MyAgent.destination + "; pathpending: " + animatedUnit.MyAgent.pathPending);
                    float agentDestinationDrift = Vector3.Distance(destinationPosition, animatedUnit.MyAgent.destination);
                    if (agentDestinationDrift >= (animatedUnit.MyAgent.stoppingDistance + navMeshDistancePadding) && destinationPosition != animatedUnit.MyAgent.destination) {
                        //Debug.Log(gameObject.name + ": FixedUpdate() Vector3.Distance(destinationPosition, animatedUnit.MyAgent.destination): " + agentDestinationDrift + "; animatedUnit.MyAgent.stoppingDistance: " + animatedUnit.MyAgent.stoppingDistance);
                        //Debug.Log(gameObject.name + ": FixedUpdate() agent.destination: " + animatedUnit.MyAgent.destination + " but should be point: " + destinationPosition + ". Issuing MoveToPoint()");
                        MoveToPoint(destinationPosition);
                    } else {
                        //Debug.Log(gameObject.name + ": FixedUpdate() agent.destination: " + animatedUnit.MyAgent.destination + " matches point (within stopping distance): " + destinationPosition + ". Disable moveToDestination boolean");
                        moveToDestination = false;
                    }
                }
            }

            if (animatedUnit.MyAgent.velocity.sqrMagnitude > 0) {
                OnMovement();
                if (animatedUnit.MyCharacterAnimator != null) {
                    animatedUnit.MyCharacterAnimator.SetMoving(true);
                    //animatedUnit.MyCharacterAnimator.SetVelocityZ(animatedUnit.MyAgent.velocity.magnitude);
                    //animatedUnit.MyCharacterAnimator.SetVelocity(animatedUnit.MyAgent.velocity);
                    animatedUnit.MyCharacterAnimator.SetVelocity((characterUnit as MonoBehaviour).transform.InverseTransformDirection(animatedUnit.MyAgent.velocity));
                }
            } else {
                if (animatedUnit.MyCharacterAnimator != null) {
                    animatedUnit.MyCharacterAnimator.SetMoving(false);
                    //animatedUnit.MyCharacterAnimator.SetVelocityZ(0);
                    animatedUnit.MyCharacterAnimator.SetVelocity(Vector3.zero);
                }
            }

        }

        public Vector3 CorrectedNavmeshPosition(Vector3 testPosition) {
            //Debug.Log(gameObject.name + ".CharacterMotor.CorrectedNavmeshPosition(" + testPosition + ")");

            NavMeshHit hit;
            
            // get current mask
            /*
            int currentMask = NavMesh.AllAreas;
            bool foundCurrentNavMesh = false;
            if (NavMesh.SamplePosition(transform.position, out hit, 0.1f, NavMesh.AllAreas)) {
                currentMask = hit.mask;
                //Debug.Log(gameObject.name + ".CharacterMotor.CorrectedNavmeshPosition(" + testPosition + ") character transform: " + transform.position + " was on NavMesh: " + currentMask);
                foundCurrentNavMesh = true;
            }
            */

            // NOTE: SUPER MESSED UP... UNITY WILL NOT RAYCAST FROM BELOW TERRAIN, MUST BE FROM TOP BECAUSE PHYSICS RAYCAST WILL NOT HIT BACK FACE OF TRIANGLES

            // THIS NEEDS FIXING THANKS TO UNITY BUG OF NOT PROPERLY HANDLING SAMPLEPOSITION AND IGNORING XZ AND ONLY USING Y
            // start with a RADIUS OF 0.5 AND CONTINUALLY EXPAND 0.5 AT A TIME UNTIL WE FIND A VALID POINT

            /*
            // new code to try all areas only after current area - this hopefully does not lead to lag when switching areas
            float sampleRadius = 0.5f;
            while (sampleRadius <= maxNavMeshSampleRadius) { 
                if (NavMesh.SamplePosition(testPosition, out hit, sampleRadius, currentMask)) {
                    //Debug.Log(gameObject.name + ": CharacterMotor.FixedUpdate(): testPosition " + testPosition + " on NavMesh found closest point: " + hit.position + ")");
                    return hit.position;
                }
                sampleRadius += navMeshSampleStepSize;
            }
            */

            // attempt sample at 0.5f radius using current navmesharea.  if this works, we found a valid point on the current navmesh
            if (NavMesh.SamplePosition(testPosition, out hit, 0.5f, NavMesh.AllAreas)) {
                //Debug.Log(gameObject.name + ".CharacterMotor.CorrectedNavmeshPosition(): testPosition " + testPosition + " was on current NavMesh near: " + hit.position + ")");
                return hit.position;
            }

            // we did not find a valid point on the current navmesh
            // since unity prefers finding stuff downward rather than sideways, we want to shoot updward to find out if we landed under a hill
            // attempt vertical updward shot to find position on current navmesh first
            RaycastHit raycastHit;
            Vector3 firstTestPosition = Vector3.zero;
            bool foundMatch = false;
            //if (Physics.Raycast(testPosition + new Vector3(0f, -0.1f, 0f), Vector3.up, out raycastHit, 10f, (PlayerManager.MyInstance.MyCharacter.MyCharacterController as PlayerController).movementMask)) {
            if (Physics.Raycast(testPosition + new Vector3(0f, 10f, 0f), Vector3.down, out raycastHit, 10f, PlayerManager.MyInstance.MyDefaultGroundMask)) {
                firstTestPosition = raycastHit.point;
                foundMatch = true;
                //Debug.Log(gameObject.name + ".CharacterMotor.CorrectedNavmeshPosition(): testPosition " + testPosition + " got hit above on walkable ground: " + firstTestPosition + "; mask: " + raycastHit.collider.name);
            }

            float sampleRadius = 0.5f;
            if (foundMatch) {
                // our upward raycast found a walkable area.  is it the same area?  check outward for valid point on same area
                while (sampleRadius <= maxNavMeshSampleRadius) {
                    if (NavMesh.SamplePosition(firstTestPosition, out hit, sampleRadius, NavMesh.AllAreas)) {
                        //Debug.Log(gameObject.name + ": CharacterMotor.FixedUpdate(): testPosition " + testPosition + " got hit above on walkable ground at current mask: " + firstTestPosition + ")");
                        return hit.position;
                    }
                    sampleRadius += navMeshSampleStepSize;
                }
                // if we actually got a hit, but did not detect a navmesh, then don't try raycast downward.  the hit was probably on a steep up hill and trying a downcast from our current
                // level would result in a ray inside the hill shooting downward to a potentially inaccessible navmesh below
                return Vector3.zero;
            }

            // now try raycast downward in case we are at the top of a hill
            firstTestPosition = Vector3.zero;
            foundMatch = false;
            if (Physics.Raycast(testPosition, Vector3.down, out raycastHit, 10f, PlayerManager.MyInstance.MyDefaultGroundMask)) {
                firstTestPosition = raycastHit.point;
                foundMatch = true;
                //Debug.Log(gameObject.name + ".CharacterMotor.CorrectedNavmeshPosition(): testPosition " + testPosition + " got hit below on walkable ground: " + firstTestPosition + ")");
            }

            sampleRadius = 0.5f;
            if (foundMatch) {
                // our downward raycast found a walkable area.  is it the same area?  check outward for valid point on same area
                while (sampleRadius <= maxNavMeshSampleRadius) {
                    if (NavMesh.SamplePosition(firstTestPosition, out hit, sampleRadius, NavMesh.AllAreas)) {
                        //Debug.Log(gameObject.name + ": CharacterMotor.FixedUpdate(): testPosition " + testPosition + " got hit below on walkable ground at current mask: " + firstTestPosition + ")");
                        return hit.position;
                    }
                    sampleRadius += navMeshSampleStepSize;
                }
            }

            // we didn't find anything on the same navmesharea above the raycast hit, so it's probably a floor of another area.  try just searching outward for any navmesh instead
            // it's possible we are switching areas between navmesh boundaries
            
            sampleRadius = 0.5f;
            while (sampleRadius <= maxNavMeshSampleRadius) {
                if (NavMesh.SamplePosition(testPosition, out hit, sampleRadius, NavMesh.AllAreas)) {
                    //Debug.Log(gameObject.name + ": CharacterMotor.FixedUpdate(): testPosition " + testPosition + " on NavMesh found closest point on a different navmesh at : " + hit.position + ")");
                    return hit.position;
                }
                sampleRadius += navMeshSampleStepSize;
            }
            

            //Debug.Log(gameObject.name + ": CharacterMotor.FixedUpdate(): testPosition: " + testPosition + " was not within " + maxNavMeshSampleRadius + " of a NavMesh!");
            // should the code that calls this go into evade 

            Debug.Log(gameObject.name + ".CharacterMotor.CorrectedNavmeshPosition(" + testPosition + "): COULD NOT FIND VALID POSITION WITH RADIUS: " + maxNavMeshSampleRadius + ", RETURNING VECTOR3.ZERO!!!");
            return Vector3.zero;
        }

        public void FreezeCharacter() {
            //Debug.Log(gameObject.name + "CharacterMotor.FreezeCharacter()");
            animatedUnit.MyAgent.enabled = false;
            frozen = true;
        }

        public void UnFreezeCharacter() {
            //Debug.Log(gameObject.name + "CharacterMotor.UnFreezeCharacter()");
            animatedUnit.MyAgent.enabled = true;
            frozen = false;
        }

        // move toward the position at a normal speed
        public Vector3 MoveToPoint(Vector3 point) {
            //Debug.Log(gameObject.name + "CharacterMotor.MoveToPoint(" + point + "). current location: " + transform.position + "; frame: " + Time.frameCount);
            if (frozen) {
                //Debug.Log(gameObject.name + "CharacterMotor.MoveToPoint(" + point + "). current location: " + transform.position + "; frame: " + Time.frameCount + "; FROZEN, DOING NOTHING!!!");
                return Vector3.zero;
            }

            if (!animatedUnit.MyAgent.enabled) {
                //Debug.Log(gameObject.name + ".CharacterMotor.MoveToPoint(" + point + "): agent is disabled.  Will not give move instruction.");
                return Vector3.zero;
            }

            // moving to a point only happens when we click on the ground.  Since we are not tracking a moving target, we can let the agent update the rotation
            animatedUnit.MyAgent.updateRotation = true;
            //Debug.Log(gameObject.name + ".CharacterMotor.MoveToPoint(" + point + "): calling animatedUnit.MyAgent.ResetPath()");
            ResetPath();
            destinationPosition = CorrectedNavmeshPosition(point);
            // set to false for test
            moveToDestination = false;
            setMoveDestination = true;
            // leaving this unset so it gets picked up in the next fixedupdate because navmeshagent doesn't actually reset path until after current frame.
            //animatedUnit.MyAgent.SetDestination(point);
            return destinationPosition;
        }

        public void MoveToPosition(Vector3 newPosition) {
            //Debug.Log(gameObject.name + ".CharacterMotor.MoveToPosition(" + newPosition + ")");
            if (frozen) {
                return;
            }
            animatedUnit.MyRigidBody.MovePosition(newPosition);
            OnMovement();
        }

        public Vector3 getVelocity() {
            return animatedUnit.MyAgent.velocity;
        }

        public virtual void Move(Vector3 moveDirection, bool isKnockBack = false) {
            //Debug.Log(gameObject.name + ".CharacterMotor.Move(" + moveDirection + "). current position: " + transform.position);
            if (frozen) {
                //Debug.Log(gameObject.name + ".CharacterMotor.Move(" + moveDirection + "): frozen and doing nothing!!!");
                return;
            }
            if (characterUnit != null && animatedUnit.MyAgent != null && animatedUnit.MyAgent.enabled) {
                //Debug.Log(gameObject.name + ".CharacterMotor.Move(" + moveDirection + "): moving via navmeshagent");

                //agent.Move(moveDirection);
                ResetPath();
                animatedUnit.MyAgent.updateRotation = false;
                animatedUnit.MyAgent.velocity = moveDirection;
            } else {
                //float currentYVelocity = moveDirection.y != 0 ? moveDirection.y : animatedUnit.MyRigidBody.velocity.y;
                //Debug.Log("characterUnit.yVelocity is " + currentYVelocity);
                //Vector3 newMoveDirection = new Vector3(moveDirection.x, currentYVelocity, moveDirection.z);
                //Debug.Log(gameObject.name + ".CharacterMotor.Move() newMoveDirection: " + moveDirection + "; animatedUnit.MyRigidBody.constraints: " + animatedUnit.MyRigidBody.constraints);
                animatedUnit.MyRigidBody.velocity = moveDirection;
                //animatedUnit.MyRigidBody.MovePosition(transform.position + moveDirection);
                //animatedUnit.MyRigidBody.AddForce(moveDirection, ForceMode.VelocityChange);
            }
            if (moveDirection != Vector3.zero) {
                OnMovement();
            }
        }

        public void Jump(float jumpSpeed) {
            //Debug.Log(gameObject.name + ".CharacterMotor.Jump(" + jumpSpeed + "). current position: " + transform.position);
            if (frozen) {
                return;
            }
            animatedUnit.MyRigidBody.AddRelativeForce(new Vector3(0, jumpSpeed, 0), ForceMode.VelocityChange);
        }

        public void RotateTowardsTarget(Vector3 targetPosition, float rotationSpeed) {
            //Debug.Log("RotateTowardsMovementTarget()");
            if (frozen) {
                return;
            }
            Quaternion targetRotation = Quaternion.LookRotation(targetPosition - new Vector3(transform.position.x, 0, transform.position.z));
            transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, (rotationSpeed * Time.deltaTime) * rotationSpeed);
        }

        public void BeginFaceSouthEast() {
            //Debug.Log(gameObject.name + ".CharacterMotor.BeginFaceSouthEast()");
            //RotateToward((new Vector3(1, 0, -1)).normalized);
            Rotate((new Vector3(1, 0, -1)).normalized);
            //Rotate((new Vector3(-1, 0, 1)).normalized);
        }

        public void RotateToward(Vector3 rotateDirection) {
            //Debug.Log(gameObject.name + ".CharacterMotor.RotateToward(): " + rotateDirection);
            if (frozen) {
                return;
            }
            if (animatedUnit.MyAgent.enabled) {
                //Debug.Log("nav mesh agent is enabled");
                //Debug.Log(gameObject.name + ".CharacterMotor.RotateToward(): " + rotateDirection);
                ResetPath();
                animatedUnit.MyAgent.updateRotation = true;
                animatedUnit.MyAgent.velocity = rotateDirection;
            } else {
                //Debug.Log("nav mesh agent is disabled");
                animatedUnit.MyRigidBody.velocity = rotateDirection;
            }
        }

        public void Rotate(Vector3 rotateDirection) {
            //Debug.Log(gameObject.name + ".CharacterMotor.Rotate(): " + rotateDirection);
            if (frozen) {
                return;
            }
            //(characterUnit as MonoBehaviour).transform.Rotate(rotateDirection);
            transform.Rotate(rotateDirection);
        }


        public void FollowTarget(GameObject newTarget) {
            //Debug.Log(gameObject.name + ".CharacterMotor.FollowTarget()");
            if (frozen) {
                return;
            }
            animatedUnit.MyAgent.stoppingDistance = 0.2f;
            //agent.stoppingDistance = myStats.hitBox;
            // moving to a target happens when we click on an interactable.  Since it might be moving, we will manually update the rotation every frame
            animatedUnit.MyAgent.updateRotation = false;
            GameObject oldTarget = target;
            target = newTarget;
            if (oldTarget == null) {
                MoveToPoint(target.transform.position);
            }

        }

        public void StopFollowingTarget() {
            //Debug.Log(gameObject.name + ".CharacterMotor.StopFollowingTarget()");
            target = null;
            moveToDestination = false;
            if (frozen) {
                return;
            }
            if (characterUnit == null) {
                return;
            }
            if (animatedUnit.MyAgent == null) {
                return;
            }
            if (animatedUnit.MyAgent.isActiveAndEnabled) {
                //Debug.Log(gameObject.name + ".CharacterMotor.StopFollowingTarget()");
                animatedUnit.MyAgent.stoppingDistance = 0.2f;
                animatedUnit.MyAgent.updateRotation = true;
                target = null;
                moveToDestination = false;
                //lastTargetLocation = Vector3.zero;
                ResetPath();
            }
        }

        public void FaceTarget(GameObject newTarget) {
            //Debug.Log(gameObject.name + ".CharacterMotor.FaceTarget(" + newTarget.name + ")");
            if (frozen) {
                return;
            }
            Vector3 direction = (newTarget.transform.position - transform.position).normalized;
            //Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
            if (animatedUnit.MyAgent.enabled) {
                animatedUnit.MyAgent.updateRotation = false;
                //animatedUnit.MyAgent.r
            }
            //transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, Time.deltaTime * 500);
            //transform.rotation = lookRotation;
            //Debug.Log(gameObject.name + ".CharacterMotor.FaceTarget(" + newTarget.name + "): direction: " + direction);
            if (direction != Vector3.zero) {
                transform.forward = direction;
            }
            if (animatedUnit.MyAgent.enabled) {
                animatedUnit.MyAgent.updateRotation = true;
                //animatedUnit.MyAgent.r
            }
        }

        public void StartNavAgent() {
            //Debug.Log(gameObject.name + ".CharacterMotor.StartNavAgent()");
            if (!animatedUnit.MyAgent.enabled) {
                animatedUnit.MyAgent.enabled = true;
                animatedUnit.MyRigidBody.isKinematic = true;
            }
        }

        public void StopNavAgent() {
            //Debug.Log(gameObject.name + ".CharacterMotor.StopNavAgent()");
            if (animatedUnit.MyAgent.enabled) {
                animatedUnit.MyAgent.enabled = false;
            }
        }

        public void ResetPath() {
            //Debug.Log(gameObject.name + ".CharacterMotor.ResetPath() in frame: " + Time.frameCount);
            if (animatedUnit.MyAgent.enabled == true) {
                //Debug.Log(gameObject.name + ".CharacterMotor.FixedUpdate(): navhaspath: " + animatedUnit.MyAgent.hasPath + "; isOnNavMesh: " + animatedUnit.MyAgent.isOnNavMesh + "; pathpending: " + animatedUnit.MyAgent.pathPending);
                animatedUnit.MyAgent.ResetPath();
                lastResetFrame = Time.frameCount;
                //Debug.Log(gameObject.name + ": CharacterMotor.FixedUpdate(): AFTER RESETPATH: current location: " + transform.position + "; NavMeshAgentDestination: " + animatedUnit.MyAgent.destination + "; destinationPosition: " + destinationPosition + "; frame: " + Time.frameCount + "; last reset: " + lastResetFrame + "; pathpending: " + animatedUnit.MyAgent.pathPending + "; pathstatus: " + animatedUnit.MyAgent.pathStatus + "; hasPath: " + animatedUnit.MyAgent.hasPath);
                //Debug.Log(gameObject.name + ".CharacterMotor.FixedUpdate(): after reset: navhaspath: " + animatedUnit.MyAgent.hasPath + "; isOnNavMesh: " + animatedUnit.MyAgent.isOnNavMesh + "; pathpending: " + animatedUnit.MyAgent.pathPending);
            }
        }

        public void ReceiveAnimatorMovment(Vector3 movementDelta) {
            if (MyUseRootMotion) {
                transform.position += movementDelta;
            }
        }
    }

}