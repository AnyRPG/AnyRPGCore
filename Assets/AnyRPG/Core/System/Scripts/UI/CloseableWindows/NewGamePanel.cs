using AnyRPG;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class NewGamePanel : WindowContentController {

        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        private TextMeshProUGUI playerNameLabel = null;

        [SerializeField]
        private CharacterPreviewPanelController characterPreviewPanel = null;

        [SerializeField]
        private NewGameDetailsPanelController detailsPanel = null;

        [SerializeField]
        private NewGameMecanimCharacterPanelController characterPanel = null;

        [SerializeField]
        private UMACharacterEditorPanelController umaCharacterPanel = null;

        [SerializeField]
        private NewGameClassPanelController classPanel = null;

        [SerializeField]
        private NewGameFactionPanelController factionPanel = null;

        [SerializeField]
        private NewGameSpecializationPanelController specializationPanel = null;

        [Header("Bottom Buttons")]
        /*
        [SerializeField]
        private HighlightButton returnButton = null;
        */

        [SerializeField]
        private HighlightButton detailsButton = null;

        [SerializeField]
        private HighlightButton characterButton = null;

        [SerializeField]
        private HighlightButton appearanceButton = null;

        [SerializeField]
        private HighlightButton factionButton = null;

        [SerializeField]
        private HighlightButton classButton = null;

        [SerializeField]
        private HighlightButton specializationButton = null;

        /*
        [SerializeField]
        private HighlightButton startButton = null;
        */

        // game manager references
        protected SaveManager saveManager = null;
        protected SystemDataFactory systemDataFactory = null;
        protected CharacterCreatorManager characterCreatorManager = null;
        protected UIManager uIManager = null;
        protected LevelManager levelManager = null;
        protected NewGameManager newGameManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            //returnButton.Configure(systemGameManager);
            //detailsButton.Configure(systemGameManager);
            //characterButton.Configure(systemGameManager);
            //appearanceButton.Configure(systemGameManager);
            //factionButton.Configure(systemGameManager);
            //classButton.Configure(systemGameManager);
            //specializationButton.Configure(systemGameManager);
            //startButton.Configure(systemGameManager);

            characterPreviewPanel.Configure(systemGameManager);

            detailsPanel.Configure(systemGameManager);
            detailsPanel.SetNewGamePanel(this);

            characterPanel.Configure(systemGameManager);
            characterPanel.SetParentPanel(this);

            umaCharacterPanel.Configure(systemGameManager);
            umaCharacterPanel.SetParentPanel(this);

            classPanel.Configure(systemGameManager);
            classPanel.SetNewGamePanel(this);

            factionPanel.Configure(systemGameManager);
            factionPanel.SetNewGamePanel(this);

            specializationPanel.Configure(systemGameManager);
            specializationPanel.SetNewGamePanel(this);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            saveManager = systemGameManager.SaveManager;
            systemDataFactory = systemGameManager.SystemDataFactory;
            characterCreatorManager = systemGameManager.CharacterCreatorManager;
            uIManager = systemGameManager.UIManager;
            levelManager = systemGameManager.LevelManager;
            newGameManager = systemGameManager.NewGameManager;
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("LoadGamePanel.OnCloseWindow()");
            base.ReceiveClosedWindowNotification();

            newGameManager.OnSetPlayerName -= HandleSetPlayerName;
            newGameManager.OnSetUnitProfile -= HandleSetUnitProfile;
            newGameManager.OnSetCharacterClass -= HandleSetCharacterClass;
            newGameManager.OnSetClassSpecialization -= HandleSetClassSpecialization;
            newGameManager.OnSetFaction -= HandleSetFaction;
            newGameManager.OnUpdateEquipmentList -= HandleUpdateEquipmentList;
            newGameManager.OnUpdateFactionList -= HandleUpdateFactionList;
            newGameManager.OnUpdateCharacterClassList -= HandleUpdateCharacterClassList;
            newGameManager.OnUpdateClassSpecializationList -= HandleUpdateClassSpecializationList;
            newGameManager.OnUpdateUnitProfileList -= HandleUpdateUnitProfileList;

            saveManager.ClearSharedData();
            characterPreviewPanel.OnTargetCreated -= HandleTargetCreated;
            characterPreviewPanel.OnTargetReady -= HandleTargetReady;
            characterPreviewPanel.ReceiveClosedWindowNotification();
            if (systemConfigurationManager.NewGameUMAAppearance == true) {
                umaCharacterPanel.ReceiveClosedWindowNotification();
            } else {
                characterPanel.ReceiveClosedWindowNotification();
            }
            specializationPanel.ReceiveClosedWindowNotification();
            factionPanel.ReceiveClosedWindowNotification();
            classPanel.ReceiveClosedWindowNotification();
            detailsPanel.ReceiveClosedWindowNotification();
            OnCloseWindow(this);
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("LoadGamePanel.OnOpenWindow()");

            base.ReceiveOpenWindowNotification();

            newGameManager.OnSetPlayerName += HandleSetPlayerName;
            newGameManager.OnSetUnitProfile += HandleSetUnitProfile;
            newGameManager.OnSetCharacterClass += HandleSetCharacterClass;
            newGameManager.OnSetClassSpecialization += HandleSetClassSpecialization;
            newGameManager.OnSetFaction += HandleSetFaction;
            newGameManager.OnUpdateEquipmentList += HandleUpdateEquipmentList;
            newGameManager.OnUpdateFactionList += HandleUpdateFactionList;
            newGameManager.OnUpdateCharacterClassList += HandleUpdateCharacterClassList;
            newGameManager.OnUpdateClassSpecializationList += HandleUpdateClassSpecializationList;
            newGameManager.OnUpdateUnitProfileList += HandleUpdateUnitProfileList;

            ClearData();

            ClearButtons();

            saveManager.ClearSharedData();

            newGameManager.SetupSaveData();

            factionPanel.ReceiveOpenWindowNotification();


            // class goes before specialization because it acts as a filter for it
            classPanel.ReceiveOpenWindowNotification();
            specializationPanel.ReceiveOpenWindowNotification();
            
            // now that faction is set, and character panel is opened (which caused the first available unit to be selected), it's time to render the unit
            // inform the preview panel so the character can be rendered
            characterPreviewPanel.OnTargetReady += HandleTargetReady;
            characterPreviewPanel.OnTargetCreated += HandleTargetCreated;
            characterPreviewPanel.CapabilityConsumer = newGameManager;
            characterPreviewPanel.ReceiveOpenWindowNotification();

            // attempt to open the UMA window first
            if (systemConfigurationManager.NewGameUMAAppearance == true) {
                umaCharacterPanel.ReceiveOpenWindowNotification();
            }

            // if the UMA window is not in use, or there was no UMA unit available, try the mecanim window
            if (systemConfigurationManager.NewGameUMAAppearance == false
                || (systemConfigurationManager.NewGameUMAAppearance == true && characterCreatorManager.PreviewUnitController == null)) {
                characterPanel.ReceiveOpenWindowNotification();
            }

            // details should be last because it relies on all the information set in the previous methods
            detailsPanel.ReceiveOpenWindowNotification();

            //OpenDetailsPanel();
            uINavigationControllers[0].SetCurrentButton(detailsButton);
            SetNavigationController(uINavigationControllers[0]);
            detailsButton.Interact();

            // testing appearance last since it relies on at very minimum the unit profile being set

            if (systemConfigurationManager.NewGameAudioProfile != null) {
                audioManager.StopMusic();
                audioManager.PlayMusic(systemConfigurationManager.NewGameAudioProfile.AudioClip);
            }

            /*
            if (controlsManager.GamePadModeActive && focusFirstButtonOnOpen == true) {
                currentNavigationController.FocusFirstButton();
                return;
            }

            if (controlsManager.GamePadModeActive && focusActiveSubPanel == true) {
                openSubPanel.FocusFirstButton();
                return;
            }
            */

        }

        public void ClearButtons() {
            // disable character button if option not allowed or no faction exists
            if (systemConfigurationManager.NewGameAppearance == true) {
                characterButton.gameObject.SetActive(true);
            } else {
                characterButton.gameObject.SetActive(false);
            }

            if (systemConfigurationManager.NewGameUMAAppearance == true) {
                appearanceButton.gameObject.SetActive(true);
            } else {
                appearanceButton.gameObject.SetActive(false);
            }

            // disable faction button if option not allowed or no faction exists
            factionButton.gameObject.SetActive(false);
            if (systemConfigurationManager.NewGameFaction == true) {
                foreach (Faction faction in systemDataFactory.GetResourceList<Faction>()) {
                    if (faction.NewGameOption == true) {
                        factionButton.gameObject.SetActive(true);
                        break;
                    }
                }
            }

            // disable class button if option not allowed or no faction exists
            classButton.gameObject.SetActive(false);
            if (systemConfigurationManager.NewGameClass == true) {
                foreach (CharacterClass characterClass in systemDataFactory.GetResourceList<CharacterClass>()) {
                    if (characterClass.NewGameOption == true) {
                        classButton.gameObject.SetActive(true);
                        break;
                    }
                }
            }

            // disable specialization button if option not allowed or class button disabled (specializations do not have a specific new game option)
            if (systemConfigurationManager.NewGameSpecialization == true) {
                if (classButton.gameObject.activeSelf == true) {
                    specializationButton.gameObject.SetActive(true);
                } else {
                    specializationButton.gameObject.SetActive(false);
                }
            } else {
                specializationButton.gameObject.SetActive(false);
            }
        }

        private void ClearData() {
            newGameManager.ClearData();
            detailsPanel.ResetInputText(newGameManager.PlayerName);
        }

        public void HandleSetPlayerName(string newPlayerName) {
            playerNameLabel.text = newPlayerName;
        }

        public void HandleSetUnitProfile(UnitProfile newUnitProfile) {
            Debug.Log("NewGamePanel.HandleSetUnitProfile(" + newUnitProfile.DisplayName + ")");

            characterPreviewPanel.ReloadUnit();
            characterPanel.SetUnitProfile(newUnitProfile);
        }

        public void HandleSetCharacterClass(CharacterClass newCharacterClass) {
            //Debug.Log("NewGamePanel.HandleShowCharacterClass()");
            detailsPanel.SetCharacterClass(newCharacterClass);

            classPanel.SetCharacterClass(newCharacterClass);
        }

        public void HandleSetClassSpecialization(ClassSpecialization newClassSpecialization) {
            detailsPanel.SetClassSpecialization(newGameManager.ClassSpecialization);

            specializationPanel.SetClassSpecialization(newClassSpecialization);

            if (newGameManager.ClassSpecialization != null) {
                specializationButton.Button.interactable = true;
            } else {
                specializationButton.Button.interactable = false;
            }
        }

        public void HandleSetFaction(Faction newFaction) {
            //Debug.Log("NewGamePanel.HandleShowFaction()");

            detailsPanel.SetFaction(newFaction);

            factionPanel.SetFaction(newFaction);

        }

        public void HandleUpdateEquipmentList() {
            //Debug.Log("NameGamePanel.UpdateEquipmentList()");

            // show the equipment
            EquipCharacter();

        }

        public void HandleUpdateUnitProfileList() {
            characterPanel.ShowOptionButtons();
        }

        public void HandleUpdateFactionList() {
            factionPanel.ShowOptionButtons();
        }

        public void HandleUpdateCharacterClassList() {
            classPanel.ShowOptionButtons();
        }

        public void HandleUpdateClassSpecializationList() {
            specializationPanel.ShowOptionButtons();
        }

        protected void UnHightlightButtons(HighlightButton skipButton = null) {
            if (skipButton != detailsButton) {
                detailsButton.UnHighlightBackground();
            }
            if (skipButton != characterButton) {
                characterButton.UnHighlightBackground();
            }
            if (skipButton != appearanceButton) {
                appearanceButton.UnHighlightBackground();
            }
            if (skipButton != factionButton) {
                factionButton.UnHighlightBackground();
            }
            if (skipButton != classButton) {
                classButton.UnHighlightBackground();
            }
            if (skipButton != specializationButton) {
                specializationButton.UnHighlightBackground();
            }
        }

        private void ClosePanels() {
            characterPanel.HidePanel();
            umaCharacterPanel.HidePanel();
            classPanel.HidePanel();
            factionPanel.HidePanel();
            specializationPanel.HidePanel();
            detailsPanel.HidePanel();
        }

        public void OpenDetailsPanel() {
            Debug.Log("NewGamePanel.OpenDetailsPanel()");

            ClosePanels();
            detailsPanel.ShowPanel();
            SetOpenSubPanel(detailsPanel);
            UnHightlightButtons(detailsButton);
        }

        public void OpenClassPanel() {
            ClosePanels();
            classPanel.ShowPanel();
            SetOpenSubPanel(classPanel);
            UnHightlightButtons(classButton);
        }

        public void OpenCharacterPanel() {
            Debug.Log("NewGamePanel.OpenCharacterPanel()");

            ClosePanels();
            characterPanel.ShowPanel();
            SetOpenSubPanel(characterPanel);
            UnHightlightButtons(characterButton);
        }

        public void OpenAppearancePanel() {
            ClosePanels();
            umaCharacterPanel.ShowPanel();
            SetOpenSubPanel(umaCharacterPanel);
            UnHightlightButtons(appearanceButton);
        }

        public void OpenFactionPanel() {
            //Debug.Log("NewGamePanel.OpenFactionPanel()");

            ClosePanels();
            factionPanel.ShowPanel();
            SetOpenSubPanel(factionPanel);
            UnHightlightButtons(factionButton);
        }

        public void OpenSpecializationPanel() {
            // this is only called from buttons, so safe to assume it's already been populated with buttons when the window opened or a class was selected
            if (specializationPanel.OptionButtons.Count > 0) {
                ClosePanels();
                specializationPanel.ShowPanel();
                SetOpenSubPanel(specializationPanel);
            }
            UnHightlightButtons(specializationButton);
        }

        public void SetCharacterProperties() {
            //Debug.Log("NewGameCharacterPanelController.SetCharacterProperties()");

            characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.SetUnitProfile(newGameManager.UnitProfile, true, -1, false, false);
            //characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.SetUnitProfile(newGameManager.UnitProfile, true, -1, false, false);
            characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.SetUnitType(newGameManager.UnitType, true, true, false);
            characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.SetCharacterRace(newGameManager.CharacterRace, true, true, false);
            characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.SetCharacterClass(newGameManager.CharacterClass, true, true, false);
            characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.SetClassSpecialization(newGameManager.ClassSpecialization, true, true, false);
            characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.SetCharacterFaction(newGameManager.Faction, true, true, false);

        }

        public void HandleTargetCreated() {
            // just a reminder
            EquipCharacter();
        }


        public void HandleTargetReady() {
            //Debug.Log("NewGameCharacterPanelController.HandleTargetReady()");
            //EquipCharacter();
        }

        public void EquipCharacter() {
            //Debug.Log("NewGamePanel.EquipCharacter()");

            if (characterCreatorManager.PreviewUnitController == null) {
                // if this is called on the initial load then the preview panel isn't open yet
                //Debug.Log("NewGamePanel.EquipCharacter(): no preview unit available");
                return;
            }

            /*
            if (characterPreviewPanel.CharacterReady == false) {
                // attempting this before the character is spawned will make it go invisible (UMA bug)
                //Debug.Log("NewGameCharacterPanelController.EquipCharacter(): character not ready yet, exiting.");
                return;
            }
            */

            // set character class etc first so preview works and can equip character
            SetCharacterProperties();

            int changes = 0;
            CharacterEquipmentManager characterEquipmentManager = characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager;
            if (characterEquipmentManager != null) {
                //Debug.Log("NewGameCharacterPanelController.EquipCharacter(): found equipment manager");

                // unequip equipment not in current list
                //characterEquipmentManager.UnequipAll(false);
                List<Equipment> removeList = new List<Equipment>();
                foreach (Equipment equipment in characterEquipmentManager.CurrentEquipment.Values) {
                    //Debug.Log("NewGamePanel.EquipCharacter(): checking for removal : " + (equipment == null ? "null" : equipment.DisplayName));
                    if (equipment != null && !newGameManager.EquipmentList.ContainsValue(equipment)) {
                        removeList.Add(equipment);
                    }
                }
                foreach (Equipment equipment in removeList) {
                    //characterEquipmentManager.Unequip(equipment, false, false, false);
                    characterEquipmentManager.Unequip(equipment, true, true, false);
                    changes++;
                }

                // equip equipment in list but not yet equipped
                if (newGameManager.EquipmentList != null) {
                    //Debug.Log("NewGameCharacterPanelController.EquipCharacter(): equipment list is not null");
                    foreach (Equipment equipment in newGameManager.EquipmentList.Values) {
                        //Debug.Log("NewGameCharacterPanelController.EquipCharacter(): ask to equip: " + equipment.DisplayName);
                        if (!characterEquipmentManager.CurrentEquipment.ContainsValue(equipment)) {
                            characterEquipmentManager.Equip(equipment, null, false, false, false);
                            changes++;
                        }
                    }
                }
                if (characterPreviewPanel.CharacterReady == true && changes > 0) {
                    //Debug.Log("NewGamePanel.EquipCharacter(): character is ready");
                    characterCreatorManager.PreviewUnitController.UnitModelController.RebuildModelAppearance();
                }
                //characterPreviewPanel.BuildModelAppearance();
            }
        }

        public void ClosePanel() {
            //Debug.Log("CharacterCreatorPanel.ClosePanel()");
            uIManager.newGameWindow.CloseWindow();
            levelManager.PlayLevelSounds();
        }

        public void NewGame() {
            //Debug.Log("LoadGamePanel.NewGame()");

            newGameManager.SetPlayerUMARecipe(characterPreviewPanel.GetCurrentRecipe());

            uIManager.confirmNewGameMenuWindow.OpenWindow();
        }


    }

}