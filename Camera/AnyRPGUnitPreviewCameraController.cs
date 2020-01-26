using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnyRPG {

    public class AnyRPGUnitPreviewCameraController : MonoBehaviour {
        // public variables
        public event System.Action OnTargetReady = delegate { };

        [SerializeField]
        private Transform target = null;

        // track the position of the window to ensure we are over it
        private RectTransform rectTransform;

        //public Vector3 offset;
        //public float rightMouseLookSpeed = 10f;
        public float cameraSpeed = 4f;
        public float minZoom = 1f;
        public float maxZoom = 5f;
        public float maxVerticalPan = 45;
        public float minVerticalPan = -45;

        // to slow down mouse pan
        public float mousePanSpeedDivider = 5f;

        // target offsets, the position relative to the target where the camera will face and all rotations will be performed around
        public Vector3 initialTargetOffset = Vector3.zero;
        private Vector3 currentTargetOffset;

        // camera offset, the position of the camera relative to the target location
        private Vector3 initialCameraOffset;

        // calculated every frame after apply quaternion rotations to the initial location
        private Vector3 currentCameraOffset;

        public float yawSpeed = 10f;

        // the calculated position we want the camera to go to
        private Vector3 wantedPosition;

        // the location we want the camera to look at
        private Vector3 targetPosition;

        public float initialYDegrees = 0f;
        public float initialXDegrees = 0f;

        private float currentYDegrees = 0f;
        private float currentXDegrees = 0f;

        private float currentZoomDistance = 0f;

        // keep track if we are panning or zooming this frame
        private bool cameraPan = false;
        private bool cameraZoom = false;

        private bool leftMouseClickedOverThisWindow = false;
        private bool rightMouseClickedOverThisWindow = false;
        private bool middleMouseClickedOverThisWindow = false;

        private Vector3[] worldCorners = new Vector3[4];

        private Transform followTransform;
        private bool targetInitialized = false;

        private bool mouseOutsideWindow = true;

        public Transform MyTarget { get => target; set => target = value; }

        private void Awake() {
            //Debug.Log("AnyRPGCharacterPreviewCameraController.Awake()");

            rectTransform = gameObject.GetComponent<RectTransform>();

        }

        public void ClearTarget() {
            //Debug.Log("AnyRPGCharacterPreviewCameraController.ClearTarget()");
            target = null;
            followTransform = null;
            CameraManager.MyInstance.MyUnitPreviewCamera.enabled = false;
        }

        public void SetTarget(Transform newTarget) {
            //Debug.Log("AnyRPGCharacterPreviewCameraController.SetTarget(" + newTarget.name + ")");

            // initial zoom distance is based on offset
            initialCameraOffset = new Vector3(0, 0, 2);
            currentZoomDistance = initialCameraOffset.magnitude;

            currentYDegrees = initialYDegrees;
            currentXDegrees = initialXDegrees;
            //Debug.Log("Awake(): currentZoomDistance: " + currentZoomDistance);

            target = newTarget;
            StartCoroutine(WaitForFollowTarget());
            CameraManager.MyInstance.MyUnitPreviewCamera.enabled = true;
        }

        private void SetTargetPosition() {
            //Debug.Log("AnyRPGCharacterPreviewCameraController.SetTargetPosition()");
            if (followTransform != null) {
                targetPosition = followTransform.position + currentTargetOffset;
            }
            //Debug.Log("SetTargetPosition(): currentTargetOffset: " + currentTargetOffset);
        }

        public void InitializeCamera(Transform newTarget) {
            //Debug.Log("AnyRPGCameraController.InitializeCamera(" + newTarget.gameObject.name + ")");
            SetTarget(newTarget);
            //JumpToFollowSpot();
        }

        private void LateUpdate() {

            if (!targetInitialized) {
                //Debug.Log("UnitFrameController.Update(). Not initialized yet.  Exiting.");
                return;
            }

            if (target == null) {
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
                currentCameraOffset = xQuaternion * yQuaternion * initialCameraOffset;
                //Debug.Log("currentYDegrees: " + currentYDegrees + "; currentXDegrees: " + currentXDegrees + "xInput: " + xInput + "; yInput: " + yInput + "; initialCameraOffset: " + initialCameraOffset + "; currentCameraOffset: " + currentCameraOffset);
                cameraPan = true;
            }

            // move the rotation point away from the center of the target using middle mouse button
            if ((!mouseOutsideWindow || middleMouseClickedOverThisWindow) && InputManager.MyInstance.middleMouseButtonDown) {
                //float xInput = Input.GetAxis("Mouse X") * yawSpeed;
                float xInput = Input.GetAxis("Mouse X");
                float yInput = Input.GetAxis("Mouse Y");
                currentTargetOffset = new Vector3(currentTargetOffset.x + (xInput / mousePanSpeedDivider), currentTargetOffset.y - (yInput / mousePanSpeedDivider), currentTargetOffset.z);
                //Debug.Log("xInput: " + xInput + "; yInput: " + yInput + "; currentTargetOffset: " + currentTargetOffset);
                cameraPan = true;
            }

            // THIS MUST BE DOWN HERE SO ITS UPDATED BASED ON MIDDLE MOUSE PAN
            SetTargetPosition();

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
                wantedPosition = followTransform.TransformPoint((currentCameraOffset.normalized * currentZoomDistance)) + currentTargetOffset;
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
            CameraManager.MyInstance.MyUnitPreviewCamera.transform.position = wantedPosition;
        }

        private void SmoothToWantedPosition() {
            //Debug.Log("SmoothToWantedPosition(" + wantedPosition + ")");
            CameraManager.MyInstance.MyUnitPreviewCamera.transform.position = Vector3.MoveTowards(CameraManager.MyInstance.MyUnitPreviewCamera.transform.position, wantedPosition, cameraSpeed);
        }

        private void LookAtTargetPosition() {
            //Debug.Log("AnyRPGCameraController.LookAtTargetPosition()");
            CameraManager.MyInstance.MyUnitPreviewCamera.transform.LookAt(targetPosition);
        }

        private void InitializeFollowLocation() {
            //Debug.Log("CameraController.InitializeFollowLocation()");
            currentCameraOffset = initialCameraOffset;
            //Debug.Log("CameraController.InitializeFollowLocation(): initialCameraLocation in local space: " + initialCameraLocalLocation);
            //Debug.Log("CameraController.InitializeFollowLocation(): initialCameraLocation in world space: " + initialCameraLocation);
            //cameraOffsetVector = initialCameraLocation - targetPosition;
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

        private IEnumerator WaitForFollowTarget() {
            //Debug.Log("WaitForFollowTarget()");
            if (target == null) {
                //Debug.Log("WaitForFollowTarget(): target is null!!!!");
            }
            Transform targetBone = target.transform;
            string initialTargetString = string.Empty;
            Vector3 unitTargetOffset = initialTargetOffset;

            if (target.GetComponent<CharacterUnit>() == null) {
                //Debug.Log("WaitForFollowTarget(): target.GetComponent<CharacterUnit>() is null!!!!");
            } else {
                //Debug.Log("WaitForFollowTarget(): target.GetComponent<CharacterUnit>(): " + target.GetComponent<CharacterUnit>().MyDisplayName);
                initialTargetString = target.GetComponent<CharacterUnit>().MyUnitFrameTarget;
                if (target.GetComponent<CharacterUnit>().MyPlayerPreviewTarget != string.Empty) {
                    targetBone = target.transform.FindChildByRecursive(target.GetComponent<CharacterUnit>().MyPlayerPreviewTarget);
                }
                unitTargetOffset = target.GetComponent<CharacterUnit>().MyPlayerPreviewInitialOffset;
            }

            if (initialTargetString != string.Empty) {
                //Debug.Log("WaitForFollowTarget(): searching for unitFrameTarget: " + initialTargetString);
                while (target.transform.FindChildByRecursive(initialTargetString) == null) {
                    //targetBone = target.transform.Find(initialTargetString);
                    //if (targetBone == null) {
                    yield return null;
                    /*} else {
                        Debug.Log("WaitForFollowTarget(): found unitFrameTarget: " + initialTargetString);
                        break;
                    }*/
                }
                currentTargetOffset = unitTargetOffset;
            } else {
                currentTargetOffset = unitTargetOffset;
            }
            followTransform = targetBone;
            targetInitialized = true;
            JumpToFollowSpot();
            OnTargetReady();
        }

        public void OnDisable() {
            StopAllCoroutines();
        }
    }

}