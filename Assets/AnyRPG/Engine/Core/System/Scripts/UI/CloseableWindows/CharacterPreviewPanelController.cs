using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AnyRPG {

    public class CharacterPreviewPanelController : WindowContentController {


        public event Action OnTargetReady = delegate { };
        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        private CharacterPreviewCameraController previewCameraController = null;

        // track whether targetreadycallback has been activated or not
        private bool characterReady = false;

        private bool windowOpened = false;

        // need a reference to the capabilityConsumer calling window to get Unit Profile
        private ICapabilityConsumer capabilityConsumer = null;

        public CharacterPreviewCameraController MyPreviewCameraController { get => previewCameraController; set => previewCameraController = value; }
        public bool CharacterReady { get => characterReady; }
        public ICapabilityConsumer CapabilityConsumer { get => capabilityConsumer; set => capabilityConsumer = value; }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("CharacterPreviewPanelController.RecieveClosedWindowNotification()");
            base.RecieveClosedWindowNotification();
            characterReady = false;
            SystemGameManager.Instance.CharacterCreatorManager.HandleCloseWindow();
            previewCameraController.ClearTarget();
            OnCloseWindow(this);
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("CharacterPreviewPanelController.ReceiveOpenWindowNotification()");
            windowOpened = true;
            characterReady = false;
            SetPreviewTarget();
        }

        public void ReloadUnit() {
            //Debug.Log("CharacterPreviewPanelController.ReloadUnit()");
            if (windowOpened == false) {
                // the window has not been opened (initialized) yet, so don't try to spawn any unit
                return;
            }
            ClearPreviewTarget();
            SetPreviewTarget();
        }

        public void ClearPreviewTarget() {
            //Debug.Log("CharacterPreviewPanelController.ClearPreviewTarget()");
            // not really close window, but it will despawn the preview unit
            SystemGameManager.Instance.CharacterCreatorManager.HandleCloseWindow();
        }

        private void SetPreviewTarget() {
            //Debug.Log("CharacterPreviewPanelController.SetPreviewTarget()");
            if (SystemGameManager.Instance.CharacterCreatorManager.PreviewUnitController != null
                || capabilityConsumer == null
                || capabilityConsumer.UnitProfile == null) {
                //Debug.Log("CharacterPreviewPanelController.SetPreviewTarget() UMA avatar is already spawned!");
                return;
            }

            //spawn correct preview unit
            SystemGameManager.Instance.CharacterCreatorManager.HandleOpenWindow(capabilityConsumer.UnitProfile);

            if (SystemGameManager.Instance.CameraManager != null && SystemGameManager.Instance.CameraManager.CharacterPreviewCamera != null) {
                //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting target");
                if (MyPreviewCameraController != null) {
                    MyPreviewCameraController.OnTargetReady += TargetReadyCallback;
                    MyPreviewCameraController.InitializeCamera(SystemGameManager.Instance.CharacterCreatorManager.PreviewUnitController);
                    //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting Target Ready Callback");
                } else {
                    Debug.LogError("CharacterPanel.SetPreviewTarget(): Character Preview Camera Controller is null. Please set it in the inspector");
                }
            }
        }

        public void TargetReadyCallback() {
            //Debug.Log("CharacterPreviewPanelController.TargetReadyCallback()");
            MyPreviewCameraController.OnTargetReady -= TargetReadyCallback;
            characterReady = true;

            OnTargetReady();
            StartCoroutine(PointlessDelay());
        }


        public IEnumerator PointlessDelay() {
            //Debug.Log("CharacterPreviewPanelController.EquipCharacter(): found equipment manager");
            yield return null;
            RebuildUMA();
        }


        public void RebuildUMA() {
            //Debug.Log("CharacterCreatorPanel.RebuildUMA()");
            //Debug.Log("CharacterPreviewPanelController.RebuildUMA(): BuildCharacter(): buildenabled: " + umaAvatar.BuildCharacterEnabled + "; frame: " + Time.frameCount);
            if (SystemGameManager.Instance.CharacterCreatorManager.PreviewUnitController?.DynamicCharacterAvatar != null) {
                SystemGameManager.Instance.CharacterCreatorManager.PreviewUnitController.DynamicCharacterAvatar.BuildCharacter();
            }
        }

        public string GetCurrentRecipe() {
            if (SystemGameManager.Instance.CharacterCreatorManager.PreviewUnitController?.DynamicCharacterAvatar == null) {
                return string.Empty;
            }
            return SystemGameManager.Instance.CharacterCreatorManager.PreviewUnitController.DynamicCharacterAvatar.GetCurrentRecipe();
        }

    }

}