using AnyRPG;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UMA;
using UMA.CharacterSystem;
using UnityEngine.EventSystems;

namespace AnyRPG {
    public enum AnyRPGCharacterState {
        Idle = 0,
        Move = 1,
        Jump = 2,
        Knockback = 3,
        Fall = 4,
        Roll = 8
    }
    [RequireComponent(typeof(CharacterController))]
    public class PlayerUnitMovementController : AnyRPGStateMachine {

        //Components.
        private CharacterController characterController;
        private CharacterUnit characterUnit;

        public AnyRPGCharacterState rpgCharacterState;

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
        //private float acquiringGroundDistance = 0.11f;
        //private float maintainingGroundDistance = 0.5f;

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


        private LayerMask groundMask;

        // downward raycast height
        private float rayCastHeight = 0.75f;
        public bool debug = true;

        private Vector3 acquiringGroundExtents = new Vector3(0.3f, 0.09f, 0.3f);
        private Vector3 maintainingGroundExtents = new Vector3(0.65f, 0.5f, 0.65f);

        // raycast to determine ground normal
        private RaycastHit groundHitInfo;

        // raycasts to determine 
        private RaycastHit downHitInfo;
        private RaycastHit forwardHitInfo;

        // ensure that pressing forward moves us in the direction of the ground angle to avoid jittery movement on slopes
        private Vector3 forwardDirection;
        private Vector3 backwardDirection;

        // keep the player moving the same direction in the air
        private Vector3 airForwardDirection;
        private Quaternion airRotation;

        // the frame in which the player last entered a jump state
        private int lastJumpFrame;

        private List<ContactPoint> forwardContactPoints = new List<ContactPoint>();
        private List<ContactPoint> backwardContactPoints = new List<ContactPoint>();
        private List<ContactPoint> bottomContactPoints = new List<ContactPoint>();

        // the minimum height at which a collision is considered valid to calculate a forward or backward angle.  This is to prevent bottom collions that falsely register as front or back collisions
        private float collisionMinimumHeight = 0.05f;

        private bool rotateModel = false;


        private void Awake() {
            if (PlayerManager.MyInstance != null) {
                groundMask = PlayerManager.MyInstance.DefaultGroundMask;
            }
        }

        private void Start() {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.Start()");
            //Set currentState to idle on startup.
            airForwardDirection = transform.forward;

            SwitchCollisionOn();
            SetupRotateModel();
        }

        public void SetupRotateModel() {
            CharacterUnit tmpUnit = GetComponent<CharacterUnit>();
            DynamicCharacterAvatar dynamicCharacterAvatar = GetComponent<DynamicCharacterAvatar>();
            if (tmpUnit == null && dynamicCharacterAvatar == null) {
                rotateModel = true;
            }
        }

        public void OrchestratorStart() {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.OrchestrateStartup()");
            GetComponentReferences();
        }

        public void OrchestratorFinish() {
            ConfigureStateMachine();
        }

        public void ConfigureStateMachine() {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.ConfigureStateMachine()");
            currentState = AnyRPGCharacterState.Idle;
            rpgCharacterState = AnyRPGCharacterState.Idle;
            if (characterController != null) {
                characterController.OrchestrateStartup();
            }
        }

        public void GetComponentReferences() {
            characterController = GetComponent<CharacterController>();
            if (characterController == null) {
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.GetComponentReferences(): unable to get AnyRPGCharacterController");
            }
        }

        public void SetCharacterUnit(CharacterUnit characterUnit) {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.SetCharacterUnit()");
            this.characterUnit = characterUnit;
        }

        //Put any code in here you want to run BEFORE the state's update function. This is run regardless of what state you're in.
        protected override void EarlyGlobalStateUpdate() {
            //Debug.Log(gameObject.name + ".earlyGlobalStateUpdate()");
            CalculateForward();
            CalculateBackward();
            CalculateGroundAngle();
            CheckGround();
            ApplyGravity();
            DrawDebugLines();
        }

