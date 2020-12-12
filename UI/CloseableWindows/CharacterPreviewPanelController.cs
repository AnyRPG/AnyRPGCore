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

        // need a reference to the capabilityConsumer calling window to get Unit Profile
        private ICapabilityConsumer capabilityConsumer = null;

        public CharacterPreviewCameraController MyPreviewCameraController { get => previewCameraController; set => previewCameraController = value; }
        public bool CharacterReady { get => characterReady; }
        public ICapabilityConsumer CapabilityConsumer { get => capabilityConsumer; set => capabilityConsumer = value; }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("CharacterCreatorPanel.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            previewCameraController.ClearTarget();
            characterReady = false;
            CharacterCreatorManager.MyInstance.HandleCloseWindow();
            OnCloseWindow(this);
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("NewGameCharacterPanelController.ReceiveOpenWindowNotification()");

            characterReady = false;
            SetPreviewTarget();
        }

        public void ReloadUnit() {
            //Debug.Log("NewGameCharacterPanelController.ReloadUnit()");
            if (characterReady == false) {
                // the window has not been opened (initialized) yet, so don't try to spawn any unit
                return;
            }
            ClearPreviewTarget();
            SetPreviewTarget();
        }

        public void ClearPreviewTarget() {
            //Debug.Log("NewGameCharacterPanelController.ClearPreviewTarget()");
            // not really close window, but it will despawn the preview unit
            CharacterCreatorManager.MyInstance.HandleCloseWindow();
        }

        private void SetPreviewTarget() {
            //Debug.Log("NewGameCharacterPanelController.SetPreviewTarget()");
            if (CharacterCreatorManager.MyInstance.PreviewUnitController != null) {
                //Debug.Log("CharacterPanel.SetPreviewTarget() UMA avatar is already spawned!");
                return;
            }
            //spawn correct preview unit
            CharacterCreatorManager.MyInstance.HandleOpenWindow(capabilityConsumer.UnitProfile);

            if (CameraManager.MyInstance != null && CameraManager.MyInstance.CharacterPreviewCamera != null) {
                //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting target");
                if (MyPreviewCameraController != null) {
                    MyPreviewCameraController.OnTargetReady += TargetReadyCallback;
                    MyPreviewCameraController.InitializeCamera(CharacterCreatorManager.MyInstance.PreviewUnitController);
                    //Debug.Log("CharacterPanel.SetPreviewTarget(): preview camera was available, setting Target Ready Callback");
                } else {
                    Debug.LogError("CharacterPanel.SetPreviewTarget(): Character Preview Camera Controller is null. Please set it in the inspector");
                }
            }
        }

        public void TargetReadyCallback() {
            //Debug.Log("NewGameCharacterPanelController.TargetReadyCallback()");
            MyPreviewCameraController.OnTargetReady -= TargetReadyCallback;
            characterReady = true;

            OnTargetReady();
            StartCoroutine(PointlessDelay());
        }


        public IEnumerator PointlessDelay() {
            //Debug.Log("NewGameCharacterPanelController.EquipCharacter(): found equipment manager");
            yield return null;
            RebuildUMA();
        }


        public void RebuildUMA() {
            //Debug.Log("CharacterCreatorPanel.RebuildUMA()");
            //Debug.Log("NewGameCharacterPanelController.RebuildUMA(): BuildCharacter(): buildenabled: " + umaAvatar.BuildCharacterEnabled + "; frame: " + Time.frameCount);
            if (CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar != null) {
                CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.BuildCharacter();
            }
        }

        public string GetCurrentRecipe() {
            if (CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar == null) {
                return string.Empty;
            }
            return CharacterCreatorManager.MyInstance.PreviewUnitController.DynamicCharacterAvatar.GetCurrentRecipe();
        }

    }

}