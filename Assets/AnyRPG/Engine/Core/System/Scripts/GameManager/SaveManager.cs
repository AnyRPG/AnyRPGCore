using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;
using System.Text.RegularExpressions;

namespace AnyRPG {
    public class SaveManager : MonoBehaviour {

        #region Singleton
        private static SaveManager instance;

        public static SaveManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SaveManager>();
                }

                return instance;
            }
        }
        #endregion

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

        private Dictionary<string, Dictionary<Type, Dictionary<string, QuestObjectiveSaveData>>> questObjectiveSaveDataDictionary = new Dictionary<string, Dictionary<Type, Dictionary<string, QuestObjectiveSaveData>>>();

        public Dictionary<string, QuestSaveData> QuestSaveDataDictionary { get => questSaveDataDictionary; set => questSaveDataDictionary = value; }
        public Dictionary<string, BehaviorSaveData> BehaviorSaveDataDictionary { get => behaviorSaveDataDictionary; set => behaviorSaveDataDictionary = value; }
        public Dictionary<string, DialogSaveData> DialogSaveDataDictionary { get => dialogSaveDataDictionary; set => dialogSaveDataDictionary = value; }
        public Dictionary<string, SceneNodeSaveData> SceneNodeSaveDataDictionary { get => sceneNodeSaveDataDictionary; set => sceneNodeSaveDataDictionary = value; }
        public Dictionary<string, CutsceneSaveData> CutsceneSaveDataDictionary { get => cutsceneSaveDataDictionary; set => cutsceneSaveDataDictionary = value; }
        public Dictionary<string, Dictionary<Type, Dictionary<string, QuestObjectiveSaveData>>> QuestObjectiveSaveDataDictionary { get => questObjectiveSaveDataDictionary; set => questObjectiveSaveDataDictionary = value; }

        protected bool eventSubscriptionsInitialized = false;

        void Start() {
            //Debug.Log("Savemanager.Start()");
            CreateEventSubscriptions();
            GetSaveDataList();
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.MyInstance.OnEquipmentChanged += SaveUMASettings;
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.MyInstance.OnEquipmentChanged -= SaveUMASettings;
            eventSubscriptionsInitialized = false;
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            CleanupEventSubscriptions();
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
                anyRPGSaveData.unitProfileName = SystemConfigurationManager.MyInstance.CharacterCreatorUnitProfileName;
            }
            if (anyRPGSaveData.PlayerUMARecipe == null) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): Player UMA Recipe is null.  Setting to empty");
                anyRPGSaveData.PlayerUMARecipe = string.Empty;
            }
            if (anyRPGSaveData.CurrentScene == null) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): CurrentScene is null.  Setting to default");
                anyRPGSaveData.CurrentScene = SystemConfigurationManager.MyInstance.DefaultStartingZone;
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
            if ((oldItem != null && oldItem.MyUMARecipes.Count > 0) || (newItem != null && newItem.MyUMARecipes.Count > 0)) {
                SaveUMASettings();
            }
        }


        public void SaveUMASettings() {
            //Debug.Log("SaveManager.SaveUMASettings()");
            if (PlayerManager.MyInstance == null) {
                return;
            }
            if (PlayerManager.MyInstance.UnitController == null) {
                return;
            }
            if (PlayerManager.MyInstance.UnitController.DynamicCharacterAvatar != null) {
                //Debug.Log("SaveManager.SaveUMASettings(): avatar exists");
                if (recipeString == string.Empty) {
                    //Debug.Log("SaveManager.SaveUMASettings(): recipestring is empty");
                    recipeString = PlayerManager.MyInstance.UnitController.DynamicCharacterAvatar.GetCurrentRecipe();
                } else {
                    //Debug.Log("SaveManager.SaveUMASettings(): recipestring is not empty");
                    recipeString = PlayerManager.MyInstance.UnitController.DynamicCharacterAvatar.GetCurrentRecipe();
                }
            }
        }

        public void SaveUMASettings(string newRecipe) {
            //Debug.Log("SaveManager.SaveUMASettings(string)");
            recipeString = newRecipe;
        }

        public void ClearUMASettings() {
            //Debug.Log("SaveManager.ClearUMASettings()");
            recipeString = string.Empty;
        }

        public void LoadUMASettings(bool rebuild = true) {
            //Debug.Log("Savemanager.LoadUMASettings()");
            if (recipeString == string.Empty) {
                //Debug.Log("Savemanager.LoadUMASettings(): recipe string is empty. exiting!");
                return;
            }
            LoadUMASettings(recipeString, PlayerManager.MyInstance.UnitController.DynamicCharacterAvatar, rebuild);
        }

        public void LoadUMASettings(DynamicCharacterAvatar _dynamicCharacterAvatar, bool rebuild = true) {
            //Debug.Log("Savemanager.LoadUMASettings(DynamicCharacterAvatar)");
            if (recipeString == string.Empty) {
                //Debug.Log("Savemanager.LoadUMASettings(): recipe string is empty. exiting!");
                return;
            }
            LoadUMASettings(recipeString, _dynamicCharacterAvatar, rebuild);
        }


        public void LoadUMASettings(string _recipeString, DynamicCharacterAvatar _dynamicCharacterAvatar, bool rebuild = true) {
            //Debug.Log("Savemanager.LoadUMASettings(string, DynamicCharacterAvatar)");
            if (_recipeString == null || _recipeString == string.Empty || _dynamicCharacterAvatar == null) {
                //Debug.Log("Savemanager.LoadUMASettings(): _recipeString is empty. exiting!");
                return;
            }
            _dynamicCharacterAvatar.ClearSlots();
            //Debug.Log("Savemanager.LoadUMASettings(): " + recipeString);
            _dynamicCharacterAvatar.SetLoadString(_recipeString);
            //_dynamicCharacterAvatar.LoadFromRecipeString(_recipeString, DynamicCharacterAvatar.LoadOptions.);
            /*
            if (rebuild) {
                _dynamicCharacterAvatar.BuildCharacter();
            }
            */
        }

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
            if (PlayerManager.MyInstance.ActiveUnitController != null) {
                bool canSave = true;
                Collider playerCollider = PlayerManager.MyInstance.ActiveUnitController.Collider;
                int validMask = (1 << LayerMask.NameToLayer("Triggers") | 1 << LayerMask.NameToLayer("Interactable") | 1 << LayerMask.NameToLayer("Ignore Raycast"));
                Collider[] hitColliders = Physics.OverlapCapsule(playerCollider.bounds.center + new Vector3(0, playerCollider.bounds.extents.y, 0),
                    playerCollider.bounds.center - new Vector3(0, playerCollider.bounds.extents.y, 0),
                    PlayerManager.MyInstance.ActiveUnitController.Collider.bounds.extents.x, validMask);
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
                    MessageFeedManager.MyInstance.WriteMessage("You cannot save here");
                    return false;
                }
            }

            // do this first because persistent objects need to add their locations to the scene node before we write it to disk
            SystemEventManager.TriggerEvent("OnSaveGame", new EventParamProperties());

            anyRPGSaveData.PlayerLevel = PlayerManager.MyInstance.MyCharacter.CharacterStats.Level;
            anyRPGSaveData.currentExperience = PlayerManager.MyInstance.MyCharacter.CharacterStats.CurrentXP;
            anyRPGSaveData.playerName = PlayerManager.MyInstance.MyCharacter.CharacterName;
            if (PlayerManager.MyInstance.MyCharacter.Faction != null) {
                anyRPGSaveData.playerFaction = PlayerManager.MyInstance.MyCharacter.Faction.DisplayName;
            }
            if (PlayerManager.MyInstance.MyCharacter.CharacterRace != null) {
                anyRPGSaveData.characterRace = PlayerManager.MyInstance.MyCharacter.CharacterRace.DisplayName;
            }
            if (PlayerManager.MyInstance.MyCharacter.CharacterClass != null) {
                anyRPGSaveData.characterClass = PlayerManager.MyInstance.MyCharacter.CharacterClass.DisplayName;
            }
            if (PlayerManager.MyInstance.MyCharacter.ClassSpecialization != null) {
                anyRPGSaveData.classSpecialization = PlayerManager.MyInstance.MyCharacter.ClassSpecialization.DisplayName;
            }
            anyRPGSaveData.unitProfileName = PlayerManager.MyInstance.MyCharacter.UnitProfile.DisplayName;

            // moved to resource power data
            //anyRPGSaveData.currentHealth = PlayerManager.MyInstance.MyCharacter.CharacterStats.currentHealth;

            anyRPGSaveData.PlayerLocationX = PlayerManager.MyInstance.ActiveUnitController.transform.position.x;
            anyRPGSaveData.PlayerLocationY = PlayerManager.MyInstance.ActiveUnitController.transform.position.y;
            anyRPGSaveData.PlayerLocationZ = PlayerManager.MyInstance.ActiveUnitController.transform.position.z;
            anyRPGSaveData.PlayerRotationX = PlayerManager.MyInstance.ActiveUnitController.transform.forward.x;
            anyRPGSaveData.PlayerRotationY = PlayerManager.MyInstance.ActiveUnitController.transform.forward.y;
            anyRPGSaveData.PlayerRotationZ = PlayerManager.MyInstance.ActiveUnitController.transform.forward.z;
            //Debug.Log("Savemanager.SaveGame() rotation: " + anyRPGSaveData.PlayerRotationX + ", " + anyRPGSaveData.PlayerRotationY + ", " + anyRPGSaveData.PlayerRotationZ);
            anyRPGSaveData.PlayerUMARecipe = recipeString;
            anyRPGSaveData.CurrentScene = LevelManager.MyInstance.ActiveSceneName;

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
            //Debug.Log("Savemanager.SaveResourcePowerData()");
            foreach (PowerResource powerResource in PlayerManager.MyInstance.MyCharacter.CharacterStats.PowerResourceDictionary.Keys) {
                //Debug.Log("Savemanager.SaveQuestData(): Getting quest data from SystemQuestManager: " + quest.MyName);
                ResourcePowerSaveData resourcePowerData = new ResourcePowerSaveData();
                resourcePowerData.MyName = powerResource.DisplayName;
                resourcePowerData.amount = PlayerManager.MyInstance.MyCharacter.CharacterStats.PowerResourceDictionary[powerResource].currentValue;
                anyRPGSaveData.resourcePowerSaveData.Add(resourcePowerData);
                //Debug.Log("Savemanager.SaveQuestData(): " + questSaveData.MyName + ", turnedIn: " + questSaveData.turnedIn + ", inLog: " + questSaveData.inLog);
            }
            //Debug.Log("Savemanager.SaveQuestData(): size: " + anyRPGSaveData.questSaveData.Count);

        }

        public QuestSaveData GetQuestSaveData(Quest quest) {
            QuestSaveData saveData;
            if (questSaveDataDictionary.ContainsKey(quest.DisplayName)) {
                saveData = questSaveDataDictionary[quest.DisplayName];
            } else {
                saveData = new QuestSaveData();
                saveData.MyName = quest.DisplayName;
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
                saveData.MyName = dialog.DisplayName;
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
                saveData.MyName = sceneNode.DisplayName;
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
                saveData.MyName = cutscene.DisplayName;
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
                saveData.MyName = behaviorProfile.DisplayName;
                behaviorSaveDataDictionary.Add(behaviorProfile.DisplayName, saveData);
            }
            return saveData;
        }

        public QuestObjectiveSaveData GetQuestObjectiveSaveData(string questName, Type objectiveType, string objectiveName) {
            QuestObjectiveSaveData saveData;

            // first, check if this quest is in the main objective dictionary.  If not, add it.
            Dictionary<Type, Dictionary<string, QuestObjectiveSaveData>> questObjectiveSaveData;
            if (questObjectiveSaveDataDictionary.ContainsKey(questName)) {
                questObjectiveSaveData = questObjectiveSaveDataDictionary[questName];
            } else {
                questObjectiveSaveData = new Dictionary<Type, Dictionary<string, QuestObjectiveSaveData>>();
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
                saveData.MyName = objectiveName;
                questObjectiveSaveDataType.Add(objectiveName, saveData);
            }

            return saveData;
        }

        public void SaveQuestData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveQuestData()");

            anyRPGSaveData.questSaveData.Clear();
            foreach (QuestSaveData questSaveData in questSaveDataDictionary.Values) {
                //Debug.Log("Savemanager.SaveQuestData(): Getting quest data from SystemQuestManager: " + quest.MyName);

                QuestSaveData finalSaveData = questSaveData;

                if (questObjectiveSaveDataDictionary.ContainsKey(questSaveData.MyName)) {

                    // kill
                    List<QuestObjectiveSaveData> killObjectiveSaveDataList = new List<QuestObjectiveSaveData>();
                    if (questObjectiveSaveDataDictionary[questSaveData.MyName].ContainsKey(typeof(KillObjective))) {
                        foreach (QuestObjectiveSaveData saveData in questObjectiveSaveDataDictionary[questSaveData.MyName][typeof(KillObjective)].Values) {
                            killObjectiveSaveDataList.Add(saveData);
                        }
                    }

                    // use interactable
                    List<QuestObjectiveSaveData> useInteractableObjectiveSaveDataList = new List<QuestObjectiveSaveData>();
                    if (questObjectiveSaveDataDictionary[questSaveData.MyName].ContainsKey(typeof(UseInteractableObjective))) {
                        foreach (QuestObjectiveSaveData saveData in questObjectiveSaveDataDictionary[questSaveData.MyName][typeof(UseInteractableObjective)].Values) {
                            useInteractableObjectiveSaveDataList.Add(saveData);
                        }
                    }

                    // ability
                    List<QuestObjectiveSaveData> abilityObjectiveSaveDataList = new List<QuestObjectiveSaveData>();
                    if (questObjectiveSaveDataDictionary[questSaveData.MyName].ContainsKey(typeof(AbilityObjective))) {
                        foreach (QuestObjectiveSaveData saveData in questObjectiveSaveDataDictionary[questSaveData.MyName][typeof(AbilityObjective)].Values) {
                            abilityObjectiveSaveDataList.Add(saveData);
                        }
                    }

                    // collect
                    List<QuestObjectiveSaveData> collectObjectiveSaveDataList = new List<QuestObjectiveSaveData>();
                    if (questObjectiveSaveDataDictionary[questSaveData.MyName].ContainsKey(typeof(CollectObjective))) {
                        foreach (QuestObjectiveSaveData saveData in questObjectiveSaveDataDictionary[questSaveData.MyName][typeof(CollectObjective)].Values) {
                            collectObjectiveSaveDataList.Add(saveData);
                        }
                    }

                    // trade skill
                    List<QuestObjectiveSaveData> tradeSkillObjectiveSaveDataList = new List<QuestObjectiveSaveData>();
                    if (questObjectiveSaveDataDictionary[questSaveData.MyName].ContainsKey(typeof(TradeSkillObjective))) {
                        foreach (QuestObjectiveSaveData saveData in questObjectiveSaveDataDictionary[questSaveData.MyName][typeof(TradeSkillObjective)].Values) {
                            tradeSkillObjectiveSaveDataList.Add(saveData);
                        }
                    }

                    finalSaveData.killObjectives = killObjectiveSaveDataList;
                    finalSaveData.collectObjectives = collectObjectiveSaveDataList;
                    finalSaveData.useInteractableObjectives = useInteractableObjectiveSaveDataList;
                    finalSaveData.tradeSkillObjectives = tradeSkillObjectiveSaveDataList;
                    finalSaveData.abilityObjectives = abilityObjectiveSaveDataList;
                }
                finalSaveData.inLog = QuestLog.MyInstance.HasQuest(questSaveData.MyName);
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
            /*
            foreach (SceneNode sceneNode in SystemSceneNodeManager.MyInstance.GetResourceList()) {

                sceneNodeSaveData.persistentObjects = new List<PersistentObjectSaveData>();
                foreach (PersistentObjectSaveData persistentObjectSaveData in sceneNode.PersistentObjects.Values) {
                    sceneNodeSaveData.persistentObjects.Add(persistentObjectSaveData);
                }

                anyRPGSaveData.sceneNodeSaveData.Add(sceneNodeSaveData);
            }
            */
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

            foreach (StatusEffectNode statusEffectNode in PlayerManager.MyInstance.MyCharacter.CharacterStats.StatusEffects.Values) {
                if (statusEffectNode.StatusEffect.ClassTrait == false
                    && statusEffectNode.StatusEffect.SourceCharacter == (PlayerManager.MyInstance.MyCharacter as IAbilityCaster)) {
                    StatusEffectSaveData statusEffectSaveData = new StatusEffectSaveData();
                    statusEffectSaveData.MyName = statusEffectNode.StatusEffect.DisplayName;
                    statusEffectSaveData.remainingSeconds = (int)statusEffectNode.StatusEffect.GetRemainingDuration();
                    anyRPGSaveData.statusEffectSaveData.Add(statusEffectSaveData);
                }
            }
        }

        public void SaveActionBarData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveActionBarData()");
            foreach (ActionButton actionButton in UIManager.MyInstance.ActionBarManager.GetActionButtons()) {
                ActionBarSaveData saveData = new ActionBarSaveData();
                saveData.MyName = (actionButton.Useable == null ? string.Empty : (actionButton.Useable as IDescribable).DisplayName);
                saveData.savedName = (actionButton.SavedUseable == null ? string.Empty : (actionButton.SavedUseable as IDescribable).DisplayName);
                saveData.isItem = (actionButton.Useable == null ? false : (actionButton.Useable is Item ? true : false));
                //Debug.Log("Savemanager.SaveActionBarData(): saveData.MyName:" + saveData.MyName + "; saveData.isItem" + saveData.isItem);
                anyRPGSaveData.actionBarSaveData.Add(saveData);
            }
        }

        public void SaveInventorySlotData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveInventorySlotData()");
            foreach (SlotScript slotScript in InventoryManager.MyInstance.GetSlots()) {
                InventorySlotSaveData saveData = new InventorySlotSaveData();
                saveData.MyName = (slotScript.MyItem == null ? string.Empty : slotScript.MyItem.ResourceName);
                saveData.stackCount = (slotScript.MyItem == null ? 0 : slotScript.MyCount);
                saveData.DisplayName = (slotScript.MyItem == null ? string.Empty : slotScript.MyItem.DisplayName);
                if (slotScript.MyItem != null) {
                    if (slotScript.MyItem.ItemQuality != null) {
                        saveData.itemQuality = (slotScript.MyItem == null ? string.Empty : slotScript.MyItem.ItemQuality.ResourceName);
                    }
                    if ((slotScript.MyItem as Equipment is Equipment)) {
                        saveData.randomSecondaryStatIndexes = (slotScript.MyItem == null ? null : (slotScript.MyItem as Equipment).RandomStatIndexes);
                    }
                    saveData.dropLevel = (slotScript.MyItem == null ? 0 : slotScript.MyItem.DropLevel);
                }

                anyRPGSaveData.inventorySlotSaveData.Add(saveData);
            }
        }

        public void SaveReputationData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveReputationData()");
            foreach (FactionDisposition factionDisposition in PlayerManager.MyInstance.MyCharacter.CharacterFactionManager.DispositionDictionary) {
                if (factionDisposition == null) {
                    Debug.Log("Savemanager.SaveReputationData(): no disposition");
                    continue;
                }
                if (factionDisposition.Faction == null) {
                    Debug.Log("Savemanager.SaveReputationData() no faction");
                    continue;
                }
                ReputationSaveData saveData = new ReputationSaveData();
                saveData.MyName = factionDisposition.Faction.DisplayName;
                saveData.MyAmount = factionDisposition.disposition;
                anyRPGSaveData.reputationSaveData.Add(saveData);
            }
        }

        public void SaveCurrencyData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveCurrencyData()");
            foreach (CurrencyNode currencyNode in PlayerManager.MyInstance.MyCharacter.CharacterCurrencyManager.MyCurrencyList.Values) {
                CurrencySaveData currencySaveData = new CurrencySaveData();
                currencySaveData.MyAmount = currencyNode.MyAmount;
                currencySaveData.MyName = currencyNode.currency.DisplayName;
                anyRPGSaveData.currencySaveData.Add(currencySaveData);
            }
        }

        public void SaveEquippedBagData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveEquippedBagData()");
            foreach (BagNode bagNode in InventoryManager.MyInstance.MyBagNodes) {
                //Debug.Log("Savemanager.SaveEquippedBagData(): got bagNode");
                EquippedBagSaveData saveData = new EquippedBagSaveData();
                saveData.MyName = (bagNode.MyBag != null ? bagNode.MyBag.DisplayName : string.Empty);
                saveData.slotCount = (bagNode.MyBag != null ? bagNode.MyBag.MySlots : 0);
                saveData.isBankBag = bagNode.MyIsBankNode;
                anyRPGSaveData.equippedBagSaveData.Add(saveData);
            }
        }

        public void SaveAbilityData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveAbilityData()");
            foreach (BaseAbility baseAbility in PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.RawAbilityList.Values) {
                AbilitySaveData saveData = new AbilitySaveData();
                saveData.MyName = baseAbility.DisplayName;
                anyRPGSaveData.abilitySaveData.Add(saveData);
            }
        }

        public void SavePetData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveAbilityData()");
            foreach (UnitProfile unitProfile in PlayerManager.MyInstance.MyCharacter.CharacterPetManager.UnitProfiles) {
                PetSaveData saveData = new PetSaveData();
                saveData.MyName = unitProfile.DisplayName;
                anyRPGSaveData.petSaveData.Add(saveData);
            }
        }

        public void SaveEquipmentData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveEquipmentData()");
            if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.CharacterEquipmentManager != null) {
                foreach (Equipment equipment in PlayerManager.MyInstance.MyCharacter.CharacterEquipmentManager.CurrentEquipment.Values) {
                    EquipmentSaveData saveData = new EquipmentSaveData();
                    saveData.MyName = (equipment == null ? string.Empty : equipment.ResourceName);
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
            foreach (string skillName in PlayerManager.MyInstance.MyCharacter.CharacterSkillManager.MySkillList.Keys) {
                SkillSaveData saveData = new SkillSaveData();
                saveData.MyName = skillName;
                anyRPGSaveData.skillSaveData.Add(saveData);
            }
        }

        public void SaveRecipeData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveRecipeData()");
            foreach (string recipeName in PlayerManager.MyInstance.MyCharacter.CharacterRecipeManager.RecipeList.Keys) {
                RecipeSaveData saveData = new RecipeSaveData();
                saveData.MyName = recipeName;
                anyRPGSaveData.recipeSaveData.Add(saveData);
            }
        }

        public void LoadResourcePowerData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadQuestData()");

            foreach (ResourcePowerSaveData resourcePowerSaveData in anyRPGSaveData.resourcePowerSaveData) {
                //Debug.Log("Savemanager.LoadQuestData(): loading questsavedata");
                PlayerManager.MyInstance.MyCharacter.CharacterStats.SetResourceAmount(resourcePowerSaveData.MyName, resourcePowerSaveData.amount);
            }

        }

        public void LoadQuestData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadQuestData()");
            questSaveDataDictionary.Clear();
            questObjectiveSaveDataDictionary.Clear();
            foreach (QuestSaveData questSaveData in anyRPGSaveData.questSaveData) {
                questSaveDataDictionary.Add(questSaveData.MyName, questSaveData);

                // add kill objectives to dictionary
                Dictionary<string, QuestObjectiveSaveData> tmpDictionary = new Dictionary<string, QuestObjectiveSaveData>();
                Dictionary<Type, Dictionary<string, QuestObjectiveSaveData>> objectiveDictionary = new Dictionary<Type, Dictionary<string, QuestObjectiveSaveData>>();
                foreach (QuestObjectiveSaveData questObjectiveSaveData in questSaveData.killObjectives) {
                    tmpDictionary.Add(questObjectiveSaveData.MyName, questObjectiveSaveData);
                }
                objectiveDictionary.Add(typeof(KillObjective), tmpDictionary);

                // add collect objectives to dictionary
                tmpDictionary = new Dictionary<string, QuestObjectiveSaveData>();
                foreach (QuestObjectiveSaveData questObjectiveSaveData in questSaveData.collectObjectives) {
                    tmpDictionary.Add(questObjectiveSaveData.MyName, questObjectiveSaveData);
                }
                objectiveDictionary.Add(typeof(CollectObjective), tmpDictionary);

                // add use interactable objectives to dictionary
                tmpDictionary = new Dictionary<string, QuestObjectiveSaveData>();
                foreach (QuestObjectiveSaveData questObjectiveSaveData in questSaveData.useInteractableObjectives) {
                    tmpDictionary.Add(questObjectiveSaveData.MyName, questObjectiveSaveData);
                }
                objectiveDictionary.Add(typeof(UseInteractableObjective), tmpDictionary);

                // add tradeskill objectives to dictionary
                tmpDictionary = new Dictionary<string, QuestObjectiveSaveData>();
                foreach (QuestObjectiveSaveData questObjectiveSaveData in questSaveData.tradeSkillObjectives) {
                    tmpDictionary.Add(questObjectiveSaveData.MyName, questObjectiveSaveData);
                }
                objectiveDictionary.Add(typeof(TradeSkillObjective), tmpDictionary);

                // add ability objectives to dictionary
                tmpDictionary = new Dictionary<string, QuestObjectiveSaveData>();
                foreach (QuestObjectiveSaveData questObjectiveSaveData in questSaveData.abilityObjectives) {
                    tmpDictionary.Add(questObjectiveSaveData.MyName, questObjectiveSaveData);
                }
                objectiveDictionary.Add(typeof(AbilityObjective), tmpDictionary);
                questObjectiveSaveDataDictionary.Add(questSaveData.MyName, objectiveDictionary);
            }

            foreach (QuestSaveData questSaveData in anyRPGSaveData.questSaveData) {
                //Debug.Log("Savemanager.LoadQuestData(): loading questsavedata");
                QuestLog.MyInstance.AcceptQuest(questSaveData);
            }
        }

        
        public void LoadDialogData(AnyRPGSaveData anyRPGSaveData) {
            dialogSaveDataDictionary.Clear();
            foreach (DialogSaveData dialogSaveData in anyRPGSaveData.dialogSaveData) {
                dialogSaveDataDictionary.Add(dialogSaveData.MyName, dialogSaveData);
            }
        }

        public void LoadBehaviorData(AnyRPGSaveData anyRPGSaveData) {
            behaviorSaveDataDictionary.Clear();
            foreach (BehaviorSaveData behaviorSaveData in anyRPGSaveData.behaviorSaveData) {
                behaviorSaveDataDictionary.Add(behaviorSaveData.MyName, behaviorSaveData);
            }
        }

        public void LoadSceneNodeData(AnyRPGSaveData anyRPGSaveData) {
            sceneNodeSaveDataDictionary.Clear();
            foreach (SceneNodeSaveData sceneNodeSaveData in anyRPGSaveData.sceneNodeSaveData) {
                sceneNodeSaveDataDictionary.Add(sceneNodeSaveData.MyName, sceneNodeSaveData);
            }
        }
        
        public void LoadCutsceneData(AnyRPGSaveData anyRPGSaveData) {
            cutsceneSaveDataDictionary.Clear();
            foreach (CutsceneSaveData cutsceneSaveData in anyRPGSaveData.cutsceneSaveData) {
                cutsceneSaveDataDictionary.Add(cutsceneSaveData.MyName, cutsceneSaveData);
            }
        }
        

        public void LoadStatusEffectData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadStatusEffectData()");
            foreach (StatusEffectSaveData statusEffectSaveData in anyRPGSaveData.statusEffectSaveData) {
                //Debug.Log("Savemanager.LoadStatusEffectData(): applying " + statusEffectSaveData.MyName);
                PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.ApplySavedStatusEffects(statusEffectSaveData);
            }
        }

        public void LoadEquippedBagData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadEquippedBagData()");
            //foreach (EquippedBagSaveData saveData in anyRPGSaveData.equippedBagSaveData) {
            InventoryManager.MyInstance.LoadEquippedBagData(anyRPGSaveData.equippedBagSaveData);
            //}

        }

        public void LoadInventorySlotData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadInventorySlotData()");
            int counter = 0;
            foreach (InventorySlotSaveData inventorySlotSaveData in anyRPGSaveData.inventorySlotSaveData) {
                if (inventorySlotSaveData.MyName != string.Empty && inventorySlotSaveData.MyName != null) {
                    if (SystemItemManager.MyInstance != null) {
                        for (int i = 0; i < inventorySlotSaveData.stackCount; i++) {
                            Item newItem = SystemItemManager.MyInstance.GetNewResource(inventorySlotSaveData.MyName);
                            if (newItem == null) {
                                Debug.Log("Savemanager.LoadInventorySlotData(): COULD NOT LOAD ITEM FROM ITEM MANAGER: " + inventorySlotSaveData.MyName);
                            } else {
                                newItem.DisplayName = inventorySlotSaveData.DisplayName;
                                newItem.DropLevel = inventorySlotSaveData.dropLevel;
                                if (newItem.RandomItemQuality == true) {
                                    newItem.ItemQuality = SystemItemQualityManager.MyInstance.GetResource(inventorySlotSaveData.itemQuality);
                                }
                                if ((newItem as Equipment) is Equipment) {
                                    if (inventorySlotSaveData.randomSecondaryStatIndexes != null) {
                                        (newItem as Equipment).RandomStatIndexes = inventorySlotSaveData.randomSecondaryStatIndexes;
                                        (newItem as Equipment).InitializeRandomStatsFromIndex();
                                    }
                                }

                                InventoryManager.MyInstance.AddItem(newItem, counter);
                            }
                        }
                    } else {
                        //Debug.Log("SystemItemManager is null!");
                    }
                } else {
                    //Debug.Log("Savemanager.LoadInventorySlotData()");
                }
                counter++;
            }
        }

        public void LoadEquipmentData(AnyRPGSaveData anyRPGSaveData, CharacterEquipmentManager characterEquipmentManager) {
            //Debug.Log("Savemanager.LoadEquipmentData()");
            foreach (EquipmentSaveData equipmentSaveData in anyRPGSaveData.equipmentSaveData) {
                //Debug.Log("Savemanager.LoadEquipmentData(): checking equipment");
                if (equipmentSaveData.MyName != string.Empty) {
                    //Debug.Log("Savemanager.LoadEquipmentData(): checking equipment: using item: " + equipmentSaveData.MyName);
                    Equipment newItem = (SystemItemManager.MyInstance.GetNewResource(equipmentSaveData.MyName) as Equipment);
                    if (newItem != null) {
                        newItem.DisplayName = equipmentSaveData.DisplayName;
                        newItem.DropLevel = equipmentSaveData.dropLevel;
                        if (newItem.RandomItemQuality == true) {
                            newItem.ItemQuality = SystemItemQualityManager.MyInstance.GetResource(equipmentSaveData.itemQuality);
                        }
                        if (equipmentSaveData.randomSecondaryStatIndexes != null) {
                            newItem.RandomStatIndexes = equipmentSaveData.randomSecondaryStatIndexes;
                            newItem.InitializeRandomStatsFromIndex();
                        }
                        if (characterEquipmentManager != null) {
                            characterEquipmentManager.Equip(newItem);
                        } else {
                            //Debug.Log("Issue with equipment manager on player");
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
                factionDisposition.Faction = SystemFactionManager.MyInstance.GetResource(reputationSaveData.MyName);
                factionDisposition.disposition = reputationSaveData.MyAmount;
                PlayerManager.MyInstance.MyCharacter.CharacterFactionManager.AddReputation(factionDisposition.Faction, (int)factionDisposition.disposition, false);
                //counter++;
            }
        }

        public void LoadCurrencyData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadCurrencyData()");
            foreach (CurrencySaveData currencySaveData in anyRPGSaveData.currencySaveData) {
                PlayerManager.MyInstance.MyCharacter.CharacterCurrencyManager.AddCurrency(SystemCurrencyManager.MyInstance.GetResource(currencySaveData.MyName), currencySaveData.MyAmount);
            }
        }

        public void LoadAbilityData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadAbilityData()");

            foreach (AbilitySaveData abilitySaveData in anyRPGSaveData.abilitySaveData) {
                if (abilitySaveData.MyName != string.Empty) {
                    PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.LoadAbility(abilitySaveData.MyName);
                }
            }
        }

        public void LoadPetData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadAbilityData()");

            foreach (PetSaveData petSaveData in anyRPGSaveData.petSaveData) {
                if (petSaveData.MyName != string.Empty) {
                    PlayerManager.MyInstance.MyCharacter.CharacterPetManager.AddPet(petSaveData.MyName);
                }
            }

        }

        public void LoadSkillData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadSkillData()");
            foreach (SkillSaveData skillSaveData in anyRPGSaveData.skillSaveData) {
                PlayerManager.MyInstance.MyCharacter.CharacterSkillManager.LoadSkill(skillSaveData.MyName);
            }
        }

        public void LoadRecipeData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadRecipeData()");
            foreach (RecipeSaveData recipeSaveData in anyRPGSaveData.recipeSaveData) {
                PlayerManager.MyInstance.MyCharacter.CharacterRecipeManager.LoadRecipe(recipeSaveData.MyName);
            }
        }

        public void LoadActionBarData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadActionBarData()");

            IUseable useable = null;
            int counter = 0;
            foreach (ActionBarSaveData actionBarSaveData in anyRPGSaveData.actionBarSaveData) {
                useable = null;
                if (actionBarSaveData.isItem == true) {
                    // find item in bag
                    //Debug.Log("Savemanager.LoadActionBarData(): searching for usable(" + actionBarSaveData.MyName + ") in inventory");
                    /*
                    List<Item> itemList = InventoryManager.MyInstance.GetItems(actionBarSaveData.MyName, 1);
                    if (itemList.Count > 0) {
                        //Debug.Log("Savemanager.LoadActionBarData(): searching for usable(" + actionBarSaveData.MyName + ") in inventory and itemlist.count was: " + itemList.Count);
                        useable = itemList[0] as IUseable;
                    } else {
                        //Debug.Log("Savemanager.LoadActionBarData(): searching for usable(" + actionBarSaveData.MyName + ") in inventory and itemlist.count was: " + itemList.Count);
                    }
                    */
                    useable = SystemItemManager.MyInstance.GetResource(actionBarSaveData.MyName);
                } else {
                    // find ability from system ability manager
                    //Debug.Log("Savemanager.LoadActionBarData(): searching for usable in ability manager");
                    if (actionBarSaveData.MyName != null && actionBarSaveData.MyName != string.Empty) {
                        useable = SystemAbilityManager.MyInstance.GetResource(actionBarSaveData.MyName);
                    } else {
                        //Debug.Log("Savemanager.LoadActionBarData(): saved action bar had no name");
                    }
                    if (actionBarSaveData.savedName != null && actionBarSaveData.savedName != string.Empty) {
                        IUseable savedUseable = SystemAbilityManager.MyInstance.GetResource(actionBarSaveData.savedName);
                        if (savedUseable != null) {
                            UIManager.MyInstance.ActionBarManager.GetActionButtons()[counter].SavedUseable = savedUseable;
                        }
                    }
                }
                if (useable != null) {
                    //Debug.Log("Savemanager.LoadActionBarData(): setting useable on button: " + counter + "; actionbutton: " + UIManager.MyInstance.MyActionBarManager.GetActionButtons()[counter].name + UIManager.MyInstance.MyActionBarManager.GetActionButtons()[counter].GetInstanceID());
                    UIManager.MyInstance.ActionBarManager.GetActionButtons()[counter].SetUseable(useable, false);
                } else {
                    //Debug.Log("Savemanager.LoadActionBarData(): no usable set on this actionbutton");
                    // testing remove things that weren't saved, it will prevent duplicate abilities if they are moved
                    // this means if new abilities are added to a class/etc between play sessions they won't be on the bars
                    UIManager.MyInstance.ActionBarManager.GetActionButtons()[counter].ClearUseable();
                }
                counter++;
            }
            UIManager.MyInstance.ActionBarManager.UpdateVisuals();
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

            SystemWindowManager.MyInstance.loadGameWindow.CloseWindow();
            SystemWindowManager.MyInstance.newGameWindow.CloseWindow();

            // load default scene
            LevelManager.MyInstance.LoadFirstScene();
        }

        public void PerformInventorySetup() {
            // initialize inventory so there is a place to put the inventory
            InventoryManager.MyInstance.PerformSetupActivities();

            InventoryManager.MyInstance.CreateDefaultBankBag();
            InventoryManager.MyInstance.CreateDefaultBackpack();
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
        public void LoadUMARecipe(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadSharedData()");

            // appearance
            string loadedRecipeString = anyRPGSaveData.PlayerUMARecipe;
            if (loadedRecipeString != null && loadedRecipeString != string.Empty) {
                //Debug.Log("Savemanager.LoadSharedData(): recipe string in save data was not empty or null, loading UMA settings");
                SaveUMASettings(loadedRecipeString);
                // we have UMA data so should load the UMA unit instead of the default
                //PlayerManager.MyInstance.SetUMAPrefab();
            } else {
                //Debug.Log("Savemanager.LoadSharedData(): recipe string in save data was empty or null, setting player prefab to default");
                ClearUMASettings();
                //PlayerManager.MyInstance.SetDefaultPrefab();
            }

        }

        public CapabilityConsumerSnapshot GetCapabilityConsumerSnapshot(AnyRPGSaveData saveData) {
            CapabilityConsumerSnapshot returnValue = new CapabilityConsumerSnapshot();
            returnValue.UnitProfile = UnitProfile.GetUnitProfileReference(saveData.unitProfileName);
            returnValue.CharacterRace = SystemCharacterRaceManager.MyInstance.GetResource(saveData.characterRace);
            returnValue.CharacterClass = SystemCharacterClassManager.MyInstance.GetResource(saveData.characterClass);
            returnValue.ClassSpecialization = SystemClassSpecializationManager.MyInstance.GetResource(saveData.classSpecialization);
            returnValue.Faction = SystemFactionManager.MyInstance.GetResource(saveData.playerFaction);
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
            InventoryManager.MyInstance.PerformSetupActivities();

            // player level
            PlayerManager.MyInstance.InitialLevel = anyRPGSaveData.PlayerLevel;

            // scene and location
            Vector3 playerLocation = new Vector3(anyRPGSaveData.PlayerLocationX, anyRPGSaveData.PlayerLocationY, anyRPGSaveData.PlayerLocationZ);
            Vector3 playerRotation = new Vector3(anyRPGSaveData.PlayerRotationX, anyRPGSaveData.PlayerRotationY, anyRPGSaveData.PlayerRotationZ);
            //Debug.Log("Savemanager.LoadGame() rotation: " + anyRPGSaveData.PlayerRotationX + ", " + anyRPGSaveData.PlayerRotationY + ", " + anyRPGSaveData.PlayerRotationZ);

            // disable auto-accept achievements since we haven't loaded the data that tells us if they are complete yet
            SystemQuestManager.MyInstance.CleanupEventSubscriptions();

            // spawn player connection so all the data can be loaded
            PlayerManager.MyInstance.SpawnPlayerConnection();
            PlayerManager.MyInstance.MyCharacter.CharacterStats.CurrentXP = anyRPGSaveData.currentExperience;

            // testing: load this before setting providers so no duplicates on bars
            //LoadActionBarData(anyRPGSaveData);

            CapabilityConsumerSnapshot capabilityConsumerSnapshot = GetCapabilityConsumerSnapshot(anyRPGSaveData);

            PlayerManager.MyInstance.MyCharacter.ApplyCapabilityConsumerSnapshot(capabilityConsumerSnapshot);

            // this must be called after the snapshot is applied, because the unit profile could contain a default name
            PlayerManager.MyInstance.SetPlayerName(anyRPGSaveData.playerName);


            // THIS NEEDS TO BE DOWN HERE SO THE PLAYERSTATS EXISTS TO SUBSCRIBE TO THE EQUIP EVENTS AND INCREASE STATS
            LoadUMARecipe(anyRPGSaveData);

            // complex data
            LoadEquippedBagData(anyRPGSaveData);
            LoadInventorySlotData(anyRPGSaveData);
            LoadAbilityData(anyRPGSaveData);


            // testing - move here to prevent learning auto-attack ability twice
            LoadEquipmentData(anyRPGSaveData, PlayerManager.MyInstance.MyCharacter.CharacterEquipmentManager);

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
            SystemQuestManager.MyInstance.CreateEventSubscriptions();

            // set resources last after equipment loaded for modifiers
            LoadResourcePowerData(anyRPGSaveData);

            LoadWindowPositions();

            SystemWindowManager.MyInstance.loadGameWindow.CloseWindow();
            // load the proper level now that everything should be setup
            LevelManager.MyInstance.LoadLevel(anyRPGSaveData.CurrentScene, playerLocation, playerRotation);
        }

        public void ClearSystemManagedCharacterData() {
            //Debug.Log("Savemanager.ClearSystemmanagedCharacterData()");

            // not needed anymore because no singleton?
            //CharacterEquipmentManager.MyInstance.ClearEquipment();

            InventoryManager.MyInstance.ClearData();
            if (PopupWindowManager.MyInstance != null) {
                if (PopupWindowManager.MyInstance.bankWindow != null) {
                    if (PopupWindowManager.MyInstance.bankWindow.CloseableWindowContents != null) {
                        (PopupWindowManager.MyInstance.bankWindow.CloseableWindowContents as BankPanel).ClearSlots();
                        (PopupWindowManager.MyInstance.bankWindow.CloseableWindowContents as BankPanel).MyBagBarController.ClearBagButtons();
                    } else {
                        //Debug.Log("windowcontents was null");
                    }
                } else {
                    //Debug.Log("bankwindow was null");
                }
            } else {
                //Debug.Log("popupwindowmanager was was null");
            }

            //SystemGameManager.MyInstance.ReloadResourceLists();

            UIManager.MyInstance.ActionBarManager.ClearActionBars(true);
            QuestLog.MyInstance.ClearLog();
            PlayerManager.MyInstance.ResetInitialLevel();

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
                PopupWindowManager.MyInstance.abilityBookWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("AbilityBookWindowX"), PlayerPrefs.GetFloat("AbilityBookWindowY"), 0);
            if (PlayerPrefs.HasKey("SkillBookWindowX") && PlayerPrefs.HasKey("SkillBookWindowY"))
                PopupWindowManager.MyInstance.skillBookWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("SkillBookWindowX"), PlayerPrefs.GetFloat("SkillBookWindowY"), 0);
            if (PlayerPrefs.HasKey("ReputationBookWindowX") && PlayerPrefs.HasKey("ReputationBookWindowY"))
                PopupWindowManager.MyInstance.reputationBookWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("ReputationBookWindowX"), PlayerPrefs.GetFloat("ReputationBookWindowY"), 0);
            if (PlayerPrefs.HasKey("CurrencyListWindowX") && PlayerPrefs.HasKey("CurrencyListWindowY"))
                PopupWindowManager.MyInstance.currencyListWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("CurrencyListWindowX"), PlayerPrefs.GetFloat("CurrencyListWindowY"), 0);
            if (PlayerPrefs.HasKey("CharacterPanelWindowX") && PlayerPrefs.HasKey("CharacterPanelWindowY"))
                PopupWindowManager.MyInstance.characterPanelWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("CharacterPanelWindowX"), PlayerPrefs.GetFloat("CharacterPanelWindowY"), 0);
            if (PlayerPrefs.HasKey("LootWindowX") && PlayerPrefs.HasKey("LootWindowY"))
                PopupWindowManager.MyInstance.lootWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("LootWindowX"), PlayerPrefs.GetFloat("LootWindowY"), 0);
            if (PlayerPrefs.HasKey("VendorWindowX") && PlayerPrefs.HasKey("VendorWindowY"))
                PopupWindowManager.MyInstance.vendorWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("VendorWindowX"), PlayerPrefs.GetFloat("VendorWindowY"), 0);
            if (PlayerPrefs.HasKey("ChestWindowX") && PlayerPrefs.HasKey("ChestWindowY"))
                PopupWindowManager.MyInstance.chestWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("ChestWindowX"), PlayerPrefs.GetFloat("ChestWindowY"), 0);
            if (PlayerPrefs.HasKey("BankWindowX") && PlayerPrefs.HasKey("BankWindowY"))
                PopupWindowManager.MyInstance.bankWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("BankWindowX"), PlayerPrefs.GetFloat("BankWindowY"), 0);
            if (PlayerPrefs.HasKey("QuestLogWindowX") && PlayerPrefs.HasKey("QuestLogWindowY"))
                PopupWindowManager.MyInstance.questLogWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("QuestLogWindowX"), PlayerPrefs.GetFloat("QuestLogWindowY"), 0);
            if (PlayerPrefs.HasKey("AchievementListWindowX") && PlayerPrefs.HasKey("AchievementListWindowY"))
                PopupWindowManager.MyInstance.achievementListWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("AchievementListWindowX"), PlayerPrefs.GetFloat("AchievementListWindowY"), 0);
            if (PlayerPrefs.HasKey("QuestGiverWindowX") && PlayerPrefs.HasKey("QuestGiverWindowY"))
                PopupWindowManager.MyInstance.questGiverWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("QuestGiverWindowX"), PlayerPrefs.GetFloat("QuestGiverWindowY"), 0);
            if (PlayerPrefs.HasKey("SkillTrainerWindowX") && PlayerPrefs.HasKey("SkillTrainerWindowY"))
                PopupWindowManager.MyInstance.skillTrainerWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("SkillTrainerWindowX"), PlayerPrefs.GetFloat("SkillTrainerWindowY"), 0);
            if (PlayerPrefs.HasKey("InteractionWindowX") && PlayerPrefs.HasKey("InteractionWindowY"))
                PopupWindowManager.MyInstance.interactionWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("InteractionWindowX"), PlayerPrefs.GetFloat("InteractionWindowY"), 0);
            if (PlayerPrefs.HasKey("CraftingWindowX") && PlayerPrefs.HasKey("CraftingWindowY"))
                PopupWindowManager.MyInstance.craftingWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("CraftingWindowX"), PlayerPrefs.GetFloat("CraftingWindowY"), 0);
            if (PlayerPrefs.HasKey("MainMapWindowX") && PlayerPrefs.HasKey("MainMapWindowY"))
                PopupWindowManager.MyInstance.mainMapWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("MainMapWindowX"), PlayerPrefs.GetFloat("MainMapWindowY"), 0);
            if (PlayerPrefs.HasKey("QuestTrackerWindowX") && PlayerPrefs.HasKey("QuestTrackerWindowY"))
                UIManager.MyInstance.QuestTrackerWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("QuestTrackerWindowX"), PlayerPrefs.GetFloat("QuestTrackerWindowY"), 0);
            if (PlayerPrefs.HasKey("CombatLogWindowX") && PlayerPrefs.HasKey("CombatLogWindowY"))
                UIManager.MyInstance.CombatLogWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("CombatLogWindowX"), PlayerPrefs.GetFloat("CombatLogWindowY"), 0);

            if (PlayerPrefs.HasKey("MessageFeedManagerX") && PlayerPrefs.HasKey("MessageFeedManagerY"))
                MessageFeedManager.MyInstance.MessageFeedGameObject.transform.position = new Vector3(PlayerPrefs.GetFloat("MessageFeedManagerX"), PlayerPrefs.GetFloat("MessageFeedManagerY"), 0);

            if (PlayerPrefs.HasKey("FloatingCastBarControllerX") && PlayerPrefs.HasKey("FloatingCastBarControllerY")) {
                //Debug.Log("UIManager.MyInstance.MyFloatingCastBarController.transform.position: " + UIManager.MyInstance.MyFloatingCastBarController.transform.position);
                UIManager.MyInstance.FloatingCastBarController.transform.position = new Vector3(PlayerPrefs.GetFloat("FloatingCastBarControllerX"), PlayerPrefs.GetFloat("FloatingCastBarControllerY"), 0);
                //Debug.Log("UIManager.MyInstance.MyFloatingCastBarController.transform.position after set: " + UIManager.MyInstance.MyFloatingCastBarController.transform.position);
            }

            if (PlayerPrefs.HasKey("StatusEffectPanelControllerX") && PlayerPrefs.HasKey("StatusEffectPanelControllerY"))
                UIManager.MyInstance.StatusEffectPanelController.transform.position = new Vector3(PlayerPrefs.GetFloat("StatusEffectPanelControllerX"), PlayerPrefs.GetFloat("StatusEffectPanelControllerY"), 0);

            if (PlayerPrefs.HasKey("PlayerUnitFrameControllerX") && PlayerPrefs.HasKey("PlayerUnitFrameControllerY"))
                UIManager.MyInstance.PlayerUnitFrameController.transform.position = new Vector3(PlayerPrefs.GetFloat("PlayerUnitFrameControllerX"), PlayerPrefs.GetFloat("PlayerUnitFrameControllerY"), 0);

            if (PlayerPrefs.HasKey("FocusUnitFrameControllerX") && PlayerPrefs.HasKey("FocusUnitFrameControllerY"))
                UIManager.MyInstance.FocusUnitFrameController.transform.position = new Vector3(PlayerPrefs.GetFloat("FocusUnitFrameControllerX"), PlayerPrefs.GetFloat("FocusUnitFrameControllerY"), 0);

            if (PlayerPrefs.HasKey("MiniMapControllerX") && PlayerPrefs.HasKey("MiniMapControllerY"))
                UIManager.MyInstance.MiniMapController.transform.position = new Vector3(PlayerPrefs.GetFloat("MiniMapControllerX"), PlayerPrefs.GetFloat("MiniMapControllerY"), 0);

            if (PlayerPrefs.HasKey("XPBarControllerX") && PlayerPrefs.HasKey("XPBarControllerY"))
                UIManager.MyInstance.XPBarController.transform.position = new Vector3(PlayerPrefs.GetFloat("XPBarControllerX"), PlayerPrefs.GetFloat("XPBarControllerY"), 0);

            if (PlayerPrefs.HasKey("BottomPanelX") && PlayerPrefs.HasKey("BottomPanelY"))
                UIManager.MyInstance.BottomPanel.transform.position = new Vector3(PlayerPrefs.GetFloat("BottomPanelX"), PlayerPrefs.GetFloat("BottomPanelY"), 0);

            if (PlayerPrefs.HasKey("SidePanelX") && PlayerPrefs.HasKey("SidePanelY"))
                UIManager.MyInstance.SidePanel.transform.position = new Vector3(PlayerPrefs.GetFloat("SidePanelX"), PlayerPrefs.GetFloat("SidePanelY"), 0);

            if (PlayerPrefs.HasKey("MouseOverWindowX") && PlayerPrefs.HasKey("MouseOverWindowY"))
                UIManager.MyInstance.MouseOverWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("MouseOverWindowX"), PlayerPrefs.GetFloat("MouseOverWindowY"), 0);

        }

        public void SaveWindowPositions() {
            //Debug.Log("Savemanager.SaveWindowPositions()");

            PlayerPrefs.SetFloat("AbilityBookWindowX", PopupWindowManager.MyInstance.abilityBookWindow.transform.position.x);
            PlayerPrefs.SetFloat("AbilityBookWindowY", PopupWindowManager.MyInstance.abilityBookWindow.transform.position.y);

            PlayerPrefs.SetFloat("SkillBookWindowX", PopupWindowManager.MyInstance.skillBookWindow.transform.position.x);
            PlayerPrefs.SetFloat("SkillBookWindowY", PopupWindowManager.MyInstance.skillBookWindow.transform.position.y);

            //Debug.Log("abilityBookWindowX: " + abilityBookWindowX + "; abilityBookWindowY: " + abilityBookWindowY);
            PlayerPrefs.SetFloat("ReputationBookWindowX", PopupWindowManager.MyInstance.reputationBookWindow.transform.position.x);
            PlayerPrefs.SetFloat("ReputationBookWindowY", PopupWindowManager.MyInstance.reputationBookWindow.transform.position.y);
            PlayerPrefs.SetFloat("CurrencyListWindowX", PopupWindowManager.MyInstance.currencyListWindow.transform.position.x);
            PlayerPrefs.SetFloat("CurrencyListWindowY", PopupWindowManager.MyInstance.currencyListWindow.transform.position.y);

            PlayerPrefs.SetFloat("CharacterPanelWindowX", PopupWindowManager.MyInstance.characterPanelWindow.transform.position.x);
            PlayerPrefs.SetFloat("CharacterPanelWindowY", PopupWindowManager.MyInstance.characterPanelWindow.transform.position.y);
            PlayerPrefs.SetFloat("LootWindowX", PopupWindowManager.MyInstance.lootWindow.transform.position.x);
            PlayerPrefs.SetFloat("LootWindowY", PopupWindowManager.MyInstance.lootWindow.transform.position.y);
            PlayerPrefs.SetFloat("VendorWindowX", PopupWindowManager.MyInstance.vendorWindow.transform.position.x);
            PlayerPrefs.SetFloat("VendorWindowY", PopupWindowManager.MyInstance.vendorWindow.transform.position.y);
            PlayerPrefs.SetFloat("ChestWindowX", PopupWindowManager.MyInstance.chestWindow.transform.position.x);
            PlayerPrefs.SetFloat("ChestWindowY", PopupWindowManager.MyInstance.chestWindow.transform.position.y);
            PlayerPrefs.SetFloat("BankWindowX", PopupWindowManager.MyInstance.bankWindow.transform.position.x);
            PlayerPrefs.SetFloat("BankWindowY", PopupWindowManager.MyInstance.bankWindow.transform.position.y);
            PlayerPrefs.SetFloat("QuestLogWindowX", PopupWindowManager.MyInstance.questLogWindow.transform.position.x);
            PlayerPrefs.SetFloat("QuestLogWindowY", PopupWindowManager.MyInstance.questLogWindow.transform.position.y);
            PlayerPrefs.SetFloat("AchievementListWindowX", PopupWindowManager.MyInstance.achievementListWindow.transform.position.x);
            PlayerPrefs.SetFloat("AchievementListWindowY", PopupWindowManager.MyInstance.achievementListWindow.transform.position.y);
            PlayerPrefs.SetFloat("QuestGiverWindowX", PopupWindowManager.MyInstance.questGiverWindow.transform.position.x);
            PlayerPrefs.SetFloat("QuestGiverWindowY", PopupWindowManager.MyInstance.questGiverWindow.transform.position.y);
            PlayerPrefs.SetFloat("SkillTrainerWindowX", PopupWindowManager.MyInstance.skillTrainerWindow.transform.position.x);
            PlayerPrefs.SetFloat("SkillTrainerWindowY", PopupWindowManager.MyInstance.skillTrainerWindow.transform.position.y);
            PlayerPrefs.SetFloat("InteractionWindowX", PopupWindowManager.MyInstance.interactionWindow.transform.position.x);
            PlayerPrefs.SetFloat("InteractionWindowY", PopupWindowManager.MyInstance.interactionWindow.transform.position.y);
            PlayerPrefs.SetFloat("CraftingWindowX", PopupWindowManager.MyInstance.craftingWindow.transform.position.x);
            PlayerPrefs.SetFloat("CraftingWindowY", PopupWindowManager.MyInstance.craftingWindow.transform.position.y);
            PlayerPrefs.SetFloat("MainMapWindowX", PopupWindowManager.MyInstance.mainMapWindow.transform.position.x);
            PlayerPrefs.SetFloat("MainMapWindowY", PopupWindowManager.MyInstance.mainMapWindow.transform.position.y);
            PlayerPrefs.SetFloat("QuestTrackerWindowX", UIManager.MyInstance.QuestTrackerWindow.transform.position.x);
            PlayerPrefs.SetFloat("QuestTrackerWindowY", UIManager.MyInstance.QuestTrackerWindow.transform.position.y);
            PlayerPrefs.SetFloat("CombatLogWindowX", UIManager.MyInstance.CombatLogWindow.transform.position.x);
            PlayerPrefs.SetFloat("CombatLogWindowY", UIManager.MyInstance.CombatLogWindow.transform.position.y);

            PlayerPrefs.SetFloat("MessageFeedManagerX", MessageFeedManager.MyInstance.MessageFeedGameObject.transform.position.x);
            PlayerPrefs.SetFloat("MessageFeedManagerY", MessageFeedManager.MyInstance.MessageFeedGameObject.transform.position.y);

            //Debug.Log("Saving FloatingCastBarController: " + UIManager.MyInstance.MyFloatingCastBarController.transform.position.x + "; " + UIManager.MyInstance.MyFloatingCastBarController.transform.position.y);
            PlayerPrefs.SetFloat("FloatingCastBarControllerX", UIManager.MyInstance.FloatingCastBarController.transform.position.x);
            PlayerPrefs.SetFloat("FloatingCastBarControllerY", UIManager.MyInstance.FloatingCastBarController.transform.position.y);

            PlayerPrefs.SetFloat("StatusEffectPanelControllerX", UIManager.MyInstance.StatusEffectPanelController.transform.position.x);
            PlayerPrefs.SetFloat("StatusEffectPanelControllerY", UIManager.MyInstance.StatusEffectPanelController.transform.position.y);

            PlayerPrefs.SetFloat("PlayerUnitFrameControllerX", UIManager.MyInstance.PlayerUnitFrameController.transform.position.x);
            PlayerPrefs.SetFloat("PlayerUnitFrameControllerY", UIManager.MyInstance.PlayerUnitFrameController.transform.position.y);

            PlayerPrefs.SetFloat("FocusUnitFrameControllerX", UIManager.MyInstance.FocusUnitFrameController.transform.position.x);
            PlayerPrefs.SetFloat("FocusUnitFrameControllerY", UIManager.MyInstance.FocusUnitFrameController.transform.position.y);

            PlayerPrefs.SetFloat("MiniMapControllerX", UIManager.MyInstance.MiniMapController.transform.position.x);
            PlayerPrefs.SetFloat("MiniMapControllerY", UIManager.MyInstance.MiniMapController.transform.position.y);

            PlayerPrefs.SetFloat("XPBarControllerX", UIManager.MyInstance.XPBarController.transform.position.x);
            PlayerPrefs.SetFloat("XPBarControllerY", UIManager.MyInstance.XPBarController.transform.position.y);

            PlayerPrefs.SetFloat("BottomPanelX", UIManager.MyInstance.BottomPanel.transform.position.x);
            PlayerPrefs.SetFloat("BottomPanelY", UIManager.MyInstance.BottomPanel.transform.position.y);

            PlayerPrefs.SetFloat("SidePanelX", UIManager.MyInstance.SidePanel.transform.position.x);
            PlayerPrefs.SetFloat("SidePanelY", UIManager.MyInstance.SidePanel.transform.position.y);

            PlayerPrefs.SetFloat("MouseOverWindowX", UIManager.MyInstance.MouseOverWindow.transform.position.x);
            PlayerPrefs.SetFloat("MouseOverWindowY", UIManager.MyInstance.MouseOverWindow.transform.position.y);

            if (InventoryManager.MyInstance.MyBagNodes != null && InventoryManager.MyInstance.MyBagNodes.Count > 0) {
                for (int i = 0; i < 13; i++) {
                    //Debug.Log("SaveManager.SaveWindowPositions(): " + i);
                    if (InventoryManager.MyInstance.MyBagNodes[i].MyBagWindow.IsOpen) {
                        PlayerPrefs.SetFloat("InventoryWindowX" + i, InventoryManager.MyInstance.MyBagNodes[i].MyBagWindow.transform.position.x);
                        PlayerPrefs.SetFloat("InventoryWindowY" + i, InventoryManager.MyInstance.MyBagNodes[i].MyBagWindow.transform.position.y);
                    } else {
                        //Debug.Log("SaveManager.SaveWindowPositions(): " + i + "X: " + InventoryManager.MyInstance.MyBagNodes[i].MyBagWindow.transform.position.x + "; y: " + InventoryManager.MyInstance.MyBagNodes[i].MyBagWindow.transform.position.y + " WINDOW CLOSED@!!!!, NOT SAVING");
                    }
                }
            }

        }

        public void DeleteGame(AnyRPGSaveData anyRPGSaveData) {
            File.Delete(Application.persistentDataPath + "/" + makeSaveDirectoryName() + "/" + anyRPGSaveData.DataFileName);
            SystemEventManager.MyInstance.NotifyOnDeleteSaveData();
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

        public static string makeSaveDirectoryName() {

            string replaceString = string.Empty;
            Regex regex = new Regex("[^a-zA-Z0-9]");
            if (SystemConfigurationManager.MyInstance != null) {
                replaceString = regex.Replace(SystemConfigurationManager.MyInstance.GameName, "");
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