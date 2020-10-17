using AnyRPG;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UMA;
using UMA.Examples;
using UMA.CharacterSystem;
using UMA.CharacterSystem.Examples;

namespace AnyRPG {

    public class NewGamePanel : WindowContentController {

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

        public CharacterClass CharacterClass { get => characterClass; set => characterClass = value; }
        public ClassSpecialization ClassSpecialization { get => classSpecialization; set => classSpecialization = value; }
        public Faction Faction { get => faction; set => faction = value; }
        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }
        public AnyRPGSaveData SaveData { get => saveData; set => saveData = value; }
        public Dictionary<EquipmentSlotType, Equipment> EquipmentList { get => equipmentList; set => equipmentList = value; }

        #endregion

        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        private TextMeshProUGUI playerNameLabel = null;

        [SerializeField]
        private NewGameCharacterPreviewPanelController characterPreviewPanel = null;

        [SerializeField]
        private NewGameDetailsPanelController detailsPanel = null;

        [SerializeField]
        private NewGameMecanimCharacterPanelController characterPanel = null;

        [SerializeField]
        private NewGameCharacterPanelController umaCharacterPanel = null;

        [SerializeField]
        private NewGameClassPanelController classPanel = null;

        [SerializeField]
        private NewGameFactionPanelController factionPanel = null;

        [SerializeField]
        private NewGameSpecializationPanelController specializationPanel = null;

        private string playerName = "Player Name";
        private CharacterClass characterClass = null;
        private ClassSpecialization classSpecialization = null;
        private Faction faction = null;
        private UnitProfile unitProfile = null;

        private AnyRPGSaveData saveData;

        private Dictionary<EquipmentSlotType, Equipment> equipmentList = new Dictionary<EquipmentSlotType, Equipment>();

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("LoadGamePanel.OnCloseWindow()");
            base.RecieveClosedWindowNotification();

            SaveManager.MyInstance.ClearSharedData();
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


            if (SystemConfigurationManager.MyInstance.NewGameUMAAppearance == true) {
                umaCharacterPanel.ReceiveOpenWindowNotification();
                // first, inform the preview panel so the character can be rendered
                characterPreviewPanel.ReceiveOpenWindowNotification();
            }

            factionPanel.ReceiveOpenWindowNotification();

            if (SystemConfigurationManager.MyInstance.NewGameUMAAppearance == false) {
                // faction panel should be opened first in non uma mode to allow filtering of faction based models
                characterPanel.ReceiveOpenWindowNotification();

                // now that faction is set, and character panel is opened (which caused the first available unit to be selected), it's time to render the unit
                characterPreviewPanel.ReceiveOpenWindowNotification();
            }

            // class goes before specialization because it acts as a filter for it
            classPanel.ReceiveOpenWindowNotification();
            specializationPanel.ReceiveOpenWindowNotification();

            // details should be last because it relies on all the information set in the previous methods
            detailsPanel.ReceiveOpenWindowNotification();

            OpenDetailsPanel();

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
            //Debug.Log("NewGamePanel.SetUnitProfile()");

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

            EquipCharacter();

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

        public void EquipCharacter() {

            equipmentList = new Dictionary<EquipmentSlotType, Equipment>();

            if (SystemConfigurationManager.MyInstance.NewGameFaction == true && faction != null) {
                foreach (Equipment equipment in faction.EquipmentList) {
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

            // save the equipment
            SaveEquipmentData();

            // show the equipment
            characterPreviewPanel.EquipCharacter();

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

            EquipCharacter();

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

            EquipCharacter();

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