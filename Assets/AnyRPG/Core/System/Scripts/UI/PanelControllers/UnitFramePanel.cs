using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class UnitFramePanel : UnitFramePanelBase {

        [SerializeField]
        private PreviewManager previewManager = null;

        [SerializeField]
        private bool spawnPreviewUnit = false;

        // the next 2 things need to be updated to focus on the right character
        [SerializeField]
        protected Transform cameraTransform = null;

        // replaces cameraTransform;
        [SerializeField]
        protected Camera previewCamera = null;

        [SerializeField]
        protected RawImage portraitSnapshotImage = null;

        [SerializeField]
        protected Vector3 cameraLookOffsetDefault = new Vector3(0, 1.6f, 0);

        [SerializeField]
        protected Vector3 cameraPositionOffsetDefault = new Vector3(0, 1.6f, 0.66f);

        protected Vector3 cameraLookOffset = Vector3.zero;

        protected Vector3 cameraPositionOffset = Vector3.zero;

        protected Coroutine waitForCameraCoroutine = null;

        // avoid GC by using global variables for these
        protected Vector3 wantedPosition = Vector3.zero;
        protected Vector3 wantedLookPosition = Vector3.zero;


        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            if (previewCamera != null) {
                previewCamera.enabled = false;
            }
        }

        protected override void ProcessInitializeController() {
            base.ProcessInitializeController();
            portraitSnapshotImage.texture = portraitTexture;
        }

        private void LateUpdate() {
            if (systemConfigurationManager.UIConfiguration.RealTimeUnitFrameCamera) {
                UpdateCameraPosition();
            }
        }

        public override void ConfigurePortrait(Sprite icon) {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.ConfigurePortrait()");

            base.ConfigurePortrait(icon);
            portraitSnapshotImage.gameObject.SetActive(false);
        }

        public void ConfigureSnapshotPortrait() {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.ConfigureSnapshotPortrait()");

            portraitImage.gameObject.SetActive(false);
            portraitSnapshotImage.gameObject.SetActive(true);
            if (previewManager.UnitController.CameraTargetReady) {
                HandleTargetReady();
            }// else {
             // testing subscribe no matter what in case unit appearance changes
            SubscribeToTargetReady();
            //}
        }

        public override void SetTarget(UnitController unitController) {
            base.SetTarget(unitController);
            if (systemConfigurationManager.UIConfiguration.RealTimeUnitFrameCamera == true && previewCamera != null) {
                previewCamera.enabled = true;
            }
        }

        public override void ClearTarget(bool closeWindowOnClear = true) {
            if (waitForCameraCoroutine != null) {
                StopCoroutine(waitForCameraCoroutine);
            }
            UnsubscribeFromTargetReady();

            base.ClearTarget();
            if (previewCamera != null) {
                previewCamera.enabled = false;
            }
            if (spawnPreviewUnit && previewManager.UnitController != null) {
                previewManager.DespawnUnit();
            }
        }

        public void OnDisable() {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.OnDisable(): {GetInstanceID()}");

            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            UnsubscribeFromTargetReady();
        }

        private IEnumerator WaitForCamera() {
            //private IEnumerator WaitForCamera(int frameNumber) {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.WaitForCamera(): " + namePlateController.Interactable.GetInstanceID() + "; frame: " + frameNumber);
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.WaitForCamera(): " + namePlateController.Interactable.GetInstanceID());
            yield return null;
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.WaitForCamera(): about to render " + namePlateController.Interactable.GetInstanceID() + "; initial frame: " + frameNumber + "; current frame: " + lastWaitFrame);
            if (previewManager.UnitController.IsBuilding() == true) {
                //Debug.Log($"{gameObject.name}.UnitFramePanelBase.WaitForCamera(): a new wait was started. initial frame: " + frameNumber +  "; current wait: " + lastWaitFrame);
            } else {
                //Debug.Log($"{gameObject.name}.UnitFramePanelBase.WaitForCamera(): rendering");
                UpdateCameraPosition();
                previewCamera.Render();
                waitForCameraCoroutine = null;
                //namePlateController?.Interactable.ClearSnapshotRequest();
            }
        }

        private void UpdateCameraPosition() {
            if (!targetInitialized || previewManager.UnitController.CameraTargetReady == false) {
                //Debug.Log("UnitFrameController.Update(). Not initialized yet.  Exiting.");
                return;
            }
            if (previewManager.UnitController.CameraTargetReady == true && followTransform == null) {
                //Debug.Log($"{gameObject.name}UnitFrameController.Update(). Follow transform is null. possibly dead unit despawned. Exiting.");
                ClearTarget();
                return;
            }

            if (cameraTransform != null) {

                wantedPosition = previewManager.UnitController.transform.TransformPoint(previewManager.UnitController.transform.InverseTransformPoint(followTransform.position) + cameraPositionOffset);
                wantedLookPosition = previewManager.UnitController.transform.TransformPoint(previewManager.UnitController.transform.InverseTransformPoint(followTransform.position) + cameraLookOffset);
                cameraTransform.position = wantedPosition;
                cameraTransform.LookAt(wantedLookPosition);

            } else {
            }
        }

        protected override void PostTargetInitialization() {
            base.PostTargetInitialization();

            if (spawnPreviewUnit) {
                previewManager.SpawnUnit(unitController);
            }
            
            InitializePosition();
            previewManager.UnitController.ConfigureUnitFrame(this, previewCamera != null);
        }

        protected void InitializePosition() {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.InitializePosition()");

            if (previewManager.UnitController != null) {
                cameraPositionOffset = previewManager.UnitController.NamePlateController.UnitFrameCameraPositionOffset;
            } else {
                cameraPositionOffset = cameraPositionOffsetDefault;
            }
            if (previewManager.UnitController.NamePlateController.UnitFrameCameraLookOffset != null) {
                cameraLookOffset = previewManager.UnitController.NamePlateController.UnitFrameCameraLookOffset;
            } else {
                cameraLookOffset = cameraLookOffsetDefault;
            }
        }

        public void SubscribeToTargetReady() {
            previewManager.UnitController.OnCameraTargetReady += HandleTargetReady;
            subscribedToTargetReady = true;
        }

        public void UnsubscribeFromTargetReady() {
            if (subscribedToTargetReady == false) {
                return;
            }

            if (previewManager.UnitController != null) {
                previewManager.UnitController.OnCameraTargetReady -= HandleTargetReady;
            }

            subscribedToTargetReady = false;
        }

        public void HandleTargetReady() {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.HandleTargetReady()");

            //UnsubscribeFromTargetReady();
            GetFollowTarget();
            UpdateCameraPosition();
            //lastWaitFrame++;
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.HandleTargetReady() " + namePlateController.Interactable.GetInstanceID() + "; frame : " + lastWaitFrame);
            //if (waitForCameraCoroutine == null) {
            //waitForCameraCoroutine = StartCoroutine(WaitForCamera(lastWaitFrame));
            //}
            waitForCameraCoroutine = StartCoroutine(WaitForCamera());
            //namePlateController?.NamePlateUnit.RequestSnapshot();
        }

        private void GetFollowTarget() {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.WaitForFollowTarget()");
            Transform targetBone = previewManager.UnitController.NamePlateController.NamePlateUnit.transform;
            string unitFrameTarget = previewManager.UnitController.NamePlateController.UnitFrameTarget;
            //Debug.Log("Unit Frame: Searching for target: " + unitFrameTarget);
            if (unitFrameTarget != string.Empty) {
                if (previewManager.UnitController.gameObject != null) {
                    targetBone = previewManager.UnitController.transform.FindChildByRecursive(unitFrameTarget);
                    if (targetBone == null) {
                        Debug.LogWarning($"{gameObject.name}.UnitFramePanelBase.GetFollowTarget(): Could not find targetBone: {unitFrameTarget}");
                    }
                }
            }
            this.followTransform = targetBone;
        }


    }

}