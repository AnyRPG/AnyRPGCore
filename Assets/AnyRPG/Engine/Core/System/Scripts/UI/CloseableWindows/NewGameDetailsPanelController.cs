using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class NewGameDetailsPanelController : WindowContentController {

        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        [Header("Name")]
        public TMP_InputField textInput;

        [Header("Details")]

        [SerializeField]
        private GameObject characterClassLabel = null;

        [SerializeField]
        private NewGameCharacterClassButton characterClassButton = null;

        [SerializeField]
        private GameObject classSpecializationLabel = null;

        [SerializeField]
        private NewGameClassSpecializationButton classSpecializationButton = null;

        [SerializeField]
        private GameObject factionLabel = null;

        [SerializeField]
        private NewGameFactionButton factionButton = null;

        [SerializeField]
        private CanvasGroup canvasGroup = null;

        /*
        private bool validFactionExists = false;
        private bool validClassExists = false;
        private bool validSpecializationExists = false;
        */

        public void ResetInputText(string newText) {
            textInput.text = newText;
        }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("CharacterCreatorPanel.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            OnCloseWindow(this);
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("NewGameCharacterPanelController.ReceiveOpenWindowNotification()");
            base.ReceiveOpenWindowNotification();
            ClearLabels();
        }

        public void ClearLabels() {
            factionLabel.SetActive(false);
            factionButton.gameObject.SetActive(false);
            if (SystemGameManager.Instance.SystemConfigurationManager.NewGameFaction == true) {
                foreach (Faction faction in SystemDataFactory.Instance.GetResourceList<Faction>()) {
                    if (faction.NewGameOption == true) {
                        factionLabel.SetActive(true);
                        factionButton.gameObject.SetActive(true);
                        break;
                    }
                }
            }
            
            characterClassLabel.SetActive(false);
            characterClassButton.gameObject.SetActive(false);
            if (SystemGameManager.Instance.SystemConfigurationManager.NewGameClass == true) {
                foreach (CharacterClass characterClass in SystemDataFactory.Instance.GetResourceList<CharacterClass>()) {
                    if (characterClass.NewGameOption == true) {
                        characterClassLabel.SetActive(true);
                        characterClassButton.gameObject.SetActive(true);
                        break;
                    }
                }
            }

            classSpecializationLabel.SetActive(false);
            classSpecializationButton.gameObject.SetActive(false);
            if (SystemGameManager.Instance.SystemConfigurationManager.NewGameSpecialization == true) {
                foreach (ClassSpecialization classSpecialization in SystemDataFactory.Instance.GetResourceList<ClassSpecialization>()) {
                    if (classSpecialization.NewGameOption == true) {
                        classSpecializationLabel.SetActive(true);
                        classSpecializationButton.gameObject.SetActive(true);
                        break;
                    }
                }
            }

        }

        public void SetPlayerName(string newPlayerName) {
            NewGamePanel.Instance.SetPlayerName(newPlayerName);
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


        public void ClosePanel() {
            //Debug.Log("CharacterCreatorPanel.ClosePanel()");
            SystemGameManager.Instance.UIManager.SystemWindowManager.characterCreatorWindow.CloseWindow();
        }

        public void SetCharacterClass(CharacterClass newCharacterClass) {
            //if (newCharacterClass != null && SystemGameManager.Instance.SystemConfigurationManager.NewGameClass == true) {
            if (SystemGameManager.Instance.SystemConfigurationManager.NewGameClass == true) {
                characterClassLabel.SetActive(true);
                characterClassButton.gameObject.SetActive(true);
                characterClassButton.AddCharacterClass(newCharacterClass);
            } else {
                characterClassLabel.SetActive(false);
                characterClassButton.gameObject.SetActive(false);
            }
        }

        public void SetFaction(Faction newfaction) {
            //if (newfaction != null && SystemGameManager.Instance.SystemConfigurationManager.NewGameFaction == true) {
            if (SystemGameManager.Instance.SystemConfigurationManager.NewGameFaction == true) {
                factionLabel.SetActive(true);
                factionButton.gameObject.SetActive(true);
                factionButton.AddFaction(newfaction);
            } else {
                factionLabel.SetActive(false);
                factionButton.gameObject.SetActive(false);
            }
        }

        public void SetClassSpecialization(ClassSpecialization newClassSpecialization) {
            //if (newClassSpecialization != null && SystemGameManager.Instance.SystemConfigurationManager.NewGameSpecialization == true) {
            if (SystemGameManager.Instance.SystemConfigurationManager.NewGameSpecialization == true) {
                classSpecializationLabel.SetActive(true);
                classSpecializationButton.gameObject.SetActive(true);
                classSpecializationButton.AddClassSpecialization(newClassSpecialization);
            } else {
                classSpecializationLabel.SetActive(false);
                classSpecializationButton.gameObject.SetActive(false);

            }
        }


    }

}