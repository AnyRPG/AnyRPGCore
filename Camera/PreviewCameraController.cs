using AnyRPG;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;

namespace AnyRPG {

    public class PreviewCameraController : MonoBehaviour {
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
        public float maxZoom = 10f;
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

        // reference to the dynamic character avatar on the mount, if it exists
        protected DynamicCharacterAvatar dynamicCharacterAvatar = null;

        // keep reference to bone we should be searching for on uma create
        protected string initialTargetString = string.Empty;

        protected virtual void Awake() {
            //Debug.Log("AnyRPGCharacterPreviewCameraController.Awake()");

            rectTransform = gameObject.GetComponent<RectTransform>();

        }

        public void ClearTarget() {
            //Debug.Log("AnyRPGCharacterPreviewCameraController.ClearTarget()");
            unitController = null;
            followTransform = null;
            dynamicCharacterAvatar = null;
            initialTargetString = string.Empty;
            DisableCamera();
        }

        public virtual void DisableCamera() {
            if (currentCamera != null) {
                currentCamera.enabled = false;
            }
        }

        public virtual void EnableCamera() {
            if (currentCamera != null) {
                currentCamera.enabled = true;
            }
        }

        public void SetTarget(UnitController unitController) {
            //Debug.Log("AnyRPGCharacterPreviewCameraController.SetTarget(" + newTarget.name + ")");

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
            //Debug.Log(gameObject.name + ".UnitFrameController.InitializePosition()");
            if (unitController.NamePlateController.UnitPreviewCameraPositionOffset != null) {
                initialCameraPositionOffset = unitController.NamePlateController.UnitPreviewCameraPositionOffset;
            } else {
                initialCameraPositionOffset = cameraPositionOffsetDefault;
            }

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
            //Debug.Log("AnyRPGCameraController.InitializeCamera(" + newTarget.gameObject.name + ")");
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
                if (InputManager.MyInstance.rightMouseButtonDown) {
                    rightMouseClickedOverThisWindow = true;
                }
                if (InputManager.MyInstance.leftMouseButtonDown) {
                    leftMouseClickedOverThisWindow = true;
                }
                if (InputManager.MyInstance.middleMouseButtonDown) {
                    middleMouseClickedOverThisWindow = true;
                }
            }

            cameraPan = false;
            cameraZoom = false;

            // handleZoom
            if (!mouseOutsideWindow && InputManager.MyInstance.mouseScrolled) {
                //Debug.Log("Mouse Scrollwheel: " + Input.GetAxis("Mouse ScrollWheel"));
                currentZoomDistance += (Input.GetAxis("Mouse ScrollWheel") * cameraSpeed * -1);
                currentZoomDistance = Mathf.Clamp(currentZoomDistance, minZoom, maxZoom);
                cameraZoom = true;
            }

            // pan with the left or turn with the right mouse button
            if ((!mouseOutsideWindow || rightMouseClickedOverThisWindow || leftMouseClickedOverThisWindow) && (InputManager.MyInstance.rightMouseButtonDown || InputManager.MyInstance.leftMouseButtonDown)) {
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
            if ((!mouseOutsideWindow || middleMouseClickedOverThisWindow) && InputManager.MyInstance.middleMouseButtonDown) {
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

        public void HandleCharacterCreated(UMAData umaData) {
            //Debug.Log("PreviewCameraController.HandleCharacterCreated(): " + umaData);
            UnsubscribeFromUMACreate();
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

        public void UnsubscribeFromUMACreate() {
            if (dynamicCharacterAvatar != null) {
                dynamicCharacterAvatar.umaData.OnCharacterCreated -= HandleCharacterCreated;
                //dynamicCharacterAvatar.umaData.OnCharacterUpdated -= HandleCharacterUpdated;
            }
        }

        public void SubscribeToUMACreate() {

            // is this stuff necessary on ai characters?
            
            UnitController unitController = dynamicCharacterAvatar.gameObject.GetComponent<UnitController>();
            if (unitController != null && unitController.UnitAnimator != null) {
                unitController.UnitAnimator.InitializeAnimator();
            } else {

            }
            dynamicCharacterAvatar.Initialize();
            
            // is this stuff necessary end

            UMAData umaData = dynamicCharacterAvatar.umaData;
            umaData.OnCharacterCreated += HandleCharacterCreated;
            umaData.OnCharacterBeforeDnaUpdated += HandleCharacterBeforeDnaUpdated;
            umaData.OnCharacterBeforeUpdated += HandleCharacterBeforeUpdated;
            umaData.OnCharacterDnaUpdated += HandleCharacterDnaUpdated;
            umaData.OnCharacterDestroyed += HandleCharacterDestroyed;
            umaData.OnCharacterUpdated += HandleCharacterUpdated;

        }

        public void HandleCharacterBeforeDnaUpdated(UMAData umaData) {
            //Debug.Log("PreviewCameraController.BeforeDnaUpdated(): " + umaData);
        }
        public void HandleCharacterBeforeUpdated(UMAData umaData) {
            //Debug.Log("PreviewCameraController.OnCharacterBeforeUpdated(): " + umaData);
        }
        public void HandleCharacterDnaUpdated(UMAData umaData) {
            //Debug.Log("PreviewCameraController.OnCharacterDnaUpdated(): " + umaData);
        }
        public void HandleCharacterDestroyed(UMAData umaData) {
            //Debug.Log("PreviewCameraController.OnCharacterDestroyed(): " + umaData);
        }
        public void HandleCharacterUpdated(UMAData umaData) {
            //Debug.Log("PreviewCameraController.HandleCharacterUpdated(): " + umaData + "; frame: " + Time.frameCount);
            //HandleCharacterCreated(umaData);
        }


        private void FindFollowTarget() {
            //Debug.Log("CharacterPreviewCameraController.FindFollowTarget()");
            if (unitController == null) {
                //Debug.Log("WaitForFollowTarget(): target is null!!!!");
            }
            Transform targetBone = null;
            Vector3 unitTargetOffset = Vector3.zero;

            UnitController targetUnitController = null;
            targetUnitController = unitController.GetComponent<UnitController>();
            if (targetUnitController == null) {
                //Debug.Log("WaitForFollowTarget(): target.GetComponent<CharacterUnit>() is null!!!!");
            } else {
                //Debug.Log("WaitForFollowTarget(): target.GetComponent<CharacterUnit>(): " + target.GetComponent<CharacterUnit>().MyDisplayName);
                initialTargetString = targetUnitController.NamePlateController.PlayerPreviewTarget;
                if (initialTargetString != string.Empty) {
                    targetBone = unitController.transform.FindChildByRecursive(initialTargetString);
                }
                unitTargetOffset = targetUnitController.NamePlateController.UnitPreviewCameraLookOffset;
            }
            currentCameraLookOffset = unitTargetOffset;

            if (targetBone == null) {
                // we did not find the target bone.  Either there was an error, or this was an UMA unit that didn't spawn yet.
                dynamicCharacterAvatar = unitController.GetComponent<DynamicCharacterAvatar>();
                if (dynamicCharacterAvatar != null) {
                    SubscribeToUMACreate();
                } else {
                    if (initialTargetString != string.Empty) {
                        Debug.LogWarning("AnyRPGCharacterPreviewCameraController.FindFollowTarget(): Character was not UMA and could not find bone. Check inspector");
                    }
                    followTransform = unitController.transform;
                    HandleTargetAvailable();
                }
            } else {
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

        public void OnDisable() {
            StopAllCoroutines();
        }
    }

}