        //Put any code in here you want to run AFTER the state's update function.  This is run regardless of what state you're in.
        protected override void LateGlobalStateUpdate() {
            if (characterUnit == null) {
                return;
            }

            // testing: do nothing if idle state to prevent resetting movement to zero and interfering with moving platforms
            if (rpgCharacterState == AnyRPGCharacterState.Idle) {
                //Debug.Log("Idle state active, not moving");
                return;
            }

            //Move the player by our velocity every frame.
            // transform the velocity from local space to world space so we move the character forward on his z axis, not the global world z axis
            Vector3 relativeMovement = CharacterRelativeInput(currentMoveVelocity);
            if (relativeMovement.magnitude > 0.1 || PlayerManager.MyInstance.PlayerController.inputJump) {
                PlayerManager.MyInstance.ActiveUnitController.UnitMotor.Move(relativeMovement);
            } else {

                Vector3 localVelocity = Vector3.zero;
                if (PlayerManager.MyInstance.ActiveUnitController != null && PlayerManager.MyInstance.ActiveUnitController.RigidBody != null) {
                    localVelocity = transform.InverseTransformDirection(PlayerManager.MyInstance.ActiveUnitController.RigidBody.velocity);
                }
                if (localVelocity.x != 0f || localVelocity.z != 0f || localVelocity.y != 0f) {
                    //Debug.Log("Character is moving at velocity: " + PlayerManager.MyInstance.ActiveUnitController.MyRigidBody.velocity + "; local: " + localVelocity + ", but no input was given.  Stopping Character!");
                    PlayerManager.MyInstance.ActiveUnitController.UnitMotor.Move(new Vector3(0, Mathf.Clamp(localVelocity.y, -53, 0), 0));
                }
            }

            //If alive and is moving, set animator.
            if (!useMeshNav && characterUnit.BaseCharacter.CharacterStats.IsAlive && PlayerManager.MyInstance.PlayerController.canMove) {

                // handle movement
                if (currentMoveVelocity.magnitude > 0 && PlayerManager.MyInstance.PlayerController.HasMoveInput()) {
                    //Debug.Log(gameObject.name + ".PlayerUnitMovementController.LateGlobalSuperUpdate(): animator velocity: " + PlayerManager.MyInstance.ActiveUnitController.MyCharacterAnimator.MyAnimator.velocity + "; angular: " + PlayerManager.MyInstance.ActiveUnitController.MyCharacterAnimator.MyAnimator.angularVelocity);
                    if (PlayerManager.MyInstance.PlayerController.inputStrafe == true) {
                        PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetStrafing(true);
                    } else {
                        PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetStrafing(false);
                    }
                    isMoving = true;
                    PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetMoving(true);
                    PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetVelocity(currentMoveVelocity, rotateModel);
                }/* else {
                    isMoving = false;
                    PlayerManager.MyInstance.ActiveUnitController.MyCharacterAnimator.SetMoving(false);
                    PlayerManager.MyInstance.ActiveUnitController.MyCharacterAnimator.SetStrafing(false);
                    PlayerManager.MyInstance.ActiveUnitController.MyCharacterAnimator.SetVelocity(currentMoveVelocity, rotateModel);
                }*/
                if (PlayerManager.MyInstance.ActiveUnitController != null && PlayerManager.MyInstance.ActiveUnitController.UnitAnimator != null) {
                    PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetTurnVelocity(currentTurnVelocity.x);
                } else {

                }
            }

            if (characterUnit.BaseCharacter.CharacterStats.IsAlive && PlayerManager.MyInstance.PlayerController.canMove) {
                // code to prevent turning when clicking on UI elements
                if (InputManager.MyInstance.rightMouseButtonDown && PlayerManager.MyInstance.PlayerController.HasMoveInput() && (!InputManager.MyInstance.rightMouseButtonClickedOverUI || (NamePlateManager.MyInstance != null ? NamePlateManager.MyInstance.MouseOverNamePlate() : false))) {
                    //Debug.Log(gameObject.name + ".PlayerUnitMovementController.LateGlobalSuperUpdate(): resetting transform.forward");

                    transform.forward = new Vector3(CameraManager.MyInstance.MainCameraController.MyWantedDirection.x, 0, CameraManager.MyInstance.MainCameraController.MyWantedDirection.z);
                    CameraManager.MyInstance.MainCamera.GetComponent<AnyRPGCameraController>().ResetWantedPosition();
                }

                if (PlayerManager.MyInstance.PlayerController.inputTurn != 0) {
                    //Debug.Log(gameObject.name + ".PlayerUnitMovementController.LateGlobalSuperUpdate(): rotating " + currentTurnVelocity.x);
                    PlayerManager.MyInstance.ActiveUnitController.UnitMotor.Rotate(new Vector3(0, currentTurnVelocity.x, 0));
                }
            }

        }

