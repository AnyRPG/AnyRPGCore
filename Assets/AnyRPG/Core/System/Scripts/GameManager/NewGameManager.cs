using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    public class NewGameManager : ConfiguredClass, ICharacterEditor, ICharacterConfigurationProvider {

        public event System.Action<UnitProfile> OnSetUnitProfile = delegate { };
        public event System.Action<string> OnSetPlayerName = delegate { };
        public event System.Action<string> OnResetPlayerName = delegate { };
        public event System.Action<Faction> OnSetFaction = delegate { };
        public event System.Action<CharacterRace> OnSetCharacterRace = delegate { };
        public event System.Action<CharacterClass> OnSetCharacterClass = delegate { };
        public event System.Action<ClassSpecialization> OnSetClassSpecialization = delegate { };
        public event System.Action OnUpdateEquipmentList = delegate { };
        public event System.Action OnUpdateFactionList = delegate { };
        public event System.Action OnUpdateCharacterRaceList = delegate { };
        public event System.Action OnUpdateCharacterClassList = delegate { };
        public event System.Action OnUpdateClassSpecializationList = delegate { };
        public event System.Action OnUpdateUnitProfileList = delegate { };

        private string defaultPlayerName = "Player Name";
        private string playerName = "Player Name";
        private UnitProfile unitProfile = null;
        private UnitType unitType = null;
        private CharacterRace characterRace = null;
        private CharacterClass characterClass = null;
        private ClassSpecialization classSpecialization = null;
        private Faction faction = null;

        private Material defaultPlatformMaterial = null;
        private Material defaultTopMaterial = null;
        private Material defaultBottomMaterial = null;
        private Material defaultNorthMaterial = null;
        private Material defaultSouthMaterial = null;
        private Material defaultEastMaterial = null;
        private Material defaultWestMaterial = null;

        private GameObject environmentPreviewPrefab = null;
        private Material platformMaterial = null;

        private Material topMaterial = null;
        private Material bottomMaterial = null;
        private Material northMaterial = null;
        private Material southMaterial = null;
        private Material eastMaterial = null;
        private Material westMaterial = null;

        private CapabilityConsumerProcessor capabilityConsumerProcessor = null;

        private CharacterSaveData defaultCharacterSaveData = null;

        //private Dictionary<EquipmentSlotType, Equipment> equipmentList = new Dictionary<EquipmentSlotType, Equipment>();
        private EquipmentManager defaultEquipmentManager = null;

        // valid choices for new game
        private List<Faction> factionList = new List<Faction>();
        private List<CharacterRace> characterRaceList = new List<CharacterRace>();
        private List<CharacterClass> characterClassList = new List<CharacterClass>();
        private List<ClassSpecialization> classSpecializationList = new List<ClassSpecialization>();
        private List<UnitProfile> unitProfileList = new List<UnitProfile>();

        // game manager references
        private SaveManager saveManager = null;
        private CharacterCreatorManager characterCreatorManager = null;

        public Faction Faction { get => faction; }
        public CharacterRace CharacterRace { get => characterRace; set => characterRace = value; }
        public CharacterClass CharacterClass { get => characterClass; }
        public ClassSpecialization ClassSpecialization { get => classSpecialization; set => classSpecialization = value; }
        public UnitProfile UnitProfile { get => unitProfile; }
        public CharacterSaveData DefaultCharacterSaveData { get => defaultCharacterSaveData; set => defaultCharacterSaveData = value; }
        public Dictionary<EquipmentSlotProfile, EquipmentInventorySlot> EquipmentList { get => defaultEquipmentManager.CurrentEquipment; }
        public UnitType UnitType { get => unitType; set => unitType = value; }
        public CapabilityConsumerProcessor CapabilityConsumerProcessor { get => capabilityConsumerProcessor; }
        public string PlayerName { get => playerName; set => playerName = value; }
        public List<Faction> FactionList { get => factionList; }
        public List<CharacterRace> CharacterRaceList { get => characterRaceList; }
        public List<CharacterClass> CharacterClassList { get => characterClassList; }
        public List<ClassSpecialization> ClassSpecializationList { get => classSpecializationList; }
        public List<UnitProfile> UnitProfileList { get => unitProfileList; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            defaultEquipmentManager = new EquipmentManager(systemGameManager);
            if (systemConfigurationManager.DefaultPlayerName != string.Empty &&
                systemConfigurationManager.PlayerNameSource == PlayerNameSource.DefaultPlayerName) {
                defaultPlayerName = systemConfigurationManager.DefaultPlayerName;
            }

            GetDefaultMaterials();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            saveManager = systemGameManager.SaveManager;
            characterCreatorManager = systemGameManager.CharacterCreatorManager;
        }

        public void NewLocalGame() {
            //Debug.Log("NewGameManager.NewLocalGame()");

            if (defaultCharacterSaveData == null) {
                // this could have come from a main menu button press
                InitializeData();
            }
            saveManager.NewGame(defaultCharacterSaveData);
            
            // reset to null so next button press doesn't start game with same data again
            defaultCharacterSaveData = null;
        }

        public void CreateNetworkCharacter() {
            //Debug.Log("Savemanager.CreateNetworkGame(AnyRPGSaveData)");

            networkManagerClient.RequestCreatePlayerCharacter(defaultCharacterSaveData);
        }


        private void GetDefaultMaterials() {
            defaultPlatformMaterial = characterCreatorManager.PlatformRenderer.material;
            defaultTopMaterial = characterCreatorManager.TopRenderer.material;
            defaultBottomMaterial = characterCreatorManager.BottomRenderer.material;
            defaultNorthMaterial = characterCreatorManager.NorthRenderer.material;
            defaultSouthMaterial = characterCreatorManager.SouthRenderer.material;
            defaultEastMaterial = characterCreatorManager.EastRenderer.material;
            defaultWestMaterial = characterCreatorManager.WestRenderer.material;
        }

        private void ResetPlayerName(string newPlayerName) {
            //Debug.Log($"NewGameManager.ResetPlayerName({newPlayerName})");

            SetPlayerName(newPlayerName);

            OnResetPlayerName(newPlayerName);
        }

        private void SetPlayerName(string newPlayerName) {
            //Debug.Log($"NewGameManager.SetPlayerName({newPlayerName})");

            if (newPlayerName == "") {
                return;
            }

            playerName = newPlayerName;
            defaultCharacterSaveData.CharacterName = playerName;
        }

        public void EditPlayerName(string newPlayerName) {
            //Debug.Log($"NewGameManager.EditPlayerName({newPlayerName})");

            SetPlayerName(newPlayerName);

            OnSetPlayerName(newPlayerName);
        }

        public void InitializeData() {
            //Debug.Log("NewGameManager.InitializeData()");

            playerName = string.Empty;
            unitProfile = null;
            unitType = null;
            characterRace = null;
            characterClass = null;
            classSpecialization = null;
            faction = null;
            capabilityConsumerProcessor = null;
            systemItemManager.ClientReset();
            defaultCharacterSaveData = saveManager.CreateSaveData();

        }

        public void SetupSaveData() {
            //Debug.Log("NewGameManager.SetupSaveData()");

            if (systemConfigurationManager.PlayerNameSource == PlayerNameSource.DefaultPlayerName) {
                ResetPlayerName(defaultPlayerName);
            }

            // testing - this is done later by updateUnitProfileList() anyway
            //unitProfile = systemConfigurationManager.DefaultUnitProfile;


            if (systemConfigurationManager.NewGameFaction == true) {
                UpdateFactionList();
            }

            if (systemConfigurationManager.NewGameRace == true) {
                UpdateCharacterRaceList();
            }

            UpdateUnitProfileList();

            UpdateCharacterEnvironment();


            UpdateCharacterClassList();
            
            // testing - not needed because updating character class list will result in class getting set, which will update the class specialization list
            //UpdateClassSpecializationList();

            /*
             // is this necessary ? It's already done above on line 147
            if (systemConfigurationManager.NewGameFaction == false || factionList.Count == 0) {
                UpdateUnitProfileList();
            }
            */
        }

        protected void UpdateFactionList() {
            //Debug.Log("NewGameManager.UpdateFactionList()");

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

        protected void UpdateCharacterRaceList() {
            //Debug.Log("NewGameManager.UpdateCharacterRaceList()");

            characterRaceList.Clear();

            if (faction != null) {
                characterRaceList.AddRange(faction.Races);
            }

            // add default races that are accessible regardless of faction
            foreach (CharacterRace characterRace in systemDataFactory.GetResourceList<CharacterRace>()) {
                if (characterRace.NewGameOption == true
                    && characterRaceList.Contains(characterRace) == false) {
                    characterRaceList.Add(characterRace);
                }
            }

            OnUpdateCharacterRaceList();

            if (characterRaceList.Count > 0) {
                if (characterRaceList.Contains(characterRace) == false || characterRace == null) {
                    SetCharacterRace(characterRaceList[0]);
                }
            } else {
                SetCharacterRace(null);
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

            PopulateUnitProfileList();

            OnUpdateUnitProfileList();

            if (unitProfileList.Count > 0) {
                if (unitProfileList.Contains(unitProfile)) {
                    SetUnitProfile(unitProfileList[unitProfileList.IndexOf(unitProfile)]);
                } else {
                    SetUnitProfile(unitProfileList[0]);
                }
            }
        }

        private void PopulateUnitProfileList() {
            
            if (systemConfigurationManager.CharacterSelectionType == CharacterSelectionType.DefaultCharacter) {
                unitProfileList.Add(systemConfigurationManager.DefaultPlayerUnitProfile);
                return;
            }

            if (systemConfigurationManager.CharacterSelectionType == CharacterSelectionType.CharacterList) {
                
                // add default profiles
                if (faction == null || faction.AddSystemProfiles == true) {
                    AddDefaultProfiles();
                }

                // add faction specific profiles
                if (faction != null) {
                    foreach (UnitProfile unitProfile in faction.CharacterCreatorProfiles) {
                        if (unitProfileList.Contains(unitProfile) == false) {
                            unitProfileList.Add(unitProfile);
                        }
                    }
                }
                return;
            }

            if (systemConfigurationManager.CharacterSelectionType == CharacterSelectionType.RaceAndGender) {
                if (characterRace != null) {
                    if (characterRace.MaleUnitProfile != null) {
                        unitProfileList.Add(characterRace.MaleUnitProfile);
                        return;
                    }
                    if (characterRace.FemaleUnitProfile != null) {
                        unitProfileList.Add(characterRace.FemaleUnitProfile);
                        return;
                    }
                }
            }

        }

        private void AddDefaultProfiles() {
            if (systemConfigurationManager.DefaultPlayerUnitProfile != null) {
                unitProfileList.Add(systemConfigurationManager.DefaultPlayerUnitProfile);
            }
            foreach (UnitProfile unitProfile in systemConfigurationManager.DefaultUnitProfileList) {
                if (unitProfileList.Contains(unitProfile) == false) {
                    unitProfileList.Add(unitProfile);
                }
            }
        }


        public void SetUnitProfile(UnitProfile newUnitProfile) {
            //Debug.Log($"NewGameManager.SetUnitProfile({newUnitProfile.ResourceName})");

            if (unitProfile == newUnitProfile) {
                return;
            }

            unitProfile = newUnitProfile;
            defaultCharacterSaveData.UnitProfileName = unitProfile.ResourceName;

            OnSetUnitProfile(newUnitProfile);

            if (systemConfigurationManager.PlayerNameSource == PlayerNameSource.UnitProfile) {
                if (newUnitProfile.CharacterName != string.Empty) {
                    ResetPlayerName(newUnitProfile.CharacterName);
                } else {
                    ResetPlayerName(newUnitProfile.DisplayName);
                }
            }

            //UpdateCharacterEnvironment();
        }

        private void UpdateCharacterEnvironment() {
            //Debug.Log("NewGameManager.UpdateCharacterEnvironment()");

            environmentPreviewPrefab = GetEnvironmentPreviewPrefab();
            characterCreatorManager.SpawnEnvironmentPreviewPrefab(environmentPreviewPrefab);

            platformMaterial = GetPlatformMaterial();
            characterCreatorManager.SetPlatformMaterial(platformMaterial);

            topMaterial = GetTopMaterial();
            bottomMaterial = GetBottomMaterial();
            northMaterial = GetNorthMaterial();
            southMaterial = GetSouthMaterial();
            eastMaterial = GetEastMaterial();
            westMaterial = GetWestMaterial();
            characterCreatorManager.SetSkybox(topMaterial,
                bottomMaterial,
                northMaterial,
                southMaterial,
                eastMaterial,
                westMaterial);
        }

        private GameObject GetEnvironmentPreviewPrefab() {
            //Debug.Log("NewGameManager.GetEnvironmentPreviewPrefab()");

            if (characterRace?.EnvironmentPreview.EnvironmentPreviewPrefab != null) {
                return characterRace.EnvironmentPreview.EnvironmentPreviewPrefab;
            }
            if (faction?.EnvironmentPreview.EnvironmentPreviewPrefab != null) {
                return faction.EnvironmentPreview.EnvironmentPreviewPrefab;
            }
            return null;
        }

        private Material GetPlatformMaterial() {
            //Debug.Log("NewGameManager.GetPlatformMaterial()");

            if (characterRace?.EnvironmentPreview.PlatformMaterial != null) {
                return characterRace.EnvironmentPreview.PlatformMaterial;
            }
            if (faction?.EnvironmentPreview.PlatformMaterial != null) {
                return faction.EnvironmentPreview.PlatformMaterial;
            }
            return defaultPlatformMaterial;
        }


        private Material GetTopMaterial() {
            if (characterRace?.EnvironmentPreview.TopMaterial != null) {
                return characterRace.EnvironmentPreview.TopMaterial;
            }
            if (faction?.EnvironmentPreview.TopMaterial != null) {
                return faction.EnvironmentPreview.TopMaterial;
            }
            return defaultTopMaterial;
        }

        private Material GetBottomMaterial() {
            if (characterRace?.EnvironmentPreview.BottomMaterial != null) {
                return characterRace.EnvironmentPreview.BottomMaterial;
            }
            if (faction?.EnvironmentPreview.BottomMaterial != null) {
                return faction.EnvironmentPreview.BottomMaterial;
            }
            return defaultBottomMaterial;
        }

        private Material GetNorthMaterial() {
            if (characterRace?.EnvironmentPreview.NorthMaterial != null) {
                return characterRace.EnvironmentPreview.NorthMaterial;
            }
            if (faction?.EnvironmentPreview.NorthMaterial != null) {
                return faction.EnvironmentPreview.NorthMaterial;
            }
            return defaultNorthMaterial;
        }

        private Material GetSouthMaterial() {
            if (characterRace?.EnvironmentPreview.SouthMaterial != null) {
                return characterRace.EnvironmentPreview.SouthMaterial;
            }
            if (faction?.EnvironmentPreview.SouthMaterial != null) {
                return faction.EnvironmentPreview.SouthMaterial;
            }
            return defaultSouthMaterial;
        }

        private Material GetEastMaterial() {
            if (characterRace?.EnvironmentPreview.EastMaterial != null) {
                return characterRace.EnvironmentPreview.EastMaterial;
            }
            if (faction?.EnvironmentPreview.EastMaterial != null) {
                return faction.EnvironmentPreview.EastMaterial;
            }
            return defaultEastMaterial;
        }

        private Material GetWestMaterial() {
            if (characterRace?.EnvironmentPreview.WestMaterial != null) {
                return characterRace.EnvironmentPreview.WestMaterial;
            }
            if (faction?.EnvironmentPreview.WestMaterial != null) {
                return faction.EnvironmentPreview.WestMaterial;
            }
            return defaultWestMaterial;
        }

        public void SetCharacterClass(CharacterClass newCharacterClass) {
            //Debug.Log("NewGamePanel.SetCharacterClass(" + (newCharacterClass == null ? "null" : newCharacterClass.DisplayName) + ")");

            if (characterClass == newCharacterClass) {
                return;
            }

            characterClass = newCharacterClass;
            defaultCharacterSaveData.CharacterClass = characterClass.ResourceName;
            OnSetCharacterClass(newCharacterClass);

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

        public void SetClassSpecialization(ClassSpecialization newClassSpecialization) {
            //Debug.Log("NewGamePanel.SetClassSpecialization(" + (newClassSpecialization == null ? "null" : newClassSpecialization.DisplayName) + ")");

            if (classSpecialization !=  newClassSpecialization || newClassSpecialization == null) {
                classSpecialization = newClassSpecialization;
                if (classSpecialization != null) {
                    defaultCharacterSaveData.ClassSpecialization = classSpecialization.ResourceName;
                } else {
                    defaultCharacterSaveData.ClassSpecialization = string.Empty;
                }

                UpdateEquipmentList();

                // must call this after setting specialization so its available to the UI
                OnSetClassSpecialization(newClassSpecialization);
            }

        }

        public void ChooseNewFaction(Faction newFaction) {
            //Debug.Log("NewGameManager.SetFaction(" + (newFaction == null ? "null" : newFaction.DisplayName) + ")");

            if (faction == newFaction) {
                return;
            }

            SetFaction(newFaction);
            if (systemConfigurationManager.NewGameRace == true) {
                UpdateCharacterRaceList();
            }

            UpdateUnitProfileList();

            UpdateCharacterEnvironment();
        }

        public void SetFaction(Faction newFaction) {
            //Debug.Log("NewGameManager.SetFaction(" + (newFaction == null ? "null" : newFaction.DisplayName) + ")");

            if (faction == newFaction) {
                return;
            }

            faction = newFaction;

            UpdateEquipmentList();

            defaultCharacterSaveData.CharacterFaction = faction.ResourceName;

            PerformFactionOverrides(defaultCharacterSaveData, faction);

            OnSetFaction(newFaction);
        }

        public void PerformFactionOverrides(CharacterSaveData characterSaveData, Faction usedFaction) {
            if (usedFaction != null && usedFaction.DefaultStartingZone != string.Empty) {
                characterSaveData.CurrentScene = usedFaction.DefaultStartingZone;
            } else {
                characterSaveData.CurrentScene = systemConfigurationManager.DefaultStartingZone;
            }
            if (usedFaction != null) {
                characterSaveData.OverrideLocationTag = usedFaction.DefaultStartingLocationTag;
            }
        }

        public void ChooseNewCharacterRace(CharacterRace characterRace) {
            //Debug.Log($"NewGameManager.SetCharacterRace({(characterRace == null ? "null" : characterRace.DisplayName)})");

            if (this.characterRace == characterRace) {
                return;
            }

            SetCharacterRace(characterRace);

            UpdateUnitProfileList();

            UpdateCharacterEnvironment();
        }

        public void SetCharacterRace(CharacterRace characterRace) {
            //Debug.Log($"NewGameManager.SetCharacterRace({(characterRace == null ? "null" : characterRace.DisplayName)})");

            if (this.characterRace == characterRace) {
                return;
            }

            this.characterRace = characterRace;

            UpdateEquipmentList();

            defaultCharacterSaveData.CharacterRace = characterRace.ResourceName;

            // this code seems like a copy and paste that was never removed.  Monitor for breakage.
            /*
            if (faction != null && faction.DefaultStartingZone != string.Empty) {
                playerCharacterSaveData.SaveData.CurrentScene = faction.DefaultStartingZone;
            } else {
                playerCharacterSaveData.SaveData.CurrentScene = systemConfigurationManager.DefaultStartingZone;
            }
            if (faction != null) {
                levelManager.OverrideSpawnLocationTag = faction.DefaultStartingLocationTag;
            }
            */

            OnSetCharacterRace(characterRace);
        }

        public CharacterSaveData CreateNewPlayerSaveData(CharacterSaveData requestedSaveData) {
            
            // setup mutable properties
            CharacterSaveData characterSaveData = saveManager.CreateSaveData();
            characterSaveData.AppearanceString = requestedSaveData.AppearanceString;
            characterSaveData.SwappableMeshSaveData = requestedSaveData.SwappableMeshSaveData;
            characterSaveData.CharacterName = requestedSaveData.CharacterName;
            characterSaveData.UnitProfileName = requestedSaveData.UnitProfileName;
            characterSaveData.CharacterClass = requestedSaveData.CharacterClass;
            characterSaveData.ClassSpecialization = requestedSaveData.ClassSpecialization;
            characterSaveData.CharacterFaction = requestedSaveData.CharacterFaction;

            // set default starting zone
            Faction newFaction = systemDataFactory.GetResource<Faction>(requestedSaveData.CharacterFaction);
            PerformFactionOverrides(characterSaveData, newFaction);

            // setup initial equipment
            EquipmentManager newEquipmentManager = new EquipmentManager(systemGameManager);
            UpdateEquipmentList(newEquipmentManager, characterSaveData);

            return characterSaveData;
        }

        public void UpdateEquipmentList() {
            UpdateEquipmentList(defaultEquipmentManager, defaultCharacterSaveData);
            
            // show the equipment
            OnUpdateEquipmentList();
        }

        public void UpdateEquipmentList(EquipmentManager equipmentManager, CharacterSaveData characterSaveData) {
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
                    equipmentManager.EquipEquipment(systemItemManager.GetNewInstantiatedItem(equipment) as InstantiatedEquipment);
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
                    equipmentManager.EquipEquipment(systemItemManager.GetNewInstantiatedItem(equipment) as InstantiatedEquipment);
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
                        equipmentManager.EquipEquipment(systemItemManager.GetNewInstantiatedItem(equipment) as InstantiatedEquipment);
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
                    equipmentManager.EquipEquipment(systemItemManager.GetNewInstantiatedItem(equipment) as InstantiatedEquipment);
                }
            }

            // save the equipment
            SaveEquipmentData(equipmentManager, characterSaveData);
        }

        public void SaveEquipmentData(EquipmentManager equipmentManager, CharacterSaveData characterSaveData) {
            //Debug.Log("NewGameManager.SaveEquipmentData()");

            if (equipmentManager.CurrentEquipment == null) {
                // nothing to save
                return;
            }
            characterSaveData.EquipmentSaveData = new List<EquipmentInventorySlotSaveData>();
            foreach (EquipmentInventorySlot equipmentInventorySlot in equipmentManager.CurrentEquipment.Values) {
                EquipmentInventorySlotSaveData tmpSaveData = new EquipmentInventorySlotSaveData();
                if (equipmentInventorySlot.InstantiatedEquipment != null) {
                    tmpSaveData.HasItem = true;
                    tmpSaveData.ItemInstanceId = equipmentInventorySlot.InstantiatedEquipment.InstanceId;
                }
                characterSaveData.EquipmentSaveData.Add(tmpSaveData);
            }
        }

        public void SetSaveData(CharacterSaveData characterSaveData) {
            this.defaultCharacterSaveData = characterSaveData;
        }

        public CharacterConfigurationRequest GetCharacterConfigurationRequest() {
            CharacterConfigurationRequest characterConfigurationRequest = new CharacterConfigurationRequest(this);
            characterConfigurationRequest.unitControllerMode = UnitControllerMode.Preview;
            characterConfigurationRequest.characterName = playerName;
            return characterConfigurationRequest;
        }
    }

}