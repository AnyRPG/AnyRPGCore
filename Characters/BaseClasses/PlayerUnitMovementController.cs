using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public enum RPGCharacterStateFREE {
    Idle = 0,
    Move = 1,
    Jump = 2,
    Fall = 4,
    Roll = 8
}

public class PlayerUnitMovementController : SuperStateMachine {

    // inform subscribers that we moved
    public event System.Action OnMovement = delegate { };

    //Components.
    private SuperCharacterController superCharacterController;
    private CharacterUnit characterUnit;

    public RPGCharacterStateFREE rpgCharacterState;

    [HideInInspector] public bool useMeshNav = false;
    [HideInInspector] public Vector3 lookDirection { get; private set; }

    //Jumping.
    [HideInInspector] public bool canJump;
    public float gravity = 50.0f;
    public float jumpAcceleration = 5.0f;
    public float jumpHeight = 3.0f;

    //Movement.
    [HideInInspector] public Vector3 currentMoveVelocity;
    [HideInInspector] public Vector3 currentTurnVelocity;
    [HideInInspector] public bool isMoving = false;
    public float rotationSpeed = 5f;

    //Air control.
    public float inAirSpeed = 6f;
    private float acquiringGroundDistance = 0.11f;
    private float maintainingGroundDistance = 0.5f;

    public float stepHeight = 0.5f;
    public float maxGroundAngle = 120;
    private float backwardGroundAngle;
    private float groundAngle;

    private bool tempGrounded;

    // technically, "front" and "forward" are the current direction of travel to detect obstacles no matter which way you are heading for these next 2 variables
    private bool nearFrontObstacle = false;
    // forward raycast length
    //private float rayCastLength = 0.5f;
    private float rayCastLength = 0.5f;


    public LayerMask groundMask;
    // downward raycast height
    private float rayCastHeight = 0.75f;
    public bool debug = true;

    private Vector3 acquiringGroundExtents = new Vector3(0.3f, 0.09f, 0.3f);
    private Vector3 maintainingGroundExtents = new Vector3(0.65f, 0.5f, 0.65f);

    // raycast to determine ground normal
    RaycastHit groundHitInfo;

    // raycasts to determine 
    RaycastHit downHitInfo;
    RaycastHit forwardHitInfo;

    // ensure that pressing forward moves us in the direction of the ground angle to avoid jittery movement on slopes
    private Vector3 forwardDirection;
    private Vector3 backwardDirection;

    // keep the player moving the same direction in the air
    private Vector3 airForwardDirection;
    private Quaternion airRotation;

    private List<ContactPoint> forwardContactPoints = new List<ContactPoint>();
    private List<ContactPoint> backwardContactPoints = new List<ContactPoint>();
    private List<ContactPoint> bottomContactPoints = new List<ContactPoint>();

    // the minimum height at which a collision is considered valid to calculate a forward or backward angle.  This is to prevent bottom collions that falsely register as front or back collisions
    private float collisionMinimumHeight = 0.05f;

    void Awake() {
        superCharacterController = GetComponent<SuperCharacterController>();
        characterUnit = GetComponent<CharacterUnit>();

        //Set currentState to idle on startup.
        currentState = RPGCharacterStateFREE.Idle;
        rpgCharacterState = RPGCharacterStateFREE.Idle;
        airForwardDirection = transform.forward;
    }

    private void Start() {
        SwitchCollisionOn();
    }

    //Put any code in here you want to run BEFORE the state's update function. This is run regardless of what state you're in.
    protected override void EarlyGlobalSuperUpdate() {
        //Debug.Log("EarlyGlobalSuperUpdate()");
        CalculateForward();
        CalculateBackward();
        CalculateGroundAngle();
        CheckGround();
        ApplyGravity();
        DrawDebugLines();
    }

