using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class NewGameMecanimCharacterPanelController : WindowContentController {

        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

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

        [SerializeField]
        private HighlightButton appearanceButton = null;

        private NewGameUnitButton selectedUnitButton = null;

        private List<NewGameUnitButton> optionButtons = new List<NewGameUnitButton>();

        // game manager references
        private ObjectPooler objectPooler = null;
        private NewGameManager newGameManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            appearanceButton.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            objectPooler = systemGameManager.ObjectPooler;
            newGameManager = systemGameManager.NewGameManager;
        }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("NewGameMecanimCharacterPanelController.RecieveClosedWindowNotification()");
            base.RecieveClosedWindowNotification();
            OnCloseWindow(this);
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("NewGameMecanimCharacterPanelController.ReceiveOpenWindowNotification()");
            base.ReceiveOpenWindowNotification();
            ShowOptionButtonsCommon();
        }

        public void ClearOptionButtons() {
            // clear the quest list so any quests left over from a previous time opening the window aren't shown
            //Debug.Log("LoadGamePanel.ClearLoadButtons()");
            foreach (NewGameUnitButton optionButton in optionButtons) {
                if (optionButton != null) {
                    optionButton.DeSelect();
                    objectPooler.ReturnObjectToPool(optionButton.gameObject);
                }
            }
            optionButtons.Clear();
        }

        public void ShowOptionButtonsCommon() {
            //Debug.Log("NewGameMecanimCharacterPanelController.ShowOptionButtonsCommon()");
            ClearOptionButtons();

            if ((newGameManager.Faction != null && newGameManager.Faction.HideDefaultProfiles == false)
                || systemConfigurationManager.AlwaysShowDefaultProfiles == true
                || newGameManager.Faction == null) {
                //Debug.Log("NewGameMecanimCharacterPanelController.ShowOptionButtonsCommon(): showing default profiles");
                AddDefaultProfiles();
            }
            if (newGameManager.Faction != null) {
                foreach (UnitProfile unitProfile in newGameManager.Faction.CharacterCreatorProfiles) {
                    //Debug.Log("NewGameMecanimCharacterPanelController.ShowOptionButtonsCommon(): found valid unit profile: " + unitProfile.DisplayName);
                    GameObject go = objectPooler.GetPooledObject(buttonPrefab, buttonArea.transform);
                    NewGameUnitButton optionButton = go.GetComponent<NewGameUnitButton>();
                    optionButton.Configure(systemGameManager);
                    optionButton.AddUnitProfile(unitProfile);
                    optionButtons.Add(optionButton);
                }
            }
            if (optionButtons.Count > 0) {
                optionButtons[0].Select();
            }
        }

        private void AddDefaultProfiles() {
            if (systemConfigurationManager.DefaultPlayerUnitProfile != null) {
                GameObject go = objectPooler.GetPooledObject(buttonPrefab, buttonArea.transform);
                NewGameUnitButton optionButton = go.GetComponent<NewGameUnitButton>();
                optionButton.Configure(systemGameManager);
                optionButton.AddUnitProfile(systemConfigurationManager.DefaultPlayerUnitProfile);
                optionButtons.Add(optionButton);
            }
            foreach (UnitProfile unitProfile in systemConfigurationManager.CharacterCreatorProfiles) {
                //Debug.Log("NewGameMecanimCharacterPanelController.ShowOptionButtonsCommon(): found valid unit profile: " + unitProfile.DisplayName);
                GameObject go = objectPooler.GetPooledObject(buttonPrefab, buttonArea.transform);
                NewGameUnitButton optionButton = go.GetComponent<NewGameUnitButton>();
                optionButton.Configure(systemGameManager);
                optionButton.AddUnitProfile(unitProfile);
                optionButtons.Add(optionButton);
            }
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
            ShowOptionButtonsCommon();
        }

        public void SetBody(NewGameUnitButton newGameUnitButton) {
            if (selectedUnitButton != null && selectedUnitButton != newGameUnitButton) {
                selectedUnitButton.DeSelect();
            }
            selectedUnitButton = newGameUnitButton;
        }


    }

}