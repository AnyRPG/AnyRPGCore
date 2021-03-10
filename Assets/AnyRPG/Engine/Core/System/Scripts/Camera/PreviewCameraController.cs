using AnyRPG;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnyRPG {

    public class PreviewCameraController : MonoBehaviour, IPointerDownHandler {
        // public variables
        public event System.Action OnTargetReady = delegate { };

        //[SerializeField]
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
        public float minZoom = 1f;

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

        public float yawSpeed = 10f;

        // the calculated position we want the camera to go to
        protected Vector3 wantedPosition;

        protected Vector3 wantedLookPosition;

        // the location we want the camera to look at
        //protected Vector3 targetPosition;

        protected Camera currentCamera = null;

        public float initialYDegrees = 0f;
        public float initialXDegrees = 0f;

        protected float currentYDegrees = 0f;
        protected float currentXDegrees = 0f;

        protected float currentZoomDistance = 0f;

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

        protected virtual void Awake() {
            //Debug.Log("AnyRPGCharacterPreviewCameraController.Awake()");

            rectTransform = gameObject.GetComponent<RectTransform>();
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

            FindFollowTarget();
            EnableCamera();
        }

        public void InitializePosition() {
            //Debug.Log(gameObject.name + ".PreviewCameraController.InitializePosition()");
            if (unitController == null) {
                //Debug.Log(gameObject.name + ".UnitFrameController.InitializePosition(): unitController is null");
            }
            if (unitController.NamePlateController == null) {
                //Debug.Log(gameObject.name + ".UnitFrameController.InitializePosition(): unitController.NamePlateController is null");
            }

            if (unitController.NamePlateController.UnitPreviewCameraPositionOffset != null) {
                initialCameraPositionOffset = unitController.NamePlateController.UnitPreviewCameraPositionOffset;
                //Debug.Log(gameObject.name + ".UnitFrameController.InitializePosition(): initialCameraPositionOffset from unitController: " + initialCameraPositionOffset);
            } else {
                initialCameraPositionOffset = cameraPositionOffsetDefault;
            }
            currentMaxZoom = initialCameraPositionOffset.z + maxZoomDifference;

            if (unitController.NamePlateController.UnitPreviewCameraLookOffset != null) {
                initialCameraLookOffset = unitController.NamePlateController.UnitPreviewCameraLookOffset;
            } else {
                initialCameraLookOffset = cameraLookOffsetDefault;
            }
            currentCameraLookOffset = initialCameraLookOffset;

            initialLookVector = initialCameraPositionOffset - initialCameraLookOffset;

            currentCameraPositionOffset = initialLookVector;

            //Debug.Log(gameObject.name + ".UnitFrameController.InitializePosition() currentCameraPositionOffset: " + currentCameraPositionOffset + "; currentCameraLookOffset: " + currentCameraLookOffset + "; initialLookVector" + initialLookVector);
        }


        public void InitializeCamera(UnitController unitController) {
            //Debug.Log("AnyRPGCameraController.InitializeCamera(" + (unitController == null ? "null" : unitController.gameObject.name) + ")");
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
            Vector3 mousePosition = Input.mousePosition;
            if (InputManager.MyInstance.rightMouseButtonUp) {
                rightMouseClickedOverThisWindow = false;
            }
            if (InputManager.MyInstance.leftMouseButtonUp) {
                leftMouseClickedOverThisWindow = false;
            }
            if (InputManager.MyInstance.middleMouseButtonUp) {
                middleMouseClickedOverThisWindow = false;
            }
            /*
            for (var i = 0; i < 4; i++) {
                //Debug.Log("World Corner " + i + " : " + worldCorners[i]);
            }
            */
            if (mousePosition.x < worldCorners[0].x || mousePosition.x > worldCorners[2].x || mousePosition.y < worldCorners[0].y || mousePosition.y > worldCorners[2].y) {
                mouseOutsideWindow = true;
                //Debug.Log("mouse scroll was outside of onscreen bounds.  ignoring!");
                /*
                if (!rightMouseClickedOverThisWindow && !leftMouseClickedOverThisWindow) {
                    return;
                }
                */
            } else {
                mouseOutsideWindow = false;
                /*
                if (InputManager.MyInstance.rightMouseButtonDown) {
                    rightMouseClickedOverThisWindow = true;
                }
                if (InputManager.MyInstance.leftMouseButtonDown) {
                    leftMouseClickedOverThisWindow = true;
                }
                if (InputManager.MyInstance.middleMouseButtonDown) {
                    middleMouseClickedOverThisWindow = true;
                }
                */
            }

            cameraPan = false;
            cameraZoom = false;

            // handleZoom
            if (!mouseOutsideWindow && InputManager.MyInstance.mouseScrolled) {
                //Debug.Log("Mouse Scrollwheel: " + Input.GetAxis("Mouse ScrollWheel"));
                currentZoomDistance += (Input.GetAxis("Mouse ScrollWheel") * cameraSpeed * -1);
                currentZoomDistance = Mathf.Clamp(currentZoomDistance, minZoom, currentMaxZoom);
                cameraZoom = true;
            }

            // pan with the left or turn with the right mouse button
            if (!mouseOutsideWindow
                && (rightMouseClickedOverThisWindow || leftMouseClickedOverThisWindow)
                && (InputManager.MyInstance.rightMouseButtonDown || InputManager.MyInstance.leftMouseButtonDown)) {
                float xInput = Input.GetAxis("Mouse X") * yawSpeed;
                currentXDegrees += xInput;
                Quaternion xQuaternion = Quaternion.AngleAxis(currentXDegrees, Vector3.up);
                //Debug.Log("xInput: " + xInput + "; currentXDegrees: " + currentXDegrees + "; xQuaternion: " + xQuaternion);
                //Quaternion camTurnAngle = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * yawSpeed, Vector3.up);
                //cameraOffsetVector = camTurnAngle * cameraOffsetVector;
                //Debug.Log("Camera Offset Vector after rotationX: " + cameraOffsetVector);
                //camTurnAngle = Quaternion.AngleAxis(-Input.GetAxis("Mouse Y") * yawSpeed, transform.right);
                float yInput = Input.GetAxis("Mouse Y") * yawSpeed;
                currentYDegrees += yInput;
                currentYDegrees = Mathf.Clamp(currentYDegrees, minVerticalPan, maxVerticalPan);
                Quaternion yQuaternion = Quaternion.AngleAxis(currentYDegrees, Vector3.right);
                //currentCameraOffset = xQuaternion * yQuaternion * initialCameraPositionOffset;
                //currentCameraPositionOffset = xQuaternion * yQuaternion * initialCameraPositionOffset;
                //currentCameraPositionOffset = xQuaternion * yQuaternion * initialCameraLookOffset;
                currentCameraPositionOffset = xQuaternion * yQuaternion * initialLookVector;
                //Debug.Log("currentYDegrees: " + currentYDegrees + "; currentXDegrees: " + currentXDegrees + "xInput: " + xInput + "; yInput: " + yInput + "; initialCameraOffset: " + initialCameraOffset + "; currentCameraOffset: " + currentCameraOffset);
                cameraPan = true;
            }

            // move the rotation point away from the center of the target using middle mouse button
            if (!mouseOutsideWindow
                && middleMouseClickedOverThisWindow
                && InputManager.MyInstance.middleMouseButtonDown) {
                //float xInput = Input.GetAxis("Mouse X") * yawSpeed;
                float xInput = Input.GetAxis("Mouse X");
                float yInput = Input.GetAxis("Mouse Y");
                //currentCameraPositionOffset = new Vector3(currentCameraPositionOffset.x + (xInput / mousePanSpeedDivider), currentCameraPositionOffset.y - (yInput / mousePanSpeedDivider), currentCameraPositionOffset.z);
                currentCameraLookOffset = new Vector3(currentCameraLookOffset.x + (xInput / mousePanSpeedDivider), currentCameraLookOffset.y - (yInput / mousePanSpeedDivider), currentCameraLookOffset.z);
                //Debug.Log("xInput: " + xInput + "; yInput: " + yInput + "; currentTargetOffset: " + currentTargetOffset);
                cameraPan = true;
            }

            // THIS MUST BE DOWN HERE SO ITS UPDATED BASED ON MIDDLE MOUSE PAN
            //SetTargetPosition();

            // follow the player
            //if (hasMoved || cameraZoom || (cameraPan && !InputManager.MyInstance.rightMouseButtonClickedOverUI && !InputManager.MyInstance.leftMouseButtonClickedOverUI) ) {
            SetWantedPosition();
            //}

            //CompensateForWalls();
            if (cameraZoom || cameraPan) {
                //Debug.Log("Camera was zoomed or panned.  Jumping to Wanted Position.");
                JumpToWantedPosition();
            } else {
                // camera is just doing a regular follow, smoother motion is good
                SmoothToWantedPosition();
            }
            LookAtTargetPosition();
        }

        private void SetWantedPosition() {
            //Debug.Log("SetWantedPosition(): targetPosition: " + targetPosition + "; localwanted: " + (currentCameraOffset.normalized * currentZoomDistance));
            if (followTransform != null) {
                //wantedPosition = followTransform.TransformPoint((currentCameraOffset.normalized * currentZoomDistance)) + currentCameraPositionOffset;
                //wantedPosition = followTransform.TransformPoint((currentCameraPositionOffset.normalized * currentZoomDistance)) + initialCameraPositionOffset;
                //wantedPosition = followTransform.TransformPoint((currentCameraPositionOffset.normalized * currentZoomDistance));
                wantedPosition = followTransform.TransformPoint((currentCameraPositionOffset.normalized * currentZoomDistance)) + currentCameraLookOffset;

                wantedLookPosition = followTransform.TransformPoint(currentCameraLookOffset);
            } else {
                //Debug.Log("SetWantedPosition(): targetPosition: " + targetPosition + "; localwanted: " + (currentCameraOffset.normalized * currentZoomDistance));
            }
            //Debug.Log("SetWantedPosition(): currentTargetOffset: " + currentTargetOffset + "; wantedPosition: " + wantedPosition);
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

        public void HandleModelReady() {
            //Debug.Log("PreviewCameraController.HandleModelReady()");
            UnsubscribeFromModelReady();
            if (initialTargetString != string.Empty) {
                Transform targetBone = unitController.transform.FindChildByRecursive(initialTargetString);
                if (targetBone == null) {
                    Debug.LogWarning("AnyRPGCharacterPreviewCameraController.HandleCharacterCreated(): UMA is ready and could not find target bone: " + initialTargetString);
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

            if (unitController != null) {
                unitController.OnModelReady -= HandleModelReady;
            }
        }

        public void SubscribeToModelReady() {
            //Debug.Log("PreviewCameraController.SubscribeToModelReady()");

            if (unitController != null) {
                unitController.OnModelReady += HandleModelReady;
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
                initialTargetString = unitController.NamePlateController.UnitPreviewTarget;
                if (initialTargetString != string.Empty) {
                    targetBone = unitController.transform.FindChildByRecursive(initialTargetString);
                }
                unitTargetOffset = unitController.NamePlateController.UnitPreviewCameraLookOffset;
            }
            currentCameraLookOffset = unitTargetOffset;

            if (targetBone == null) {
                //Debug.Log("PreviewCameraController.FindFollowTarget(): targetBone is null");
                // we did not find the target bone.  Either there was an error, or this was an UMA unit that didn't spawn yet.
                if (unitController.ModelReady == false) {
                    //Debug.Log("PreviewCameraController.FindFollowTarget(): model is not ready yet, subscribing to model ready");
                    SubscribeToModelReady();
                } else {
                    //Debug.Log("PreviewCameraController.FindFollowTarget(): model is ready");
                    if (initialTargetString != string.Empty) {
                        Debug.LogWarning("AnyRPGCharacterPreviewCameraController.FindFollowTarget(): Character was not UMA and could not find bone. Check inspector");
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

        public void OnDisable() {
            UnsubscribeFromModelReady();
            StopAllCoroutines();
        }
    }

}