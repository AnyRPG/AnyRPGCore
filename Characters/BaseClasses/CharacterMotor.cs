using UnityEngine;
using UnityEngine.AI;

public class CharacterMotor : MonoBehaviour {

    public event System.Action OnMovement = delegate { };

    protected GameObject target;

    protected bool moveToDestination = false;
    protected Vector3 destinationPosition = Vector3.zero;

    // when checking if destinationPosition matches NavMeshAgent destination, add a padding amount to account for destinations that are not precisely on the NavMesh
    protected float navMeshDistancePadding = 0.1f;

    protected ICharacterUnit characterUnit;

    // default value meant to be overwritten by a controller (AI/player)
    protected float movementSpeed = 1f;

    private bool frozen = false;

    private bool setMoveDestination = false;

    // properties
    public float MyMovementSpeed { get => movementSpeed; set => movementSpeed = value; }
    public GameObject MyTarget { get => target; }
    public bool MyFrozen { get => frozen; set => frozen = value; }
    public float MyNavMeshDistancePadding { get => navMeshDistancePadding; }

    protected virtual void Awake() {
        characterUnit = GetComponent<CharacterUnit>();
    }

    protected virtual void Start() {
        // meant to be overwritten
        //Debug.Log(gameObject.name + ".CharacterMotor.Start(): navhaspath: " + characterUnit.MyAgent.hasPath + "; isOnNavMesh: " + characterUnit.MyAgent.isOnNavMesh + "; pathpending: " + characterUnit.MyAgent.pathPending);
    }

    protected virtual void Update() {
        /*
        Debug.Log(gameObject.name + ".CharacterMotor.Update(): resetting velocity to 0");
        characterUnit.MyRigidBody.velocity = new Vector3(0, characterUnit.MyRigidBody.velocity.y + (- 9.81f * Time.deltaTime), 0);
        */
        //Debug.Log(gameObject.name + ".CharacterMotor.Update(): navhaspath: " + characterUnit.MyAgent.hasPath + "; isOnNavMesh: " + characterUnit.MyAgent.isOnNavMesh + "; pathpending: " + characterUnit.MyAgent.pathPending);
    }

