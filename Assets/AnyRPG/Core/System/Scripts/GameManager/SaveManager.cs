using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text.RegularExpressions;

namespace AnyRPG {
    public class SaveManager : ConfiguredMonoBehaviour {

        // game manager references
        private SystemEventManager systemEventManager = null;
        private PlayerManager playerManager = null;
        private MessageFeedManager messageFeedManager = null;
        private LevelManager levelManager = null;
        private QuestLog questLog = null;
        private ActionBarManager actionBarManager = null;
        private InventoryManager inventoryManager = null;
        private SystemItemManager systemItemManager = null;
        private SystemDataFactory systemDataFactory = null;
        private UIManager uIManager = null;
        private SystemAchievementManager systemAchievementManager = null;

        //private UMAData umaSaveData = null;
        private string recipeString = string.Empty;
        private string jsonSavePath = string.Empty;

        // prevent infinite loop loading list, and why would anyone need more than 1000 save games at this point
        private int maxSaveFiles = 1000;

        private string saveFileName = "AnyRPGPlayerSaveData";

        private AnyRPGSaveData currentSaveData = new AnyRPGSaveData();

        // data to turn into json for save
        private List<AnyRPGSaveData> anyRPGSaveDataList = new List<AnyRPGSaveData>();

        // keep track of mutable properties on scriptableObjects
        private Dictionary<string, QuestSaveData> questSaveDataDictionary = new Dictionary<string, QuestSaveData>();
        private Dictionary<string, BehaviorSaveData> behaviorSaveDataDictionary = new Dictionary<string, BehaviorSaveData>();
        private Dictionary<string, DialogSaveData> dialogSaveDataDictionary = new Dictionary<string, DialogSaveData>();
        private Dictionary<string, SceneNodeSaveData> sceneNodeSaveDataDictionary = new Dictionary<string, SceneNodeSaveData>();
        private Dictionary<string, CutsceneSaveData> cutsceneSaveDataDictionary = new Dictionary<string, CutsceneSaveData>();

        // [questName][objectiveType][objectiveName] : questObjectiveSavaData
        private Dictionary<string, Dictionary<string, Dictionary<string, QuestObjectiveSaveData>>> questObjectiveSaveDataDictionary = new Dictionary<string, Dictionary<string, Dictionary<string, QuestObjectiveSaveData>>>();

        public Dictionary<string, QuestSaveData> QuestSaveDataDictionary { get => questSaveDataDictionary; set => questSaveDataDictionary = value; }
        public Dictionary<string, BehaviorSaveData> BehaviorSaveDataDictionary { get => behaviorSaveDataDictionary; set => behaviorSaveDataDictionary = value; }
        public Dictionary<string, DialogSaveData> DialogSaveDataDictionary { get => dialogSaveDataDictionary; set => dialogSaveDataDictionary = value; }
        public Dictionary<string, SceneNodeSaveData> SceneNodeSaveDataDictionary { get => sceneNodeSaveDataDictionary; set => sceneNodeSaveDataDictionary = value; }
        public Dictionary<string, CutsceneSaveData> CutsceneSaveDataDictionary { get => cutsceneSaveDataDictionary; set => cutsceneSaveDataDictionary = value; }
        public Dictionary<string, Dictionary<string, Dictionary<string, QuestObjectiveSaveData>>> QuestObjectiveSaveDataDictionary { get => questObjectiveSaveDataDictionary; set => questObjectiveSaveDataDictionary = value; }
        public string RecipeString { get => recipeString; }

        protected bool eventSubscriptionsInitialized = false;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            CreateEventSubscriptions();
            GetSaveDataList();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemEventManager = systemGameManager.SystemEventManager;
            playerManager = systemGameManager.PlayerManager;
            levelManager = systemGameManager.LevelManager;
            questLog = systemGameManager.QuestLog;
            inventoryManager = systemGameManager.InventoryManager;
            systemItemManager = systemGameManager.SystemItemManager;
            systemDataFactory = systemGameManager.SystemDataFactory;
            systemAchievementManager = systemGameManager.SystemAchievementManager;
            uIManager = systemGameManager.UIManager;
            messageFeedManager = uIManager.MessageFeedManager;
            actionBarManager = uIManager.ActionBarManager;
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            systemEventManager.OnEquipmentChanged += SaveUMASettings;
            eventSubscriptionsInitialized = true;
        }