    //Put any code in here you want to run AFTER the state's update function.  This is run regardless of what state you're in.
    protected override void LateGlobalSuperUpdate() {
        //Debug.Log("LateGlobalSuperUpdate()");

        //Move the player by our velocity every frame.
        // transform the velocity from local space to world space so we move the character forward on his z axis, not the global world z axis
        Vector3 relativeMovement = CharacterRelativeInput(currentMoveVelocity);
        if (currentMoveVelocity != Vector3.zero) {
            //Debug.Log("PlayerUnitMovementController.LateGlobalSuperUpdate() currentMoveVelocity: " + currentMoveVelocity);
        }
        if (relativeMovement.magnitude > 0.1 || (characterUnit.MyCharacter.MyCharacterController as PlayerController).inputJump) {
            //if (relativeMovement.magnitude != 0 || (characterUnit.MyCharacter.MyCharacterController as PlayerController).inputJump) {
            //if (relativeMovement.x != 0 || relativeMovement.z != 0) {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.LateGlobalSuperUpdate() relativeMovement: " + relativeMovement + "; inputJump: " + (characterUnit.MyCharacter.MyCharacterController as PlayerController).inputJump);
            characterUnit.MyCharacterMotor.Move(relativeMovement);
            //}
            /*
            if ((characterUnit.MyCharacter.MyCharacterController as PlayerController).inputJump) {
                characterUnit.MyCharacterMotor.Jump();
            }
            */
        } else {
            //Debug.Log("RelativeMovement.magnitude: " + relativeMovement.magnitude);
            Vector3 localVelocity = transform.InverseTransformDirection(characterUnit.MyRigidBody.velocity);
            if (localVelocity.x != 0f || localVelocity.z != 0f || localVelocity.y != 0f) {
                //Debug.Log("Character is moving at velocity: " + characterUnit.MyRigidBody.velocity + "; local: " + localVelocity + ", but no input was given.  Stopping Character!");
                characterUnit.MyCharacterMotor.Move(new Vector3(0, Mathf.Clamp(localVelocity.y, -53, 0), 0));
            }
        }

        //If alive and is moving, set animator.
        if (!useMeshNav && characterUnit.MyCharacter.MyCharacterStats.IsAlive && (characterUnit.MyCharacter.MyCharacterController as PlayerController).canMove) {

            // handle movement
            if (currentMoveVelocity.magnitude > 0 && (characterUnit.MyCharacter.MyCharacterController as PlayerController).HasMoveInput()) {
                if ((characterUnit.MyCharacter.MyCharacterController as PlayerController).inputStrafe == true) {
                    characterUnit.MyCharacterAnimator.SetStrafing(true);
                } else {
                    characterUnit.MyCharacterAnimator.SetStrafing(false);
                }
                isMoving = true;
                characterUnit.MyCharacterAnimator.SetMoving(true);
                //characterUnit.MyCharacterAnimator.SetVelocityZ(currentMoveVelocity.z);
                //characterUnit.MyCharacterAnimator.SetVelocityX(currentMoveVelocity.x);
                //characterUnit.MyCharacterAnimator.SetVelocityY(currentMoveVelocity.y);
                characterUnit.MyCharacterAnimator.SetVelocity(currentMoveVelocity);
            } else {
                isMoving = false;
                characterUnit.MyCharacterAnimator.SetMoving(false);
                characterUnit.MyCharacterAnimator.SetStrafing(false);
                //characterUnit.MyCharacterAnimator.SetVelocityZ(currentMoveVelocity.z);
                //characterUnit.MyCharacterAnimator.SetVelocityX(currentMoveVelocity.x);
                //characterUnit.MyCharacterAnimator.SetVelocityY(currentMoveVelocity.y);
                characterUnit.MyCharacterAnimator.SetVelocity(currentMoveVelocity);
            }
            characterUnit.MyCharacterAnimator.SetTurnVelocity(currentTurnVelocity.x);
        }

        if (characterUnit.MyCharacter.MyCharacterStats.IsAlive && (characterUnit.MyCharacter.MyCharacterController as PlayerController).canMove) {
            // code to prevent turning when clicking on UI elements
            if (InputManager.MyInstance.rightMouseButtonDown && (characterUnit.MyCharacter.MyCharacterController as PlayerController).HasMoveInput() && (!InputManager.MyInstance.rightMouseButtonClickedOverUI || (NamePlateCanvasController.MyInstance != null ? NamePlateCanvasController.MyInstance.MouseOverNamePlate() : false))) {
                //if (InputManager.MyInstance.rightMouseButtonDown && (characterUnit.MyCharacter.MyCharacterController as PlayerController).HasAnyInput()) {
                //transform.forward = new Vector3(CameraManager.MyInstance.MyMainCamera.transform.forward.x, 0, CameraManager.MyInstance.MyMainCamera.transform.forward.z);
                transform.forward = new Vector3(CameraManager.MyInstance.MyMainCameraController.MyWantedDirection.x, 0, CameraManager.MyInstance.MyMainCameraController.MyWantedDirection.z);
                //Debug.Log("PlayerUnitMovementController.LateGlobalSuperUpdate(): setting transform.forward: " + transform.forward);
                //characterUnit.MyCharacterMotor.RotateTowardsTarget(transform.TransformDirection(transform.forward), rotationSpeed);
                CameraManager.MyInstance.MyMainCamera.GetComponent<AnyRPGCameraController>().ResetWantedPosition();
            }

            if ((characterUnit.MyCharacter.MyCharacterController as PlayerController).inputTurn != 0) {
                //Debug.Log("inputTurn != 0; turning character");
                //characterUnit.MyCharacterMotor.Rotate(new Vector3(0, (characterUnit.MyCharacter.MyCharacterController as PlayerController).TurnInput.x, 0));
                characterUnit.MyCharacterMotor.Rotate(new Vector3(0, currentTurnVelocity.x, 0));
            }
        }

    }

    void EnterGroundStateCommon() {
        canJump = true;
        characterUnit.MyCharacterAnimator.SetJumping(0);
        airForwardDirection = transform.forward;
    }

    //Below are the state functions. Each one is called based on the name of the state, so when currentState = Idle, we call Idle_EnterState. If currentState = Jump, we call Jump_SuperUpdate()
    void Idle_EnterState() {
        //Debug.Log("Idle_EnterState() Freezing all constraints");
        superCharacterController.EnableSlopeLimit();
        characterUnit.MyRigidBody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

        // reset velocity from any falling movement that was happening
        currentMoveVelocity = Vector3.zero;
        EnterGroundStateCommon();
    }