        void EnterGroundStateCommon() {
            canJump = true;
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetJumping(0);
            airForwardDirection = transform.forward;
        }

        //Below are the state functions. Each one is called based on the name of the state, so when currentState = Idle, we call Idle_EnterState. If currentState = Jump, we call Jump_StateUpdate()
        void Idle_EnterState() {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.Idle_EnterState() Freezing all constraints");
            if (PlayerManager.MyInstance.ActiveUnitController != null && PlayerManager.MyInstance.ActiveUnitController.RigidBody != null) {
                PlayerManager.MyInstance.ActiveUnitController.FreezePositionXZ();
            }

            // reset velocity from any falling movement that was happening
            currentMoveVelocity = Vector3.zero;
            EnterGroundStateCommon();

            isMoving = false;
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetMoving(false);
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetStrafing(false);

            // testing stop turning animation from playing
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetTurnVelocity(0f);

            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetVelocity(currentMoveVelocity, rotateModel);

        }

        //Run every frame we are in the idle state.
        void Idle_StateUpdate() {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.Idle_StateUpdate()");

            if (characterUnit == null) {
                // still waiting for character to spawn
                return;
            }

            if (PlayerManager.MyInstance.PlayerController.allowedInput && PlayerManager.MyInstance.PlayerController.inputJump) {
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.Idle_StateUpdate(): entering jump state");
                currentState = AnyRPGCharacterState.Jump;
                rpgCharacterState = AnyRPGCharacterState.Jump;
                return;
            }
            if (!MaintainingGround()) {
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.Idle_StateUpdate(): entering fall state");
                currentState = AnyRPGCharacterState.Fall;
                rpgCharacterState = AnyRPGCharacterState.Fall;
                return;
            }
            if ((PlayerManager.MyInstance.PlayerController.HasMoveInput() || PlayerManager.MyInstance.PlayerController.HasTurnInput()) && PlayerManager.MyInstance.PlayerController.canMove) {
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.Idle_StateUpdate(): entering move state");
                currentState = AnyRPGCharacterState.Move;
                rpgCharacterState = AnyRPGCharacterState.Move;
                return;
            }
            // factor in slightly uneven ground which gravity will cause the unit to slide on even when standing still with position and rotation locked
            // DETECT SUPER LOW RIGIDBODY VELOCITY AND FREEZE CHARACTER
            if (Mathf.Abs(PlayerManager.MyInstance.ActiveUnitController.RigidBody.velocity.y) < 0.01 && MaintainingGround() == true) {

                // note: disabled this to test if it was causing issues with moving platforms
                //currentMoveVelocity = new Vector3(0, 0, 0);
                
                // disable gravity while this close to the ground so we don't slide down slight inclines
                // freezing y position was causing character to not get lifted by bridges
                PlayerManager.MyInstance.ActiveUnitController.FreezePositionXZ();
                //PlayerManager.MyInstance.ActiveUnitController.MyRigidBody.constraints = RigidbodyConstraints.FreezeAll;
            } else {

                // allow the character to fall until they reach the ground
                PlayerManager.MyInstance.ActiveUnitController.FreezePositionXZ();

                // note: disabled this to test if it was causing issues with moving platforms
                //currentMoveVelocity = new Vector3(0, Mathf.Clamp(PlayerManager.MyInstance.ActiveUnitController.MyRigidBody.velocity.y, -53, 0), 0);
            }
        }

