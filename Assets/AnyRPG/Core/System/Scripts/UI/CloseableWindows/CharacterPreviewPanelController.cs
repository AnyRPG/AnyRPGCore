using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AnyRPG {

    public class CharacterPreviewPanelController : WindowContentController {


        public event Action OnTargetReady = delegate { };
        public event Action OnTargetCreated = delegate { };
        //public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };
        public override event Action<CloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        private CharacterPreviewCameraController previewCameraController = null;

        // track whether targetreadycallback has been activated or not
        private bool characterReady = false;

        private bool windowOpened = false;

        // need a reference to the capabilityConsumer calling window to get Unit Profile
        private ICapabilityConsumer capabilityConsumer = null;

        // game manager references
        private CharacterCreatorManager characterCreatorManager = null;
        private CameraManager cameraManager = null;

        public CharacterPreviewCameraController PreviewCameraController { get => previewCameraController; set => previewCameraController = value; }
        public bool CharacterReady { get => characterReady; }
        public ICapabilityConsumer CapabilityConsumer { get => capabilityConsumer; set => capabilityConsumer = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            previewCameraController.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            characterCreatorManager = systemGameManager.CharacterCreatorManager;
            cameraManager = systemGameManager.CameraManager;
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("CharacterPreviewPanelController.RecieveClosedWindowNotification()");
            base.ReceiveClosedWindowNotification();
            characterReady = false;
            windowOpened = false;
            characterCreatorManager.OnUnitCreated -= HandleTargetCreated;
            characterCreatorManager.HandleCloseWindow();
            previewCameraController.ClearTarget();
            OnCloseWindow(this);
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("CharacterPreviewPanelController.ProcessOpenWindowNotification()");
            base.ProcessOpenWindowNotification();
            windowOpened = true;
            characterReady = false;
            characterCreatorManager.OnUnitCreated += HandleTargetCreated;
            SetPreviewTarget();
        }

        public void ReloadUnit() {
            //Debug.Log("CharacterPreviewPanelController.ReloadUnit()");
            if (windowOpened == false) {
                // the window has not been opened (initialized) yet, so don't try to spawn any unit
                return;
            }
            ClearPreviewTarget();
            characterReady = false;
            SetPreviewTarget();
        }

        public void ClearPreviewTarget() {
            //Debug.Log("CharacterPreviewPanelController.ClearPreviewTarget()");
            // not really close window, but it will despawn the preview unit
            characterCreatorManager.HandleCloseWindow();
        }

        private void SetPreviewTarget() {
            //Debug.Log("CharacterPreviewPanelController.SetPreviewTarget()");
            if (characterCreatorManager.PreviewUnitController != null
                || capabilityConsumer == null
                || capabilityConsumer.UnitProfile == null) {
                //Debug.Log("CharacterPreviewPanelController.SetPreviewTarget() UMA avatar is already spawned!");
                return;
            }

            //spawn correct preview unit
            characterCreatorManager.SpawnUnit(capabilityConsumer.UnitProfile);

            if (cameraManager.CharacterPreviewCamera != null) {
                //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting target");
                if (PreviewCameraController != null) {
                    PreviewCameraController.OnTargetReady += HandleTargetReady;
                    PreviewCameraController.InitializeCamera(characterCreatorManager.PreviewUnitController);
                    //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting Target Ready Callback");
                } else {
                    Debug.LogError("CharacterPanel.SetPreviewTarget(): Character Preview Camera Controller is null. Please set it in the inspector");
                }
            }
        }

        public void HandleTargetCreated() {
            //Debug.Log("CharacterPreviewPanelController.HandleTargetCreated()");
            OnTargetCreated();
        }

        public void HandleTargetReady() {
            //Debug.Log("CharacterPreviewPanelController.TargetReadyCallback()");
            PreviewCameraController.OnTargetReady -= HandleTargetReady;
            characterReady = true;

            OnTargetReady();
        }

        public void SaveAppearanceData(AnyRPGSaveData saveData) {
            if (characterCreatorManager.PreviewUnitController?.UnitModelController == null) {
                return;
            }
            characterCreatorManager.PreviewUnitController.UnitModelController.SaveAppearanceSettings(saveData);
        }

    }

}