using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AnyRPG {

    public class NewGameCharacterPreviewPanelController : WindowContentController {

        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        private CharacterPreviewCameraController previewCameraController = null;

        // track whether targetreadycallback has been activated or not
        private bool characterReady = false;

        public CharacterPreviewCameraController MyPreviewCameraController { get => previewCameraController; set => previewCameraController = value; }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("CharacterCreatorPanel.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            previewCameraController.ClearTarget();
            characterReady = false;
            CharacterCreatorManager.MyInstance.HandleCloseWindow();
            OnCloseWindow(this);
            // close interaction window too for smoother experience
            PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("NewGameCharacterPanelController.ReceiveOpenWindowNotification()");

            characterReady = false;
            SetPreviewTarget();
        }

        public void SetPlayerName(string newPlayerName) {
            NewGamePanel.MyInstance.SetPlayerName(newPlayerName);
        }

        public void ReloadUnit() {
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
            CharacterCreatorManager.MyInstance.HandleOpenWindow(NewGamePanel.MyInstance.UnitProfile);

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
            Debug.Log("NewGameCharacterPanelController.TargetReadyCallback()");
            MyPreviewCameraController.OnTargetReady -= TargetReadyCallback;
            characterReady = true;

            EquipCharacter();
            StartCoroutine(PointlessDelay());
        }

        public void EquipCharacter() {
            //Debug.Log("NewGameCharacterPanelController.EquipCharacter()");

            if (characterReady == false) {
                // attempting this before the character is spawned will make it go invisible (UMA bug)
                //Debug.Log("NewGameCharacterPanelController.EquipCharacter(): character not ready yet, exiting.");
                return;
            }

            CharacterEquipmentManager characterEquipmentManager = CharacterCreatorManager.MyInstance.PreviewUnitController.BaseCharacter.CharacterEquipmentManager;
            if (characterEquipmentManager != null) {
                //Debug.Log("NewGameCharacterPanelController.EquipCharacter(): found equipment manager");
                characterEquipmentManager.UnequipAll(false);
                if (NewGamePanel.MyInstance.EquipmentList != null) {
                    //Debug.Log("NewGameCharacterPanelController.EquipCharacter(): equipment list is not null");
                    foreach (Equipment equipment in NewGamePanel.MyInstance.EquipmentList.Values) {
                        //Debug.Log("NewGameCharacterPanelController.EquipCharacter(): ask to equip: " + equipment.DisplayName);
                        characterEquipmentManager.Equip(equipment, null, false, false);
                    }
                    RebuildUMA();
                }
            }
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