using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class NewGameMecanimCharacterPanelController : WindowContentController {

        /*
        [Header("Appearance")]

        [SerializeField]
        private HighlightButton bodyButton = null;
        */

        [Header("Configuration")]

        [SerializeField]
        private GameObject buttonPrefab = null;

        [SerializeField]
        private GameObject buttonArea = null;

        [SerializeField]
        private CanvasGroup canvasGroup = null;

        /*
        [SerializeField]
        private HighlightButton appearanceButton = null;
        */

        private NewGameUnitButton selectedUnitButton = null;

        private List<NewGameUnitButton> optionButtons = new List<NewGameUnitButton>();

        // game manager references
        private ObjectPooler objectPooler = null;
        private NewGameManager newGameManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            //appearanceButton.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            objectPooler = systemGameManager.ObjectPooler;
            newGameManager = systemGameManager.NewGameManager;
        }

        /*
        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("NewGameMecanimCharacterPanelController.RecieveClosedWindowNotification()");
            base.ReceiveClosedWindowNotification();
            OnCloseWindow(this);
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("NewGameMecanimCharacterPanelController.ProcessOpenWindowNotification()");
            base.ProcessOpenWindowNotification();

        }
        */

        public void ClearOptionButtons() {
            //Debug.Log("LoadGamePanel.ClearLoadButtons()");
            foreach (NewGameUnitButton optionButton in optionButtons) {
                if (optionButton != null) {
                    optionButton.DeSelect();
                    objectPooler.ReturnObjectToPool(optionButton.gameObject);
                }
            }
            uINavigationControllers[0].ClearActiveButtons();
            optionButtons.Clear();
        }

        public void ShowOptionButtons() {
            //Debug.Log("NewGameMecanimCharacterPanelController.ShowOptionButtons()");

            ClearOptionButtons();

            for (int i = 0; i < newGameManager.UnitProfileList.Count; i++) {
                //Debug.Log("NewGameMecanimCharacterPanelController.ShowOptionButtonsCommon(): found valid unit profile: " + unitProfile.DisplayName);
                GameObject go = objectPooler.GetPooledObject(buttonPrefab, buttonArea.transform);
                NewGameUnitButton optionButton = go.GetComponent<NewGameUnitButton>();
                optionButton.Configure(systemGameManager);
                optionButton.AddUnitProfile(newGameManager.UnitProfileList[i]);
                optionButtons.Add(optionButton);
                uINavigationControllers[0].AddActiveButton(optionButton);
            }
            /*
            if (optionButtons.Count > 0) {
                SetNavigationController(uINavigationControllers[0]);
            }
            */
        }


        public void HidePanel() {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        public void ShowPanel() {
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            //ShowOptionButtonsCommon();
        }

        public void SetUnitProfile(UnitProfile newUnitProfile) {
            //Debug.Log("NewGameMecanimCharacterPanelController.SetUnitProfile(" + (newUnitProfile == null ? "null" : newUnitProfile.DisplayName) + ")");

            // deselect old button
            if (selectedUnitButton != null && selectedUnitButton.UnitProfile != newUnitProfile) {
                selectedUnitButton.DeSelect();
                selectedUnitButton.UnHighlightBackground();
            }

            // select new button
            for (int i = 0; i < optionButtons.Count; i++) {
                if (optionButtons[i].UnitProfile == newUnitProfile) {
                    selectedUnitButton = optionButtons[i];
                    uINavigationControllers[0].SetCurrentIndex(i);
                    optionButtons[i].HighlightBackground();
                }
            }
        }


    }

}