using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UMA.CharacterSystem;

namespace AnyRPG {

    public class UMACharacterEditorPanelController : CharacterAppearancePanel {

        //public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };
        public override event Action<CloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        protected CanvasGroup canvasGroup = null;

        private DynamicCharacterAvatar dynamicCharacterAvatar = null;

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("CharacterCreatorPanel.OnCloseWindow()");
            base.ReceiveClosedWindowNotification();
            OnCloseWindow(this);
        }

        /*
        public override void ProcessOpenWindowNotification() {
            //Debug.Log("UMACharacterEditorPanelController.ProcessOpenWindowNotification()");
            base.ProcessOpenWindowNotification();
            //uINavigationControllers[0].FocusCurrentButton();
        }
        */

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
            /*
            if (characterCreatorManager == null) {
                Debug.Log("UMACharacterEditorPanelController.SetupOptions() : characterCreatorManager is null");
            }
            */
            if (characterCreatorManager.PreviewUnitController?.UnitModelController?.UMAModelController?.DynamicCharacterAvatar == null) {
                mainNoOptionsArea.SetActive(true);
                return;
            }
            if (characterCreatorManager.PreviewUnitController?.UnitModelController?.ModelReady == true) {
                mainButtonsArea.SetActive(true);
                OpenAppearanceOptionsArea();
                //appearanceButton.HighlightBackground();
                InitializeSexButtons();
                uINavigationControllers[0].FocusCurrentButton();
            }
        }

        public void HandleTargetReady() {
            //Debug.Log("UMACharacterEditorPanelController.HandleTargetReady()");
            dynamicCharacterAvatar = characterCreatorManager.PreviewUnitController.UnitModelController.UMAModelController.DynamicCharacterAvatar;
            SetupOptions();
        }

        /*
        public override void OpenAppearanceOptionsArea() {
            base.OpenAppearanceOptionsArea();
            uINavigationControllers[1].UpdateNavigationList();
            //SetNavigationController(uINavigationControllers[1]);
        }
        */

        /*
        public override void OpenColorsOptionsArea() {
            base.OpenColorsOptionsArea();
            //SetNavigationController(uINavigationControllers[2]);
        }
        */

        /*
        public override void OpenSexOptionsArea() {
            base.OpenSexOptionsArea();
            //SetNavigationController(uINavigationControllers[3]);
        }
        */

    }

}