    //Run every frame we are in the idle state.
    void Idle_SuperUpdate() {
        //Debug.Log("Idle_SuperUpdate() : canMove: " + (characterUnit.MyCharacter.MyCharacterController as PlayerController).canMove);
        //If Jump.
        if ((characterUnit.MyCharacter.MyCharacterController as PlayerController).allowedInput && (characterUnit.MyCharacter.MyCharacterController as PlayerController).inputJump) {
            currentState = RPGCharacterStateFREE.Jump;
            rpgCharacterState = RPGCharacterStateFREE.Jump;
            return;
        }
        if (!MaintainingGround()) {
            currentState = RPGCharacterStateFREE.Fall;
            rpgCharacterState = RPGCharacterStateFREE.Fall;
            return;
        }
        if (((characterUnit.MyCharacter.MyCharacterController as PlayerController).HasMoveInput() || (characterUnit.MyCharacter.MyCharacterController as PlayerController).HasTurnInput()) && (characterUnit.MyCharacter.MyCharacterController as PlayerController).canMove) {
            //Debug.Log("Idle_SuperUpdate. movingput. seting state move");
            currentState = RPGCharacterStateFREE.Move;
            rpgCharacterState = RPGCharacterStateFREE.Move;
            return;
        }
        //Apply friction to slow to a halt.
        //currentMoveVelocity = Vector3.MoveTowards(currentMoveVelocity, Vector3.zero, groundFriction * superCharacterController.deltaTime);
        // factor in slightly uneven ground which gravity will cause the unit to slide on even when standing still with position and rotation locked
        // DETECT SUPER LOW RIGIDBODY VELOCITY AND FREEZE CHARACTER
        if (Mathf.Abs(characterUnit.MyRigidBody.velocity.y) < 0.01 && MaintainingGround() == true) {
            //if (transform.InverseTransformPoint(downHitInfo.point).magnitude < 0.01) {
            //Debug.Log("Idle_SuperUpdate(). feet are planted on the ground, not applying downforce");
            currentMoveVelocity = new Vector3(0, 0, 0);
            // disable gravity while this close to the ground so we don't slide down slight inclines
            characterUnit.MyRigidBody.constraints = RigidbodyConstraints.FreezeAll;
        } else {
            //Debug.Log(gameObject.name + ".Idle_SuperUpdate(): magnitute of distance to ground: " + transform.InverseTransformPoint(downHitInfo.point).magnitude + "; velocityY: " + characterUnit.MyRigidBody.velocity.y + "; maintaining ground: " + MaintainingGround());
            
            // allow the character to fall until they reach the ground
            characterUnit.MyRigidBody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            //Debug.Log("Idle_SuperUpdate(). Moving: ground is at relative point: " + transform.InverseTransformPoint(downHitInfo.point));
            //Debug.Log("Idle_SuperUpdate(). Moving: groundx is at relative point: " + transform.InverseTransformPoint(downHitInfo.point).x);
            //Debug.Log("Idle_SuperUpdate(). Moving: groundy is at relative point: " + transform.InverseTransformPoint(downHitInfo.point).y);
            //Debug.Log("Idle_SuperUpdate(). Moving: groundz is at relative point: " + transform.InverseTransformPoint(downHitInfo.point).z);
            currentMoveVelocity = new Vector3(0, Mathf.Clamp(characterUnit.MyRigidBody.velocity.y, -53, 0), 0);
        }
        //Debug.Log("Idle_SuperUpdate(): not switching states, current move velocity clamped to: " + currentMoveVelocity);
    }

    void Idle_ExitState() {
        //Debug.Log("Idle_ExitState(). Freezing Rotation only");
        characterUnit.MyRigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        //Run once when exit the idle state.
    }

    void Move_EnterState() {
        //Debug.Log("Move_EnterState()");
        EnterGroundStateCommon();
    }

    void Move_SuperUpdate() {
        //Debug.Log("Move_SuperUpdate()");

        airForwardDirection = transform.forward;
        airRotation = transform.rotation;
        //Debug.Log("Move_SuperUpdate(): airForwardDirection: " + airForwardDirection);
        if ((characterUnit.MyCharacter.MyCharacterController as PlayerController).allowedInput && (characterUnit.MyCharacter.MyCharacterController as PlayerController).inputJump) {
            currentState = RPGCharacterStateFREE.Jump;
            rpgCharacterState = RPGCharacterStateFREE.Jump;
            return;
        }
        if (!MaintainingGround()) {
            //Debug.Log("Move_SuperUpdate(): !MaintainingGround(): entering Fall State! transform position: " + transform.position + "; ground: " + groundHitInfo.point + "; local ground: " + transform.InverseTransformPoint(groundHitInfo.point));
            currentState = RPGCharacterStateFREE.Fall;
            rpgCharacterState = RPGCharacterStateFREE.Fall;
            return;
        }

        //Set speed determined by movement type.

        // since we are in the move state, reset velocity to zero so we can pick up the new values
        // allow falling while moving by clamping existing y velocity
        currentMoveVelocity = new Vector3(0, Mathf.Clamp(characterUnit.MyRigidBody.velocity.y, -53, 0), 0);

        if (((characterUnit.MyCharacter.MyCharacterController as PlayerController).HasMoveInput() || (characterUnit.MyCharacter.MyCharacterController as PlayerController).HasTurnInput()) && (characterUnit.MyCharacter.MyCharacterController as PlayerController).canMove) {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.Move_SuperUpdate(): HasMoveInput or hasTurnInput = true");

            // set clampValue to default of max movement speed
            float clampValue = PlayerManager.MyInstance.MyMaxMovementSpeed;

            // set a clamp value to limit movement speed to walking if going backward
            /*
            if (currentMoveVelocity.z < 0) {
                clampValue = 1;
            }
            */

            // get current movement speed and clamp it to current clamp value
            float calculatedSpeed = PlayerManager.MyInstance.MyCharacter.MyCharacterController.MyMovementSpeed;
            calculatedSpeed = Mathf.Clamp(calculatedSpeed, 0, clampValue);

            if ((characterUnit.MyCharacter.MyCharacterController as PlayerController).HasMoveInput()) {
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.Move_SuperUpdate(): HasMoveInput = true");
                // multiply normalized movement by calculated speed to get actual movement
                currentMoveVelocity = LocalMovement() * calculatedSpeed;
            }
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.Move_SuperUpdate(): Set currentMoveVelocity: " + currentMoveVelocity);
            if ((characterUnit.MyCharacter.MyCharacterController as PlayerController).HasTurnInput()) {
                //currentTurnVelocity = (characterUnit.MyCharacter.MyCharacterController as PlayerController).TurnInput * calculatedSpeed * ((PlayerPrefs.GetFloat("KeyboardTurnSpeed") * 1) + 1.0f);
                currentTurnVelocity = (characterUnit.MyCharacter.MyCharacterController as PlayerController).TurnInput * ((PlayerPrefs.GetFloat("KeyboardTurnSpeed") * 5) + 6.0f);
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.Move_SuperUpdate(): Set currentTurnVelocity: " + currentTurnVelocity);
            }
        } else {
            /*
            Debug.Log("Entering Idle State!: moveInput: " + (characterUnit.MyCharacter.MyCharacterController as PlayerController).HasMoveInput());
            Debug.Log("Entering Idle State!: turnInput: " + (characterUnit.MyCharacter.MyCharacterController as PlayerController).HasTurnInput());
            Debug.Log("Entering Idle State!: canMove: " + (characterUnit.MyCharacter.MyCharacterController as PlayerController).canMove);
            */
            currentTurnVelocity = Vector3.zero;
            currentState = RPGCharacterStateFREE.Idle;
            rpgCharacterState = RPGCharacterStateFREE.Idle;
            return;
        }
    }

