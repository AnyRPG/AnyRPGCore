using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UMA;
using UMA.Examples;
using UMA.CharacterSystem;
using UMA.CharacterSystem.Examples;

namespace AnyRPG {

    public class UMACharacterEditorPanelController : CharacterAppearancePanel {

        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        protected CanvasGroup canvasGroup = null;

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("CharacterCreatorPanel.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            OnCloseWindow(this);
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("UMACharacterEditorPanelController.ReceiveOpenWindowNotification()");
            base.ReceiveOpenWindowNotification();
        }

        public void HidePanel() {
            //Debug.Log("UMACharacterEditorPanelController.HidePanel()");
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        public void ShowPanel() {
            //Debug.Log("UMACharacterEditorPanelController.ShowPanel()");
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            SetupOptions();
        }

        public void SetupOptions() {
            //Debug.Log("UMACharacterEditorPanelController.SetupOptions()");
            CloseOptionsAreas();
            mainButtonsArea.SetActive(false);
            mainNoOptionsArea.SetActive(false);
            // there are no options to show if this is not an UMA
            if (characterCreatorManager == null) {
                Debug.Log("UMACharacterEditorPanelController.SetupOptions() : characterCreatorManager is null");
            }
            if (characterCreatorManager.PreviewUnitController?.DynamicCharacterAvatar == null) {
                mainNoOptionsArea.SetActive(true);
                return;
            }
            if (characterCreatorManager.PreviewUnitController?.ModelReady == true) {
                mainButtonsArea.SetActive(true);
                OpenAppearanceOptionsArea();
                InitializeSexButtons();
            }
        }

        public void HandleTargetReady() {
            //Debug.Log("UMACharacterEditorPanelController.HandleTargetReady()");
            SetupOptions();
        }


    }

}