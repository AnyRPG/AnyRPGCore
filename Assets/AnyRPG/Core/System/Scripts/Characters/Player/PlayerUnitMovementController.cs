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
        Roll = 5,
        Swim = 6,
        Fly = 7,
        Glide = 8
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

        // the normal of the closest ground to the player feet
        private Vector3 groundNormal;

        // the closest ground point to the player feet, determined by raycasts
        private Vector3 groundPoint;

        // the normal of the closest ground to the player feet
        private Vector3 slopeNormal;

        // the closest ground point to the player feet, determined by raycasts
        private Vector3 slopePoint;

        // the downward direction of any slope the player is touching
        private Vector3 slopeDirection;

        // the raw angle of the ground below
        private float groundAngle;
        
        // the raw angle of the ground below
        private float slopeAngle;

        // used for determining how far above the ground the player is for applying downforce
        private float closestGroundDistance = 0f;

        // the closest distance that is above the players feet that is greater than the slope limit
        private float closestSlopeDistance = 0f;

        // the closest ground that is under the players feet
        private float closestTouchingGroundDistance = 0f;

        /*
        // the highest distance under the raycast that is blow the slope limit
        private float highestWalkableGroundDistance = 0f;
        */

        // a calculated value
        //private float groundAngle;

        // is there an obstacle close in front of us in the direction of travel
        private bool nearFrontObstacle = false;

        // the angle of the obstacle in front of us
        private float frontObstacleAngle;

        // determine if there is a change in the angle from the current ground (which could be a ramp) to the obstacle in front of us (which could be the same ramp)
        private bool frontAngleDifferent = false;

        // determine if an obstacle in front of us is a stairs
        private bool nearStairs = false;

        // a calculated normal to be used when close to stairs to allow a virtual ramp to them isntead of jerky direct upward motion when you collide with them
        private Vector3 stairRampNormal;

        private bool closeToGround;
        private bool touchingGround;
        private bool touchingSlope;

        // forward raycast length
        //private float rayCastLength = 0.5f;
        //private float frontRayCastLength = 0.5f;



        private LayerMask groundMask;

        // downward raycast height
        private float closeToGroundHeight = 0.25f;
        private float touchingGroundHeight = 0.05f;
        private float colliderRadius = 0.3f;
        public bool debug = true;

        private bool useFallDamage = false;
        private float fallDamagePerMeter = 2f;
        private float fallDamageMinDistance = 10f;
        private float currentFallDistance = 0f;
        private float fallStartHeight = 0f;

        //private Vector3 touchingGroundExtents = new Vector3(0.2f, 0.01f, 0.1f);
        private Vector3 maintainingGroundExtents = new Vector3(0.65f, 0.5f, 0.65f);

        // raycast to determine ground normal
        private RaycastHit groundHitInfo;

        // raycasts to determine 
        private RaycastHit downHitInfo;

        // raycast to determine if an object is in front of the player
        private RaycastHit forwardHitInfo;
        private Vector3 forwardHitPoint;

        // raycast to determine if an object is in an arc in front of the player
        private RaycastHit obstacleHitInfo;

        // downward raycast to determine if an object in front of the player is stairs
        private RaycastHit stairDownHitInfo;
        private Vector3 stairDownHitPoint;

        // raycast to determine if player is touching ground in a circle around it
        //private RaycastHit touchingGroundHitInfo;

        // ensure that pressing forward moves us in the direction of the ground angle to avoid jittery movement on slopes
        //private Vector3 forwardDirection;
        //private Vector3 backwardDirection;

        // keep the player moving the same direction in the air
        private Vector3 airForwardDirection;
        private Quaternion airRotation;

        // the frame in which the player last entered a jump state
        private int lastJumpFrame;

        // game manager references
        protected PlayerManager playerManager = null;
        protected InputManager inputManager = null;
        protected NamePlateManager namePlateManager = null;
        protected CameraManager cameraManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            rotationSpeed = systemConfigurationManager.MaxTurnSpeed;
            groundMask = playerManager.DefaultGroundMask;
            GetComponentReferences();
            if (movementStateController != null) {
                movementStateController.enabled = true;
            }
            stairDetectionDistance = Mathf.Tan(Mathf.Deg2Rad * (90f - slopeLimit)) * stepHeight;
            useFallDamage = systemConfigurationManager.UseFallDamage;
            fallDamagePerMeter = systemConfigurationManager.FallDamagePerMeter;
            fallDamageMinDistance = systemConfigurationManager.FallDamageMinDistance;
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
            //Debug.Log("relativeMovement: (" + relativeMovement.x + ", " + relativeMovement.y + ", " + relativeMovement.z + ")");
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
                if ((playerManager.ActiveUnitController.transform.position.y + playerManager.ActiveUnitController.FloatHeight) <= playerManager.ActiveUnitController.CurrentWater[0].SurfaceHeight) {
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

        public void CalculateFallDamage() {
            if (useFallDamage && currentFallDistance > fallDamageMinDistance) {
                playerManager.ActiveCharacter.CharacterStats.TakeFallDamage(currentFallDistance * fallDamagePerMeter);
            }
            currentFallDistance = 0f;
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
            CalculateFallDamage();
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

            if (playerManager.PlayerController.allowedInput && playerManager.ActiveUnitController.CanFly && playerManager.PlayerController.inputFly) {
                currentState = AnyRPGCharacterState.Jump;
                return;
            }

            if (!MaintainingGround() || groundAngle > slopeLimit) {
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
            CalculateFallDamage();
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

            if (playerManager.PlayerController.allowedInput && playerManager.ActiveUnitController.CanFly && playerManager.PlayerController.inputFly) {
                currentState = AnyRPGCharacterState.Jump;
                return;
            }

            if (!MaintainingGround() || groundAngle > slopeLimit) {
                if (playerManager.ActiveUnitController.CanFly) {
                    currentState = AnyRPGCharacterState.Fly;
                    return;
                } else {
                    if (playerManager.ActiveUnitController.CanGlide) {
                        currentState = AnyRPGCharacterState.Glide;
                        return;
                    }
                    currentState = AnyRPGCharacterState.Fall;
                    return;
                }
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
                    adjustedlocalMoveVelocity = NormalizedLocalMovement(calculatedSpeed) * calculatedSpeed;
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

        void Move_ExitState() {
            playerManager.ActiveUnitController.UnitAnimator.SetMoving(false);
        }

        void Fly_EnterState() {
            //Debug.Log("PlayerUnitMovementController.Fly_EnterState()");
            currentFallDistance = 0f;
            if (playerManager.ActiveUnitController != null) {
                playerManager.ActiveUnitController.StartFlying();
                playerManager.ActiveUnitController.RigidBody.useGravity = false;
                playerManager.ActiveUnitController.UnitAnimator.SetBool("Flying", true);
                playerManager.ActiveUnitController.UnitAnimator.SetTrigger("FlyTrigger");
            }
        }

        void Fly_StateUpdate() {
            //Debug.Log("PlayerUnitMovementController.Fly_StateUpdate()");
            airForwardDirection = playerManager.ActiveUnitController.transform.forward;

            if (touchingGround == true && playerManager.PlayerController.inputFly == false) {
                if (playerManager.PlayerController.HasMoveInput()) {
                    currentState = AnyRPGCharacterState.Move;
                    return;
                } else {
                    currentState = AnyRPGCharacterState.Idle;
                    return;
                }
            }
            if (playerManager.ActiveUnitController.InWater == true && CheckForSwimming() == true) {
                currentState = AnyRPGCharacterState.Swim;
                return;
            }
            if (playerManager.ActiveUnitController.CanFly == false) {
                currentState = AnyRPGCharacterState.Fall;
                return;
            }


            if ((playerManager.PlayerController.HasFlyMoveInput() || playerManager.PlayerController.HasTurnInput())
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
                float calculatedSpeed = playerManager.ActiveUnitController.FlySpeed;
                calculatedSpeed = Mathf.Clamp(calculatedSpeed, 0, clampValue);

                if (playerManager.PlayerController.HasFlyMoveInput()) {
                    // multiply normalized movement by calculated speed to get actual movement
                    localMoveVelocity = NormalizedFlyMovement() * calculatedSpeed;
                    adjustedlocalMoveVelocity = localMoveVelocity;
                    //Debug.Log(gameObject.name + ".PlayerUnitMovementController.Swim_StateUpdate() currentMoveVelocity: " + currentMoveVelocity);
                }
                CalculateTurnVelocity();


                // ============ ANIMATOR PARAMETERS ============
                playerManager.ActiveUnitController.UnitAnimator.SetMoving(true);
                playerManager.ActiveUnitController.UnitAnimator.SetTurnVelocity(currentTurnVelocity.x);

            } else {
                // ============ RIGIDBODY CONSTRAINTS ============
                // prevent constant drifting through air after stop moving
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

        void Fly_ExitState() {
            //Debug.Log("PlayerUnitMovementController.Fly_ExitState()");
            if (playerManager.ActiveUnitController != null) {
                playerManager.ActiveUnitController.StopFlying();
                playerManager.ActiveUnitController.RigidBody.useGravity = true;
                playerManager.ActiveUnitController.UnitAnimator.SetBool("Flying", false);
                playerManager.ActiveUnitController.RigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            }
        }

        void Swim_EnterState() {
            currentFallDistance = 0f;
            if (playerManager.ActiveUnitController != null) {
                EnterGroundStateCommon();
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
                if (playerManager.PlayerController.allowedInput
                    && playerManager.ActiveUnitController.CanFly
                    && playerManager.PlayerController.inputFly
                    && CheckForSwimming() == false) {
                    currentState = AnyRPGCharacterState.Fly;
                    return;
                }
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

            if (playerManager.ActiveUnitController.InWater == true) {
                if (CheckForSwimming() == true) {
                    currentState = AnyRPGCharacterState.Swim;
                    return;
                }
            }

            if (touchingGround && playerManager.ActiveUnitController.RigidBody.velocity.y < 0.1) {
                if ((playerManager.PlayerController.HasMoveInput() || playerManager.PlayerController.HasTurnInput()) && playerManager.PlayerController.canMove) {
                    // new code to allow not freezing up when landing - fix, should be fall or somehow prevent from getting into move during takeoff
                    currentState = AnyRPGCharacterState.Move;
                    return;
                }
                currentState = AnyRPGCharacterState.Idle;
                return;
            }

            //MoveRelative();
        }

        public void KnockBack() {
            //Debug.Log("Knockback()");
            currentState = AnyRPGCharacterState.Knockback;
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

            if (playerManager.ActiveUnitController.InWater == true) {
                if (CheckForSwimming() == true) {
                    currentState = AnyRPGCharacterState.Swim;
                    return;
                }
            }

            if (playerManager.PlayerController.allowedInput
                && playerManager.ActiveUnitController.CanFly
                && playerManager.PlayerController.inputFly) { 
                currentState = AnyRPGCharacterState.Fly;
                return;
            }


            if (playerManager.ActiveUnitController.RigidBody.velocity.y <= 0f && Time.frameCount > (lastJumpFrame + 2)) {
                if (playerManager.ActiveUnitController.CanGlide) {
                    currentState = AnyRPGCharacterState.Glide;
                    return;
                }
                currentState = AnyRPGCharacterState.Fall;
                return;
            }

            //MoveRelative();
        }

        void Fall_EnterState() {
            canJump = false;
            if (playerManager.ActiveUnitController.UnitAnimator.GetInt("Jumping") != 2) {
                playerManager.ActiveUnitController.UnitAnimator.SetTrigger("FallTrigger");
                playerManager.ActiveUnitController.UnitAnimator.SetJumping(2);
            }
            currentFallDistance = 0f;
            fallStartHeight = playerManager.ActiveUnitController.transform.position.y;

            // clamp y velocity to prevent launching off ramps
            playerManager.ActiveUnitController.UnitMotor?.Move(new Vector3(playerManager.ActiveUnitController.RigidBody.velocity.x, Mathf.Clamp(playerManager.ActiveUnitController.RigidBody.velocity.y, -53, 0), playerManager.ActiveUnitController.RigidBody.velocity.z));
        }

        void Fall_StateUpdate() {

            if (playerManager.ActiveUnitController.InWater == true) {
                if (CheckForSwimming() == true) {
                    currentState = AnyRPGCharacterState.Swim;
                    return;
                }
            }

            if (playerManager.PlayerController.allowedInput
                && playerManager.ActiveUnitController.CanFly
                && playerManager.PlayerController.inputFly) {
                currentState = AnyRPGCharacterState.Fly;
                return;
            }

            if (playerManager.PlayerController.allowedInput
                && playerManager.ActiveUnitController.CanGlide) {
                currentState = AnyRPGCharacterState.Glide;
                return;
            }


            if (touchingGround && groundAngle <= slopeLimit) {
                if ((playerManager.PlayerController.HasMoveInput() || playerManager.PlayerController.HasTurnInput()) && playerManager.PlayerController.canMove) {
                    currentState = AnyRPGCharacterState.Move;
                    return;
                }
                currentState = AnyRPGCharacterState.Idle;
                return;
            }

            // testing disable move call to let physics move the character
            //MoveRelative();
        }

        void Fall_ExitState() {
            playerManager.ActiveUnitController.UnitAnimator.SetJumping(0);
            currentFallDistance = fallStartHeight - playerManager.ActiveUnitController.transform.position.y;
        }

        void Glide_EnterState() {
            //Debug.Log("PlayerUnitMovementController.Glide_EnterState()");
            currentFallDistance = 0f;
            canJump = false;
            if (playerManager.ActiveUnitController.UnitAnimator.GetInt("Jumping") != 2) {
                playerManager.ActiveUnitController.UnitAnimator.SetTrigger("FallTrigger");
                playerManager.ActiveUnitController.UnitAnimator.SetJumping(2);
            }
            playerManager.ActiveUnitController.RigidBody.useGravity = false;
            playerManager.ActiveUnitController.UnitAnimator.SetTurnVelocity(0f);
            playerManager.ActiveUnitController.RigidBody.constraints = RigidbodyConstraints.FreezeRotation;

            // clamp y velocity to prevent launching off ramps
            playerManager.ActiveUnitController.UnitMotor?.Move(new Vector3(playerManager.ActiveUnitController.RigidBody.velocity.x, Mathf.Clamp(playerManager.ActiveUnitController.RigidBody.velocity.y, -53, 0), playerManager.ActiveUnitController.RigidBody.velocity.z));
        }

        void Glide_StateUpdate() {
            //Debug.Log("PlayerUnitMovementController.Glide_StateUpdate()");

            if (playerManager.ActiveUnitController.InWater == true) {
                if (CheckForSwimming() == true) {
                    //Debug.Log("PlayerUnitMovementController.Glide_StateUpdate() swimming");
                    currentState = AnyRPGCharacterState.Swim;
                    return;
                }
            }

            if (playerManager.PlayerController.allowedInput
                && playerManager.ActiveUnitController.CanFly
                && playerManager.PlayerController.inputFly) {
                //Debug.Log("PlayerUnitMovementController.Glide_StateUpdate() flying");
                currentState = AnyRPGCharacterState.Fly;
                return;
            }

            if (touchingGround) {
                if (groundAngle <= slopeLimit) {
                    if ((playerManager.PlayerController.HasMoveInput() || playerManager.PlayerController.HasTurnInput()) && playerManager.PlayerController.canMove) {
                        //Debug.Log("PlayerUnitMovementController.Glide_StateUpdate() moving");
                        currentState = AnyRPGCharacterState.Move;
                        return;
                    }
                    //Debug.Log("PlayerUnitMovementController.Glide_StateUpdate() idling");
                    currentState = AnyRPGCharacterState.Idle;
                    return;
                }
            }

            if (playerManager.ActiveUnitController.CanGlide == false) {
                //Debug.Log("PlayerUnitMovementController.Glide_StateUpdate() falling");
                currentState = AnyRPGCharacterState.Fall;
                return;
            }

            // ============ VELOCITY CALCULATIONS ============

            // set clampValue to default of max movement speed
            float clampValue = playerManager.MaxMovementSpeed;

            // get current movement speed and clamp it to current clamp value
            float calculatedSpeed = playerManager.ActiveUnitController.GlideSpeed;
            calculatedSpeed = Mathf.Clamp(calculatedSpeed, 0, clampValue);

            // multiply normalized movement by calculated speed to get actual movement
            localMoveVelocity = NormalizedGlideMovement(calculatedSpeed) * calculatedSpeed;
            adjustedlocalMoveVelocity = localMoveVelocity;
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.Swim_StateUpdate() currentMoveVelocity: " + currentMoveVelocity);

            CalculateTurnVelocity();

            MoveRelative();
        }

        void Glide_ExitState() {
            //Debug.Log("PlayerUnitMovementController.Glide_ExitState()");
            playerManager.ActiveUnitController.UnitAnimator.SetJumping(0);
            playerManager.ActiveUnitController.RigidBody.useGravity = true;

        }

        public Vector3 NormalizedGlideMovement(float calculatedSpeed) {
            // it's safe to check for touching ground here because although we should be gliding
            // we can still reach this block of code if we touch ground that is too sloped to walk on
            if (touchingSlope == true && (playerManager.ActiveUnitController.transform.InverseTransformPoint(slopePoint).z > 0f)) {
                //Debug.Log("NormalizedGlideMovement(" + calculatedSpeed + ") slopePoint: " + playerManager.ActiveUnitController.transform.InverseTransformPoint(slopePoint));
                //Debug.Log("NormalizedGlideMovement(" + calculatedSpeed + ") downCross: (" + slopeDirection.x + ", " + slopeDirection.y + ", " + slopeDirection.z + ")");

                return playerManager.ActiveUnitController.transform.InverseTransformDirection(slopeDirection * (playerManager.ActiveUnitController.GlideFallSpeed / calculatedSpeed));
                //Vector3 groundDown = Quaternion.FromToRotation(Vector3.up, Vector3.Cross(groundCross, groundHitInfo.normal)) * playerManager.ActiveUnitController.transform.forward;
                //Debug.DrawLine(groundHitInfo.point, groundHitInfo.point + groundDown, Color.blue);
                //return groundDirection * playerManager.ActiveUnitController.GlideFallSpeed;
            } else { 
                float glideGravity = -playerManager.ActiveUnitController.GlideFallSpeed / calculatedSpeed;
                Vector3 returnValue = new Vector3(0f, glideGravity, 1f);
                return returnValue;
            }
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
            /*
            if (inputVector != Vector3.zero) {
                qRelativeVelocity = Quaternion.LookRotation(airForwardDirection, Vector3.up) * inputVector;
            }
            */

            Vector3 tRelativeVelocity = playerManager.ActiveUnitController.transform.TransformDirection(inputVector);
            /*
            if (qRelativeVelocity != Vector3.zero && tRelativeVelocity != Vector3.zero) {
                //Debug.Log("CharacterRelativeInput(" + inputVector + "): qRelativeVelocity: " + qRelativeVelocity + "; tRelativeVelocity: " + tRelativeVelocity);
            }
            //Debug.Log("PlayerUnitMovementController.CharacterRelativeInput(" + inputVector + "): return " + qRelativeVelocity + "; transformF: " + playerManager.ActiveUnitController.transform.forward + "; airForwardDirection: " + airForwardDirection);
            */
            //return qRelativeVelocity;
            return tRelativeVelocity;
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

        private bool RaycastForGround(float raycastHeight = 0.3f) {
            bool returnValue = false;
            closestSlopeDistance = raycastHeight;
            closestTouchingGroundDistance = raycastHeight;
            //highestWalkableGroundDistance = 0f;

            // create a ring of downward raycasts in a circle around the player at evenly spaced angles
            for (int i = 0; i < 12; i++) {
                Vector3 raycastPoint = (playerManager.ActiveUnitController.transform.position + (Vector3.up * raycastHeight) + (Vector3.up * 0.01f)) + (Quaternion.AngleAxis((360f / 12f) * i, Vector3.up) * Vector3.forward * colliderRadius);
                Debug.DrawLine(raycastPoint, new Vector3(raycastPoint.x, raycastPoint.y - raycastHeight - 0.02f, raycastPoint.z), Color.cyan);
                if (Physics.Raycast(raycastPoint, Vector3.down, out groundHitInfo, Mathf.Infinity, groundMask)) {
                    float groundHitHeight = playerManager.ActiveUnitController.transform.InverseTransformPoint(groundHitInfo.point).y;
                    
                    // determine if this is the closest ground distance, not caring if the player is actually touching it or not
                    // tihs calculation is only used for applying downforce later
                    if (groundHitHeight < 0f && groundHitHeight > closestGroundDistance) {
                        closestGroundDistance = groundHitHeight;
                    }

                    // determine if the player is touching this ground
                    if ((groundHitInfo.point.y > (raycastPoint.y - (raycastHeight + 0.02f))) && Vector3.Angle(groundHitInfo.normal, Vector3.up) <= slopeLimit) {
                        returnValue = true;
                        // save the ground info if the slope is the closest to the player feet, preferring ground that is at or below the player feet
                        if (((closestTouchingGroundDistance < 0f) && (groundHitHeight < 0f) && (playerManager.ActiveUnitController.transform.position.y + groundHitHeight < closestTouchingGroundDistance))
                                || (groundHitHeight == 0f)
                                || (groundHitHeight < closestTouchingGroundDistance)) {
                            groundPoint = groundHitInfo.point;
                            groundNormal = groundHitInfo.normal;
                            Debug.DrawLine(groundHitInfo.point, groundHitInfo.point + groundHitInfo.normal, Color.red);
                            Vector3 groundCross = Vector3.Cross(groundHitInfo.normal, Vector3.up);
                            Debug.DrawLine(groundHitInfo.point, groundHitInfo.point + groundCross, Color.red);
                            Vector3 downCross = Vector3.Cross(groundHitInfo.normal, groundCross);
                            Debug.DrawLine(groundHitInfo.point, groundHitInfo.point + downCross, Color.red);

                        }
                    }

                    // determine if the player is touching a slope
                    if (groundHitHeight > 0f && Vector3.Angle(groundHitInfo.normal, Vector3.up) > slopeLimit) {
                        touchingSlope = true;
                        // save the slope info if the slope is the closest to the player feet
                        if (groundHitHeight < closestSlopeDistance) {
                            closestSlopeDistance = groundHitHeight;
                            slopePoint = groundHitInfo.point;
                            slopeNormal = groundHitInfo.normal;
                            slopeAngle = Vector3.Angle(groundHitInfo.normal, Vector3.up);

                            Debug.DrawLine(slopePoint, slopePoint + slopeNormal, Color.blue);
                            Vector3 groundCross = Vector3.Cross(slopeNormal, Vector3.up);
                            Debug.DrawLine(slopePoint, slopePoint + groundCross, Color.blue);
                            slopeDirection = Vector3.Cross(slopeNormal, groundCross);
                            Debug.DrawLine(slopePoint, slopePoint + slopeDirection, Color.blue);

                        }
                    }

                    // determine if the point hit is considered at a walkable height and if it's the highest walkable height
                    /*
                    if (groundHitHeight > 0f && groundHitHeight > highestWalkableGroundDistance && Vector3.Angle(groundHitInfo.normal, Vector3.up) <= slopeLimit) {
                        highestWalkableGroundDistance = groundHitHeight;
                    }
                    */
                }
            }
            /*
            if (returnValue == true) {
                Debug.Log("RaycastForGround() : ground angle: " + Vector3.Angle(groundNormal, Vector3.up));
            }
            */

            // in the case that the player is not touching the ground and is touching a slope, set the ground point and ground normal to slope values
            // because otherwise they will remain set to whatever ground was directly underneath the player, no matter how far below that is
            if (returnValue == false && touchingSlope == true) {
                groundPoint = slopePoint;
                groundNormal = slopeNormal;
            }
            Debug.DrawLine(groundPoint, groundPoint + Vector3.up, Color.magenta);
            return returnValue;
        }

        public bool MaintainingGround() {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.MaintainingGround");
            return closeToGround;
        }

        private Vector3 NormalizedSwimMovement() {
            //Debug.Log("PlayerUnitMovementController.NormalizedSwimMovement(): groundAngle: " + groundAngle + "; backwardGroundAngle: " + backwardGroundAngle);

            Vector3 returnValue = playerManager.PlayerController.NormalizedMoveInput;

            // check for right mouse button held down to adjust swim angle based on camera angle
            bool chestBelowWater = (playerManager.ActiveUnitController.transform.position.y + playerManager.ActiveUnitController.FloatHeight) < (playerManager.ActiveUnitController.CurrentWater[0].SurfaceHeight - (playerManager.ActiveUnitController.SwimSpeed * Time.fixedDeltaTime));

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
                || (playerManager.PlayerController.inputFly == true && (chestBelowWater == true || playerManager.ActiveUnitController.CanFly))) {
                returnValue.y = (playerManager.PlayerController.inputFly == true ? 1 : 0) + (playerManager.PlayerController.inputSink == true ? -1 : 0);
            }

            return returnValue;
        }

        private Vector3 NormalizedFlyMovement() {
            //Debug.Log("PlayerUnitMovementController.NormalizedSwimMovement(): groundAngle: " + groundAngle + "; backwardGroundAngle: " + backwardGroundAngle);

            Vector3 returnValue = playerManager.PlayerController.NormalizedMoveInput;

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
                    || (cameraAngle < 0f && returnValue.z < 0f && !touchingGround) // camera below and moving back / down
                    ) {
                    //Debug.Log(gameObject.name + ".PlayerUnitMovementController.SwimMovement(): camera Angle: " + cameraAngle + "; direction: " + returnValue.z +
                    //    "; chest height: " + (playerManager.ActiveUnitController.transform.position.y + playerManager.ActiveUnitController.ChestHeight) + "; surface: " + playerManager.ActiveUnitController.CurrentWater[0].SurfaceHeight + "; speed: " + (playerManager.ActiveUnitController.SwimSpeed * Time.deltaTime));
                    returnValue = Quaternion.AngleAxis(cameraManager.MainCameraGameObject.transform.localEulerAngles.x, Vector3.right) * returnValue;
                }
            }

            // if the jump or crouch buttons were held down, their values override the camera angle and allow movement straight up or down
            // ignore if swim speed would not result in a bounce out of the water
            if (playerManager.PlayerController.inputSink == true
                || playerManager.PlayerController.inputFly == true) {
                returnValue.y = (playerManager.PlayerController.inputFly == true ? 1 : 0) + (playerManager.PlayerController.inputSink == true ? -1 : 0);
            }

            return returnValue;
        }

        private Vector3 NormalizedLocalMovement(float calculatedSpeed) {
            //Debug.Log("PlayerUnitMovementController.LocalMovement(): groundAngle: " + groundAngle + "; backwardGroundAngle: " + backwardGroundAngle);
            Vector3 normalizedInput = playerManager.PlayerController.NormalizedMoveInput;

            // determine if there is an obstacle in front, and if it is stairs
            CheckFrontObstacle(calculatedSpeed);

            Vector3 newReturnValue;
            float usedAngle = groundAngle;
            
            // the normal is the normal of the ground below the player
            Vector3 localGroundNormal = playerManager.ActiveUnitController.transform.InverseTransformDirection(groundNormal);
            Vector3 usedGroundNormal = groundNormal;
            // the player is near a front obstacle, and that obstacle is below the slope limit, use its normal
            if (nearFrontObstacle && frontAngleDifferent && frontObstacleAngle < slopeLimit) {
                //Debug.Log("near front obstacle and front angle (" + frontObstacleAngle + ") is different than ground angle (" + rawGroundAngle + "), adjusting forward direction");
                localGroundNormal = playerManager.ActiveUnitController.transform.InverseTransformDirection(forwardHitInfo.normal);
                usedGroundNormal = forwardHitInfo.normal;
            } else {
                // the player is near stairs in the direction of travel
                //if (RaycastForStairs(playerManager.ActiveUnitController.transform.TransformDirection(normalizedInput), 0.5f)) {
                if (nearStairs) {
                    //Debug.Log("near stairs, adjusting forward direction");
                    //localGroundNormal = playerManager.ActiveUnitController.transform.InverseTransformDirection(forwardHitInfo.normal);

                    // 0.2f is an arbitrary distance at the top of the stair is below the start of the curve on the bottom of a capsule collider of 2m height
                    // if the stairs are higher than 0.3f (the start of the vertical section on the collider) and the player is too close to the stair,
                    // any angled approach will lose all momentum from running straight into the stair and the player will get stuck

                    if (stairDownHitPoint.y - playerManager.ActiveUnitController.transform.position.y < 0.2f
                        || playerManager.ActiveUnitController.transform.InverseTransformPoint(new Vector3(forwardHitPoint.x, playerManager.ActiveUnitController.transform.position.y, forwardHitPoint.z)).magnitude > (colliderRadius + 0.01f)) {
                        localGroundNormal = playerManager.ActiveUnitController.transform.InverseTransformDirection(stairRampNormal);
                        usedGroundNormal = stairRampNormal;
                        //Debug.Log("using stair ramp normal: " + localGroundNormal + "; height: " + (stairDownHitPoint.y - playerManager.ActiveUnitController.transform.position.y));
                    } else {
                        //Debug.Log("distance from wall: " + playerManager.ActiveUnitController.transform.InverseTransformPoint(forwardHitInfo.point).magnitude);
                        localGroundNormal = playerManager.ActiveUnitController.transform.InverseTransformDirection(forwardHitInfo.normal);
                        usedGroundNormal = forwardHitInfo.normal;
                        //Debug.Log("using front normal: " + localGroundNormal);
                    }
                }
            }

            // translate the input so that the up direction is the same as the normal (up direction) of whatever ground or slope the player is on
            // this prevents losing speed up hills from slamming horizontally into the hill
            newReturnValue = Vector3.Cross(Quaternion.LookRotation(normalizedInput, Vector3.up) * playerManager.ActiveUnitController.transform.InverseTransformDirection(playerManager.ActiveUnitController.transform.right), localGroundNormal);
            // next line works when facing world axis only
            //newReturnValue = playerManager.ActiveUnitController.transform.InverseTransformDirection(Quaternion.LookRotation(forwardDirection, localGroundNormal) * normalizedInput);
            //Vector3 forwardDirection = playerManager.ActiveUnitController.transform.forward;
            /*
            Vector3 forwardDirection = Vector3.forward;
            Vector3 worldInput = playerManager.ActiveUnitController.transform.TransformDirection(normalizedInput);
            //Vector3 rotatedInput = Quaternion.LookRotation(forwardDirection, groundNormal) * worldInput;
            //Vector3 rotatedForward = Quaternion.LookRotation(forwardDirection, groundNormal) * forwardDirection;
            Vector3 rotatedInput = Quaternion.LookRotation(forwardDirection, groundNormal) * worldInput;
            // handle vertical ground normal case, which results in loss of momentum and odd rotations in some directions
            if (groundNormal.y == 0f) {
                //rotatedInput = worldInput + (Vector3.up);
                rotatedInput = Vector3.up;
            }

            newReturnValue = playerManager.ActiveUnitController.transform.InverseTransformDirection(rotatedInput);
            Debug.Log("position: " + playerManager.ActiveUnitController.transform.position + "; forwardDirection: " + forwardDirection + "; worldInput: " + worldInput + "; rotatedInput: " + rotatedInput + "; groundNormal: " + groundNormal + "; newReturnValue: " + newReturnValue);
            */

            // limit upward momentum near stairs to prevent overshooting the stairs in the vertical direction
            if (nearStairs) {
                //Debug.Log("unclamped returnValue: " + newReturnValue.y + "; deltaTime: " + Time.deltaTime + "; fixedDeltaTime: " + Time.fixedDeltaTime);
                //float clampedReturnValue = Mathf.Clamp(newReturnValue.y, 0f, (playerManager.ActiveUnitController.transform.InverseTransformPoint(stairDownHitInfo.point).y / calculatedSpeed) * (1/Time.deltaTime));
                float clampedReturnValue = Mathf.Clamp(newReturnValue.y, 0f, playerManager.ActiveUnitController.transform.InverseTransformPoint(stairDownHitPoint).y / calculatedSpeed / Time.fixedDeltaTime);
                //Debug.Log("clamped returnValue: " + clampedReturnValue + "; unclamped: " + newReturnValue.y + "; deltaTime: " + Time.deltaTime + "; fixedDeltaTime: " + Time.fixedDeltaTime);
                newReturnValue.y = clampedReturnValue;
            }

            // apply downforce
            if (groundAngle == 0 && nearFrontObstacle == false && nearStairs == false && touchingGround == false) {
                // this should make the character stick to the ground better when actively moving while grounded
                // ONLY APPLY Y DOWNFORCE ON FLAT GROUND, this will apply a y downforce multiplied by speed, not the existing y downforce from physics (gravity)
                float yValue = 0f;
                if (playerManager.ActiveUnitController.transform.InverseTransformPoint(groundPoint).y < -0.001f) {
                    // set a downforce value that should take the character exactly to the ground, and not lower to avoid losing momentum from physics colission with ground
                    yValue = Mathf.Clamp(1, 0, -closestGroundDistance / calculatedSpeed / Time.fixedDeltaTime) * -1;
                    //yValue = -1;
                    /*
                    Debug.Log("NormalizedLocalMovement() position: " + playerManager.ActiveUnitController.transform.position.y +
                        "; Applying extra down force: " + yValue +
                        "; ground distance: " + closestGroundDistance);
                      */  
                    //Debug.Break();
                }
                newReturnValue = new Vector3(newReturnValue.x, yValue, newReturnValue.z);
            }
            //Debug.Log("newReturnValue: (" + newReturnValue.x + ", " + newReturnValue.y + ", " + newReturnValue.z + ")");
            return newReturnValue;
        }

        /// <summary>
        /// Calculate the initial velocity of a jump based off gravity and desired maximum height attained
        /// </summary>
        /// <param name="jumpHeight"></param>
        /// <param name="gravity"></param>
        /// <returns></returns>
        private float CalculateJumpSpeed(float jumpHeight, float gravity) {
            return Mathf.Sqrt(2 * jumpHeight * gravity);
        }

        private void CheckGround() {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CheckGround()");

            closestGroundDistance = 0f;
            touchingSlope = false;

            // set an inital ground distance based on a direct downward raycast from the center of the player
            // later, a more accurate search will be done to see if there is a closer ground distance at the edges of the player capsule
            if (Physics.Raycast(playerManager.ActiveUnitController.transform.position + (Vector3.up * 0.25f), -Vector3.up, out groundHitInfo, Mathf.Infinity, groundMask)) {
                closestGroundDistance = playerManager.ActiveUnitController.transform.InverseTransformPoint(groundHitInfo.point).y;
                groundNormal = groundHitInfo.normal;
                groundPoint = groundHitInfo.point;
            }

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
                if (RaycastForGround()) {
                    touchingGround = true;
                } else {
                    touchingGround = false;
                    // downward cast for normals
                }
            }

            groundAngle = Vector3.Angle(groundNormal, Vector3.up);
            //Debug.Log("rawGroundAngle: " + rawGroundAngle);

            // this is necessary in case the player is moving fast and went off a cliff and we want to apply downforce
            // also needed in case of moving up stairs that are higher than 0.25f (the close to ground height)
            Collider[] hitColliders = Physics.OverlapBox(playerManager.ActiveUnitController.transform.position, maintainingGroundExtents, playerManager.ActiveUnitController.transform.rotation, groundMask);
            if (hitColliders.Length > 0) {
                closeToGround = true;
            }
            
        }

        private void CheckFrontObstacle(float calculatedSpeed) {
            // reset variables
            nearFrontObstacle = false;
            frontAngleDifferent = false;
            nearStairs = false;
            float detectionDistance = stairDetectionDistance + (calculatedSpeed * Time.fixedDeltaTime);
            //float detectionDistance = stairDetectionDistance;

            // determine direction of travel in world space
            Vector3 directionOfTravel = playerManager.ActiveUnitController.transform.forward;
            if (localMoveVelocity.x != 0 || localMoveVelocity.z != 0) {
                directionOfTravel = playerManager.ActiveUnitController.transform.TransformDirection(new Vector3(localMoveVelocity.x, 0, localMoveVelocity.z)).normalized;
            }

            // raycast from center in direction of travel
            Vector3 originPoint = playerManager.ActiveUnitController.transform.TransformPoint(Quaternion.LookRotation(playerManager.ActiveUnitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(0f, 0.001f, colliderRadius));
            //Vector3 originPoint = playerManager.ActiveUnitController.transform.TransformPoint(Quaternion.LookRotation(playerManager.ActiveUnitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(0f, 0.001f, 0f));
            Debug.DrawLine(originPoint,
                originPoint + (directionOfTravel * detectionDistance),
                Color.black);
            if (Physics.Raycast(originPoint, directionOfTravel, out forwardHitInfo, detectionDistance, groundMask)) {
                // we are near an obstacle in front center
                nearFrontObstacle = true;
            } else {
                // raycast from left in direction of travel
                originPoint = playerManager.ActiveUnitController.transform.TransformPoint(Quaternion.LookRotation(playerManager.ActiveUnitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(-colliderRadius , 0.001f, 0f));
                Debug.DrawLine(originPoint,
                    originPoint + (directionOfTravel * detectionDistance),
                    Color.black);
                if (Physics.Raycast(originPoint, directionOfTravel, out forwardHitInfo, detectionDistance, groundMask)) {
                    // we are near an obstacle in front left
                    nearFrontObstacle = true;
                } else {
                    // raycast from right in direction of travel
                    originPoint = playerManager.ActiveUnitController.transform.TransformPoint(Quaternion.LookRotation(playerManager.ActiveUnitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(colliderRadius, 0.001f, 0f));
                    Debug.DrawLine(originPoint,
                        originPoint + (directionOfTravel * detectionDistance),
                        Color.black);
                    if (Physics.Raycast(originPoint, directionOfTravel, out forwardHitInfo, detectionDistance, groundMask)) {
                        // we are near an obstacle in front right
                        nearFrontObstacle = true;
                    }
                }
            }

            // detect higher stairs (multiple stairs that are under the maximum step height)
            bool nearFrontHighStair = false;
            Vector3 highestDownNormal;
            Vector3 highestDownOriginPoint;
            Vector3 highestDownPoint = playerManager.ActiveUnitController.transform.position;
            RaycastHit highForwardHitInfo = new RaycastHit();

            if (nearFrontObstacle) {
                RaycastHit frontHighStairDownHitInfo;

                // raycast from front in direction of travel downwards
                Vector3 downOriginPoint = playerManager.ActiveUnitController.transform.TransformPoint(Quaternion.LookRotation(playerManager.ActiveUnitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(0f, stepHeight + 0.01f, detectionDistance));
                //Vector3 originPoint = playerManager.ActiveUnitController.transform.TransformPoint(Quaternion.LookRotation(playerManager.ActiveUnitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(0f, 0.001f, 0f));
                Debug.DrawLine(downOriginPoint,
                    downOriginPoint + (Vector3.down * stepHeight),
                    Color.black);
                if (Physics.Raycast(downOriginPoint, Vector3.down, out frontHighStairDownHitInfo, stepHeight, groundMask)) {
                    //Debug.Log("we are near an obstacle in front center at " + frontHighStairDownHitInfo.point);
                    // we are near an obstacle in front center
                    if (Vector3.Angle(frontHighStairDownHitInfo.normal, Vector3.up) <= slopeLimit) {
                        nearFrontHighStair = true;
                        highestDownOriginPoint = downOriginPoint;
                        highestDownNormal = frontHighStairDownHitInfo.normal;
                        highestDownPoint = frontHighStairDownHitInfo.point;
                        Vector3 newOriginPoint = playerManager.ActiveUnitController.transform.TransformPoint(Quaternion.LookRotation(playerManager.ActiveUnitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(0f, playerManager.ActiveUnitController.transform.InverseTransformPoint(highestDownPoint).y - 0.001f, 0f));
                        Physics.Raycast(newOriginPoint, directionOfTravel, out highForwardHitInfo, detectionDistance, groundMask);
                        Debug.DrawLine(newOriginPoint, newOriginPoint + directionOfTravel, Color.black);
                    }
                }
                
                // raycast from left in direction of travel
                downOriginPoint = playerManager.ActiveUnitController.transform.TransformPoint(Quaternion.LookRotation(playerManager.ActiveUnitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(-colliderRadius, stepHeight + 0.01f, detectionDistance));
                Debug.DrawLine(downOriginPoint,
                    downOriginPoint + (Vector3.down * stepHeight),
                    Color.black);
                if (Physics.Raycast(downOriginPoint, Vector3.down, out frontHighStairDownHitInfo, stepHeight, groundMask)) {
                    //Debug.Log("we are near an obstacle in front left at " + frontHighStairDownHitInfo.point);
                    // we are near an obstacle in front left
                    if (Vector3.Angle(frontHighStairDownHitInfo.normal, Vector3.up) <= slopeLimit) {

                        nearFrontHighStair = true;
                        if (frontHighStairDownHitInfo.point.y > highestDownPoint.y) {
                            highestDownOriginPoint = downOriginPoint;
                            highestDownNormal = frontHighStairDownHitInfo.normal;
                            highestDownPoint = frontHighStairDownHitInfo.point;
                            Vector3 newOriginPoint = playerManager.ActiveUnitController.transform.TransformPoint(Quaternion.LookRotation(playerManager.ActiveUnitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(-colliderRadius, playerManager.ActiveUnitController.transform.InverseTransformPoint(highestDownPoint).y - 0.001f, 0f));
                            Physics.Raycast(newOriginPoint, directionOfTravel, out highForwardHitInfo, detectionDistance, groundMask);
                            Debug.DrawLine(newOriginPoint, newOriginPoint + directionOfTravel, Color.black);
                        }
                    }
                }
                // raycast from right in direction of travel
                downOriginPoint = playerManager.ActiveUnitController.transform.TransformPoint(Quaternion.LookRotation(playerManager.ActiveUnitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(colliderRadius, stepHeight + 0.01f, detectionDistance));
                Debug.DrawLine(downOriginPoint,
                    downOriginPoint + (Vector3.down * stepHeight),
                    Color.black);
                if (Physics.Raycast(downOriginPoint, Vector3.down, out frontHighStairDownHitInfo, stepHeight, groundMask)) {
                    //Debug.Log("we are near an obstacle in front right at " + frontHighStairDownHitInfo.point);
                    // we are near an obstacle in front right
                    if (Vector3.Angle(frontHighStairDownHitInfo.normal, Vector3.up) <= slopeLimit) {

                        nearFrontHighStair = true;
                        if (frontHighStairDownHitInfo.point.y > highestDownPoint.y) {
                            highestDownOriginPoint = downOriginPoint;
                            highestDownNormal = frontHighStairDownHitInfo.normal;
                            highestDownPoint = frontHighStairDownHitInfo.point;
                            Vector3 newOriginPoint = playerManager.ActiveUnitController.transform.TransformPoint(Quaternion.LookRotation(playerManager.ActiveUnitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(colliderRadius, playerManager.ActiveUnitController.transform.InverseTransformPoint(highestDownPoint).y - 0.001f, 0f));
                            Physics.Raycast(newOriginPoint, directionOfTravel, out highForwardHitInfo, detectionDistance, groundMask);
                            Debug.DrawLine(newOriginPoint, newOriginPoint + directionOfTravel, Color.black);
                        }
                    }
                }

            }


            Vector3 frontStairHeight = Vector3.zero;
            if (nearFrontObstacle) {
                frontObstacleAngle = Vector3.Angle(forwardHitInfo.normal, Vector3.up);
                //Debug.Log("front obstacle angle: " + frontObstacleAngle);

                // we could be going up a ramp, determine if the obstacle in front has a different angle than the ground below us
                if (forwardHitInfo.normal != groundNormal) {
                    //Debug.Log("front obstacle angle is different than ground angle: " + rawGroundAngle);
                    frontAngleDifferent = true;
                }

                // check if the obstacle is stairs
                if (frontAngleDifferent == true && frontObstacleAngle > slopeLimit) {
                    Vector3 raycastPoint = forwardHitInfo.point + (directionOfTravel * 0.01f);
                    raycastPoint = new Vector3(raycastPoint.x, playerManager.ActiveUnitController.transform.position.y + stepHeight + 0.001f, raycastPoint.z);
                    //Debug.Log("CheckFrontObstacle() front Angle Different and frontObstacle > slopeLimit; localMoveVelocity: " + localMoveVelocity + "; directionOfTravel: " + directionOfTravel + "; forwardHitInfo: " + forwardHitInfo.point + "; player: " + playerManager.ActiveUnitController.transform.position + "; raycastpoint: " + raycastPoint);
                    Debug.DrawLine(raycastPoint, new Vector3(raycastPoint.x, raycastPoint.y - stepHeight - 0.001f, raycastPoint.z), Color.cyan);
                    if (Physics.Raycast(raycastPoint, Vector3.down, out stairDownHitInfo, stepHeight, groundMask)) {
                        // we hit something that is low enough to step on, if it is below the slope limit, we can consider it to be a stair step
                        if (Vector3.Angle(stairDownHitInfo.normal, Vector3.up) < slopeLimit) {
                            frontStairHeight = stairDownHitInfo.point;
                            /*
                            Debug.Log("CheckFrontObstacle(): y position: " + playerManager.ActiveUnitController.transform.position.y +
                                "; stairs detected angle: " + Vector3.Angle(stairDownHitInfo.normal, Vector3.up) +
                                "; stairHeight: " + "(" + stairHeight.x + ", " + stairHeight.y + ", " + stairHeight.z + ")" +
                                "; object: " + stairDownHitInfo.collider.gameObject.name);
                                */
                            nearStairs = true;
                        }
                    }
                }
                if (nearStairs || nearFrontHighStair) {

                    // new code to detect stairs from greater distance and make angle upward at a more gradual slope to prevent the jittery updward movement that comes from using
                    // the completely horizontal normal you get from striking the front of the stairs
                    Vector3 bottomPoint;
                    Vector3 angleRay = Vector3.zero;
                    //Debug.Log("nearStairs: " + nearStairs + "; nearFrontHighStairs: " + nearFrontHighStair + "; frontStairHeight.y: " + frontStairHeight.y + "; highestDownPoint.y: " + highestDownPoint.y);
                    if (nearFrontHighStair == true && frontStairHeight.y < highestDownPoint.y) {
                        bottomPoint = playerManager.ActiveUnitController.transform.position;
                        forwardHitPoint = highForwardHitInfo.point;
                        stairDownHitPoint = highestDownPoint;
                        angleRay = new Vector3(forwardHitPoint.x, stairDownHitPoint.y, forwardHitPoint.z) - bottomPoint;
                        //Debug.Log("using high stair values; highForwardHitInfo: " + highForwardHitInfo.point + "; bottomPoint: " + bottomPoint + "; angleRay: " + angleRay);
                    } else {
                        bottomPoint = originPoint;
                        forwardHitPoint = forwardHitInfo.point;
                        stairDownHitPoint = stairDownHitInfo.point;
                        angleRay = new Vector3(forwardHitPoint.x, stairDownHitPoint.y, forwardHitPoint.z) - bottomPoint;
                    }

                    Debug.DrawLine(bottomPoint,
                        bottomPoint + angleRay,
                        Color.cyan);

                    Vector3 secondPoint = bottomPoint + (Quaternion.AngleAxis(90f, Vector3.up) * angleRay);
                    secondPoint = new Vector3(secondPoint.x, bottomPoint.y, secondPoint.z);
                    Debug.DrawLine(bottomPoint,
                        secondPoint,
                        Color.red);
                    Vector3 calculatedNormal = Vector3.Cross(angleRay, secondPoint - bottomPoint).normalized;
                    Debug.DrawLine(bottomPoint,
                        bottomPoint + calculatedNormal,
                        Color.red);
                    //Debug.Log("CheckFrontObstacle() calculatedNormal: " + calculatedNormal + "; angleRay: " + angleRay + "; line2: " + (secondPoint - bottomPoint) + "; angle: " + Vector3.Angle(angleRay.normalized, Vector3.up));
                    //Debug.Log("CheckFrontObstacle() angle: " + Vector3.Angle(angleRay.normalized, Vector3.up));
                    stairRampNormal = calculatedNormal;
                }

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

            //Debug.DrawLine(playerManager.ActiveUnitController.transform.position, playerManager.ActiveUnitController.transform.position + forwardDirection * closeToGroundHeight * 2, Color.blue);
            //Debug.DrawLine(playerManager.ActiveUnitController.transform.position, playerManager.ActiveUnitController.transform.position + backwardDirection * closeToGroundHeight * 2, Color.magenta);
            Debug.DrawLine(playerManager.ActiveUnitController.transform.position + (Vector3.up * 0.25f), (playerManager.ActiveUnitController.transform.position + (Vector3.up * 0.25f)) - (Vector3.up * closeToGroundHeight), Color.green);

        }

        private void OnDrawGizmos() {
            if (playerManager == null || playerManager.ActiveUnitController == null) {
                return;
            }
            /*
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(playerManager.ActiveUnitController.transform.position, (Quaternion.LookRotation(playerManager.ActiveUnitController.transform.forward) * touchingGroundExtents) * 2);
            */

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(playerManager.ActiveUnitController.transform.position, maintainingGroundExtents * 2);
        }


    }

}