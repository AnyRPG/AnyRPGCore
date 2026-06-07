using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace AnyRPG {

    public class PreviewCameraController : ConfiguredMonoBehaviour, IPointerDownHandler, IScrollHandler {
        
        // events
        public event System.Action OnTargetReady = delegate { };

        protected UnitController unitController = null;

        [SerializeField]
        protected Vector3 cameraLookOffsetDefault = new Vector3(0f, 0.8f, 0f);

        [SerializeField]
        protected Vector3 cameraPositionOffsetDefault = new Vector3(0f, 0.8f, 2f);

        // track the position of the window to ensure we are over it
        protected RectTransform rectTransform;

        //public Vector3 offset;
        //public float rightMouseLookSpeed = 10f;
        public float cameraSpeed = 4f;
        private float gamepadZoomSpeed = 0.05f;
        public float minZoom = 1f;

        // avoid use of local variables
        private RaycastHit wallHit = new RaycastHit();

        [Tooltip("The maximum zoom distance is how far past the initial zoom you can zoom out")]
        [SerializeField]
        private float maxZoomDifference = 5f;

        private float currentMaxZoom = 5f;

        public float maxVerticalPan = 75;
        public float minVerticalPan = -75;

        // to slow down mouse pan
        public float mousePanSpeedDivider = 5f;

        // calculated every frame after apply quaternion rotations to the initial location
        //protected Vector3 currentCameraOffset;

        protected Vector3 initialCameraLookOffset = Vector3.zero;
        protected Vector3 currentCameraLookOffset = Vector3.zero;

        protected Vector3 initialCameraPositionOffset = Vector3.zero;
        protected Vector3 currentCameraPositionOffset = Vector3.zero;

        protected Vector3 initialLookVector = Vector3.zero;

        [Tooltip("Mouse Yaw Speed")]
        public float yawSpeed = 10f;

        [Tooltip("Gamepad analog stick yaw speed")]
        public float analogYawSpeed = 1f;

        // the calculated position we want the camera to go to
        protected Vector3 wantedPosition;

        protected Vector3 wantedLookPosition;

        // the location we want the camera to look at
        //protected Vector3 targetPosition;

        protected Camera currentCamera = null;

        public float initialYDegrees = 0f;
        public float initialXDegrees = 0f;

        /// <summary>
        /// Rotate the target instead of the camera
        /// </summary>
        public bool rotateTarget = false;

        [Tooltip("Ignore these layers when checking if walls are in the way of the camera view of the character")]
        [SerializeField]
        private LayerMask ignoreMask = ~0;

        protected Quaternion initialTargetRotation;

        protected float currentYDegrees = 0f;
        protected float currentXDegrees = 0f;
        protected float adjustedXDegrees = 0f;

        protected float currentZoomDistance = 0f;
        protected float scrollDelta = 0f;

        // keep track if we are panning or zooming this frame
        protected bool cameraPan = false;
        protected bool cameraZoom = false;

        protected bool leftMouseClickedOverThisWindow = false;
        protected bool rightMouseClickedOverThisWindow = false;
        protected bool middleMouseClickedOverThisWindow = false;

        protected Vector3[] worldCorners = new Vector3[4];

        protected Transform followTransform;
        protected bool targetInitialized = false;

        protected bool mouseOutsideWindow = true;

        // keep reference to bone we should be searching for on uma create
        protected string initialTargetString = string.Empty;

        // game manager references
        protected InputManager inputManager = null;
        protected CameraManager cameraManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            rectTransform = gameObject.GetComponent<RectTransform>();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            inputManager = systemGameManager.InputManager;
            cameraManager = systemGameManager.CameraManager;
        }

        public void ClearTarget() {
            //Debug.Log("PreviewCameraController.ClearTarget()");
            UnsubscribeFromModelReady();
            unitController = null;
            followTransform = null;
            initialTargetString = string.Empty;
            DisableCamera();
        }

        public virtual void DisableCamera() {
            //Debug.Log("PreviewCameraController.DisableCamera()");
            if (currentCamera != null) {
                // blank out the display so the next panel that opens doesn't have the previous character rendered for the first frame
                // since the gameObject is not destroyed until the end of the frame, it is necessary to update the culling mask before rendering the blank frame
                int oldMask = currentCamera.cullingMask;
                currentCamera.cullingMask = 0;
                currentCamera.Render();
                currentCamera.cullingMask = oldMask;
                currentCamera.enabled = false;
            }
        }

        public virtual void EnableCamera() {
            if (currentCamera != null) {
                currentCamera.enabled = true;
            }
        }

        public void SetTarget(UnitController unitController) {
            //Debug.Log("PreviewCameraController.SetTarget(" + (unitController == null ? "null" : unitController.gameObject.name) + ")");

            // initial zoom distance is based on offset
            //initialCameraPositionOffset = new Vector3(0, 0, 2);
            //currentZoomDistance = initialCameraPositionOffset.magnitude;

            //Debug.Log("Awake(): currentZoomDistance: " + currentZoomDistance);

            this.unitController = unitController;
            InitializePosition();

            currentZoomDistance = initialLookVector.magnitude;

            //Debug.Log("PreviewCameraController.SetTarget(): currentZoomDistance: " + currentZoomDistance);

            currentYDegrees = initialYDegrees;
            currentXDegrees = initialXDegrees;
            initialTargetRotation = unitController.gameObject.transform.rotation;

            FindFollowTarget();
            EnableCamera();
        }

        public void InitializePosition() {
            //Debug.Log($"{gameObject.name}.PreviewCameraController.InitializePosition()");
            if (unitController == null) {
                //Debug.Log($"{gameObject.name}.UnitFrameController.InitializePosition(): unitController is null");
            }

            if (unitController.UnitProfile.UnitPrefabProps.UnitPreviewProps.UnitPreviewCameraPositionOffset != null) {
                initialCameraPositionOffset = unitController.UnitProfile.UnitPrefabProps.UnitPreviewProps.UnitPreviewCameraPositionOffset;
                //Debug.Log($"{gameObject.name}.UnitFrameController.InitializePosition(): initialCameraPositionOffset from unitController: " + initialCameraPositionOffset);
            } else {
                initialCameraPositionOffset = cameraPositionOffsetDefault;
            }
            currentMaxZoom = initialCameraPositionOffset.z + maxZoomDifference;

            if (unitController.UnitProfile.UnitPrefabProps.UnitPreviewProps.UnitPreviewCameraLookOffset != null) {
                initialCameraLookOffset = unitController.UnitProfile.UnitPrefabProps.UnitPreviewProps.UnitPreviewCameraLookOffset;
            } else {
                initialCameraLookOffset = cameraLookOffsetDefault;
            }
            currentCameraLookOffset = initialCameraLookOffset;

            initialLookVector = initialCameraPositionOffset - initialCameraLookOffset;

            currentCameraPositionOffset = initialLookVector;

            //Debug.Log($"{gameObject.name}.UnitFrameController.InitializePosition() currentCameraPositionOffset: " + currentCameraPositionOffset + "; currentCameraLookOffset: " + currentCameraLookOffset + "; initialLookVector" + initialLookVector);
        }


        public void InitializeCamera(UnitController unitController) {
            //Debug.Log("PreviewCameraController.InitializeCamera(" + (unitController == null ? "null" : unitController.gameObject.name) + ")");
            SetTarget(unitController);
            //JumpToFollowSpot();
        }

        private void LateUpdate() {

            if (!targetInitialized) {
                //Debug.Log("UnitFrameController.Update(). Not initialized yet.  Exiting.");
                return;
            }

            if (unitController == null) {
                // camera has nothing to follow so don't calculate movement
                return;
            }
            rectTransform.GetWorldCorners(worldCorners);
            if (inputManager.rightMouseButtonUp) {
                rightMouseClickedOverThisWindow = false;
            }
            if (inputManager.leftMouseButtonUp) {
                leftMouseClickedOverThisWindow = false;
            }
            if (inputManager.middleMouseButtonUp) {
                middleMouseClickedOverThisWindow = false;
            }
            if (inputManager.mousePosition.x < worldCorners[0].x || inputManager.mousePosition.x > worldCorners[2].x || inputManager.mousePosition.y < worldCorners[0].y || inputManager.mousePosition.y > worldCorners[2].y) {
                mouseOutsideWindow = true;
            } else {
                mouseOutsideWindow = false;
            }

            cameraPan = false;
            cameraZoom = false;

            // ==== MOUSE ZOOM ====
            GetMouseZoom();

            // ==== GAMEPAD ZOOM ====
            if (inputManager.rightAnalogVertical != 0f
                && inputManager.KeyBindWasPressedOrHeld("GAMEPADBUTTONRIGHTSTICK")) {

                currentZoomDistance += (inputManager.rightAnalogVertical * gamepadZoomSpeed * -1);
                currentZoomDistance = Mathf.Clamp(currentZoomDistance, minZoom, currentMaxZoom);
                cameraZoom = true;

            }

            // ==== MOUSE PAN ====
            // disabling mouseOutsideWindow so character doesn't stop rotating as soon as you get to another part of the panel
            if (//!mouseOutsideWindow &&
                (rightMouseClickedOverThisWindow || leftMouseClickedOverThisWindow)
                && (inputManager.rightMouseButtonDown || inputManager.leftMouseButtonDown)) {
                float xInput = inputManager.mouseDeltaX * yawSpeed;
                currentXDegrees += xInput;
                float yInput = inputManager.mouseDeltaY * yawSpeed;
                currentYDegrees += yInput;

                //Debug.Log($"currentYDegrees: {currentYDegrees}; currentXDegrees: {currentXDegrees}; xInput: {xInput}; yInput: {yInput}");
                cameraPan = true;
            }

            // ==== GAMEPAD PAN ====
            if (inputManager.KeyBindWasPressedOrHeld("GAMEPADBUTTONRIGHTSTICK") == false
                && (inputManager.rightAnalogHorizontal != 0 || inputManager.rightAnalogVertical != 0)) {

                if (inputManager.rightAnalogHorizontal != 0) {
                    currentXDegrees += inputManager.rightAnalogHorizontal * analogYawSpeed;
                }

                if (inputManager.rightAnalogVertical != 0) {
                    currentYDegrees += inputManager.rightAnalogVertical * analogYawSpeed;
                }

                cameraPan = true;
            }

            if (cameraPan == true) {
                currentYDegrees = Mathf.Clamp(currentYDegrees, minVerticalPan, maxVerticalPan);
                if (rotateTarget == true) {
                    adjustedXDegrees = currentXDegrees * -1;
                } else {
                    adjustedXDegrees = currentXDegrees;
                }
                Quaternion xQuaternion = Quaternion.AngleAxis(adjustedXDegrees, Vector3.up);
                Quaternion yQuaternion = Quaternion.AngleAxis(currentYDegrees, Vector3.right);
                if (rotateTarget == true) {
                    unitController.transform.rotation = initialTargetRotation * xQuaternion;
                    currentCameraPositionOffset = yQuaternion * initialLookVector;
                } else {
                    currentCameraPositionOffset = xQuaternion * yQuaternion * initialLookVector;
                }
            }

            // move the rotation point away from the center of the target using middle mouse button
            if (!mouseOutsideWindow
                && middleMouseClickedOverThisWindow
                && inputManager.middleMouseButtonDown) {
                //currentCameraPositionOffset = new Vector3(currentCameraPositionOffset.x + (xInput / mousePanSpeedDivider), currentCameraPositionOffset.y - (yInput / mousePanSpeedDivider), currentCameraPositionOffset.z);
                currentCameraLookOffset = new Vector3(currentCameraLookOffset.x + (inputManager.mouseDeltaX / mousePanSpeedDivider), currentCameraLookOffset.y - (inputManager.mouseDeltaY / mousePanSpeedDivider), currentCameraLookOffset.z);
                //Debug.Log("xInput: " + xInput + "; yInput: " + yInput + "; currentTargetOffset: " + currentTargetOffset);
                cameraPan = true;
            }

            // THIS MUST BE DOWN HERE SO ITS UPDATED BASED ON MIDDLE MOUSE PAN
            //SetTargetPosition();

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

            scrollDelta = 0f;
        }

        private void GetMouseZoom() {
            if (scrollDelta == 0f) {
                return;
            }
            currentZoomDistance += (scrollDelta * -1f);
            currentZoomDistance = Mathf.Clamp(currentZoomDistance, minZoom, currentMaxZoom);

            cameraZoom = true;
        }

        private void CompensateForWalls() {
            //Debug.Log("drawing Camera debug line from targetPosition: " + targetPosition + " to wantedPosition: " + wantedPosition);
            Debug.DrawLine(wantedLookPosition, wantedPosition, Color.cyan);
            //wallHit = new RaycastHit();
            if (Physics.Linecast(wantedLookPosition, wantedPosition, out wallHit, ~ignoreMask)) {
                //Debug.Log("hit: " + wallHit.transform.name);
                Debug.DrawRay(wallHit.point, wallHit.point - wantedLookPosition, Color.red);
                wantedPosition = new Vector3(wallHit.point.x, wallHit.point.y, wallHit.point.z);
                wantedPosition = Vector3.MoveTowards(wantedPosition, wantedLookPosition, 0.2f);
            }
        }


        private void SetWantedPosition() {
            //Debug.Log("SetWantedPosition(): targetPosition: " + targetPosition + "; localwanted: " + (currentCameraOffset.normalized * currentZoomDistance));

            if (followTransform == null) {
                return;
            }

            if (rotateTarget == true) {
                wantedPosition = followTransform.position + ((currentCameraPositionOffset.normalized * currentZoomDistance) + currentCameraLookOffset);
            } else {
                wantedPosition = followTransform.TransformPoint((currentCameraPositionOffset.normalized * currentZoomDistance)) + currentCameraLookOffset;
            }

            wantedLookPosition = followTransform.TransformPoint(currentCameraLookOffset);

        }

        public void ResetWantedPosition() {
            //Debug.Log("ResetWantedPosition()");
            currentXDegrees = 0f;
        }

        private void JumpToWantedPosition() {
            //Debug.Log("JumpToWantedPosition()");
            currentCamera.transform.position = wantedPosition;
        }

        private void SmoothToWantedPosition() {
            //Debug.Log("SmoothToWantedPosition(" + wantedPosition + ")");
            currentCamera.transform.position = Vector3.MoveTowards(currentCamera.transform.position, wantedPosition, cameraSpeed);
        }

        private void LookAtTargetPosition() {
            //Debug.Log("AnyRPGCameraController.LookAtTargetPosition()");
            currentCamera.transform.LookAt(wantedLookPosition);
        }

        /*
        private void InitializeFollowLocation() {
            //Debug.Log("CameraController.InitializeFollowLocation()");
            //currentCameraOffset = initialCameraPositionOffset;
            currentCameraPositionOffset = initialCameraPositionOffset;
            //Debug.Log("CameraController.InitializeFollowLocation(): initialCameraLocation in local space: " + initialCameraLocalLocation);
            //Debug.Log("CameraController.InitializeFollowLocation(): initialCameraLocation in world space: " + initialCameraLocation);
            //cameraOffsetVector = initialCameraLocation - targetPosition;
            //Debug.Log("CameraController.InitialCameraLocation(): cameraOffsetVector in local space: " + cameraOffsetVector);
        }
        */


        private void JumpToFollowSpot() {
            //Debug.Log("CameraController.JumpToFollowSpot()");
            //InitializeFollowLocation();
            SetWantedPosition();
            JumpToWantedPosition();
            LookAtTargetPosition();

            // set the initial camera offset vector which will be used as the basis for all future relative camera movement
            //Debug.Log("Camera offset is " + cameraOffsetVector);
        }

        public void HandleModelCreated() {
            //Debug.Log("PreviewCameraController.HandleModelReady()");
            UnsubscribeFromModelReady();
            if (initialTargetString != null && initialTargetString != string.Empty) {
                Transform targetBone = unitController.transform.FindChildByRecursive(initialTargetString);
                if (targetBone == null) {
                    Debug.LogWarning("AnyRPGCharacterPreviewCameraController.HandleCharacterCreated(): Character model is ready and could not find target bone: " + initialTargetString);
                } else {
                    followTransform = targetBone;
                    HandleTargetAvailable();
                    return;
                }
            }
            followTransform = unitController.transform;
            HandleTargetAvailable();

        }

        public void UnsubscribeFromModelReady() {
            //Debug.Log("PreviewCameraController.UnsubscribeFromModelReady()");

            if (unitController?.UnitModelController != null) {
                unitController.UnitModelController.OnModelCreated -= HandleModelCreated;
            }
        }

        public void SubscribeToModelCreated() {
            //Debug.Log("PreviewCameraController.SubscribeToModelReady()");

            if (unitController?.UnitModelController != null) {
                //unitController.UnitModelController.OnModelUpdated += HandleModelReady;
                unitController.UnitModelController.OnModelCreated += HandleModelCreated;
            }
        }

        private void FindFollowTarget() {
            //Debug.Log("PreviewCameraController.FindFollowTarget()");
            Transform targetBone = null;
            Vector3 unitTargetOffset = Vector3.zero;

            if (unitController == null) {
                //Debug.Log("PreviewCameraController.WaitForFollowTarget(): CharacterUnit.GetCharacterUnit(target) is null!!!!");
            } else {
                //Debug.Log("PreviewCameraController.FindFollowTarget(): unitController is not null");
                initialTargetString = unitController.UnitProfile.UnitPrefabProps.UnitPreviewProps.UnitPreviewTarget;
                if (initialTargetString != null && initialTargetString != string.Empty) {
                    targetBone = unitController.transform.FindChildByRecursive(initialTargetString);
                }
                unitTargetOffset = unitController.UnitProfile.UnitPrefabProps.UnitPreviewProps.UnitPreviewCameraLookOffset;
            }
            currentCameraLookOffset = unitTargetOffset;

            if (targetBone == null) {
                //Debug.Log("PreviewCameraController.FindFollowTarget(): targetBone is null");
                // we did not find the target bone.  Either there was an error, or this was an UMA unit that didn't spawn yet.
                if (unitController?.UnitModelController?.ModelCreated == false) {
                    //Debug.Log("PreviewCameraController.FindFollowTarget(): model is not ready yet, subscribing to model ready");
                    SubscribeToModelCreated();
                } else {
                    //Debug.Log("PreviewCameraController.FindFollowTarget(): model is ready");
                    if (initialTargetString != string.Empty) {
                        Debug.LogWarning("AnyRPGCharacterPreviewCameraController.FindFollowTarget(): Could not find bone. Check inspector");
                    }
                    followTransform = unitController.transform;
                    HandleTargetAvailable();
                }
            } else {
                //Debug.Log("PreviewCameraController.FindFollowTarget(): targetBone is not null");
                followTransform = targetBone;
                HandleTargetAvailable();
            }

        }

        public void HandleTargetAvailable() {
            //Debug.Log("PreviewCameraController.HandleTargetAvailable()");
            targetInitialized = true;
            JumpToFollowSpot();
            OnTargetReady();
        }

        public void OnPointerDown(PointerEventData eventData) {
            //Debug.Log("PreviewCameraController.OnPointerDown()");

            // Detect a left click on a slot in a bag
            if (eventData.button == PointerEventData.InputButton.Left) {
                leftMouseClickedOverThisWindow = true;
            }
            if (eventData.button == PointerEventData.InputButton.Middle) {
                middleMouseClickedOverThisWindow = true;
            }
            if (eventData.button == PointerEventData.InputButton.Right) {
                rightMouseClickedOverThisWindow = true;
            }
        }

        public void OnScroll(PointerEventData eventData) {
            //Debug.Log($"{gameObject.name}.PreviewCameraController.OnScroll()");

            scrollDelta += eventData.scrollDelta.y / 24f;
        }


        public void OnDisable() {
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            UnsubscribeFromModelReady();
            StopAllCoroutines();
        }

    }

}