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

        // the movement input in relation to the character, without speed adjustment
        private Vector3 localInput;

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

        private Vector3 bottomOriginPoint = Vector3.zero;
        private Vector3 lowObstacleOriginPoint = Vector3.zero;
        private Vector3 topOriginPoint = Vector3.zero;
        private Vector3 forwardOriginPoint = Vector3.zero;
        private Vector3 losOriginPoint = Vector3.zero;

        // the raw angle of the ground below
        private float groundAngle;
        
        // the raw angle of the ground below
        private float slopeAngle;

        // used for determining how far above the ground the player is for applying downforce
        private float closestGroundDistance = 0f;

        private float closestWalkableGroundDistance = 0f;

        // the closest distance that is above the players feet that is greater than the slope limit
        private float closestSlopeDistance = 0f;

        // the closest ground that is under the players feet
        private float closestTouchingGroundDistance = 0f;

        // the highest distance under the raycast that is blow the slope limit
        private float highestWalkableGroundDistance = 0f;

        // a calculated value
        //private float groundAngle;

        private bool rayCastForGroundRun = false;

        // is there an obstacle directly in front of the player as determined by a downward raycast 1cm in front of collider
        private bool nearFrontObstacle = false;

        // is there an obstacle close in front of us near the ground in the direction of travel
        private bool nearBottomFrontObstacle = false;

        // is there an obstacle close in front of us that is less than stepheight from the closest ground
        private bool nearLowObstacle = false;

        // is there an obstacle close in front of us near the stair limit in the direction of travel
        private bool nearTopFrontObstacle = false;

        // the angle of the obstacle in front of us near the feet
        private float bottomFrontObstacleAngle;

        // the angle of the obstacle in front of us near the stair limit
        private float topFrontObstacleAngle;

        // determine if there is a change in the angle from the current ground (which could be a ramp) to the obstacle in front of us (which could be the same ramp)
        private bool bottomFrontAngleDifferent = false;

        // determine if there is a change in the angle from the current ground (which could be a ramp) to the obstacle in front of us (which could be the same ramp)
        private bool topFrontAngleDifferent = false;

        // determine if an obstacle in front of us is a stairs
        private bool nearBottomStairs = false;

        // determine if an obstacle in front of us near the stair limit is a stairs
        private bool nearTopStairs = false;

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
        //private Vector3 maintainingGroundExtents = new Vector3(0.65f, 0.5f, 0.65f);
        private Vector3 maintainingGroundExtents = new Vector3(0.3f, 0.5f, 0.3f);

        // raycast to determine ground normal
        private RaycastHit centerDownHitInfo;

        // raycasts to determine 
        private RaycastHit downHitInfo;

        // raycast to determine if an object is in front of the player near the ground
        private RaycastHit bottomForwardHitInfo;

        // raycast to determine if the forward hit info starting point is visible or behind an obstacle
        private RaycastHit losHitInfo;

        // raycast to determine if an object is in front of the player near the ground
        private RaycastHit obstacleCastHitInfo;

        // raycast to determine if an object is in front of the player near the stair limit
        private RaycastHit topForwardHitInfo;

        private Vector3 forwardHitPoint;

        // raycast to determine if an object is in an arc in front of the player
        private RaycastHit obstacleHitInfo;

        // downward raycast to determine if an object in front of the player is stairs
        private RaycastHit bottomStairDownHitInfo;

        // downward raycast to determine if an object in front of the player is stairs
        private RaycastHit topStairDownHitInfo;

        // forward raycast to determine the closest point on the top stair
        //private RaycastHit topStairForwardHitInfo;

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
        protected ControlsManager controlsManager = null;
        protected WindowManager windowManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            rotationSpeed = systemConfigurationManager.MaxTurnSpeed;
            groundMask = playerManager.DefaultGroundMask;
            GetComponentReferences();
            if (movementStateController != null) {
                movementStateController.enabled = true;
            }
            //stairDetectionDistance = Mathf.Tan(Mathf.Deg2Rad * (90f - slopeLimit)) * stepHeight;
            // in order to get a perfect 45 degree up stairs, they must be detected from half the distance of the collider plus the step height
            // if just stepheight is used, it will be a 45, but the bottom of the collider will intersect the stairs
            stairDetectionDistance = (colliderRadius / 2f) + stepHeight;
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
            controlsManager = systemGameManager.ControlsManager;
            windowManager = systemGameManager.WindowManager;
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
            //ApplyGravity();
            DrawDebugLines();
        }

        public void MoveRelative() {
            Vector3 relativeMovement;
            //if (controlsManager.GamePadModeActive) {
                //relativeMovement = CameraRelativeInput(adjustedlocalMoveVelocity);
            //} else {
                relativeMovement = CharacterRelativeInput(adjustedlocalMoveVelocity);
            //}
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
                if (
                    (inputManager.rightMouseButtonDown
                    && playerManager.ActiveUnitController.UnitProfile.UnitPrefabProps.RotateModel == false // account for rotate model mode when using keyboard
                    && (!inputManager.rightMouseButtonClickedOverUI || (namePlateManager != null ? namePlateManager.MouseOverNamePlate() : false)))
                    || (controlsManager.GamePadModeActive == false && Input.GetAxis("RightAnalogHorizontal") != 0f && windowManager.CurrentWindow == null)
                    ) {
                    //Debug.Log(gameObject.name + ".PlayerUnitMovementController.LateGlobalSuperUpdate(): resetting playerManager.ActiveUnitController.transform.forward");

                    playerManager.ActiveUnitController.transform.forward = new Vector3(cameraManager.MainCameraController.WantedDirection.x, 0, cameraManager.MainCameraController.WantedDirection.z);
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
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.Idle_EnterState()");
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

            if (!MaintainingGround() && groundAngle > slopeLimit) {
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

        private void GetLocalInput() {
            if (controlsManager.GamePadModeActive || playerManager.ActiveUnitController.UnitProfile.UnitPrefabProps.RotateModel) {
                // calculate the input relative to the camera in world space
                Vector3 cameraInput = Quaternion.Euler(0f, cameraManager.ActiveMainCamera.transform.rotation.eulerAngles.y, 0f) * playerManager.PlayerController.NormalizedMoveInput;

                localInput = playerManager.ActiveUnitController.transform.InverseTransformDirection(cameraInput);
                //Debug.Log("normalizedInput: " + playerManager.PlayerController.NormalizedMoveInput + "; cameraInput: " + cameraInput + "; localInput: " + localInput);
            } else {
                localInput = playerManager.PlayerController.NormalizedMoveInput;
            }
        }

        void Move_StateUpdate() {

            airForwardDirection = playerManager.ActiveUnitController.transform.forward;

            float calculatedSpeed = 0f;


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

            // since we are in the move state, reset velocity to zero so we can pick up the new values
            // allow falling while moving by clamping existing y velocity
            localMoveVelocity = new Vector3(0, Mathf.Clamp(playerManager.ActiveUnitController.RigidBody.velocity.y, -53, 0), 0);
            adjustedlocalMoveVelocity = localMoveVelocity;

            // determine direction of travel in world space
            Vector3 directionOfTravel = playerManager.ActiveUnitController.transform.forward;

            if (playerManager.PlayerController.HasMoveInput() && playerManager.PlayerController.canMove) {

                // set clampValue to default of max movement speed
                float clampValue = playerManager.MaxMovementSpeed;

                // set a clamp value to limit movement speed to walking if going backward
                /*
                if (currentMoveVelocity.z < 0) {
                    clampValue = 1;
                }
                */

                GetLocalInput();

                // get current movement speed and clamp it to current clamp value
                calculatedSpeed = Mathf.Clamp(playerManager.ActiveUnitController.MovementSpeed, 0, clampValue);

                // multiply normalized movement by calculated speed to get actual local movement
                localMoveVelocity = localInput * calculatedSpeed;

                if (localMoveVelocity.x != 0 || localMoveVelocity.z != 0) {
                    //if (controlsManager.GamePadModeActive == true) {
                        //directionOfTravel = CameraRelativeInput(new Vector3(localMoveVelocity.x, 0, localMoveVelocity.z)).normalized;
                        //Debug.Log("directionOfTravel: " + directionOfTravel);
                    //} else {
                        directionOfTravel = playerManager.ActiveUnitController.transform.TransformDirection(new Vector3(localMoveVelocity.x, 0, localMoveVelocity.z)).normalized;
                    //}
                }

                // determine if there is an obstacle in front, and if it is stairs
                CheckFrontObstacle(calculatedSpeed, directionOfTravel);

            }

            //if (!MaintainingGround() || (groundAngle > slopeLimit && nearBottomFrontObstacle == true && nearTopFrontObstacle == true && touchingGround == false)) {
            //Debug.Log("groundAngle: " + groundAngle + "; closestWalkablegrounddistance: " + closestWalkableGroundDistance + "; nearLowObstacle: " + nearLowObstacle + "; nearBottomFrontObstacle: " + nearBottomFrontObstacle + "; touchingGround: " + touchingGround);
            //if (!MaintainingGround() || (groundAngle > slopeLimit && touchingGround == false && nearLowObstacle == false)) {
            if (
                !MaintainingGround() ||
                //(groundAngle > slopeLimit && touchingGround == false && (nearLowObstacle == false || (nearLowObstacle == true && closestWalkableGroundDistance < -stepHeight))) // closestGroundDistance check for running off low obstacle
                (groundAngle > slopeLimit && nearBottomFrontObstacle == true && nearLowObstacle == false) ||
                (groundAngle > slopeLimit && nearBottomFrontObstacle == false && nearLowObstacle == false && closestWalkableGroundDistance < -stepHeight)
                ) { // closetoGround check for running backward off low obstacle
                //Debug.Log("groundAngle: " + groundAngle + "; closestWalkablegrounddistance: " + closestWalkableGroundDistance + "; nearLowObstacle: " + nearLowObstacle + "; nearBottomFrontObstacle: " + nearBottomFrontObstacle + "; touchingGround: " + touchingGround);
                //Debug.Break();
                if (playerManager.ActiveUnitController.CanFly) {
                    currentState = AnyRPGCharacterState.Fly;
                    return;
                } else {
                    if (playerManager.ActiveUnitController.CanGlide) {
                        currentState = AnyRPGCharacterState.Glide;
                        return;
                    }
                    //if (touchingGround == false) {
                        currentState = AnyRPGCharacterState.Fall;
                        return;
                    //}
                }
            }

            if ((playerManager.PlayerController.HasMoveInput() || playerManager.PlayerController.HasTurnInput()) && playerManager.PlayerController.canMove) {

                if (playerManager.PlayerController.HasMoveInput()) {
                    
                    adjustedlocalMoveVelocity = NormalizedLocalMovement(calculatedSpeed, directionOfTravel) * calculatedSpeed;
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
            //Debug.Log("PlayerUnitMovementController.Swim_EnterState()");
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
                    //Debug.Log("Idle-> Move: touchingGround: " + touchingGround);
                    currentState = AnyRPGCharacterState.Move;
                    //Debug.Break();
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
        /// World space movement based off camera facing.
        /// </summary>
        Vector3 CameraRelativeInput(Vector3 inputVector) {
            Debug.Log("PlayerUnitMovementController.CameraRelativeInput(" + inputVector + ")");
            //Forward vector relative to the camera
            
            return Quaternion.LookRotation(new Vector3(cameraManager.ActiveMainCamera.transform.forward.x, 0f, cameraManager.ActiveMainCamera.transform.forward.z).normalized) * inputVector;
        }

        Vector3 CameraRelativeInputOld(float inputX, float inputZ) {
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
            highestWalkableGroundDistance = 0f;

            // create a ring of downward raycasts in a circle around the player at evenly spaced angles
            for (int i = 0; i < 12; i++) {
                Vector3 raycastPoint = (playerManager.ActiveUnitController.transform.position + (Vector3.up * raycastHeight) + (Vector3.up * 0.01f)) + (Quaternion.AngleAxis((360f / 12f) * i, Vector3.up) * Vector3.forward * colliderRadius);
                Debug.DrawLine(raycastPoint, new Vector3(raycastPoint.x, raycastPoint.y - raycastHeight - 0.02f, raycastPoint.z), Color.cyan);
                if (Physics.Raycast(raycastPoint, Vector3.down, out centerDownHitInfo, Mathf.Infinity, groundMask)) {
                    float groundHitHeight = playerManager.ActiveUnitController.transform.InverseTransformPoint(centerDownHitInfo.point).y;
                    
                    // determine if this is the closest ground distance, not caring if the player is actually touching it or not
                    // tihs calculation is only used for applying downforce later
                    if (groundHitHeight < 0f && groundHitHeight > closestGroundDistance) {
                        closestGroundDistance = groundHitHeight;
                    }
                    if (groundHitHeight < 0f && groundHitHeight > closestWalkableGroundDistance) {
                        if (Vector3.Angle(Vector3.up, centerDownHitInfo.normal) <= slopeLimit) {
                            closestWalkableGroundDistance = groundHitHeight;
                        }
                    }

                    // determine if the player is touching this ground
                    if ((centerDownHitInfo.point.y > (raycastPoint.y - (raycastHeight + 0.02f))) && Vector3.Angle(centerDownHitInfo.normal, Vector3.up) <= slopeLimit) {

                        // due to physics engine allowing objects to pass partially into each other, ensure that any contact above 0.01f from player feet is an angle.  flat ground show be at or below the feet
                        //if ((groundHitInfo.normal == Vector3.up && groundHitInfo.point.y < (playerManager.ActiveUnitController.transform.position.y + 0.01f)) || Vector3.Angle(groundHitInfo.normal, Vector3.up) > 0f) {
                            returnValue = true;
                            // save the ground info if the slope is the closest to the player feet, preferring ground that is at or below the player feet
                            if (((closestTouchingGroundDistance < 0f) && (groundHitHeight < 0f) && (playerManager.ActiveUnitController.transform.position.y + groundHitHeight < closestTouchingGroundDistance))
                                    || (groundHitHeight == 0f)
                                    || (groundHitHeight < closestTouchingGroundDistance)) {
                                groundPoint = centerDownHitInfo.point;
                                groundNormal = centerDownHitInfo.normal;
                                Debug.DrawLine(centerDownHitInfo.point, centerDownHitInfo.point + centerDownHitInfo.normal, Color.red);
                                Vector3 groundCross = Vector3.Cross(centerDownHitInfo.normal, Vector3.up);
                                Debug.DrawLine(centerDownHitInfo.point, centerDownHitInfo.point + groundCross, Color.red);
                                Vector3 downCross = Vector3.Cross(centerDownHitInfo.normal, groundCross);
                                Debug.DrawLine(centerDownHitInfo.point, centerDownHitInfo.point + downCross, Color.red);

                            }
                        //}
                        
                    }

                    // determine if the player is touching a slope
                    if (groundHitHeight > 0f && Vector3.Angle(centerDownHitInfo.normal, Vector3.up) > slopeLimit) {
                        touchingSlope = true;
                        // save the slope info if the slope is the closest to the player feet
                        if (groundHitHeight < closestSlopeDistance) {
                            closestSlopeDistance = groundHitHeight;
                            slopePoint = centerDownHitInfo.point;
                            slopeNormal = centerDownHitInfo.normal;
                            slopeAngle = Vector3.Angle(centerDownHitInfo.normal, Vector3.up);

                            Debug.DrawLine(slopePoint, slopePoint + slopeNormal, Color.blue);
                            Vector3 groundCross = Vector3.Cross(slopeNormal, Vector3.up);
                            Debug.DrawLine(slopePoint, slopePoint + groundCross, Color.blue);
                            slopeDirection = Vector3.Cross(slopeNormal, groundCross);
                            Debug.DrawLine(slopePoint, slopePoint + slopeDirection, Color.blue);

                        }
                    }

                    // determine if the point hit is considered at a walkable height and if it's the highest walkable height
                    
                    if (groundHitHeight > 0f && groundHitHeight > highestWalkableGroundDistance && Vector3.Angle(centerDownHitInfo.normal, Vector3.up) <= slopeLimit) {
                        highestWalkableGroundDistance = groundHitHeight;
                    }
                    
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

            rayCastForGroundRun = true;
            return returnValue;
        }

        public bool MaintainingGround() {
            //Debug.Log(gameObject.name + ".PlayerUnitMovementController.MaintainingGround");
            return closeToGround;
        }

        private Vector3 NormalizedSwimMovement() {
            //Debug.Log("PlayerUnitMovementController.NormalizedSwimMovement(): groundAngle: " + groundAngle + "; backwardGroundAngle: " + backwardGroundAngle);

            GetLocalInput();
            Vector3 returnValue = localInput;

            // check for right mouse button held down to adjust swim angle based on camera angle
            bool chestBelowWater = (playerManager.ActiveUnitController.transform.position.y + playerManager.ActiveUnitController.FloatHeight) < (playerManager.ActiveUnitController.CurrentWater[0].SurfaceHeight - (playerManager.ActiveUnitController.SwimSpeed * Time.fixedDeltaTime));

            if (inputManager.rightMouseButtonDown
                && playerManager.PlayerController.HasMoveInput()
                && (!inputManager.rightMouseButtonClickedOverUI || (namePlateManager != null ? namePlateManager.MouseOverNamePlate() : false))) {

                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.SwimMovement(): camera Angle: " + cameraManager.MainCameraGameObject.transform.localEulerAngles.x);

                // prevent constant bouncing out of water using right mouse
                // always allow downward motion
                // only allow upward motion if the swim speed would not result in a bounce
                float cameraAngle = (cameraManager.MainCamera.transform.localEulerAngles.x < 180f ? cameraManager.MainCamera.transform.localEulerAngles.x : cameraManager.MainCamera.transform.localEulerAngles.x - 360f);
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.SwimMovement(): camera Angle: " + cameraAngle);
                // ignore angle if already touching ground underwater to prevent hitting bottom and stopping while trying to swim forward
                if ((cameraAngle > 0f && returnValue.z > 0f && !touchingGround) // camera above and moving forward / down
                    || (cameraAngle > 0f && returnValue.z < 0f && chestBelowWater == true) // camera above and moving back / up
                    || (cameraAngle < 0f && returnValue.z < 0f && !touchingGround) // camera below and moving back / down
                    || (cameraAngle < 0f && returnValue.z > 0f && chestBelowWater == true) // camera below and forward / up
                    ) {
                    //Debug.Log(gameObject.name + ".PlayerUnitMovementController.SwimMovement(): camera Angle: " + cameraAngle + "; direction: " + returnValue.z +
                    //    "; chest height: " + (playerManager.ActiveUnitController.transform.position.y + playerManager.ActiveUnitController.ChestHeight) + "; surface: " + playerManager.ActiveUnitController.CurrentWater[0].SurfaceHeight + "; speed: " + (playerManager.ActiveUnitController.SwimSpeed * Time.deltaTime));
                    returnValue = Quaternion.AngleAxis(cameraManager.MainCamera.transform.localEulerAngles.x, Vector3.right) * returnValue;
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

            GetLocalInput();
            Vector3 returnValue = localInput;

            if (inputManager.rightMouseButtonDown
                && playerManager.PlayerController.HasMoveInput()
                && (!inputManager.rightMouseButtonClickedOverUI || (namePlateManager != null ? namePlateManager.MouseOverNamePlate() : false))) {

                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.SwimMovement(): camera Angle: " + cameraManager.MainCameraGameObject.transform.localEulerAngles.x);

                // prevent constant bouncing out of water using right mouse
                // always allow downward motion
                // only allow upward motion if the swim speed would not result in a bounce
                float cameraAngle = (cameraManager.MainCamera.transform.localEulerAngles.x < 180f ? cameraManager.MainCamera.transform.localEulerAngles.x : cameraManager.MainCamera.transform.localEulerAngles.x - 360f);
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.SwimMovement(): camera Angle: " + cameraAngle);
                // ignore angle if already touching ground underwater to prevent hitting bottom and stopping while trying to swim forward
                if ((cameraAngle > 0f && returnValue.z > 0f && !touchingGround) // camera above and moving forward / down
                    || (cameraAngle < 0f && returnValue.z < 0f && !touchingGround) // camera below and moving back / down
                    ) {
                    //Debug.Log(gameObject.name + ".PlayerUnitMovementController.SwimMovement(): camera Angle: " + cameraAngle + "; direction: " + returnValue.z +
                    //    "; chest height: " + (playerManager.ActiveUnitController.transform.position.y + playerManager.ActiveUnitController.ChestHeight) + "; surface: " + playerManager.ActiveUnitController.CurrentWater[0].SurfaceHeight + "; speed: " + (playerManager.ActiveUnitController.SwimSpeed * Time.deltaTime));
                    returnValue = Quaternion.AngleAxis(cameraManager.MainCamera.transform.localEulerAngles.x, Vector3.right) * returnValue;
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

        private Vector3 NormalizedLocalMovement(float calculatedSpeed, Vector3 directionOfTravel) {
            //Debug.Log("PlayerUnitMovementController.NormalizedLocalMovement(" + directionOfTravel + ")");

            Vector3 newReturnValue;
            float usedAngle = groundAngle;
            
            // the normal is the normal of the ground below the player
            Vector3 localGroundNormal = playerManager.ActiveUnitController.transform.InverseTransformDirection(groundNormal);
            //Vector3 usedGroundNormal = groundNormal;

            if (Vector3.Angle(groundNormal, Vector3.up) > slopeLimit) {
                // if standing on jagged ground below stair height at angle greater than slope limit, prevent getting stuck due to capsule collider geometry
                localGroundNormal = Vector3.up;
                //usedGroundNormal = Vector3.up;
            }

            // the player is near a front obstacle, and that obstacle is below the slope limit, use its normal
            
            //Debug.Log("nearBottomFrontObstacle: " + nearBottomFrontObstacle +
                //"; angle: " + bottomFrontObstacleAngle + "; nearTopFrontObstacle: " + nearTopFrontObstacle + "; nearBottomStairs: " + nearBottomStairs + "; nearFrontObstacle: " + nearFrontObstacle);
               
            if (nearBottomFrontObstacle &&
                ((bottomFrontAngleDifferent && bottomFrontObstacleAngle < slopeLimit && nearFrontObstacle == true)
                || (nearTopFrontObstacle == false && nearBottomStairs == false && nearFrontObstacle == true))
                ) {
                localGroundNormal = playerManager.ActiveUnitController.transform.InverseTransformDirection(bottomForwardHitInfo.normal);
                //Debug.Log("using bottomForwardHitInfo.normal");
                //usedGroundNormal = bottomForwardHitInfo.normal;
                //Debug.Break();
            } else {
                //Debug.Log("nearBottomFrontObstacle: " + nearBottomFrontObstacle + "; bottomFrontAngleDifferent: " + bottomFrontAngleDifferent + "; neartopFrontObstacle: " + nearTopFrontObstacle);
                // the player is near stairs in the direction of travel
                //if (RaycastForStairs(playerManager.ActiveUnitController.transform.TransformDirection(normalizedInput), 0.5f)) {
                if (nearBottomStairs) {
                    //Debug.Log("near bottom stairs, adjusting forward direction");
                    //localGroundNormal = playerManager.ActiveUnitController.transform.InverseTransformDirection(forwardHitInfo.normal);

                    // 0.2f is an arbitrary distance at the top of the stair is below the start of the curve on the bottom of a capsule collider of 2m height
                    // if the stairs are higher than 0.3f (the start of the vertical section on the collider) and the player is too close to the stair,
                    // any angled approach will lose all momentum from running straight into the stair and the player will get stuck
                    // this value seems to be momentum dependent.  at walking speed of 1, player will not make it over stairs unless it's 0.15f
                    // at 0.15f there is a noticeable slowdown still, and even at 0.1f.  0.05f seems to do it at that speed

                    //if (stairDownHitPoint.y - playerManager.ActiveUnitController.transform.position.y < 0.2f
                    if (//nearFrontObstacle == false && // if you are touching an obstacle in front, the stair ramp angle should not be used
                        (
                        bottomStairDownHitInfo.point.y - playerManager.ActiveUnitController.transform.position.y < 0.2f
                        || (playerManager.ActiveUnitController.transform.InverseTransformPoint(new Vector3(forwardHitPoint.x, playerManager.ActiveUnitController.transform.position.y, forwardHitPoint.z)).magnitude > (colliderRadius + 0.01f) && nearFrontObstacle == false)
                        )
                        ) {
                        localGroundNormal = playerManager.ActiveUnitController.transform.InverseTransformDirection(stairRampNormal);
                        //usedGroundNormal = stairRampNormal;
                        //Debug.Log("using stair ramp normal: " + localGroundNormal + "; height: " + (stairDownHitPoint.y - playerManager.ActiveUnitController.transform.position.y));
                        //Debug.Break();
                    } else {
                        //Debug.Log("distance from wall: " + playerManager.ActiveUnitController.transform.InverseTransformPoint(forwardHitInfo.point).magnitude);
                        localGroundNormal = playerManager.ActiveUnitController.transform.InverseTransformDirection(bottomForwardHitInfo.normal);
                        //usedGroundNormal = bottomForwardHitInfo.normal;

                        //Debug.Log("using front normal: " + localGroundNormal + "; nearFrontObstacle: " + nearFrontObstacle);
                        //Debug.Break();
                        
                    }
                }
            }

            // to prevent odd floating point issues, set any ground normal that is up to directly up
            if (Mathf.Approximately(localGroundNormal.y, 1f)) {
                localGroundNormal = Vector3.up;
                //Debug.Log("localGroundNormal is now vector3.up");
            } else {
                //Debug.Break();
            }

            // translate the input so that the up direction is the same as the normal (up direction) of whatever ground or slope the player is on
            // this prevents losing speed up hills from slamming horizontally into the hill

            // WORKING VALUE
            //if (controlsManager.GamePadModeActive) {
                //Vector3 cameraInput = Quaternion.Euler(0f, cameraManager.ActiveMainCamera.transform.rotation.eulerAngles.y, 0f) * normalizedInput;
                //Vector3 newNormalizedInput = playerManager.ActiveUnitController.transform.InverseTransformDirection(cameraInput);
                //Debug.Log("normalizedInput: " + normalizedInput + "; cameraInput: "+ cameraInput + "; newNormalizedInput: " + newNormalizedInput);
                //newReturnValue = Vector3.Cross(Quaternion.LookRotation(normalizedInput, Vector3.up) * cameraManager.ActiveMainCamera.transform.InverseTransformDirection(cameraManager.ActiveMainCamera.transform.right), localGroundNormal);
                //newReturnValue = Vector3.Cross(Quaternion.LookRotation(newNormalizedInput, Vector3.up) * playerManager.ActiveUnitController.transform.InverseTransformDirection(playerManager.ActiveUnitController.transform.right), localGroundNormal);
            //} else {
                newReturnValue = Vector3.Cross(Quaternion.LookRotation(localInput, Vector3.up) * playerManager.ActiveUnitController.transform.InverseTransformDirection(playerManager.ActiveUnitController.transform.right), localGroundNormal);
            //}
            //Debug.Log("newReturnValue: " + newReturnValue);
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
            if (nearBottomStairs) {
                //Debug.Log("unclamped returnValue: " + newReturnValue.y + "; deltaTime: " + Time.deltaTime + "; fixedDeltaTime: " + Time.fixedDeltaTime);
                float clampedReturnValue = Mathf.Clamp(newReturnValue.y, 0f, playerManager.ActiveUnitController.transform.InverseTransformPoint(stairDownHitPoint).y / calculatedSpeed / Time.fixedDeltaTime);
                //float clampedReturnValue = Mathf.Clamp(newReturnValue.y, 0f, playerManager.ActiveUnitController.transform.InverseTransformPoint(bottomStairDownHitInfo.point).y / calculatedSpeed / Time.fixedDeltaTime);
                //Debug.Log("clamped returnValue: " + clampedReturnValue + "; unclamped: " + newReturnValue.y + "; deltaTime: " + Time.deltaTime + " distanceToGround: " + closestGroundDistance + " stairHeight: " + playerManager.ActiveUnitController.transform.InverseTransformPoint(bottomStairDownHitInfo.point).y);
                newReturnValue.y = clampedReturnValue;
            }

            // apply downforce
            if (groundAngle == 0 && nearBottomFrontObstacle == false && nearBottomStairs == false && touchingGround == false) {
                //Debug.Log("apply downforce");
                // this should make the character stick to the ground better when actively moving while grounded
                // ONLY APPLY Y DOWNFORCE ON FLAT GROUND, this will apply a y downforce multiplied by speed, not the existing y downforce from physics (gravity)
                float yValue = 0f;
                if (playerManager.ActiveUnitController.transform.InverseTransformPoint(groundPoint).y < -0.001f) {
                    // set a downforce value that should take the character exactly to the ground, and not lower to avoid losing momentum from physics colission with ground
                    yValue = Mathf.Clamp(1, 0, -closestGroundDistance / calculatedSpeed / Time.fixedDeltaTime) * -1;
                    //yValue = -1;
                    /*
                    Debug.Log("NormalizedLocalMovement() position.y: " + playerManager.ActiveUnitController.transform.position.y +
                        "; Applying extra down force: " + yValue +
                        "; ground distance: " + closestGroundDistance);
                      */  
                    //Debug.Break();
                    
                }
                newReturnValue = new Vector3(newReturnValue.x, yValue, newReturnValue.z);
            }
            //Debug.Log("newReturnValue: (" + newReturnValue.x + ", " + newReturnValue.y + ", " + newReturnValue.z + ") localGroundNormal: (" + localGroundNormal.x + ", " + localGroundNormal.y + ", " + localGroundNormal.z + ") nearBottomStairs: " + nearBottomStairs + "; closestGroundDistance: " + closestGroundDistance);
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

            //closestGroundDistance = 0f;
            closestGroundDistance = -stepHeight - 0.01f; // reset closest ground distance to something below step height so player is not considered to be touching by default
            closestWalkableGroundDistance = -stepHeight - 0.01f; // reset closest ground distance to something below step height so player is not considered to be touching by default
            touchingSlope = false;
            touchingGround = false;
            rayCastForGroundRun = false;
            closeToGround = false;

            // set an inital ground distance based on a direct downward raycast from the center of the player
            // later, a more accurate search will be done to see if there is a closer ground distance at the edges of the player capsule
            if (Physics.Raycast(playerManager.ActiveUnitController.transform.position + (Vector3.up * 0.25f), -Vector3.up, out centerDownHitInfo, Mathf.Infinity, groundMask)) {
                groundNormal = centerDownHitInfo.normal;
                groundPoint = centerDownHitInfo.point;
                if (Vector3.Angle(Vector3.up, centerDownHitInfo.normal) <= slopeLimit) {
                    closestGroundDistance = playerManager.ActiveUnitController.transform.InverseTransformPoint(centerDownHitInfo.point).y;
                    closestWalkableGroundDistance = playerManager.ActiveUnitController.transform.InverseTransformPoint(centerDownHitInfo.point).y;
                    if ((playerManager.ActiveUnitController.transform.InverseTransformPoint(centerDownHitInfo.point).y * -1) <= stepHeight ) {
                        closeToGround = true;
                    }
                    if ((playerManager.ActiveUnitController.transform.InverseTransformPoint(centerDownHitInfo.point).y * -1) <= touchingGroundHeight) {
                        touchingGround = true;
                    }
                }
            }

            if (touchingGround == false) {
                if (RaycastForGround()) {
                    touchingGround = true;
                }
            }

            /*
            // downward cast for close to ground
            if (Physics.Raycast(playerManager.ActiveUnitController.transform.position + (Vector3.up * 0.25f), -Vector3.up, out downHitInfo, (closeToGroundHeight + 0.25f), groundMask)) {
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CheckGround(): grounded is true");
                closeToGround = true;
            } else {
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CheckGround(): grounded is false");
                closeToGround = false;
            }
            */

            /*
            // downward cast for touching ground
            if (Physics.Raycast(playerManager.ActiveUnitController.transform.position + (Vector3.up * 0.25f), -Vector3.up, out downHitInfo, (touchingGroundHeight + 0.25f), groundMask)) {
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CheckGround(): grounded is true");
                touchingGround = true;
            } else {
                //Debug.Log(gameObject.name + ".PlayerUnitMovementController.CheckGround(): grounded is false");
                if (RaycastForGround()) {
                    touchingGround = true;
                }
            }
            */

            groundAngle = Vector3.Angle(groundNormal, Vector3.up);
            //Debug.Log("rawGroundAngle: " + rawGroundAngle);

            // this is necessary in case the player is moving fast and went off a cliff and we want to apply downforce
            // also needed in case of moving up stairs that are higher than 0.25f (the close to ground height)
            Collider[] hitColliders = Physics.OverlapBox(playerManager.ActiveUnitController.transform.position, maintainingGroundExtents, playerManager.ActiveUnitController.transform.rotation, groundMask);
            if (hitColliders.Length > 0) {
                closeToGround = true;
            }
            
        }

        private bool NearBottomStair(Vector3 directionOfTravel) {
            Vector3 bottomFrontStairHeight = Vector3.zero;
            if (nearBottomFrontObstacle) {
                bottomFrontObstacleAngle = Vector3.Angle(bottomForwardHitInfo.normal, Vector3.up);
                //Debug.Log("front obstacle angle: " + frontObstacleAngle);

                // we could be going up a ramp, determine if the obstacle in front has a different angle than the ground below us
                if (bottomForwardHitInfo.normal != groundNormal) {
                    //Debug.Log("front obstacle angle is different than ground angle: " + rawGroundAngle);
                    bottomFrontAngleDifferent = true;
                }

                // check if the obstacle is stairs
                if (bottomFrontAngleDifferent == true && bottomFrontObstacleAngle > slopeLimit) {
                    Vector3 raycastPoint = bottomForwardHitInfo.point + (directionOfTravel * 0.01f);
                    raycastPoint = new Vector3(raycastPoint.x, playerManager.ActiveUnitController.transform.position.y + stepHeight + 0.001f, raycastPoint.z);
                    //Debug.Log("CheckFrontObstacle() front Angle Different and frontObstacle > slopeLimit; localMoveVelocity: " + localMoveVelocity + "; directionOfTravel: " + directionOfTravel + "; forwardHitInfo: " + forwardHitInfo.point + "; player: " + playerManager.ActiveUnitController.transform.position + "; raycastpoint: " + raycastPoint);
                    Debug.DrawLine(raycastPoint, new Vector3(raycastPoint.x, raycastPoint.y - stepHeight - 0.001f, raycastPoint.z), Color.cyan);
                    if (Physics.Raycast(raycastPoint, Vector3.down, out bottomStairDownHitInfo, stepHeight, groundMask)) {
                        // we hit something that is low enough to step on, if it is below the slope limit, we can consider it to be a stair step
                        if (Vector3.Angle(bottomStairDownHitInfo.normal, Vector3.up) < slopeLimit) {
                            bottomFrontStairHeight = bottomStairDownHitInfo.point;
                            /*
                            Debug.Log("CheckFrontObstacle(): y position: " + playerManager.ActiveUnitController.transform.position.y +
                                "; stairs detected angle: " + Vector3.Angle(stairDownHitInfo.normal, Vector3.up) +
                                "; stairHeight: " + "(" + stairHeight.x + ", " + stairHeight.y + ", " + stairHeight.z + ")" +
                                "; object: " + stairDownHitInfo.collider.gameObject.name);
                                */
                            nearBottomStairs = true;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool NearTopStair(Vector3 directionOfTravel) {
            Vector3 topFrontStairHeight = Vector3.zero;
            if (nearTopFrontObstacle) {
                topFrontObstacleAngle = Vector3.Angle(topForwardHitInfo.normal, Vector3.up);
                //Debug.Log("front obstacle angle: " + frontObstacleAngle);

                // we could be going up a ramp, determine if the obstacle in front has a different angle than the ground below us
                if (topForwardHitInfo.normal != groundNormal) {
                    //Debug.Log("front obstacle angle is different than ground angle: " + rawGroundAngle);
                    topFrontAngleDifferent = true;
                }

                // check if the obstacle is stairs
                if (topFrontAngleDifferent == true && topFrontObstacleAngle > slopeLimit) {
                    Vector3 raycastPoint = topForwardHitInfo.point + (directionOfTravel * 0.01f);
                    raycastPoint = new Vector3(raycastPoint.x, bottomStairDownHitInfo.point.y + stepHeight + 0.001f, raycastPoint.z);
                    //Debug.Log("CheckFrontObstacle() front Angle Different and frontObstacle > slopeLimit; localMoveVelocity: " + localMoveVelocity + "; directionOfTravel: " + directionOfTravel + "; forwardHitInfo: " + forwardHitInfo.point + "; player: " + playerManager.ActiveUnitController.transform.position + "; raycastpoint: " + raycastPoint);
                    Debug.DrawLine(raycastPoint, new Vector3(raycastPoint.x, raycastPoint.y - stepHeight - 0.001f, raycastPoint.z), Color.cyan);
                    if (Physics.Raycast(raycastPoint, Vector3.down, out topStairDownHitInfo, stepHeight, groundMask)) {
                        // we hit something that is low enough to step on, if it is below the slope limit, we can consider it to be a stair step
                        if (Vector3.Angle(topStairDownHitInfo.normal, Vector3.up) < slopeLimit) {
                            topFrontStairHeight = topStairDownHitInfo.point;
                            /*
                            Debug.Log("CheckFrontObstacle(): y position: " + playerManager.ActiveUnitController.transform.position.y +
                                "; stairs detected angle: " + Vector3.Angle(stairDownHitInfo.normal, Vector3.up) +
                                "; stairHeight: " + "(" + stairHeight.x + ", " + stairHeight.y + ", " + stairHeight.z + ")" +
                                "; object: " + stairDownHitInfo.collider.gameObject.name);
                                */
                            nearTopStairs = true;
                            /*
                            // determine exact corner of highest stair
                            Vector3 exactTopOriginPoint = new Vector3(topOriginPoint.x, topStairDownHitInfo.point.y - 0.001f, topOriginPoint.z);
                            Debug.DrawLine(exactTopOriginPoint, exactTopOriginPoint + directionOfTravel, Color.cyan);
                            Physics.Raycast(exactTopOriginPoint, directionOfTravel, out topStairForwardHitInfo, detectionDistance, groundMask);
                            */
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// This function actually detects the lack of a front obstacle and works in conjunction with nearBottomFrontObstacle to determine
        /// if the bottom front obstacle is less than the step height from the ground
        /// </summary>
        /// <param name="directionOfTravel"></param>
        /// <param name="detectionDistance"></param>
        private void PerformLowObstacleCasts(Vector3 directionOfTravel, float detectionDistance) {
            //Debug.Log("PlayerUnitMovementController.PerformLowObstacleCasts()");

            int validResultCount = 0;

            losOriginPoint = playerManager.ActiveUnitController.transform.TransformPoint(Vector3.up * (closestWalkableGroundDistance + stepHeight + 0.001f));

            lowObstacleOriginPoint = playerManager.ActiveUnitController.transform.TransformPoint(Quaternion.LookRotation(playerManager.ActiveUnitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(0f, closestWalkableGroundDistance + stepHeight + 0.001f, 0f));
            // raycast from center in direction of travel
            Debug.DrawLine(lowObstacleOriginPoint,
                lowObstacleOriginPoint + (directionOfTravel * (detectionDistance + colliderRadius)),
                Color.black);

            if (Physics.Raycast(lowObstacleOriginPoint, directionOfTravel, (detectionDistance + colliderRadius), groundMask) == true) {
                return;
            } else {
                // check if origin point is on the other side of an obstacle
                // disable if statement because this is the bottom center of capsule and no need to raycast since los origin and bottom origin are identical
                //if (Physics.Raycast(losOriginPoint, bottomOriginPoint - losOriginPoint, Vector3.Magnitude(bottomOriginPoint - losOriginPoint), groundMask) == true) {
                    // near an obstacle in front center
                    //Debug.Log("near a low obstacle in front center");
                    validResultCount++;
                    //return;
                //}

            }

            lowObstacleOriginPoint = playerManager.ActiveUnitController.transform.TransformPoint(Quaternion.LookRotation(playerManager.ActiveUnitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(-colliderRadius, closestWalkableGroundDistance + stepHeight + 0.001f, 0f));
            // raycast from left in direction of travel
            Debug.DrawLine(lowObstacleOriginPoint,
                lowObstacleOriginPoint + (directionOfTravel * detectionDistance),
                Color.black);
            if (Physics.Raycast(lowObstacleOriginPoint, directionOfTravel, detectionDistance, groundMask) == true) {
                return;
            } else {
                // check if origin point is on the other side of an obstacle
                if (Physics.Raycast(losOriginPoint, lowObstacleOriginPoint - losOriginPoint, Vector3.Magnitude(lowObstacleOriginPoint - losOriginPoint), groundMask) == false) {
                    // near an obstacle in front left
                    //Debug.Log("near a low obstacle in front left");
                    validResultCount++;
                    //return;
                }

            }


            lowObstacleOriginPoint = playerManager.ActiveUnitController.transform.TransformPoint(Quaternion.LookRotation(playerManager.ActiveUnitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(colliderRadius, closestWalkableGroundDistance + stepHeight + 0.001f, 0f));
            // raycast from right in direction of travel
            Debug.DrawLine(lowObstacleOriginPoint,
                lowObstacleOriginPoint + (directionOfTravel * detectionDistance),
                Color.black);
            if (Physics.Raycast(lowObstacleOriginPoint, directionOfTravel, detectionDistance, groundMask) == true) {
                return;
            } else {
                // check if origin point is on the other side of an obstacle
                if (Physics.Raycast(losOriginPoint, lowObstacleOriginPoint - losOriginPoint, Vector3.Magnitude(lowObstacleOriginPoint - losOriginPoint), groundMask) == false) {
                    // near an obstacle in front right
                    //Debug.Log("near a low obstacle in front right");
                    validResultCount++;
                    //return;
                }

            }

            if (validResultCount == 3) {
                // if we made it this far without hitting anything, it's a low obstacle
                nearLowObstacle = true;
            }

        }

        private void PerformFrontObstacleCasts(Vector3 directionOfTravel, float detectionDistance, float groundOffset) {

            losOriginPoint = playerManager.ActiveUnitController.transform.TransformPoint(Vector3.up * groundOffset);

            bottomOriginPoint = playerManager.ActiveUnitController.transform.TransformPoint(Quaternion.LookRotation(playerManager.ActiveUnitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(0f, groundOffset, 0f));
            // raycast from center in direction of travel
            Debug.DrawLine(bottomOriginPoint,
                bottomOriginPoint + (directionOfTravel * (detectionDistance + colliderRadius)),
                Color.black);

            if (Physics.Raycast(bottomOriginPoint, directionOfTravel, out bottomForwardHitInfo, (detectionDistance + colliderRadius), groundMask)) {
                // check if origin point is on the other side of an obstacle
                //if (Physics.Raycast(losOriginPoint, bottomOriginPoint - losOriginPoint, out losHitInfo, Vector3.Magnitude(bottomOriginPoint - losOriginPoint), groundMask) == false) {
                    // near an obstacle in front center
                    //Debug.Log("near a bottom obstacle in front center");
                    nearBottomFrontObstacle = true;
                    // disabled if statement to allow for walking up high slopes below stair limit
                    if (NearBottomStair(directionOfTravel)) {
                        //NearBottomStair(directionOfTravel);
                        topOriginPoint = playerManager.ActiveUnitController.transform.TransformPoint(Quaternion.LookRotation(playerManager.ActiveUnitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(0f, playerManager.ActiveUnitController.transform.InverseTransformPoint(bottomStairDownHitInfo.point).y + 0.001f, 0f));

                        Debug.DrawLine(topOriginPoint,
                                                        topOriginPoint + (directionOfTravel * (detectionDistance + colliderRadius)),
                                                        Color.black);
                        if (Physics.Raycast(topOriginPoint, directionOfTravel, out topForwardHitInfo, (detectionDistance + colliderRadius), groundMask)) {
                            // near an obstacle in front center
                            //Debug.Log("near a top obstacle in front center");
                            nearTopFrontObstacle = true;
                            NearTopStair(directionOfTravel);
                        }
                    } else {
                        topOriginPoint = playerManager.ActiveUnitController.transform.TransformPoint(Quaternion.LookRotation(playerManager.ActiveUnitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(0f, stepHeight, 0f));

                        Debug.DrawLine(topOriginPoint,
                                                        topOriginPoint + (directionOfTravel * (detectionDistance + colliderRadius)),
                                                        Color.black);
                        if (Physics.Raycast(topOriginPoint, directionOfTravel, out topForwardHitInfo, (detectionDistance + colliderRadius), groundMask)) {
                            // near an obstacle in front center
                            //Debug.Log("near a top obstacle in front center");
                            nearTopFrontObstacle = true;
                        }

                    }
                    return;
                //}
            }

            bottomOriginPoint = playerManager.ActiveUnitController.transform.TransformPoint(Quaternion.LookRotation(playerManager.ActiveUnitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(-colliderRadius, groundOffset, 0f));
            // raycast from left in direction of travel
            Debug.DrawLine(bottomOriginPoint,
                bottomOriginPoint + (directionOfTravel * detectionDistance),
                Color.black);
            if (Physics.Raycast(bottomOriginPoint, directionOfTravel, out bottomForwardHitInfo, detectionDistance, groundMask)) {
                // check if origin point is on the other side of an obstacle
                if (Physics.Raycast(losOriginPoint, bottomOriginPoint - losOriginPoint, out losHitInfo, Vector3.Magnitude(bottomOriginPoint - losOriginPoint), groundMask) == false) {
                    // near an obstacle in front left
                    //Debug.Log("near a bottom obstacle in front left");
                    nearBottomFrontObstacle = true;
                    // disabled if statement to allow for walking up high slopes below stair limit
                    if (NearBottomStair(directionOfTravel)) {
                        //NearBottomStair(directionOfTravel);
                        topOriginPoint = playerManager.ActiveUnitController.transform.TransformPoint(Quaternion.LookRotation(playerManager.ActiveUnitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(-colliderRadius, playerManager.ActiveUnitController.transform.InverseTransformPoint(bottomStairDownHitInfo.point).y + 0.001f, 0f));
                        Debug.DrawLine(topOriginPoint,
                            topOriginPoint + (directionOfTravel * detectionDistance),
                            Color.black);
                        if (Physics.Raycast(topOriginPoint, directionOfTravel, out topForwardHitInfo, detectionDistance, groundMask)) {
                            // near an obstacle in front left
                            //Debug.Log("near a top obstacle in front left");
                            nearTopFrontObstacle = true;
                            NearTopStair(directionOfTravel);
                        }

                    } else {
                        topOriginPoint = playerManager.ActiveUnitController.transform.TransformPoint(Quaternion.LookRotation(playerManager.ActiveUnitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(-colliderRadius, stepHeight, 0f));
                        Debug.DrawLine(topOriginPoint,
                            topOriginPoint + (directionOfTravel * detectionDistance),
                            Color.black);
                        if (Physics.Raycast(topOriginPoint, directionOfTravel, out topForwardHitInfo, detectionDistance, groundMask)) {
                            // near an obstacle in front left
                            //Debug.Log("near a top obstacle in front left");
                            nearTopFrontObstacle = true;
                        }

                    }
                    return;
                }
            }


            bottomOriginPoint = playerManager.ActiveUnitController.transform.TransformPoint(Quaternion.LookRotation(playerManager.ActiveUnitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(colliderRadius, groundOffset, 0f));
            // raycast from right in direction of travel
            Debug.DrawLine(bottomOriginPoint,
                bottomOriginPoint + (directionOfTravel * detectionDistance),
                Color.black);
            if (Physics.Raycast(bottomOriginPoint, directionOfTravel, out bottomForwardHitInfo, detectionDistance, groundMask)) {
                // check if origin point is on the other side of an obstacle
                if (Physics.Raycast(losOriginPoint, bottomOriginPoint - losOriginPoint, out losHitInfo, Vector3.Magnitude(bottomOriginPoint - losOriginPoint), groundMask) == false) {
                    // near an obstacle in front right
                    //Debug.Log("near a bottom obstacle in front right");
                    nearBottomFrontObstacle = true;
                    // disabled if statement to allow for walking up high slopes below stair limit
                    if (NearBottomStair(directionOfTravel)) {
                        //NearBottomStair(directionOfTravel);
                        topOriginPoint = playerManager.ActiveUnitController.transform.TransformPoint(Quaternion.LookRotation(playerManager.ActiveUnitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(colliderRadius, playerManager.ActiveUnitController.transform.InverseTransformPoint(bottomStairDownHitInfo.point).y + 0.001f, 0f));
                        Debug.DrawLine(topOriginPoint,
                            topOriginPoint + (directionOfTravel * detectionDistance),
                            Color.black);
                        if (Physics.Raycast(topOriginPoint, directionOfTravel, out topForwardHitInfo, detectionDistance, groundMask)) {
                            // near an obstacle in front right
                            //Debug.Log("near a top obstacle in front right");
                            nearTopFrontObstacle = true;
                            NearTopStair(directionOfTravel);
                        }

                    } else {
                        topOriginPoint = playerManager.ActiveUnitController.transform.TransformPoint(Quaternion.LookRotation(playerManager.ActiveUnitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(colliderRadius, stepHeight, 0f));
                        Debug.DrawLine(topOriginPoint,
                            topOriginPoint + (directionOfTravel * detectionDistance),
                            Color.black);
                        if (Physics.Raycast(topOriginPoint, directionOfTravel, out topForwardHitInfo, detectionDistance, groundMask)) {
                            // near an obstacle in front right
                            //Debug.Log("near a top obstacle in front right");
                            nearTopFrontObstacle = true;
                        }

                    }
                    return;
                }
            }


        }

        /// <summary>
        /// check if the player is touching an obstacle that may be floating off the ground
        /// return the the height the forward raycast should be performed at
        /// </summary>
        /// <param name="directionOfTravel"></param>
        /// <returns></returns>
        private float GetFrontObstacleCastHeight(Vector3 directionOfTravel) {
            forwardOriginPoint = playerManager.ActiveUnitController.transform.TransformPoint(Quaternion.LookRotation(playerManager.ActiveUnitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(0f, stepHeight + 0.001f, colliderRadius + 0.01f));
            Debug.DrawLine(forwardOriginPoint,
                forwardOriginPoint + (Vector3.down * stepHeight),
                Color.black);

            // perform a downward raycast from center just in front of collider to see if close to contacting any obstacle
            if (Physics.Raycast(forwardOriginPoint, Vector3.down, out obstacleCastHitInfo, stepHeight, groundMask)) {
                // if there was a hit, return the height 1mm below the height of the hit
                nearFrontObstacle = true;
                return playerManager.ActiveUnitController.transform.InverseTransformPoint(obstacleCastHitInfo.point).y - 0.001f;
            }

            // if there was no hit in the center, perform a cast on the left side of the character from just in front of the collider
            forwardOriginPoint = playerManager.ActiveUnitController.transform.TransformPoint(Quaternion.LookRotation(playerManager.ActiveUnitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(colliderRadius, stepHeight + 0.001f, colliderRadius + 0.01f));
            Debug.DrawLine(forwardOriginPoint,
                forwardOriginPoint + (Vector3.down * stepHeight),
                Color.black);
            if (Physics.Raycast(forwardOriginPoint, Vector3.down, out obstacleCastHitInfo, stepHeight, groundMask)) {
                // if there was a hit, return the height 1mm below the height of the hit
                nearFrontObstacle = true;
                return playerManager.ActiveUnitController.transform.InverseTransformPoint(obstacleCastHitInfo.point).y - 0.001f;
            }

            // if there was no hit on the left, perform a cast on the right side of the character just in front of the collider
            forwardOriginPoint = playerManager.ActiveUnitController.transform.TransformPoint(Quaternion.LookRotation(playerManager.ActiveUnitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(-colliderRadius, stepHeight + 0.001f, colliderRadius + 0.01f));
            Debug.DrawLine(forwardOriginPoint,
                forwardOriginPoint + (Vector3.down * stepHeight),
                Color.black);
            if (Physics.Raycast(forwardOriginPoint, Vector3.down, out obstacleCastHitInfo, stepHeight, groundMask)) {
                // if there was a hit, return the height 1mm below the height of the hit
                nearFrontObstacle = true;
                return playerManager.ActiveUnitController.transform.InverseTransformPoint(obstacleCastHitInfo.point).y - 0.001f;
            }



            // the player is not close to touching any obstacle, return the default height
            return 0.001f;
        }

        private void CheckFrontObstacle(float calculatedSpeed, Vector3 directionOfTravel) {
            // reset variables
            nearFrontObstacle = false;
            nearBottomFrontObstacle = false;
            nearTopFrontObstacle = false;
            nearLowObstacle = false;
            bottomFrontAngleDifferent = false;
            nearBottomStairs = false;
            nearTopStairs = false;
            float detectionDistance = stairDetectionDistance + (calculatedSpeed * Time.fixedDeltaTime);
            //float detectionDistance = stairDetectionDistance;


            //PerformFrontObstacleCasts(directionOfTravel, detectionDistance, 0.001f);
            PerformFrontObstacleCasts(directionOfTravel, detectionDistance, GetFrontObstacleCastHeight(directionOfTravel));

            // it is possible that an obstacle in front starts above the ground
            // if no obstacle was detected at the ground, perform a downward raycast from slightly in front of the character to determine a better height to check from
            if (nearBottomFrontObstacle == false) {
                //Vector3 highObstacleOrigin = playerManager.ActiveUnitController.transform.position + (Vector3.up * (stepHeight + 0.001f)) + (directionOfTravel * (colliderRadius + 0.001f));
                //Debug.DrawLine(highObstacleOrigin, highObstacleOrigin - (Vector3.up * stepHeight), Color.cyan);
                //if (Physics.Raycast(highObstacleOrigin, Vector3.down, out bottomStairDownHitInfo, stepHeight, groundMask)) {
                //PerformFrontObstacleCasts(directionOfTravel, detectionDistance, playerManager.ActiveUnitController.transform.InverseTransformPoint(bottomStairDownHitInfo.point).y - 0.001f);
                if (rayCastForGroundRun == false) {
                    RaycastForGround();
                }
                if (highestWalkableGroundDistance > 0.001f) {
                    //Debug.Log("not near lower front obstacle.  checking at " + highestWalkableGroundDistance);
                    PerformFrontObstacleCasts(directionOfTravel, detectionDistance, highestWalkableGroundDistance - 0.001f);
                }
                //}
            } else {
                if (touchingGround == true || (closestWalkableGroundDistance * -1) < stepHeight) {
                    // near bottom front obstacle is true, check for low obstacle (below stair height)
                    PerformLowObstacleCasts(directionOfTravel, detectionDistance);
                }
            }


            if (nearBottomStairs || nearTopStairs) {

                // new code to detect stairs from greater distance and make angle upward at a more gradual slope to prevent the jittery updward movement that comes from using
                // the completely horizontal normal you get from striking the front of the stairs
                Vector3 bottomPoint = Vector3.zero;
                Vector3 topStairCorner;
                Vector3 bottomStairCorner;
                Vector3 bottomAngleRay = Vector3.zero;
                Vector3 topAngleRay = Vector3.zero;
                Vector3 usedAngleRay = Vector3.zero;
                //Debug.Log("nearStairs: " + nearStairs + "; nearFrontHighStairs: " + nearFrontHighStair + "; frontStairHeight.y: " + frontStairHeight.y + "; highestDownPoint.y: " + highestDownPoint.y);
                if (nearBottomStairs == true) {
                    bottomPoint = new Vector3(bottomOriginPoint.x, playerManager.ActiveUnitController.transform.position.y, bottomOriginPoint.z);
                    forwardHitPoint = bottomForwardHitInfo.point;
                    stairDownHitPoint = bottomStairDownHitInfo.point;

                    bottomStairCorner = new Vector3(bottomForwardHitInfo.point.x, bottomStairDownHitInfo.point.y, bottomForwardHitInfo.point.z);
                    bottomAngleRay = bottomStairCorner - bottomPoint;
                    Debug.DrawLine(bottomPoint, bottomPoint + bottomAngleRay, Color.cyan);

                    if (nearTopStairs == true && bottomStairDownHitInfo.point.y < topStairDownHitInfo.point.y) {
                        stairDownHitPoint = topStairDownHitInfo.point;
                        topStairCorner = new Vector3(topForwardHitInfo.point.x, topStairDownHitInfo.point.y, topForwardHitInfo.point.z);
                        //Debug.Log("topForwardHitInfo: " + topForwardHitInfo.point + "; topStairDownHitInfo: " + topStairDownHitInfo.point);
                        topAngleRay = topStairCorner - topOriginPoint;

                        Debug.DrawLine(topOriginPoint, topOriginPoint + topAngleRay, Color.cyan);
                        //Debug.Log("using high stair values; highForwardHitInfo: " + highForwardHitInfo.point + "; bottomPoint: " + bottomPoint + "; angleRay: " + angleRay);
                        bottomPoint = bottomStairCorner;
                        usedAngleRay = topStairCorner - bottomStairCorner;
                    } else {
                        usedAngleRay = bottomAngleRay;
                    }
                }

                Debug.DrawLine(bottomPoint,
                    bottomPoint + usedAngleRay,
                    Color.cyan);

                Vector3 secondPoint = bottomPoint + (Quaternion.AngleAxis(90f, Vector3.up) * usedAngleRay);
                secondPoint = new Vector3(secondPoint.x, bottomPoint.y, secondPoint.z);
                Debug.DrawLine(bottomPoint,
                    secondPoint,
                    Color.red);
                Vector3 calculatedNormal = Vector3.Cross(usedAngleRay, secondPoint - bottomPoint).normalized;
                Debug.DrawLine(bottomPoint,
                    bottomPoint + calculatedNormal,
                    Color.red);
                //Debug.Log("CheckFrontObstacle() calculatedNormal: " + calculatedNormal + "; angleRay: " + angleRay + "; line2: " + (secondPoint - bottomPoint) + "; angle: " + Vector3.Angle(angleRay.normalized, Vector3.up));
                //Debug.Log("CheckFrontObstacle() angle: " + Vector3.Angle(usedAngleRay.normalized, Vector3.up));
                stairRampNormal = calculatedNormal;
            }

        }

        /*
        private void ApplyGravity() {
            if (!closeToGround) {
                //Debug.Log("PlayerUnitMovementController.ApplyGravity(): Not Grounded");
            }
        }
        */

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