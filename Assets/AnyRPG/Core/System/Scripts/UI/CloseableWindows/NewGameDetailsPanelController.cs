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

        /*
        [SerializeField]
        private NavigableInputField playerNameInput = null;
        */

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

        private NewGamePanel newGamePanel = null;

        // game manager references
        private SystemDataFactory systemDataFactory = null;
        private UIManager uIManager = null;
        private NewGameManager newGameManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            /*
            playerNameInput.Configure(systemGameManager);
            characterClassButton.Configure(systemGameManager);
            classSpecializationButton.Configure(systemGameManager);
            factionButton.Configure(systemGameManager);
            */

            factionButton.OnInteract += OpenFactionPanel;
            characterClassButton.OnInteract += OpenClassPanel;
            classSpecializationButton.OnInteract += OpenSpecializationPanel;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemDataFactory = systemGameManager.SystemDataFactory;
            uIManager = systemGameManager.UIManager;
            newGameManager = systemGameManager.NewGameManager;
        }

        public void SetNewGamePanel(NewGamePanel newGamePanel) {
            this.newGamePanel = newGamePanel;
            parentPanel = newGamePanel;
        }

        public void OpenFactionPanel() {
            if (newGamePanel != null) {
                newGamePanel.OpenFactionPanel();
            }
        }

        public void OpenClassPanel() {
            if (newGamePanel != null) {
                newGamePanel.OpenClassPanel();
            }
        }

        public void OpenSpecializationPanel() {
            if (newGamePanel != null) {
                newGamePanel.OpenSpecializationPanel();
            }
        }

        public void ResetInputText(string newText) {
            textInput.text = newText;
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("CharacterCreatorPanel.OnCloseWindow()");
            base.ReceiveClosedWindowNotification();
            OnCloseWindow(this);
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("NewGameCharacterPanelController.ReceiveOpenWindowNotification()");
            ClearLabels();
            base.ReceiveOpenWindowNotification();
        }

        public void ClearLabels() {
            factionLabel.SetActive(false);
            factionButton.gameObject.SetActive(false);
            if (systemConfigurationManager.NewGameFaction == true) {
                foreach (Faction faction in systemDataFactory.GetResourceList<Faction>()) {
                    if (faction.NewGameOption == true) {
                        factionLabel.SetActive(true);
                        factionButton.gameObject.SetActive(true);
                        break;
                    }
                }
            }
            
            characterClassLabel.SetActive(false);
            characterClassButton.gameObject.SetActive(false);
            if (systemConfigurationManager.NewGameClass == true) {
                foreach (CharacterClass characterClass in systemDataFactory.GetResourceList<CharacterClass>()) {
                    if (characterClass.NewGameOption == true) {
                        characterClassLabel.SetActive(true);
                        characterClassButton.gameObject.SetActive(true);
                        break;
                    }
                }
            }

            classSpecializationLabel.SetActive(false);
            classSpecializationButton.gameObject.SetActive(false);
            if (systemConfigurationManager.NewGameSpecialization == true) {
                foreach (ClassSpecialization classSpecialization in systemDataFactory.GetResourceList<ClassSpecialization>()) {
                    if (classSpecialization.NewGameOption == true) {
                        classSpecializationLabel.SetActive(true);
                        classSpecializationButton.gameObject.SetActive(true);
                        break;
                    }
                }
            }
            /*
            foreach (UINavigationController uINavigationController in uINavigationControllers) {
                uINavigationController.UpdateNavigationList();
            }
            */

        }

        public void SetPlayerName(string newPlayerName) {
            newGameManager.SetPlayerName(newPlayerName);
        }

        public void HidePanel() {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            //RemoveFromWindowStack();
        }

        public void ShowPanel() {
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            //AddToWindowStack();
        }


        public void ClosePanel() {
            //Debug.Log("CharacterCreatorPanel.ClosePanel()");
            uIManager.characterCreatorWindow.CloseWindow();
        }

        public void SetCharacterClass(CharacterClass newCharacterClass) {
            //if (newCharacterClass != null && systemConfigurationManager.NewGameClass == true) {
            if (systemConfigurationManager.NewGameClass == true) {
                characterClassLabel.SetActive(true);
                characterClassButton.gameObject.SetActive(true);
                characterClassButton.AddCharacterClass(newCharacterClass);
            } else {
                characterClassLabel.SetActive(false);
                characterClassButton.gameObject.SetActive(false);
            }
        }

        public void SetFaction(Faction newfaction) {
            //if (newfaction != null && systemConfigurationManager.NewGameFaction == true) {
            if (systemConfigurationManager.NewGameFaction == true) {
                factionLabel.SetActive(true);
                factionButton.gameObject.SetActive(true);
                factionButton.AddFaction(newfaction);
            } else {
                factionLabel.SetActive(false);
                factionButton.gameObject.SetActive(false);
            }
        }

        public void SetClassSpecialization(ClassSpecialization newClassSpecialization) {
            //if (newClassSpecialization != null && systemConfigurationManager.NewGameSpecialization == true) {
            if (systemConfigurationManager.NewGameSpecialization == true) {
                classSpecializationLabel.SetActive(true);
                classSpecializationButton.gameObject.SetActive(true);
                classSpecializationButton.AddClassSpecialization(newClassSpecialization);
            } else {
                classSpecializationLabel.SetActive(false);
                classSpecializationButton.gameObject.SetActive(false);

            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            factionButton.OnInteract -= OpenFactionPanel;
        }


    }

}