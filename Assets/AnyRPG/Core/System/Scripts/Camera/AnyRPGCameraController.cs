using AnyRPG;
using System.Drawing.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnyRPG {
    public class AnyRPGCameraController : ConfiguredMonoBehaviour {
        // public variables

        [SerializeField]
        private Camera usedCamera = null;

        private Transform cameraTransform = null;

        [SerializeField]
        private Transform target = null;

        [Tooltip("Ignore these layers when checking if walls are in the way of the camera view of the character")]
        [SerializeField]
        private LayerMask ignoreMask = ~0;

        public float cameraFollowSpeed = 10f;
        public float zoomSpeed = 4f;
        private float gamepadZoomSpeed = 0.05f;
        public float minZoom = 2f;
        public float maxZoom = 15f;
        public float maxVerticalPan = 45;
        public float minVerticalPan = -45;
        public float firstPersonMaxVerticalPan = 75;
        public float firstPersonMinVerticalPan = -35;
        private float currentMaxVerticalPan = 45;
        private float currentMinVerticalPan = -45;

        public float initialYDegrees = 0f;
        public float initialXDegrees = 0f;
        public float initialZoomDistance = 4f;

        public float pitch = 2f;
        public float yawSpeed = 10f;

        public float analogYawSpeed = 5f;

        public bool followBehind = true;

        // private variables
        private Vector3 wantedPosition;
        private Vector3 targetPosition;
        private float userOffsetAngle;
        // needed to prevent annoying debug messages in console log
        private Vector3 cameraTransformForward;

        // needed in case player is holding right mouse down while camera is moving around to avoid obstacles, which can cause the forward direction of movement to get skewed
        private Vector3 wantedDirection;

        private float currentZoomDistance = 0f;
        private float currentYDegrees = 0f;
        private float currentXDegrees = 0f;

        // when using free run mode, the player angle needs to be calculated when the player rotation changes to keep the camera relative
        //private float calculatedAngle = 0f;

        //private bool hasMoved = false;
        // keep track if we are panning or zooming this frame
        private bool cameraPan = false;
        private bool cameraZoom = false;
        private bool rightCameraPan = false;

        // avoid use of local variables
        private RaycastHit wallHit = new RaycastHit();

        // game manager references
        protected InputManager inputManager = null;
        protected NamePlateManager namePlateManager = null;
        protected UIManager uIManager = null;
        protected WindowManager windowManager = null;
        protected PlayerManagerClient playerManagerClient = null;
        protected ControlsManager controlsManager = null;

        public Transform Target { get => target; set => target = value; }
        public Vector3 WantedDirection { get => wantedDirection; set => wantedDirection = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);


            if (usedCamera != null) {
                cameraTransform = usedCamera.transform;
            }
            currentMaxVerticalPan = maxVerticalPan;
            currentMinVerticalPan = minVerticalPan;

            SetInitialDegreesAndZoom();
            if (systemConfigurationManager.AllowFirstPersonCamera) {
                minZoom = 0.1f;
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            inputManager = systemGameManager.InputManager;
            uIManager = systemGameManager.UIManager;
            namePlateManager = uIManager.NamePlateManager;
            windowManager = systemGameManager.WindowManager;
            playerManagerClient = systemGameManager.PlayerManagerClient;
            controlsManager = systemGameManager.ControlsManager;
        }

        private void SetInitialDegreesAndZoom() {
            if (systemConfigurationManager.CameraViewMode == CameraViewMode.Isometric) {
                SetIsometricInitialValues();
            } else if (systemConfigurationManager.CameraViewMode == CameraViewMode.Free) {
                SetFreeInitialvalues();
            }
        }

        private void SetIsometricInitialValues() {
            currentZoomDistance = systemConfigurationManager.InitialIsometricVector.magnitude;

            // Use LookRotation to find the angles required to point from the camera to the player
            // We negate the vector because we want the rotation LOOKING at the target
            Quaternion isoRotation = Quaternion.LookRotation(-systemConfigurationManager.InitialIsometricVector);

            currentXDegrees = isoRotation.eulerAngles.y;
            currentYDegrees = -isoRotation.eulerAngles.x; // Negated to match your orbit math
        }

        private void SetFreeInitialvalues() {
            // reset the camera to directly behind the player at the previous zoom level
            currentZoomDistance = initialZoomDistance;
            currentYDegrees = initialYDegrees;
            currentXDegrees = initialXDegrees;
        }

        public void ClearTarget() {
            target = null;
        }

        public void SetTarget(Transform newTarget) {
            //Debug.Log("AnyRPGCameraController.SetTarget(" + newTarget + ")");
            target = newTarget;
            SetTargetPosition();

        }

        private void SetTargetPosition() {
            //Debug.Log("SetTargetPosition()");
            targetPosition = target.position + Vector3.up * pitch;
            //Debug.Log("SetTargetPosition(): " + targetPosition);
        }

        // use this to set camera position before player spawn so we aren't staring at some wierd spot of the level while player loads
        public void SetTargetPositionRaw(Vector3 rawTargetPosition, Vector3 forwardDirection) {
            //Debug.Log($"SetTargetPositionRaw({rawTargetPosition}, {forwardDirection})");

            SetInitialDegreesAndZoom();

            targetPosition = rawTargetPosition + Vector3.up * pitch;
            if (systemConfigurationManager.CameraViewMode == CameraViewMode.Isometric) {
                // Use the isometric rotation we just calculated in SetInitialValues
                Quaternion isoRotation = Quaternion.Euler(-currentYDegrees, currentXDegrees, 0);
                // Position the camera offset by the zoom distance along that rotation
                transform.position = targetPosition + (isoRotation * Vector3.back * currentZoomDistance);
            } else {
                // Standard FREE mode behavior: Stay behind the forwardDirection
                if (forwardDirection == Vector3.zero) {
                    transform.position = targetPosition - new Vector3(0, 0, currentZoomDistance);
                } else {
                    transform.position = targetPosition - (Quaternion.LookRotation(forwardDirection) * new Vector3(0, 0, currentZoomDistance));
                }
            }
            /*
            if (forwardDirection == Vector3.zero) {
                transform.position = targetPosition - new Vector3(0, 0, currentZoomDistance);
            } else {
                transform.position = targetPosition - (Quaternion.LookRotation(forwardDirection) * new Vector3(0, 0, currentZoomDistance));
            }
            */

            LookAtTargetPosition();
        }

        public void InitializeCamera(Transform newTarget) {
            //Debug.Log($"AnyRPGCameraController.InitializeCamera({newTarget.gameObject.name})");

            SetTarget(newTarget);
            JumpToFollowSpot();
        }

        private void LateUpdate() {
            if (target == null) {
                // camera has nothing to follow so don't calculate movement
                return;
            }
            //Debug.Log("CameraController.LateUpdate(): frame " + Time.frameCount);

            cameraPan = false;
            cameraZoom = false;
            rightCameraPan = false;
            bool wasMinZoom = currentZoomDistance == minZoom;

            /*
            if (lastTargetForward != target.transform.forward
                && playerManager.PlayerController.MovementData.inputTurn == 0f) {
                calculatedAngle = Vector3.SignedAngle(target.transform.forward, transform.forward, Vector3.up);
                currentXDegrees = calculatedAngle;
            }
            lastTargetForward = target.transform.forward;
            */

            SetTargetPosition();

            // ====MOUSE ZOOM====
            // added code at end to check if over nameplate and allow scrolling
            if (inputManager.mouseScrolled
                && (!EventSystem.current.IsPointerOverGameObject() || namePlateManager.MouseOverNamePlate())) {
                currentZoomDistance += (Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * -1);
                currentZoomDistance = Mathf.Clamp(currentZoomDistance, minZoom, maxZoom);
                cameraZoom = true;
                if (currentZoomDistance == minZoom && wasMinZoom == false) {
                    //Debug.Log("Camera zoomed to min zoom distance.  Jumping to Wanted Position.");
                    ActivateFirstPersonView();
                } else if (currentZoomDistance > minZoom && wasMinZoom == true) {
                    //Debug.Log("Camera zoomed out of first person view.  Jumping to Wanted Position.");
                    DeactivateFirstPersonView();
                }
            }

            // ====GAMEPAD ZOOM====
            if (playerManagerClient.ActiveUnitController?.CharacterAbilityManager.WaitingForTarget() == false) {
                if ((windowManager.CurrentWindow == null || windowManager.CurrentWindow.CaptureCamera == false)
                    && Input.GetAxis("RightAnalogVertical") != 0f
                    && inputManager.KeyBindWasPressedOrHeld("JOYSTICKBUTTON9")) {
                    currentZoomDistance += (Input.GetAxis("RightAnalogVertical") * gamepadZoomSpeed * -1);
                    currentZoomDistance = Mathf.Clamp(currentZoomDistance, minZoom, maxZoom);
                    cameraZoom = true;
                }
            }

            if (systemConfigurationManager.CameraViewMode == CameraViewMode.Free) {
                // ====MOUSE PAN====
                // pan with the left or turn with the right mouse button
                if (!uIManager.DragInProgress && ((inputManager.rightMouseButtonDown && !inputManager.rightMouseButtonClickedOverUI) || (inputManager.leftMouseButtonDown && !inputManager.leftMouseButtonClickedOverUI))) {
                    float usedTurnSpeed = 0f;
                    if (inputManager.rightMouseButtonDown
                        && (inputManager.rightMouseButtonDownPosition != Input.mousePosition || playerManagerClient.PlayerController.MovementData.HasMoveInput())) {
                        usedTurnSpeed = PlayerPrefs.GetFloat("MouseTurnSpeed") + 0.5f;
                        rightCameraPan = true;
                        userOffsetAngle = 0f;
                    } else {
                        usedTurnSpeed = PlayerPrefs.GetFloat("MouseLookSpeed") + 0.5f;
                    }
                    currentXDegrees += Input.GetAxis("Mouse X") * yawSpeed * usedTurnSpeed;
                    currentYDegrees += (Input.GetAxis("Mouse Y") * yawSpeed * usedTurnSpeed) * (PlayerPrefs.GetInt("MouseInvert") == 0 ? 1 : -1);

                    if (!inputManager.rightMouseButtonDown) {
                        // RE-ANCHOR: Calculate how far 'off-center' we are from the player's current facing.
                        // This allows the camera to stay at this specific side-angle when you let go.
                        userOffsetAngle = Mathf.DeltaAngle(target.eulerAngles.y, currentXDegrees);
                    }

                    cameraPan = true;
                }

                // ====GAMEPAD PAN====
                if (playerManagerClient.ActiveUnitController?.CharacterAbilityManager.WaitingForTarget() == false) {
                    if ((windowManager.CurrentWindow == null || windowManager.CurrentWindow.CaptureCamera == false)
                    && inputManager.KeyBindWasPressedOrHeld("JOYSTICKBUTTON9") == false
                    && (Input.GetAxis("RightAnalogHorizontal") != 0f || Input.GetAxis("RightAnalogVertical") != 0f)) {
                        if (Input.GetAxis("RightAnalogHorizontal") != 0f) {
                            currentXDegrees += Input.GetAxis("RightAnalogHorizontal") * analogYawSpeed * (PlayerPrefs.GetFloat("JoystickLookSpeed"));
                            cameraPan = true;
                        }
                        if (Input.GetAxis("RightAnalogVertical") != 0f) {
                            currentYDegrees += (Input.GetAxis("RightAnalogVertical") * analogYawSpeed * (PlayerPrefs.GetFloat("JoystickLookSpeed"))) * (PlayerPrefs.GetInt("JoystickInvert") == 0 ? 1 : -1);
                            cameraPan = true;
                        }
                    }
                }

                if (currentXDegrees > 180f) {
                    currentXDegrees -= 360f;
                }

                if (currentXDegrees < -180f) {
                    currentXDegrees += 360f;
                }

                // if the camera was panned or we just zoomed into first person view, clamp the vertical angle to prevent weird camera angles
                if (cameraPan || (currentZoomDistance == minZoom && wasMinZoom == false)) {
                    currentYDegrees = Mathf.Clamp(currentYDegrees, currentMinVerticalPan, currentMaxVerticalPan);
                }
            }

            // follow the player
            //if (hasMoved || cameraZoom || (cameraPan && !inputManager.rightMouseButtonClickedOverUI && !inputManager.leftMouseButtonClickedOverUI) ) {
            SetWantedPosition();
            //}

            CompensateForWalls();
            if (cameraZoom || cameraPan) {
                //Debug.Log("Camera was zoomed or panned.  Jumping to Wanted Position.");
                JumpToWantedPosition();
            } else {
                // camera is just doing a regular follow, smoother motion is good
                SmoothToWantedPosition();
            }
            LookAtTargetPosition();

            SystemEventManager.TriggerEvent("AfterCameraUpdate", new EventParamProperties());
        }

        private void ActivateFirstPersonView() {
            Debug.Log("AnyRPGCameraController.ActivateFirstPersonView()");

            playerManagerClient.UnitController?.UnitModelController.ActivateFirstPersonView();
            currentMaxVerticalPan = firstPersonMaxVerticalPan;
            currentMinVerticalPan = firstPersonMinVerticalPan;
        }

        private void DeactivateFirstPersonView() {
            Debug.Log("AnyRPGCameraController.DeactivateFirstPersonView()");

            playerManagerClient.UnitController.UnitModelController.DeactivateFirstPersonView();
            currentMaxVerticalPan = maxVerticalPan;
            currentMinVerticalPan = minVerticalPan;
        }

        private void CompensateForWalls() {
            //Debug.Log("drawing Camera debug line from targetPosition: " + targetPosition + " to wantedPosition: " + wantedPosition);
            Debug.DrawLine(targetPosition, wantedPosition, Color.cyan);
            //wallHit = new RaycastHit();
            if (Physics.Linecast(targetPosition, wantedPosition, out wallHit, ~ignoreMask)) {
                //Debug.Log("hit: " + wallHit.transform.name);
                Debug.DrawRay(wallHit.point, wallHit.point - targetPosition, Color.red);
                wantedPosition = new Vector3(wallHit.point.x, wallHit.point.y, wallHit.point.z);
                wantedPosition = Vector3.MoveTowards(wantedPosition, targetPosition, 0.2f);
            }
        }

        private void SetWantedPosition() {
            // 1. Determine if we should "lock" to character's back or stay in world space
            // If we're right-clicking, the character follows the camera (already fixed).
            // If we're moving with keyboard, we want to stay behind/at-offset (standard behavior).
            // If the NavMeshAgent is active and moving, we want the world-relative camera.

            if (!rightCameraPan) {
                if (playerManagerClient.PlayerController != null
                    && systemConfigurationManager.CameraViewMode != CameraViewMode.Isometric
                    && (playerManagerClient.PlayerController.MovementData.HasMoveInput() || playerManagerClient.PlayerController.MovementData.HasTurnInput())
                    && playerManagerClient.ActiveUnitController.UnitProfile.UnitPrefabProps.RotateModel == false) {
                    // STANDARD: Stick to the character's rotation (camera moves with character turns)
                    currentXDegrees = target.eulerAngles.y + userOffsetAngle;
                } else {
                    // NAVMESH MODE and ROTATE MODEL MODE: Do nothing to currentXDegrees. 
                    // The camera will stay at its current world angle while the character turns beneath it.

                    // Re-calculate userOffsetAngle so if you press a key, the camera doesn't snap.
                    if (systemGameManager.GameMode == GameMode.Local) {
                        userOffsetAngle = Mathf.DeltaAngle(target.eulerAngles.y, currentXDegrees);
                    } else {
                        userOffsetAngle = Mathf.DeltaAngle(target.parent.transform.eulerAngles.y, currentXDegrees);
                    }
                }
            }

            // 2. The rest of the math remains exactly as we fixed it before
            Quaternion orbitRotation = Quaternion.Euler(-currentYDegrees, currentXDegrees, 0);
            Vector3 directionToCamera = orbitRotation * Vector3.back;

            wantedPosition = target.position + new Vector3(0, pitch, 0) + (directionToCamera * currentZoomDistance);
            wantedDirection = orbitRotation * Vector3.forward;
        }

        private void JumpToWantedPosition() {
            //Debug.Log($"AnyRPGCameraController.JumpToWantedPosition() wantedPosition: {wantedPosition}");

            transform.position = wantedPosition;
        }

        private void SmoothToWantedPosition() {
            //Debug.Log($"AnyRPGCameraController.SmoothToWantedPosition() wantedPosition: {wantedPosition}");

            transform.position = Vector3.MoveTowards(transform.position, wantedPosition, cameraFollowSpeed);
        }

        private void LookAtTargetPosition() {
            //Debug.Log("AnyRPGCameraController.LookAtTargetPosition()");

            cameraTransformForward = new Vector3(targetPosition.x, 0f, targetPosition.z) - new Vector3(transform.position.x, 0f, transform.position.z);
            if (cameraTransformForward != Vector3.zero) {
                transform.forward = cameraTransformForward;
            }
            cameraTransform.LookAt(targetPosition);
        }

        private void InitializeFollowLocation() {
            //Debug.Log($"AnyRPGCameraController.InitializeFollowLocation()");

            if (systemConfigurationManager.CameraViewMode != CameraViewMode.Isometric) {
                userOffsetAngle = 0f;
                currentXDegrees = target.eulerAngles.y + userOffsetAngle;
            } else {
                // In Isometric, we want to keep the currentXDegrees 
                // derived from your isoMetricVector.
                // We just need to update the offset so if we DO switch modes, it doesn't snap.
                userOffsetAngle = Mathf.DeltaAngle(target.eulerAngles.y, currentXDegrees);
            }
        }

        private void JumpToFollowSpot() {
            //Debug.Log("AnyRPGCameraController.JumpToFollowSpot()");

            // set the initial camera offset vector which will be used as the basis for all future relative camera movement
            InitializeFollowLocation();
            SetWantedPosition();
            JumpToWantedPosition();
            LookAtTargetPosition();
        }

    }

}