    void Jump_EnterState() {
        //Debug.Log(gameObject.name + ".Jump_EnterState(). currentMoveVelocity: " + currentMoveVelocity);
        superCharacterController.DisableSlopeLimit();
        //currentMoveVelocity += superCharacterController.up * CalculateJumpSpeed(jumpHeight, gravity);
        currentMoveVelocity.y = (Vector3.up * jumpAcceleration).y;
        //Debug.Log(gameObject.name + ".Jump_EnterState(). new currentMoveVelocity: " + currentMoveVelocity);
        canJump = false;
        characterUnit.MyCharacterAnimator.SetInteger("Jumping", 1);
        characterUnit.MyCharacterAnimator.SetTrigger("JumpTrigger");
    }

    void Jump_SuperUpdate() {
        //Debug.Log(gameObject.name + ".Jump_SuperUpdate(). currentMoveVelocity: " + currentMoveVelocity + "; rigidbody world velocity: " + characterUnit.MyRigidBody.velocity + "; airForwardDirection: " + airForwardDirection + "; transform.Forward: " + transform.forward);
        // new code to allow bouncing off walls instead of getting stuck flying into them
        //currentMoveVelocity = CharacterRelativeInput(transform.InverseTransformDirection(characterUnit.MyRigidBody.velocity));
        Vector3 airForwardVelocity = Quaternion.LookRotation(airForwardDirection, Vector3.up) * characterUnit.MyRigidBody.velocity;
        currentMoveVelocity = transform.InverseTransformDirection(characterUnit.MyRigidBody.velocity);
        Vector3 inverseMoveVelocity = Quaternion.Inverse(airRotation) * transform.InverseTransformDirection(characterUnit.MyRigidBody.velocity);
        Vector3 fromtoMoveVelocity1 = Quaternion.FromToRotation(airForwardDirection, transform.forward) * transform.InverseTransformDirection(characterUnit.MyRigidBody.velocity);
        Vector3 fromtoMoveVelocity2 = Quaternion.FromToRotation(transform.forward, airForwardDirection) * transform.InverseTransformDirection(characterUnit.MyRigidBody.velocity);
        Vector3 fromtoMoveVelocity3 = Quaternion.Inverse(Quaternion.FromToRotation(airForwardDirection, transform.forward)) * transform.InverseTransformDirection(characterUnit.MyRigidBody.velocity);
        Vector3 fromtoMoveVelocity4 = Quaternion.Inverse(Quaternion.FromToRotation(transform.forward, airForwardDirection)) * transform.InverseTransformDirection(characterUnit.MyRigidBody.velocity);
        //Debug.Log(gameObject.name + ".Jump_SuperUpdate(). rigidBodyVelocityToLocal: " + currentMoveVelocity + "; airForwardVelocity" + airForwardVelocity + "; inverseMoveVelocity: " + inverseMoveVelocity + "; fromtoMoveVelocities: " + fromtoMoveVelocity1 + ", " + fromtoMoveVelocity2 + ", " + fromtoMoveVelocity3 + ", " + fromtoMoveVelocity4);
        currentMoveVelocity = fromtoMoveVelocity1;
        //currentMoveVelocity = new Vector3(currentMoveVelocity.x, characterUnit.MyRigidBody.velocity.y, currentMoveVelocity.z);
        //Vector3 planarMoveDirection = Math3d.ProjectVectorOnPlane(superCharacterController.up, currentMoveVelocity);
        //Debug.Log(gameObject.name + ".Jump_SuperUpdate(). planarMoveDirection: " + planarMoveDirection);
        //Vector3 verticalMoveDirection = currentMoveVelocity - planarMoveDirection;
        //Debug.Log(gameObject.name + ".Jump_SuperUpdate(). verticalMoveDirection: " + verticalMoveDirection);
        if (AcquiringGround() && characterUnit.MyRigidBody.velocity.y < 0.1) {
            //if (Vector3.Angle(verticalMoveDirection, superCharacterController.up) > 90 && AcquiringGround()) {
            //currentMoveVelocity = planarMoveDirection;
            if (((characterUnit.MyCharacter.MyCharacterController as PlayerController).HasMoveInput() || (characterUnit.MyCharacter.MyCharacterController as PlayerController).HasTurnInput()) && (characterUnit.MyCharacter.MyCharacterController as PlayerController).canMove) {
                // new code to allow not freezing up when landing
                currentState = RPGCharacterStateFREE.Move;
                rpgCharacterState = RPGCharacterStateFREE.Move;
                return;
            }
            currentState = RPGCharacterStateFREE.Idle;
            rpgCharacterState = RPGCharacterStateFREE.Idle;
            return;
        }
        //planarMoveDirection = Vector3.MoveTowards(planarMoveDirection, LocalMovement() * inAirSpeed, jumpAcceleration * superCharacterController.deltaTime);
    }