        void Idle_ExitState() {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.Idle_ExitState(). Freezing Rotation only");
            PlayerManager.MyInstance.ActiveUnitController.RigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            //Run once when exit the idle state.
        }

        void Move_EnterState() {
            //Debug.Log("Move_EnterState()");
            EnterGroundStateCommon();
        }

        void Move_StateUpdate() {

            airForwardDirection = transform.forward;
            airRotation = transform.rotation;
            if (PlayerManager.MyInstance.PlayerController.allowedInput && PlayerManager.MyInstance.PlayerController.inputJump) {
                currentState = AnyRPGCharacterState.Jump;
                rpgCharacterState = AnyRPGCharacterState.Jump;
                return;
            }
            if (!MaintainingGround()) {
                currentState = AnyRPGCharacterState.Fall;
                rpgCharacterState = AnyRPGCharacterState.Fall;
                return;
            }

            //Set speed determined by movement type.

            // since we are in the move state, reset velocity to zero so we can pick up the new values
            // allow falling while moving by clamping existing y velocity
            currentMoveVelocity = new Vector3(0, Mathf.Clamp(PlayerManager.MyInstance.ActiveUnitController.RigidBody.velocity.y, -53, 0), 0);

            if ((PlayerManager.MyInstance.PlayerController.HasMoveInput() || PlayerManager.MyInstance.PlayerController.HasTurnInput()) && PlayerManager.MyInstance.PlayerController.canMove) {

                // set clampValue to default of max movement speed
                float clampValue = PlayerManager.MyInstance.MaxMovementSpeed;

                // set a clamp value to limit movement speed to walking if going backward
                /*
                if (currentMoveVelocity.z < 0) {
                    clampValue = 1;
                }
                */

                // get current movement speed and clamp it to current clamp value
                float calculatedSpeed = PlayerManager.MyInstance.MyCharacter.UnitController.MovementSpeed;
                calculatedSpeed = Mathf.Clamp(calculatedSpeed, 0, clampValue);

                if (PlayerManager.MyInstance.PlayerController.HasMoveInput()) {
                    // multiply normalized movement by calculated speed to get actual movement
                    currentMoveVelocity = LocalMovement() * calculatedSpeed;
                }
                if (PlayerManager.MyInstance.PlayerController.HasTurnInput()) {
                    currentTurnVelocity = PlayerManager.MyInstance.PlayerController.TurnInput * ((PlayerPrefs.GetFloat("KeyboardTurnSpeed") * 5) + 6.0f);
                }
            } else {
                currentTurnVelocity = Vector3.zero;
                currentState = AnyRPGCharacterState.Idle;
                rpgCharacterState = AnyRPGCharacterState.Idle;
                return;
            }
        }

        void Knockback_EnterState() {
            //Debug.Log("Knockback_EnterState()");
            //currentMoveVelocity.y = (Vector3.up * jumpAcceleration).y;
            canJump = false;
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetJumping(1);
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetTrigger("JumpTrigger");
        }

        void Knockback_StateUpdate() {
            //Debug.Log("Knockback_StateUpdate()");
            // new code to allow bouncing off walls instead of getting stuck flying into them
            //currentMoveVelocity = CharacterRelativeInput(transform.InverseTransformDirection(PlayerManager.MyInstance.ActiveUnitController.MyRigidBody.velocity));
            Vector3 airForwardVelocity = Quaternion.LookRotation(airForwardDirection, Vector3.up) * PlayerManager.MyInstance.ActiveUnitController.RigidBody.velocity;
            currentMoveVelocity = transform.InverseTransformDirection(PlayerManager.MyInstance.ActiveUnitController.RigidBody.velocity);
            Vector3 fromtoMoveVelocity = Quaternion.FromToRotation(airForwardDirection, transform.forward) * transform.InverseTransformDirection(PlayerManager.MyInstance.ActiveUnitController.RigidBody.velocity);
            currentMoveVelocity = fromtoMoveVelocity;
            if (AcquiringGround() && PlayerManager.MyInstance.ActiveUnitController.RigidBody.velocity.y < 0.1) {
                if ((PlayerManager.MyInstance.PlayerController.HasMoveInput() || PlayerManager.MyInstance.PlayerController.HasTurnInput()) && PlayerManager.MyInstance.PlayerController.canMove) {
                    // new code to allow not freezing up when landing - fix, should be fall or somehow prevent from getting into move during takeoff
                    currentState = AnyRPGCharacterState.Move;
                    rpgCharacterState = AnyRPGCharacterState.Move;
                    return;
                }
                currentState = AnyRPGCharacterState.Idle;
                rpgCharacterState = AnyRPGCharacterState.Idle;
                return;
            }
        }

