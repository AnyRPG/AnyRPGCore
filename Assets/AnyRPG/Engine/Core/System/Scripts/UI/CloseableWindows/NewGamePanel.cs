using AnyRPG;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class NewGamePanel : WindowContentController, ICapabilityConsumer {

        #region Singleton
        private static NewGamePanel instance;

        public static NewGamePanel Instance {
            get {
                return instance;
            }
        }

        private void Awake() {
            instance = this;
        }
        #endregion

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

        [SerializeField]
        private Button characterButton = null;

        [SerializeField]
        private Button appearanceButton = null;

        [SerializeField]
        private Button factionButton = null;

        [SerializeField]
        private Button classButton = null;

        [SerializeField]
        private Button specializationButton = null;


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

        public CharacterClass CharacterClass { get => characterClass; set => characterClass = value; }
        public ClassSpecialization ClassSpecialization { get => classSpecialization; set => classSpecialization = value; }
        public Faction Faction { get => faction; set => faction = value; }
        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }
        public AnyRPGSaveData SaveData { get => saveData; set => saveData = value; }
        public Dictionary<EquipmentSlotType, Equipment> EquipmentList { get => equipmentList; set => equipmentList = value; }
        public UnitType UnitType { get => unitType; set => unitType = value; }
        public CharacterRace CharacterRace { get => characterRace; set => characterRace = value; }
        public CapabilityConsumerProcessor CapabilityConsumerProcessor { get => capabilityConsumerProcessor; }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("LoadGamePanel.OnCloseWindow()");
            base.RecieveClosedWindowNotification();

            SystemGameManager.Instance.SaveManager.ClearSharedData();
            characterPreviewPanel.OnTargetReady -= HandleTargetReady;
            characterPreviewPanel.RecieveClosedWindowNotification();
            if (SystemGameManager.Instance.SystemConfigurationManager.NewGameUMAAppearance == true) {
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

        public void ClearButtons() {
            // disable character button if option not allowed or no faction exists
            if (SystemGameManager.Instance.SystemConfigurationManager.NewGameAppearance == true) {
                characterButton.gameObject.SetActive(true);
            } else {
                characterButton.gameObject.SetActive(false);
            }

            if (SystemGameManager.Instance.SystemConfigurationManager.NewGameUMAAppearance == true) {
                appearanceButton.gameObject.SetActive(true);
            } else {
                appearanceButton.gameObject.SetActive(false);
            }

            // disable faction button if option not allowed or no faction exists
            factionButton.gameObject.SetActive(false);
            if (SystemGameManager.Instance.SystemConfigurationManager.NewGameFaction == true) {
                foreach (Faction faction in SystemFactionManager.Instance.GetResourceList()) {
                    if (faction.NewGameOption == true) {
                        factionButton.gameObject.SetActive(true);
                        break;
                    }
                }
            }

            // disable class button if option not allowed or no faction exists
            classButton.gameObject.SetActive(false);
            if (SystemGameManager.Instance.SystemConfigurationManager.NewGameClass == true) {
                foreach (CharacterClass characterClass in SystemCharacterClassManager.Instance.GetResourceList()) {
                    if (characterClass.NewGameOption == true) {
                        classButton.gameObject.SetActive(true);
                        break;
                    }
                }
            }

            // disable specialization button if option not allowed or class button disabled (specializations do not have a specific new game option)
            if (SystemGameManager.Instance.SystemConfigurationManager.NewGameSpecialization == true) {
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
            playerName = "Player Name";
            detailsPanel.ResetInputText(playerName);
            unitProfile = null;
            unitType = null;
            characterRace = null;
            characterClass = null;
            classSpecialization = null;
            faction = null;
            capabilityConsumerProcessor = null;

        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("LoadGamePanel.OnOpenWindow()");
            ClearData();

            ClearButtons();

            SystemGameManager.Instance.SaveManager.ClearSharedData();

            SetupSaveData();

            /*
            if (SystemGameManager.Instance.SystemConfigurationManager.NewGameUMAAppearance == true) {
                umaCharacterPanel.ReceiveOpenWindowNotification();
                // first, inform the preview panel so the character can be rendered
            }
            characterPreviewPanel.OnTargetReady += HandleTargetReady;
            characterPreviewPanel.CapabilityConsumer = this;
            characterPreviewPanel.ReceiveOpenWindowNotification();
            */

            factionPanel.ReceiveOpenWindowNotification();

            // now that faction is set, and character panel is opened (which caused the first available unit to be selected), it's time to render the unit
            // inform the preview panel so the character can be rendered
            characterPreviewPanel.OnTargetReady += HandleTargetReady;
            characterPreviewPanel.CapabilityConsumer = this;
            characterPreviewPanel.ReceiveOpenWindowNotification();

            // attempt to open the UMA window first
            if (SystemGameManager.Instance.SystemConfigurationManager.NewGameUMAAppearance == true) {
                umaCharacterPanel.ReceiveOpenWindowNotification();
            }

            // if the UMA window is not in use, or there was no UMA unit available, try the mecanim window
            if (SystemGameManager.Instance.SystemConfigurationManager.NewGameUMAAppearance == false
                || (SystemGameManager.Instance.SystemConfigurationManager.NewGameUMAAppearance == true && SystemGameManager.Instance.CharacterCreatorManager.PreviewUnitController == null)) {
                characterPanel.ReceiveOpenWindowNotification();
            }

            // class goes before specialization because it acts as a filter for it
            classPanel.ReceiveOpenWindowNotification();
            specializationPanel.ReceiveOpenWindowNotification();

            // details should be last because it relies on all the information set in the previous methods
            detailsPanel.ReceiveOpenWindowNotification();

            OpenDetailsPanel();

            // testing appearance last since it relies on at very minimum the unit profile being set

            if (SystemGameManager.Instance.SystemConfigurationManager.NewGameAudioProfile != null) {
                SystemGameManager.Instance.AudioManager.StopMusic();
                SystemGameManager.Instance.AudioManager.PlayMusic(SystemGameManager.Instance.SystemConfigurationManager.NewGameAudioProfile.AudioClip);
            }
        }
         
        public void SetupSaveData() {
            //Debug.Log("NewGamePanel.SetupSaveData()");

            saveData = new AnyRPGSaveData();
            saveData = SystemGameManager.Instance.SaveManager.InitializeResourceLists(saveData, false);
            saveData.playerName = playerName;
            saveData.PlayerLevel = 1;
            saveData.CurrentScene = SystemGameManager.Instance.SystemConfigurationManager.DefaultStartingZone;
            unitProfile = SystemGameManager.Instance.SystemConfigurationManager.CharacterCreatorUnitProfile;
            saveData.unitProfileName = SystemGameManager.Instance.SystemConfigurationManager.CharacterCreatorUnitProfileName;
        }

        public void SetUnitProfile(NewGameUnitButton newGameUnitButton) {
            //Debug.Log("NewGamePanel.SetUnitProfile(" + newGameUnitButton.UnitProfile.DisplayName + ")");

            unitProfile = newGameUnitButton.UnitProfile;
            saveData.unitProfileName = unitProfile.DisplayName;
            characterPreviewPanel.ReloadUnit();
            characterPanel.SetBody(newGameUnitButton);
        }

        public void SetPlayerName(string newPlayerName) {
            playerName = newPlayerName;
            saveData.playerName = playerName;
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

        public void ShowCharacterClass(NewGameCharacterClassButton newGameCharacterClassButton) {
            //Debug.Log("NewGamePanel.ShowCharacterClass()");

            classPanel.ShowCharacterClass(newGameCharacterClassButton);
            if (characterClass != newGameCharacterClassButton.CharacterClass) {
                classSpecialization = null;
                characterClass = newGameCharacterClassButton.CharacterClass;
                detailsPanel.SetCharacterClass(characterClass);

                // since a new character class is chosen, the specialization list must be updated to match the class
                specializationPanel.ShowOptionButtonsCommon();

                // the specialization must also be updated on the details panel
                detailsPanel.SetClassSpecialization(classSpecialization);

                saveData.characterClass = characterClass.DisplayName;

                if (classSpecialization != null) {
                    saveData.classSpecialization = classSpecialization.DisplayName;
                    specializationButton.interactable = true;
                } else {
                    saveData.classSpecialization = string.Empty;
                    // only update equipment if specialization is null.  otherwise it has already been updated
                    UpdateEquipmentList();
                    specializationButton.interactable = false;
                }
            }
        }

        public void ShowClassSpecialization(NewGameClassSpecializationButton newGameClassSpecializationButton) {
            specializationPanel.ShowClassSpecialization(newGameClassSpecializationButton);
            if (newGameClassSpecializationButton == null) {
                classSpecialization = null;
            } else {
                classSpecialization = newGameClassSpecializationButton.ClassSpecialization;
            }

            UpdateEquipmentList();

            detailsPanel.SetClassSpecialization(classSpecialization);

            if (classSpecialization != null) {
                saveData.classSpecialization = classSpecialization.DisplayName;
            } else {
                saveData.classSpecialization = string.Empty;
            }

        }

        public void ShowFaction(NewGameFactionButton newGameFactionButton) {
            //Debug.Log("NewGamePanel.ShowFaction()");

            factionPanel.ShowFaction(newGameFactionButton);
            faction = newGameFactionButton.Faction;

            UpdateEquipmentList();

            detailsPanel.SetFaction(faction);

            saveData.playerFaction = faction.DisplayName;
            if (faction != null && faction.DefaultStartingZone != null && faction.DefaultStartingZone != string.Empty) {
                saveData.CurrentScene = faction.DefaultStartingZone;
            } else {
                saveData.CurrentScene = SystemGameManager.Instance.SystemConfigurationManager.DefaultStartingZone;
            }

            /*
            if (SystemGameManager.Instance.SystemConfigurationManager.NewGameUMAAppearance == false) {
                characterPanel.ShowOptionButtonsCommon();
            }
            */

        }

        public void UpdateEquipmentList() {
            //Debug.Log("NameGamePanel.UpdateEquipmentList()");

            equipmentList.Clear();

            if (unitProfile != null) {
                foreach (Equipment equipment in unitProfile.EquipmentList) {
                    if (equipmentList.ContainsKey(equipment.EquipmentSlotType)) {
                        equipmentList[equipment.EquipmentSlotType] = equipment;
                    } else {
                        equipmentList.Add(equipment.EquipmentSlotType, equipment);
                    }
                }
            }

            if (characterRace != null) {
                foreach (Equipment equipment in characterRace.EquipmentList) {
                    if (equipmentList.ContainsKey(equipment.EquipmentSlotType)) {
                        equipmentList[equipment.EquipmentSlotType] = equipment;
                    } else {
                        equipmentList.Add(equipment.EquipmentSlotType, equipment);
                    }
                }
            }

            if (SystemGameManager.Instance.SystemConfigurationManager.NewGameClass == true && characterClass != null) {
                foreach (Equipment equipment in characterClass.EquipmentList) {
                    if (equipmentList.ContainsKey(equipment.EquipmentSlotType)) {
                        equipmentList[equipment.EquipmentSlotType] = equipment;
                    } else {
                        equipmentList.Add(equipment.EquipmentSlotType, equipment);
                    }
                }
                if (SystemGameManager.Instance.SystemConfigurationManager.NewGameSpecialization == true && classSpecialization != null) {
                    foreach (Equipment equipment in classSpecialization.EquipmentList) {
                        if (equipmentList.ContainsKey(equipment.EquipmentSlotType)) {
                            equipmentList[equipment.EquipmentSlotType] = equipment;
                        } else {
                            equipmentList.Add(equipment.EquipmentSlotType, equipment);
                        }
                    }
                }
            }

            if (SystemGameManager.Instance.SystemConfigurationManager.NewGameFaction == true && faction != null) {
                foreach (Equipment equipment in faction.EquipmentList) {
                    if (equipmentList.ContainsKey(equipment.EquipmentSlotType)) {
                        equipmentList[equipment.EquipmentSlotType] = equipment;
                    } else {
                        equipmentList.Add(equipment.EquipmentSlotType, equipment);
                    }
                }
            }

            // save the equipment
            SaveEquipmentData();

            // show the equipment
            EquipCharacter();

        }

        public void SetCharacterProperties() {
            //Debug.Log("NewGameCharacterPanelController.SetCharacterProperties()");

            SystemGameManager.Instance.CharacterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.SetUnitProfile(UnitProfile, true, -1, false);
            SystemGameManager.Instance.CharacterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.SetUnitType(UnitType);
            SystemGameManager.Instance.CharacterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.SetCharacterRace(CharacterRace);
            SystemGameManager.Instance.CharacterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.SetCharacterClass(CharacterClass);
            SystemGameManager.Instance.CharacterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.SetClassSpecialization(ClassSpecialization);
            SystemGameManager.Instance.CharacterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.SetCharacterFaction(Faction);
        }

        public void HandleTargetReady() {
            //Debug.Log("NewGameCharacterPanelController.HandleTargetReady()");
            EquipCharacter();
        }

        public void EquipCharacter() {
            //Debug.Log("NewGameCharacterPanelController.EquipCharacter()");

            if (characterPreviewPanel.CharacterReady == false) {
                // attempting this before the character is spawned will make it go invisible (UMA bug)
                //Debug.Log("NewGameCharacterPanelController.EquipCharacter(): character not ready yet, exiting.");
                return;
            }

            // set character class etc first so preview works and can equip character
            SetCharacterProperties();

            CharacterEquipmentManager characterEquipmentManager = SystemGameManager.Instance.CharacterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager;
            if (characterEquipmentManager != null) {
                //Debug.Log("NewGameCharacterPanelController.EquipCharacter(): found equipment manager");

                // unequip equipment not in current list
                //characterEquipmentManager.UnequipAll(false);
                List<Equipment> removeList = new List<Equipment>();
                foreach (Equipment equipment in characterEquipmentManager.CurrentEquipment.Values) {
                    if (!EquipmentList.ContainsValue(equipment)) {
                        removeList.Add(equipment);
                    }
                }
                foreach (Equipment equipment in removeList) {
                    characterEquipmentManager.Unequip(equipment, false);
                }

                // equip equipment in list but not yet equipped
                if (EquipmentList != null) {
                    //Debug.Log("NewGameCharacterPanelController.EquipCharacter(): equipment list is not null");
                    foreach (Equipment equipment in EquipmentList.Values) {
                        //Debug.Log("NewGameCharacterPanelController.EquipCharacter(): ask to equip: " + equipment.DisplayName);
                        if (!characterEquipmentManager.CurrentEquipment.ContainsValue(equipment)) {
                            characterEquipmentManager.Equip(equipment, null, false, false);
                        }
                    }
                }
                characterPreviewPanel.RebuildUMA();
            }
        }

        public void SaveEquipmentData() {
            if (equipmentList == null) {
                // nothing to save
                return;
            }
            saveData.equipmentSaveData = new List<EquipmentSaveData>();
            foreach (Equipment equipment in equipmentList.Values) {
                EquipmentSaveData tmpSaveData = new EquipmentSaveData();
                tmpSaveData.MyName = (equipment == null ? string.Empty : equipment.ResourceName);
                tmpSaveData.DisplayName = (equipment == null ? string.Empty : equipment.DisplayName);
                if (equipment != null) {
                    if (equipment.ItemQuality != null) {
                        tmpSaveData.itemQuality = (equipment == null ? string.Empty : equipment.ItemQuality.ResourceName);
                    }
                    tmpSaveData.dropLevel = equipment.DropLevel;
                    tmpSaveData.randomSecondaryStatIndexes = (equipment == null ? null : equipment.RandomStatIndexes);
                }
                saveData.equipmentSaveData.Add(tmpSaveData);
            }
        }

       

        public void ClosePanel() {
            //Debug.Log("CharacterCreatorPanel.ClosePanel()");
            SystemGameManager.Instance.UIManager.SystemWindowManager.newGameWindow.CloseWindow();
            SystemGameManager.Instance.LevelManager.PlayLevelSounds();
        }

        public void NewGame() {
            //Debug.Log("LoadGamePanel.NewGame()");

            saveData.PlayerUMARecipe = characterPreviewPanel.GetCurrentRecipe();

            SystemGameManager.Instance.UIManager.SystemWindowManager.confirmNewGameMenuWindow.OpenWindow();
        }


    }

}