    protected virtual void FixedUpdate() {
        //Debug.Log(gameObject.name + ".CharacterMotor.FixedUpdate(). current location: " + transform.position);
        //Debug.Log(gameObject.name + ".CharacterMotor.FixedUpdate(): navhaspath: " + characterUnit.MyAgent.hasPath + "; isOnNavMesh: " + characterUnit.MyAgent.isOnNavMesh + "; pathpending: " + characterUnit.MyAgent.pathPending);
        if (frozen) {
            return;
        }
        if (characterUnit.MyAgent != null && characterUnit.MyAgent.isActiveAndEnabled) {
            characterUnit.MyAgent.speed = characterUnit.MyCharacter.MyCharacterController.MyMovementSpeed;
        } else {
            //Debug.Log(gameObject.name + ": motor.FixedUpdate(): agent is disabled. Motor will do nothing");
            // unused gravity stuff
            //characterUnit.MyRigidBody.velocity = new Vector3(0, characterUnit.MyRigidBody.velocity.y + (-9.81f * Time.deltaTime), 0);
            /*
            Vector3 newRelativeForce = new Vector3(0, -(9.81f * 9.81f * Time.fixedDeltaTime), 0);
            Debug.Log(gameObject.name + ".CharacterMotor.FixedUpdate(): newRelativeForce: " + newRelativeForce);
            characterUnit.MyRigidBody.AddRelativeForce(newRelativeForce);
            */
            return;
        }

        if (target != null) {
            //Debug.Log(gameObject.name + ": CharacterMotor.FixedUpdate() target = " + target.name);
            // sometimes objects are above the ground.  if you don't ignore height you may try to move upward.
            // check if you are in the same spot as it and ignore height.
            //float distanceToTarget = Vector3.Distance(new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z), transform.position);
            //Debug.Log("Target is " + target.ToString() + " and target position is " + target.position.ToString() + " and my position is " + transform.position.ToString());
            //if (distanceToTarget > 0.1f) {
            if (characterUnit.MyCharacter.MyCharacterController.IsTargetInHitBox(target)) {
                //target.GetComponent<Interactable>().Interact(gameObject);
                StopFollowingTarget();
            } else {
                if (Vector3.Distance(target.transform.position, characterUnit.MyAgent.destination) > characterUnit.MyAgent.stoppingDistance * 2) {
                    //Debug.Log(gameObject.name + ": FixedUpdate() agent.destination: " + characterUnit.MyAgent.destination + " but should be " + target.transform.position + ". Issuing MoveToPoint()");
                    MoveToPoint(target.transform.position);
                } else {
                    //Debug.Log(gameObject.name + "(" + transform.position + "): CharacterMotor.Update(): destination is: " + agent.destination + "; target: " + target.transform.position);
                }
                //lastTargetLocation = target.transform.position;
                FaceTarget(target);
            }
            //} else if (moveToDestination == true) {
            // i think this next statement was the short version to stop agents getting stuck on corners so it continously updated the destination
        } else {
            if (moveToDestination == true && destinationPosition != characterUnit.MyAgent.destination) {
                //Debug.Log(gameObject.name + ": CharacterMotor.FixedUpdate(): moveToDestination: true. current location: " + transform.position + "; destinationPosition: " + destinationPosition + "; characterUnit.MyAgent.destination: " + characterUnit.MyAgent.destination + "; pathpending: " + characterUnit.MyAgent.pathPending);
                float agentDestinationDrift = Vector3.Distance(destinationPosition, characterUnit.MyAgent.destination);
                if (agentDestinationDrift >= (characterUnit.MyAgent.stoppingDistance + navMeshDistancePadding) && destinationPosition != characterUnit.MyAgent.destination) {
                    //Debug.Log(gameObject.name + ": FixedUpdate() Vector3.Distance(destinationPosition, characterUnit.MyAgent.destination): " + agentDestinationDrift + "; characterUnit.MyAgent.stoppingDistance: " + characterUnit.MyAgent.stoppingDistance);
                    //Debug.Log(gameObject.name + ": FixedUpdate() agent.destination: " + characterUnit.MyAgent.destination + " but should be point: " + destinationPosition + ". Issuing MoveToPoint()");
                    MoveToPoint(destinationPosition);
                } else {
                    //Debug.Log(gameObject.name + ": FixedUpdate() agent.destination: " + characterUnit.MyAgent.destination + " matches point (within stopping distance): " + destinationPosition + ". Disable moveToDestination boolean");
                    moveToDestination = false;
                }
            }
        }

        if (characterUnit.MyAgent.velocity.sqrMagnitude > 0) {
            OnMovement();
            if (characterUnit.MyCharacterAnimator != null) {
                characterUnit.MyCharacterAnimator.SetMoving(true);
                characterUnit.MyCharacterAnimator.SetVelocityZ(characterUnit.MyAgent.velocity.magnitude);
            }
        } else {
            if (characterUnit.MyCharacterAnimator != null) {
                characterUnit.MyCharacterAnimator.SetMoving(false);
                characterUnit.MyCharacterAnimator.SetVelocityZ(0);
            }
        }

        // set this here and let it actually take effect on the next fixedupdate.  This should have resulted in at least 1 fixedupdate for agent to let its path reset
        if (setMoveDestination) {
            //Debug.Log(gameObject.name + ": CharacterMotor.FixedUpdate(): setMoveDestination: true.  Set move destination: " + destinationPosition + "; current location: " + transform.position);
            moveToDestination = true;
            // check if position is valid
            NavMeshHit hit;
            if (NavMesh.SamplePosition(destinationPosition, out hit, 10.0f, NavMesh.AllAreas)) {
                //Debug.Log(gameObject.name + ": CharacterMotor.FixedUpdate(): destinationPosition " + destinationPosition + " on NavMesh found closest point: " + hit.position + ")");
                destinationPosition = hit.position;
            } else {
                Debug.Log(gameObject.name + ": CharacterMotor.FixedUpdate(): destinationPosition " + destinationPosition + " was not on NavMesh!");
            }
            //Debug.Log(gameObject.name + ": CharacterMotor.FixedUpdate(): calling: characterUnit.MyAgent.SetDestination(" + destinationPosition + ")");
            characterUnit.MyAgent.SetDestination(destinationPosition);
            setMoveDestination = false;
        }
    }

    private void SetMovePoint() {

    }

    public void FreezeCharacter() {
        //Debug.Log(gameObject.name + "CharacterMotor.FreezeCharacter()");
        characterUnit.MyAgent.enabled = false;
        frozen = true;
    }

    public void UnFreezeCharacter() {
        //Debug.Log(gameObject.name + "CharacterMotor.UnFreezeCharacter()");
        characterUnit.MyAgent.enabled = true;
        frozen = false;
    }

    // move toward the position at a normal speed
    public void MoveToPoint(Vector3 point) {
        //Debug.Log(gameObject.name + "CharacterMotor.MoveToPoint(" + point + "). current location: " + transform.position);
        if (frozen) {
            return;
        }

        if (!characterUnit.MyAgent.enabled) {
            //Debug.Log(gameObject.name + ".CharacterMotor.MoveToPoint(" + point + "): agent is disabled.  Will not give move instruction.");
            return;
        }

        // moving to a point only happens when we click on the ground.  Since we are not tracking a moving target, we can let the agent update the rotation
        characterUnit.MyAgent.updateRotation = true;
        //Debug.Log(gameObject.name + ".CharacterMotor.MoveToPoint(" + point + "): calling characterUnit.MyAgent.ResetPath()");
        characterUnit.MyAgent.ResetPath();
        destinationPosition = point;
        // set to false for test
        moveToDestination = false;
        setMoveDestination = true;
        // leaving this unset so it gets picked up in the next fixedupdate because navmeshagent doesn't actually reset path until after current frame.
        //characterUnit.MyAgent.SetDestination(point);
    }

