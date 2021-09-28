using AnyRPG;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnyRPG {
    public class AnyRPGCameraController : ConfiguredMonoBehaviour {
        // public variables

        [SerializeField]
        private Transform target = null;

        [Tooltip("Ignore these layers when checking if walls are in the way of the camera view of the character")]
        [SerializeField]
        private LayerMask ignoreMask = ~0;

        public float cameraFollowSpeed = 10f;
        public float zoomSpeed = 4f;
        public float minZoom = 2f;
        public float maxZoom = 15f;
        public float maxVerticalPan = 45;
        public float minVerticalPan = -45;

        public float initialYDegrees = 0f;

        public float initialYOffset = 1f;
        public float initialZOffset = 4f;

        public float pitch = 2f;
        public float yawSpeed = 10;

        public bool followBehind = true;

        // private variables
        private Vector3 wantedPosition;
        private Vector3 cameraOffsetVector;
        private Vector3 lastPlayerPosition;
        private Vector3 targetPosition;
        private Vector3 initialCameraOffset;

        // needed in case player is holding right mouse down while camera is moving around to avoid obstacles, which can cause the forward direction of movement to get skewed
        private Vector3 wantedDirection;

        private float currentZoomDistance = 0f;
        private float currentYDegrees = 0f;
        private float currentXDegrees = 0f;

        //private bool hasMoved = false;
        // keep track if we are panning or zooming this frame
        private bool cameraPan = false;
        private bool cameraZoom = false;

        // avoid use of local variables
        private RaycastHit wallHit = new RaycastHit();

        // game manager references
        protected InputManager inputManager = null;
        protected NamePlateManager namePlateManager = null;
        protected UIManager uIManager = null;

        public Transform Target { get => target; set => target = value; }
        public Vector3 WantedDirection { get => wantedDirection; set => wantedDirection = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            inputManager = systemGameManager.InputManager;
            uIManager = systemGameManager.UIManager;
            namePlateManager = uIManager.NamePlateManager;
        }

        private void Awake() {
            //Debug.Log("AnyRPGCameraController.Awake()");
            //currentZoomDistance = new Vector3(0, initialYOffset, initialZOffset).magnitude;
            currentZoomDistance = initialZOffset;
            currentYDegrees = initialYDegrees;
            //Debug.Log("Awake(): currentZoomDistance: " + currentZoomDistance);
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
            //Debug.Log("AnyRPGCameraController.SetTargetPosition(" + rawTargetPosition + ", " + forwardDirection + ")");

            // reset the camera to directly behind the player at the previous zoom level
            currentXDegrees = 0f;
            currentYDegrees = 0f;
            currentZoomDistance = initialZOffset;

            targetPosition = rawTargetPosition + Vector3.up * pitch;
            transform.position = targetPosition - (Quaternion.LookRotation(forwardDirection) * new Vector3(0, 0, currentZoomDistance));

            // this next line will give an exact duplicate of where the camera was in relation to the player at the time they changed scenes
            // it's disabled for now because it actually makes a bit more sense to have the camera behind the player so they can see the level they are zoning into
            //transform.position = rawTargetPosition + Quaternion.LookRotation(forwardDirection) * (Quaternion.LookRotation(Vector3.forward) * ((cameraOffsetVector.normalized * currentZoomDistance) + new Vector3(0, pitch, 0)));
            LookAtTargetPosition();
            //Debug.Log("SetTargetPosition(): " + targetPosition);
        }

        public void InitializeCamera(Transform newTarget) {
            //Debug.Log("AnyRPGCameraController.InitializeCamera(" + newTarget.gameObject.name + ")");
            SetTarget(newTarget);
            JumpToFollowSpot();
            lastPlayerPosition = target.position;
        }

        private void LateUpdate() {
            if (target == null) {
                // camera has nothing to follow so don't calculate movement
                return;
            }
            //Debug.Log("CameraController.LateUpdate(): frame " + Time.frameCount);


            /*
            if (systemConfigurationManager != null && systemConfigurationManager.MyUseThirdPartyCameraControl == true) {
                return;
            }
            */

            cameraPan = false;
            cameraZoom = false;

            // keep track of the player's current and previous position
            /*
            if (lastPlayerPosition == target.position) {
                hasMoved = false;
            } else {
                hasMoved = true;
            }
            */
            lastPlayerPosition = target.position;
            SetTargetPosition();

            // handleZoom
            // added code at end to check if over nameplate and allow scrolling
            //if (inputManager.mouseScrolled && (!EventSystem.current.IsPointerOverGameObject() || (namePlateManager != null ? namePlateManager.MouseOverNamePlate() : false))) {
            if (inputManager.mouseScrolled
                && (!EventSystem.current.IsPointerOverGameObject() || namePlateManager.MouseOverNamePlate())) {
                //Debug.Log("Mouse Scrollwheel: " + Input.GetAxis("Mouse ScrollWheel"));
                currentZoomDistance += (Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * -1);
                currentZoomDistance = Mathf.Clamp(currentZoomDistance, minZoom, maxZoom);
                cameraZoom = true;
            }

            // pan with the left or turn with the right mouse button
            // IF START HAVING MORE ISSUES WITH PAN AND TURN IN FUTURE, JUST COMMENT BELOW LINE AND RE-ENABLE COMMENTED LINE BELOW IT SINCE QUATERNIONS ARE NOW ALWAYS CALCULATED IN SETWANTEDPOSITION
            if (!uIManager.DragInProgress && ((inputManager.rightMouseButtonDown && !inputManager.rightMouseButtonClickedOverUI) || (inputManager.leftMouseButtonDown && !inputManager.leftMouseButtonClickedOverUI))) {
                //if (!uIManager.MyDragInProgress && ((inputManager.rightMouseButtonDown && !inputManager.rightMouseButtonClickedOverUI && inputManager.rightMouseButtonDownPosition != Input.mousePosition) || (inputManager.leftMouseButtonDown && !inputManager.leftMouseButtonClickedOverUI && inputManager.leftMouseButtonDownPosition != Input.mousePosition))) {
                //float xInput = Input.GetAxis("Mouse X") * yawSpeed;
                float usedTurnSpeed = 0f;
                if (inputManager.rightMouseButtonDown) {
                    usedTurnSpeed = PlayerPrefs.GetFloat("MouseTurnSpeed") + 0.5f;
                } else {
                    usedTurnSpeed = PlayerPrefs.GetFloat("MouseLookSpeed") + 0.5f;
                }
                //currentXDegrees += xInput * usedTurnSpeed;
                currentXDegrees += Input.GetAxis("Mouse X") * yawSpeed * usedTurnSpeed;
                //Debug.Log("xInput: " + xInput + "; currentXDegrees: " + currentXDegrees + "; xQuaternion: " + xQuaternion);
                //Quaternion camTurnAngle = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * yawSpeed, Vector3.up);
                //cameraOffsetVector = camTurnAngle * cameraOffsetVector;
                //Debug.Log("Camera Offset Vector after rotationX: " + cameraOffsetVector);
                //camTurnAngle = Quaternion.AngleAxis(-Input.GetAxis("Mouse Y") * yawSpeed, transform.right);
                //float yInput = Input.GetAxis("Mouse Y") * yawSpeed;
                //currentYDegrees += (yInput * usedTurnSpeed) * (PlayerPrefs.GetInt("MouseInvert") == 0 ? 1 : -1);
                currentYDegrees += (Input.GetAxis("Mouse Y") * yawSpeed * usedTurnSpeed) * (PlayerPrefs.GetInt("MouseInvert") == 0 ? 1 : -1);
                currentYDegrees = Mathf.Clamp(currentYDegrees, minVerticalPan, maxVerticalPan);

                cameraPan = true;

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
            transform.LookAt(targetPosition);

            SystemEventManager.TriggerEvent("AfterCameraUpdate", new EventParamProperties());
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
            //Debug.Log("SetWantedPosition(): targetPosition: " + targetPosition);

            /*
            Quaternion xQuaternion = Quaternion.AngleAxis(currentXDegrees, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis(currentYDegrees, -Vector3.right);
            cameraOffsetVector = xQuaternion * yQuaternion * initialCameraOffset;
            */
            // reduce use of local variables
            cameraOffsetVector = Quaternion.AngleAxis(currentXDegrees, Vector3.up) * Quaternion.AngleAxis(currentYDegrees, -Vector3.right) * initialCameraOffset;

            wantedPosition = target.TransformPoint((cameraOffsetVector.normalized * currentZoomDistance) + new Vector3(0, pitch, 0));
            //Debug.Log("SetWantedPosition(): wantedPosition: " + wantedPosition + "; currentZoomDistance: " + currentZoomDistance);
            //wantedDirection = (target.TransformPoint(Vector3.zero) - wantedPosition).normalized;
            wantedDirection = (target.transform.position - wantedPosition).normalized;
            //Debug.Log("SetWantedPosition(): wantedPosition: " + wantedPosition + "; currentZoomDistance: " + currentZoomDistance + ";wantedDirection: " + wantedDirection);
        }

        public void ResetWantedPosition() {
            //Debug.Log("ResetWantedPosition()");
            currentXDegrees = 0f;
            //SetWantedPosition();
        }

        private void JumpToWantedPosition() {
            //Debug.Log("AnyRPGCameraController.JumpToWantedPosition()");
            transform.position = wantedPosition;
        }

        private void SmoothToWantedPosition() {
            //Debug.Log("SmoothToWantedPosition(" + wantedPosition + ")");
            transform.position = Vector3.MoveTowards(transform.position, wantedPosition, cameraFollowSpeed);
        }

        private void LookAtTargetPosition() {
            //Debug.Log("AnyRPGCameraController.LookAtTargetPosition()");
            transform.LookAt(targetPosition);
        }

        private void InitializeFollowLocation() {
            //Debug.Log("AnyRPGCameraController.InitializeFollowLocation()");
            Vector3 initialCameraLocalLocation = new Vector3(0, pitch, -initialZOffset);
            //Debug.Log("CameraController.InitializeFollowLocation(): initialCameraLocation in local space: " + initialCameraLocalLocation);
            Vector3 initialCameraLocation = target.TransformPoint(initialCameraLocalLocation);
            //Debug.Log("CameraController.InitializeFollowLocation(): initialCameraLocation in world space: " + initialCameraLocation);
            //cameraOffsetVector = initialCameraLocation - targetPosition;
            cameraOffsetVector = target.InverseTransformPoint(initialCameraLocation) - target.InverseTransformPoint(targetPosition);
            initialCameraOffset = cameraOffsetVector;
            //Debug.Log("CameraController.InitialCameraLocation(): cameraOffsetVector in local space: " + cameraOffsetVector);
        }

        private void JumpToFollowSpot() {
            //Debug.Log("CameraController.JumpToFollowSpot()");
            InitializeFollowLocation();
            SetWantedPosition();
            JumpToWantedPosition();
            LookAtTargetPosition();

            // set the initial camera offset vector which will be used as the basis for all future relative camera movement
            //Debug.Log("Camera offset is " + cameraOffsetVector);
        }

    }

}
