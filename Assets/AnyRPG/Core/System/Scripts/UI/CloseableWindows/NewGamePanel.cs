using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class NewGamePanel : WindowContentController {

        public override event Action<CloseableWindowContents> OnCloseWindow = delegate { };

        [Header("New Game Panel")]

        [SerializeField]
        private TextMeshProUGUI playerNameLabel = null;

        [SerializeField]
        private GameObject panelParent = null;

        [SerializeField]
        private CharacterPreviewPanelController characterPreviewPanel = null;

        [SerializeField]
        private NewGameDetailsPanelController detailsPanel = null;

        [SerializeField]
        private NewGameMecanimCharacterPanelController characterPanel = null;

        [SerializeField]
        private DefaultAppearancePanel defaultAppearancePanel = null;

        [SerializeField]
        private NewGameFactionPanelController factionPanel = null;

        [SerializeField]
        private NewGameRacePanelController racePanel = null;

        [SerializeField]
        private NewGameClassPanelController classPanel = null;

        [SerializeField]
        private NewGameSpecializationPanelController specializationPanel = null;

        [Header("Bottom Buttons")]
        /*
        [SerializeField]
        private HighlightButton returnButton = null;
        */

        [SerializeField]
        private UINavigationController mainNavigationController = null;

        [SerializeField]
        private HighlightButton detailsButton = null;

        [SerializeField]
        private HighlightButton characterButton = null;

        [SerializeField]
        private HighlightButton appearanceButton = null;

        [SerializeField]
        private HighlightButton factionButton = null;

        [SerializeField]
        private HighlightButton raceButton = null;

        [SerializeField]
        private HighlightButton classButton = null;

        [SerializeField]
        private HighlightButton specializationButton = null;

        private Dictionary<GameObject, AppearancePanel> appearanceEditorPanels = new Dictionary<GameObject, AppearancePanel>();
        private Dictionary<Type, GameObject> appearanceEditorPanelTypes = new Dictionary<Type, GameObject>();
        private AppearancePanel currentAppearanceEditorPanel = null;


        // game manager references
        protected SaveManager saveManager = null;
        protected SystemDataFactory systemDataFactory = null;
        protected CharacterCreatorManager characterCreatorManager = null;
        protected UIManager uIManager = null;
        protected LevelManager levelManager = null;
        protected NewGameManager newGameManager = null;
        protected ObjectPooler objectPooler = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            characterPreviewPanel.Configure(systemGameManager);

            detailsPanel.SetNewGamePanel(this);
            classPanel.SetNewGamePanel(this);
            factionPanel.SetNewGamePanel(this);
            racePanel.SetNewGamePanel(this);
            specializationPanel.SetNewGamePanel(this);

            defaultAppearancePanel.SetCharacterEditor(newGameManager);

            AddDefaultAppearancePanel();
            GetAvailableAppearancePanels();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            saveManager = systemGameManager.SaveManager;
            systemDataFactory = systemGameManager.SystemDataFactory;
            characterCreatorManager = systemGameManager.CharacterCreatorManager;
            uIManager = systemGameManager.UIManager;
            levelManager = systemGameManager.LevelManager;
            newGameManager = systemGameManager.NewGameManager;
            objectPooler = systemGameManager.ObjectPooler;
        }

        private void AddDefaultAppearancePanel() {
            appearanceEditorPanels.Add(defaultAppearancePanel.gameObject, defaultAppearancePanel);
        }

        private void GetAvailableAppearancePanels() {

            foreach (AppearanceEditorProfile appearanceEditorProfile in systemDataFactory.GetResourceList<AppearanceEditorProfile>()) {
                if (appearanceEditorProfile.ModelProviderType != null) {
                    appearanceEditorPanelTypes.Add(appearanceEditorProfile.ModelProviderType, appearanceEditorProfile.Prefab);
                }
            }
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("NewGamePanel.ReceiveClosedWindowNotification()");
            base.ReceiveClosedWindowNotification();

            newGameManager.OnSetPlayerName -= HandleSetPlayerName;
            newGameManager.OnResetPlayerName -= HandleResetPlayerName;
            newGameManager.OnSetUnitProfile -= HandleSetUnitProfile;
            newGameManager.OnSetFaction -= HandleSetFaction;
            newGameManager.OnSetCharacterRace -= HandleSetCharacterRace;
            newGameManager.OnSetCharacterClass -= HandleSetCharacterClass;
            newGameManager.OnSetClassSpecialization -= HandleSetClassSpecialization;
            newGameManager.OnUpdateEquipmentList -= HandleUpdateEquipmentList;
            newGameManager.OnUpdateFactionList -= HandleUpdateFactionList;
            newGameManager.OnUpdateCharacterRaceList -= HandleUpdateCharacterRaceList;
            newGameManager.OnUpdateCharacterClassList -= HandleUpdateCharacterClassList;
            newGameManager.OnUpdateClassSpecializationList -= HandleUpdateClassSpecializationList;
            newGameManager.OnUpdateUnitProfileList -= HandleUpdateUnitProfileList;

            saveManager.ClearSharedData();
            //characterPreviewPanel.OnTargetCreated -= HandleTargetCreated;
            //characterPreviewPanel.OnTargetReady -= HandleTargetReady;

            characterCreatorManager.OnUnitCreated -= HandleUnitCreated;
            characterCreatorManager.OnModelCreated -= HandleModelCreated;

            characterPreviewPanel.ReceiveClosedWindowNotification();

            foreach (AppearancePanel appearancePanel in appearanceEditorPanels.Values) {
                appearancePanel.ReceiveClosedWindowNotification();
            }
            defaultAppearancePanel.ReceiveClosedWindowNotification();

            characterPanel.ReceiveClosedWindowNotification();
            factionPanel.ReceiveClosedWindowNotification();
            racePanel.ReceiveClosedWindowNotification();
            classPanel.ReceiveClosedWindowNotification();
            specializationPanel.ReceiveClosedWindowNotification();
            detailsPanel.ReceiveClosedWindowNotification();

            characterCreatorManager.DisableLight();
            characterCreatorManager.DespawnEnvironmentPreviewPrefab();
            OnCloseWindow(this);
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("NewGamePanel.ProcessOpenWindowNotification()");

            base.ProcessOpenWindowNotification();

            ProcessOpenWindow();
        }

        private void ProcessOpenWindow() {
            //Debug.Log("NewGamePanel.ProcessOpenWindow()");

            newGameManager.OnSetPlayerName += HandleSetPlayerName;
            newGameManager.OnResetPlayerName += HandleResetPlayerName;
            newGameManager.OnSetUnitProfile += HandleSetUnitProfile;
            newGameManager.OnSetFaction += HandleSetFaction;
            newGameManager.OnSetCharacterRace += HandleSetCharacterRace;
            newGameManager.OnSetCharacterClass += HandleSetCharacterClass;
            newGameManager.OnSetClassSpecialization += HandleSetClassSpecialization;
            newGameManager.OnUpdateEquipmentList += HandleUpdateEquipmentList;
            newGameManager.OnUpdateFactionList += HandleUpdateFactionList;
            newGameManager.OnUpdateCharacterRaceList += HandleUpdateCharacterRaceList;
            newGameManager.OnUpdateCharacterClassList += HandleUpdateCharacterClassList;
            newGameManager.OnUpdateClassSpecializationList += HandleUpdateClassSpecializationList;
            newGameManager.OnUpdateUnitProfileList += HandleUpdateUnitProfileList;

            ClearData();

            ClearButtons();

            saveManager.ClearSharedData();

            factionPanel.ReceiveOpenWindowNotification();
            racePanel.ReceiveOpenWindowNotification();
            // class goes before specialization because it acts as a filter for it
            classPanel.ReceiveOpenWindowNotification();
            specializationPanel.ReceiveOpenWindowNotification();

            newGameManager.SetupSaveData();

            // now that faction is set, and character panel is opened (which caused the first available unit to be selected), it's time to render the unit
            // inform the preview panel so the character can be rendered
            //characterPreviewPanel.OnTargetCreated += HandleTargetCreated;
            //characterPreviewPanel.OnTargetReady += HandleTargetReady;
            characterCreatorManager.OnUnitCreated += HandleUnitCreated;
            characterCreatorManager.OnModelCreated += HandleModelCreated;

            characterPreviewPanel.CapabilityConsumer = newGameManager;
            characterPreviewPanel.ReceiveOpenWindowNotification();

            //Debug.Log("Preview Unit Ready: " + characterCreatorManager?.PreviewUnitController?.CameraTargetReady);

            defaultAppearancePanel.ReceiveOpenWindowNotification();

            // details should be last because it relies on all the information set in the previous methods
            detailsPanel.ReceiveOpenWindowNotification();

            mainNavigationController.SetCurrentButton(detailsButton);
            if (controlsManager.GamePadInputActive == true) {
                mainNavigationController.FocusCurrentButton();
            }


            OpenDetailsPanel();

            if (systemConfigurationManager.NewGameAudioProfile != null) {
                audioManager.StopMusic();
                audioManager.PlayMusic(systemConfigurationManager.NewGameAudioProfile.AudioClip);
            }

            characterCreatorManager.EnableLight();
        }

        public void ClearButtons() {
            // disable character button if option not allowed or no faction exists
            if (systemConfigurationManager.NewGameAppearance == true) {
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

            // disable character button if not allowed
            if (systemConfigurationManager.CharacterSelectionType == CharacterSelectionType.CharacterList) {
                characterButton.gameObject.SetActive(true);
            } else {
                characterButton.gameObject.SetActive(false);
            }

            // disable race button if option not allowed
            if (systemConfigurationManager.CharacterSelectionType == CharacterSelectionType.RaceAndGender) {
                raceButton.gameObject.SetActive(true);
            } else {
                raceButton.gameObject.SetActive(false);
            }

            // disable class button if option not allowed
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
                    specializationButton.gameObject.SetActive(false);
                    foreach (ClassSpecialization classSpecialization in systemDataFactory.GetResourceList<ClassSpecialization>()) {
                        if (classSpecialization.NewGameOption == true) {
                            specializationButton.gameObject.SetActive(true);
                            break;
                        }
                    }
                } else {
                    specializationButton.gameObject.SetActive(false);
                }
            } else {
                specializationButton.gameObject.SetActive(false);
            }

            uINavigationControllers[0].UpdateNavigationList();
        }

        private void ClearData() {
            //Debug.Log("NewGamePanel.ClearData()");
            newGameManager.ClearData();

            //detailsPanel.ResetInputText(newGameManager.PlayerName);
        }

        public void HandleResetPlayerName(string newPlayerName) {
            //Debug.Log($"NewGamePanel.HandleResetPlayerName({newPlayerName})");

            detailsPanel.ResetInputText(newGameManager.PlayerName);
            SetPlayerNameText(newPlayerName);
        }

        public void HandleSetPlayerName(string newPlayerName) {
            //Debug.Log($"NewGamePanel.HandleSetPlayerName({newPlayerName})");

            SetPlayerNameText(newPlayerName);
        }

        private void SetPlayerNameText(string newPlayerName) {
            //Debug.Log($"NewGamePanel.SetPlayerNameText({newPlayerName})");

            playerNameLabel.text = newPlayerName;
        }

        public void HandleSetUnitProfile(UnitProfile newUnitProfile) {
            //Debug.Log("NewGamePanel.HandleSetUnitProfile(" + newUnitProfile.DisplayName + ")");

            characterPreviewPanel.ReloadUnit();
            characterPanel.SetUnitProfile(newUnitProfile);
        }

        public void HandleSetCharacterClass(CharacterClass newCharacterClass) {
            //Debug.Log("NewGamePanel.HandleShowCharacterClass()");
            detailsPanel.SetCharacterClass(newCharacterClass);

            classPanel.SetCharacterClass(newCharacterClass);
        }

        public void HandleSetClassSpecialization(ClassSpecialization newClassSpecialization) {
            //Debug.Log("NewGamePanel.HandleSetClassSpecialization(" + (newClassSpecialization == null ? "null" : newClassSpecialization.DisplayName) + ")");

            detailsPanel.SetClassSpecialization(newGameManager.ClassSpecialization);

            specializationPanel.SetClassSpecialization(newClassSpecialization);

            if (newGameManager.ClassSpecialization != null) {
                specializationButton.Button.interactable = true;
            } else {
                specializationButton.Button.interactable = false;
            }

            uINavigationControllers[0].UpdateNavigationList();
            uINavigationControllers[0].HighlightCurrentButton();
        }

        public void HandleSetFaction(Faction newFaction) {
            //Debug.Log("NewGamePanel.HandleShowFaction()");

            detailsPanel.SetFaction(newFaction);

            factionPanel.SetFaction(newFaction);

        }

        public void HandleSetCharacterRace(CharacterRace newRace) {
            //Debug.Log("NewGamePanel.HandleSetRace()");

            detailsPanel.SetCharacterRace(newRace);

            racePanel.SetCharacterRace(newRace);

        }

        public void HandleUpdateEquipmentList() {
            //Debug.Log("NameGamePanel.UpdateEquipmentList()");

            // show the equipment
            EquipCharacter();

        }

        public void HandleUpdateUnitProfileList() {
            //Debug.Log("NameGamePanel.HandleUpdateUnitProfileList()");
            characterPanel.ShowOptionButtons();
        }

        public void HandleUpdateFactionList() {
            //Debug.Log("NameGamePanel.HandleUpdateFactionList()");

            factionPanel.ShowOptionButtons();
        }

        public void HandleUpdateCharacterRaceList() {
            //Debug.Log("NameGamePanel.HandleUpdateCharacterRaceList()");

            racePanel.ShowOptionButtons();
        }

        public void HandleUpdateCharacterClassList() {
            //Debug.Log("NameGamePanel.HandleUpdateCharacterClassList()");

            classPanel.ShowOptionButtons();
        }

        public void HandleUpdateClassSpecializationList() {
            //Debug.Log("NameGamePanel.HandleUpdateClassSpecializationList()");

            specializationPanel.ShowOptionButtons();
        }

        /*
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
        */

        private void ClosePanels(CloseableWindowContents skipPanel) {
            if (skipPanel != detailsPanel) {
                detailsPanel.HidePanel();
            }
            if (skipPanel != characterPanel) {
                characterPanel.HidePanel();
            }
            foreach (AppearancePanel appearancePanel in appearanceEditorPanels.Values) {
                if (skipPanel != appearancePanel) {
                    appearancePanel.HidePanel();
                }
            }
            if (skipPanel != factionPanel) {
                factionPanel.HidePanel();
            }
            if (skipPanel != racePanel) {
                racePanel.HidePanel();
            }
            if (skipPanel != specializationPanel) {
                specializationPanel.HidePanel();
            }
            if (skipPanel != classPanel) {
                classPanel.HidePanel();
            }
        }

        public void OpenDetailsPanel() {
            //Debug.Log("NewGamePanel.OpenDetailsPanel()");

            if (openSubPanel != detailsPanel) {
                ClosePanels(detailsPanel);
                detailsPanel.ShowPanel();
                SetOpenSubPanel(detailsPanel, false);
            }

            detailsButton.HighlightBackground();
            uINavigationControllers[0].UnHightlightButtonBackgrounds(detailsButton);
        }

        public void OpenCharacterPanel() {
            //Debug.Log("NewGamePanel.OpenCharacterPanel()");

            if (openSubPanel != characterPanel) {
                ClosePanels(characterPanel);
                characterPanel.ShowPanel();
                SetOpenSubPanel(characterPanel, false);
            }

            characterButton.HighlightBackground();
            uINavigationControllers[0].UnHightlightButtonBackgrounds(characterButton);
        }

        public void OpenAppearancePanel() {
            //Debug.Log("NewGamePanel.OpenAppearancePanel()");

            if (openSubPanel != currentAppearanceEditorPanel) {
                ClosePanels(currentAppearanceEditorPanel);
                currentAppearanceEditorPanel.ShowPanel();
                SetOpenSubPanel(currentAppearanceEditorPanel, false);
            }

            appearanceButton.HighlightBackground();
            uINavigationControllers[0].UnHightlightButtonBackgrounds(appearanceButton);
        }

        private void ActivateCorrectAppearancePanel() {
            //Debug.Log("NewGamePanel.ActivateCorrectAppearancePanel()");

            if (characterCreatorManager.PreviewUnitController == null) {
                Debug.Log("NewGamePanel.ActivateCorrectAppearancePanel() null");
            }
            if (characterCreatorManager.PreviewUnitController.UnitProfile == null) {
                Debug.Log("NewGamePanel.ActivateCorrectAppearancePanel() null");
            }
            if (characterCreatorManager.PreviewUnitController.UnitProfile.UnitPrefabProps.ModelProvider == null) {
                currentAppearanceEditorPanel = defaultAppearancePanel;
                return;
            }

            //Debug.Log("provider type is " + characterCreatorManager.PreviewUnitController.UnitProfile.UnitPrefabProps.ModelProvider.GetType());

            if (appearanceEditorPanelTypes.ContainsKey(characterCreatorManager.PreviewUnitController.UnitProfile.UnitPrefabProps.ModelProvider.GetType()) == false) {
                currentAppearanceEditorPanel = defaultAppearancePanel;
                return;
            }

            GameObject panelPrefab = appearanceEditorPanelTypes[characterCreatorManager.PreviewUnitController.UnitProfile.UnitPrefabProps.ModelProvider.GetType()];

            if (appearanceEditorPanels.ContainsKey(panelPrefab) == false) {
                AppearancePanel appearancePanel = objectPooler.GetPooledObject(panelPrefab, panelParent.transform).GetComponent<AppearancePanel>();
                appearancePanel.Configure(systemGameManager);
                appearancePanel.SetParentPanel(this);
                appearancePanel.SetCharacterEditor(newGameManager);
                appearancePanel.ReceiveOpenWindowNotification();
                appearancePanel.transform.SetSiblingIndex(1);
                appearanceEditorPanels.Add(panelPrefab, appearancePanel);
                subPanels.Add(appearancePanel);
            }

            currentAppearanceEditorPanel = appearanceEditorPanels[panelPrefab];
            //currentAppearancePanel.SetupOptions();
        }

        public void OpenFactionPanel(bool focus) {
            //Debug.Log($"NewGamePanel.OpenFactionPanel({focus})");

            if (openSubPanel != factionPanel) {
                ClosePanels(factionPanel);
                factionPanel.ShowPanel();
                SetOpenSubPanel(factionPanel, focus);
            }

            uINavigationControllers[0].SetCurrentButton(factionButton);
            factionButton.HighlightBackground();
            uINavigationControllers[0].UnHightlightButtonBackgrounds(factionButton);
        }

        public void OpenRacePanel(bool focus) {
            //Debug.Log($"NewGamePanel.OpenRacePanel({focus})");

            if (openSubPanel != racePanel) {
                ClosePanels(racePanel);
                racePanel.ShowPanel();
                SetOpenSubPanel(racePanel, focus);
            }

            uINavigationControllers[0].SetCurrentButton(raceButton);
            raceButton.HighlightBackground();
            uINavigationControllers[0].UnHightlightButtonBackgrounds(raceButton);
        }

        public void OpenClassPanel(bool focus) {
            //Debug.Log($"NewGamePanel.OpenClassPanel({focus})");

            if (openSubPanel != classPanel) {
                ClosePanels(classPanel);
                classPanel.ShowPanel();
                SetOpenSubPanel(classPanel, focus);
            }

            uINavigationControllers[0].SetCurrentButton(classButton);
            classButton.HighlightBackground();
            uINavigationControllers[0].UnHightlightButtonBackgrounds(classButton);
        }

        public void OpenSpecializationPanel(bool focus) {
            //Debug.Log($"NewGamePanel.OpenSpecializationPanel({focus})");

            // this is only called from buttons, so safe to assume it's already been populated with buttons when the window opened or a class was selected
            if (specializationPanel.OptionButtons.Count > 0 && openSubPanel != specializationPanel) {
                ClosePanels(specializationPanel);
                specializationPanel.ShowPanel();
                SetOpenSubPanel(specializationPanel, focus);
            }

            uINavigationControllers[0].SetCurrentButton(specializationButton);
            specializationButton.HighlightBackground();
            uINavigationControllers[0].UnHightlightButtonBackgrounds(specializationButton);
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

        public void HandleUnitCreated() {
            //Debug.Log("NewGamePanel.HandleUnitCreated()");

            ActivateCorrectAppearancePanel();
            currentAppearanceEditorPanel.HandleUnitCreated();

            EquipCharacter();
        }


        public void HandleModelCreated() {
            //Debug.Log("NewGamePanel.HandleModelCreated()");

            currentAppearanceEditorPanel.SetupOptions();
        }

        public void EquipCharacter() {
            //Debug.Log("NewGamePanel.EquipCharacter()");

            if (characterCreatorManager.PreviewUnitController == null) {
                // if this is called on the initial load then the preview panel isn't open yet
                //Debug.Log("NewGamePanel.EquipCharacter(): no preview unit available");
                return;
            }

            // set character class etc first so preview works and can equip character
            SetCharacterProperties();

            int changes = 0;
            CharacterEquipmentManager characterEquipmentManager = characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager;
            if (characterEquipmentManager != null) {
                //Debug.Log("NewGameCharacterPanelController.EquipCharacter(): found equipment manager");

                // unequip equipment not in current list
                List<Equipment> removeList = new List<Equipment>();
                foreach (Equipment equipment in characterEquipmentManager.CurrentEquipment.Values) {
                    //Debug.Log("NewGamePanel.EquipCharacter(): checking for removal : " + (equipment == null ? "null" : equipment.DisplayName));
                    if (equipment != null && !newGameManager.EquipmentList.ContainsValue(equipment)) {
                        removeList.Add(equipment);
                    }
                }
                foreach (Equipment equipment in removeList) {
                    //characterEquipmentManager.Unequip(equipment, false, false, false);
                    characterEquipmentManager.Unequip(equipment);
                    changes++;
                }

                // equip equipment in list but not yet equipped
                if (newGameManager.EquipmentList != null) {
                    //Debug.Log("NewGameCharacterPanelController.EquipCharacter(): equipment list is not null");
                    foreach (EquipmentSlotProfile equipmentSlotProfile in newGameManager.EquipmentList.Keys) {
                        //Debug.Log("NewGameCharacterPanelController.EquipCharacter(): ask to equip: " + equipment.DisplayName);
                        if (characterEquipmentManager.CurrentEquipment.ContainsKey(equipmentSlotProfile) == false
                            || characterEquipmentManager.CurrentEquipment[equipmentSlotProfile] == null
                            || characterEquipmentManager.CurrentEquipment[equipmentSlotProfile] != newGameManager.EquipmentList[equipmentSlotProfile]) {
                            characterEquipmentManager.Equip(newGameManager.EquipmentList[equipmentSlotProfile], equipmentSlotProfile);
                            changes++;
                        }
                    }
                }
                if (characterPreviewPanel.CharacterReady == true && changes > 0) {
                    //Debug.Log("NewGamePanel.EquipCharacter(): character is ready");
                    characterCreatorManager.PreviewUnitController.UnitModelController.RebuildModelAppearance();
                }
            }
        }

        public void ClosePanel() {
            //Debug.Log("NewGamePanel.ClosePanel()");
            uIManager.newGameWindow.CloseWindow();
            levelManager.PlayLevelSounds();
        }

        public void NewGame() {
            //Debug.Log("NewGamePanel.NewGame()");

            SaveAppearanceData(newGameManager.SaveData);

            uIManager.confirmNewGameMenuWindow.OpenWindow();
        }

        public void SaveAppearanceData(AnyRPGSaveData saveData) {
            //Debug.Log("CharacterPreviewPanelController.SaveAppearanceData()");

            if (characterCreatorManager.PreviewUnitController?.UnitModelController == null) {
                return;
            }
            characterCreatorManager.PreviewUnitController.UnitModelController.SaveAppearanceSettings(newGameManager, saveData);
        }


    }

}