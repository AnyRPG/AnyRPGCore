using AnyRPG;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class NewGameManager : ConfiguredMonoBehaviour, ICapabilityConsumer {

        public event System.Action<UnitProfile> OnSetUnitProfile = delegate { };
        public event System.Action<string> OnSetPlayerName = delegate { };
        public event System.Action<CharacterClass> OnSetCharacterClass = delegate { };
        public event System.Action<ClassSpecialization> OnSetClassSpecialization = delegate { };
        public event System.Action<Faction> OnSetFaction = delegate { };
        public event System.Action OnUpdateEquipmentList = delegate { };
        public event System.Action OnUpdateFactionList = delegate { };
        public event System.Action OnUpdateCharacterClassList = delegate { };
        public event System.Action OnUpdateClassSpecializationList = delegate { };
        public event System.Action OnUpdateUnitProfileList = delegate { };

        private string playerName = "Player Name";
        private UnitProfile unitProfile = null;
        private UnitType unitType = null;
        private CharacterRace characterRace = null;
        private CharacterClass characterClass = null;
        private ClassSpecialization classSpecialization = null;
        private Faction faction = null;

        private CapabilityConsumerProcessor capabilityConsumerProcessor = null;

        private AnyRPGSaveData saveData;

        //private Dictionary<EquipmentSlotType, Equipment> equipmentList = new Dictionary<EquipmentSlotType, Equipment>();
        private EquipmentManager equipmentManager = null;

        // valid choices for new game
        private List<Faction> factionList = new List<Faction>();
        private List<CharacterClass> characterClassList = new List<CharacterClass>();
        private List<ClassSpecialization> classSpecializationList = new List<ClassSpecialization>();
        private List<UnitProfile> unitProfileList = new List<UnitProfile>();

        // game manager references
        private SaveManager saveManager = null;
        private SystemDataFactory systemDataFactory = null;
        private CharacterCreatorManager characterCreatorManager = null;
        private UIManager uIManager = null;
        private LevelManager levelManager = null;

        public CharacterClass CharacterClass { get => characterClass; }
        public ClassSpecialization ClassSpecialization { get => classSpecialization; set => classSpecialization = value; }
        public Faction Faction { get => faction; }
        public UnitProfile UnitProfile { get => unitProfile; set => unitProfile = value; }
        public AnyRPGSaveData SaveData { get => saveData; set => saveData = value; }
        public Dictionary<EquipmentSlotProfile, Equipment> EquipmentList { get => equipmentManager.CurrentEquipment; }
        public UnitType UnitType { get => unitType; set => unitType = value; }
        public CharacterRace CharacterRace { get => characterRace; set => characterRace = value; }
        public CapabilityConsumerProcessor CapabilityConsumerProcessor { get => capabilityConsumerProcessor; }
        public string PlayerName { get => playerName; set => playerName = value; }
        public List<Faction> FactionList { get => factionList; }
        public List<CharacterClass> CharacterClassList { get => characterClassList; }
        public List<ClassSpecialization> ClassSpecializationList { get => classSpecializationList; }
        public List<UnitProfile> UnitProfileList { get => unitProfileList; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            saveManager = systemGameManager.SaveManager;
            systemDataFactory = systemGameManager.SystemDataFactory;
            characterCreatorManager = systemGameManager.CharacterCreatorManager;
            uIManager = systemGameManager.UIManager;
            levelManager = systemGameManager.LevelManager;

            equipmentManager = new EquipmentManager(systemGameManager);
        }

        public void ClearData() {
            //Debug.Log("NewGameManager.ClearData()");
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
            //Debug.Log("NewGameManager.SetupSaveData()");

            saveData = new AnyRPGSaveData();
            saveData = saveManager.InitializeResourceLists(saveData, false);
            saveData.playerName = playerName;
            saveData.PlayerLevel = 1;
            saveData.CurrentScene = systemConfigurationManager.DefaultStartingZone;
            unitProfile = systemConfigurationManager.CharacterCreatorUnitProfile;
            saveData.unitProfileName = systemConfigurationManager.CharacterCreatorUnitProfileName;

            UpdateFactionList();
            UpdateCharacterClassList();
            
            // testing - not needed because updating character class list will result in class getting set, which will update the class specialization list
            //UpdateClassSpecializationList();

            if (systemConfigurationManager.NewGameFaction == false || factionList.Count == 0) {
                UpdateUnitProfileList();
            }
        }

        protected void UpdateFactionList() {

            factionList.Clear();

            foreach (Faction faction in systemDataFactory.GetResourceList<Faction>()) {
                if (faction.NewGameOption == true) {
                    factionList.Add(faction);
                }
            }

            OnUpdateFactionList();

            if (factionList.Count > 0
                && (factionList.Contains(faction) == false || faction == null)) {
                SetFaction(factionList[0]);
            }
        }

        protected void UpdateCharacterClassList() {

            characterClassList.Clear();

            foreach (CharacterClass characterClass in systemDataFactory.GetResourceList<CharacterClass>()) {
                if (characterClass.NewGameOption == true) {
                    characterClassList.Add(characterClass);
                }
            }

            OnUpdateCharacterClassList();

            if (characterClassList.Count > 0
                && (characterClassList.Contains(characterClass) == false || characterClass == null)) {
                SetCharacterClass(characterClassList[0]);
            }
        }

        protected void UpdateClassSpecializationList() {
            //Debug.Log("NewGameManager.UpdateClassSpecializationList()");

            classSpecializationList.Clear();

            foreach (ClassSpecialization classSpecialization in systemDataFactory.GetResourceList<ClassSpecialization>()) {
                //Debug.Log("LoadGamePanel.ShowLoadButtonsCommon(): setting a button with saved game data");
                if (characterClass != null
                    && classSpecialization.CharacterClasses != null
                    && classSpecialization.CharacterClasses.Contains(characterClass)
                    && classSpecialization.NewGameOption == true) {
                    classSpecializationList.Add(classSpecialization);
                }
            }

            OnUpdateClassSpecializationList();

            if (classSpecializationList.Count > 0) { 
                if (classSpecializationList.Contains(classSpecialization) == false || classSpecialization == null) {
                    SetClassSpecialization(classSpecializationList[0]);
                }
            } else {
                SetClassSpecialization(null);
            }
        }

        protected void UpdateUnitProfileList() {
            //Debug.Log("NewGameManager.UpdateUnitProfileList()");
            unitProfileList.Clear();

            if ((faction != null && faction.HideDefaultProfiles == false)
                            || systemConfigurationManager.AlwaysShowDefaultProfiles == true
                            || faction == null) {
                //Debug.Log("NewGameMecanimCharacterPanelController.ShowOptionButtonsCommon(): showing default profiles");
                AddDefaultProfiles();
            }

            if (faction != null) {
                foreach (UnitProfile unitProfile in faction.CharacterCreatorProfiles) {
                    unitProfileList.Add(unitProfile);
                }
            }

            OnUpdateUnitProfileList();

            if (unitProfileList.Count > 0
                //&& (unitProfileList.Contains(unitProfile) == false || unitProfile == null)) {
                ) {
                if (unitProfileList.Contains(unitProfile)) {
                    SetUnitProfile(unitProfileList[unitProfileList.IndexOf(unitProfile)]);
                } else {
                    SetUnitProfile(unitProfileList[0]);
                }
            }
        }

        private void AddDefaultProfiles() {
            if (systemConfigurationManager.DefaultPlayerUnitProfile != null) {
                unitProfileList.Add(systemConfigurationManager.DefaultPlayerUnitProfile);
            }
            foreach (UnitProfile unitProfile in systemConfigurationManager.CharacterCreatorProfiles) {
                unitProfileList.Add(unitProfile);
            }
        }

        public void SetPlayerName(string newPlayerName) {
            playerName = newPlayerName;
            saveData.playerName = playerName;

            OnSetPlayerName(newPlayerName);
        }

        public void SetUnitProfile(UnitProfile newUnitProfile) {
            //Debug.Log("NewGameManager.SetUnitProfile(" + newUnitProfile.DisplayName + ")");

            unitProfile = newUnitProfile;
            saveData.unitProfileName = unitProfile.DisplayName;

            OnSetUnitProfile(newUnitProfile);
        }

        public void SetCharacterClass(CharacterClass newCharacterClass) {
            //Debug.Log("NewGamePanel.ShowCharacterClass()");

            if (characterClass != newCharacterClass) {
                characterClass = newCharacterClass;
                saveData.characterClass = characterClass.DisplayName;
                OnSetCharacterClass(newCharacterClass);
                //OnSetClassSpecialization(null);

                UpdateClassSpecializationList();

                
                // not all classes have specializations
                // update equipment list manually in that case
                // FIX - THIS WAS COMMENTED OUT FOR SOME REASON - MONITOR FOR BREAKAGE
                // it needed to be re-enabled because character doens't get equipment if they have no spec
                // re-commented out because class specialization is always set now, even if its set to null because there are no specs
                /*
                if (classSpecializationList.Count == 0) {
                    UpdateEquipmentList();
                }
                */
                
            }
        }

        public void SetClassSpecialization(ClassSpecialization newClassSpecialization) {
            //Debug.Log("NewGamePanel.SetClassSpecialization(" + (newClassSpecialization == null ? "null" : newClassSpecialization.DisplayName) + ")");

            if (classSpecialization !=  newClassSpecialization || newClassSpecialization == null) {
                classSpecialization = newClassSpecialization;
                if (classSpecialization != null) {
                    saveData.classSpecialization = classSpecialization.DisplayName;
                } else {
                    saveData.classSpecialization = string.Empty;
                }

                UpdateEquipmentList();

                // must call this after setting specialization so its available to the UI
                OnSetClassSpecialization(newClassSpecialization);

            }


        }

        public void SetFaction(Faction newFaction) {
            //Debug.Log("NewGameManager.ShowFaction()");

            faction = newFaction;

            UpdateEquipmentList();

            saveData.playerFaction = faction.DisplayName;

            if (faction != null && faction.DefaultStartingZone != null && faction.DefaultStartingZone != string.Empty) {
                saveData.CurrentScene = faction.DefaultStartingZone;
            } else {
                saveData.CurrentScene = systemConfigurationManager.DefaultStartingZone;
            }
            if (faction != null) {
                levelManager.OverrideSpawnLocationTag = faction.DefaultStartingLocationTag;
            }

            OnSetFaction(newFaction);

            UpdateUnitProfileList();
        }

        public void UpdateEquipmentList() {
            //Debug.Log("NameGameManager.UpdateEquipmentList()");

            equipmentManager.ClearEquipmentList();

            // testing - the new game manager should ignore special UnitProfile equipment that is only meant for NPCs
            // commented out the following code : 
            /*
            if (unitProfile != null) {
                foreach (Equipment equipment in unitProfile.EquipmentList) {
                    if (equipmentList.ContainsKey(equipment.EquipmentSlotType)) {
                        equipmentList[equipment.EquipmentSlotType] = equipment;
                    } else {
                        equipmentList.Add(equipment.EquipmentSlotType, equipment);
                    }
                }
            }
            */

            if (characterRace != null) {
                foreach (Equipment equipment in characterRace.EquipmentList) {
                    /*
                    if (equipmentList.ContainsKey(equipment.EquipmentSlotType)) {
                        equipmentList[equipment.EquipmentSlotType] = equipment;
                    } else {
                        equipmentList.Add(equipment.EquipmentSlotType, equipment);
                    }
                    */
                    equipmentManager.EquipEquipment(equipment);
                }
            }

            if (systemConfigurationManager.NewGameClass == true && characterClass != null) {
                foreach (Equipment equipment in characterClass.EquipmentList) {
                    /*
                    if (equipmentList.ContainsKey(equipment.EquipmentSlotType)) {
                        equipmentList[equipment.EquipmentSlotType] = equipment;
                    } else {
                        equipmentList.Add(equipment.EquipmentSlotType, equipment);
                    }
                    */
                    equipmentManager.EquipEquipment(equipment);
                }
                if (systemConfigurationManager.NewGameSpecialization == true && classSpecialization != null) {
                    foreach (Equipment equipment in classSpecialization.EquipmentList) {
                        /*
                        if (equipmentList.ContainsKey(equipment.EquipmentSlotType)) {
                            equipmentList[equipment.EquipmentSlotType] = equipment;
                        } else {
                            equipmentList.Add(equipment.EquipmentSlotType, equipment);
                        }
                        */
                        equipmentManager.EquipEquipment(equipment);
                    }
                }
            }

            if (systemConfigurationManager.NewGameFaction == true && faction != null) {
                foreach (Equipment equipment in faction.EquipmentList) {
                    /*
                    if (equipmentList.ContainsKey(equipment.EquipmentSlotType)) {
                        equipmentList[equipment.EquipmentSlotType] = equipment;
                    } else {
                        equipmentList.Add(equipment.EquipmentSlotType, equipment);
                    }
                    */
                    equipmentManager.EquipEquipment(equipment);
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
            if (equipmentManager.CurrentEquipment == null) {
                // nothing to save
                return;
            }
            saveData.equipmentSaveData = new List<EquipmentSaveData>();
            foreach (Equipment equipment in equipmentManager.CurrentEquipment.Values) {
                EquipmentSaveData tmpSaveData = new EquipmentSaveData();
                tmpSaveData.EquipmentName = (equipment == null ? string.Empty : equipment.ResourceName);
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