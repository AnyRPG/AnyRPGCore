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

        private NewGameUnitButton selectedUnitButton = null;

        private List<NewGameUnitButton> optionButtons = new List<NewGameUnitButton>();


        public override void RecieveClosedWindowNotification() {
            //Debug.Log("NewGameMecanimCharacterPanelController.RecieveClosedWindowNotification()");
            base.RecieveClosedWindowNotification();
            OnCloseWindow(this);
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("NewGameMecanimCharacterPanelController.ReceiveOpenWindowNotification()");

            ShowOptionButtonsCommon();
        }

        public void ClearOptionButtons() {
            // clear the quest list so any quests left over from a previous time opening the window aren't shown
            //Debug.Log("LoadGamePanel.ClearLoadButtons()");
            foreach (NewGameUnitButton optionButton in optionButtons) {
                if (optionButton != null) {
                    Destroy(optionButton.gameObject);
                }
            }
            optionButtons.Clear();
        }

        public void ShowOptionButtonsCommon() {
            //Debug.Log("NewGameMecanimCharacterPanelController.ShowOptionButtonsCommon()");
            ClearOptionButtons();

            if (NewGamePanel.MyInstance.Faction != null) {
                //Debug.Log("NewGameMecanimCharacterPanelController.ShowOptionButtonsCommon(): faction is not null : " + NewGamePanel.MyInstance.Faction.DisplayName);
                if (NewGamePanel.MyInstance.Faction.HideDefaultProfiles == false) {
                    foreach (UnitProfile unitProfile in SystemConfigurationManager.MyInstance.CharacterCreatorProfiles) {
                        //Debug.Log("NewGameMecanimCharacterPanelController.ShowOptionButtonsCommon(): found valid unit profile: " + unitProfile.DisplayName);
                        GameObject go = Instantiate(buttonPrefab, buttonArea.transform);
                        NewGameUnitButton optionButton = go.GetComponent<NewGameUnitButton>();
                        optionButton.AddUnitProfile(unitProfile);
                        optionButtons.Add(optionButton);
                    }
                }
                foreach (UnitProfile unitProfile in NewGamePanel.MyInstance.Faction.CharacterCreatorProfiles) {
                    //Debug.Log("NewGameMecanimCharacterPanelController.ShowOptionButtonsCommon(): found valid unit profile: " + unitProfile.DisplayName);
                    GameObject go = Instantiate(buttonPrefab, buttonArea.transform);
                    NewGameUnitButton optionButton = go.GetComponent<NewGameUnitButton>();
                    optionButton.AddUnitProfile(unitProfile);
                    optionButtons.Add(optionButton);
                }
            }
            if (optionButtons.Count > 0) {
                optionButtons[0].Select();
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
        }

        public void SetBody(NewGameUnitButton newGameUnitButton) {
            if (selectedUnitButton != null && selectedUnitButton != newGameUnitButton) {
                selectedUnitButton.DeSelect();
            }
            selectedUnitButton = newGameUnitButton;
        }


    }

}