        // not currently called because this script is active the entire time the application is active
        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            systemEventManager.OnEquipmentChanged -= SaveUMASettings;
            eventSubscriptionsInitialized = false;
        }

        public List<AnyRPGSaveData> GetSaveDataList() {
            //Debug.Log("GetSaveDataList()");
            anyRPGSaveDataList.Clear();
            foreach (FileInfo fileInfo in GetSaveFileList()) {
                //Debug.Log("GetSaveDataList(): fileInfo.Name: " + fileInfo.Name);
                AnyRPGSaveData anyRPGSaveData = LoadSaveDataFromFile(Application.persistentDataPath + "/" + makeSaveDirectoryName() + "/" + fileInfo.Name);
                anyRPGSaveDataList.Add(anyRPGSaveData);
            }
            return anyRPGSaveDataList;
        }

        public AnyRPGSaveData LoadSaveDataFromFile(string fileName) {
            AnyRPGSaveData anyRPGSaveData = JsonUtility.FromJson<AnyRPGSaveData>(File.ReadAllText(fileName));

            // when loaded from file, overrides should always be true because the file may have been saved before these were added
            anyRPGSaveData.OverrideLocation = true;
            anyRPGSaveData.OverrideRotation = true;

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
                anyRPGSaveData.unitProfileName = systemConfigurationManager.CharacterCreatorUnitProfileName;
            }
            if (anyRPGSaveData.PlayerUMARecipe == null) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): Player UMA Recipe is null.  Setting to empty");
                anyRPGSaveData.PlayerUMARecipe = string.Empty;
            }
            if (anyRPGSaveData.CurrentScene == null) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): CurrentScene is null.  Setting to default");
                anyRPGSaveData.CurrentScene = systemConfigurationManager.DefaultStartingZone;
            }
            if (anyRPGSaveData.DataFileName == null || anyRPGSaveData.DataFileName == string.Empty) {
                anyRPGSaveData.DataFileName = Path.GetFileName(fileName);
            }
            anyRPGSaveData = InitializeResourceLists(anyRPGSaveData, false);

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

        public AnyRPGSaveData InitializeResourceLists(AnyRPGSaveData anyRPGSaveData, bool overWrite) {
            //Debug.Log("SaveManager.InitializeResourceLists()");


            // things that should only be overwritten if null because we read this data from other sources
            //if (anyRPGSaveData.questSaveData == null || overWrite) {
            if (anyRPGSaveData.questSaveData == null) {
                anyRPGSaveData.questSaveData = new List<QuestSaveData>();
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
            if (anyRPGSaveData.equippedBagSaveData == null || overWrite) {
                anyRPGSaveData.equippedBagSaveData = new List<EquippedBagSaveData>();
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


        public void SaveUMASettings(Equipment oldItem, Equipment newItem) {
            //Debug.Log("SaveManager.SaveUMASettings(Equipment, Equipement)");
            if ((oldItem?.UMARecipeProfile?.UMARecipes != null && oldItem.UMARecipeProfile.UMARecipes.Count > 0)
                || (newItem?.UMARecipeProfile?.UMARecipes != null && newItem.UMARecipeProfile.UMARecipes.Count > 0)) {
                SaveAppearanceSettings();
            }
        }


        public void SaveAppearanceSettings() {
            //Debug.Log("SaveManager.SaveAppearanceSettings()");
            if (playerManager.UnitController?.UnitModelController != null) {
                //Debug.Log("SaveManager.SaveUMASettings(): avatar exists");
                //if (recipeString == string.Empty) {
                    //Debug.Log("SaveManager.SaveUMASettings(): recipestring is empty");
                    playerManager.UnitController.UnitModelController.SaveAppearanceSettings();
                //}
            }
        }

        public void SaveRecipeString(string newRecipe) {
            //Debug.Log("SaveManager.SaveRecipeString(): " + newRecipe);
            recipeString = newRecipe;
        }

        public void ClearUMASettings() {
            //Debug.Log("SaveManager.ClearUMASettings()");
            recipeString = string.Empty;
        }

        /*
        public void LoadUMASettings(DynamicCharacterAvatar _dynamicCharacterAvatar, bool rebuild = true) {
            //Debug.Log("Savemanager.LoadUMASettings(" + _dynamicCharacterAvatar.gameObject.name + ", " + rebuild + ")");
            if (recipeString == string.Empty) {
                //Debug.Log("Savemanager.LoadUMASettings(): recipe string is empty. exiting!");
                return;
            }
            LoadUMASettings(recipeString, _dynamicCharacterAvatar, rebuild);
        }

        public void LoadUMASettings(string _recipeString, DynamicCharacterAvatar _dynamicCharacterAvatar, bool rebuild = true) {
            //Debug.Log("Savemanager.LoadUMASettings(string, " + _dynamicCharacterAvatar.gameObject.name + ", " + rebuild + ")");
            if (_recipeString == null || _recipeString == string.Empty || _dynamicCharacterAvatar == null) {
                //Debug.Log("Savemanager.LoadUMASettings(): _recipeString is empty. exiting!");
                return;
            }
            //Debug.Log("Savemanager.LoadUMASettings(): " + recipeString);
            //Debug.Log("Savemanager.LoadUMASettings(string, " + _dynamicCharacterAvatar.gameObject.name + ", " + rebuild + "): dynamicCharacterAvatar.SetLoadString()");
            _dynamicCharacterAvatar.SetLoadString(_recipeString);
            
            if (rebuild) {
                //Debug.Log("Savemanager.LoadUMASettings(string, " + _dynamicCharacterAvatar.gameObject.name + ", " + rebuild + "): dynamicCharacterAvatar.BuildCharacter()");
                _dynamicCharacterAvatar.BuildCharacter();
            }
        }

        */


        /*
        public void SaveUMASettings(UMAData umaData) {

        }
        */

        // save a game for the first time
        public bool SaveGame() {
            bool foundValidName = false;
            if (currentSaveData.DataFileName == null || currentSaveData.DataFileName == string.Empty) {
                //if (currentSaveData.Equals(default(AnyRPGSaveData))) {
                //Debug.Log("Savemanager.SaveGame(): Current save data is empty, creating new save file");
                string finalSaveFileName = GetNewSaveFileName();
                if (finalSaveFileName != string.Empty) {
                    foundValidName = true;
                }
                if (foundValidName) {
                    currentSaveData.DataFileName = finalSaveFileName;
                }
            } else {
                foundValidName = true;
            }
            if (foundValidName) {
                return SaveGame(currentSaveData);
            } else {
                Debug.Log("Too Many save files(" + maxSaveFiles + "), delete some");
                return false;
            }
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
                Collider[] hitColliders = Physics.OverlapCapsule(playerCollider.bounds.center + new Vector3(0, playerCollider.bounds.extents.y, 0),
                    playerCollider.bounds.center - new Vector3(0, playerCollider.bounds.extents.y, 0),
                    playerManager.ActiveUnitController.Collider.bounds.extents.x, validMask);
                foreach (Collider hitCollider in hitColliders) {
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

            anyRPGSaveData.PlayerLevel = playerManager.MyCharacter.CharacterStats.Level;
            anyRPGSaveData.currentExperience = playerManager.MyCharacter.CharacterStats.CurrentXP;
            anyRPGSaveData.playerName = playerManager.MyCharacter.CharacterName;
            if (playerManager.MyCharacter.Faction != null) {
                anyRPGSaveData.playerFaction = playerManager.MyCharacter.Faction.DisplayName;
            }
            if (playerManager.MyCharacter.CharacterRace != null) {
                anyRPGSaveData.characterRace = playerManager.MyCharacter.CharacterRace.DisplayName;
            }
            if (playerManager.MyCharacter.CharacterClass != null) {
                anyRPGSaveData.characterClass = playerManager.MyCharacter.CharacterClass.DisplayName;
            }
            if (playerManager.MyCharacter.ClassSpecialization != null) {
                anyRPGSaveData.classSpecialization = playerManager.MyCharacter.ClassSpecialization.DisplayName;
            }
            anyRPGSaveData.unitProfileName = playerManager.MyCharacter.UnitProfile.DisplayName;

            // moved to resource power data
            //anyRPGSaveData.currentHealth = playerManager.MyCharacter.CharacterStats.currentHealth;
            anyRPGSaveData.OverrideLocation = true;
            anyRPGSaveData.OverrideRotation = true;
            anyRPGSaveData.PlayerLocationX = playerManager.ActiveUnitController.transform.position.x;
            anyRPGSaveData.PlayerLocationY = playerManager.ActiveUnitController.transform.position.y;
            anyRPGSaveData.PlayerLocationZ = playerManager.ActiveUnitController.transform.position.z;
            anyRPGSaveData.PlayerRotationX = playerManager.ActiveUnitController.transform.forward.x;
            anyRPGSaveData.PlayerRotationY = playerManager.ActiveUnitController.transform.forward.y;
            anyRPGSaveData.PlayerRotationZ = playerManager.ActiveUnitController.transform.forward.z;
            //Debug.Log("Savemanager.SaveGame() rotation: " + anyRPGSaveData.PlayerRotationX + ", " + anyRPGSaveData.PlayerRotationY + ", " + anyRPGSaveData.PlayerRotationZ);
            SaveAppearanceSettings();
            anyRPGSaveData.PlayerUMARecipe = recipeString;
            anyRPGSaveData.CurrentScene = levelManager.ActiveSceneName;

            // shared code to setup resource lists on load of old version file or save of new one
            anyRPGSaveData = InitializeResourceLists(anyRPGSaveData, true);

            SaveResourcePowerData(anyRPGSaveData);

            SaveQuestData(anyRPGSaveData);

            SaveDialogData(anyRPGSaveData);
            SaveBehaviorData(anyRPGSaveData);
            SaveActionBarData(anyRPGSaveData);
            SaveInventorySlotData(anyRPGSaveData);
            SaveEquippedBagData(anyRPGSaveData);
            SaveAbilityData(anyRPGSaveData);
            SaveSkillData(anyRPGSaveData);
            SaveRecipeData(anyRPGSaveData);
            SaveReputationData(anyRPGSaveData);
            SaveEquipmentData(anyRPGSaveData);
            SaveCurrencyData(anyRPGSaveData);
            SaveSceneNodeData(anyRPGSaveData);
            SaveCutsceneData(anyRPGSaveData);
            SaveStatusEffectData(anyRPGSaveData);
            SavePetData(anyRPGSaveData);

            SaveWindowPositions();

            //Debug.Log("Savemanager.SaveQuestData(): size: " + anyRPGSaveData.questSaveData.Count);

            string saveDate = string.Empty;
            if (anyRPGSaveData.DataCreatedOn == null || anyRPGSaveData.DataCreatedOn == string.Empty) {
                anyRPGSaveData.DataCreatedOn = DateTime.Now.ToLongDateString();
            }
            SaveDataFile(anyRPGSaveData);

            PlayerPrefs.SetString("LastSaveDataFileName", anyRPGSaveData.DataFileName);

            return true;
        }

        public void SaveDataFile(AnyRPGSaveData dataToSave) {
            dataToSave.DataSavedOn = DateTime.Now.ToLongDateString();

            string jsonString = JsonUtility.ToJson(dataToSave);
            //Debug.Log(jsonString);
            string jsonSavePath = Application.persistentDataPath + "/" + makeSaveDirectoryName() + "/" + dataToSave.DataFileName;
            File.WriteAllText(jsonSavePath, jsonString);

        }

        public void SaveResourcePowerData(AnyRPGSaveData anyRPGSaveData) {
            foreach (PowerResource powerResource in playerManager.MyCharacter.CharacterStats.PowerResourceDictionary.Keys) {
                ResourcePowerSaveData resourcePowerData = new ResourcePowerSaveData();
                resourcePowerData.ResourceName = powerResource.DisplayName;
                resourcePowerData.amount = playerManager.MyCharacter.CharacterStats.PowerResourceDictionary[powerResource].currentValue;
                anyRPGSaveData.resourcePowerSaveData.Add(resourcePowerData);
            }
        }

        public void SetQuestSaveData(string questName, QuestSaveData questSaveData) {
            if (questSaveDataDictionary.ContainsKey(questName)) {
                questSaveDataDictionary[questName] = questSaveData;
            } else {
                questSaveDataDictionary.Add(questName, questSaveData);
            }
        }

        public QuestSaveData GetQuestSaveData(Quest quest) {
            QuestSaveData saveData;
            if (questSaveDataDictionary.ContainsKey(quest.DisplayName)) {
                saveData = questSaveDataDictionary[quest.DisplayName];
            } else {
                saveData = new QuestSaveData();
                saveData.QuestName = quest.DisplayName;
                questSaveDataDictionary.Add(quest.DisplayName, saveData);
            }
            return saveData;
        }

        public DialogSaveData GetDialogSaveData(Dialog dialog) {
            DialogSaveData saveData;
            if (dialogSaveDataDictionary.ContainsKey(dialog.DisplayName)) {
                saveData = dialogSaveDataDictionary[dialog.DisplayName];
            } else {
                saveData = new DialogSaveData();
                saveData.DialogName = dialog.DisplayName;
                dialogSaveDataDictionary.Add(dialog.DisplayName, saveData);
            }
            return saveData;
        }

        public SceneNodeSaveData GetSceneNodeSaveData(SceneNode sceneNode) {
            SceneNodeSaveData saveData;
            if (sceneNodeSaveDataDictionary.ContainsKey(sceneNode.DisplayName)) {
                saveData = sceneNodeSaveDataDictionary[sceneNode.DisplayName];
            } else {
                saveData = new SceneNodeSaveData();
                saveData.persistentObjects = new List<PersistentObjectSaveData>();
                saveData.SceneName = sceneNode.DisplayName;
                sceneNodeSaveDataDictionary.Add(sceneNode.DisplayName, saveData);
            }
            return saveData;
        }

        public CutsceneSaveData GetCutsceneSaveData(Cutscene cutscene) {
            CutsceneSaveData saveData;
            if (cutsceneSaveDataDictionary.ContainsKey(cutscene.DisplayName)) {
                //Debug.Log("Savemanager.GetCutsceneSaveData(): loading existing save data for: " + cutscene.DisplayName);
                saveData = cutsceneSaveDataDictionary[cutscene.DisplayName];
            } else {
                //Debug.Log("Savemanager.GetCutsceneSaveData(): generating new cutscene save data for: " + cutscene.DisplayName);
                saveData = new CutsceneSaveData();
                saveData.CutsceneName = cutscene.DisplayName;
                cutsceneSaveDataDictionary.Add(cutscene.DisplayName, saveData);
            }
            return saveData;
        }

        public BehaviorSaveData GetBehaviorSaveData(BehaviorProfile behaviorProfile) {
            BehaviorSaveData saveData;
            if (behaviorSaveDataDictionary.ContainsKey(behaviorProfile.DisplayName)) {
                saveData = behaviorSaveDataDictionary[behaviorProfile.DisplayName];
            } else {
                saveData = new BehaviorSaveData();
                saveData.BehaviorName = behaviorProfile.DisplayName;
                behaviorSaveDataDictionary.Add(behaviorProfile.DisplayName, saveData);
            }
            return saveData;
        }

        public void ResetQuestObjectiveSaveData(string questName) {
            questObjectiveSaveDataDictionary[questName] = new Dictionary<string, Dictionary<string, QuestObjectiveSaveData>>();
        }

        public QuestObjectiveSaveData GetQuestObjectiveSaveData(string questName, string objectiveType, string objectiveName) {
            QuestObjectiveSaveData saveData;

            // first, check if this quest is in the main objective dictionary.  If not, add it.
            Dictionary<string, Dictionary<string, QuestObjectiveSaveData>> questObjectiveSaveData;
            if (questObjectiveSaveDataDictionary.ContainsKey(questName)) {
                questObjectiveSaveData = questObjectiveSaveDataDictionary[questName];
            } else {
                questObjectiveSaveData = new Dictionary<string, Dictionary<string, QuestObjectiveSaveData>>();
                questObjectiveSaveDataDictionary.Add(questName, questObjectiveSaveData);
            }

            Dictionary<string, QuestObjectiveSaveData> questObjectiveSaveDataType;
            if (questObjectiveSaveData.ContainsKey(objectiveType)) {
                questObjectiveSaveDataType = questObjectiveSaveData[objectiveType];
            } else {
                questObjectiveSaveDataType = new Dictionary<string, QuestObjectiveSaveData>();
                questObjectiveSaveData.Add(objectiveType, questObjectiveSaveDataType);
            }

            if (questObjectiveSaveDataType.ContainsKey(objectiveName)) {
                saveData = questObjectiveSaveDataType[objectiveName];
            } else {
                saveData = new QuestObjectiveSaveData();
                saveData.ObjectiveName = objectiveName;
                saveData.ObjectiveType = objectiveType;
                questObjectiveSaveDataType.Add(objectiveName, saveData);
            }

            return saveData;
        }

        public void SaveQuestData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveQuestData()");

            anyRPGSaveData.questSaveData.Clear();
            foreach (QuestSaveData questSaveData in questSaveDataDictionary.Values) {
                QuestSaveData finalSaveData = questSaveData;
                if (questObjectiveSaveDataDictionary.ContainsKey(questSaveData.QuestName)) {
                    
                    List<QuestObjectiveSaveData> questObjectiveSaveDataList = new List<QuestObjectiveSaveData>();
                    foreach (string typeName in questObjectiveSaveDataDictionary[questSaveData.QuestName].Keys) {
                        foreach (QuestObjectiveSaveData saveData in questObjectiveSaveDataDictionary[questSaveData.QuestName][typeName].Values) {
                            questObjectiveSaveDataList.Add(saveData);
                        }

                    }

                    finalSaveData.questObjectives = questObjectiveSaveDataList;
                }
                finalSaveData.inLog = questLog.HasQuest(questSaveData.QuestName);
                anyRPGSaveData.questSaveData.Add(finalSaveData);
            }

        }
        
        public void SaveDialogData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveDialogData()");
            anyRPGSaveData.dialogSaveData.Clear();
            foreach (DialogSaveData dialogSaveData in dialogSaveDataDictionary.Values) {
                anyRPGSaveData.dialogSaveData.Add(dialogSaveData);
            }
        }
        
        public void SaveBehaviorData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveQuestData()");
            anyRPGSaveData.behaviorSaveData.Clear();
            foreach (BehaviorSaveData behaviorSaveData in behaviorSaveDataDictionary.Values) {
                anyRPGSaveData.behaviorSaveData.Add(behaviorSaveData);
            }
        }

        public void SaveSceneNodeData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveSceneNodeData()");

            anyRPGSaveData.sceneNodeSaveData.Clear();
            foreach (SceneNodeSaveData sceneNodeSaveData in sceneNodeSaveDataDictionary.Values) {
                anyRPGSaveData.sceneNodeSaveData.Add(sceneNodeSaveData);
            }
        }

        public void SaveCutsceneData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveSceneNodeData()");

            anyRPGSaveData.cutsceneSaveData.Clear();
            foreach (CutsceneSaveData cutsceneSaveData in cutsceneSaveDataDictionary.Values) {
                anyRPGSaveData.cutsceneSaveData.Add(cutsceneSaveData);
            }
        }

        public void SaveStatusEffectData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveSceneNodeData()");

            foreach (StatusEffectNode statusEffectNode in playerManager.MyCharacter.CharacterStats.StatusEffects.Values) {
                if (statusEffectNode.StatusEffect.ClassTrait == false
                    && statusEffectNode.AbilityEffectContext.AbilityCaster == (playerManager.MyCharacter as IAbilityCaster)) {
                    StatusEffectSaveData statusEffectSaveData = new StatusEffectSaveData();
                    statusEffectSaveData.StatusEffectName = statusEffectNode.StatusEffect.DisplayName;
                    statusEffectSaveData.remainingSeconds = (int)statusEffectNode.GetRemainingDuration();
                    anyRPGSaveData.statusEffectSaveData.Add(statusEffectSaveData);
                }
            }
        }

        public void SaveActionBarData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveActionBarData()");
            foreach (ActionButton actionButton in actionBarManager.GetMouseActionButtons()) {
                SaveActionButtonSaveData(actionButton, anyRPGSaveData.actionBarSaveData);
            }
            foreach (ActionButton actionButton in actionBarManager.GetGamepadActionButtons()) {
                SaveActionButtonSaveData(actionButton, anyRPGSaveData.gamepadActionBarSaveData);
            }
        }

        private void SaveActionButtonSaveData(ActionButton actionButton, List<ActionBarSaveData> actionBarSaveData) {
            ActionBarSaveData saveData = new ActionBarSaveData();
            saveData.DisplayName = (actionButton.Useable == null ? string.Empty : (actionButton.Useable as IDescribable).DisplayName);
            saveData.savedName = (actionButton.SavedUseable == null ? string.Empty : (actionButton.SavedUseable as IDescribable).DisplayName);
            saveData.isItem = (actionButton.Useable == null ? false : (actionButton.Useable is Item ? true : false));
            actionBarSaveData.Add(saveData);
        }

        public void SaveInventorySlotData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveInventorySlotData()");
            foreach (SlotScript slotScript in inventoryManager.GetSlots()) {
                InventorySlotSaveData saveData = new InventorySlotSaveData();
                saveData.ItemName = (slotScript.Item == null ? string.Empty : slotScript.Item.ResourceName);
                saveData.stackCount = (slotScript.Item == null ? 0 : slotScript.Count);
                saveData.DisplayName = (slotScript.Item == null ? string.Empty : slotScript.Item.DisplayName);
                if (slotScript.Item != null) {
                    if (slotScript.Item.ItemQuality != null) {
                        saveData.itemQuality = (slotScript.Item == null ? string.Empty : slotScript.Item.ItemQuality.ResourceName);
                    }
                    if ((slotScript.Item as Equipment is Equipment)) {
                        saveData.randomSecondaryStatIndexes = (slotScript.Item == null ? null : (slotScript.Item as Equipment).RandomStatIndexes);
                    }
                    saveData.dropLevel = (slotScript.Item == null ? 0 : slotScript.Item.DropLevel);
                }

                anyRPGSaveData.inventorySlotSaveData.Add(saveData);
            }
        }

        public void SaveReputationData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveReputationData()");
            foreach (FactionDisposition factionDisposition in playerManager.MyCharacter.CharacterFactionManager.DispositionDictionary) {
                if (factionDisposition == null) {
                    Debug.Log("Savemanager.SaveReputationData(): no disposition");
                    continue;
                }
                if (factionDisposition.Faction == null) {
                    Debug.Log("Savemanager.SaveReputationData() no faction");
                    continue;
                }
                ReputationSaveData saveData = new ReputationSaveData();
                saveData.ReputationName = factionDisposition.Faction.DisplayName;
                saveData.Amount = factionDisposition.disposition;
                anyRPGSaveData.reputationSaveData.Add(saveData);
            }
        }

        public void SaveCurrencyData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveCurrencyData()");
            foreach (CurrencyNode currencyNode in playerManager.MyCharacter.CharacterCurrencyManager.CurrencyList.Values) {
                CurrencySaveData currencySaveData = new CurrencySaveData();
                currencySaveData.Amount = currencyNode.Amount;
                currencySaveData.CurrencyName = currencyNode.currency.DisplayName;
                anyRPGSaveData.currencySaveData.Add(currencySaveData);
            }
        }

        public void SaveEquippedBagData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveEquippedBagData()");
            foreach (BagNode bagNode in inventoryManager.BagNodes) {
                //Debug.Log("Savemanager.SaveEquippedBagData(): got bagNode");
                EquippedBagSaveData saveData = new EquippedBagSaveData();
                saveData.BagName = (bagNode.Bag != null ? bagNode.Bag.DisplayName : string.Empty);
                saveData.slotCount = (bagNode.Bag != null ? bagNode.Bag.Slots : 0);
                saveData.isBankBag = bagNode.IsBankNode;
                anyRPGSaveData.equippedBagSaveData.Add(saveData);
            }
        }

        public void SaveAbilityData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveAbilityData()");
            foreach (BaseAbility baseAbility in playerManager.MyCharacter.CharacterAbilityManager.RawAbilityList.Values) {
                AbilitySaveData saveData = new AbilitySaveData();
                saveData.AbilityName = baseAbility.DisplayName;
                anyRPGSaveData.abilitySaveData.Add(saveData);
            }
        }

        public void SavePetData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveAbilityData()");
            foreach (UnitProfile unitProfile in playerManager.MyCharacter.CharacterPetManager.UnitProfiles) {
                PetSaveData saveData = new PetSaveData();
                saveData.PetName = unitProfile.DisplayName;
                anyRPGSaveData.petSaveData.Add(saveData);
            }
        }

        public void SaveEquipmentData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveEquipmentData()");
            if (playerManager != null && playerManager.MyCharacter != null && playerManager.MyCharacter.CharacterEquipmentManager != null) {
                foreach (Equipment equipment in playerManager.MyCharacter.CharacterEquipmentManager.CurrentEquipment.Values) {
                    EquipmentSaveData saveData = new EquipmentSaveData();
                    saveData.EquipmentName = (equipment == null ? string.Empty : equipment.ResourceName);
                    saveData.DisplayName = (equipment == null ? string.Empty : equipment.DisplayName);
                    if (equipment != null) {
                        if (equipment.ItemQuality != null) {
                            saveData.itemQuality = (equipment == null ? string.Empty : equipment.ItemQuality.ResourceName);
                        }
                        saveData.dropLevel = equipment.DropLevel;
                        saveData.randomSecondaryStatIndexes = (equipment == null ? null : equipment.RandomStatIndexes);
                    }
                    anyRPGSaveData.equipmentSaveData.Add(saveData);
                }
            }
        }

        public void SaveSkillData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveSkillData()");
            foreach (string skillName in playerManager.MyCharacter.CharacterSkillManager.MySkillList.Keys) {
                SkillSaveData saveData = new SkillSaveData();
                saveData.SkillName = skillName;
                anyRPGSaveData.skillSaveData.Add(saveData);
            }
        }

        public void SaveRecipeData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveRecipeData()");
            foreach (string recipeName in playerManager.MyCharacter.CharacterRecipeManager.RecipeList.Keys) {
                RecipeSaveData saveData = new RecipeSaveData();
                saveData.RecipeName = recipeName;
                anyRPGSaveData.recipeSaveData.Add(saveData);
            }
        }

        public void LoadResourcePowerData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadQuestData()");

            foreach (ResourcePowerSaveData resourcePowerSaveData in anyRPGSaveData.resourcePowerSaveData) {
                //Debug.Log("Savemanager.LoadQuestData(): loading questsavedata");
                playerManager.MyCharacter.CharacterStats.SetResourceAmount(resourcePowerSaveData.ResourceName, resourcePowerSaveData.amount);
            }

        }

        public void LoadQuestData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadQuestData()");
            questSaveDataDictionary.Clear();
            questObjectiveSaveDataDictionary.Clear();
            foreach (QuestSaveData questSaveData in anyRPGSaveData.questSaveData) {

                if (questSaveData.QuestName == null || questSaveData.QuestName == string.Empty) {
                    // don't load invalid quest data
                    continue;
                }
                questSaveDataDictionary.Add(questSaveData.QuestName, questSaveData);

                Dictionary<string, Dictionary<string, QuestObjectiveSaveData>> objectiveDictionary = new Dictionary<string, Dictionary<string, QuestObjectiveSaveData>>();

                // add objectives to dictionary
                foreach (QuestObjectiveSaveData questObjectiveSaveData in questSaveData.questObjectives) {
                    // perform null check to allow opening of older save files without null reference
                    if (questObjectiveSaveData.ObjectiveType != null && questObjectiveSaveData.ObjectiveType != string.Empty) {
                        if (!objectiveDictionary.ContainsKey(questObjectiveSaveData.ObjectiveType)) {
                            objectiveDictionary.Add(questObjectiveSaveData.ObjectiveType, new Dictionary<string, QuestObjectiveSaveData>());
                        }
                        objectiveDictionary[questObjectiveSaveData.ObjectiveType].Add(questObjectiveSaveData.ObjectiveName, questObjectiveSaveData);
                    }
                }

                questObjectiveSaveDataDictionary.Add(questSaveData.QuestName, objectiveDictionary);
            }

            foreach (QuestSaveData questSaveData in anyRPGSaveData.questSaveData) {
                //Debug.Log("Savemanager.LoadQuestData(): loading questsavedata");
                questLog.AcceptQuest(questSaveData);
            }
        }

        
        public void LoadDialogData(AnyRPGSaveData anyRPGSaveData) {
            dialogSaveDataDictionary.Clear();
            foreach (DialogSaveData dialogSaveData in anyRPGSaveData.dialogSaveData) {
                if (dialogSaveData.DialogName != null && dialogSaveData.DialogName != string.Empty) {
                    dialogSaveDataDictionary.Add(dialogSaveData.DialogName, dialogSaveData);
                }
            }
        }

        public void LoadBehaviorData(AnyRPGSaveData anyRPGSaveData) {
            behaviorSaveDataDictionary.Clear();
            foreach (BehaviorSaveData behaviorSaveData in anyRPGSaveData.behaviorSaveData) {
                if (behaviorSaveData.BehaviorName != null && behaviorSaveData.BehaviorName != string.Empty) {
                    behaviorSaveDataDictionary.Add(behaviorSaveData.BehaviorName, behaviorSaveData);
                }
            }
        }

        public void LoadSceneNodeData(AnyRPGSaveData anyRPGSaveData) {
            sceneNodeSaveDataDictionary.Clear();
            foreach (SceneNodeSaveData sceneNodeSaveData in anyRPGSaveData.sceneNodeSaveData) {
                if (sceneNodeSaveData.SceneName != null && sceneNodeSaveData.SceneName != string.Empty) {
                    sceneNodeSaveDataDictionary.Add(sceneNodeSaveData.SceneName, sceneNodeSaveData);
                }
            }
        }
        
        public void LoadCutsceneData(AnyRPGSaveData anyRPGSaveData) {
            cutsceneSaveDataDictionary.Clear();
            foreach (CutsceneSaveData cutsceneSaveData in anyRPGSaveData.cutsceneSaveData) {
                if (cutsceneSaveData.CutsceneName != null && cutsceneSaveData.CutsceneName != string.Empty) {
                    cutsceneSaveDataDictionary.Add(cutsceneSaveData.CutsceneName, cutsceneSaveData);
                }
            }
        }
        

        public void LoadStatusEffectData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadStatusEffectData()");
            foreach (StatusEffectSaveData statusEffectSaveData in anyRPGSaveData.statusEffectSaveData) {
                playerManager.MyCharacter.CharacterAbilityManager.ApplySavedStatusEffects(statusEffectSaveData);
            }
        }

        public void LoadEquippedBagData(AnyRPGSaveData anyRPGSaveData) {
            Debug.Log("Savemanager.LoadEquippedBagData()");
            inventoryManager.LoadEquippedBagData(anyRPGSaveData.equippedBagSaveData);
        }

        public void LoadInventorySlotData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadInventorySlotData()");
            int counter = 0;
            foreach (InventorySlotSaveData inventorySlotSaveData in anyRPGSaveData.inventorySlotSaveData) {
                if (inventorySlotSaveData.ItemName != string.Empty && inventorySlotSaveData.ItemName != null) {
                    for (int i = 0; i < inventorySlotSaveData.stackCount; i++) {
                        Item newItem = systemItemManager.GetNewResource(inventorySlotSaveData.ItemName);
                        if (newItem == null) {
                            Debug.Log("Savemanager.LoadInventorySlotData(): COULD NOT LOAD ITEM FROM ITEM MANAGER: " + inventorySlotSaveData.ItemName);
                        } else {
                            newItem.DisplayName = inventorySlotSaveData.DisplayName;
                            newItem.DropLevel = inventorySlotSaveData.dropLevel;
                            // disabled the if condition since all items can now have item quality overrides from vendor
                            //if (newItem.RandomItemQuality == true) {
                            if (inventorySlotSaveData.itemQuality != null && inventorySlotSaveData.itemQuality != string.Empty) {
                                newItem.ItemQuality = systemDataFactory.GetResource<ItemQuality>(inventorySlotSaveData.itemQuality);
                            }
                            //}
                            if ((newItem as Equipment) is Equipment) {
                                if (inventorySlotSaveData.randomSecondaryStatIndexes != null) {
                                    (newItem as Equipment).RandomStatIndexes = inventorySlotSaveData.randomSecondaryStatIndexes;
                                    (newItem as Equipment).InitializeRandomStatsFromIndex();
                                }
                            }

                            inventoryManager.AddItem(newItem, counter);
                        }
                    }
                }
                counter++;
            }
        }

        public void LoadEquipmentData(AnyRPGSaveData anyRPGSaveData, CharacterEquipmentManager characterEquipmentManager) {
            //Debug.Log("Savemanager.LoadEquipmentData()");
            foreach (EquipmentSaveData equipmentSaveData in anyRPGSaveData.equipmentSaveData) {
                if (equipmentSaveData.EquipmentName != string.Empty) {
                    Equipment newItem = (systemItemManager.GetNewResource(equipmentSaveData.EquipmentName) as Equipment);
                    if (newItem != null) {
                        newItem.DisplayName = equipmentSaveData.DisplayName;
                        newItem.DropLevel = equipmentSaveData.dropLevel;
                        if (equipmentSaveData.itemQuality != null && equipmentSaveData.itemQuality != string.Empty) {
                            newItem.ItemQuality = systemDataFactory.GetResource<ItemQuality>(equipmentSaveData.itemQuality);
                        }
                        if (equipmentSaveData.randomSecondaryStatIndexes != null) {
                            newItem.RandomStatIndexes = equipmentSaveData.randomSecondaryStatIndexes;
                            newItem.InitializeRandomStatsFromIndex();
                        }
                        if (characterEquipmentManager != null) {
                            characterEquipmentManager.Equip(newItem, null, false, false, false);
                        }
                    }
                }
            }
        }

        public void LoadReputationData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadReputationData()");
            //int counter = 0;
            foreach (ReputationSaveData reputationSaveData in anyRPGSaveData.reputationSaveData) {
                FactionDisposition factionDisposition = new FactionDisposition();
                factionDisposition.Faction = systemDataFactory.GetResource<Faction>(reputationSaveData.ReputationName);
                factionDisposition.disposition = reputationSaveData.Amount;
                playerManager.MyCharacter.CharacterFactionManager.LoadReputation(factionDisposition.Faction, (int)factionDisposition.disposition);
                //counter++;
            }
        }

        public void LoadCurrencyData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadCurrencyData()");
            foreach (CurrencySaveData currencySaveData in anyRPGSaveData.currencySaveData) {
                playerManager.MyCharacter.CharacterCurrencyManager.AddCurrency(systemDataFactory.GetResource<Currency>(currencySaveData.CurrencyName), currencySaveData.Amount);
            }
        }

        public void LoadAbilityData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadAbilityData()");

            foreach (AbilitySaveData abilitySaveData in anyRPGSaveData.abilitySaveData) {
                if (abilitySaveData.AbilityName != string.Empty) {
                    playerManager.MyCharacter.CharacterAbilityManager.LoadAbility(abilitySaveData.AbilityName);
                }
            }
        }

        public void LoadPetData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadAbilityData()");

            foreach (PetSaveData petSaveData in anyRPGSaveData.petSaveData) {
                if (petSaveData.PetName != string.Empty) {
                    playerManager.MyCharacter.CharacterPetManager.AddPet(petSaveData.PetName);
                }
            }

        }

        public void LoadSkillData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadSkillData()");
            foreach (SkillSaveData skillSaveData in anyRPGSaveData.skillSaveData) {
                playerManager.MyCharacter.CharacterSkillManager.LoadSkill(skillSaveData.SkillName);
            }
        }

        public void LoadRecipeData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadRecipeData()");
            foreach (RecipeSaveData recipeSaveData in anyRPGSaveData.recipeSaveData) {
                playerManager.MyCharacter.CharacterRecipeManager.LoadRecipe(recipeSaveData.RecipeName);
            }
        }

        public void LoadActionBarData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadActionBarData()");

            LoadActionButtonData(anyRPGSaveData.actionBarSaveData, actionBarManager.GetMouseActionButtons());
            LoadActionButtonData(anyRPGSaveData.gamepadActionBarSaveData, actionBarManager.GetGamepadActionButtons());
            actionBarManager.UpdateVisuals();
        }

        private void LoadActionButtonData(List<ActionBarSaveData> actionBarSaveDatas, List<ActionButton> actionButtons) {
            IUseable useable = null;
            int counter = 0;
            foreach (ActionBarSaveData actionBarSaveData in actionBarSaveDatas) {
                useable = null;
                if (actionBarSaveData.isItem == true) {
                    // find item in bag
                    //Debug.Log("Savemanager.LoadActionBarData(): searching for usable(" + actionBarSaveData.MyName + ") in inventory");
                    useable = systemDataFactory.GetResource<Item>(actionBarSaveData.DisplayName);
                } else {
                    // find ability from system ability manager
                    //Debug.Log("Savemanager.LoadActionBarData(): searching for usable in ability manager");
                    if (actionBarSaveData.DisplayName != null && actionBarSaveData.DisplayName != string.Empty) {
                        useable = systemDataFactory.GetResource<BaseAbility>(actionBarSaveData.DisplayName);
                    } else {
                        //Debug.Log("Savemanager.LoadActionBarData(): saved action bar had no name");
                    }
                    if (actionBarSaveData.savedName != null && actionBarSaveData.savedName != string.Empty) {
                        IUseable savedUseable = systemDataFactory.GetResource<BaseAbility>(actionBarSaveData.savedName);
                        if (savedUseable != null) {
                            actionButtons[counter].SavedUseable = savedUseable;
                        }
                    }
                }
                if (useable != null) {
                    actionButtons[counter].SetUseable(useable, false);
                } else {
                    //Debug.Log("Savemanager.LoadActionBarData(): no usable set on this actionbutton");
                    // testing remove things that weren't saved, it will prevent duplicate abilities if they are moved
                    // this means if new abilities are added to a class/etc between play sessions they won't be on the bars
                    actionButtons[counter].ClearUseable();
                }
                counter++;
            }
        }

        public void TryNewGame() {
            //Debug.Log("Savemanager.TryNewGame()");

            NewGame();
        }

        public void NewGame() {
            //Debug.Log("Savemanager.NewGame()");
            ClearSharedData();

            // do this so a new game doesn't reset window positions every time
            LoadWindowPositions();

            PerformInventorySetup();

            uIManager.loadGameWindow.CloseWindow();
            uIManager.newGameWindow.CloseWindow();

            // load default scene
            levelManager.LoadFirstScene();
        }

        public void PerformInventorySetup() {
            // initialize inventory so there is a place to put the inventory
            inventoryManager.PerformSetupActivities();

            inventoryManager.CreateDefaultBankBag();
            inventoryManager.CreateDefaultBackpack();
        }

        public void LoadGame() {
            //Debug.Log("SaveManager.LoadGame()");
            if (PlayerPrefs.HasKey("LastSaveDataFileName")) {
                //Debug.Log("SaveManager.LoadGame(): Last Save Data: " + PlayerPrefs.GetString("LastSaveDataFileName"));
                GetSaveDataList();
                foreach (AnyRPGSaveData anyRPGSaveData in anyRPGSaveDataList) {
                    if (anyRPGSaveData.DataFileName != null && anyRPGSaveData.DataFileName == PlayerPrefs.GetString("LastSaveDataFileName")) {
                        //Debug.Log("SaveManager.LoadGame(): Last Save Data: " + PlayerPrefs.GetString("LastSaveDataFileName") + " was found.  Loading Game...");
                        LoadGame(anyRPGSaveData);
                        return;
                    }
                }
            }
            NewGame();
        }

        // data needed by both the load window and in game play
        public void LoadRecipeString(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadUMARecipe()");

            // appearance
            string loadedRecipeString = anyRPGSaveData.PlayerUMARecipe;
            if (loadedRecipeString != null && loadedRecipeString != string.Empty) {
                //Debug.Log("Savemanager.LoadSharedData(): recipe string in save data was not empty or null, loading UMA settings");
                SaveRecipeString(loadedRecipeString);
                // we have UMA data so should load the UMA unit instead of the default
                //playerManager.SetUMAPrefab();
            } else {
                //Debug.Log("Savemanager.LoadSharedData(): recipe string in save data was empty or null, setting player prefab to default");
                ClearUMASettings();
                //playerManager.SetDefaultPrefab();
            }

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
            ClearUMASettings();

            ClearSystemManagedCharacterData();

            // added to prevent overwriting existing save file after going to main menu after saving game and starting new game
            currentSaveData = new AnyRPGSaveData();
            currentSaveData = InitializeResourceLists(currentSaveData, false);

        }


        public void LoadGame(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadGame()");
            ClearSharedData();
            currentSaveData = anyRPGSaveData;

            // initialize inventory so there is a place to put the inventory
            inventoryManager.PerformSetupActivities();

            // player level
            playerManager.InitialLevel = anyRPGSaveData.PlayerLevel;

            // scene and location
            Vector3 playerLocation = new Vector3(anyRPGSaveData.PlayerLocationX, anyRPGSaveData.PlayerLocationY, anyRPGSaveData.PlayerLocationZ);
            Vector3 playerRotation = new Vector3(anyRPGSaveData.PlayerRotationX, anyRPGSaveData.PlayerRotationY, anyRPGSaveData.PlayerRotationZ);
            //Debug.Log("Savemanager.LoadGame() rotation: " + anyRPGSaveData.PlayerRotationX + ", " + anyRPGSaveData.PlayerRotationY + ", " + anyRPGSaveData.PlayerRotationZ);

            // disable auto-accept achievements since we haven't loaded the data that tells us if they are complete yet
            systemAchievementManager.CleanupEventSubscriptions();

            // spawn player connection so all the data can be loaded
            playerManager.SpawnPlayerConnection();
            playerManager.MyCharacter.CharacterStats.CurrentXP = anyRPGSaveData.currentExperience;

            // testing: load this before setting providers so no duplicates on bars
            //LoadActionBarData(anyRPGSaveData);

            CapabilityConsumerSnapshot capabilityConsumerSnapshot = GetCapabilityConsumerSnapshot(anyRPGSaveData);

            playerManager.MyCharacter.ApplyCapabilityConsumerSnapshot(capabilityConsumerSnapshot);

            // this must be called after the snapshot is applied, because the unit profile could contain a default name
            playerManager.SetPlayerName(anyRPGSaveData.playerName);


            // THIS NEEDS TO BE DOWN HERE SO THE PLAYERSTATS EXISTS TO SUBSCRIBE TO THE EQUIP EVENTS AND INCREASE STATS
            LoadRecipeString(anyRPGSaveData);

            // complex data
            LoadEquippedBagData(anyRPGSaveData);
            LoadInventorySlotData(anyRPGSaveData);
            LoadAbilityData(anyRPGSaveData);


            // testing - move here to prevent learning auto-attack ability twice
            LoadEquipmentData(anyRPGSaveData, playerManager.MyCharacter.CharacterEquipmentManager);

            LoadSkillData(anyRPGSaveData);
            LoadRecipeData(anyRPGSaveData);
            LoadReputationData(anyRPGSaveData);

            LoadDialogData(anyRPGSaveData);
            LoadBehaviorData(anyRPGSaveData);
            LoadSceneNodeData(anyRPGSaveData);
            LoadCutsceneData(anyRPGSaveData);

            // quest data gets loaded last because it could rely on other data such as dialog completion status, which don't get saved because they are inferred
            LoadQuestData(anyRPGSaveData);

            // test loading this earlier to avoid having duplicates on bars
            LoadActionBarData(anyRPGSaveData);

            LoadCurrencyData(anyRPGSaveData);
            LoadStatusEffectData(anyRPGSaveData);
            LoadPetData(anyRPGSaveData);

            // now that we have loaded the quest data, we can re-enable references
            systemAchievementManager.CreateEventSubscriptions();

            // set resources last after equipment loaded for modifiers
            LoadResourcePowerData(anyRPGSaveData);

            LoadWindowPositions();

            uIManager.loadGameWindow.CloseWindow();

            // configure location and rotation overrides
            if (anyRPGSaveData.OverrideLocation == true) {
                levelManager.SetSpawnLocationOverride(playerLocation);
            }
            if (anyRPGSaveData.OverrideRotation == true) {
                levelManager.SetSpawnRotationOverride(playerRotation);
            }
            //levelManager.LoadLevel(anyRPGSaveData.CurrentScene, playerLocation, playerRotation);
            // load the proper level now that everything should be setup
            levelManager.LoadLevel(anyRPGSaveData.CurrentScene);
        }

        public void ClearSystemManagedCharacterData() {
            //Debug.Log("Savemanager.ClearSystemmanagedCharacterData()");

            inventoryManager.ClearData();

            actionBarManager.ClearActionBars(true);
            questLog.ClearLog();
            playerManager.ResetInitialLevel();

            // clear describable resource mutable data dictionaries
            questSaveDataDictionary.Clear();
            behaviorSaveDataDictionary.Clear();
            DialogSaveDataDictionary.Clear();
            sceneNodeSaveDataDictionary.Clear();
            cutsceneSaveDataDictionary.Clear();
            questObjectiveSaveDataDictionary.Clear();
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
            if (PlayerPrefs.HasKey("CharacterPanelWindowX") && PlayerPrefs.HasKey("CharacterPanelWindowY"))
                uIManager.characterPanelWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("CharacterPanelWindowX"), PlayerPrefs.GetFloat("CharacterPanelWindowY"), 0);
            if (PlayerPrefs.HasKey("LootWindowX") && PlayerPrefs.HasKey("LootWindowY"))
                uIManager.lootWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("LootWindowX"), PlayerPrefs.GetFloat("LootWindowY"), 0);
            if (PlayerPrefs.HasKey("VendorWindowX") && PlayerPrefs.HasKey("VendorWindowY"))
                uIManager.vendorWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("VendorWindowX"), PlayerPrefs.GetFloat("VendorWindowY"), 0);
            if (PlayerPrefs.HasKey("ChestWindowX") && PlayerPrefs.HasKey("ChestWindowY"))
                uIManager.chestWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("ChestWindowX"), PlayerPrefs.GetFloat("ChestWindowY"), 0);
            if (PlayerPrefs.HasKey("BankWindowX") && PlayerPrefs.HasKey("BankWindowY"))
                uIManager.bankWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("BankWindowX"), PlayerPrefs.GetFloat("BankWindowY"), 0);
            if (PlayerPrefs.HasKey("QuestLogWindowX") && PlayerPrefs.HasKey("QuestLogWindowY"))
                uIManager.questLogWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("QuestLogWindowX"), PlayerPrefs.GetFloat("QuestLogWindowY"), 0);
            if (PlayerPrefs.HasKey("AchievementListWindowX") && PlayerPrefs.HasKey("AchievementListWindowY"))
                uIManager.achievementListWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("AchievementListWindowX"), PlayerPrefs.GetFloat("AchievementListWindowY"), 0);
            if (PlayerPrefs.HasKey("QuestGiverWindowX") && PlayerPrefs.HasKey("QuestGiverWindowY"))
                uIManager.questGiverWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("QuestGiverWindowX"), PlayerPrefs.GetFloat("QuestGiverWindowY"), 0);
            if (PlayerPrefs.HasKey("SkillTrainerWindowX") && PlayerPrefs.HasKey("SkillTrainerWindowY"))
                uIManager.skillTrainerWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("SkillTrainerWindowX"), PlayerPrefs.GetFloat("SkillTrainerWindowY"), 0);
            if (PlayerPrefs.HasKey("InteractionWindowX") && PlayerPrefs.HasKey("InteractionWindowY"))
                uIManager.interactionWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("InteractionWindowX"), PlayerPrefs.GetFloat("InteractionWindowY"), 0);
            if (PlayerPrefs.HasKey("CraftingWindowX") && PlayerPrefs.HasKey("CraftingWindowY"))
                uIManager.craftingWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("CraftingWindowX"), PlayerPrefs.GetFloat("CraftingWindowY"), 0);
            if (PlayerPrefs.HasKey("MainMapWindowX") && PlayerPrefs.HasKey("MainMapWindowY"))
                uIManager.mainMapWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("MainMapWindowX"), PlayerPrefs.GetFloat("MainMapWindowY"), 0);
            if (PlayerPrefs.HasKey("QuestTrackerWindowX") && PlayerPrefs.HasKey("QuestTrackerWindowY"))
                uIManager.QuestTrackerWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("QuestTrackerWindowX"), PlayerPrefs.GetFloat("QuestTrackerWindowY"), 0);
            if (PlayerPrefs.HasKey("CombatLogWindowX") && PlayerPrefs.HasKey("CombatLogWindowY"))
                uIManager.CombatLogWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("CombatLogWindowX"), PlayerPrefs.GetFloat("CombatLogWindowY"), 0);

            if (PlayerPrefs.HasKey("MessageFeedManagerX") && PlayerPrefs.HasKey("MessageFeedManagerY"))
                messageFeedManager.MessageFeedWindow.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("MessageFeedManagerX"), PlayerPrefs.GetFloat("MessageFeedManagerY"), 0);

            if (PlayerPrefs.HasKey("FloatingCastBarControllerX") && PlayerPrefs.HasKey("FloatingCastBarControllerY")) {
                uIManager.FloatingCastBarController.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("FloatingCastBarControllerX"), PlayerPrefs.GetFloat("FloatingCastBarControllerY"), 0);
            }

            if (PlayerPrefs.HasKey("StatusEffectPanelControllerX") && PlayerPrefs.HasKey("StatusEffectPanelControllerY"))
                uIManager.StatusEffectPanelController.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("StatusEffectPanelControllerX"), PlayerPrefs.GetFloat("StatusEffectPanelControllerY"), 0);

            if (PlayerPrefs.HasKey("PlayerUnitFrameControllerX") && PlayerPrefs.HasKey("PlayerUnitFrameControllerY"))
                uIManager.PlayerUnitFrameController.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("PlayerUnitFrameControllerX"), PlayerPrefs.GetFloat("PlayerUnitFrameControllerY"), 0);

            if (PlayerPrefs.HasKey("FocusUnitFrameControllerX") && PlayerPrefs.HasKey("FocusUnitFrameControllerY"))
                uIManager.FocusUnitFrameController.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("FocusUnitFrameControllerX"), PlayerPrefs.GetFloat("FocusUnitFrameControllerY"), 0);

            if (PlayerPrefs.HasKey("MiniMapControllerX") && PlayerPrefs.HasKey("MiniMapControllerY"))
                uIManager.MiniMapController.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("MiniMapControllerX"), PlayerPrefs.GetFloat("MiniMapControllerY"), 0);

            if (PlayerPrefs.HasKey("XPBarControllerX") && PlayerPrefs.HasKey("XPBarControllerY"))
                uIManager.XPBarController.RectTransform.anchoredPosition = new Vector3(PlayerPrefs.GetFloat("XPBarControllerX"), PlayerPrefs.GetFloat("XPBarControllerY"), 0);

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
            PlayerPrefs.SetFloat("ChestWindowX", uIManager.chestWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("ChestWindowY", uIManager.chestWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("BankWindowX", uIManager.bankWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("BankWindowY", uIManager.bankWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("QuestLogWindowX", uIManager.questLogWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("QuestLogWindowY", uIManager.questLogWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("AchievementListWindowX", uIManager.achievementListWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("AchievementListWindowY", uIManager.achievementListWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("QuestGiverWindowX", uIManager.questGiverWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("QuestGiverWindowY", uIManager.questGiverWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("SkillTrainerWindowX", uIManager.skillTrainerWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("SkillTrainerWindowY", uIManager.skillTrainerWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("InteractionWindowX", uIManager.interactionWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("InteractionWindowY", uIManager.interactionWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("CraftingWindowX", uIManager.craftingWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("CraftingWindowY", uIManager.craftingWindow.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("MainMapWindowX", uIManager.mainMapWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("MainMapWindowY", uIManager.mainMapWindow.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("QuestTrackerWindowX", uIManager.QuestTrackerWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("QuestTrackerWindowY", uIManager.QuestTrackerWindow.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("CombatLogWindowX", uIManager.CombatLogWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("CombatLogWindowY", uIManager.CombatLogWindow.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("MessageFeedManagerX", messageFeedManager.MessageFeedWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("MessageFeedManagerY", messageFeedManager.MessageFeedWindow.RectTransform.anchoredPosition.y);

            //Debug.Log("Saving FloatingCastBarController: " + uIManager.MyFloatingCastBarController.RectTransform.anchoredPosition.x + "; " + uIManager.MyFloatingCastBarController.RectTransform.anchoredPosition.y);
            PlayerPrefs.SetFloat("FloatingCastBarControllerX", uIManager.FloatingCastBarController.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("FloatingCastBarControllerY", uIManager.FloatingCastBarController.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("StatusEffectPanelControllerX", uIManager.StatusEffectPanelController.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("StatusEffectPanelControllerY", uIManager.StatusEffectPanelController.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("PlayerUnitFrameControllerX", uIManager.PlayerUnitFrameController.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("PlayerUnitFrameControllerY", uIManager.PlayerUnitFrameController.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("FocusUnitFrameControllerX", uIManager.FocusUnitFrameController.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("FocusUnitFrameControllerY", uIManager.FocusUnitFrameController.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("MiniMapControllerX", uIManager.MiniMapController.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("MiniMapControllerY", uIManager.MiniMapController.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("XPBarControllerX", uIManager.XPBarController.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("XPBarControllerY", uIManager.XPBarController.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("BottomPanelX", uIManager.BottomPanel.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("BottomPanelY", uIManager.BottomPanel.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("SidePanelX", uIManager.SidePanel.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("SidePanelY", uIManager.SidePanel.RectTransform.anchoredPosition.y);

            PlayerPrefs.SetFloat("MouseOverWindowX", uIManager.MouseOverWindow.RectTransform.anchoredPosition.x);
            PlayerPrefs.SetFloat("MouseOverWindowY", uIManager.MouseOverWindow.RectTransform.anchoredPosition.y);

        }

        public void DeleteGame(AnyRPGSaveData anyRPGSaveData) {
            File.Delete(Application.persistentDataPath + "/" + makeSaveDirectoryName() + "/" + anyRPGSaveData.DataFileName);
            SystemEventManager.TriggerEvent("OnDeleteSaveData", new EventParamProperties());
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

    }

}