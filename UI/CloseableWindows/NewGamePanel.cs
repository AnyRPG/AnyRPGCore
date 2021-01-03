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

        public static NewGamePanel MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<NewGamePanel>();
                }

                return instance;
            }
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

            SaveManager.MyInstance.ClearSharedData();
            characterPreviewPanel.OnTargetReady -= HandleTargetReady;
            characterPreviewPanel.RecieveClosedWindowNotification();
            if (SystemConfigurationManager.MyInstance.NewGameUMAAppearance == true) {
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

            SaveManager.MyInstance.ClearSharedData();

            SetupSaveData();

            /*
            if (SystemConfigurationManager.MyInstance.NewGameUMAAppearance == true) {
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

            if (SystemConfigurationManager.MyInstance.NewGameUMAAppearance == true) {
                umaCharacterPanel.ReceiveOpenWindowNotification();
            }
            if (SystemConfigurationManager.MyInstance.NewGameUMAAppearance == false) {
                characterPanel.ReceiveOpenWindowNotification();
            }

            // class goes before specialization because it acts as a filter for it
            classPanel.ReceiveOpenWindowNotification();
            specializationPanel.ReceiveOpenWindowNotification();

            // details should be last because it relies on all the information set in the previous methods
            detailsPanel.ReceiveOpenWindowNotification();

            OpenDetailsPanel();

            // testing appearance last since it relies on at very minimum the unit profile being set
            



            if (SystemConfigurationManager.MyInstance.NewGameAudioProfile != null) {
                AudioManager.MyInstance.StopMusic();
                AudioManager.MyInstance.PlayMusic(SystemConfigurationManager.MyInstance.NewGameAudioProfile.AudioClip);
            }
        }
         
        public void SetupSaveData() {
            //Debug.Log("NewGamePanel.SetupSaveData()");

            saveData = new AnyRPGSaveData();
            saveData = SaveManager.MyInstance.InitializeResourceLists(saveData, false);
            saveData.playerName = playerName;
            saveData.PlayerLevel = 1;
            saveData.CurrentScene = SystemConfigurationManager.MyInstance.DefaultStartingZone;
            unitProfile = SystemConfigurationManager.MyInstance.CharacterCreatorUnitProfile;
            saveData.unitProfileName = SystemConfigurationManager.MyInstance.CharacterCreatorUnitProfileName;
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
            if (SystemConfigurationManager.MyInstance.NewGameUMAAppearance == true) {
                umaCharacterPanel.ShowPanel();
            } else {
                characterPanel.ShowPanel();
            }
        }

        public void OpenFactionPanel() {
            //Debug.Log("NewGamePanel.OpenFactionPanel()");

            ClosePanels();
            factionPanel.ShowPanel();
        }

        public void OpenSpecializationPanel() {
            ClosePanels();
            specializationPanel.ShowPanel();
        }

        public void ShowCharacterClass(NewGameCharacterClassButton newGameCharacterClassButton) {
            classPanel.ShowCharacterClass(newGameCharacterClassButton);
            characterClass = newGameCharacterClassButton.CharacterClass;

            // since a new character class is chosen, the specialization list must be updated to match the class
            specializationPanel.ShowOptionButtonsCommon();

            UpdateEquipmentList();

            detailsPanel.SetCharacterClass(characterClass);

            // the specialization must also be updated on the details panel
            detailsPanel.SetClassSpecialization(classSpecialization);

            saveData.characterClass = characterClass.DisplayName;

            if (classSpecialization != null) {
                saveData.classSpecialization = classSpecialization.DisplayName;
            } else {
                saveData.classSpecialization = string.Empty;
            }
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

            if (SystemConfigurationManager.MyInstance.NewGameClass == true && characterClass != null) {
                foreach (Equipment equipment in characterClass.EquipmentList) {
                    if (equipmentList.ContainsKey(equipment.EquipmentSlotType)) {
                        equipmentList[equipment.EquipmentSlotType] = equipment;
                    } else {
                        equipmentList.Add(equipment.EquipmentSlotType, equipment);
                    }
                }
                if (SystemConfigurationManager.MyInstance.NewGameSpecialization == true && classSpecialization != null) {
                    foreach (Equipment equipment in classSpecialization.EquipmentList) {
                        if (equipmentList.ContainsKey(equipment.EquipmentSlotType)) {
                            equipmentList[equipment.EquipmentSlotType] = equipment;
                        } else {
                            equipmentList.Add(equipment.EquipmentSlotType, equipment);
                        }
                    }
                }
            }

            if (SystemConfigurationManager.MyInstance.NewGameFaction == true && faction != null) {
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

            CharacterCreatorManager.MyInstance.PreviewUnitController.CharacterUnit.BaseCharacter.SetUnitProfile(UnitProfile);
            CharacterCreatorManager.MyInstance.PreviewUnitController.CharacterUnit.BaseCharacter.SetUnitType(UnitType);
            CharacterCreatorManager.MyInstance.PreviewUnitController.CharacterUnit.BaseCharacter.SetCharacterRace(CharacterRace);
            CharacterCreatorManager.MyInstance.PreviewUnitController.CharacterUnit.BaseCharacter.SetCharacterClass(CharacterClass);
            CharacterCreatorManager.MyInstance.PreviewUnitController.CharacterUnit.BaseCharacter.SetClassSpecialization(ClassSpecialization);
            CharacterCreatorManager.MyInstance.PreviewUnitController.CharacterUnit.BaseCharacter.SetCharacterFaction(Faction);
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

            CharacterEquipmentManager characterEquipmentManager = CharacterCreatorManager.MyInstance.PreviewUnitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager;
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
                saveData.CurrentScene = SystemConfigurationManager.MyInstance.DefaultStartingZone;
            }

            if (SystemConfigurationManager.MyInstance.NewGameUMAAppearance == false) {
                characterPanel.ShowOptionButtonsCommon();
            }

        }

        public void ClosePanel() {
            //Debug.Log("CharacterCreatorPanel.ClosePanel()");
            SystemWindowManager.MyInstance.newGameWindow.CloseWindow();
            LevelManager.MyInstance.PlayLevelSounds();
        }

        public void NewGame() {
            //Debug.Log("LoadGamePanel.NewGame()");

            saveData.PlayerUMARecipe = characterPreviewPanel.GetCurrentRecipe();

            SystemWindowManager.MyInstance.confirmNewGameMenuWindow.OpenWindow();
        }


    }

}