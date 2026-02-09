using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AnyRPG {
    public class SaveManager : ConfiguredClass {

        private Dictionary<string, CutsceneSaveData> cutsceneSaveDataDictionary = new Dictionary<string, CutsceneSaveData>();

        private string baseSaveFolderName = string.Empty;
        private int playerCharacterIdCounter = 1;

        private string offlineStateSaveFolderName = string.Empty;
        private const string offlineStateSaveFileName = "OfflineState.json";
        private OfflineStateSaveData offlineStateSaveData = new OfflineStateSaveData();

        // prevent infinite loop loading list, and why would anyone need more than 1000 save games at this point
        //private int maxSaveFiles = 1000;

        //private string saveFileName = "AnyRPGPlayerSaveData";

        protected bool eventSubscriptionsInitialized = false;

        // game manager references
        private PlayerManager playerManager = null;
        private PlayerManagerServer playerManagerServer = null;
        private MessageFeedManager messageFeedManager = null;
        private LevelManager levelManager = null;
        private UIManager uIManager = null;
        private NewGameManager newGameManager = null;
        private LoadGameManager loadGameManager = null;

        public Dictionary<string, CutsceneSaveData> CutsceneSaveDataDictionary { get => cutsceneSaveDataDictionary; set => cutsceneSaveDataDictionary = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            MakeBaseSaveFolder();
            LoadOfflineStateSaveData();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            playerManagerServer = systemGameManager.PlayerManagerServer;
            levelManager = systemGameManager.LevelManager;
            uIManager = systemGameManager.UIManager;
            messageFeedManager = uIManager.MessageFeedManager;
            newGameManager = systemGameManager.NewGameManager;
            loadGameManager = systemGameManager.LoadGameManager;
        }

        private void MakeBaseSaveFolder() {
            //Debug.Log("PlayerCharacterService.MakeSaveFolder()");

            Regex regex = new Regex("[^a-zA-Z0-9]");
            string gameNameString = regex.Replace(systemConfigurationManager.GameName, "");
            if (gameNameString == string.Empty) {
                return;
            }
            offlineStateSaveFolderName = $"{Application.persistentDataPath}/{gameNameString}/Offline";
            baseSaveFolderName = $"{Application.persistentDataPath}/{gameNameString}/Offline/PlayerCharacters";
            if (!Directory.Exists($"{Application.persistentDataPath}/{gameNameString}")) {
                Directory.CreateDirectory($"{Application.persistentDataPath}/{gameNameString}");
            }
            if (!Directory.Exists($"{Application.persistentDataPath}/{gameNameString}/Offline")) {
                Directory.CreateDirectory($"{Application.persistentDataPath}/{gameNameString}/Offline");
            }
            if (!Directory.Exists(baseSaveFolderName)) {
                Directory.CreateDirectory(baseSaveFolderName);
            }
        }

        public void LoadOfflineStateSaveData() {
            //Debug.Log($"SaveManager.LoadOfflineStateSaveData()");

            string jsonSavePath = $"{offlineStateSaveFolderName}/{offlineStateSaveFileName}";
            if (File.Exists(jsonSavePath)) {
                string jsonString = File.ReadAllText(jsonSavePath);
                offlineStateSaveData = JsonUtility.FromJson<OfflineStateSaveData>(jsonString);
                playerCharacterIdCounter = offlineStateSaveData.playerCharacterIdCounter;
            }
        }

        public void SaveOfflineStateSaveData() {
            //Debug.Log($"SaveManager.SaveOfflineStateSaveData()");

            string jsonString = JsonUtility.ToJson(offlineStateSaveData);
            string jsonSavePath = $"{offlineStateSaveFolderName}/{offlineStateSaveFileName}";
            File.WriteAllText(jsonSavePath, jsonString);
        }

        private int GetNewPlayerCharacterId() {
            //Debug.Log("SaveManager.GetNewPlayerCharacterId()");

            int returnValue = playerCharacterIdCounter;
            playerCharacterIdCounter++;
            offlineStateSaveData.playerCharacterIdCounter = playerCharacterIdCounter;
            SaveOfflineStateSaveData();

            //Debug.Log($"SaveManager.GetNewPlayerCharacterId() return {returnValue}");
            return returnValue;
        }

        public List<PlayerCharacterSaveData> GetSaveDataList() {
            //Debug.Log($"SaveManager.GetSaveDataList()");
            List<PlayerCharacterSaveData> saveDataList = new List<PlayerCharacterSaveData>();
            foreach (FileInfo fileInfo in GetSaveFileList()) {
                //Debug.Log("GetSaveDataList(): fileInfo.Name: " + fileInfo.Name);
                PlayerCharacterSaveData playerCharacterSaveData = LoadPlayerCharacterSaveDataFromFile($"{baseSaveFolderName}/{fileInfo.Name}");
                saveDataList.Add(playerCharacterSaveData);
            }
            return saveDataList;
        }

        public PlayerCharacterSaveData LoadPlayerCharacterSaveDataFromFile(string fileName) {
            //Debug.Log($"SaveManager.LoadSaveDataFromFile({fileName})");

            string fileContents = File.ReadAllText(fileName);
            return LoadPlayerCharacterSaveDataFromString(fileContents);
        }

        public PlayerCharacterSaveData LoadPlayerCharacterSaveDataFromString(string fileContents) {
            PlayerCharacterSaveData playerCharacterSaveData = JsonUtility.FromJson<PlayerCharacterSaveData>(fileContents);
            string saveDate = string.Empty;
            if (playerCharacterSaveData.CharacterSaveData.DataCreatedOn == string.Empty) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): DataCreatedOn is null setting to now");
                playerCharacterSaveData.CharacterSaveData.DataCreatedOn = DateTime.Now.ToLongDateString();
            }
            if (playerCharacterSaveData.CharacterSaveData.DataSavedOn == string.Empty) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): DataSavedOn is null setting to now");
                playerCharacterSaveData.CharacterSaveData.DataSavedOn = DateTime.Now.ToLongDateString();
            }
            CharacterSaveDataPostLoad(playerCharacterSaveData.CharacterSaveData);
            return playerCharacterSaveData;
        }

        public CharacterSaveData LoadCharacterSaveDataFromString(string fileContents) {
            //Debug.Log($"SaveManager.LoadSaveDataFromString({fileContents})");

            CharacterSaveData characterSaveData = JsonUtility.FromJson<CharacterSaveData>(fileContents);
            if (characterSaveData == null) {
                return null;
            }

            CharacterSaveDataPostLoad(characterSaveData);

            return characterSaveData;
        }

        public void CharacterSaveDataPostLoad(CharacterSaveData characterSaveData) {
            // when loaded from file, overrides should always be true because the file may have been saved before these were added
            // disabled because new games create a save file, so if we create a new game, then quit, then load that save file,
            // it will not have the overrides set, and we don't want to override the location and rotation
            // AnyRPG v1.0 will not be backwards compatible with old save files.
            //characterSaveData.OverrideLocation = true;
            //characterSaveData.OverrideRotation = true;

            if (characterSaveData.CharacterName == null) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): Player Name is null.  Setting to Unknown");
                characterSaveData.CharacterName = "Unknown";
            }
            if (characterSaveData.CharacterFaction == null) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): Player Faction is null.  Setting to default");
                characterSaveData.CharacterFaction = string.Empty;
            }
            if (characterSaveData.CharacterRace == null) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): Player Faction is null.  Setting to default");
                characterSaveData.CharacterRace = string.Empty;
            }
            if (characterSaveData.CharacterClass == null) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): Player Faction is null.  Setting to default");
                characterSaveData.CharacterClass = string.Empty;
            }
            if (characterSaveData.ClassSpecialization == null) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): Player Faction is null.  Setting to default");
                characterSaveData.ClassSpecialization = string.Empty;
            }
            if (characterSaveData.UnitProfileName == null) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): Player Faction is null.  Setting to default");
                characterSaveData.UnitProfileName = systemConfigurationManager.DefaultUnitProfileName;
            }
            if (characterSaveData.AppearanceString == null) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): Player appearance string is null.  Setting to empty");
                characterSaveData.AppearanceString = string.Empty;
            }
            if (characterSaveData.CurrentScene == null) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): CurrentScene is null.  Setting to default");
                characterSaveData.CurrentScene = systemConfigurationManager.DefaultStartingZone;
            }
            InitializeSaveDataResourceLists(characterSaveData, false);
        }

        public void InitializeSaveDataResourceLists(CharacterSaveData characterSaveData, bool overWrite) {
            //Debug.Log("SaveManager.InitializeResourceLists()");

            // things that are safe to overwrite any time because we get this data from other sources
            if (characterSaveData.SwappableMeshSaveData == null || overWrite) {
                characterSaveData.SwappableMeshSaveData = new List<SwappableMeshSaveData>();
            }
            if (characterSaveData.ResourcePowerSaveData == null || overWrite) {
                characterSaveData.ResourcePowerSaveData = new List<ResourcePowerSaveData>();
            }
            if (characterSaveData.ActionBarSaveData == null || overWrite) {
                characterSaveData.ActionBarSaveData = new List<ActionBarSaveData>();
            }
            if (characterSaveData.GamepadActionBarSaveData == null || overWrite) {
                characterSaveData.GamepadActionBarSaveData = new List<ActionBarSaveData>();
            }
            if (characterSaveData.InventorySlotSaveData == null || overWrite) {
                characterSaveData.InventorySlotSaveData = new List<InventorySlotSaveData>();
            }
            if (characterSaveData.BankSlotSaveData == null || overWrite) {
                characterSaveData.BankSlotSaveData = new List<InventorySlotSaveData>();
            }
            if (characterSaveData.EquippedBagSaveData == null || overWrite) {
                characterSaveData.EquippedBagSaveData = new List<EquippedBagSaveData>();
            }
            if (characterSaveData.EquippedBankBagSaveData == null || overWrite) {
                characterSaveData.EquippedBankBagSaveData = new List<EquippedBagSaveData>();
            }
            if (characterSaveData.AbilitySaveData == null || overWrite) {
                characterSaveData.AbilitySaveData = new List<AbilitySaveData>();
            }
            if (characterSaveData.SkillSaveData == null || overWrite) {
                characterSaveData.SkillSaveData = new List<SkillSaveData>();
            }
            if (characterSaveData.RecipeSaveData == null || overWrite) {
                characterSaveData.RecipeSaveData = new List<RecipeSaveData>();
            }
            if (characterSaveData.ReputationSaveData == null || overWrite) {
                characterSaveData.ReputationSaveData = new List<ReputationSaveData>();
            }
            if (characterSaveData.EquipmentSaveData == null || overWrite) {
                characterSaveData.EquipmentSaveData = new List<EquipmentInventorySlotSaveData>();
            }
            if (characterSaveData.CurrencySaveData == null || overWrite) {
                characterSaveData.CurrencySaveData = new List<CurrencySaveData>();
            }
            if (characterSaveData.StatusEffectSaveData == null || overWrite) {
                characterSaveData.StatusEffectSaveData = new List<StatusEffectSaveData>();
            }
            if (characterSaveData.PetSaveData == null || overWrite) {
                characterSaveData.PetSaveData = new List<PetSaveData>();
            }
        }

        public List<FileInfo> GetSaveFileList() {
            List<FileInfo> returnList = new List<FileInfo>();
            DirectoryInfo directoryInfo = new DirectoryInfo(baseSaveFolderName);

            foreach (FileInfo fileInfo in directoryInfo.GetFiles("*.json")) {
                returnList.Add(fileInfo);
            }
            return returnList;
        }

        public bool SaveGame(CharacterSaveData characterSaveData) {
            //Debug.Log("Savemanager.SaveGame()");

            // check if the player is inside a trigger
            // disallow saving if they are because we don't want to trigger boss spawns
            // or cutscenes when the player loads back in the game
            if (playerManager.ActiveUnitController != null) {
                bool canSave = true;
                Collider playerCollider = playerManager.ActiveUnitController.Collider;
                int validMask = (1 << LayerMask.NameToLayer("Triggers") | 1 << LayerMask.NameToLayer("Interactable") | 1 << LayerMask.NameToLayer("Ignore Raycast"));
                Collider[] hitColliders = new Collider[100];
                playerManager.ActiveUnitController.PhysicsScene.OverlapCapsule(playerCollider.bounds.center + new Vector3(0, playerCollider.bounds.extents.y, 0),
                    playerCollider.bounds.center - new Vector3(0, playerCollider.bounds.extents.y, 0),
                    playerManager.ActiveUnitController.Collider.bounds.extents.x, hitColliders, validMask);
                foreach (Collider hitCollider in hitColliders) {
                    if (hitCollider == null) {
                        continue;
                    }
                    if (hitCollider.isTrigger == true) {
                        Interactable interactable = hitCollider.gameObject.GetComponent<Interactable>();
                        if (interactable != null && interactable.IsTrigger == true) {
                            canSave = false;
                            break;
                        }
                    }
                }
                if (canSave == false) {
                    messageFeedManager.WriteMessage("You cannot save here");
                    return false;
                }
            }

            // do this first because persistent objects need to add their locations to the scene node before we write it to disk
            SystemEventManager.TriggerEvent("OnSaveGame", new EventParamProperties());

            SaveCutsceneData(characterSaveData);
            playerManager.UnitController.CharacterSaveManager.SaveGameData();

            SaveWindowPositions();

            //Debug.Log("Savemanager.SaveQuestData(): size: " + characterSaveData.questSaveData.Count);

            // create save data bundle
            PlayerCharacterSaveData playerCharacterSaveData = new PlayerCharacterSaveData(characterSaveData, systemItemManager);
            bool saveResult = SaveDataFile(playerCharacterSaveData);
            if (saveResult) {
                PlayerPrefs.SetInt("LastOfflinePlayerCharacterId", characterSaveData.CharacterId);
            }

            return saveResult;
        }

        public bool SaveDataFile(PlayerCharacterSaveData playerCharacterSaveData) {

            playerCharacterSaveData.CharacterSaveData.DataSavedOn = DateTime.Now.ToLongDateString();

            string jsonString = JsonUtility.ToJson(playerCharacterSaveData);
            //Debug.Log(jsonString);
            string jsonSavePath = $"{baseSaveFolderName}/{playerCharacterSaveData.CharacterSaveData.CharacterId}.json";
            File.WriteAllText(jsonSavePath, jsonString);
            
            return true;
        }
    
        public void NewGame(CharacterSaveData characterSaveData) {
            //Debug.Log($"Savemanager.NewGame({characterSaveData.UnitProfileName})");

            ClearSystemManagedSaveData();

            CreateLocalGame(characterSaveData);
        }

        private void CreateLocalGame(CharacterSaveData characterSaveData) {
            //Debug.Log($"Savemanager.CreateLocalGame({playerCharacterSaveData.SaveData.unitProfileName})");

            // assign new character id
            int newCharacterId = GetNewPlayerCharacterId();
            characterSaveData.CharacterId = newCharacterId;

            // create save data bundle
            PlayerCharacterSaveData playerCharacterSaveData = new PlayerCharacterSaveData(characterSaveData, systemItemManager);

            SaveDataFile(playerCharacterSaveData);
            PlayerPrefs.SetInt("LastOfflinePlayerCharacterId", characterSaveData.CharacterId);

            LoadGame(playerCharacterSaveData);
        }

        public void PerformInventorySetup(CharacterSaveData characterSaveData) {
            //Debug.Log("SaveManager.PerformInventorySetup()");

            // initialize inventory
            int bagCount = 0;

            // add default backpack
            if (systemConfigurationManager.DefaultBackpackItem != null && systemConfigurationManager.DefaultBackpackItem != string.Empty) {
                Bag bag = systemDataFactory.GetResource<Item>(systemConfigurationManager.DefaultBackpackItem) as Bag;
                if (bag == null) {
                    Debug.LogError("SaveManager.PerformInventorySetup(): Check SystemConfigurationManager in inspector and set DefaultBackpack to valid name");
                    // would like to return null here but this is a value type :(
                    return;
                }
                InstantiatedBag instantiatedBag = systemItemManager.GetNewInstantiatedItem(systemConfigurationManager.DefaultBackpackItem) as InstantiatedBag;
                if (instantiatedBag != null) {
                    EquippedBagSaveData saveData = new EquippedBagSaveData();
                    saveData.HasItem = true;
                    saveData.ItemInstanceId = instantiatedBag.InstanceId;
                    characterSaveData.EquippedBagSaveData.Add(saveData);
                    bagCount++;

                    // add inventory slots from bag
                    for (int i = 0; i < bag.Slots; i++) {
                        characterSaveData.InventorySlotSaveData.Add(new InventorySlotSaveData());
                    }
                }
            }

            // add empty bag nodes
            for (int i = bagCount; i < systemConfigurationManager.MaxInventoryBags; i++) {
                characterSaveData.EquippedBagSaveData.Add(new EquippedBagSaveData());
            }

            // add empty bank nodes
            for (int i = bagCount; i < systemConfigurationManager.MaxBankBags; i++) {
                characterSaveData.EquippedBankBagSaveData.Add(new EquippedBagSaveData());
            }

            // add default bank contents
            for (int i = 0; i < systemConfigurationManager.DefaultBankSlots; i++) {
                if (systemConfigurationManager.DefaultBankContents.Count > i) {
                    InventorySlotSaveData inventorySlotSaveData = new InventorySlotSaveData();
                    InstantiatedItem instantiatedItem = systemItemManager.GetNewInstantiatedItem(systemConfigurationManager.DefaultBankContents[i]);
                    if (instantiatedItem != null) {
                        //Debug.Log($"SaveManager.PerformInventorySetup(): adding default bank item: {instantiatedItem.ResourceName}");
                        inventorySlotSaveData.ItemInstanceIds.Add(instantiatedItem.InstanceId);
                    }
                    characterSaveData.BankSlotSaveData.Add(inventorySlotSaveData);
                }
            }

        }

        /*
        public InventorySlotSaveData GetEmptySlotSaveData() {
            InventorySlotSaveData saveData = new InventorySlotSaveData();
            saveData.ItemName = string.Empty;
            saveData.DisplayName = string.Empty;
            saveData.itemQuality = string.Empty;
            saveData.dropLevel = 0;
            saveData.randomSecondaryStatIndexes = new List<int>();

            return saveData;
        }
        */

        public void CreateDefaultBackpack() {
            //Debug.Log("InventoryManager.CreateDefaultBackpack()");
            if (systemConfigurationManager.DefaultBackpackItem != null && systemConfigurationManager.DefaultBackpackItem != string.Empty) {
                InstantiatedBag instantiatedBag = playerManager.UnitController.CharacterInventoryManager.GetNewInstantiatedItem(systemConfigurationManager.DefaultBackpackItem) as InstantiatedBag;
                if (instantiatedBag == null) {
                    Debug.LogError("InventoryManager.CreateDefaultBankBag(): CHECK INVENTORYMANAGER IN INSPECTOR AND SET DEFAULTBACKPACK TO VALID NAME");
                    return;
                }
                playerManager.UnitController.CharacterInventoryManager.AddInventoryBag(instantiatedBag);
            }
        }

        public void LoadGame() {
            //Debug.Log("SaveManager.LoadGame()");
            if (PlayerPrefs.HasKey("LastOfflinePlayerCharacterId")) {
                //Debug.Log("SaveManager.LoadGame(): Last Save Data: " + PlayerPrefs.GetString("LastSaveDataFileName"));
                loadGameManager.LoadCharacterList();
                foreach (PlayerCharacterSaveData playerCharacterSaveData in loadGameManager.CharacterList) {
                    if (playerCharacterSaveData.CharacterSaveData.CharacterId == PlayerPrefs.GetInt("LastOfflinePlayerCharacterId")) {
                        //Debug.Log("SaveManager.LoadGame(): Last Save Data: " + PlayerPrefs.GetString("LastSaveDataFileName") + " was found.  Loading Game...");
                        LoadGame(playerCharacterSaveData);
                        return;
                    }
                }
            }
            newGameManager.NewLocalGame();
        }

        public CapabilityConsumerSnapshot GetCapabilityConsumerSnapshot(CharacterSaveData saveData) {
            CapabilityConsumerSnapshot returnValue = new CapabilityConsumerSnapshot(systemGameManager);
            returnValue.UnitProfile = systemDataFactory.GetResource<UnitProfile>(saveData.UnitProfileName);
            returnValue.CharacterRace = systemDataFactory.GetResource<CharacterRace>(saveData.CharacterRace);
            returnValue.CharacterClass = systemDataFactory.GetResource<CharacterClass>(saveData.CharacterClass);
            returnValue.ClassSpecialization = systemDataFactory.GetResource<ClassSpecialization>(saveData.ClassSpecialization);
            returnValue.Faction = systemDataFactory.GetResource<Faction>(saveData.CharacterFaction);
            return returnValue;
        }

        public void ClearSharedData() {
            //Debug.Log("SaveManager.ClearSharedData()");

            systemItemManager.ClientReset();
            ClearSystemManagedSaveData();
        }

        public CharacterSaveData CreateSaveData() {
            //Debug.Log("SaveManager.CreateSaveData()");

            CharacterSaveData newSaveData = new CharacterSaveData();
            newSaveData.CharacterName = systemConfigurationManager.DefaultPlayerName;
            newSaveData.CharacterLevel = 1;
            newSaveData.InitializeResourceAmounts = true;
            newSaveData.CurrentScene = systemConfigurationManager.DefaultStartingZone;
            newSaveData.UnitProfileName = systemConfigurationManager.DefaultUnitProfileName;
            //Debug.Log($"SaveManager.CreateSaveData() unitProfileName: {systemConfigurationManager.DefaultUnitProfileName}");

            if (newSaveData.DataCreatedOn == null || newSaveData.DataCreatedOn == string.Empty) {
                newSaveData.DataCreatedOn = DateTime.Now.ToLongDateString();
            }

            SceneNode sceneNode = systemDataFactory.GetResource<SceneNode>(systemConfigurationManager.DefaultStartingZone);
            if (sceneNode != null) {
                newSaveData.CurrentScene = sceneNode.SceneFile;
            } else {
                newSaveData.CurrentScene = systemConfigurationManager.DefaultStartingZone;
                //Debug.LogError("LevelManager.LoadLevel(" + levelName + "): could not find scene node with that name!");
            }

            InitializeSaveDataResourceLists(newSaveData, false);

            // initialize inventory
            PerformInventorySetup(newSaveData);

            return newSaveData;
        }

        /*
        public AnyRPGSaveData InitializeSaveDataProperties(AnyRPGSaveData saveData) {
            saveData.playerName = string.Empty;
            saveData.unitProfileName = string.Empty;
            saveData.characterRace = string.Empty;
            saveData.characterClass = string.Empty;
            saveData.classSpecialization = string.Empty;
            saveData.playerFaction = string.Empty;
            saveData.appearanceString = string.Empty;
            saveData.CurrentScene = string.Empty;
            saveData.DataCreatedOn = string.Empty;
            saveData.DataFileName = string.Empty;

            return saveData;
        }
        */


        public void LoadGame(PlayerCharacterSaveData playerCharacterSaveData) {
            //Debug.Log($"Savemanager.LoadGame({playerCharacterSaveData.SaveData.unitProfileName})");

            uIManager.loadGameWindow.CloseWindow();

            ClearSharedData();
            systemItemManager.LoadPlayerCharacterSaveData(playerCharacterSaveData);

            // scene and location
            Vector3 playerLocation = new Vector3(playerCharacterSaveData.CharacterSaveData.PlayerLocationX, playerCharacterSaveData.CharacterSaveData.PlayerLocationY, playerCharacterSaveData.CharacterSaveData.PlayerLocationZ);
            Vector3 playerRotation = new Vector3(playerCharacterSaveData.CharacterSaveData.PlayerRotationX, playerCharacterSaveData.CharacterSaveData.PlayerRotationY, playerCharacterSaveData.CharacterSaveData.PlayerRotationZ);
            //Debug.Log("Savemanager.LoadGame() rotation: " + characterSaveData.PlayerRotationX + ", " + characterSaveData.PlayerRotationY + ", " + characterSaveData.PlayerRotationZ);

            playerManager.SpawnPlayerConnection(playerCharacterSaveData.CharacterSaveData);

            LoadCutsceneData(playerCharacterSaveData.CharacterSaveData);

            // testing - moved to UIManager initialization so it works in online mode
            //LoadWindowPositions();

            CapabilityConsumerSnapshot capabilityConsumerSnapshot = GetCapabilityConsumerSnapshot(playerCharacterSaveData.CharacterSaveData);

            SpawnPlayerRequest loadSceneRequest = new SpawnPlayerRequest();
            // configure location and rotation overrides
            if (playerCharacterSaveData.CharacterSaveData.OverrideLocation == true) {
                loadSceneRequest.overrideSpawnLocation = true;
                loadSceneRequest.spawnLocation = playerLocation;
                //Debug.Log($"Savemanager.LoadGame() overrideSpawnLocation: {loadSceneRequest.overrideSpawnLocation} location: {loadSceneRequest.spawnLocation}");
            }
            if (playerCharacterSaveData.CharacterSaveData.OverrideRotation == true) {
                loadSceneRequest.overrideSpawnDirection = true;
                loadSceneRequest.spawnForwardDirection = playerRotation;
                //Debug.Log($"Savemanager.LoadGame() overrideRotation: {loadSceneRequest.overrideSpawnDirection} location: {loadSceneRequest.spawnForwardDirection}");
            }
            // debug print the location and rotation
            //Debug.Log($"Savemanager.LoadGame(): Spawning player at {loadSceneRequest.spawnLocation} with rotation {loadSceneRequest.spawnForwardDirection}");
            playerManagerServer.AddSpawnRequest(networkManagerClient.AccountId, loadSceneRequest);
            //levelManager.LoadLevel(characterSaveData.CurrentScene, playerLocation, playerRotation);
            // load the proper level now that everything should be setup
            levelManager.LoadLevel(playerCharacterSaveData.CharacterSaveData.CurrentScene);
        }

        public void ClearSystemManagedSaveData() {
            //Debug.Log("Savemanager.ClearSystemmanagedCharacterData()");
            cutsceneSaveDataDictionary.Clear();
        }

        public void LoadWindowPositions() {
            //Debug.Log("Savemanager.LoadWindowPositions()");

            // bag windows
            // set to playerprefs values

            if (PlayerPrefs.HasKey("AbilityBookWindowX") && PlayerPrefs.HasKey("AbilityBookWindowY"))
                uIManager.abilityBookWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("AbilityBookWindowX"), PlayerPrefs.GetFloat("AbilityBookWindowY"), 0);
            if (PlayerPrefs.HasKey("AuctionWindowX") && PlayerPrefs.HasKey("AuctionWindowY"))
                uIManager.auctionWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("AuctionWindowX"), PlayerPrefs.GetFloat("AuctionWindowY"), 0);
            if (PlayerPrefs.HasKey("SkillBookWindowX") && PlayerPrefs.HasKey("SkillBookWindowY"))
                uIManager.skillBookWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("SkillBookWindowX"), PlayerPrefs.GetFloat("SkillBookWindowY"), 0);
            if (PlayerPrefs.HasKey("ReputationBookWindowX") && PlayerPrefs.HasKey("ReputationBookWindowY"))
                uIManager.reputationBookWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("ReputationBookWindowX"), PlayerPrefs.GetFloat("ReputationBookWindowY"), 0);
            if (PlayerPrefs.HasKey("CurrencyListWindowX") && PlayerPrefs.HasKey("CurrencyListWindowY"))
                uIManager.currencyListWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("CurrencyListWindowX"), PlayerPrefs.GetFloat("CurrencyListWindowY"), 0);
            if (PlayerPrefs.HasKey("AchievementListWindowX") && PlayerPrefs.HasKey("AchievementListWindowY"))
                uIManager.achievementListWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("AchievementListWindowX"), PlayerPrefs.GetFloat("AchievementListWindowY"), 0);
            if (PlayerPrefs.HasKey("CharacterPanelWindowX") && PlayerPrefs.HasKey("CharacterPanelWindowY"))
                uIManager.characterPanelWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("CharacterPanelWindowX"), PlayerPrefs.GetFloat("CharacterPanelWindowY"), 0);
            if (PlayerPrefs.HasKey("LootWindowX") && PlayerPrefs.HasKey("LootWindowY"))
                uIManager.lootWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("LootWindowX"), PlayerPrefs.GetFloat("LootWindowY"), 0);
            if (PlayerPrefs.HasKey("VendorWindowX") && PlayerPrefs.HasKey("VendorWindowY"))
                uIManager.vendorWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("VendorWindowX"), PlayerPrefs.GetFloat("VendorWindowY"), 0);
            //if (PlayerPrefs.HasKey("ChestWindowX") && PlayerPrefs.HasKey("ChestWindowY"))
                //uIManager.chestWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("ChestWindowX"), PlayerPrefs.GetFloat("ChestWindowY"), 0);
            if (PlayerPrefs.HasKey("BankWindowX") && PlayerPrefs.HasKey("BankWindowY"))
                uIManager.bankWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("BankWindowX"), PlayerPrefs.GetFloat("BankWindowY"), 0);
            if (PlayerPrefs.HasKey("InventoryWindowX") && PlayerPrefs.HasKey("InventoryWindowY"))
                uIManager.inventoryWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("InventoryWindowX"), PlayerPrefs.GetFloat("InventoryWindowY"), 0);
            if (PlayerPrefs.HasKey("QuestLogWindowX") && PlayerPrefs.HasKey("QuestLogWindowY"))
                uIManager.questLogWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("QuestLogWindowX"), PlayerPrefs.GetFloat("QuestLogWindowY"), 0);
            if (PlayerPrefs.HasKey("QuestGiverWindowX") && PlayerPrefs.HasKey("QuestGiverWindowY"))
                uIManager.questGiverWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("QuestGiverWindowX"), PlayerPrefs.GetFloat("QuestGiverWindowY"), 0);
            if (PlayerPrefs.HasKey("SkillTrainerWindowX") && PlayerPrefs.HasKey("SkillTrainerWindowY"))
                uIManager.skillTrainerWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("SkillTrainerWindowX"), PlayerPrefs.GetFloat("SkillTrainerWindowY"), 0);
            if (PlayerPrefs.HasKey("MusicPlayerWindowX") && PlayerPrefs.HasKey("MusicPlayerWindowY"))
                uIManager.musicPlayerWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("MusicPlayerWindowX"), PlayerPrefs.GetFloat("MusicPlayerWindowY"), 0);
            if (PlayerPrefs.HasKey("InteractionWindowX") && PlayerPrefs.HasKey("InteractionWindowY"))
                uIManager.interactionWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("InteractionWindowX"), PlayerPrefs.GetFloat("InteractionWindowY"), 0);
            if (PlayerPrefs.HasKey("CraftingWindowX") && PlayerPrefs.HasKey("CraftingWindowY"))
                uIManager.craftingWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("CraftingWindowX"), PlayerPrefs.GetFloat("CraftingWindowY"), 0);
            if (PlayerPrefs.HasKey("MainMapWindowX") && PlayerPrefs.HasKey("MainMapWindowY"))
                uIManager.mainMapWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("MainMapWindowX"), PlayerPrefs.GetFloat("MainMapWindowY"), 0);
            if (PlayerPrefs.HasKey("DialogWindowX") && PlayerPrefs.HasKey("DialogWindowY"))
                uIManager.dialogWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("DialogWindowX"), PlayerPrefs.GetFloat("DialogWindowY"), 0);
            if (PlayerPrefs.HasKey("MailboxWindowX") && PlayerPrefs.HasKey("MailboxWindowY"))
                uIManager.mailboxWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("MailboxWindowX"), PlayerPrefs.GetFloat("MailboxWindowY"), 0);
            if (PlayerPrefs.HasKey("MailComposeWindowX") && PlayerPrefs.HasKey("MailComposeWindowY"))
                uIManager.mailComposeWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("MailComposeWindowX"), PlayerPrefs.GetFloat("MailComposeWindowY"), 0);
            if (PlayerPrefs.HasKey("MailViewWindowX") && PlayerPrefs.HasKey("MailViewWindowY"))
                uIManager.mailViewWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("MailViewWindowX"), PlayerPrefs.GetFloat("MailViewWindowY"), 0);
            if (PlayerPrefs.HasKey("AuctionWindowX") && PlayerPrefs.HasKey("AuctionWindowY"))
                uIManager.auctionWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("AuctionWindowX"), PlayerPrefs.GetFloat("AuctionWindowY"), 0);
            if (PlayerPrefs.HasKey("SocialWindowX") && PlayerPrefs.HasKey("SocialWindowY"))
                uIManager.socialWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("SocialWindowX"), PlayerPrefs.GetFloat("SocialWindowY"), 0);

            if (PlayerPrefs.HasKey("QuestTrackerWindowX") && PlayerPrefs.HasKey("QuestTrackerWindowY"))
                uIManager.QuestTrackerWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("QuestTrackerWindowX"), PlayerPrefs.GetFloat("QuestTrackerWindowY"), 0);
            if (PlayerPrefs.HasKey("MessageLogWindowX") && PlayerPrefs.HasKey("MessageLogWindowY"))
                uIManager.MessageLogWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("MessageLogWindowX"), PlayerPrefs.GetFloat("MessageLogWindowY"), 0);

            if (PlayerPrefs.HasKey("MessageFeedManagerX") && PlayerPrefs.HasKey("MessageFeedManagerY"))
                messageFeedManager.MessageFeedWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("MessageFeedManagerX"), PlayerPrefs.GetFloat("MessageFeedManagerY"), 0);

            if (PlayerPrefs.HasKey("FloatingCastBarControllerX") && PlayerPrefs.HasKey("FloatingCastBarControllerY")) {
                uIManager.FloatingCastBarWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("FloatingCastBarControllerX"), PlayerPrefs.GetFloat("FloatingCastBarControllerY"), 0);
            }

            if (PlayerPrefs.HasKey("StatusEffectPanelControllerX") && PlayerPrefs.HasKey("StatusEffectPanelControllerY"))
                uIManager.StatusEffectWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("StatusEffectPanelControllerX"), PlayerPrefs.GetFloat("StatusEffectPanelControllerY"), 0);

            if (PlayerPrefs.HasKey("PlayerUnitFrameControllerX") && PlayerPrefs.HasKey("PlayerUnitFrameControllerY"))
                uIManager.PlayerUnitFrameWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("PlayerUnitFrameControllerX"), PlayerPrefs.GetFloat("PlayerUnitFrameControllerY"), 0);

            if (PlayerPrefs.HasKey("FocusUnitFrameControllerX") && PlayerPrefs.HasKey("FocusUnitFrameControllerY"))
                uIManager.FocusUnitFrameWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("FocusUnitFrameControllerX"), PlayerPrefs.GetFloat("FocusUnitFrameControllerY"), 0);

            if (PlayerPrefs.HasKey("GroupUnitFramesWindowX") && PlayerPrefs.HasKey("GroupUnitFramesWindowY"))
                uIManager.GroupUnitFramesWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("GroupUnitFramesWindowX"), PlayerPrefs.GetFloat("GroupUnitFramesWindowY"), 0);

            if (PlayerPrefs.HasKey("MiniMapControllerX") && PlayerPrefs.HasKey("MiniMapControllerY"))
                uIManager.MiniMapWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("MiniMapControllerX"), PlayerPrefs.GetFloat("MiniMapControllerY"), 0);

            if (PlayerPrefs.HasKey("XPBarControllerX") && PlayerPrefs.HasKey("XPBarControllerY"))
                uIManager.XPBarWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("XPBarControllerX"), PlayerPrefs.GetFloat("XPBarControllerY"), 0);

            if (PlayerPrefs.HasKey("BottomPanelX") && PlayerPrefs.HasKey("BottomPanelY"))
                uIManager.BottomPanel.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("BottomPanelX"), PlayerPrefs.GetFloat("BottomPanelY"), 0);

            if (PlayerPrefs.HasKey("SidePanelX") && PlayerPrefs.HasKey("SidePanelY"))
                uIManager.SidePanel.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("SidePanelX"), PlayerPrefs.GetFloat("SidePanelY"), 0);

            if (PlayerPrefs.HasKey("MouseOverWindowX") && PlayerPrefs.HasKey("MouseOverWindowY"))
                uIManager.MouseOverWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("MouseOverWindowX"), PlayerPrefs.GetFloat("MouseOverWindowY"), 0);

        }

       

        public void SaveWindowPositions() {
            //Debug.Log("Savemanager.SaveWindowPositions()");

            PlayerPrefs.SetFloat("AbilityBookWindowX", uIManager.abilityBookWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("AbilityBookWindowY", uIManager.abilityBookWindow.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("AuctionWindowX", uIManager.auctionWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("AuctionWindowY", uIManager.auctionWindow.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("SkillBookWindowX", uIManager.skillBookWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("SkillBookWindowY", uIManager.skillBookWindow.RectTransform.anchoredPosition.y);

            //Debug.Log("abilityBookWindowX: " + abilityBookWindowX + "; abilityBookWindowY: " + abilityBookWindowY);
            PlayerPrefs.SetFloat("ReputationBookWindowX", uIManager.reputationBookWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("ReputationBookWindowY", uIManager.reputationBookWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("CurrencyListWindowX", uIManager.currencyListWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("CurrencyListWindowY", uIManager.currencyListWindow.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("CharacterPanelWindowX", uIManager.characterPanelWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("CharacterPanelWindowY", uIManager.characterPanelWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("LootWindowX", uIManager.lootWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("LootWindowY", uIManager.lootWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("VendorWindowX", uIManager.vendorWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("VendorWindowY", uIManager.vendorWindow.RectTransform.anchoredPosition.y);
            //PlayerPrefs.SetFloat("ChestWindowX", uIManager.chestWindow.RectTransform.anchoredPosition.x);
            //PlayerPrefs.SetFloat("ChestWindowY", uIManager.chestWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("BankWindowX", uIManager.bankWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("BankWindowY", uIManager.bankWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("InventoryWindowX", uIManager.inventoryWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("InventoryWindowY", uIManager.inventoryWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("QuestLogWindowX", uIManager.questLogWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("QuestLogWindowY", uIManager.questLogWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("AchievementListWindowX", uIManager.achievementListWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("AchievementListWindowY", uIManager.achievementListWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("QuestGiverWindowX", uIManager.questGiverWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("QuestGiverWindowY", uIManager.questGiverWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("SkillTrainerWindowX", uIManager.skillTrainerWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("SkillTrainerWindowY", uIManager.skillTrainerWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("MusicPlayerWindowX", uIManager.musicPlayerWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("MusicPlayerWindowY", uIManager.musicPlayerWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("InteractionWindowX", uIManager.interactionWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("InteractionWindowY", uIManager.interactionWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("CraftingWindowX", uIManager.craftingWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("CraftingWindowY", uIManager.craftingWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("MainMapWindowX", uIManager.mainMapWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("MainMapWindowY", uIManager.mainMapWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("DialogWindowX", uIManager.dialogWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("DialogWindowY", uIManager.dialogWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("MailboxWindowX", uIManager.mailboxWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("MailboxWindowY", uIManager.mailboxWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("MailComposeWindowX", uIManager.mailComposeWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("MailComposeWindowY", uIManager.mailComposeWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("MailViewWindowX", uIManager.mailViewWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("MailViewWindowY", uIManager.mailViewWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("SocialWindowX", uIManager.socialWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("SocialWindowY", uIManager.socialWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("AuctionWindowX", uIManager.auctionWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("AuctionWindowY", uIManager.auctionWindow.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("QuestTrackerWindowX", uIManager.QuestTrackerWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("QuestTrackerWindowY", uIManager.QuestTrackerWindow.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("MessageLogWindowX", uIManager.MessageLogWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("MessageLogWindowY", uIManager.MessageLogWindow.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("MessageFeedManagerX", messageFeedManager.MessageFeedWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("MessageFeedManagerY", messageFeedManager.MessageFeedWindow.RectTransform.anchoredPosition.y);

            //Debug.Log("Saving FloatingCastBarController: " + uIManager.MyFloatingCastBarController.RectTransform.anchoredPosition.x + "; " + uIManager.MyFloatingCastBarController.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("FloatingCastBarControllerX", uIManager.FloatingCastBarWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("FloatingCastBarControllerY", uIManager.FloatingCastBarWindow.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("StatusEffectPanelControllerX", uIManager.StatusEffectWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("StatusEffectPanelControllerY", uIManager.StatusEffectWindow.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("PlayerUnitFrameControllerX", uIManager.PlayerUnitFrameWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("PlayerUnitFrameControllerY", uIManager.PlayerUnitFrameWindow.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("FocusUnitFrameControllerX", uIManager.FocusUnitFrameWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("FocusUnitFrameControllerY", uIManager.FocusUnitFrameWindow.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("GroupUnitFramesWindowX", uIManager.GroupUnitFramesWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("GroupUnitFramesWindowY", uIManager.GroupUnitFramesWindow.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("MiniMapControllerX", uIManager.MiniMapWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("MiniMapControllerY", uIManager.MiniMapWindow.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("XPBarControllerX", uIManager.XPBarWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("XPBarControllerY", uIManager.XPBarWindow.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("BottomPanelX", uIManager.BottomPanel.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("BottomPanelY", uIManager.BottomPanel.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("SidePanelX", uIManager.SidePanel.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("SidePanelY", uIManager.SidePanel.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("MouseOverWindowX", uIManager.MouseOverWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("MouseOverWindowY", uIManager.MouseOverWindow.RectTransform.anchoredPosition.y);

        }

        public void DeleteGame(CharacterSaveData characterSaveData) {
            //Debug.Log("Savemanager.DeleteGame()");

            string saveFileName = $"{baseSaveFolderName}/{characterSaveData.CharacterId}.json";
            File.Delete($"{saveFileName}");
        }

        public void CopyGame(CharacterSaveData characterSaveData) {
            string sourceFileName = $"{baseSaveFolderName}/{characterSaveData.CharacterId}.json";
            int newCharacterId = GetNewPlayerCharacterId();
            string newSaveFileName = $"{baseSaveFolderName}/{newCharacterId}.json";

            File.Copy(sourceFileName, newSaveFileName);
            PlayerCharacterSaveData tmpSaveData = LoadPlayerCharacterSaveDataFromFile(newSaveFileName);
            tmpSaveData.CharacterSaveData.CharacterId = newCharacterId;
            SaveDataFile(tmpSaveData);
        }

        public List<PersistentObjectSaveData> GetPersistentObjects(SceneNode sceneNode) {
            if (playerManagerServer.PlayerCharacterMonitors.ContainsKey(0) == false) {
                Debug.LogWarning($"SaveManager.GetPersistentObjects({sceneNode.ResourceName}): playerManager.UnitController is null.  Returning empty list.");
                return new List<PersistentObjectSaveData>();
            }
            return GetSceneNodeSaveData(sceneNode).PersistentObjects;
        }

        public SceneNodeSaveData GetSceneNodeSaveData(SceneNode sceneNode) {
            CharacterSaveData characterSaveData = playerManagerServer.PlayerCharacterMonitors[0].characterSaveData;
            foreach (SceneNodeSaveData sceneNodeSaveData in characterSaveData.SceneNodeSaveData) {
                if (sceneNodeSaveData.SceneName == sceneNode.ResourceName) {
                    return sceneNodeSaveData;
                }
            }
            SceneNodeSaveData saveData = new SceneNodeSaveData();
            saveData.PersistentObjects = new List<PersistentObjectSaveData>();
            saveData.SceneName = sceneNode.ResourceName;
            return saveData;
        }

        public void SaveSceneNodeSaveData(SceneNodeSaveData sceneNodeSaveData) {
            //Debug.Log(DisplayName + ".SceneNode.SaveSceneNodeSaveData(" + sceneNodeSaveData.SceneName + ")");
            CharacterSaveData characterSaveData = playerManagerServer.PlayerCharacterMonitors[0].characterSaveData;
            foreach (SceneNodeSaveData _sceneNodeSaveData in characterSaveData.SceneNodeSaveData) {
                if (_sceneNodeSaveData.SceneName == sceneNodeSaveData.SceneName) {
                    characterSaveData.SceneNodeSaveData.Remove(_sceneNodeSaveData);
                    break;
                }
            }
            characterSaveData.SceneNodeSaveData.Add(sceneNodeSaveData);
        }

        public void SavePersistentObject(string UUID, PersistentObjectSaveData persistentObjectSaveData, SceneNode sceneNode) {
            //Debug.Log(DisplayName + ".SceneNode.SavePersistentObject(" + UUID + ")");

            SceneNodeSaveData saveData = GetSceneNodeSaveData(sceneNode);
            foreach (PersistentObjectSaveData _persistentObjectSaveData in saveData.PersistentObjects) {
                if (_persistentObjectSaveData.UUID == UUID) {
                    saveData.PersistentObjects.Remove(_persistentObjectSaveData);
                    break;
                }
            }
            saveData.PersistentObjects.Add(persistentObjectSaveData);
            SaveSceneNodeSaveData(saveData);
        }

        public PersistentObjectSaveData GetPersistentObject(string UUID, SceneNode sceneNode) {
            foreach (PersistentObjectSaveData _persistentObjectSaveData in GetSceneNodeSaveData(sceneNode).PersistentObjects) {
                if (_persistentObjectSaveData.UUID == UUID) {
                    return _persistentObjectSaveData;
                }
            }
            return new PersistentObjectSaveData();
        }

        public CutsceneSaveData GetCutsceneSaveData(Cutscene cutscene) {
            CutsceneSaveData saveData;
            if (cutsceneSaveDataDictionary.ContainsKey(cutscene.ResourceName)) {
                //Debug.Log("Savemanager.GetCutsceneSaveData(): loading existing save data for: " + cutscene.DisplayName);
                saveData = cutsceneSaveDataDictionary[cutscene.ResourceName];
            } else {
                //Debug.Log("Savemanager.GetCutsceneSaveData(): generating new cutscene save data for: " + cutscene.DisplayName);
                saveData = new CutsceneSaveData();
                saveData.CutsceneName = cutscene.ResourceName;
                cutsceneSaveDataDictionary.Add(cutscene.ResourceName, saveData);
            }
            return saveData;
        }

        public void LoadCutsceneData(CharacterSaveData characterSaveData) {
            cutsceneSaveDataDictionary.Clear();
            foreach (CutsceneSaveData cutsceneSaveData in characterSaveData.CutsceneSaveData) {
                if (cutsceneSaveData.CutsceneName != null && cutsceneSaveData.CutsceneName != string.Empty) {
                    cutsceneSaveDataDictionary.Add(cutsceneSaveData.CutsceneName, cutsceneSaveData);
                }
            }
        }

        public void SaveCutsceneData(CharacterSaveData characterSaveData) {
            //Debug.Log("Savemanager.SaveSceneNodeData()");

            characterSaveData.CutsceneSaveData.Clear();
            foreach (CutsceneSaveData cutsceneSaveData in cutsceneSaveDataDictionary.Values) {
                characterSaveData.CutsceneSaveData.Add(cutsceneSaveData);
            }
        }


    }

}