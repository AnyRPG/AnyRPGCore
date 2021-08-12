using AnyRPG;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class NewGameManager : ConfiguredMonoBehaviour, ICapabilityConsumer {

        public event System.Action<NewGameUnitButton> OnSetUnitProfile = delegate { };
        public event System.Action<string> OnSetPlayerName = delegate { };
        public event System.Action<NewGameCharacterClassButton> OnShowCharacterClass = delegate { };
        public event System.Action<NewGameCharacterClassButton> OnChangeCharacterClass = delegate { };
        public event System.Action<NewGameClassSpecializationButton> OnShowClassSpecialization = delegate { };
        public event System.Action<NewGameFactionButton> OnShowFaction = delegate { };
        public event System.Action OnUpdateEquipmentList = delegate { };

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

        // game manager references
        private SaveManager saveManager = null;
        private SystemConfigurationManager systemConfigurationManager = null;
        private SystemDataFactory systemDataFactory = null;
        private CharacterCreatorManager characterCreatorManager = null;
        private UIManager uIManager = null;
        private LevelManager levelManager = null;

        public CharacterClass CharacterClass { get => characterClass; set => characterClass = value; }
        public ClassSpecialization ClassSpecialization { get => classSpecialization; set => classSpecialization = value; }
        public Faction Faction { get => faction; set => faction = value; }
        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }
        public AnyRPGSaveData SaveData { get => saveData; set => saveData = value; }
        public Dictionary<EquipmentSlotType, Equipment> EquipmentList { get => equipmentList; set => equipmentList = value; }
        public UnitType UnitType { get => unitType; set => unitType = value; }
        public CharacterRace CharacterRace { get => characterRace; set => characterRace = value; }
        public CapabilityConsumerProcessor CapabilityConsumerProcessor { get => capabilityConsumerProcessor; }
        public string PlayerName { get => playerName; set => playerName = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            saveManager = systemGameManager.SaveManager;
            systemConfigurationManager = systemGameManager.SystemConfigurationManager;
            systemDataFactory = systemGameManager.SystemDataFactory;
            characterCreatorManager = systemGameManager.CharacterCreatorManager;
            uIManager = systemGameManager.UIManager;
            levelManager = systemGameManager.LevelManager;
        }

        public void ClearData() {
            playerName = "Player Name";
            unitProfile = null;
            unitType = null;
            characterRace = null;
            characterClass = null;
            classSpecialization = null;
            faction = null;
            capabilityConsumerProcessor = null;

        }

        public void SetupSaveData() {
            //Debug.Log("NewGamePanel.SetupSaveData()");

            saveData = new AnyRPGSaveData();
            saveData = saveManager.InitializeResourceLists(saveData, false);
            saveData.playerName = playerName;
            saveData.PlayerLevel = 1;
            saveData.CurrentScene = systemConfigurationManager.DefaultStartingZone;
            unitProfile = systemConfigurationManager.CharacterCreatorUnitProfile;
            saveData.unitProfileName = systemConfigurationManager.CharacterCreatorUnitProfileName;
        }

        public void SetUnitProfile(NewGameUnitButton newGameUnitButton) {
            //Debug.Log("NewGamePanel.SetUnitProfile(" + newGameUnitButton.UnitProfile.DisplayName + ")");

            unitProfile = newGameUnitButton.UnitProfile;
            saveData.unitProfileName = unitProfile.DisplayName;

            OnSetUnitProfile(newGameUnitButton);
        }

        public void SetPlayerName(string newPlayerName) {
            playerName = newPlayerName;
            saveData.playerName = playerName;

            OnSetPlayerName(newPlayerName);
        }

        public void ShowCharacterClass(NewGameCharacterClassButton newGameCharacterClassButton) {
            //Debug.Log("NewGamePanel.ShowCharacterClass()");
            OnShowCharacterClass(newGameCharacterClassButton);

            if (characterClass != newGameCharacterClassButton.CharacterClass) {
                classSpecialization = null;
                characterClass = newGameCharacterClassButton.CharacterClass;
                saveData.characterClass = characterClass.DisplayName;

                OnChangeCharacterClass(newGameCharacterClassButton);

                if (classSpecialization != null) {
                    saveData.classSpecialization = classSpecialization.DisplayName;
                } else {
                    saveData.classSpecialization = string.Empty;
                    // only update equipment if specialization is null.  otherwise it has already been updated
                    UpdateEquipmentList();
                }
            }
        }

        public void ShowClassSpecialization(NewGameClassSpecializationButton newGameClassSpecializationButton) {

            if (newGameClassSpecializationButton == null) {
                classSpecialization = null;
            } else {
                classSpecialization = newGameClassSpecializationButton.ClassSpecialization;
            }

            UpdateEquipmentList();

            // must call this after setting specialization so its available to the UI
            OnShowClassSpecialization(newGameClassSpecializationButton);

            if (classSpecialization != null) {
                saveData.classSpecialization = classSpecialization.DisplayName;
            } else {
                saveData.classSpecialization = string.Empty;
            }

        }

        public void ShowFaction(NewGameFactionButton newGameFactionButton) {
            //Debug.Log("NewGamePanel.ShowFaction()");


            faction = newGameFactionButton.Faction;

            UpdateEquipmentList();

            OnShowFaction(newGameFactionButton);

            saveData.playerFaction = faction.DisplayName;
            if (faction != null && faction.DefaultStartingZone != null && faction.DefaultStartingZone != string.Empty) {
                saveData.CurrentScene = faction.DefaultStartingZone;
            } else {
                saveData.CurrentScene = systemConfigurationManager.DefaultStartingZone;
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

            if (systemConfigurationManager.NewGameClass == true && characterClass != null) {
                foreach (Equipment equipment in characterClass.EquipmentList) {
                    if (equipmentList.ContainsKey(equipment.EquipmentSlotType)) {
                        equipmentList[equipment.EquipmentSlotType] = equipment;
                    } else {
                        equipmentList.Add(equipment.EquipmentSlotType, equipment);
                    }
                }
                if (systemConfigurationManager.NewGameSpecialization == true && classSpecialization != null) {
                    foreach (Equipment equipment in classSpecialization.EquipmentList) {
                        if (equipmentList.ContainsKey(equipment.EquipmentSlotType)) {
                            equipmentList[equipment.EquipmentSlotType] = equipment;
                        } else {
                            equipmentList.Add(equipment.EquipmentSlotType, equipment);
                        }
                    }
                }
            }

            if (systemConfigurationManager.NewGameFaction == true && faction != null) {
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
            OnUpdateEquipmentList();

        }

        public void SetCharacterProperties() {
            //Debug.Log("NewGameCharacterPanelController.SetCharacterProperties()");

            characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.SetUnitProfile(UnitProfile, true, -1, false);
            characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.SetUnitType(UnitType);
            characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.SetCharacterRace(CharacterRace);
            characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.SetCharacterClass(CharacterClass);
            characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.SetClassSpecialization(ClassSpecialization);
            characterCreatorManager.PreviewUnitController.CharacterUnit.BaseCharacter.SetCharacterFaction(Faction);
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

        public void SetPlayerUMARecipe(string newRecipe) {
            //Debug.Log("NewGameManager.SetPlayerUMARecipe()");

            saveData.PlayerUMARecipe = newRecipe;
        }


    }

}