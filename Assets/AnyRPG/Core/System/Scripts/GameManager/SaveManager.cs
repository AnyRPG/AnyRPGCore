using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AnyRPG {
    public class SaveManager : ConfiguredClass {

        private Dictionary<string, CutsceneSaveData> cutsceneSaveDataDictionary = new Dictionary<string, CutsceneSaveData>();

        private string jsonSavePath = string.Empty;

        // prevent infinite loop loading list, and why would anyone need more than 1000 save games at this point
        private int maxSaveFiles = 1000;

        private string saveFileName = "AnyRPGPlayerSaveData";

        protected bool eventSubscriptionsInitialized = false;

        // game manager references
        private SystemEventManager systemEventManager = null;
        private PlayerManager playerManager = null;
        private PlayerManagerServer playerManagerServer = null;
        private MessageFeedManager messageFeedManager = null;
        private LevelManager levelManager = null;
        private AchievementLog achievementLog = null;
        private ActionBarManager actionBarManager = null;
        private SystemItemManager systemItemManager = null;
        private UIManager uIManager = null;
        private NewGameManager newGameManager = null;
        private NetworkManagerClient networkManagerClient = null;
        private LoadGameManager loadGameManager = null;

        public Dictionary<string, CutsceneSaveData> CutsceneSaveDataDictionary { get => cutsceneSaveDataDictionary; set => cutsceneSaveDataDictionary = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemEventManager = systemGameManager.SystemEventManager;
            playerManager = systemGameManager.PlayerManager;
            playerManagerServer = systemGameManager.PlayerManagerServer;
            levelManager = systemGameManager.LevelManager;
            achievementLog = systemGameManager.AchievementLog;
            systemItemManager = systemGameManager.SystemItemManager;
            uIManager = systemGameManager.UIManager;
            messageFeedManager = uIManager.MessageFeedManager;
            actionBarManager = uIManager.ActionBarManager;
            newGameManager = systemGameManager.NewGameManager;
            networkManagerClient = systemGameManager.NetworkManagerClient;
            loadGameManager = systemGameManager.LoadGameManager;
        }

        public List<PlayerCharacterSaveData> GetSaveDataList() {
            //Debug.Log("GetSaveDataList()");
            List<PlayerCharacterSaveData> saveDataList = new List<PlayerCharacterSaveData>();
            //anyRPGSaveDataList.Clear();
            foreach (FileInfo fileInfo in GetSaveFileList()) {
                //Debug.Log("GetSaveDataList(): fileInfo.Name: " + fileInfo.Name);
                AnyRPGSaveData anyRPGSaveData = LoadSaveDataFromFile(Application.persistentDataPath + "/" + makeSaveDirectoryName() + "/" + fileInfo.Name);
                saveDataList.Add(new PlayerCharacterSaveData() {
                    PlayerCharacterId = 0,
                    SaveData = anyRPGSaveData
                });
            }
            return saveDataList;
        }

        public AnyRPGSaveData LoadSaveDataFromFile(string fileName) {
            //Debug.Log($"SaveManager.LoadSaveDataFromFile({fileName})");

            string fileContents = File.ReadAllText(fileName);
            return LoadSaveDataFromString(fileContents);
        }

        public AnyRPGSaveData LoadSaveDataFromString(string fileContents) {
            //Debug.Log($"SaveManager.LoadSaveDataFromString({fileContents})");

            AnyRPGSaveData anyRPGSaveData = JsonUtility.FromJson<AnyRPGSaveData>(fileContents);

            // when loaded from file, overrides should always be true because the file may have been saved before these were added
            // disabled because new games create a save file, so if we create a new game, then quit, then load that save file,
            // it will not have the overrides set, and we don't want to override the location and rotation
            // AnyRPG v1.0 will not be backwards compatible with old save files.
            //anyRPGSaveData.OverrideLocation = true;
            //anyRPGSaveData.OverrideRotation = true;

            if (anyRPGSaveData.playerName == null) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): Player Name is null.  Setting to Unknown");
                anyRPGSaveData.playerName = "Unknown";
            }
            if (anyRPGSaveData.playerFaction == null) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): Player Faction is null.  Setting to default");
                anyRPGSaveData.playerFaction = string.Empty;
            }
            if (anyRPGSaveData.characterRace == null) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): Player Faction is null.  Setting to default");
                anyRPGSaveData.characterRace = string.Empty;
            }
            if (anyRPGSaveData.characterClass == null) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): Player Faction is null.  Setting to default");
                anyRPGSaveData.characterClass = string.Empty;
            }
            if (anyRPGSaveData.classSpecialization == null) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): Player Faction is null.  Setting to default");
                anyRPGSaveData.classSpecialization = string.Empty;
            }
            if (anyRPGSaveData.unitProfileName == null) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): Player Faction is null.  Setting to default");
                anyRPGSaveData.unitProfileName = systemConfigurationManager.DefaultUnitProfileName;
            }
            if (anyRPGSaveData.appearanceString == null) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): Player appearance string is null.  Setting to empty");
                anyRPGSaveData.appearanceString = string.Empty;
            }
            if (anyRPGSaveData.CurrentScene == null) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): CurrentScene is null.  Setting to default");
                anyRPGSaveData.CurrentScene = systemConfigurationManager.DefaultStartingZone;
            }
            /*
            if (anyRPGSaveData.DataFileName == null || anyRPGSaveData.DataFileName == string.Empty) {
                anyRPGSaveData.DataFileName = Path.GetFileName(fileName);
            }
            */
            anyRPGSaveData = InitializeSaveDataResourceLists(anyRPGSaveData, false);

            string saveDate = string.Empty;
            if (anyRPGSaveData.DataCreatedOn == null || anyRPGSaveData.DataCreatedOn == string.Empty) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): DataCreatedOn is null setting to now");
                anyRPGSaveData.DataCreatedOn = DateTime.Now.ToLongDateString();
            }
            if (anyRPGSaveData.DataSavedOn == null || anyRPGSaveData.DataSavedOn == string.Empty) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): DataSavedOn is null setting to now");
                anyRPGSaveData.DataSavedOn = DateTime.Now.ToLongDateString();
            }

            return anyRPGSaveData;
        }

        public AnyRPGSaveData InitializeSaveDataResourceLists(AnyRPGSaveData anyRPGSaveData, bool overWrite) {
            //Debug.Log("SaveManager.InitializeResourceLists()");


            // things that should only be overwritten if null because we read this data from other sources
            //if (anyRPGSaveData.questSaveData == null || overWrite) {
            if (anyRPGSaveData.questSaveData == null) {
                anyRPGSaveData.questSaveData = new List<QuestSaveData>();
            }
            if (anyRPGSaveData.achievementSaveData == null) {
                anyRPGSaveData.achievementSaveData = new List<QuestSaveData>();
            }
            if (anyRPGSaveData.dialogSaveData == null) {
                anyRPGSaveData.dialogSaveData = new List<DialogSaveData>();
            }
            if (anyRPGSaveData.behaviorSaveData == null) {
                anyRPGSaveData.behaviorSaveData = new List<BehaviorSaveData>();
            }
            if (anyRPGSaveData.sceneNodeSaveData == null) {
                anyRPGSaveData.sceneNodeSaveData = new List<SceneNodeSaveData>();
            }
            if (anyRPGSaveData.cutsceneSaveData == null) {
                anyRPGSaveData.cutsceneSaveData = new List<CutsceneSaveData>();
            }

            // things that are safe to overwrite any time because we get this data from other sources
            if (anyRPGSaveData.swappableMeshSaveData == null || overWrite) {
                anyRPGSaveData.swappableMeshSaveData = new List<SwappableMeshSaveData>();
            }
            if (anyRPGSaveData.resourcePowerSaveData == null || overWrite) {
                anyRPGSaveData.resourcePowerSaveData = new List<ResourcePowerSaveData>();
            }
            if (anyRPGSaveData.actionBarSaveData == null || overWrite) {
                anyRPGSaveData.actionBarSaveData = new List<ActionBarSaveData>();
            }
            if (anyRPGSaveData.gamepadActionBarSaveData == null || overWrite) {
                anyRPGSaveData.gamepadActionBarSaveData = new List<ActionBarSaveData>();
            }
            if (anyRPGSaveData.inventorySlotSaveData == null || overWrite) {
                anyRPGSaveData.inventorySlotSaveData = new List<InventorySlotSaveData>();
            }
            if (anyRPGSaveData.bankSlotSaveData == null || overWrite) {
                anyRPGSaveData.bankSlotSaveData = new List<InventorySlotSaveData>();
            }
            if (anyRPGSaveData.equippedBagSaveData == null || overWrite) {
                anyRPGSaveData.equippedBagSaveData = new List<EquippedBagSaveData>();
            }
            if (anyRPGSaveData.equippedBankBagSaveData == null || overWrite) {
                anyRPGSaveData.equippedBankBagSaveData = new List<EquippedBagSaveData>();
            }
            if (anyRPGSaveData.abilitySaveData == null || overWrite) {
                anyRPGSaveData.abilitySaveData = new List<AbilitySaveData>();
            }
            if (anyRPGSaveData.skillSaveData == null || overWrite) {
                anyRPGSaveData.skillSaveData = new List<SkillSaveData>();
            }
            if (anyRPGSaveData.recipeSaveData == null || overWrite) {
                anyRPGSaveData.recipeSaveData = new List<RecipeSaveData>();
            }
            if (anyRPGSaveData.reputationSaveData == null || overWrite) {
                anyRPGSaveData.reputationSaveData = new List<ReputationSaveData>();
            }
            if (anyRPGSaveData.equipmentSaveData == null || overWrite) {
                anyRPGSaveData.equipmentSaveData = new List<EquipmentSaveData>();
            }
            if (anyRPGSaveData.currencySaveData == null || overWrite) {
                anyRPGSaveData.currencySaveData = new List<CurrencySaveData>();
            }
            if (anyRPGSaveData.statusEffectSaveData == null || overWrite) {
                anyRPGSaveData.statusEffectSaveData = new List<StatusEffectSaveData>();
            }
            if (anyRPGSaveData.petSaveData == null || overWrite) {
                anyRPGSaveData.petSaveData = new List<PetSaveData>();
            }
            return anyRPGSaveData;
        }

        public List<FileInfo> GetSaveFileList() {
            List<FileInfo> returnList = new List<FileInfo>();
            DirectoryInfo directoryInfo = new DirectoryInfo(Application.persistentDataPath + "/" + makeSaveDirectoryName());

            foreach (FileInfo fileInfo in directoryInfo.GetFiles(saveFileName + "*.json")) {
                returnList.Add(fileInfo);
            }
            return returnList;
        }

        public string GetNewSaveFileName() {
            bool foundValidName = false;
            List<FileInfo> fileInfoList = GetSaveFileList();

            List<string> fileNameList = new List<string>();
            foreach (FileInfo fileInfo in fileInfoList) {
                fileNameList.Add(fileInfo.Name);
            }
            string finalSaveFileName = string.Empty;
            for (int i = 0; i < Mathf.Max(fileInfoList.Count, maxSaveFiles); i++) {
                finalSaveFileName = saveFileName + i + ".json";
                if (!fileNameList.Contains(finalSaveFileName)) {
                    foundValidName = true;
                    break;
                }
            }
            if (foundValidName) {
                return finalSaveFileName;
            }

            return string.Empty;
        }

        public bool SaveGame(AnyRPGSaveData anyRPGSaveData) {
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

            SaveCutsceneData(anyRPGSaveData);
            SaveItemIdCount(anyRPGSaveData);
            playerManager.UnitController.CharacterSaveManager.SaveGameData();

            SaveWindowPositions();

            //Debug.Log("Savemanager.SaveQuestData(): size: " + anyRPGSaveData.questSaveData.Count);

            bool saveResult = SaveDataFile(anyRPGSaveData);
            if (saveResult) {
                PlayerPrefs.SetString("LastSaveDataFileName", anyRPGSaveData.DataFileName);
            }

            return saveResult;
        }

        public void SaveItemIdCount(AnyRPGSaveData saveData) {
            saveData.ClientItemIdCount = systemItemManager.ClientItemIdCount;
        }

        public bool SaveDataFile(AnyRPGSaveData dataToSave) {

            bool foundValidName = false;
            if (dataToSave.DataFileName == null || dataToSave.DataFileName == string.Empty) {
                //Debug.Log("Savemanager.SaveGame(): Current save data is empty, creating new save file");
                string finalSaveFileName = GetNewSaveFileName();
                if (finalSaveFileName != string.Empty) {
                    foundValidName = true;
                }
                if (foundValidName) {
                    dataToSave.DataFileName = finalSaveFileName;
                }
            } else {
                foundValidName = true;
            }
            if (foundValidName == false) {
                Debug.Log("Too Many save files(" + maxSaveFiles + "), delete some");
                return false;
            }

            dataToSave.DataSavedOn = DateTime.Now.ToLongDateString();

            string jsonString = JsonUtility.ToJson(dataToSave);
            //Debug.Log(jsonString);
            string jsonSavePath = Application.persistentDataPath + "/" + makeSaveDirectoryName() + "/" + dataToSave.DataFileName;
            File.WriteAllText(jsonSavePath, jsonString);
            
            return true;
        }

       
    
        public void NewGame(PlayerCharacterSaveData playerCharacterSaveData) {
            //Debug.Log($"Savemanager.NewGame({playerCharacterSaveData.SaveData.unitProfileName})");

            ClearSystemManagedSaveData();


            if (systemGameManager.GameMode == GameMode.Local) {
                CreateLocalGame(playerCharacterSaveData);
            } else {
                CreateNetworkGame(playerCharacterSaveData);
            }

        }

        private void CreateNetworkGame(PlayerCharacterSaveData playerCharacterSaveData) {
            //Debug.Log("Savemanager.CreateNetworkGame(AnyRPGSaveData)");

            networkManagerClient.CreatePlayerCharacter(playerCharacterSaveData.SaveData);
        }

        private void CreateLocalGame(PlayerCharacterSaveData playerCharacterSaveData) {
            //Debug.Log($"Savemanager.CreateLocalGame({playerCharacterSaveData.SaveData.unitProfileName})");

            SaveDataFile(playerCharacterSaveData.SaveData);
            PlayerPrefs.SetString("LastSaveDataFileName", playerCharacterSaveData.SaveData.DataFileName);

            LoadGame(playerCharacterSaveData);
        }

        public void PerformInventorySetup(AnyRPGSaveData anyRPGSaveData) {
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
                EquippedBagSaveData saveData = new EquippedBagSaveData();
                saveData.BagName = bag.ResourceName;
                saveData.slotCount = bag.Slots;
                anyRPGSaveData.equippedBagSaveData.Add(saveData);
                bagCount++;

                // add inventory slots from bag
                for (int i = 0; i < bag.Slots; i++) {
                    anyRPGSaveData.inventorySlotSaveData.Add(GetEmptySlotSaveData());
                }
            }

            // add empty bag nodes
            for (int i = bagCount; i < systemConfigurationManager.MaxInventoryBags; i++) {
                anyRPGSaveData.equippedBagSaveData.Add(new EquippedBagSaveData());
            }

            // add empty bank nodes
            for (int i = bagCount; i < systemConfigurationManager.MaxBankBags; i++) {
                anyRPGSaveData.equippedBankBagSaveData.Add(new EquippedBagSaveData());
            }

            // add default bank contents
            for (int i = 0; i < systemConfigurationManager.DefaultBankSlots; i++) {
                if (systemConfigurationManager.DefaultBankContents.Count > i) {
                    InventorySlotSaveData inventorySlotSaveData = GetEmptySlotSaveData();
                    InstantiatedItem instantiatedItem = systemItemManager.GetNewInstantiatedItem(systemConfigurationManager.DefaultBankContents[i]);
                    if (instantiatedItem != null) {
                        //Debug.Log($"SaveManager.PerformInventorySetup(): adding default bank item: {instantiatedItem.ResourceName}");
                        inventorySlotSaveData = instantiatedItem.GetSlotSaveData();
                        inventorySlotSaveData.stackCount = 1;
                    }
                    anyRPGSaveData.bankSlotSaveData.Add(inventorySlotSaveData);
                }
            }

            if (networkManagerServer.ServerModeActive == false) {
                anyRPGSaveData.ClientItemIdCount = systemItemManager.ClientItemIdCount;
            }
        }

        public InventorySlotSaveData GetEmptySlotSaveData() {
            InventorySlotSaveData saveData = new InventorySlotSaveData();
            saveData.ItemName = string.Empty;
            saveData.DisplayName = string.Empty;
            saveData.itemQuality = string.Empty;
            saveData.dropLevel = 0;
            saveData.randomSecondaryStatIndexes = new List<int>();

            return saveData;
        }

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
            if (PlayerPrefs.HasKey("LastSaveDataFileName")) {
                //Debug.Log("SaveManager.LoadGame(): Last Save Data: " + PlayerPrefs.GetString("LastSaveDataFileName"));
                loadGameManager.LoadCharacterList();
                foreach (PlayerCharacterSaveData playerCharacterSaveData in loadGameManager.CharacterList) {
                    if (playerCharacterSaveData.SaveData.DataFileName != null && playerCharacterSaveData.SaveData.DataFileName == PlayerPrefs.GetString("LastSaveDataFileName")) {
                        //Debug.Log("SaveManager.LoadGame(): Last Save Data: " + PlayerPrefs.GetString("LastSaveDataFileName") + " was found.  Loading Game...");
                        LoadGame(playerCharacterSaveData);
                        return;
                    }
                }
            }
            newGameManager.NewGame();
        }

        public CapabilityConsumerSnapshot GetCapabilityConsumerSnapshot(AnyRPGSaveData saveData) {
            CapabilityConsumerSnapshot returnValue = new CapabilityConsumerSnapshot(systemGameManager);
            returnValue.UnitProfile = systemDataFactory.GetResource<UnitProfile>(saveData.unitProfileName);
            returnValue.CharacterRace = systemDataFactory.GetResource<CharacterRace>(saveData.characterRace);
            returnValue.CharacterClass = systemDataFactory.GetResource<CharacterClass>(saveData.characterClass);
            returnValue.ClassSpecialization = systemDataFactory.GetResource<ClassSpecialization>(saveData.classSpecialization);
            returnValue.Faction = systemDataFactory.GetResource<Faction>(saveData.playerFaction);
            return returnValue;
        }

        public void ClearSharedData() {
            //Debug.Log("SaveManager.ClearSharedData()");

            systemItemManager.ClientReset();
            ClearSystemManagedSaveData();
        }

        public PlayerCharacterSaveData CreateSaveData() {
            //Debug.Log("SaveManager.CreateSaveData()");

            AnyRPGSaveData newSaveData = new AnyRPGSaveData();
            newSaveData.playerName = systemConfigurationManager.DefaultPlayerName;
            newSaveData.PlayerLevel = 1;
            newSaveData.initializeResourceAmounts = true;
            newSaveData.CurrentScene = systemConfigurationManager.DefaultStartingZone;
            newSaveData.unitProfileName = systemConfigurationManager.DefaultUnitProfileName;
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

            newSaveData = InitializeSaveDataResourceLists(newSaveData, false);

            // initialize inventory
            PerformInventorySetup(newSaveData);

            return new PlayerCharacterSaveData() {
                PlayerCharacterId = 0,
                SaveData = newSaveData
            };
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

            // scene and location
            Vector3 playerLocation = new Vector3(playerCharacterSaveData.SaveData.PlayerLocationX, playerCharacterSaveData.SaveData.PlayerLocationY, playerCharacterSaveData.SaveData.PlayerLocationZ);
            Vector3 playerRotation = new Vector3(playerCharacterSaveData.SaveData.PlayerRotationX, playerCharacterSaveData.SaveData.PlayerRotationY, playerCharacterSaveData.SaveData.PlayerRotationZ);
            //Debug.Log("Savemanager.LoadGame() rotation: " + anyRPGSaveData.PlayerRotationX + ", " + anyRPGSaveData.PlayerRotationY + ", " + anyRPGSaveData.PlayerRotationZ);

            playerManager.SpawnPlayerConnection(playerCharacterSaveData);

            LoadCutsceneData(playerCharacterSaveData.SaveData);
            LoadItemIdData(playerCharacterSaveData.SaveData);

            LoadWindowPositions();

            CapabilityConsumerSnapshot capabilityConsumerSnapshot = GetCapabilityConsumerSnapshot(playerCharacterSaveData.SaveData);

            SpawnPlayerRequest loadSceneRequest = new SpawnPlayerRequest();
            // configure location and rotation overrides
            if (playerCharacterSaveData.SaveData.OverrideLocation == true) {
                loadSceneRequest.overrideSpawnLocation = true;
                loadSceneRequest.spawnLocation = playerLocation;
                //Debug.Log($"Savemanager.LoadGame() overrideSpawnLocation: {loadSceneRequest.overrideSpawnLocation} location: {loadSceneRequest.spawnLocation}");
            }
            if (playerCharacterSaveData.SaveData.OverrideRotation == true) {
                loadSceneRequest.overrideSpawnDirection = true;
                loadSceneRequest.spawnForwardDirection = playerRotation;
                //Debug.Log($"Savemanager.LoadGame() overrideRotation: {loadSceneRequest.overrideSpawnDirection} location: {loadSceneRequest.spawnForwardDirection}");
            }
            // debug print the location and rotation
            //Debug.Log($"Savemanager.LoadGame(): Spawning player at {loadSceneRequest.spawnLocation} with rotation {loadSceneRequest.spawnForwardDirection}");
            playerManagerServer.AddSpawnRequest(networkManagerClient.AccountId, loadSceneRequest);
            //levelManager.LoadLevel(anyRPGSaveData.CurrentScene, playerLocation, playerRotation);
            // load the proper level now that everything should be setup
            levelManager.LoadLevel(playerCharacterSaveData.SaveData.CurrentScene);
        }

        private void LoadItemIdData(AnyRPGSaveData saveData) {
            //Debug.Log("SaveManager.LoadItemIdData()");

            systemItemManager.SetClientItemIdCount(saveData.ClientItemIdCount);
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

            if (PlayerPrefs.HasKey("QuestTrackerWindowX") && PlayerPrefs.HasKey("QuestTrackerWindowY"))
                uIManager.QuestTrackerWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("QuestTrackerWindowX"), PlayerPrefs.GetFloat("QuestTrackerWindowY"), 0);
            if (PlayerPrefs.HasKey("CombatLogWindowX") && PlayerPrefs.HasKey("CombatLogWindowY"))
                uIManager.CombatLogWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("CombatLogWindowX"), PlayerPrefs.GetFloat("CombatLogWindowY"), 0);

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

            PlayerPrefs.SetFloat("QuestTrackerWindowX", uIManager.QuestTrackerWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("QuestTrackerWindowY", uIManager.QuestTrackerWindow.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("CombatLogWindowX", uIManager.CombatLogWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("CombatLogWindowY", uIManager.CombatLogWindow.RectTransform.anchoredPosition.y);

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

        public void DeleteGame(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.DeleteGame()");

            File.Delete(Application.persistentDataPath + "/" + makeSaveDirectoryName() + "/" + anyRPGSaveData.DataFileName);
            //SystemEventManager.TriggerEvent("OnDeleteSaveData", new EventParamProperties());
        }

        public void CopyGame(AnyRPGSaveData anyRPGSaveData) {
            string sourceFileName = Application.persistentDataPath + "/" + makeSaveDirectoryName() + "/" + anyRPGSaveData.DataFileName;
            string newSaveFileName = GetNewSaveFileName();
            if (newSaveFileName == string.Empty) {
                // there was an issue, don't try to copy
                return;
            }

            string destinationFileName = Application.persistentDataPath + "/" + makeSaveDirectoryName() + "/" + newSaveFileName;
            File.Copy(sourceFileName, destinationFileName);
            AnyRPGSaveData tmpSaveData = LoadSaveDataFromFile(destinationFileName);
            tmpSaveData.DataFileName = newSaveFileName;
            SaveDataFile(tmpSaveData);
        }

        public string makeSaveDirectoryName() {

            string replaceString = string.Empty;
            Regex regex = new Regex("[^a-zA-Z0-9]");
            if (systemConfigurationManager != null) {
                replaceString = regex.Replace(systemConfigurationManager.GameName, "");
            }

            if (replaceString != string.Empty) {
                if (!Directory.Exists(Application.persistentDataPath + "/" + replaceString)) {
                    Directory.CreateDirectory(Application.persistentDataPath + "/" + replaceString);
                }
            }
            return replaceString;
        }

        public List<PersistentObjectSaveData> GetPersistentObjects(SceneNode sceneNode) {
            if (playerManagerServer.PlayerCharacterMonitors.ContainsKey(0) == false) {
                Debug.Log($"SaveManager.GetPersistentObjects({sceneNode.ResourceName}): playerManager.UnitController is null.  Returning empty list.");
                return new List<PersistentObjectSaveData>();
            }
            return GetSceneNodeSaveData(sceneNode).persistentObjects;
        }

        public SceneNodeSaveData GetSceneNodeSaveData(SceneNode sceneNode) {
            AnyRPGSaveData anyRPGSaveData = playerManagerServer.PlayerCharacterMonitors[0].playerCharacterSaveData.SaveData;
            foreach (SceneNodeSaveData sceneNodeSaveData in anyRPGSaveData.sceneNodeSaveData) {
                if (sceneNodeSaveData.SceneName == sceneNode.ResourceName) {
                    return sceneNodeSaveData;
                }
            }
            SceneNodeSaveData saveData = new SceneNodeSaveData();
            saveData.persistentObjects = new List<PersistentObjectSaveData>();
            saveData.SceneName = sceneNode.ResourceName;
            return saveData;
        }

        public void SaveSceneNodeSaveData(SceneNodeSaveData sceneNodeSaveData) {
            //Debug.Log(DisplayName + ".SceneNode.SaveSceneNodeSaveData(" + sceneNodeSaveData.SceneName + ")");
            AnyRPGSaveData anyRPGSaveData = playerManagerServer.PlayerCharacterMonitors[0].playerCharacterSaveData.SaveData;
            foreach (SceneNodeSaveData _sceneNodeSaveData in anyRPGSaveData.sceneNodeSaveData) {
                if (_sceneNodeSaveData.SceneName == sceneNodeSaveData.SceneName) {
                    anyRPGSaveData.sceneNodeSaveData.Remove(_sceneNodeSaveData);
                    break;
                }
            }
            anyRPGSaveData.sceneNodeSaveData.Add(sceneNodeSaveData);
        }

        public void SavePersistentObject(string UUID, PersistentObjectSaveData persistentObjectSaveData, SceneNode sceneNode) {
            //Debug.Log(DisplayName + ".SceneNode.SavePersistentObject(" + UUID + ")");

            SceneNodeSaveData saveData = GetSceneNodeSaveData(sceneNode);
            foreach (PersistentObjectSaveData _persistentObjectSaveData in saveData.persistentObjects) {
                if (_persistentObjectSaveData.UUID == UUID) {
                    saveData.persistentObjects.Remove(_persistentObjectSaveData);
                    break;
                }
            }
            saveData.persistentObjects.Add(persistentObjectSaveData);
            SaveSceneNodeSaveData(saveData);
        }

        public PersistentObjectSaveData GetPersistentObject(string UUID, SceneNode sceneNode) {
            foreach (PersistentObjectSaveData _persistentObjectSaveData in GetSceneNodeSaveData(sceneNode).persistentObjects) {
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

        public void LoadCutsceneData(AnyRPGSaveData anyRPGSaveData) {
            cutsceneSaveDataDictionary.Clear();
            foreach (CutsceneSaveData cutsceneSaveData in anyRPGSaveData.cutsceneSaveData) {
                if (cutsceneSaveData.CutsceneName != null && cutsceneSaveData.CutsceneName != string.Empty) {
                    cutsceneSaveDataDictionary.Add(cutsceneSaveData.CutsceneName, cutsceneSaveData);
                }
            }
        }

        public void SaveCutsceneData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveSceneNodeData()");

            anyRPGSaveData.cutsceneSaveData.Clear();
            foreach (CutsceneSaveData cutsceneSaveData in cutsceneSaveDataDictionary.Values) {
                anyRPGSaveData.cutsceneSaveData.Add(cutsceneSaveData);
            }
        }


    }

}