        public void KnockBack() {
            //Debug.Log("Knockback()");
            currentState = AnyRPGCharacterState.Knockback;
            rpgCharacterState = AnyRPGCharacterState.Knockback;
        }


        void Jump_EnterState() {
            currentMoveVelocity.y = (Vector3.up * jumpAcceleration).y;
            canJump = false;
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetJumping(1);
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetTrigger("JumpTrigger");
            lastJumpFrame = Time.frameCount;
        }

        void Jump_StateUpdate() {
            // new code to allow bouncing off walls instead of getting stuck flying into them
            //currentMoveVelocity = CharacterRelativeInput(transform.InverseTransformDirection(PlayerManager.MyInstance.ActiveUnitController.MyRigidBody.velocity));
            Vector3 airForwardVelocity = Quaternion.LookRotation(airForwardDirection, Vector3.up) * PlayerManager.MyInstance.ActiveUnitController.RigidBody.velocity;
            currentMoveVelocity = transform.InverseTransformDirection(PlayerManager.MyInstance.ActiveUnitController.RigidBody.velocity);
            Vector3 fromtoMoveVelocity = Quaternion.FromToRotation(airForwardDirection, transform.forward) * transform.InverseTransformDirection(PlayerManager.MyInstance.ActiveUnitController.RigidBody.velocity);
            currentMoveVelocity = fromtoMoveVelocity;
            if (AcquiringGround() && PlayerManager.MyInstance.ActiveUnitController.RigidBody.velocity.y <= 0f && Time.frameCount > lastJumpFrame + 2) {
                if ((PlayerManager.MyInstance.PlayerController.HasMoveInput() || PlayerManager.MyInstance.PlayerController.HasTurnInput()) && PlayerManager.MyInstance.PlayerController.canMove) {
                    // new code to allow not freezing up when landing - fix, should be fall or somehow prevent from getting into move during takeoff
                    //Debug.Log("Jump_StateUpdate() : Entering movement state on frame: " + Time.frameCount + "; Jumped: " + lastJumpFrame);
                    currentState = AnyRPGCharacterState.Move;
                    rpgCharacterState = AnyRPGCharacterState.Move;
                    return;
                }
                currentState = AnyRPGCharacterState.Idle;
                rpgCharacterState = AnyRPGCharacterState.Idle;
                return;
            }
        }

        void Fall_EnterState() {
            //Debug.Log("Fall_EnterState()");
            canJump = false;
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetJumping(2);
            PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetTrigger("JumpTrigger");
        }

        void Fall_StateUpdate() {
            // new code to allow bouncing off walls instead of getting stuck flying into them
            Vector3 fromtoMoveVelocity = Quaternion.FromToRotation(airForwardDirection, transform.forward) * transform.InverseTransformDirection(PlayerManager.MyInstance.ActiveUnitController.RigidBody.velocity);
            //currentMoveVelocity = transform.InverseTransformDirection(CharacterRelativeInput(PlayerManager.MyInstance.ActiveUnitController.MyRigidBody.velocity));
            currentMoveVelocity = new Vector3(fromtoMoveVelocity.x, Mathf.Clamp(fromtoMoveVelocity.y, -53, 0), fromtoMoveVelocity.z);
            //currentMoveVelocity = new Vector3(currentMoveVelocity.x, 0, currentMoveVelocity.z);
            if (AcquiringGround()) {
                if ((PlayerManager.MyInstance.PlayerController.HasMoveInput() || PlayerManager.MyInstance.PlayerController.HasTurnInput()) && PlayerManager.MyInstance.PlayerController.canMove) {
                    // new code to allow not freezing up when landing
                    currentState = AnyRPGCharacterState.Move;
                    rpgCharacterState = AnyRPGCharacterState.Move;
                    return;
                }
                currentState = AnyRPGCharacterState.Idle;
                rpgCharacterState = AnyRPGCharacterState.Idle;
                return;
            }
        }