    void Fall_EnterState() {
        //Debug.Log("Fall_EnterState()");
        superCharacterController.DisableSlopeLimit();
        canJump = false;
        characterUnit.MyCharacterAnimator.SetInteger("Jumping", 2);
        characterUnit.MyCharacterAnimator.SetTrigger("JumpTrigger");
    }

    void Fall_SuperUpdate() {
        //Debug.Log(gameObject.name + ".PlayerUnitMovementController.Fall_SuperUpdate(). currentMoveVelocity: " + currentMoveVelocity);
        // new code to allow bouncing off walls instead of getting stuck flying into them
        Vector3 fromtoMoveVelocity1 = Quaternion.FromToRotation(airForwardDirection, transform.forward) * transform.InverseTransformDirection(characterUnit.MyRigidBody.velocity);
        //currentMoveVelocity = transform.InverseTransformDirection(CharacterRelativeInput(characterUnit.MyRigidBody.velocity));
        currentMoveVelocity = new Vector3(fromtoMoveVelocity1.x, Mathf.Clamp(fromtoMoveVelocity1.y, -53, 0), fromtoMoveVelocity1.z);
        //currentMoveVelocity = new Vector3(currentMoveVelocity.x, 0, currentMoveVelocity.z);
        if (AcquiringGround()) {
            //currentMoveVelocity = Math3d.ProjectVectorOnPlane(superCharacterController.up, currentMoveVelocity);
            if (((characterUnit.MyCharacter.MyCharacterController as PlayerController).HasMoveInput() || (characterUnit.MyCharacter.MyCharacterController as PlayerController).HasTurnInput()) && (characterUnit.MyCharacter.MyCharacterController as PlayerController).canMove) {
                // new code to allow not freezing up when landing
                currentState = RPGCharacterStateFREE.Move;
                rpgCharacterState = RPGCharacterStateFREE.Move;
                return;
            }
            currentState = RPGCharacterStateFREE.Idle;
            rpgCharacterState = RPGCharacterStateFREE.Idle;
            return;
        }
    }

    public void SwitchCollisionOn() {
        if (characterUnit.MyCharacter != null) {
            (characterUnit.MyCharacter.MyCharacterController as PlayerController).canMove = true;
            superCharacterController.enabled = true;
            characterUnit.MyCharacterAnimator.DisableRootMotion();
        }
    }

    Vector3 CharacterRelativeInput(Vector3 inputVector) {
        //Debug.Log("PlayerUnitMovementController.CharacterRelativeInput(" + inputVector + ")");
        // switch to quaternion rotation instead of transformDirection so direction can be maintained in air no matter which way player faces in air
        Vector3 qRelativeVelocity = Vector3.zero;
        if (inputVector != Vector3.zero) {
            qRelativeVelocity = Quaternion.LookRotation(airForwardDirection, Vector3.up) * inputVector;
        }
        Vector3 tRelativeVelocity = transform.TransformDirection(inputVector);
        if (qRelativeVelocity != Vector3.zero && tRelativeVelocity != Vector3.zero) {
            //Debug.Log("CharacterRelativeInput(" + inputVector + "): qRelativeVelocity: " + qRelativeVelocity + "; tRelativeVelocity: " + tRelativeVelocity);
        }
        return qRelativeVelocity;
    }

    Vector3 WorldRelativeInput(float inputX, float inputZ) {
        Vector3 relativeVelocity = new Vector3(inputX, 0, inputZ);
        return relativeVelocity;
    }

    /// <summary>
    /// Movement based off camera facing.
    /// </summary>
    Vector3 CameraRelativeInput(float inputX, float inputZ) {
        //Forward vector relative to the camera along the x-z plane   
        Vector3 forward = Camera.main.transform.TransformDirection(Vector3.forward);
        forward.y = 0;
        forward = forward.normalized;
        //Right vector relative to the camera always orthogonal to the forward vector.
        Vector3 right = new Vector3(forward.z, 0, -forward.x);
        Vector3 relativeVelocity = inputX * right + inputZ * forward;

        return relativeVelocity;
    }

    private bool AcquiringGround() {
        //Debug.Log("PlayerUnitMovementController.AcquiringGround()");
        Collider[] hitColliders = Physics.OverlapBox(transform.position, acquiringGroundExtents, Quaternion.identity, groundMask);
        /*
        foreach (Collider hitCollider in hitColliders) {
            //Debug.Log("Overlap Box Hit : " + hitColliders[i].name + i);
            if (((1 << hitCollider.gameObject.layer) & groundMask) != 0) {
                return true;
            }
        }
        */
        if (hitColliders.Length > 0) {
            //Debug.Log("PlayerUnitMovementController.AcquiringGround(): Grounded!");
            return true;
        }
        return false;
        //return superCharacterController.currentGround.IsGrounded(false, acquiringGroundDistance);
    }

