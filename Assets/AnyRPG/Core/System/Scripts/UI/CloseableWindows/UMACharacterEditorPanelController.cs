using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UMA.CharacterSystem;

namespace AnyRPG {

    public class UMACharacterEditorPanelController : UMAAppearancePanel {

        //public override event Action<CloseableWindowContents> OnCloseWindow = delegate { };

        /*
        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("CharacterCreatorPanel.OnCloseWindow()");
            base.ReceiveClosedWindowNotification();
            OnCloseWindow(this);
        }
        */

        /*
        public override void ProcessOpenWindowNotification() {
            //Debug.Log("UMACharacterEditorPanelController.ProcessOpenWindowNotification()");
            base.ProcessOpenWindowNotification();
            //uINavigationControllers[0].FocusCurrentButton();
        }
        */

        public override void SetupOptions() {
            //Debug.Log("UMACharacterEditorPanelController.SetupOptions()");
            base.SetupOptions();

            CloseOptionsAreas();
            mainButtonsArea.SetActive(false);
            mainNoOptionsArea.SetActive(false);

            umaModelController = characterCreatorManager.PreviewUnitController?.UnitModelController.ModelAppearanceController.GetModelAppearanceController<UMAModelController>();

            if (umaModelController == null) {
                // somehow this panel was opened but the preview model is not configured as an UMA model
                mainNoOptionsArea.SetActive(true);
                return;
            }

            dynamicCharacterAvatar = umaModelController.DynamicCharacterAvatar;

            // there are no options to show if this is not an UMA
            if (dynamicCharacterAvatar == null) {
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