        public void SwitchCollisionOn() {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.SwitchCollisionOn()");
            if (characterUnit != null && characterUnit.BaseCharacter != null) {
                PlayerManager.MyInstance.PlayerController.canMove = true;
                if (characterController != null) {
                    characterController.enabled = true;
                }
            }
            if (PlayerManager.MyInstance.ActiveUnitController != null && PlayerManager.MyInstance.ActiveUnitController.UnitAnimator != null) {
                if (PlayerManager.MyInstance.ActiveUnitController.UnitAnimator != null) {
                    PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.DisableRootMotion();
                }
            }
        }

        /// <summary>
        /// switch to quaternion rotation instead of transformDirection so direction can be maintained in air no matter which way player faces in air
        /// </summary>
        /// <param name="inputVector"></param>
        /// <returns></returns>
        Vector3 CharacterRelativeInput(Vector3 inputVector) {
            //Debug.Log("PlayerUnitMovementController.CharacterRelativeInput(" + inputVector + ")");

            Vector3 qRelativeVelocity = Vector3.zero;
            if (inputVector != Vector3.zero) {
                qRelativeVelocity = Quaternion.LookRotation(airForwardDirection, Vector3.up) * inputVector;
            }
            Vector3 tRelativeVelocity = transform.TransformDirection(inputVector);
            if (qRelativeVelocity != Vector3.zero && tRelativeVelocity != Vector3.zero) {
                //Debug.Log("CharacterRelativeInput(" + inputVector + "): qRelativeVelocity: " + qRelativeVelocity + "; tRelativeVelocity: " + tRelativeVelocity);
            }
            //Debug.Log("PlayerUnitMovementController.CharacterRelativeInput(" + inputVector + "): return " + qRelativeVelocity + "; transformF: " + transform.forward + "; airForwardDirection: " + airForwardDirection);
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
            Vector3 forward = CameraManager.MyInstance.MyActiveMainCamera.transform.TransformDirection(Vector3.forward);
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
        }

        public bool MaintainingGround() {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.MaintainingGround");
            return tempGrounded;
        }