    public bool MaintainingGround() {
        //return superCharacterController.currentGround.IsGrounded(true, maintainingGroundDistance);
        return tempGrounded;
    }

    private Vector3 LocalMovement() {
        //Debug.Log("PlayerUnitMovementController.LocalMovement(): groundAngle: " + groundAngle + "; backwardGroundAngle: " + backwardGroundAngle);
        Vector3 normalizedInput = (characterUnit.MyCharacter.MyCharacterController as PlayerController).NormalizedMoveInput;

        // testing applying downforce on ground that is sloped downward - can't do it because that will later be rotated which could result in the "down" force moving backward at angles beyond 45degrees
        if (groundAngle == 0 && backwardGroundAngle == 0 && nearFrontObstacle == false) {
            //if (groundHitInfo.normal == Vector3.up) {
            // this should make the character stick to the ground better when actively moving while grounded
            // ONLY APPLY Y DOWNFORCE ON FLAT GROUND, this will apply a y downforce multiplied by speed, not the existing y downforce from physics (gravity)
            float yValue = 0f;
            if (transform.InverseTransformPoint(groundHitInfo.point) != Vector3.zero) {
                yValue = Mathf.Clamp(characterUnit.MyRigidBody.velocity.normalized.y, -1, 0);
                //Debug.Log("LocalMovement(): We are above the (flat) ground and there are no near collisions.  Applying extra ground force: " + yValue);
            }
            normalizedInput = new Vector3(normalizedInput.x, yValue, normalizedInput.z);
        }
        Vector3 newReturnValue;
        float usedAngle = 0f;
        if (normalizedInput.z > 0) {
            usedAngle = groundAngle;
            if (!nearFrontObstacle && forwardContactPoints.Count == 0) {
                // moving forward, use forward angle calculated to get over objects
                // hopefully this still allows us the correct ground angle when going downhill with no obstacles in front
                if (groundAngle > 0) {
                    // code to stop going up if standing with center over slope, but front of feet on flat surface with no obstacles in front
                    //Debug.Log("PlayerUnitMovementController.LocalMovement(): no front obstacles, ignoring ground angle because we are likely above the ground");
                    usedAngle = 0f;
                }
            }
            newReturnValue = Quaternion.AngleAxis(usedAngle, -Vector3.right) * normalizedInput;
        } else {
            usedAngle = backwardGroundAngle;
            // moving forward, use forward angle calculated to get over objects
            if (!nearFrontObstacle) {
                // hopefully this still allows us the correct ground angle when going downhill with no obstacles in front
                if (backwardGroundAngle > 0) {
                    // code to stop going up if standing with center over slope, but front of feet on flat surface with no obstacles in front
                    //Debug.Log("PlayerUnitMovementController.LocalMovement(): no front obstacles, ignoring ground angle because we are likely above the ground");
                    usedAngle = 0f;
                }
            }
            newReturnValue = Quaternion.AngleAxis(usedAngle, Vector3.right) * normalizedInput;
        }
        //Debug.Log("PlayerUnitMovementController.LocalMovement(): normalizedInput: " + normalizedInput + "; usedAngle: " + usedAngle + "; AngleAxis: " + Quaternion.AngleAxis(usedAngle, -Vector3.right) + "; newReturnValue: " + newReturnValue);
        return newReturnValue;
    }

    // Calculate the initial velocity of a jump based off gravity and desired maximum height attained
    private float CalculateJumpSpeed(float jumpHeight, float gravity) {
        return Mathf.Sqrt(2 * jumpHeight * gravity);
    }

    private void CalculateForward() {
        if (!MaintainingGround()) {
            forwardDirection = airForwardDirection;
            //forwardDirection = transform.forward;
            return;
        }

        // PUT CODE HERE TO RECOGNIZE THE HIGHEST ANGLE AS THE FORWARD DIRECTION

        if (forwardContactPoints.Count > 0) {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateForward(): forwardContactPoints.Count: " + forwardContactPoints.Count);
            int counter = 0;
            int smallestIndex = -1;
            
            // find highest contact point
            foreach (ContactPoint contactPoint in forwardContactPoints) {
                Vector3 localContactPoint = transform.InverseTransformPoint(contactPoint.point);
                // ensure forward contact point is above a certain height and actually in front of the character
                if (localContactPoint.y > collisionMinimumHeight && localContactPoint != Vector3.zero) {
                    if (smallestIndex == -1 || localContactPoint.y > transform.InverseTransformPoint(forwardContactPoints[smallestIndex].point).y) {
                        //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateForward(): found highest contact point at: " + contactPoint.point + "; local: " + localContactPoint);
                        smallestIndex = counter;
                    }
                }
                counter++;
            }

            if (smallestIndex != -1) {
                // get vector between contact point and base of player
                Vector3 directionToContact = (forwardContactPoints[smallestIndex].point - transform.position).normalized;
                forwardDirection = directionToContact;
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateForward(): Vector3.Cross(downHitInfo.normal(" + downHitInfo.normal + "), -transform.right(" + -transform.right + ")): " + forwardDirection);
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateForward(): directionToContact: " + directionToContact + "; forwardDirection: " + forwardDirection);
                return;
            }
        }

        if (nearFrontObstacle && groundHitInfo.normal == Vector3.up) {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateForward(): near front obstacle is true and we didn't get any useful information from the ground normal, trying obstacle normal");
            forwardDirection = Vector3.Cross(forwardHitInfo.normal, -transform.right);
            return;
        }

        forwardDirection = Vector3.Cross(groundHitInfo.normal, -transform.right);
        //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateForward(): no forward collisions. forwardDirection = Vector3.Cross(downHitInfo.normal(" + downHitInfo.normal + "), -transform.right(" + -transform.right + ")): " + forwardDirection);
        return;

    }

