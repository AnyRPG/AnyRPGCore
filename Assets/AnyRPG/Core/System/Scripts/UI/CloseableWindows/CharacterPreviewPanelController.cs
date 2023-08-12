using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AnyRPG {

    public class CharacterPreviewPanelController : WindowContentController {

        // events
        public event Action OnTargetReady = delegate { };
        public event Action OnUnitCreated = delegate { };
        public override event Action<CloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        private CharacterPreviewCameraController previewCameraController = null;

        // track whether targetreadycallback has been activated or not
        private bool characterReady = false;

        private bool windowOpened = false;

        // need a reference to the capabilityConsumer calling window to get Unit Profile
        //private ICapabilityConsumer capabilityConsumer = null;
        private ICharacterConfigurationProvider characterConfigurationProvider = null;

        // game manager references
        private CharacterCreatorManager characterCreatorManager = null;
        private CameraManager cameraManager = null;

        public CharacterPreviewCameraController PreviewCameraController { get => previewCameraController; set => previewCameraController = value; }
        public bool CharacterReady { get => characterReady; }
        public ICharacterConfigurationProvider CharacterConfigurationProvider { get => characterConfigurationProvider; set => characterConfigurationProvider = value; }

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
            characterCreatorManager.OnUnitCreated -= HandleUnitCreated;
            characterCreatorManager.HandleCloseWindow();
            previewCameraController.ClearTarget();
            OnCloseWindow(this);
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("CharacterPreviewPanelController.ProcessOpenWindowNotification()");
            base.ProcessOpenWindowNotification();
            windowOpened = true;
            characterReady = false;
            characterCreatorManager.OnUnitCreated += HandleUnitCreated;
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
                || characterConfigurationProvider == null
                //|| capabilityConsumer.UnitProfile == null
                ) {
                //Debug.Log("CharacterPreviewPanelController.SetPreviewTarget() character is already spawned!");
                return;
            }

            CharacterConfigurationRequest characterConfigurationRequest = characterConfigurationProvider.GetCharacterConfigurationRequest();

            //spawn correct preview unit
            if (characterConfigurationRequest.unitProfile == null) {
                // if this window is opened before a unit profile is available from the provider, there will be no character to spawn
                return;
            }
            characterCreatorManager.SpawnUnit(characterConfigurationRequest);

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

        public void HandleUnitCreated() {
            //Debug.Log("CharacterPreviewPanelController.HandleUnitCreated()");
            OnUnitCreated();
        }

        public void HandleTargetReady() {
            //Debug.Log("CharacterPreviewPanelController.TargetReadyCallback()");
            PreviewCameraController.OnTargetReady -= HandleTargetReady;
            characterReady = true;

            OnTargetReady();
        }

    }

}