        private Vector3 LocalMovement() {
            //Debug.Log("PlayerUnitMovementController.LocalMovement(): groundAngle: " + groundAngle + "; backwardGroundAngle: " + backwardGroundAngle);
            Vector3 normalizedInput = PlayerManager.MyInstance.PlayerController.NormalizedMoveInput;

            // testing applying downforce on ground that is sloped downward - can't do it because that will later be rotated which could result in the "down" force moving backward at angles beyond 45degrees
            if (groundAngle == 0 && backwardGroundAngle == 0 && nearFrontObstacle == false) {
                //if (groundHitInfo.normal == Vector3.up) {
                // this should make the character stick to the ground better when actively moving while grounded
                // ONLY APPLY Y DOWNFORCE ON FLAT GROUND, this will apply a y downforce multiplied by speed, not the existing y downforce from physics (gravity)
                float yValue = 0f;
                if (transform.InverseTransformPoint(groundHitInfo.point) != Vector3.zero) {
                    yValue = Mathf.Clamp(PlayerManager.MyInstance.ActiveUnitController.RigidBody.velocity.normalized.y, -1, 0);
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
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CheckGround()");
            // downward cast for grounding
            if (Physics.Raycast(transform.position + (Vector3.up * 0.25f), -Vector3.up, out downHitInfo, rayCastHeight, groundMask)) {
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CheckGround(): grounded is true");
                tempGrounded = true;
            } else {
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CheckGround(): grounded is false");
                tempGrounded = false;
            }

            // downward cast for normals
            Physics.Raycast(transform.position + (Vector3.up * 0.25f), -Vector3.up, out groundHitInfo, Mathf.Infinity, groundMask);

            if (bottomContactPoints.Count > 0 || forwardContactPoints.Count > 0 || backwardContactPoints.Count > 0) {
                // extra check to catch contact points below maximum step height in case the character is halfway off a slope
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CheckGround(): grounded is true from contact points; bottom: " + bottomContactPoints.Count + "; front: " + forwardContactPoints.Count + "; back: " + backwardContactPoints.Count);
                tempGrounded = true;
            }

            Collider[] hitColliders = Physics.OverlapBox(transform.position, maintainingGroundExtents, Quaternion.identity, groundMask);
            if (hitColliders.Length > 0) {
                //foreach (Collider collider in hitColliders) {
                    //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CheckGround(): grounded is true from overlapbox (" + maintainingGroundExtents + "): " + collider.gameObject.name);
                //}
                tempGrounded = true;
            }
            /*
            foreach (Collider hitCollider in hitColliders) {
                //Debug.Log("Overlap Box Hit : " + hitColliders[i].name + i);
                if (((1 << hitCollider.gameObject.layer) & groundMask) != 0) {
                    tempGrounded = true;
                }
            }
            */

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
                    //float hitAngle = Vector3.Angle(contactPoint.normal, transform.forward);
                    //Debug.Log(gameObject.name + ".CharacterUnit.OnCollisionStay(): " + collision.collider.gameObject.name + "; normal: " + contactPoint.normal + "; angle: " + hitAngle);
                    Vector3 relativePoint = transform.InverseTransformPoint(contactPoint.point);
                    //Debug.Log(gameObject.name + ".CharacterUnit.OnCollisionStay(): " + collision.collider.gameObject.name + "; relativePoint: " + relativePoint);
                    if (relativePoint.z > 0 && relativePoint.y < stepHeight) {
                        //Debug.Log(gameObject.name + ".CharacterUnit.DebugCollision(): " + collision.collider.gameObject.name + "; relativePoint: " + relativePoint + " is in front of the player at world point: " + contactPoint.point);
                        // get direction to contact point
                        Vector3 direction = contactPoint.point - transform.position;
                        // extend contact point
                        direction *= 1.1f;
                        // shoot raycast downward from new point to detect stairs

                        Vector3 raycastPoint = transform.position + direction;
                        raycastPoint = new Vector3(raycastPoint.x, transform.position.y + stepHeight + 1f, raycastPoint.z);
                        //Debug.Log(gameObject.name + ".CharacterUnit.DebugCollision(): " + collision.collider.gameObject.name + "; direction is: " + direction + "; raycastpoint: " + raycastPoint);
                        Debug.DrawLine(raycastPoint, new Vector3(raycastPoint.x, raycastPoint.y - stepHeight - 1f, raycastPoint.z), Color.green);
                        RaycastHit stairHitInfo;
                        if (Physics.Raycast(raycastPoint, Vector3.down, out stairHitInfo, stepHeight + 1f, groundMask)) {
                            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CheckGround(): There is an obstacle in front of the player: " + forwardHitInfo.collider.gameObject.name + "; normal: " + forwardHitInfo.normal);
                            // we hit something that is low enough to step on
                            //nearFrontObstacle = true;
                            if (!forwardContactPoints.Contains(contactPoint)) {
                                forwardContactPoints.Add(contactPoint);
                            }
                        } else {
                            //Debug.Log(gameObject.name + ".CharacterUnit.DebugCollision(): we did not hit anything below the step height");
                        }

                        /*
                        if (!forwardContactPoints.Contains(contactPoint)) {
                            forwardContactPoints.Add(contactPoint);
                        }
                        */
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

        public void OnEnable() {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.OnEnable()");
            if (characterController != null) {
                characterController.enabled = true;
            }
        }

        public void OnDisable() {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.OnDisable()");
            if (characterController != null) {
                characterController.enabled = false;
            }
        }

    }

}