    public void MoveToPosition(Vector3 newPosition) {
        //Debug.Log(gameObject.name + ".CharacterMotor.MoveToPosition(" + newPosition + ")");
        if (frozen) {
            return;
        }
        characterUnit.MyRigidBody.MovePosition(newPosition);
        OnMovement();
    }

    public Vector3 getVelocity() {
        return characterUnit.MyAgent.velocity;
    }

    public void Move(Vector3 moveDirection) {
        //Debug.Log(gameObject.name + ".CharacterMotor.Move(" + moveDirection + "). current position: " + transform.position);
        if (frozen) {
            return;
        }
        if (characterUnit.MyAgent.enabled) {

            //agent.Move(moveDirection);
            characterUnit.MyAgent.ResetPath();
            characterUnit.MyAgent.updateRotation = false;
            characterUnit.MyAgent.velocity = moveDirection;
        } else {
            //float currentYVelocity = moveDirection.y != 0 ? moveDirection.y : characterUnit.MyRigidBody.velocity.y;
            //Debug.Log("characterUnit.yVelocity is " + currentYVelocity);
            //Vector3 newMoveDirection = new Vector3(moveDirection.x, currentYVelocity, moveDirection.z);
            //Debug.Log(gameObject.name + ".CharacterMotor.Move() newMoveDirection: " + newMoveDirection + "; characterUnit.MyRigidBody.constraints: " + characterUnit.MyRigidBody.constraints);
            characterUnit.MyRigidBody.velocity = moveDirection;
            //characterUnit.MyRigidBody.MovePosition(transform.position + moveDirection);
            //characterUnit.MyRigidBody.AddForce(moveDirection, ForceMode.VelocityChange);
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
        characterUnit.MyRigidBody.AddRelativeForce(new Vector3(0, jumpSpeed, 0), ForceMode.VelocityChange);
    }

    public void RotateTowardsTarget(Vector3 targetPosition, float rotationSpeed) {
        //Debug.Log("RotateTowardsMovementTarget()");
        if (frozen) {
            return;
        }
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - new Vector3(transform.position.x, 0, transform.position.z));
        transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, (rotationSpeed * Time.deltaTime) * rotationSpeed);
    }

    public void RotateToward(Vector3 rotateDirection) {
        Debug.Log(gameObject.name + ".CharacterMotor.RotateToward(): " + rotateDirection);
        if (frozen) {
            return;
        }
        if (characterUnit.MyAgent.enabled) {
            //Debug.Log("nav mesh agent is enabled");
            characterUnit.MyAgent.ResetPath();
            characterUnit.MyAgent.updateRotation = true;
            characterUnit.MyAgent.velocity = rotateDirection;
        } else {
            Debug.Log("nav mesh agent is disabled");
            characterUnit.MyRigidBody.velocity = rotateDirection;
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
        characterUnit.MyAgent.stoppingDistance = 0.2f;
        //agent.stoppingDistance = myStats.hitBox;
        // moving to a target happens when we click on an interactable.  Since it might be moving, we will manually update the rotation every frame
        characterUnit.MyAgent.updateRotation = false;
        target = newTarget;
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
        if (characterUnit.MyAgent == null) {
            return;
        }
        if (characterUnit.MyAgent.isActiveAndEnabled) {
            characterUnit.MyAgent.stoppingDistance = 0.2f;
            characterUnit.MyAgent.updateRotation = true;
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
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
    }

    public void StartNavAgent() {
        //Debug.Log(gameObject.name + ".CharacterMotor.StartNavAgent()");
        if (!characterUnit.MyAgent.enabled) {
            characterUnit.MyAgent.enabled = true;
            characterUnit.MyRigidBody.isKinematic = true;
        }
    }

    public void StopNavAgent() {
        //Debug.Log(gameObject.name + ".CharacterMotor.StopNavAgent()");
        if (characterUnit.MyAgent.enabled) {
            characterUnit.MyAgent.enabled = false;
        }
    }

    public void ResetPath() {
        //Debug.Log(gameObject.name + ".CharacterMotor.ResetPath()");
        if (characterUnit.MyAgent.enabled == true) {
            //Debug.Log(gameObject.name + ".CharacterMotor.FixedUpdate(): navhaspath: " + characterUnit.MyAgent.hasPath + "; isOnNavMesh: " + characterUnit.MyAgent.isOnNavMesh + "; pathpending: " + characterUnit.MyAgent.pathPending);
            characterUnit.MyAgent.ResetPath();
            //Debug.Log(gameObject.name + ".CharacterMotor.FixedUpdate(): after reset: navhaspath: " + characterUnit.MyAgent.hasPath + "; isOnNavMesh: " + characterUnit.MyAgent.isOnNavMesh + "; pathpending: " + characterUnit.MyAgent.pathPending);
        }
    }
}