    private void CalculateBackward() {
        if (!MaintainingGround()) {
            backwardDirection = -airForwardDirection;
            //backwardDirection = -transform.forward;
            return;
        }
        if (backwardContactPoints.Count > 0) {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateBackward(): rearContactPoints.Count: " + rearContactPoints.Count);
            int counter = 0;
            int smallestIndex = 0;

            // find highest contact point
            foreach (ContactPoint contactPoint in backwardContactPoints) {
                Vector3 localContactPoint = transform.InverseTransformPoint(contactPoint.point);
                if (localContactPoint.y > collisionMinimumHeight && localContactPoint != Vector3.zero) {
                    if (smallestIndex == -1 || localContactPoint.y > transform.InverseTransformPoint(backwardContactPoints[smallestIndex].point).y) {
                        //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateBackward(): found highest contact point");
                        smallestIndex = counter;
                    }
                }
                counter++;
            }
            if (smallestIndex != -1) {
                // get angle between contact point and base of player
                Vector3 directionToContact = (backwardContactPoints[smallestIndex].point - transform.position).normalized;
                backwardDirection = directionToContact;
                //forwardDirection = Vector3.Cross(directionToContact, -transform.right);
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateBackward(): directionToContact: " + directionToContact + "; rearDirection: " + backwardDirection);
                return;
            }
        }

        if (nearFrontObstacle && groundHitInfo.normal == Vector3.up) {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateForward(): near front obstacle is true and we didn't get any useful information from the ground normal, trying obstacle normal");
            backwardDirection = Vector3.Cross(forwardHitInfo.normal, transform.right);
            return;
        }

        backwardDirection = Vector3.Cross(groundHitInfo.normal, transform.right);
        //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateForward(): no backward collisions. backwardDirection = Vector3.Cross(downHitInfo.normal(" + downHitInfo.normal + "), transform.right(" + transform.right + ")): " + backwardDirection);
        return;
    }


    private void CalculateGroundAngle() {
        if (!MaintainingGround()) {
            //groundAngle = 90;
            groundAngle = 0;
            return;
        }
        /*
        if (hitInfo != null) {
            Debug.Log("hitInfo: " + hitInfo.collider.gameObject.name + "; normal: " + hitInfo.normal);
        }
        */
        float downHitAngle = Vector3.Angle(groundHitInfo.normal, transform.forward) - 90f;
        //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateGroundAngle() from downHitInfo.normal(" + downHitInfo.normal + "): " + downHitAngle);
        float forwardHitAngle = Vector3.Angle(forwardHitInfo.normal, transform.forward) - 90f;
        //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateGroundAngle() from forwardHitInfo.normal(" + forwardHitInfo.normal + "): " + forwardHitAngle);

        //groundAngle = Vector3.Angle(forwardDirection, transform.forward) + 90;
        float forwardcollisionAngle = Vector3.Angle(forwardDirection, transform.forward) * (forwardDirection.y < 0 ? -1 : 1);

        // get ground angle from downhit
        // because downhit angle is based on fowardcollissionAngle it can return 90 even on flat ground. this is ok because we will use one of the collission points that is lower than step height
        // to get a reasonable angle (or the raycast, not sure)
        groundAngle = Mathf.Max(downHitAngle, forwardcollisionAngle);
        //Debug.Log("Initial groundAngle from downHitAngle (" + groundAngle + ") using forwardcollisionAngle: " + forwardcollisionAngle);

        // we are near an obstacle, let's adjust our angle upward to avoid a head on collission
        if (nearFrontObstacle && forwardHitAngle < 60f && groundAngle > 60f) {
            // if ground angle is too big and forward hit angle is ok, use forward hit
            groundAngle = forwardHitAngle;
        } else if (nearFrontObstacle && forwardHitAngle < 60f && groundAngle < 60f) {
            // if both angles are ok, use the biggest one
            groundAngle = Mathf.Max(forwardHitAngle, groundAngle);
        }
        // our original downhit angle was too big, and our forward hit angle was too big, we need to clamp to prevent flying straight upward or climbing a slope more than 60 degrees
        if (groundAngle > 60f) {
            //Debug.Log("Warning, groundAngle after all checking > 60f (" + groundAngle + ")! clamping!  This is likely because you are standing in front of an object that is vertical at the raycast hit point");
            groundAngle = 60f;
        }
        if (groundAngle != 0) {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateGroundAngle() from forwardDirection(" + forwardDirection + "): " + forwardcollisionAngle + "; from downhitInfo: " + downHitAngle);
        }

        backwardGroundAngle = Vector3.Angle(backwardDirection, -transform.forward) * (backwardDirection.y < 0 ? -1 : 1);
        if (backwardGroundAngle != 0) {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateGroundAngle() from backwardDirection(" + backwardDirection + "): " + backwardGroundAngle);
        }
    }

