using AnyRPG;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AnyRPG {
    public enum AnyRPGCharacterState {
        Idle = 0,
        Move = 1,
        Jump = 2,
        Knockback = 3,
        Fall = 4,
        Roll = 8,
        Swim = 16
    }
    [RequireComponent(typeof(PlayerMovementStateController))]
    public class PlayerUnitMovementController : AnyRPGStateMachine {

        //Components.
        private PlayerMovementStateController movementStateController;

        //public AnyRPGCharacterState rpgCharacterState;

        [HideInInspector] public bool useMeshNav = false;
        [HideInInspector] public Vector3 lookDirection { get; private set; }
        public PlayerMovementStateController MovementStateController { get => movementStateController; }

        //Jumping.
        [HideInInspector] public bool canJump;
        public float gravity = 50.0f;
        public float jumpAcceleration = 5.0f;
        public float jumpHeight = 3.0f;

        // travel vector from the perspective of the character
        [HideInInspector] public Vector3 localMoveVelocity;
        // travel vector rotated by ground angle from the perspective of the character
        [HideInInspector] public Vector3 adjustedlocalMoveVelocity;
        [HideInInspector] public Vector3 currentTurnVelocity;

        [Tooltip("Keyboard Rotation speed in degrees per second")]
        public float rotationSpeed = 360f;

        //Air control.
        public float inAirSpeed = 6f;
        //private float acquiringGroundDistance = 0.11f;
        //private float maintainingGroundDistance = 0.5f;

        // maximum step height
        public float stepHeight = 0.5f;

        // maximum walkable slope
        public float slopeLimit = 60f;

        // calculated value based on stepHeight and slope limit
        private float stairDetectionDistance = 0f;

        // the raw angle of the ground below
        private float rawGroundAngle;
        
        // a calculated value
        private float groundAngle;

        private bool closeToGround;
        private bool touchingGround;

        // technically, "front" and "forward" are the current direction of travel to detect obstacles no matter which way you are heading for these next 2 variables
        private bool nearFrontObstacle = false;
        // forward raycast length
        //private float rayCastLength = 0.5f;
        private float rayCastLength = 0.5f;


        private LayerMask groundMask;

        // downward raycast height
        private float closeToGroundHeight = 0.25f;
        private float touchingGroundHeight = 0.05f;
        public bool debug = true;

        private Vector3 touchingGroundExtents = new Vector3(0.2f, 0.01f, 0.1f);
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
        //private List<ContactPoint> backwardContactPoints = new List<ContactPoint>();
        private List<ContactPoint> bottomContactPoints = new List<ContactPoint>();

        // the minimum height at which a collision is considered valid to calculate a forward or backward angle.  This is to prevent bottom collions that falsely register as front or back collisions
        private float collisionMinimumHeight = 0.05f;

        // game manager references
        protected PlayerManager playerManager = null;
        protected InputManager inputManager = null;
        protected NamePlateManager namePlateManager = null;
        protected CameraManager cameraManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            groundMask = playerManager.DefaultGroundMask;
            GetComponentReferences();
            if (movementStateController != null) {
                movementStateController.enabled = true;
            }
            stairDetectionDistance = Mathf.Tan(Mathf.Deg2Rad * (90f - slopeLimit)) * stepHeight;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            inputManager = systemGameManager.InputManager;
            namePlateManager = systemGameManager.UIManager.NamePlateManager;
            cameraManager = systemGameManager.CameraManager;
        }

        public void Init() {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.OrchestrateStartup()");
            //airForwardDirection = playerManager.ActiveUnitController.transform.forward;
            ConfigureStateMachine();
            SwitchCollisionOn();
        }

        public void ConfigureStateMachine() {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.ConfigureStateMachine()");
            currentState = AnyRPGCharacterState.Idle;
            //rpgCharacterState = AnyRPGCharacterState.Idle;
            if (movementStateController != null) {
                movementStateController.Init();
            }
        }

        public void GetComponentReferences() {
            movementStateController = GetComponent<PlayerMovementStateController>();
            if (movementStateController == null) {
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.GetComponentReferences(): unable to get AnyRPGCharacterController");
            } else {
                movementStateController.Configure(systemGameManager);
            }
        }

        //Put any code in here you want to run BEFORE the state's update function. This is run regardless of what state you're in.
        protected override void EarlyGlobalStateUpdate() {
            //Debug.Log(gameObject.name + ".earlyGlobalStateUpdate()");
            /*
            CalculateForward();
            //CalculateBackward();
            CalculateGroundAngle();
            CheckGround();
            */
            CheckGround();
            ApplyGravity();
            DrawDebugLines();
        }

        public void MoveRelative() {
            Vector3 relativeMovement = CharacterRelativeInput(adjustedlocalMoveVelocity);
            if (relativeMovement.magnitude > 0.1 || playerManager.PlayerController.inputJump) {
                playerManager.ActiveUnitController.UnitMotor.Move(relativeMovement);
            }
        }

        public void AnimatorMoveUpdate() {
            //If alive and is moving, set animator.
            if (useMeshNav == false
                && playerManager?.MyCharacter?.CharacterStats?.IsAlive == true
                && playerManager?.PlayerController?.canMove == true) {

                // handle movement
                if (localMoveVelocity.magnitude > 0 && playerManager.PlayerController.HasMoveInput()) {
                    //Debug.Log(gameObject.name + ".PlayerUnitMovementController.LateGlobalSuperUpdate(): animator velocity: " + playerManager.ActiveUnitController.MyCharacterAnimator.MyAnimator.velocity + "; angular: " + playerManager.ActiveUnitController.MyCharacterAnimator.MyAnimator.angularVelocity);
                    if (playerManager.PlayerController.inputStrafe == true) {
                        playerManager.ActiveUnitController.UnitAnimator.SetStrafing(true);
                    } else {
                        playerManager.ActiveUnitController.UnitAnimator.SetStrafing(false);
                    }
                    playerManager.ActiveUnitController.UnitAnimator.SetMoving(true);
                    playerManager.ActiveUnitController.UnitAnimator.SetVelocity(localMoveVelocity);
                }/* else {
                    playerManager.ActiveUnitController.MyCharacterAnimator.SetMoving(false);
                    playerManager.ActiveUnitController.MyCharacterAnimator.SetStrafing(false);
                    playerManager.ActiveUnitController.MyCharacterAnimator.SetVelocity(currentMoveVelocity, rotateModel);
                }*/
                AnimatorTurnUpdate();
            }
        }

        public void AnimatorTurnUpdate() {
            if (playerManager.ActiveUnitController.UnitAnimator != null) {
                playerManager.ActiveUnitController.UnitAnimator.SetTurnVelocity(currentTurnVelocity.x);
            }
        }

        //Put any code in here you want to run AFTER the state's update function.  This is run regardless of what state you're in.
        protected override void LateGlobalStateUpdate() {
            if (playerManager.ActiveUnitController == null) {
                return;
            }


            if (playerManager?.MyCharacter?.CharacterStats?.IsAlive == true && playerManager?.PlayerController?.canMove == true) {
                // code to prevent turning when clicking on UI elements
                // if (inputManager.rightMouseButtonDown && playerManager.PlayerController.HasMoveInput()
                if (inputManager.rightMouseButtonDown
                    && (!inputManager.rightMouseButtonClickedOverUI || (namePlateManager != null ? namePlateManager.MouseOverNamePlate() : false))) {
                    //Debug.Log(gameObject.name + ".PlayerUnitMovementController.LateGlobalSuperUpdate(): resetting playerManager.ActiveUnitController.transform.forward");

                    playerManager.ActiveUnitController.transform.forward = new Vector3(cameraManager.MainCameraController.MyWantedDirection.x, 0, cameraManager.MainCameraController.MyWantedDirection.z);
                    cameraManager.MainCameraController.ResetWantedPosition();
                }

                if (playerManager.PlayerController.inputTurn != 0) {
                    //Debug.Log(gameObject.name + ".PlayerUnitMovementController.LateGlobalSuperUpdate(): rotating " + currentTurnVelocity.x);
                    playerManager.ActiveUnitController.UnitMotor.Rotate(new Vector3(0, currentTurnVelocity.x * Time.deltaTime, 0));
                }
            }

        }

        void EnterGroundStateCommon() {
            canJump = true;
            playerManager.ActiveUnitController.UnitAnimator.SetJumping(0);
            airForwardDirection = playerManager.ActiveUnitController.transform.forward;
        }

        private bool CheckForSwimming() {
            if (playerManager.ActiveUnitController.InWater == true) {
                if ((playerManager.ActiveUnitController.transform.position.y + playerManager.ActiveUnitController.ChestHeight) <= playerManager.ActiveUnitController.CurrentWater[0].SurfaceHeight) {
                    return true;
                }
            }
            return false;
        }

        public void CalculateTurnVelocity() {
            if (playerManager.PlayerController.HasTurnInput()) {
                currentTurnVelocity = playerManager.PlayerController.TurnInput * PlayerPrefs.GetFloat("KeyboardTurnSpeed") * rotationSpeed;
            }
        }

        //Below are the state functions. Each one is called based on the name of the state, so when currentState = Idle, we call Idle_EnterState. If currentState = Jump, we call Jump_StateUpdate()
        void Idle_EnterState() {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.Idle_EnterState() Freezing all constraints");
            if (playerManager.ActiveUnitController != null) {
                // allow the character to fall until they reach the ground
                playerManager.ActiveUnitController.FreezePositionXZ();
            }

            // reset velocity from any falling movement that was happening
            localMoveVelocity = Vector3.zero;
            EnterGroundStateCommon();

            playerManager.ActiveUnitController.UnitAnimator.SetMoving(false);
            playerManager.ActiveUnitController.UnitAnimator.SetStrafing(false);
            playerManager.ActiveUnitController.UnitAnimator.SetTurnVelocity(0f);

            playerManager.ActiveUnitController.UnitAnimator.SetVelocity(localMoveVelocity);

            playerManager.ActiveUnitController.UnitMotor?.Move(new Vector3(0, Mathf.Clamp(playerManager.ActiveUnitController.RigidBody.velocity.y, -53, 0), 0));

        }

        void Idle_StateUpdate() {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.Idle_StateUpdate()");

            if (playerManager.ActiveUnitController == null) {
                // still waiting for character to spawn
                return;
            }

            if (playerManager.ActiveUnitController.InWater == true) {
                if (CheckForSwimming() == true) {
                    currentState = AnyRPGCharacterState.Swim;
                    return;
                }
            }

            if (playerManager.PlayerController.allowedInput && playerManager.PlayerController.inputJump) {
                currentState = AnyRPGCharacterState.Jump;
                return;
            }

            if (!MaintainingGround() || rawGroundAngle > slopeLimit) {
                currentState = AnyRPGCharacterState.Fall;
                return;
            }
            if ((playerManager.PlayerController.HasMoveInput() || playerManager.PlayerController.HasTurnInput()) && playerManager.PlayerController.canMove) {
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.Idle_StateUpdate(): entering move state");
                currentState = AnyRPGCharacterState.Move;
                return;
            }
            // factor in slightly uneven ground which gravity will cause the unit to slide on even when standing still with position and rotation locked
            // DETECT SUPER LOW RIGIDBODY VELOCITY AND FREEZE CHARACTER
            /*
            if (Mathf.Abs(playerManager.ActiveUnitController.RigidBody.velocity.y) < 0.01 && MaintainingGround() == true) {

                // note: disabled this to test if it was causing issues with moving platforms
                // note : re-enabled to see if it was not preventing launching up hills
                currentMoveVelocity = new Vector3(0, 0, 0);

            } else {

                // note: disabled this to test if it was causing issues with moving platforms
                // note : re-enabled to see if it was not preventing launching up hills
                currentMoveVelocity = new Vector3(0, Mathf.Clamp(playerManager.ActiveUnitController.RigidBody.velocity.y, -53, 0), 0);
            }
            */
        }

        void Idle_ExitState() {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.Idle_ExitState(). Freezing Rotation only");
            playerManager.ActiveUnitController.RigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        void Move_EnterState() {
            EnterGroundStateCommon();
        }

        void Move_StateUpdate() {

            airForwardDirection = playerManager.ActiveUnitController.transform.forward;

            if (playerManager.ActiveUnitController.InWater == true) {
                if (CheckForSwimming() == true) {
                    currentState = AnyRPGCharacterState.Swim;
                    return;
                }
            }

            if (playerManager.PlayerController.allowedInput && playerManager.PlayerController.inputJump) {
                currentState = AnyRPGCharacterState.Jump;
                return;
            }

            if (!MaintainingGround() || rawGroundAngle > slopeLimit) {
                currentState = AnyRPGCharacterState.Fall;
                return;
            }

            //Set speed determined by movement type.

            // since we are in the move state, reset velocity to zero so we can pick up the new values
            // allow falling while moving by clamping existing y velocity
            localMoveVelocity = new Vector3(0, Mathf.Clamp(playerManager.ActiveUnitController.RigidBody.velocity.y, -53, 0), 0);
            adjustedlocalMoveVelocity = localMoveVelocity;

            if ((playerManager.PlayerController.HasMoveInput() || playerManager.PlayerController.HasTurnInput()) && playerManager.PlayerController.canMove) {

                // set clampValue to default of max movement speed
                float clampValue = playerManager.MaxMovementSpeed;

                // set a clamp value to limit movement speed to walking if going backward
                /*
                if (currentMoveVelocity.z < 0) {
                    clampValue = 1;
                }
                */

                // get current movement speed and clamp it to current clamp value
                float calculatedSpeed = playerManager.ActiveUnitController.MovementSpeed;
                calculatedSpeed = Mathf.Clamp(calculatedSpeed, 0, clampValue);

                if (playerManager.PlayerController.HasMoveInput()) {
                    
                    // multiply normalized movement by calculated speed to get actual local movement
                    localMoveVelocity = playerManager.PlayerController.NormalizedMoveInput * calculatedSpeed;
                    adjustedlocalMoveVelocity = NormalizedLocalMovement() * calculatedSpeed;
                }
                CalculateTurnVelocity();
            } else {
                currentTurnVelocity = Vector3.zero;
                currentState = AnyRPGCharacterState.Idle;
                //rpgCharacterState = AnyRPGCharacterState.Idle;
                return;
            }

            MoveRelative();
            AnimatorMoveUpdate();
        }

        void Swim_EnterState() {
            if (playerManager.ActiveUnitController != null) {
                playerManager.ActiveUnitController.StartSwimming();
                playerManager.ActiveUnitController.RigidBody.useGravity = false;
                playerManager.ActiveUnitController.UnitAnimator.SetTrigger("SwimTrigger");
                playerManager.ActiveUnitController.UnitAnimator.SetBool("Swimming", true);
            }
        }

        void Swim_StateUpdate() {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.Swim_StateUpdate()");
            airForwardDirection = playerManager.ActiveUnitController.transform.forward;

            if (playerManager.ActiveUnitController.InWater == true) {
                if (CheckForSwimming() == false) {
                    currentState = AnyRPGCharacterState.Move;
                    return;
                }
            } else {
                currentState = AnyRPGCharacterState.Move;
                return;
            }

            if ((playerManager.PlayerController.HasWaterMoveInput() || playerManager.PlayerController.HasTurnInput())
                && playerManager.PlayerController.canMove) {

                // ============ RIGIDBODY CONSTRAINTS ============
                playerManager.ActiveUnitController.RigidBody.constraints = RigidbodyConstraints.FreezeRotation;

                // ============ VELOCITY CALCULATIONS ============

                // set clampValue to default of max movement speed
                float clampValue = playerManager.MaxMovementSpeed;

                // set a clamp value to limit movement speed to walking if going backward
                /*
                if (currentMoveVelocity.z < 0) {
                    clampValue = 1;
                }
                */

                // get current movement speed and clamp it to current clamp value
                float calculatedSpeed = playerManager.ActiveUnitController.SwimSpeed;
                calculatedSpeed = Mathf.Clamp(calculatedSpeed, 0, clampValue);

                if (playerManager.PlayerController.HasWaterMoveInput()) {
                    // multiply normalized movement by calculated speed to get actual movement
                    localMoveVelocity = NormalizedSwimMovement() * calculatedSpeed;
                    adjustedlocalMoveVelocity = localMoveVelocity;
                    //Debug.Log(gameObject.name + ".PlayerUnitMovementController.Swim_StateUpdate() currentMoveVelocity: " + currentMoveVelocity);
                }
                CalculateTurnVelocity();


                // ============ ANIMATOR PARAMETERS ============
                playerManager.ActiveUnitController.UnitAnimator.SetMoving(true);
                playerManager.ActiveUnitController.UnitAnimator.SetTurnVelocity(currentTurnVelocity.x);

            } else {
                // ============ RIGIDBODY CONSTRAINTS ============
                // prevent constant drifting through water after stop moving
                playerManager.ActiveUnitController.FreezeAll();

                // ============ VELOCITY CALCULATIONS ============
                localMoveVelocity = Vector3.zero;
                adjustedlocalMoveVelocity = localMoveVelocity;

                // ============ ANIMATOR PARAMETERS ============
                playerManager.ActiveUnitController.UnitAnimator.SetMoving(false);
                playerManager.ActiveUnitController.UnitAnimator.SetTurnVelocity(0f);

            }
            playerManager.ActiveUnitController.UnitAnimator.SetVelocity(localMoveVelocity);

            MoveRelative();
        }

        void Swim_ExitState() {
            if (playerManager.ActiveUnitController != null) {
                playerManager.ActiveUnitController.StopSwimming();
                playerManager.ActiveUnitController.RigidBody.useGravity = true;
                playerManager.ActiveUnitController.UnitAnimator.SetBool("Swimming", false);
            }
        }

        void Knockback_EnterState() {
            //Debug.Log("Knockback_EnterState()");
            //currentMoveVelocity.y = (Vector3.up * jumpAcceleration).y;
            canJump = false;
            playerManager.ActiveUnitController.UnitAnimator.SetJumping(1);
            playerManager.ActiveUnitController.UnitAnimator.SetTrigger("JumpTrigger");
        }

        void Knockback_StateUpdate() {
            //Debug.Log("Knockback_StateUpdate()");
            // new code to allow bouncing off walls instead of getting stuck flying into them
            //currentMoveVelocity = CharacterRelativeInput(playerManager.ActiveUnitController.transform.InverseTransformDirection(playerManager.ActiveUnitController.MyRigidBody.velocity));
            //Vector3 airForwardVelocity = Quaternion.LookRotation(airForwardDirection, Vector3.up) * playerManager.ActiveUnitController.RigidBody.velocity;
            //localMoveVelocity = playerManager.ActiveUnitController.transform.InverseTransformDirection(playerManager.ActiveUnitController.RigidBody.velocity);
            /*
            Vector3 fromtoMoveVelocity = Quaternion.FromToRotation(airForwardDirection, playerManager.ActiveUnitController.transform.forward) * playerManager.ActiveUnitController.transform.InverseTransformDirection(playerManager.ActiveUnitController.RigidBody.velocity);
            localMoveVelocity = fromtoMoveVelocity;
            */

            if (playerManager.ActiveUnitController.InWater == true) {
                if (CheckForSwimming() == true) {
                    currentState = AnyRPGCharacterState.Swim;
                    return;
                }
            }

            if (TouchingGround() && playerManager.ActiveUnitController.RigidBody.velocity.y < 0.1) {
                if ((playerManager.PlayerController.HasMoveInput() || playerManager.PlayerController.HasTurnInput()) && playerManager.PlayerController.canMove) {
                    // new code to allow not freezing up when landing - fix, should be fall or somehow prevent from getting into move during takeoff
                    currentState = AnyRPGCharacterState.Move;
                    //rpgCharacterState = AnyRPGCharacterState.Move;
                    return;
                }
                currentState = AnyRPGCharacterState.Idle;
                //rpgCharacterState = AnyRPGCharacterState.Idle;
                return;
            }

            //MoveRelative();
        }

        public void KnockBack() {
            //Debug.Log("Knockback()");
            currentState = AnyRPGCharacterState.Knockback;
            //rpgCharacterState = AnyRPGCharacterState.Knockback;
        }


        void Jump_EnterState() {
            localMoveVelocity.y = (Vector3.up * jumpAcceleration).y;
            adjustedlocalMoveVelocity = localMoveVelocity;
            canJump = false;
            playerManager.ActiveUnitController.UnitAnimator.SetJumping(1);
            playerManager.ActiveUnitController.UnitAnimator.SetTrigger("JumpTrigger");
            lastJumpFrame = Time.frameCount;
            MoveRelative();
        }

        void Jump_StateUpdate() {
            // new code to allow bouncing off walls instead of getting stuck flying into them
            //currentMoveVelocity = CharacterRelativeInput(playerManager.ActiveUnitController.transform.InverseTransformDirection(playerManager.ActiveUnitController.MyRigidBody.velocity));
            //Vector3 airForwardVelocity = Quaternion.LookRotation(airForwardDirection, Vector3.up) * playerManager.ActiveUnitController.RigidBody.velocity;
            //currentMoveVelocity = playerManager.ActiveUnitController.transform.InverseTransformDirection(playerManager.ActiveUnitController.RigidBody.velocity);
            /*
            Vector3 fromtoMoveVelocity = Quaternion.FromToRotation(airForwardDirection, playerManager.ActiveUnitController.transform.forward) * playerManager.ActiveUnitController.transform.InverseTransformDirection(playerManager.ActiveUnitController.RigidBody.velocity);
            localMoveVelocity = fromtoMoveVelocity;
            */

            if (playerManager.ActiveUnitController.InWater == true) {
                if (CheckForSwimming() == true) {
                    currentState = AnyRPGCharacterState.Swim;
                    return;
                }
            }

            if (playerManager.ActiveUnitController.RigidBody.velocity.y <= 0f && Time.frameCount > (lastJumpFrame + 2)) {
                currentState = AnyRPGCharacterState.Fall;
                return;
            }

            //MoveRelative();
        }

        void Fall_EnterState() {
            canJump = false;
            playerManager.ActiveUnitController.UnitAnimator.SetTrigger("FallTrigger");
            playerManager.ActiveUnitController.UnitAnimator.SetJumping(2);

            playerManager.ActiveUnitController.UnitMotor?.Move(new Vector3(playerManager.ActiveUnitController.RigidBody.velocity.x, Mathf.Clamp(playerManager.ActiveUnitController.RigidBody.velocity.y, -53, 0), playerManager.ActiveUnitController.RigidBody.velocity.z));
        }

        void Fall_StateUpdate() {
            // new code to allow bouncing off walls instead of getting stuck flying into them
            /*
            Vector3 fromtoMoveVelocity = Quaternion.FromToRotation(airForwardDirection, playerManager.ActiveUnitController.transform.forward) * playerManager.ActiveUnitController.transform.InverseTransformDirection(playerManager.ActiveUnitController.RigidBody.velocity);
            */
            //currentMoveVelocity = playerManager.ActiveUnitController.transform.InverseTransformDirection(CharacterRelativeInput(playerManager.ActiveUnitController.MyRigidBody.velocity));
            /*
            localMoveVelocity = new Vector3(fromtoMoveVelocity.x, Mathf.Clamp(fromtoMoveVelocity.y, -53, 0), fromtoMoveVelocity.z);
            */
            //currentMoveVelocity = new Vector3(currentMoveVelocity.x, 0, currentMoveVelocity.z);

            if (playerManager.ActiveUnitController.InWater == true) {
                if (CheckForSwimming() == true) {
                    currentState = AnyRPGCharacterState.Swim;
                    return;
                }
            }

            // testing change condition
            // see if landing looks funny
            if (TouchingGround() && rawGroundAngle <= slopeLimit) {
                if ((playerManager.PlayerController.HasMoveInput() || playerManager.PlayerController.HasTurnInput()) && playerManager.PlayerController.canMove) {
                    // new code to allow not freezing up when landing
                    currentState = AnyRPGCharacterState.Move;
                    return;
                }
                currentState = AnyRPGCharacterState.Idle;
                return;
            }

            // testing disable move call to let physics move the character
            //MoveRelative();
        }

        public void SwitchCollisionOn() {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.SwitchCollisionOn()");
            if (playerManager.ActiveUnitController != null) {
                playerManager.PlayerController.canMove = true;
                if (movementStateController != null) {
                    movementStateController.enabled = true;
                }
            }
            if (playerManager?.ActiveUnitController?.UnitAnimator != null) {
                playerManager.ActiveUnitController.UnitAnimator.DisableRootMotion();
            }
        }

        Vector3 CharacterRelativeInput(Vector3 inputVector) {
            //Debug.Log("PlayerUnitMovementController.CharacterRelativeInput(" + inputVector + ")");

            // switch to quaternion rotation instead of transformDirection so direction can be maintained in air no matter which way player faces in air
            Vector3 qRelativeVelocity = Vector3.zero;
            if (inputVector != Vector3.zero) {
                qRelativeVelocity = Quaternion.LookRotation(airForwardDirection, Vector3.up) * inputVector;
            }
            /*
            Vector3 tRelativeVelocity = playerManager.ActiveUnitController.transform.TransformDirection(inputVector);
            if (qRelativeVelocity != Vector3.zero && tRelativeVelocity != Vector3.zero) {
                //Debug.Log("CharacterRelativeInput(" + inputVector + "): qRelativeVelocity: " + qRelativeVelocity + "; tRelativeVelocity: " + tRelativeVelocity);
            }
            //Debug.Log("PlayerUnitMovementController.CharacterRelativeInput(" + inputVector + "): return " + qRelativeVelocity + "; transformF: " + playerManager.ActiveUnitController.transform.forward + "; airForwardDirection: " + airForwardDirection);
            */
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
            Vector3 forward = cameraManager.ActiveMainCamera.transform.TransformDirection(Vector3.forward);
            forward.y = 0;
            forward = forward.normalized;
            //Right vector relative to the camera always orthogonal to the forward vector.
            Vector3 right = new Vector3(forward.z, 0, -forward.x);
            Vector3 relativeVelocity = inputX * right + inputZ * forward;

            return relativeVelocity;
        }

        private bool TouchingGround() {
            //Debug.Log("PlayerUnitMovementController.AcquiringGround()");
            Collider[] hitColliders = Physics.OverlapBox(playerManager.ActiveUnitController.transform.position, touchingGroundExtents, playerManager.ActiveUnitController.transform.rotation, groundMask);

            Ray ray;
            RaycastHit colliderHit;
            foreach (Collider hitCollider in hitColliders) {
                if (hitCollider.GetType() == typeof(BoxCollider)
                    || hitCollider.GetType() == typeof(SphereCollider)
                    || hitCollider.GetType() == typeof(CapsuleCollider)
                    || (hitCollider.GetType() == typeof(MeshCollider) && (hitCollider as MeshCollider).convex == true)) {
                    Debug.Log("Overlap Box Hit : " + hitCollider.name + "; type: " + hitCollider.GetType().Name);
                    ray = new Ray(playerManager.ActiveUnitController.transform.position, hitCollider.ClosestPoint(playerManager.ActiveUnitController.transform.position) - playerManager.ActiveUnitController.transform.position);
                    hitCollider.Raycast(ray, out colliderHit, Mathf.Infinity);
                    //hitCollider.ClosestPoint(playerManager.ActiveUnitController.transform.position);
                    if (Vector3.Angle(colliderHit.normal, Vector3.up) <= slopeLimit) {
                        return true;
                    }
                } else {
                    return true;
                }
                /*
                Debug.Log("Overlap Box Hit : " + hitCollider.name);
                ray = new Ray(playerManager.ActiveUnitController.transform.position, hitCollider.ClosestPoint(playerManager.ActiveUnitController.transform.position) - playerManager.ActiveUnitController.transform.position);
                hitCollider.Raycast(ray, out colliderHit, Mathf.Infinity);
                //hitCollider.ClosestPoint(playerManager.ActiveUnitController.transform.position);
                if (Vector3.Angle(colliderHit.normal, Vector3.up) <= slopeLimit) {
                    return true;
                }
                */
            }
            
            /*
            if (hitColliders.Length > 0) {
                //Debug.Log("PlayerUnitMovementController.AcquiringGround(): Grounded!");
                return true;
            }
            */
            return false;
        }

        public bool MaintainingGround() {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.MaintainingGround");
            return closeToGround;
        }

        private Vector3 NormalizedSwimMovement() {
            //Debug.Log("PlayerUnitMovementController.NormalizedSwimMovement(): groundAngle: " + groundAngle + "; backwardGroundAngle: " + backwardGroundAngle);

            Vector3 returnValue = playerManager.PlayerController.NormalizedMoveInput;

            // check for right mouse button held down to adjust swim angle based on camera angle
            bool chestBelowWater = (playerManager.ActiveUnitController.transform.position.y + playerManager.ActiveUnitController.ChestHeight) < (playerManager.ActiveUnitController.CurrentWater[0].SurfaceHeight - (playerManager.ActiveUnitController.SwimSpeed * Time.deltaTime));

            if (inputManager.rightMouseButtonDown
                && playerManager.PlayerController.HasMoveInput()
                && (!inputManager.rightMouseButtonClickedOverUI || (namePlateManager != null ? namePlateManager.MouseOverNamePlate() : false))) {

                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.SwimMovement(): camera Angle: " + cameraManager.MainCameraGameObject.transform.localEulerAngles.x);

                // prevent constant bouncing out of water using right mouse
                // always allow downward motion
                // only allow upward motion if the swim speed would not result in a bounce
                float cameraAngle = (cameraManager.MainCameraGameObject.transform.localEulerAngles.x < 180f ? cameraManager.MainCameraGameObject.transform.localEulerAngles.x : cameraManager.MainCameraGameObject.transform.localEulerAngles.x - 360f);
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.SwimMovement(): camera Angle: " + cameraAngle);
                // ignore angle if already touching ground underwater to prevent hitting bottom and stopping while trying to swim forward
                if ((cameraAngle > 0f && returnValue.z > 0f && !touchingGround) // camera above and moving forward / down
                    || (cameraAngle > 0f && returnValue.z < 0f && chestBelowWater == true) // camera above and moving back / up
                    || (cameraAngle < 0f && returnValue.z < 0f && !touchingGround) // camera below and moving back / down
                    || (cameraAngle < 0f && returnValue.z > 0f && chestBelowWater == true) // camera below and forward / up
                    ) {
                    //Debug.Log(gameObject.name + ".PlayerUnitMovementController.SwimMovement(): camera Angle: " + cameraAngle + "; direction: " + returnValue.z +
                    //    "; chest height: " + (playerManager.ActiveUnitController.transform.position.y + playerManager.ActiveUnitController.ChestHeight) + "; surface: " + playerManager.ActiveUnitController.CurrentWater[0].SurfaceHeight + "; speed: " + (playerManager.ActiveUnitController.SwimSpeed * Time.deltaTime));
                    returnValue = Quaternion.AngleAxis(cameraManager.MainCameraGameObject.transform.localEulerAngles.x, Vector3.right) * returnValue;
                }
            }

            // if the jump or crouch buttons were held down, their values override the camera angle and allow movement straight up or down
            // ignore if swim speed would not result in a bounce out of the water
            if (playerManager.PlayerController.inputSink == true
                || (playerManager.PlayerController.inputFly == true && chestBelowWater == true)) {
                returnValue.y = (playerManager.PlayerController.inputFly == true ? 1 : 0) + (playerManager.PlayerController.inputSink == true ? -1 : 0);
            }

            return returnValue;
        }

        private Vector3 NormalizedLocalMovement() {
            //Debug.Log("PlayerUnitMovementController.LocalMovement(): groundAngle: " + groundAngle + "; backwardGroundAngle: " + backwardGroundAngle);
            Vector3 normalizedInput = playerManager.PlayerController.NormalizedMoveInput;

            CheckFrontObstacle();
            CalculateForward();
            //CalculateBackward();
            CalculateGroundAngle(playerManager.ActiveUnitController.transform.TransformDirection(normalizedInput));

            // testing applying downforce on ground that is sloped downward - can't do it because that will later be rotated which could result in the "down" force moving backward at angles beyond 45degrees
            if (groundAngle == 0 && nearFrontObstacle == false) {
                //if (groundHitInfo.normal == Vector3.up) {
                // this should make the character stick to the ground better when actively moving while grounded
                // ONLY APPLY Y DOWNFORCE ON FLAT GROUND, this will apply a y downforce multiplied by speed, not the existing y downforce from physics (gravity)
                float yValue = 0f;
                if (playerManager.ActiveUnitController.transform.InverseTransformPoint(groundHitInfo.point) != Vector3.zero) {
                    yValue = Mathf.Clamp(playerManager.ActiveUnitController.RigidBody.velocity.normalized.y, -1, 0);
                    //Debug.Log("LocalMovement(): We are above the (flat) ground and there are no near collisions.  Applying extra ground force: " + yValue);
                }
                normalizedInput = new Vector3(normalizedInput.x, yValue, normalizedInput.z);
            }
            Vector3 newReturnValue;
            float usedAngle = groundAngle;
            if (!nearFrontObstacle && forwardContactPoints.Count == 0) {
                // moving forward, use forward angle calculated to get over objects
                // hopefully this still allows us the correct ground angle when going downhill with no obstacles in front
                if (groundAngle > 0) {
                    // code to stop going up if standing with center over slope, but front of feet on flat surface with no obstacles in front
                    //Debug.Log("PlayerUnitMovementController.LocalMovement(): no front obstacles, ignoring ground angle because we are likely above the ground");
                    usedAngle = 0f;
                }
            }
            Debug.Log("NormalizedLocalMovement() used angle: " + usedAngle + "; forwardDirection: " + forwardDirection);
            //newReturnValue = Quaternion.AngleAxis(usedAngle, -Vector3.right) * normalizedInput;
            Vector3 localGroundNormal = playerManager.ActiveUnitController.transform.InverseTransformDirection(groundHitInfo.normal);

            //Debug.Log("groundHitInfo.normal: " + groundHitInfo.normal + "; local translation: " + localGroundNormal + "; forwardDirection: " + forwardDirection);


            // next line works when facing world axis
            //newReturnValue = playerManager.ActiveUnitController.transform.InverseTransformDirection(Quaternion.LookRotation(forwardDirection, localGroundNormal) * normalizedInput);

            // THIS ONE WORKS!
            newReturnValue = Vector3.Cross(Quaternion.LookRotation(normalizedInput, Vector3.up) * playerManager.ActiveUnitController.transform.InverseTransformDirection(playerManager.ActiveUnitController.transform.right), localGroundNormal);

            //Debug.Log("newReturnValue: " + newReturnValue);
            return newReturnValue;
        }

        // Calculate the initial velocity of a jump based off gravity and desired maximum height attained
        private float CalculateJumpSpeed(float jumpHeight, float gravity) {
            return Mathf.Sqrt(2 * jumpHeight * gravity);
        }

        private void CalculateForward() {
            if (!MaintainingGround()) {
                forwardDirection = airForwardDirection;
                //forwardDirection = playerManager.ActiveUnitController.transform.forward;
                return;
            }

            // PUT CODE HERE TO RECOGNIZE THE HIGHEST ANGLE AS THE FORWARD DIRECTION

            if (forwardContactPoints.Count > 0) {
                Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateForward(): forwardContactPoints.Count: " + forwardContactPoints.Count);
                int counter = 0;
                int smallestIndex = -1;

                // find highest contact point
                foreach (ContactPoint contactPoint in forwardContactPoints) {
                    Vector3 localContactPoint = playerManager.ActiveUnitController.transform.InverseTransformPoint(contactPoint.point);
                    // ensure forward contact point is above a certain height and actually in front of the character
                    if (localContactPoint.y > collisionMinimumHeight && localContactPoint != Vector3.zero) {
                        if (smallestIndex == -1 || localContactPoint.y > playerManager.ActiveUnitController.transform.InverseTransformPoint(forwardContactPoints[smallestIndex].point).y) {
                            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateForward(): found highest contact point at: " + contactPoint.point + "; local: " + localContactPoint);
                            smallestIndex = counter;
                        }
                    }
                    counter++;
                }

                if (smallestIndex != -1) {
                    // get vector between contact point and base of player
                    Vector3 directionToContact = (forwardContactPoints[smallestIndex].point - playerManager.ActiveUnitController.transform.position).normalized;
                    forwardDirection = directionToContact;
                    //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateForward(): Vector3.Cross(downHitInfo.normal(" + downHitInfo.normal + "), -transform.right(" + -transform.right + ")): " + forwardDirection);
                    Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateForward(): directionToContact: " + directionToContact + "; forwardDirection: " + forwardDirection);
                    return;
                }
            }

            if (nearFrontObstacle && groundHitInfo.normal == Vector3.up) {
                Debug.Log("CalculateForward(): near front obstacle is true and we didn't get any useful information from the ground normal, trying obstacle normal");
                forwardDirection = Vector3.Cross(forwardHitInfo.normal, -playerManager.ActiveUnitController.transform.right);
                return;
            }

            forwardDirection = Vector3.Cross(groundHitInfo.normal, -playerManager.ActiveUnitController.transform.right);
            Debug.Log("CalculateForward(): no forward collisions. forwardDirection = Vector3.Cross(groundHitInfo.normal(" + groundHitInfo.normal + "), -transform.right(" + -transform.right + ")): " + forwardDirection);
            return;

        }

        /*
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
                    Vector3 localContactPoint = playerManager.ActiveUnitController.transform.InverseTransformPoint(contactPoint.point);
                    if (localContactPoint.y > collisionMinimumHeight && localContactPoint != Vector3.zero) {
                        if (smallestIndex == -1 || localContactPoint.y > playerManager.ActiveUnitController.transform.InverseTransformPoint(backwardContactPoints[smallestIndex].point).y) {
                            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateBackward(): found highest contact point");
                            smallestIndex = counter;
                        }
                    }
                    counter++;
                }
                if (smallestIndex != -1) {
                    // get angle between contact point and base of player
                    Vector3 directionToContact = (backwardContactPoints[smallestIndex].point - playerManager.ActiveUnitController.transform.position).normalized;
                    backwardDirection = directionToContact;
                    //forwardDirection = Vector3.Cross(directionToContact, -transform.right);
                    //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateBackward(): directionToContact: " + directionToContact + "; rearDirection: " + backwardDirection);
                    return;
                }
            }

            if (nearFrontObstacle && groundHitInfo.normal == Vector3.up) {
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateForward(): near front obstacle is true and we didn't get any useful information from the ground normal, trying obstacle normal");
                backwardDirection = Vector3.Cross(forwardHitInfo.normal, playerManager.ActiveUnitController.transform.right);
                return;
            }

            backwardDirection = Vector3.Cross(groundHitInfo.normal, playerManager.ActiveUnitController.transform.right);
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateForward(): no backward collisions. backwardDirection = Vector3.Cross(downHitInfo.normal(" + downHitInfo.normal + "), playerManager.ActiveUnitController.transform.right(" + playerManager.ActiveUnitController.transform.right + ")): " + backwardDirection);
            return;
        }
        */

        private void GetGroundAngle() {

        }

        private void CalculateGroundAngle(Vector3 directionOfTravel) {
            if (!MaintainingGround()) {
                //groundAngle = 90;
                groundAngle = 0f;
                return;
            }
            /*
            if (hitInfo != null) {
                Debug.Log("hitInfo: " + hitInfo.collider.gameObject.name + "; normal: " + hitInfo.normal);
            }
            */
            float downHitAngle = Vector3.Angle(groundHitInfo.normal, directionOfTravel) - 90f;
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateGroundAngle() from downHitInfo.normal(" + downHitInfo.normal + "): " + downHitAngle);
            float forwardHitAngle = Vector3.Angle(forwardHitInfo.normal, directionOfTravel) - 90f;
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateGroundAngle() from forwardHitInfo.normal(" + forwardHitInfo.normal + "): " + forwardHitAngle);

            //groundAngle = Vector3.Angle(forwardDirection, playerManager.ActiveUnitController.transform.forward) + 90;
            float forwardcollisionAngle = Vector3.Angle(forwardDirection, directionOfTravel) * (forwardDirection.y < 0 ? -1 : 1);

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

            /*
            if (groundAngle != 0) {
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateGroundAngle() from forwardDirection(" + forwardDirection + "): " + forwardcollisionAngle + "; from downhitInfo: " + downHitAngle);
            }

            backwardGroundAngle = Vector3.Angle(backwardDirection, -playerManager.ActiveUnitController.transform.forward) * (backwardDirection.y < 0 ? -1 : 1);
            if (backwardGroundAngle != 0) {
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CalculateGroundAngle() from backwardDirection(" + backwardDirection + "): " + backwardGroundAngle);
            }
            */
        }

        private void CheckGround() {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CheckGround()");

            // downward cast for close to ground
            if (Physics.Raycast(playerManager.ActiveUnitController.transform.position + (Vector3.up * 0.25f), -Vector3.up, out downHitInfo, (closeToGroundHeight + 0.25f), groundMask)) {
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CheckGround(): grounded is true");
                closeToGround = true;
            } else {
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CheckGround(): grounded is false");
                closeToGround = false;
            }

            // downward cast for touching ground
            if (Physics.Raycast(playerManager.ActiveUnitController.transform.position + (Vector3.up * 0.25f), -Vector3.up, out downHitInfo, (touchingGroundHeight + 0.25f), groundMask)) {
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CheckGround(): grounded is true");
                touchingGround = true;
            } else {
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CheckGround(): grounded is false");
                touchingGround = false;
            }

            // downward cast for normals
            Physics.Raycast(playerManager.ActiveUnitController.transform.position + (Vector3.up * 0.25f), -Vector3.up, out groundHitInfo, Mathf.Infinity, groundMask);

            rawGroundAngle = Vector3.Angle(groundHitInfo.normal, Vector3.up);
            Debug.Log("rawGroundAngle: " + rawGroundAngle);

            if (bottomContactPoints.Count > 0 || forwardContactPoints.Count > 0) {
                // extra check to catch contact points below maximum step height in case the character is halfway off a slope
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CheckGround(): grounded is true from contact points; bottom: " + bottomContactPoints.Count + "; front: " + forwardContactPoints.Count + "; back: " + backwardContactPoints.Count);
                closeToGround = true;
            }

            Collider[] hitColliders = Physics.OverlapBox(playerManager.ActiveUnitController.transform.position, maintainingGroundExtents, playerManager.ActiveUnitController.transform.rotation, groundMask);
            if (hitColliders.Length > 0) {
                closeToGround = true;
            }
            /*
            foreach (Collider hitCollider in hitColliders) {
                //Debug.Log("Overlap Box Hit : " + hitColliders[i].name + i);
                if (((1 << hitCollider.gameObject.layer) & groundMask) != 0) {
                    tempGrounded = true;
                }
            }
            */
        }

        private void CheckFrontObstacle() {
            // forward cast
            Vector3 directionOfTravel = playerManager.ActiveUnitController.transform.forward;
            if (localMoveVelocity.x != 0 || localMoveVelocity.z != 0) {
                directionOfTravel = playerManager.ActiveUnitController.transform.TransformDirection(new Vector3(localMoveVelocity.x, 0, localMoveVelocity.z)).normalized;
            }
            Debug.DrawLine(playerManager.ActiveUnitController.transform.position + (Vector3.up * 0.05f), playerManager.ActiveUnitController.transform.position + (Vector3.up * 0.05f) + (directionOfTravel * rayCastLength), Color.black);
            if (Physics.Raycast(playerManager.ActiveUnitController.transform.position + (Vector3.up * 0.05f), directionOfTravel, out forwardHitInfo, rayCastLength, groundMask)) {
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CheckGround(): There is an obstacle in front of the player: " + forwardHitInfo.collider.gameObject.name + "; normal: " + forwardHitInfo.normal);
                nearFrontObstacle = true;
            } else {
                nearFrontObstacle = false;
            }

        }

        private void ApplyGravity() {
            if (!closeToGround) {
                //Debug.Log("PlayerUnitMovementController.ApplyGravity(): Not Grounded");
            }
        }

        private void DrawDebugLines() {
            if (!debug) {
                return;
            }

            Debug.DrawLine(playerManager.ActiveUnitController.transform.position, playerManager.ActiveUnitController.transform.position + forwardDirection * closeToGroundHeight * 2, Color.blue);
            Debug.DrawLine(playerManager.ActiveUnitController.transform.position, playerManager.ActiveUnitController.transform.position + backwardDirection * closeToGroundHeight * 2, Color.magenta);
            Debug.DrawLine(playerManager.ActiveUnitController.transform.position + (Vector3.up * 0.25f), (playerManager.ActiveUnitController.transform.position + (Vector3.up * 0.25f)) - (Vector3.up * closeToGroundHeight), Color.green);

        }

        public void OnCollisionEnter(Collision collision) {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.OnCollisionEnter()");
            DebugCollision(collision);
        }

        public void OnCollisionStay(Collision collision) {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.OnCollisionStay(): " + collision.collider.gameObject.name);
            DebugCollision(collision);
        }

        // this code no longer gets called because this controller is no longer on the player, but the player object
        private void DebugCollision(Collision collision) {
            ContactPoint[] contactPoints = new ContactPoint[collision.contactCount];
            collision.GetContacts(contactPoints);
            Debug.Log("collision count: " + contactPoints.Length);
            foreach (ContactPoint contactPoint in contactPoints) {
                if (((1 << collision.gameObject.layer) & groundMask) != 0) {
                    //Debug.Log(gameObject.name + ".CharacterUnit.OnCollisionStay(): " + collision.collider.gameObject.name + " matched the ground Layer mask at : " + contactPoint.point + "; player: " + playerManager.ActiveUnitController.transform.position);
                    //float hitAngle = Vector3.Angle(contactPoint.normal, playerManager.ActiveUnitController.transform.forward);
                    //Debug.Log(gameObject.name + ".CharacterUnit.OnCollisionStay(): " + collision.collider.gameObject.name + "; normal: " + contactPoint.normal + "; angle: " + hitAngle);
                    Vector3 absolutePoint = playerManager.ActiveUnitController.transform.InverseTransformPoint(contactPoint.point);
                    Vector3 relativePoint = Quaternion.LookRotation(localMoveVelocity) * absolutePoint;
                    Debug.Log("DebugCollision(): " + collision.collider.gameObject.name + "; absolutePoint: " + absolutePoint + "; relativePoint: " + relativePoint + "; velocity: " + localMoveVelocity);
                    if (relativePoint.z > 0 && relativePoint.y < stepHeight) {
                        //Debug.Log(gameObject.name + ".CharacterUnit.DebugCollision(): " + collision.collider.gameObject.name + "; relativePoint: " + relativePoint + " is in front of the player at world point: " + contactPoint.point);
                        // get direction to contact point
                        Vector3 direction = contactPoint.point - playerManager.ActiveUnitController.transform.position;
                        // extend contact point
                        direction *= 1.1f;
                        // shoot raycast downward from new point to detect stairs

                        Vector3 raycastPoint = playerManager.ActiveUnitController.transform.position + direction;
                        raycastPoint = new Vector3(raycastPoint.x, playerManager.ActiveUnitController.transform.position.y + stepHeight + 1f, raycastPoint.z);
                        //Debug.Log(gameObject.name + ".CharacterUnit.DebugCollision(): " + collision.collider.gameObject.name + "; direction is: " + direction + "; raycastpoint: " + raycastPoint);
                        Debug.DrawLine(raycastPoint, new Vector3(raycastPoint.x, raycastPoint.y - stepHeight - 1f, raycastPoint.z), Color.green);
                        RaycastHit stairHitInfo;
                        if (Physics.Raycast(raycastPoint, Vector3.down, out stairHitInfo, stepHeight + 1f, groundMask)) {
                            Debug.Log(gameObject.name + ".PlayerUnitMovementController.CheckGround(): There is an obstacle in front of the player: " + forwardHitInfo.collider.gameObject.name + "; normal: " + forwardHitInfo.normal);
                            // we hit something that is low enough to step on
                            //nearFrontObstacle = true;
                            if (!forwardContactPoints.Contains(contactPoint)) {
                                forwardContactPoints.Add(contactPoint);
                            }
                        } else {
                            //Debug.Log(gameObject.name + ".CharacterUnit.DebugCollision(): we did not hit anything below the step height");
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
                Debug.DrawLine(playerManager.ActiveUnitController.transform.position, contactPoint.point, Color.yellow);
            }
        }

        private void FixedUpdate() {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.FixedUpdate(): forwardContactPoints.Clear()");
            forwardContactPoints.Clear();
            //backwardContactPoints.Clear();
            bottomContactPoints.Clear();
        }

        private void OnDrawGizmos() {
            if (playerManager == null || playerManager.ActiveUnitController == null) {
                return;
            }
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(playerManager.ActiveUnitController.transform.position, (Quaternion.LookRotation(playerManager.ActiveUnitController.transform.forward) * touchingGroundExtents) * 2);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(playerManager.ActiveUnitController.transform.position, maintainingGroundExtents * 2);
        }

        /*
        public void OnDisable() {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.OnDisable()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            if (movementStateController != null) {
                movementStateController.enabled = false;
            }
        }
        */

    }

}