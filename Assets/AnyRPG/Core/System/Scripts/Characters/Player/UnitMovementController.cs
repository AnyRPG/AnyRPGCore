using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public enum CharacterMovementState {
        Idle = 0,
        Move = 1,
        Jump = 2,
        Knockback = 3,
        Fall = 4,
        Roll = 5,
        Swim = 6,
        Fly = 7,
        Glide = 8,
        NavMesh = 9,
        Riding = 10
    }

    public class UnitMovementController : ConfiguredClass {

        private UnitController unitController = null;

        public bool useMeshNav = false;
        public Vector3 lookDirection { get; private set; }

        private IMovementState currentIMovementState;
        private CharacterMovementState currentCharacterMovementState;
        private Dictionary<CharacterMovementState, IMovementState> movementStates = new Dictionary<CharacterMovementState, IMovementState>();

        MovementData accumulatedMovementData = new MovementData();
        MovementData currentMovementData = new MovementData();
        MovementData cachedMovementData = new MovementData();

        private Vector3 reconciledNavMeshAgentVelocity = Vector3.zero;

        //Jumping.
        //public bool canJump;
        public float gravity = 50.0f;
        public float jumpAcceleration = 5.0f;
        public float jumpHeight = 3.0f;

        public float MaxMovementSpeed { get => systemConfigurationManager.MaxMovementSpeed; }

        // travel vector from the perspective of the character
        public Vector3 intendedLocalMoveVelocity;

        // travel vector from the perspective of the world
        public Vector3 intendedWorldMoveVelocity;

        // the movement input in relation to the character, without speed adjustment
        //public Vector3 localInput;

        // travel vector rotated by ground angle from the perspective of the character
        public Vector3 adjustedLocalMoveVelocity;
        public Vector3 adjustedWorldMoveVelocity;
        public Vector3 currentTurnVelocity;

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
        public float groundAngle;
        
        // the raw angle of the ground below
        private float slopeAngle;

        // used for determining how far above the ground the player is for applying downforce
        private float closestGroundDistance = 0f;

        public float closestWalkableGroundDistance = 0f;

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
        public bool nearBottomFrontObstacle = false;

        // is there an obstacle close in front of us that is less than stepheight from the closest ground
        public bool nearLowObstacle = false;

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
        public bool touchingGround;
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
        public float currentFallDistance = 0f;
        public float fallStartHeight = 0f;

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
        private Quaternion airRotation;

        // the frame in which the player last entered a jump state
        public uint lastJumpFrame;
        public uint lastKnockbackFrame;

        // game manager references
        protected CameraManager cameraManager = null;
        protected ControlsManager controlsManager = null;
        //protected WindowManager windowManager = null;

        public UnitController UnitController { get => unitController; }
        public MovementData AccumulatedMovementData { get => accumulatedMovementData; set => accumulatedMovementData = value; }
        public MovementData CurrentMovementData { get => currentMovementData; set => currentMovementData = value; }
        public CharacterMovementState CurrentCharacterMovementState { get => currentCharacterMovementState; }
        public Vector3 ReconciledNavMeshAgentVelocity { get => reconciledNavMeshAgentVelocity; set => reconciledNavMeshAgentVelocity = value; }

        public UnitMovementController(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
        }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            rotationSpeed = systemConfigurationManager.MaxTurnSpeed;
            groundMask = systemConfigurationManager.DefaultGroundMask;

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
            cameraManager = systemGameManager.CameraManager;
            controlsManager = systemGameManager.ControlsManager;
        }

        public void Init() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMovementController.Init()");

            ConfigureStateMachine();
            if (unitController.UnitAnimator != null) {
                unitController.UnitAnimator.DisableRootMotion();
            }
        }

        public void ConfigureStateMachine() {
            //Debug.Log($"{unitController.gameObject.name}.PlayerUnitMovementController.ConfigureStateMachine()");
            // add a new state for each type of CharacterMovementState
            movementStates.Add(CharacterMovementState.Idle, new MovementIdleState(this, unitController));
            movementStates.Add(CharacterMovementState.Move, new MovementMoveState(this, unitController));
            movementStates.Add(CharacterMovementState.Jump, new MovementJumpState(this, unitController));
            movementStates.Add(CharacterMovementState.Knockback, new MovementKnockbackState(this, unitController));
            movementStates.Add(CharacterMovementState.Fall, new MovementFallState(this, unitController));
            movementStates.Add(CharacterMovementState.Roll, new MovementRollState(this, unitController));
            movementStates.Add(CharacterMovementState.Swim, new MovementSwimState(this, unitController));
            movementStates.Add(CharacterMovementState.Fly, new MovementFlyState(this, unitController));
            movementStates.Add(CharacterMovementState.Glide, new MovementGlideState(this, unitController));
            movementStates.Add(CharacterMovementState.NavMesh, new MovementNavMeshState(this, unitController, systemGameManager));
            movementStates.Add(CharacterMovementState.Riding, new MovementRidingState(this, unitController));
            ChangeState(CharacterMovementState.Idle, false);
        }

        public void ChangeState(CharacterMovementState newState, bool isReplay) {
            if (movementStates.ContainsKey(newState)) {
                currentCharacterMovementState = newState;
                ChangeState(movementStates[newState], isReplay);
            } else {
                Debug.LogError($"UnitMovementController.ChangeState({newState.ToString()}): no state found in movementStates dictionary");
            }
        }

        private void ChangeState(IMovementState newState, bool isReplay) {
            //Debug.Log($"{gameObject.name}: ChangeState({newState.ToString()})");
            if (currentIMovementState != null) {
                currentIMovementState.Exit(isReplay, false);
            }
            currentIMovementState = newState;

            currentIMovementState.Enter(isReplay, false);
        }

        //Put any code in here you want to run BEFORE the state's update function. This is run regardless of what state you're in.
        protected void EarlyGlobalStateUpdate() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMovementController.EarlyGlobalStateUpdate()");

            CheckGround();

            DrawDebugLines();
        }

        public void MoveRelative() {
            Vector3 relativeMovement;
            //if (controlsManager.GamePadModeActive) {
                //relativeMovement = CameraRelativeInput(adjustedlocalMoveVelocity);
            //} else {
                relativeMovement = CharacterRelativeInput(adjustedLocalMoveVelocity);
            //}
            //Debug.Log($"{unitController.gameObject.name}.UnitMovementController.MoveRelative() adjustedLocalMoveVelocity: {adjustedlocalMoveVelocity} relativeMovement: ({relativeMovement.x}, {relativeMovement.y}, {relativeMovement.z})");
            if (relativeMovement.magnitude > 0.1 || currentMovementData.InputJump) {
                unitController.UnitMotor.Move(relativeMovement);
            }
        }

        public void MoveWorld() {
            // 1. We use the World-Space velocity calculated in the Update/Replicate loop.
            // This vector is stable because it was derived from CameraWantedDirection, 
            // not the character's jittery local transform.
            Vector3 worldMovement = adjustedWorldMoveVelocity;

            // 2. Magnitude check to prevent micro-movements/drifting.
            // We also allow movement if a Jump input is detected (to maintain air momentum).
            if (worldMovement.magnitude > 0.01f || currentMovementData.InputJump) {
                // 3. Send the World-Space vector directly to the Motor.
                // UnitMotor.Move then passes this straight to the Rigidbody.
                unitController.UnitMotor.Move(worldMovement);
            }
        }

        public void AnimatorMoveUpdate() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMovementController.AnimatorMoveUpdate() root: {unitController.UnitAnimator.applyRootMotion}");

            if (useMeshNav == true && unitController.NavMeshAgent.enabled == true) {
                return;
            }
            //If alive and is moving, set animator.
            if (unitController.CharacterStats.IsAlive == true) {

                // handle movement
                if (intendedLocalMoveVelocity.magnitude > 0 && currentMovementData.HasMoveInput()) {
                    //Debug.Log($"{unitController.gameObject.name}.PlayerUnitMovementController.LateGlobalSuperUpdate(): animator velocity: " + unitController.MyCharacterAnimator.MyAnimator.velocity + "; angular: " + unitController.MyCharacterAnimator.MyAnimator.angularVelocity);
                    if (currentMovementData.InputStrafe == true) {
                        unitController.UnitAnimator.SetStrafing(true);
                    } else {
                        unitController.UnitAnimator.SetStrafing(false);
                    }
                    unitController.UnitAnimator.SetMoving(true);
                    unitController.UnitAnimator.SetVelocityFromLocal(intendedLocalMoveVelocity);
                }/* else {
                    unitController.MyCharacterAnimator.SetMoving(false);
                    unitController.MyCharacterAnimator.SetStrafing(false);
                    unitController.MyCharacterAnimator.SetVelocity(currentMoveVelocity, rotateModel);
                }*/
                AnimatorTurnUpdate();
            }
        }

        public void AnimatorTurnUpdate() {
            if (unitController.UnitAnimator != null) {
                unitController.UnitAnimator.SetTurnVelocity(currentTurnVelocity.x);
            }
        }

        protected void LateGlobalStateUpdate(double timeInterval) {

            if (unitController.CharacterStats.IsAlive == true) {
                if ((currentMovementData.RightMouseDragged == true && unitController.UnitProfile.UnitPrefabProps.RotateModel == false)
                    || (currentMovementData.GamepadModeActive == false && currentMovementData.RightAnalogHorizontal != 0f)) {

                    unitController.UnitMotor.FaceDirection(new Vector3(currentMovementData.CameraWantedDirection.x, 0, currentMovementData.CameraWantedDirection.z));
                }

                if (currentMovementData.InputTurn != 0) {
                    unitController.UnitMotor.Rotate(new Vector3(0, currentTurnVelocity.x * (float)timeInterval, 0));
                }
            }

        }

        public void EnterGroundStateCommon(bool isReplay) {
            if (isReplay == false) {
                unitController.UnitAnimator.SetJumping(0);
            }
        }

        public bool CheckForSwimming() {
            if (unitController.InWater == true) {
                //Debug.Log($"{unitController.gameObject.name}.UnitMovementController.CheckForSwimming() IN WATER = TRUE; player chest height: {(unitController.UnitMotor.MovementBody.GetPosition().y + unitController.FloatHeight)}; water surface height: {unitController.CurrentWater[0].SurfaceHeight}");
                if ((unitController.UnitMotor.MovementBody.GetPosition().y + unitController.FloatHeight) <= unitController.CurrentWater[0].SurfaceHeight) {
                    return true;
                }
            } else {
                //Debug.Log($"{unitController.gameObject.name}.UnitMovementController.CheckForSwimming() called but not in water");
            }
                return false;
        }

        public void CalculateTurnVelocity() {
            if (currentMovementData.HasTurnInput()) {
                currentTurnVelocity = currentMovementData.TurnInput * PlayerPrefs.GetFloat("KeyboardTurnSpeed") * rotationSpeed;
            }
        }

        public void CalculateFallDamage(bool isReplay) {

            if (systemGameManager.GameMode == GameMode.Network && systemGameManager.NetworkManagerServer.ServerModeActive == false) {
                return;
            }

            if (isReplay == false) {
                if (useFallDamage && currentFallDistance > fallDamageMinDistance) {
                    unitController.CharacterStats.TakeFallDamage(currentFallDistance * fallDamagePerMeter);
                }
                currentFallDistance = 0f;
            }

        }

        public void KnockBack() {
            //Debug.Log("Knockback()");
            ChangeState(CharacterMovementState.Knockback, false);
        }

        public Vector3 LocalNormalizedGlideMovement(float calculatedSpeed) {
            // it's safe to check for touching ground here because although we should be gliding
            // we can still reach this block of code if we touch ground that is too sloped to walk on
            if (touchingSlope == true && (unitController.transform.InverseTransformPoint(slopePoint).z > 0f)) {

                return unitController.transform.InverseTransformDirection(slopeDirection * (unitController.GlideFallSpeed / calculatedSpeed));
            } else { 
                float glideGravity = -unitController.GlideFallSpeed / calculatedSpeed;
                Vector3 returnValue = new Vector3(0f, glideGravity, 1f);
                return returnValue;
            }
        }

        public Vector3 WorldNormalizedGlideMovement(float calculatedSpeed) {
            // 1. Get the current Physics Forward (Stable Reference)
            Quaternion physicsRot = unitController.UnitMotor.MovementBody.GetRotation();
            Vector3 characterForward = physicsRot * Vector3.forward;

            // 2. Handle Slope Interaction (Sliding down steep terrain)
            if (touchingSlope && unitController.transform.InverseTransformPoint(slopePoint).z > 0f) {
                // Use the world slope direction, scaled by the glide fall speed ratio
                return (slopeDirection * (unitController.GlideFallSpeed / calculatedSpeed)).normalized;
            } else {
                // 3. Standard Glide (Forward + Constant Sink)
                float glideGravityRatio = -unitController.GlideFallSpeed / calculatedSpeed;

                // Construct the world vector: (Character Forward) + (Downwards Sink)
                Vector3 worldGlideVector = characterForward + (Vector3.up * glideGravityRatio);

                return worldGlideVector.normalized;
            }
        }


        private Vector3 CharacterRelativeInput(Vector3 inputVector) {
            //Debug.Log($"{unitController.gameObject.name}.PlayerUnitMovementController.CharacterRelativeInput({inputVector})");

            return unitController.transform.TransformDirection(inputVector);
        }

        /*
        private Vector3 WorldRelativeInput(float inputX, float inputZ) {
            Vector3 relativeVelocity = new Vector3(inputX, 0, inputZ);
            return relativeVelocity;
        }

        /// <summary>
        /// World space movement based off camera facing.
        /// </summary>
        private Vector3 CameraRelativeInput(Vector3 inputVector) {
            //Debug.Log("UnitMovementController.CameraRelativeInput(" + inputVector + ")");
            //Forward vector relative to the camera
            
            return Quaternion.LookRotation(new Vector3(cameraManager.ActiveMainCamera.transform.forward.x, 0f, cameraManager.ActiveMainCamera.transform.forward.z).normalized) * inputVector;
        }
        */

        private bool RaycastForGround(float raycastHeight = 0.3f) {
            bool returnValue = false;
            closestSlopeDistance = raycastHeight;
            closestTouchingGroundDistance = raycastHeight;
            highestWalkableGroundDistance = 0f;

            // create a ring of downward raycasts in a circle around the player at evenly spaced angles
            for (int i = 0; i < 12; i++) {
                Vector3 raycastPoint = (unitController.UnitMotor.MovementBody.GetPosition() + (Vector3.up * raycastHeight) + (Vector3.up * 0.01f)) + (Quaternion.AngleAxis((360f / 12f) * i, Vector3.up) * Vector3.forward * colliderRadius);
                Debug.DrawLine(raycastPoint, new Vector3(raycastPoint.x, raycastPoint.y - raycastHeight - 0.02f, raycastPoint.z), Color.cyan);
                if (unitController.PhysicsScene.Raycast(raycastPoint, Vector3.down, out centerDownHitInfo, Mathf.Infinity, groundMask)) {
                    float groundHitHeight = unitController.transform.InverseTransformPoint(centerDownHitInfo.point).y;
                    
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
                            returnValue = true;
                            // save the ground info if the slope is the closest to the player feet, preferring ground that is at or below the player feet
                            if (((closestTouchingGroundDistance < 0f) && (groundHitHeight < 0f) && (unitController.UnitMotor.MovementBody.GetPosition().y + groundHitHeight < closestTouchingGroundDistance))
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
                //Debug.Log("RaycastForGround() : ground angle: " + Vector3.Angle(groundNormal, Vector3.up));
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
            //Debug.Log($"{unitController.gameObject.name}.PlayerUnitMovementController.MaintainingGround");
            return closeToGround;
        }

        public Vector3 LocalNormalizedSwimMovement(double timeInterval) {
            //Debug.Log("UnitMovementController.NormalizedSwimMovement(): groundAngle: " + groundAngle + "; backwardGroundAngle: " + backwardGroundAngle);

            Vector3 returnValue = currentMovementData.IntendedLocalDirection;

            // check for right mouse button held down to adjust swim angle based on camera angle
            bool chestBelowWater = (unitController.UnitMotor.MovementBody.GetPosition().y + unitController.FloatHeight) < (unitController.CurrentWater[0].SurfaceHeight - (unitController.SwimSpeed * timeInterval));

            if (currentMovementData.RightMouseButtonDown && currentMovementData.HasMoveInput()) {

                // prevent constant bouncing out of water using right mouse
                // always allow downward motion
                // only allow upward motion if the swim speed would not result in a bounce
                float cameraAngle = (currentMovementData.CameraLocalEulerAngleX < 180f ? currentMovementData.CameraLocalEulerAngleX : currentMovementData.CameraLocalEulerAngleX - 360f);
                //Debug.Log($"{unitController.gameObject.name}.PlayerUnitMovementController.SwimMovement(): camera Angle: " + cameraAngle);
                // ignore angle if already touching ground underwater to prevent hitting bottom and stopping while trying to swim forward
                if ((cameraAngle > 0f && returnValue.z > 0f && !touchingGround) // camera above and moving forward / down
                    || (cameraAngle > 0f && returnValue.z < 0f && chestBelowWater == true) // camera above and moving back / up
                    || (cameraAngle < 0f && returnValue.z < 0f && !touchingGround) // camera below and moving back / down
                    || (cameraAngle < 0f && returnValue.z > 0f && chestBelowWater == true) // camera below and forward / up
                    ) {
                    returnValue = Quaternion.AngleAxis(currentMovementData.CameraLocalEulerAngleX, Vector3.right) * returnValue;
                }
            }

            // if the jump or crouch buttons were held down, their values override the camera angle and allow movement straight up or down
            // ignore if swim speed would not result in a bounce out of the water
            if (currentMovementData.InputSink == true
                || (currentMovementData.InputFly == true && (chestBelowWater == true || unitController.CanFly))) {
                returnValue.y = (currentMovementData.InputFly == true ? 1 : 0) + (currentMovementData.InputSink == true ? -1 : 0);
            }

            return returnValue;
        }

        public Vector3 WorldNormalizedSwimMovement(double timeInterval) {
            // 1. Start with the PURE world-space horizontal intent
            Vector3 returnValue = currentMovementData.IntendedWorldDirection;

            // Determine vertical context
            bool chestBelowWater = (unitController.UnitMotor.MovementBody.GetPosition().y + unitController.FloatHeight) <
                                   (unitController.CurrentWater[0].SurfaceHeight - (unitController.SwimSpeed * (float)timeInterval));

            // 2. Adjust swim pitch based on Camera Angle
            if (currentMovementData.RightMouseButtonDown && currentMovementData.HasMoveInput()) {
                float cameraAngle = (currentMovementData.CameraLocalEulerAngleX < 180f ?
                                     currentMovementData.CameraLocalEulerAngleX :
                                     currentMovementData.CameraLocalEulerAngleX - 360f);

                /*
                // We use the stable world direction and rotate it around the CAMERA's right axis
                // instead of the character's right axis to avoid the feedback loop.
                Vector3 cameraRight = Quaternion.Euler(0f, currentMovementData.CameraWantedDirection.y, 0f) * Vector3.right;
                */
                // CALCULATE DYNAMIC RIGHT AXIS (The pivot)
                // Project the camera's forward, flatten it, and cross it with UP 
                // to find the 'Right' vector relative to where you are looking.
                Vector3 camForwardFlat = currentMovementData.CameraWantedDirection;
                camForwardFlat.y = 0;
                camForwardFlat.Normalize();
                Vector3 camRightFlat = Vector3.Cross(Vector3.up, camForwardFlat);

                if ((cameraAngle > 0f && currentMovementData.InputVertical > 0f && !touchingGround) ||
                    (cameraAngle > 0f && currentMovementData.InputVertical < 0f && chestBelowWater) ||
                    (cameraAngle < 0f && currentMovementData.InputVertical < 0f && !touchingGround) ||
                    (cameraAngle < 0f && currentMovementData.InputVertical > 0f && chestBelowWater)) {
                    // Apply the pitch to the world vector
                    returnValue = Quaternion.AngleAxis(cameraAngle, camRightFlat) * returnValue;
                }
            }

            // 3. Manual override (Fly/Sink keys)
            if (currentMovementData.InputSink || (currentMovementData.InputFly && (chestBelowWater || unitController.CanFly))) {
                float verticalInput = (currentMovementData.InputFly ? 1f : 0f) + (currentMovementData.InputSink ? -1f : 0f);
                returnValue.y = verticalInput;
            }

            // 4. Return as a normalized World Direction
            // The physics loop will multiply this by swim speed later.
            return returnValue.normalized;
        }


        public Vector3 LocalNormalizedFlyMovement() {
            //Debug.Log("UnitMovementController.NormalizedFlyMovement(): groundAngle: " + groundAngle + "; backwardGroundAngle: " + backwardGroundAngle);

            Vector3 returnValue = currentMovementData.IntendedLocalDirection;

            if (currentMovementData.RightMouseButtonDown && currentMovementData.HasMoveInput()) {

                float cameraAngle = (currentMovementData.CameraLocalEulerAngleX < 180f ? currentMovementData.CameraLocalEulerAngleX : currentMovementData.CameraLocalEulerAngleX - 360f);
                //Debug.Log($"{unitController.gameObject.name}.PlayerUnitMovementController.SwimMovement(): camera Angle: " + cameraAngle);
                // ignore angle if already touching ground underwater to prevent hitting bottom and stopping while trying to swim forward
                if ((cameraAngle > 0f && returnValue.z > 0f && !touchingGround) // camera above and moving forward / down
                    || (cameraAngle < 0f && returnValue.z < 0f && !touchingGround) // camera below and moving back / down
                    ) {
                    returnValue = Quaternion.AngleAxis(currentMovementData.CameraLocalEulerAngleX, Vector3.right) * returnValue;
                }
            }

            // if the jump or crouch buttons were held down, their values override the camera angle and allow movement straight up or down
            // ignore if swim speed would not result in a bounce out of the water
            if (currentMovementData.InputSink == true
                || currentMovementData.InputFly == true) {
                returnValue.y = (currentMovementData.InputFly == true ? 1 : 0) + (currentMovementData.InputSink == true ? -1 : 0);
            }

            return returnValue;
        }

        public Vector3 WorldNormalizedFlyMovement() {
            // 1. Start with the STABLE world-space horizontal intent (Camera-Relative)
            Vector3 returnValue = currentMovementData.IntendedWorldDirection;

            // 2. Adjust Flight Pitch based on Camera Angle (Right Mouse Held)
            if (currentMovementData.RightMouseButtonDown && currentMovementData.HasMoveInput()) {

                // Normalize camera angle to -180 to 180 range
                float cameraAngle = (currentMovementData.CameraLocalEulerAngleX < 180f ?
                                     currentMovementData.CameraLocalEulerAngleX :
                                     currentMovementData.CameraLocalEulerAngleX - 360f);

                /*
                // Calculate the CAMERA'S right axis in world space for a stable rotation pivot
                Vector3 cameraRight = Quaternion.Euler(0f, currentMovementData.CameraWantedDirection.y, 0f) * Vector3.right;
                */
                
                // 1. PROJECT THE DYNAMIC RIGHT AXIS
                // We take the Camera's forward, flatten it, then find the 'Right' relative to that.
                // This axis is always perpendicular to the direction you are looking.
                Vector3 camForwardFlat = currentMovementData.CameraWantedDirection;
                camForwardFlat.y = 0;
                camForwardFlat.Normalize();

                Vector3 camRightFlat = Vector3.Cross(Vector3.up, camForwardFlat);

                // Determine if we should apply pitch (Forward/Down or Back/Down logic)
                // We use raw InputVertical (W/S) instead of local Z to ensure it's deterministic
                if ((cameraAngle > 0f && currentMovementData.InputVertical > 0f && !touchingGround) ||
                    (cameraAngle < 0f && currentMovementData.InputVertical < 0f && !touchingGround)) {
                    // Rotate the world vector around the stable camera axis
                    returnValue = Quaternion.AngleAxis(cameraAngle, camRightFlat) * returnValue;
                }
            }

            // 3. Manual Vertical Override (Fly/Sink keys)
            // These keys take priority over camera-aimed flight
            if (currentMovementData.InputSink || currentMovementData.InputFly) {
                returnValue.y = (currentMovementData.InputFly ? 1f : 0f) + (currentMovementData.InputSink ? -1f : 0f);
            }

            // 4. Return the final World Direction
            return returnValue.normalized;
        }
        /*
        public Vector3 NormalizedLocalMovement(float calculatedSpeed, Vector3 directionOfTravel, double timeInterval) {
            //Debug.Log("UnitMovementController.NormalizedLocalMovement(" + directionOfTravel + ")");

            Vector3 newReturnValue;
            float usedAngle = groundAngle;
            
            // the normal is the normal of the ground below the player
            Vector3 localGroundNormal = unitController.transform.InverseTransformDirection(groundNormal);

            if (Vector3.Angle(groundNormal, Vector3.up) > slopeLimit) {
                // if standing on jagged ground below stair height at angle greater than slope limit, prevent getting stuck due to capsule collider geometry
                localGroundNormal = Vector3.up;
            }

            // the player is near a front obstacle, and that obstacle is below the slope limit, use its normal
            if (nearBottomFrontObstacle &&
                ((bottomFrontAngleDifferent && bottomFrontObstacleAngle < slopeLimit && nearFrontObstacle == true)
                || (nearTopFrontObstacle == false && nearBottomStairs == false && nearFrontObstacle == true))
                ) {
                localGroundNormal = unitController.transform.InverseTransformDirection(bottomForwardHitInfo.normal);
            } else {
                // the player is near stairs in the direction of travel
                if (nearBottomStairs) {
                    //Debug.Log("near bottom stairs, adjusting forward direction");

                    // 0.2f is an arbitrary distance at the top of the stair is below the start of the curve on the bottom of a capsule collider of 2m height
                    // if the stairs are higher than 0.3f (the start of the vertical section on the collider) and the player is too close to the stair,
                    // any angled approach will lose all momentum from running straight into the stair and the player will get stuck
                    // this value seems to be momentum dependent.  at walking speed of 1, player will not make it over stairs unless it's 0.15f
                    // at 0.15f there is a noticeable slowdown still, and even at 0.1f.  0.05f seems to do it at that speed
                    // if you are touching an obstacle in front, the stair ramp angle should not be used
                    if (
                        (
                        bottomStairDownHitInfo.point.y - unitController.UnitMotor.MovementBody.GetPosition().y < 0.2f
                        || (unitController.transform.InverseTransformPoint(new Vector3(forwardHitPoint.x, unitController.UnitMotor.MovementBody.GetPosition().y, forwardHitPoint.z)).magnitude > (colliderRadius + 0.01f) && nearFrontObstacle == false)
                        )
                        ) {
                        localGroundNormal = unitController.transform.InverseTransformDirection(stairRampNormal);
                    } else {
                        localGroundNormal = unitController.transform.InverseTransformDirection(bottomForwardHitInfo.normal);
                    }
                }
            }

            // to prevent odd floating point issues, set any ground normal that is up to directly up
            if (Mathf.Approximately(localGroundNormal.y, 1f)) {
                localGroundNormal = Vector3.up;
            }

            // translate the input so that the up direction is the same as the normal (up direction) of whatever ground or slope the player is on
            // this prevents losing speed up hills from slamming horizontally into the hill

            newReturnValue = Vector3.Cross(Quaternion.LookRotation(currentMovementData.IntendedLocalDirection, Vector3.up) * unitController.transform.InverseTransformDirection(unitController.transform.right), localGroundNormal);

            // limit upward momentum near stairs to prevent overshooting the stairs in the vertical direction
            if (nearBottomStairs) {
                float clampedReturnValue = Mathf.Clamp(newReturnValue.y, 0f, unitController.transform.InverseTransformPoint(stairDownHitPoint).y / calculatedSpeed / (float)timeInterval);
                newReturnValue.y = clampedReturnValue;
            }

            // apply downforce
            if (groundAngle == 0 && nearBottomFrontObstacle == false && nearBottomStairs == false && touchingGround == false) {
                // this should make the character stick to the ground better when actively moving while grounded
                // ONLY APPLY Y DOWNFORCE ON FLAT GROUND, this will apply a y downforce multiplied by speed, not the existing y downforce from physics (gravity)
                float yValue = 0f;
                if (unitController.transform.InverseTransformPoint(groundPoint).y < -0.001f) {
                    // set a downforce value that should take the character exactly to the ground, and not lower to avoid losing momentum from physics colission with ground
                    yValue = Mathf.Clamp(1, 0, -closestGroundDistance / calculatedSpeed / (float)timeInterval) * -1;
                }
                newReturnValue = new Vector3(newReturnValue.x, yValue, newReturnValue.z);
            }
            return newReturnValue;
        }
        */

        public Vector3 NormalizedWorldMovement(float calculatedSpeed, double timeInterval) {
            // 1. Start with our "Truth": The stable World-Space intent from the camera
            Vector3 worldMoveInput = currentMovementData.IntendedWorldDirection;

            // 2. Determine the "Working Normal" in World Space
            Vector3 workingWorldNormal = groundNormal;

            // Slope Limit Check: If ground is too steep, treat it as flat to avoid capsule sliding issues
            if (Vector3.Angle(groundNormal, Vector3.up) > slopeLimit) {
                workingWorldNormal = Vector3.up;
            }

            // Obstacle/Stair Logic: Override the normal if we are hitting a step or ramp
            if (nearBottomFrontObstacle &&
                ((bottomFrontAngleDifferent && bottomFrontObstacleAngle < slopeLimit && nearFrontObstacle)
                || (!nearTopFrontObstacle && !nearBottomStairs && nearFrontObstacle))) {
                workingWorldNormal = bottomForwardHitInfo.normal;
            } else if (nearBottomStairs) {
                // Stair Step-up logic
                float heightOffset = bottomStairDownHitInfo.point.y - unitController.UnitMotor.MovementBody.GetPosition().y;
                bool farEnoughFromObstacle = (new Vector2(forwardHitPoint.x - unitController.UnitMotor.MovementBody.GetPosition().x, forwardHitPoint.z - unitController.UnitMotor.MovementBody.GetPosition().z).magnitude > (colliderRadius + 0.01f));

                if (heightOffset < 0.2f || (farEnoughFromObstacle && !nearFrontObstacle)) {
                    workingWorldNormal = stairRampNormal;
                } else {
                    workingWorldNormal = bottomForwardHitInfo.normal;
                }
            }

            // Cleanup vertical floating point issues
            if (Mathf.Approximately(workingWorldNormal.y, 1f)) {
                workingWorldNormal = Vector3.up;
            }

            // 3. PROJECT MOVE INPUT ONTO THE NORMAL (World Space)
            // We find the 'Right' vector relative to our move direction, then Cross with the Normal
            // to get a forward vector that perfectly hugs the slope.
            Vector3 worldRight = Vector3.Cross(Vector3.up, worldMoveInput);
            //Vector3 slopeForward = Vector3.Cross(workingWorldNormal, worldRight).normalized;
            Vector3 slopeForward = Vector3.Cross(worldRight, workingWorldNormal).normalized;

            // 4. Vertical Clamping for Stairs (preventing "launching" off steps)
            if (nearBottomStairs) {
                // Calculate the height of the stair relative to the body in world space
                float localYatStair = stairDownHitPoint.y - unitController.UnitMotor.MovementBody.GetPosition().y;
                float maxVerticalVelocity = localYatStair / (float)timeInterval;

                float clampedY = Mathf.Clamp(slopeForward.y * calculatedSpeed, -100f, maxVerticalVelocity) / calculatedSpeed;
                slopeForward = new Vector3(slopeForward.x, clampedY, slopeForward.z);
            }

            // 5. Ground Snapping (Stick to floor on flat ground)
            if (groundAngle == 0 && !nearBottomFrontObstacle && !nearBottomStairs && !touchingGround) {
                if (closestGroundDistance < -0.001f) {
                    float snapDownForce = Mathf.Clamp(1f, 0f, -closestGroundDistance / calculatedSpeed / (float)timeInterval) * -1f;
                    slopeForward.y = snapDownForce;
                }
            }

            return slopeForward;
        }


        /*
        /// <summary>
        /// Calculate the initial velocity of a jump based off gravity and desired maximum height attained
        /// </summary>
        /// <param name="jumpHeight"></param>
        /// <param name="gravity"></param>
        /// <returns></returns>
        private float CalculateJumpSpeed(float jumpHeight, float gravity) {
            return Mathf.Sqrt(2 * jumpHeight * gravity);
        }
        */

        private void CheckGround() {
            //Debug.Log($"{unitController.gameObject.name}.PlayerUnitMovementController.CheckGround()");

            //closestGroundDistance = 0f;
            closestGroundDistance = -stepHeight - 0.01f; // reset closest ground distance to something below step height so player is not considered to be touching by default
            closestWalkableGroundDistance = -stepHeight - 0.01f; // reset closest ground distance to something below step height so player is not considered to be touching by default
            touchingSlope = false;
            touchingGround = false;
            rayCastForGroundRun = false;
            closeToGround = false;

            // set an inital ground distance based on a direct downward raycast from the center of the player
            // later, a more accurate search will be done to see if there is a closer ground distance at the edges of the player capsule
            if (unitController.PhysicsScene.Raycast(unitController.UnitMotor.MovementBody.GetPosition() + (Vector3.up * 0.25f), -Vector3.up, out centerDownHitInfo, Mathf.Infinity, groundMask)) {
                groundNormal = centerDownHitInfo.normal;
                groundPoint = centerDownHitInfo.point;
                if (Vector3.Angle(Vector3.up, centerDownHitInfo.normal) <= slopeLimit) {
                    closestGroundDistance = unitController.transform.InverseTransformPoint(centerDownHitInfo.point).y;
                    closestWalkableGroundDistance = unitController.transform.InverseTransformPoint(centerDownHitInfo.point).y;
                    if ((unitController.transform.InverseTransformPoint(centerDownHitInfo.point).y * -1) <= stepHeight ) {
                        closeToGround = true;
                    }
                    if ((unitController.transform.InverseTransformPoint(centerDownHitInfo.point).y * -1) <= touchingGroundHeight) {
                        touchingGround = true;
                    }
                }
            }

            if (touchingGround == false) {
                if (RaycastForGround()) {
                    touchingGround = true;
                }
            }

            groundAngle = Vector3.Angle(groundNormal, Vector3.up);

            // this is necessary in case the player is moving fast and went off a cliff and we want to apply downforce
            // also needed in case of moving up stairs that are higher than 0.25f (the close to ground height)
            Collider[] hitColliders = new Collider[100];
            int hitCount = unitController.PhysicsScene.OverlapBox(unitController.UnitMotor.MovementBody.GetPosition(), maintainingGroundExtents, hitColliders, unitController.transform.rotation, groundMask);
            if (hitCount > 0) {
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
                    raycastPoint = new Vector3(raycastPoint.x, unitController.UnitMotor.MovementBody.GetPosition().y + stepHeight + 0.001f, raycastPoint.z);
                    Debug.DrawLine(raycastPoint, new Vector3(raycastPoint.x, raycastPoint.y - stepHeight - 0.001f, raycastPoint.z), Color.cyan);
                    if (unitController.PhysicsScene.Raycast(raycastPoint, Vector3.down, out bottomStairDownHitInfo, stepHeight, groundMask)) {
                        // we hit something that is low enough to step on, if it is below the slope limit, we can consider it to be a stair step
                        if (Vector3.Angle(bottomStairDownHitInfo.normal, Vector3.up) < slopeLimit) {
                            bottomFrontStairHeight = bottomStairDownHitInfo.point;
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
                    Debug.DrawLine(raycastPoint, new Vector3(raycastPoint.x, raycastPoint.y - stepHeight - 0.001f, raycastPoint.z), Color.cyan);
                    if (unitController.PhysicsScene.Raycast(raycastPoint, Vector3.down, out topStairDownHitInfo, stepHeight, groundMask)) {
                        // we hit something that is low enough to step on, if it is below the slope limit, we can consider it to be a stair step
                        if (Vector3.Angle(topStairDownHitInfo.normal, Vector3.up) < slopeLimit) {
                            topFrontStairHeight = topStairDownHitInfo.point;
                            nearTopStairs = true;
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
            //Debug.Log("UnitMovementController.PerformLowObstacleCasts()");

            int validResultCount = 0;

            losOriginPoint = unitController.transform.TransformPoint(Vector3.up * (closestWalkableGroundDistance + stepHeight + 0.001f));

            lowObstacleOriginPoint = unitController.transform.TransformPoint(Quaternion.LookRotation(unitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(0f, closestWalkableGroundDistance + stepHeight + 0.001f, 0f));
            // raycast from center in direction of travel
            Debug.DrawLine(lowObstacleOriginPoint,
                lowObstacleOriginPoint + (directionOfTravel * (detectionDistance + colliderRadius)),
                Color.black);

            if (unitController.PhysicsScene.Raycast(lowObstacleOriginPoint, directionOfTravel, (detectionDistance + colliderRadius), groundMask) == true) {
                return;
            } else {
                // check if origin point is on the other side of an obstacle
                // disable if statement because this is the bottom center of capsule and no need to raycast since los origin and bottom origin are identical
                //if (unitController.PhysicsScene.Raycast(losOriginPoint, bottomOriginPoint - losOriginPoint, Vector3.Magnitude(bottomOriginPoint - losOriginPoint), groundMask) == true) {
                    // near an obstacle in front center
                    //Debug.Log("near a low obstacle in front center");
                    validResultCount++;
                    //return;
                //}

            }

            lowObstacleOriginPoint = unitController.transform.TransformPoint(Quaternion.LookRotation(unitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(-colliderRadius, closestWalkableGroundDistance + stepHeight + 0.001f, 0f));
            // raycast from left in direction of travel
            Debug.DrawLine(lowObstacleOriginPoint,
                lowObstacleOriginPoint + (directionOfTravel * detectionDistance),
                Color.black);
            if (unitController.PhysicsScene.Raycast(lowObstacleOriginPoint, directionOfTravel, detectionDistance, groundMask) == true) {
                return;
            } else {
                // check if origin point is on the other side of an obstacle
                if (unitController.PhysicsScene.Raycast(losOriginPoint, lowObstacleOriginPoint - losOriginPoint, Vector3.Magnitude(lowObstacleOriginPoint - losOriginPoint), groundMask) == false) {
                    // near an obstacle in front left
                    //Debug.Log("near a low obstacle in front left");
                    validResultCount++;
                    //return;
                }

            }


            lowObstacleOriginPoint = unitController.transform.TransformPoint(Quaternion.LookRotation(unitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(colliderRadius, closestWalkableGroundDistance + stepHeight + 0.001f, 0f));
            // raycast from right in direction of travel
            Debug.DrawLine(lowObstacleOriginPoint,
                lowObstacleOriginPoint + (directionOfTravel * detectionDistance),
                Color.black);
            if (unitController.PhysicsScene.Raycast(lowObstacleOriginPoint, directionOfTravel, detectionDistance, groundMask) == true) {
                return;
            } else {
                // check if origin point is on the other side of an obstacle
                if (unitController.PhysicsScene.Raycast(losOriginPoint, lowObstacleOriginPoint - losOriginPoint, Vector3.Magnitude(lowObstacleOriginPoint - losOriginPoint), groundMask) == false) {
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

            losOriginPoint = unitController.transform.TransformPoint(Vector3.up * groundOffset);

            bottomOriginPoint = unitController.transform.TransformPoint(Quaternion.LookRotation(unitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(0f, groundOffset, 0f));
            // raycast from center in direction of travel
            Debug.DrawLine(bottomOriginPoint,
                bottomOriginPoint + (directionOfTravel * (detectionDistance + colliderRadius)),
                Color.black);

            if (unitController.PhysicsScene.Raycast(bottomOriginPoint, directionOfTravel, out bottomForwardHitInfo, (detectionDistance + colliderRadius), groundMask)) {
                // check if origin point is on the other side of an obstacle
                //if (unitController.PhysicsScene.Raycast(losOriginPoint, bottomOriginPoint - losOriginPoint, out losHitInfo, Vector3.Magnitude(bottomOriginPoint - losOriginPoint), groundMask) == false) {
                    // near an obstacle in front center
                    //Debug.Log("near a bottom obstacle in front center");
                    nearBottomFrontObstacle = true;
                    // disabled if statement to allow for walking up high slopes below stair limit
                    if (NearBottomStair(directionOfTravel)) {
                        //NearBottomStair(directionOfTravel);
                        topOriginPoint = unitController.transform.TransformPoint(Quaternion.LookRotation(unitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(0f, unitController.transform.InverseTransformPoint(bottomStairDownHitInfo.point).y + 0.001f, 0f));

                        Debug.DrawLine(topOriginPoint,
                                                        topOriginPoint + (directionOfTravel * (detectionDistance + colliderRadius)),
                                                        Color.black);
                        if (unitController.PhysicsScene.Raycast(topOriginPoint, directionOfTravel, out topForwardHitInfo, (detectionDistance + colliderRadius), groundMask)) {
                            // near an obstacle in front center
                            //Debug.Log("near a top obstacle in front center");
                            nearTopFrontObstacle = true;
                            NearTopStair(directionOfTravel);
                        }
                    } else {
                        topOriginPoint = unitController.transform.TransformPoint(Quaternion.LookRotation(unitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(0f, stepHeight, 0f));

                        Debug.DrawLine(topOriginPoint,
                                                        topOriginPoint + (directionOfTravel * (detectionDistance + colliderRadius)),
                                                        Color.black);
                        if (unitController.PhysicsScene.Raycast(topOriginPoint, directionOfTravel, out topForwardHitInfo, (detectionDistance + colliderRadius), groundMask)) {
                            // near an obstacle in front center
                            //Debug.Log("near a top obstacle in front center");
                            nearTopFrontObstacle = true;
                        }

                    }
                    return;
                //}
            }

            bottomOriginPoint = unitController.transform.TransformPoint(Quaternion.LookRotation(unitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(-colliderRadius, groundOffset, 0f));
            // raycast from left in direction of travel
            Debug.DrawLine(bottomOriginPoint,
                bottomOriginPoint + (directionOfTravel * detectionDistance),
                Color.black);
            if (unitController.PhysicsScene.Raycast(bottomOriginPoint, directionOfTravel, out bottomForwardHitInfo, detectionDistance, groundMask)) {
                // check if origin point is on the other side of an obstacle
                if (unitController.PhysicsScene.Raycast(losOriginPoint, bottomOriginPoint - losOriginPoint, out losHitInfo, Vector3.Magnitude(bottomOriginPoint - losOriginPoint), groundMask) == false) {
                    // near an obstacle in front left
                    //Debug.Log("near a bottom obstacle in front left");
                    nearBottomFrontObstacle = true;
                    // disabled if statement to allow for walking up high slopes below stair limit
                    if (NearBottomStair(directionOfTravel)) {
                        //NearBottomStair(directionOfTravel);
                        topOriginPoint = unitController.transform.TransformPoint(Quaternion.LookRotation(unitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(-colliderRadius, unitController.transform.InverseTransformPoint(bottomStairDownHitInfo.point).y + 0.001f, 0f));
                        Debug.DrawLine(topOriginPoint,
                            topOriginPoint + (directionOfTravel * detectionDistance),
                            Color.black);
                        if (unitController.PhysicsScene.Raycast(topOriginPoint, directionOfTravel, out topForwardHitInfo, detectionDistance, groundMask)) {
                            // near an obstacle in front left
                            //Debug.Log("near a top obstacle in front left");
                            nearTopFrontObstacle = true;
                            NearTopStair(directionOfTravel);
                        }

                    } else {
                        topOriginPoint = unitController.transform.TransformPoint(Quaternion.LookRotation(unitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(-colliderRadius, stepHeight, 0f));
                        Debug.DrawLine(topOriginPoint,
                            topOriginPoint + (directionOfTravel * detectionDistance),
                            Color.black);
                        if (unitController.PhysicsScene.Raycast(topOriginPoint, directionOfTravel, out topForwardHitInfo, detectionDistance, groundMask)) {
                            // near an obstacle in front left
                            //Debug.Log("near a top obstacle in front left");
                            nearTopFrontObstacle = true;
                        }

                    }
                    return;
                }
            }


            bottomOriginPoint = unitController.transform.TransformPoint(Quaternion.LookRotation(unitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(colliderRadius, groundOffset, 0f));
            // raycast from right in direction of travel
            Debug.DrawLine(bottomOriginPoint,
                bottomOriginPoint + (directionOfTravel * detectionDistance),
                Color.black);
            if (unitController.PhysicsScene.Raycast(bottomOriginPoint, directionOfTravel, out bottomForwardHitInfo, detectionDistance, groundMask)) {
                // check if origin point is on the other side of an obstacle
                if (unitController.PhysicsScene.Raycast(losOriginPoint, bottomOriginPoint - losOriginPoint, out losHitInfo, Vector3.Magnitude(bottomOriginPoint - losOriginPoint), groundMask) == false) {
                    // near an obstacle in front right
                    //Debug.Log("near a bottom obstacle in front right");
                    nearBottomFrontObstacle = true;
                    // disabled if statement to allow for walking up high slopes below stair limit
                    if (NearBottomStair(directionOfTravel)) {
                        //NearBottomStair(directionOfTravel);
                        topOriginPoint = unitController.transform.TransformPoint(Quaternion.LookRotation(unitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(colliderRadius, unitController.transform.InverseTransformPoint(bottomStairDownHitInfo.point).y + 0.001f, 0f));
                        Debug.DrawLine(topOriginPoint,
                            topOriginPoint + (directionOfTravel * detectionDistance),
                            Color.black);
                        if (unitController.PhysicsScene.Raycast(topOriginPoint, directionOfTravel, out topForwardHitInfo, detectionDistance, groundMask)) {
                            // near an obstacle in front right
                            //Debug.Log("near a top obstacle in front right");
                            nearTopFrontObstacle = true;
                            NearTopStair(directionOfTravel);
                        }

                    } else {
                        topOriginPoint = unitController.transform.TransformPoint(Quaternion.LookRotation(unitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(colliderRadius, stepHeight, 0f));
                        Debug.DrawLine(topOriginPoint,
                            topOriginPoint + (directionOfTravel * detectionDistance),
                            Color.black);
                        if (unitController.PhysicsScene.Raycast(topOriginPoint, directionOfTravel, out topForwardHitInfo, detectionDistance, groundMask)) {
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
            forwardOriginPoint = unitController.transform.TransformPoint(Quaternion.LookRotation(unitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(0f, stepHeight + 0.001f, colliderRadius + 0.01f));
            Debug.DrawLine(forwardOriginPoint,
                forwardOriginPoint + (Vector3.down * stepHeight),
                Color.black);

            // perform a downward raycast from center just in front of collider to see if close to contacting any obstacle
            if (unitController.PhysicsScene.Raycast(forwardOriginPoint, Vector3.down, out obstacleCastHitInfo, stepHeight, groundMask)) {
                // if there was a hit, return the height 1mm below the height of the hit
                nearFrontObstacle = true;
                return unitController.transform.InverseTransformPoint(obstacleCastHitInfo.point).y - 0.001f;
            }

            // if there was no hit in the center, perform a cast on the left side of the character from just in front of the collider
            forwardOriginPoint = unitController.transform.TransformPoint(Quaternion.LookRotation(unitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(colliderRadius, stepHeight + 0.001f, colliderRadius + 0.01f));
            Debug.DrawLine(forwardOriginPoint,
                forwardOriginPoint + (Vector3.down * stepHeight),
                Color.black);
            if (unitController.PhysicsScene.Raycast(forwardOriginPoint, Vector3.down, out obstacleCastHitInfo, stepHeight, groundMask)) {
                // if there was a hit, return the height 1mm below the height of the hit
                nearFrontObstacle = true;
                return unitController.transform.InverseTransformPoint(obstacleCastHitInfo.point).y - 0.001f;
            }

            // if there was no hit on the left, perform a cast on the right side of the character just in front of the collider
            forwardOriginPoint = unitController.transform.TransformPoint(Quaternion.LookRotation(unitController.transform.InverseTransformDirection(directionOfTravel)) * new Vector3(-colliderRadius, stepHeight + 0.001f, colliderRadius + 0.01f));
            Debug.DrawLine(forwardOriginPoint,
                forwardOriginPoint + (Vector3.down * stepHeight),
                Color.black);
            if (unitController.PhysicsScene.Raycast(forwardOriginPoint, Vector3.down, out obstacleCastHitInfo, stepHeight, groundMask)) {
                // if there was a hit, return the height 1mm below the height of the hit
                nearFrontObstacle = true;
                return unitController.transform.InverseTransformPoint(obstacleCastHitInfo.point).y - 0.001f;
            }



            // the player is not close to touching any obstacle, return the default height
            return 0.001f;
        }

        public void CheckFrontObstacle(float calculatedSpeed, Vector3 directionOfTravel, double timeInterval) {
            // reset variables
            nearFrontObstacle = false;
            nearBottomFrontObstacle = false;
            nearTopFrontObstacle = false;
            nearLowObstacle = false;
            bottomFrontAngleDifferent = false;
            nearBottomStairs = false;
            nearTopStairs = false;
            float detectionDistance = stairDetectionDistance + (calculatedSpeed * (float)timeInterval);

            PerformFrontObstacleCasts(directionOfTravel, detectionDistance, GetFrontObstacleCastHeight(directionOfTravel));

            // it is possible that an obstacle in front starts above the ground
            // if no obstacle was detected at the ground, perform a downward raycast from slightly in front of the character to determine a better height to check from
            if (nearBottomFrontObstacle == false) {
                if (rayCastForGroundRun == false) {
                    RaycastForGround();
                }
                if (highestWalkableGroundDistance > 0.001f) {
                    PerformFrontObstacleCasts(directionOfTravel, detectionDistance, highestWalkableGroundDistance - 0.001f);
                }
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
                    bottomPoint = new Vector3(bottomOriginPoint.x, unitController.UnitMotor.MovementBody.GetPosition().y, bottomOriginPoint.z);
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
                //Debug.Log("UnitMovementController.ApplyGravity(): Not Grounded");
            }
        }
        */

        private void DrawDebugLines() {
            if (!debug) {
                return;
            }
            Debug.DrawLine(unitController.UnitMotor.MovementBody.GetPosition() + (Vector3.up * 0.25f), (unitController.UnitMotor.MovementBody.GetPosition() + (Vector3.up * 0.25f)) - (Vector3.up * closeToGroundHeight), Color.green);
        }

        /*
        private void OnDrawGizmos() {
            if (unitController == null) {
                return;
            }
            //Gizmos.color = Color.white;
            //Gizmos.DrawWireCube(unitController.UnitMotor.MovementBody.GetPosition(), (Quaternion.LookRotation(unitController.transform.forward) * touchingGroundExtents) * 2);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(unitController.UnitMotor.MovementBody.GetPosition(), maintainingGroundExtents * 2);
        }
        */

        public void StateUpdate(MovementData movementData, double timeInterval, bool isReplay) {
            //Debug.Log($"{unitController.gameObject.name}.UnitMovementController.StateUpdate()");

            if (currentIMovementState == null) {
                return;
            }

            if (unitController.CharacterStats.IsReviving == true) {
                return;
            }

            currentMovementData = movementData;

            if (unitController.IsMounted == false) {
                if (movementData.HasAnyInput() == true
                && (networkManagerServer.ServerModeActive == true || systemGameManager.GameMode == GameMode.Local)) {
                    unitController.UnitMotor.StopFollowingTarget();
                }

                EarlyGlobalStateUpdate();
            }

            currentIMovementState.Update(isReplay, timeInterval);

            if (unitController.IsMounted == false) {
                LateGlobalStateUpdate(timeInterval);
            }
        }

        public void AddMovementData(MovementData frameData) {
            accumulatedMovementData.InputHorizontal += frameData.InputHorizontal;
            accumulatedMovementData.InputVertical += frameData.InputVertical;
            accumulatedMovementData.InputTurn += frameData.InputTurn;
            accumulatedMovementData.RightAnalogHorizontal += frameData.RightAnalogHorizontal;

            // 2. Logical OR for buttons (if true in ANY frame, it stays true for the tick)
            if (frameData.InputJump) accumulatedMovementData.InputJump = true;
            if (frameData.InputFly) accumulatedMovementData.InputFly = true;
            if (frameData.InputSink) accumulatedMovementData.InputSink = true;
            if (frameData.InputStrafe) accumulatedMovementData.InputStrafe = true;
            if (frameData.InputCrouch) accumulatedMovementData.InputCrouch = true;
            if (frameData.RightMouseButtonDown) accumulatedMovementData.RightMouseButtonDown = true;
            if (frameData.RightMouseDragged) accumulatedMovementData.RightMouseDragged = true;
            if (frameData.GamepadModeActive) accumulatedMovementData.GamepadModeActive = true;

            accumulatedMovementData.CameraWantedDirection = frameData.CameraWantedDirection;
            accumulatedMovementData.CameraLocalEulerAngleX = frameData.CameraLocalEulerAngleX;

            // 3. Increment frame counter
            accumulatedMovementData.FrameCount++;
        }

        public MovementData ProcessGatheredInput() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMovementController.ProcessGatheredInput()");

            MovementData tickReadyData = new MovementData();

            if (accumulatedMovementData.FrameCount > 0) {
                // 1. CACHE CONTINUOUS AXES
                cachedMovementData.InputHorizontal = accumulatedMovementData.InputHorizontal / accumulatedMovementData.FrameCount;
                cachedMovementData.InputVertical = accumulatedMovementData.InputVertical / accumulatedMovementData.FrameCount;
                cachedMovementData.InputTurn = accumulatedMovementData.InputTurn / accumulatedMovementData.FrameCount;
                cachedMovementData.RightAnalogHorizontal = accumulatedMovementData.RightAnalogHorizontal / accumulatedMovementData.FrameCount;

                // 2. CACHE HELD ACTIONS (KeyBindWasPressedOrHeld)
                // These stay true for every tick in this frame.
                cachedMovementData.InputFly = accumulatedMovementData.InputFly;
                cachedMovementData.InputSink = accumulatedMovementData.InputSink;
                cachedMovementData.InputStrafe = accumulatedMovementData.InputStrafe;

                // Cache these so mouse-based turning doesn't drop out
                cachedMovementData.RightMouseButtonDown = accumulatedMovementData.RightMouseButtonDown;
                cachedMovementData.RightMouseDragged = accumulatedMovementData.RightMouseDragged;
                cachedMovementData.CameraLocalEulerAngleX = accumulatedMovementData.CameraLocalEulerAngleX;
                cachedMovementData.CameraWantedDirection = accumulatedMovementData.CameraWantedDirection;

                // 3. ASSIGN ONE-SHOT TRIGGERS (KeyBindWasPressed)
                // These are ONLY true for the first tick of the frame.
                tickReadyData.InputJump = accumulatedMovementData.InputJump;
                tickReadyData.InputCrouch = accumulatedMovementData.InputCrouch;

                // 4. RESET THE ACCUMULATOR
                // This clears the one-shots so tick #2 and #3 in this frame don't double-jump.
                accumulatedMovementData.ResetMoveInput();
            } else {
                // SUB-TICK SCENARIO (Tick #2 or #3 in a single frame)
                tickReadyData.InputJump = false;
                tickReadyData.InputCrouch = false;
            }

            // 5. RE-APPLY PERSISTENT DATA (Axes + Held Actions)
            tickReadyData.InputHorizontal = cachedMovementData.InputHorizontal;
            tickReadyData.InputVertical = cachedMovementData.InputVertical;
            tickReadyData.InputTurn = cachedMovementData.InputTurn;
            tickReadyData.RightAnalogHorizontal = cachedMovementData.RightAnalogHorizontal;

            tickReadyData.InputFly = cachedMovementData.InputFly;
            tickReadyData.InputSink = cachedMovementData.InputSink;
            tickReadyData.InputStrafe = cachedMovementData.InputStrafe;

            tickReadyData.RightMouseButtonDown = cachedMovementData.RightMouseButtonDown;
            tickReadyData.RightMouseDragged = cachedMovementData.RightMouseDragged;
            tickReadyData.CameraLocalEulerAngleX = cachedMovementData.CameraLocalEulerAngleX;
            tickReadyData.CameraWantedDirection = cachedMovementData.CameraWantedDirection;

            // 6. METADATA & DERIVED VECTORS
            tickReadyData.GamepadModeActive = accumulatedMovementData.GamepadModeActive;
            tickReadyData.NormalizedMoveInput = new Vector3(tickReadyData.InputHorizontal, 0, tickReadyData.InputVertical).normalized;
            tickReadyData.TurnInput = new Vector3(tickReadyData.InputTurn, 0, 0);

            /*
            // 7. CALCULATE THE "STABLE" WORLD DIRECTION
            // We use the camera's Y-rotation captured during this tick.
            Quaternion cameraYRotation = Quaternion.Euler(0f, tickReadyData.CameraWantedDirection.y, 0f);
            Debug.Log($"Camera Y Rotation for movement: {cameraYRotation.eulerAngles.y} degrees camerawanteddirection.y {tickReadyData.CameraWantedDirection.y}");

            // This is the "Universal" intended direction (e.g., W always moves 'Into' the screen)
            tickReadyData.IntendedWorldDirection = cameraYRotation * tickReadyData.NormalizedMoveInput;
            Debug.Log($"Intended World Direction: {tickReadyData.IntendedWorldDirection} from NormalizedMoveInput {tickReadyData.NormalizedMoveInput} and Camera Y Rotation {cameraYRotation.eulerAngles.y}");
            */
            /*
            // 1.Get the stable forward direction from the camera vector
            Vector3 camForward = tickReadyData.CameraWantedDirection;
            camForward.y = 0; // Flatten to horizontal plane
            camForward.Normalize();

            // 2. Derive the 'Right' vector from that forward
            Vector3 camRight = Vector3.Cross(Vector3.up, camForward);

            // 3. Combine with WASD/Joystick input to get the World Intent
            // This ensures 'W' is always "Into the screen"
            tickReadyData.IntendedWorldDirection = (camForward * tickReadyData.InputVertical) + (camRight * tickReadyData.InputHorizontal);
            */

            Quaternion headingRotation;

            if (controlsManager.GamepadModeActive || unitController.UnitProfile.UnitPrefabProps.RotateModel) {
                // FREE ROTATE: Forward is the CAMERA
                Vector3 camForward = tickReadyData.CameraWantedDirection;
                camForward.y = 0;
                if (camForward == Vector3.zero) {
                    // If the camera forward is too small, default to character forward
                    camForward = unitController.transform.forward;
                    camForward.y = 0;
                }
                headingRotation = Quaternion.LookRotation(camForward.normalized);
            } else {
                // STRAFE MODE: Heading is the CHARACTER's physical rotation
                // Use the Rigidbody/MovementBody rotation, NOT the smoothed transform
                
                Quaternion physicsRotation = unitController.UnitMotor.MovementBody.GetRotation();
                Vector3 physicsEuler = physicsRotation.eulerAngles;

                // Flatten to Y-axis only
                headingRotation = Quaternion.Euler(0f, physicsEuler.y, 0f);
            }

            // 3. Calculate World Intent based on that heading
            tickReadyData.IntendedWorldDirection = headingRotation * tickReadyData.NormalizedMoveInput;

            //Debug.Log($"Intended World Direction: {tickReadyData.IntendedWorldDirection} from NormalizedMoveInput {tickReadyData.NormalizedMoveInput} and Camera Wanted Direction {tickReadyData.CameraWantedDirection}");

            // 8. CALCULATE INTENDED LOCAL DIRECTION (Preserving your mode logic)
            if (controlsManager.GamepadModeActive || unitController.UnitProfile.UnitPrefabProps.RotateModel) {
                // FREE ROTATE MODE: Direction is relative to the Camera.
                // We use the stable World Direction transformed by the CURRENT character rotation.
                // This is safe for the animator, but we will use the World Direction for physics.
                tickReadyData.IntendedLocalDirection = unitController.transform.InverseTransformDirection(tickReadyData.IntendedWorldDirection);
            } else {
                // STRAFE MODE: "W" is always the character's own forward.
                // In this mode, we don't look at the camera; we just use the raw WASD input.
                tickReadyData.IntendedLocalDirection = tickReadyData.NormalizedMoveInput;
            }
            /*
            if (controlsManager.GamepadModeActive || unitController.UnitProfile.UnitPrefabProps.RotateModel) {
                Vector3 cameraInput = Quaternion.Euler(0f, cameraManager.ActiveMainCamera.transform.rotation.eulerAngles.y, 0f) * tickReadyData.NormalizedMoveInput;
                tickReadyData.IntendedLocalDirection = unitController.transform.InverseTransformDirection(cameraInput);
            } else {
                tickReadyData.IntendedLocalDirection = tickReadyData.NormalizedMoveInput;
            }
            */

            return tickReadyData;
        }



        public void SetStateSilently(CharacterMovementState characterMovementState) {
            //Debug.Log($"{unitController.gameObject.name}: SetStateSilently({characterMovementState.ToString()})");

            if (movementStates.ContainsKey(characterMovementState)) {
                currentCharacterMovementState = characterMovementState;
                SetStateSilently(movementStates[characterMovementState]);
            } else {
                Debug.LogError($"UnitMovementController.SetStateSilently({characterMovementState.ToString()}): no state found in movementStates dictionary");
            }
        }

        private void SetStateSilently(IMovementState newState) {
            //Debug.Log($"{gameObject.name}: SetStateSilently({newState.ToString()})");
            if (currentIMovementState != null) {
                currentIMovementState.Exit(true, true);
            }
            currentIMovementState = newState;

            currentIMovementState.Enter(true, true);
        }
    }

}