    private void CheckGround() {
        // downward cast for grounding
        if (Physics.Raycast(transform.position + (Vector3.up * 0.25f), -Vector3.up, out downHitInfo, rayCastHeight, groundMask)) {
            tempGrounded = true;
        } else {
            tempGrounded = false;
        }

        // downward cast for normals
        Physics.Raycast(transform.position + (Vector3.up * 0.25f), -Vector3.up, out groundHitInfo, Mathf.Infinity, groundMask);

        if (bottomContactPoints.Count > 0 || forwardContactPoints.Count > 0 || backwardContactPoints.Count > 0) {
            // extra check to catch contact points below maximum step height in case the character is halfway off a slope
            tempGrounded = true;
        }

        Collider[] hitColliders = Physics.OverlapBox(transform.position, maintainingGroundExtents, Quaternion.identity);
        foreach (Collider hitCollider in hitColliders) {
            //Debug.Log("Overlap Box Hit : " + hitColliders[i].name + i);
            if (((1 << hitCollider.gameObject.layer) & groundMask) != 0) {
                tempGrounded = true;
            }
        }


        // forward cast
        Vector3 directionOfTravel = transform.forward;
        if (currentMoveVelocity.x != 0 || currentMoveVelocity.z != 0) {
            directionOfTravel = transform.TransformDirection(new Vector3(currentMoveVelocity.x, 0, currentMoveVelocity.z)).normalized;
        }
        Debug.DrawLine(transform.position + (Vector3.up * 0.05f), transform.position + (Vector3.up * 0.05f) + (directionOfTravel * rayCastLength), Color.black);
        if (Physics.Raycast(transform.position + (Vector3.up * 0.05f), directionOfTravel, out forwardHitInfo, rayCastLength, groundMask)) {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CheckGround(): There is an obstacle in front of the player: " + forwardHitInfo.collider.gameObject.name + "; normal: " + forwardHitInfo.normal);
            nearFrontObstacle = true;
        } else {
            nearFrontObstacle = false;
        }

    }

    private void ApplyGravity() {
        if (!tempGrounded) {
            //Debug.Log("PlayerUnitMovementController.ApplyGravity(): Not Grounded");
        }
    }

    private void DrawDebugLines() {
        if (!debug) {
            return;
        }

        Debug.DrawLine(transform.position, transform.position + forwardDirection * rayCastHeight * 2, Color.blue);
        Debug.DrawLine(transform.position, transform.position + backwardDirection * rayCastHeight * 2, Color.magenta);
        Debug.DrawLine(transform.position + (Vector3.up * 0.25f), (transform.position + (Vector3.up * 0.25f)) - (Vector3.up * rayCastHeight), Color.green);

    }

    public void OnCollisionEnter(Collision collision) {
        //Debug.Log(gameObject.name + ".PlayerUnitMovementController.OnCollisionEnter()");
        DebugCollision(collision);
    }

    public void OnCollisionStay(Collision collision) {
        //Debug.Log(gameObject.name + ".PlayerUnitMovementController.OnCollisionStay(): " + collision.collider.gameObject.name);
        DebugCollision(collision);
    }

    private void DebugCollision(Collision collision) {
        ContactPoint[] contactPoints = new ContactPoint[collision.contactCount];
        collision.GetContacts(contactPoints);
        foreach (ContactPoint contactPoint in contactPoints) {
            if (((1 << collision.gameObject.layer) & groundMask) != 0) {
                //Debug.Log(gameObject.name + ".CharacterUnit.OnCollisionStay(): " + collision.collider.gameObject.name + " matched the ground Layer mask at : " + contactPoint.point + "; player: " + transform.position);
                float hitAngle = Vector3.Angle(contactPoint.normal, transform.forward);
                //Debug.Log(gameObject.name + ".CharacterUnit.OnCollisionStay(): " + collision.collider.gameObject.name + "; normal: " + contactPoint.normal + "; angle: " + hitAngle);
                Vector3 relativePoint = transform.InverseTransformPoint(contactPoint.point);
                //Debug.Log(gameObject.name + ".CharacterUnit.OnCollisionStay(): " + collision.collider.gameObject.name + "; relativePoint: " + relativePoint);
                if (relativePoint.z > 0 && relativePoint.y < stepHeight) {
                    //Debug.Log(gameObject.name + ".CharacterUnit.DebugCollision(): " + collision.collider.gameObject.name + "; relativePoint: " + relativePoint + " is in front of the player!");
                    if (!forwardContactPoints.Contains(contactPoint)) {
                        forwardContactPoints.Add(contactPoint);
                    }
                } else if (relativePoint.z < 0 && relativePoint.y < stepHeight) {
                    //Debug.Log(gameObject.name + ".CharacterUnit.DebugCollision(): " + collision.collider.gameObject.name + "; relativePoint: " + relativePoint + " is behind the player!");
                    if (!backwardContactPoints.Contains(contactPoint)) {
                        backwardContactPoints.Add(contactPoint);
                    }
                } else if (relativePoint.y < stepHeight) {
                    //Debug.Log(gameObject.name + ".CharacterUnit.DebugCollision(): " + collision.collider.gameObject.name + "; relativePoint: " + relativePoint + " is under the player!");
                    if (!bottomContactPoints.Contains(contactPoint)) {
                        bottomContactPoints.Add(contactPoint);
                    }
                } else {
                    //Debug.Log(gameObject.name + ".CharacterUnit.OnCollisionStay(): " + collision.collider.gameObject.name + "; relativePoint: " + relativePoint + " is NOT in front of or behind the player or is higher than the stepheight!");
                }
            }
            Debug.DrawLine(transform.position, contactPoint.point, Color.yellow);
        }
    }

    private void FixedUpdate() {
        //Debug.Log(gameObject.name + ".PlayerUnitMovementController.FixedUpdate(): forwardContactPoints.Clear()");
        forwardContactPoints.Clear();
        backwardContactPoints.Clear();
        bottomContactPoints.Clear();
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, acquiringGroundExtents * 2);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, maintainingGroundExtents * 2);
    }

}
