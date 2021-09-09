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

        [Header("Button Buttons")]

        [SerializeField]
        private HighlightButton returnButton = null;

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

        [SerializeField]
        private HighlightButton startButton = null;

        /*
        private string playerName = "Player Name";
        private UnitProfile unitProfile = null;
        private UnitType unitType = null;
        private CharacterRace characterRace = null;
        private CharacterClass characterClass = null;
        private ClassSpecialization classSpecialization = null;
        private Faction faction = null;

        private CapabilityConsumerProcessor capabilityConsumerProcessor = null;

        private AnyRPGSaveData saveData;

        private Dictionary<EquipmentSlotType, Equipment> equipmentList = new Dictionary<EquipmentSlotType, Equipment>();
        */

        // game manager references
        private SaveManager saveManager = null;
        private SystemDataFactory systemDataFactory = null;
        private CharacterCreatorManager characterCreatorManager = null;
        private UIManager uIManager = null;
        private LevelManager levelManager = null;
        private NewGameManager newGameManager = null;

        /*
        public CharacterClass CharacterClass { get => characterClass; set => characterClass = value; }
        public ClassSpecialization ClassSpecialization { get => classSpecialization; set => classSpecialization = value; }
        public Faction Faction { get => faction; set => faction = value; }
        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }
        public AnyRPGSaveData SaveData { get => saveData; set => saveData = value; }
        public Dictionary<EquipmentSlotType, Equipment> EquipmentList { get => equipmentList; set => equipmentList = value; }
        public UnitType UnitType { get => unitType; set => unitType = value; }
        public CharacterRace CharacterRace { get => characterRace; set => characterRace = value; }
        public CapabilityConsumerProcessor CapabilityConsumerProcessor { get => capabilityConsumerProcessor; }
        */

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            returnButton.Configure(systemGameManager);
            detailsButton.Configure(systemGameManager);
            characterButton.Configure(systemGameManager);
            appearanceButton.Configure(systemGameManager);
            factionButton.Configure(systemGameManager);
            classButton.Configure(systemGameManager);
            specializationButton.Configure(systemGameManager);
            startButton.Configure(systemGameManager);

            startButton = null;
            characterPreviewPanel.Configure(systemGameManager);
            detailsPanel.Configure(systemGameManager);
            characterPanel.Configure(systemGameManager);
            umaCharacterPanel.Configure(systemGameManager);
            classPanel.Configure(systemGameManager);
            factionPanel.Configure(systemGameManager);
            specializationPanel.Configure(systemGameManager);
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

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("LoadGamePanel.OnCloseWindow()");
            base.RecieveClosedWindowNotification();

            newGameManager.OnSetPlayerName -= HandleSetPlayerName;
            newGameManager.OnSetUnitProfile -= HandleSetUnitProfile;
            newGameManager.OnUpdateEquipmentList -= HandleUpdateEquipmentList;
            newGameManager.OnShowCharacterClass -= HandleShowCharacterClass;
            newGameManager.OnChangeCharacterClass -= HandleChangeCharacterClass;
            newGameManager.OnShowClassSpecialization -= HandleShowClassSpecialization;
            newGameManager.OnShowFaction -= HandleShowFaction;

            saveManager.ClearSharedData();
            characterPreviewPanel.OnTargetCreated -= HandleTargetCreated;
            characterPreviewPanel.OnTargetReady -= HandleTargetReady;
            characterPreviewPanel.RecieveClosedWindowNotification();
            if (systemConfigurationManager.NewGameUMAAppearance == true) {
                umaCharacterPanel.RecieveClosedWindowNotification();
            } else {
                characterPanel.RecieveClosedWindowNotification();
            }
            specializationPanel.RecieveClosedWindowNotification();
            factionPanel.RecieveClosedWindowNotification();
            classPanel.RecieveClosedWindowNotification();
            detailsPanel.RecieveClosedWindowNotification();
            OnCloseWindow(this);
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("LoadGamePanel.OnOpenWindow()");
            newGameManager.OnSetPlayerName += HandleSetPlayerName;
            newGameManager.OnSetUnitProfile += HandleSetUnitProfile;
            newGameManager.OnUpdateEquipmentList += HandleUpdateEquipmentList;
            newGameManager.OnShowCharacterClass += HandleShowCharacterClass;
            newGameManager.OnChangeCharacterClass += HandleChangeCharacterClass;
            newGameManager.OnShowClassSpecialization += HandleShowClassSpecialization;
            newGameManager.OnShowFaction += HandleShowFaction;

            ClearData();

            ClearButtons();

            saveManager.ClearSharedData();

            newGameManager.SetupSaveData();

            /*
            if (systemConfigurationManager.NewGameUMAAppearance == true) {
                umaCharacterPanel.ReceiveOpenWindowNotification();
                // first, inform the preview panel so the character can be rendered
            }
            characterPreviewPanel.OnTargetReady += HandleTargetReady;
            characterPreviewPanel.CapabilityConsumer = this;
            characterPreviewPanel.ReceiveOpenWindowNotification();
            */

            factionPanel.ReceiveOpenWindowNotification();

            /*
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
            */

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

            OpenDetailsPanel();

            // testing appearance last since it relies on at very minimum the unit profile being set

            if (systemConfigurationManager.NewGameAudioProfile != null) {
                audioManager.StopMusic();
                audioManager.PlayMusic(systemConfigurationManager.NewGameAudioProfile.AudioClip);
            }
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


        public void HandleSetUnitProfile(NewGameUnitButton newGameUnitButton) {
            //Debug.Log("NewGamePanel.SetUnitProfile(" + newGameUnitButton.UnitProfile.DisplayName + ")");

            characterPreviewPanel.ReloadUnit();
            characterPanel.SetBody(newGameUnitButton);
        }

        public void HandleSetPlayerName(string newPlayerName) {
            playerNameLabel.text = newPlayerName;
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
            ClosePanels();
            detailsPanel.ShowPanel();
        }

        public void OpenClassPanel() {
            ClosePanels();
            classPanel.ShowPanel();
        }

        public void OpenCharacterPanel() {
            ClosePanels();
            characterPanel.ShowPanel();
        }

        public void OpenAppearancePanel() {
            ClosePanels();
            umaCharacterPanel.ShowPanel();
        }

        public void OpenFactionPanel() {
            //Debug.Log("NewGamePanel.OpenFactionPanel()");

            ClosePanels();
            factionPanel.ShowPanel();
        }

        public void OpenSpecializationPanel() {
            // this is only called from buttons, so safe to assume it's already been populated with buttons when the window opened or a class was selected
            if (specializationPanel.OptionButtons.Count > 0) {
                ClosePanels();
                specializationPanel.ShowPanel();
            }
        }

        public void HandleChangeCharacterClass(NewGameCharacterClassButton newGameCharacterClassButton) {
            detailsPanel.SetCharacterClass(newGameCharacterClassButton.CharacterClass);

            // since a new character class is chosen, the specialization list must be updated to match the class
            specializationPanel.ShowOptionButtonsCommon();

            // the specialization must also be updated on the details panel
            detailsPanel.SetClassSpecialization(newGameManager.ClassSpecialization);

            if (newGameManager.ClassSpecialization != null) {
                specializationButton.Button.interactable = true;
            } else {
                specializationButton.Button.interactable = false;
            }
        }

        public void HandleShowCharacterClass(NewGameCharacterClassButton newGameCharacterClassButton) {
            //Debug.Log("NewGamePanel.HandleShowCharacterClass()");

            classPanel.ShowCharacterClass(newGameCharacterClassButton);
        }

        public void HandleShowClassSpecialization(NewGameClassSpecializationButton newGameClassSpecializationButton) {
            specializationPanel.ShowClassSpecialization(newGameClassSpecializationButton);

            detailsPanel.SetClassSpecialization(newGameClassSpecializationButton.ClassSpecialization);
        }

        public void HandleShowFaction(NewGameFactionButton newGameFactionButton) {
            //Debug.Log("NewGamePanel.HandleShowFaction()");

            factionPanel.ShowFaction(newGameFactionButton);

            detailsPanel.SetFaction(newGameFactionButton.Faction);

            /*
            if (systemConfigurationManager.NewGameUMAAppearance == false) {
                characterPanel.ShowOptionButtonsCommon();
            }
            */

        }

        public void HandleUpdateEquipmentList() {
            //Debug.Log("NameGamePanel.UpdateEquipmentList()");

            // show the equipment
            EquipCharacter();

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