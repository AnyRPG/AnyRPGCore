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

        private UMAData umaSaveData = null;
        private string recipeString = string.Empty;
        private string jsonSavePath = string.Empty;

        // prevent infinite loop loading list, and why would anyone need more than 1000 save games at this point
        private int maxSaveFiles = 1000;

        private string saveFileName = "AnyRPGPlayerSaveData";

        private AnyRPGSaveData currentSaveData = new AnyRPGSaveData();

        // data to turn into json for save
        private List<AnyRPGSaveData> anyRPGSaveDataList = new List<AnyRPGSaveData>();

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
                anyRPGSaveData.playerFaction = PlayerManager.MyInstance.MyDefaultFaction.MyName;
            }
            if (anyRPGSaveData.characterClass == null) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): Player Faction is null.  Setting to default");
                anyRPGSaveData.characterClass = string.Empty;
            }
            if (anyRPGSaveData.unitProfileName == null) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): Player Faction is null.  Setting to default");
                anyRPGSaveData.unitProfileName = PlayerManager.MyInstance.MyDefaultCharacterCreatorUnitProfileName;
            }
            if (anyRPGSaveData.PlayerUMARecipe == null) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): Player UMA Recipe is null.  Setting to empty");
                anyRPGSaveData.PlayerUMARecipe = string.Empty;
            }
            if (anyRPGSaveData.CurrentScene == null) {
                //Debug.Log("SaveManager.LoadSaveDataFromFile(" + fileName + "): CurrentScene is null.  Setting to default");
                anyRPGSaveData.CurrentScene = LevelManager.MyInstance.MyDefaultStartingZone;
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
            if (anyRPGSaveData.questSaveData == null || overWrite) {
                anyRPGSaveData.questSaveData = new List<QuestSaveData>();
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
            if (anyRPGSaveData.reputationSaveData == null || overWrite) {
                anyRPGSaveData.reputationSaveData = new List<ReputationSaveData>();
            }
            if (anyRPGSaveData.equipmentSaveData == null || overWrite) {
                anyRPGSaveData.equipmentSaveData = new List<EquipmentSaveData>();
            }
            if (anyRPGSaveData.currencySaveData == null || overWrite) {
                anyRPGSaveData.currencySaveData = new List<CurrencySaveData>();
            }
            if (anyRPGSaveData.dialogSaveData == null || overWrite) {
                anyRPGSaveData.dialogSaveData = new List<DialogSaveData>();
            }
            if (anyRPGSaveData.sceneNodeSaveData == null || overWrite) {
                anyRPGSaveData.sceneNodeSaveData = new List<SceneNodeSaveData>();
            }
            if (anyRPGSaveData.statusEffectSaveData == null || overWrite) {
                anyRPGSaveData.statusEffectSaveData = new List<StatusEffectSaveData>();
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
            SaveUMASettings();
        }


        public void SaveUMASettings() {
            //Debug.Log("SaveManager.SaveUMASettings()");
            if (PlayerManager.MyInstance.MyAvatar != null) {
                //Debug.Log("SaveManager.SaveUMASettings(): avatar exists");
                if (recipeString == string.Empty) {
                    //Debug.Log("SaveManager.SaveUMASettings(): recipestring is empty");
                    recipeString = PlayerManager.MyInstance.MyAvatar.GetCurrentRecipe();
                } else {
                    //Debug.Log("SaveManager.SaveUMASettings(): recipestring is not empty");
                    recipeString = PlayerManager.MyInstance.MyAvatar.GetCurrentRecipe();
                }
            } else {
                //Debug.Log("SaveManager.SaveUMASettings(): no avatar!!!");
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

        public void LoadUMASettings() {
            //Debug.Log("Savemanager.LoadUMASettings()");
            if (recipeString == string.Empty) {
                //Debug.Log("Savemanager.LoadUMASettings(): recipe string is empty. exiting!");
                return;
            }
            LoadUMASettings(recipeString, PlayerManager.MyInstance.MyAvatar);
        }

        public void LoadUMASettings(DynamicCharacterAvatar _dynamicCharacterAvatar) {
            //Debug.Log("Savemanager.LoadUMASettings(DynamicCharacterAvatar)");
            if (recipeString == string.Empty) {
                //Debug.Log("Savemanager.LoadUMASettings(): recipe string is empty. exiting!");
                return;
            }
            LoadUMASettings(recipeString, _dynamicCharacterAvatar);
        }


        public void LoadUMASettings(string _recipeString, DynamicCharacterAvatar _dynamicCharacterAvatar) {
            //Debug.Log("Savemanager.LoadUMASettings(string, DynamicCharacterAvatar)");
            if (_recipeString == null || _recipeString == string.Empty || _dynamicCharacterAvatar == null) {
                //Debug.Log("Savemanager.LoadUMASettings(): _recipeString is empty. exiting!");
                return;
            }
            _dynamicCharacterAvatar.ClearSlots();
            //Debug.Log("Savemanager.LoadUMASettings(): " + recipeString);
            _dynamicCharacterAvatar.SetLoadString(_recipeString);
            _dynamicCharacterAvatar.LoadFromRecipeString(_recipeString);
            _dynamicCharacterAvatar.BuildCharacter();
        }

        /*
        public void SaveUMASettings(UMAData umaData) {

        }
        */

        // save a game for the first time
        public void SaveGame() {
            bool foundValidName = false;
            if (currentSaveData.Equals(default(AnyRPGSaveData))) {
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
                SaveGame(currentSaveData);
            } else {
                Debug.Log("Too Many save files(" + maxSaveFiles + "), delete some");
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


        public void SaveGame(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveGame()");

            anyRPGSaveData.PlayerLevel = PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel;
            anyRPGSaveData.currentExperience = PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyCurrentXP;
            anyRPGSaveData.playerName = PlayerManager.MyInstance.MyCharacter.MyCharacterName;
            anyRPGSaveData.playerFaction = PlayerManager.MyInstance.MyCharacter.MyFaction.MyName;
            anyRPGSaveData.characterClass = PlayerManager.MyInstance.MyCharacter.MyCharacterClass.MyName;
            anyRPGSaveData.unitProfileName = PlayerManager.MyInstance.MyCharacter.MyUnitProfileName;
            anyRPGSaveData.currentHealth = PlayerManager.MyInstance.MyCharacter.MyCharacterStats.currentHealth;
            anyRPGSaveData.currentMana = PlayerManager.MyInstance.MyCharacter.MyCharacterStats.currentMana;
            anyRPGSaveData.PlayerLocationX = PlayerManager.MyInstance.MyPlayerUnitObject.transform.position.x;
            anyRPGSaveData.PlayerLocationY = PlayerManager.MyInstance.MyPlayerUnitObject.transform.position.y;
            anyRPGSaveData.PlayerLocationZ = PlayerManager.MyInstance.MyPlayerUnitObject.transform.position.z;
            anyRPGSaveData.PlayerRotationX = PlayerManager.MyInstance.MyPlayerUnitObject.transform.forward.x;
            anyRPGSaveData.PlayerRotationY = PlayerManager.MyInstance.MyPlayerUnitObject.transform.forward.y;
            anyRPGSaveData.PlayerRotationZ = PlayerManager.MyInstance.MyPlayerUnitObject.transform.forward.z;
            //Debug.Log("Savemanager.SaveGame() rotation: " + anyRPGSaveData.PlayerRotationX + ", " + anyRPGSaveData.PlayerRotationY + ", " + anyRPGSaveData.PlayerRotationZ);
            anyRPGSaveData.PlayerUMARecipe = recipeString;
            anyRPGSaveData.CurrentScene = LevelManager.MyInstance.GetActiveSceneNode().MySceneName;

            // shared code to setup resource lists on load of old version file or save of new one
            anyRPGSaveData = InitializeResourceLists(anyRPGSaveData, true);

            SaveQuestData(anyRPGSaveData);
            SaveDialogData(anyRPGSaveData);
            SaveActionBarData(anyRPGSaveData);
            SaveInventorySlotData(anyRPGSaveData);
            SaveEquippedBagData(anyRPGSaveData);
            SaveAbilityData(anyRPGSaveData);
            SaveSkillData(anyRPGSaveData);
            SaveReputationData(anyRPGSaveData);
            SaveEquipmentData(anyRPGSaveData);
            SaveCurrencyData(anyRPGSaveData);
            SaveSceneNodeData(anyRPGSaveData);
            SaveStatusEffectData(anyRPGSaveData);

            SaveWindowPositions();

            //Debug.Log("Savemanager.SaveQuestData(): size: " + anyRPGSaveData.questSaveData.Count);

            string saveDate = string.Empty;
            if (anyRPGSaveData.DataCreatedOn == null || anyRPGSaveData.DataCreatedOn == string.Empty) {
                anyRPGSaveData.DataCreatedOn = DateTime.Now.ToLongDateString();
            }
            SaveDataFile(anyRPGSaveData);

            PlayerPrefs.SetString("LastSaveDataFileName", anyRPGSaveData.DataFileName);
        }

        public void SaveDataFile(AnyRPGSaveData dataToSave) {
            dataToSave.DataSavedOn = DateTime.Now.ToLongDateString();

            string jsonString = JsonUtility.ToJson(dataToSave);
            //Debug.Log(jsonString);
            string jsonSavePath = Application.persistentDataPath + "/" + makeSaveDirectoryName() + "/" + dataToSave.DataFileName;
            File.WriteAllText(jsonSavePath, jsonString);

        }

        public void SaveQuestData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveQuestData()");

            foreach (Quest quest in SystemQuestManager.MyInstance.GetResourceList()) {
                //Debug.Log("Savemanager.SaveQuestData(): Getting quest data from SystemQuestManager: " + quest.MyName);
                QuestSaveData questSaveData = new QuestSaveData();
                questSaveData.MyName = quest.MyName;
                questSaveData.turnedIn = quest.TurnedIn;
                questSaveData.isAchievement = quest.MyIsAchievement;
                questSaveData.inLog = QuestLog.MyInstance.HasQuest(quest.MyName);
                List<QuestObjectiveSaveData> killObjectiveSaveDataList = new List<QuestObjectiveSaveData>();
                foreach (QuestObjective questObjective in quest.MyKillObjectives) {
                    QuestObjectiveSaveData tmp = new QuestObjectiveSaveData();
                    tmp.MyName = questObjective.MyType;
                    tmp.MyAmount = questObjective.MyCurrentAmount;
                    //Debug.Log("Saving killobjective: " + tmp.MyName + "; amount: " + tmp.MyAmount);
                    killObjectiveSaveDataList.Add(tmp);
                }
                List<QuestObjectiveSaveData> useInteractableObjectiveSaveDataList = new List<QuestObjectiveSaveData>();
                foreach (QuestObjective questObjective in quest.MyUseInteractableObjectives) {
                    QuestObjectiveSaveData tmp = new QuestObjectiveSaveData();
                    tmp.MyName = questObjective.MyType;
                    tmp.MyAmount = questObjective.MyCurrentAmount;
                    useInteractableObjectiveSaveDataList.Add(tmp);
                }
                // ability
                List<QuestObjectiveSaveData> abilityObjectiveSaveDataList = new List<QuestObjectiveSaveData>();
                foreach (QuestObjective questObjective in quest.MyAbilityObjectives) {
                    //Debug.Log("Saving abilityobjective: " + questObjective.MyType);
                    QuestObjectiveSaveData tmp = new QuestObjectiveSaveData();
                    tmp.MyName = questObjective.MyType;
                    tmp.MyAmount = questObjective.MyCurrentAmount;
                    //Debug.Log("Saving abilityobjective: " + tmp.MyName + "; amount: " + tmp.MyAmount);
                    abilityObjectiveSaveDataList.Add(tmp);
                }

                List<QuestObjectiveSaveData> collectObjectiveSaveDataList = new List<QuestObjectiveSaveData>();
                foreach (QuestObjective questObjective in quest.MyCollectObjectives) {
                    QuestObjectiveSaveData tmp = new QuestObjectiveSaveData();
                    tmp.MyName = questObjective.MyType;
                    tmp.MyAmount = questObjective.MyCurrentAmount;
                    collectObjectiveSaveDataList.Add(tmp);
                }
                List<QuestObjectiveSaveData> tradeSkillObjectiveSaveDataList = new List<QuestObjectiveSaveData>();
                foreach (QuestObjective questObjective in quest.MyTradeSkillObjectives) {
                    QuestObjectiveSaveData tmp = new QuestObjectiveSaveData();
                    tmp.MyName = questObjective.MyType;
                    tmp.MyAmount = questObjective.MyCurrentAmount;
                    tradeSkillObjectiveSaveDataList.Add(tmp);
                }
                questSaveData.killObjectives = killObjectiveSaveDataList;
                questSaveData.collectObjectives = collectObjectiveSaveDataList;
                questSaveData.useInteractableObjectives = useInteractableObjectiveSaveDataList;
                questSaveData.tradeSkillObjectives = tradeSkillObjectiveSaveDataList;
                questSaveData.abilityObjectives = abilityObjectiveSaveDataList;
                anyRPGSaveData.questSaveData.Add(questSaveData);
                //Debug.Log("Savemanager.SaveQuestData(): " + questSaveData.MyName + ", turnedIn: " + questSaveData.turnedIn + ", inLog: " + questSaveData.inLog);
            }
            //Debug.Log("Savemanager.SaveQuestData(): size: " + anyRPGSaveData.questSaveData.Count);

        }

        public void SaveDialogData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveQuestData()");

            foreach (Dialog dialog in SystemDialogManager.MyInstance.GetResourceList()) {
                DialogSaveData dialogSaveData = new DialogSaveData();
                dialogSaveData.MyName = dialog.MyName;
                dialogSaveData.turnedIn = dialog.TurnedIn;
                anyRPGSaveData.dialogSaveData.Add(dialogSaveData);
                //Debug.Log("Savemanager.SaveQuestData(): " + questSaveData.MyName + ", turnedIn: " + questSaveData.turnedIn + ", inLog: " + questSaveData.inLog);
            }
        }

        public void SaveSceneNodeData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveSceneNodeData()");

            foreach (SceneNode sceneNode in SystemSceneNodeManager.MyInstance.GetResourceList()) {
                SceneNodeSaveData sceneNodeSaveData = new SceneNodeSaveData();
                sceneNodeSaveData.MyName = sceneNode.MyName;
                sceneNodeSaveData.isCutSceneViewed = sceneNode.MyCutsceneViewed;
                anyRPGSaveData.sceneNodeSaveData.Add(sceneNodeSaveData);
            }
        }

        public void SaveStatusEffectData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveSceneNodeData()");

            foreach (StatusEffectNode statusEffectNode in PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyStatusEffects.Values) {
                if (statusEffectNode.MyStatusEffect.MyClassTrait == false && statusEffectNode.MyStatusEffect.MySourceCharacter == PlayerManager.MyInstance.MyCharacter) {
                    StatusEffectSaveData statusEffectSaveData = new StatusEffectSaveData();
                    statusEffectSaveData.MyName = statusEffectNode.MyStatusEffect.MyName;
                    statusEffectSaveData.remainingSeconds = (int)statusEffectNode.MyStatusEffect.GetRemainingDuration();
                    anyRPGSaveData.statusEffectSaveData.Add(statusEffectSaveData);
                }
            }
        }

        public void SaveActionBarData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveActionBarData()");
            foreach (ActionButton actionButton in UIManager.MyInstance.MyActionBarManager.GetActionButtons()) {
                ActionBarSaveData saveData = new ActionBarSaveData();
                saveData.MyName = (actionButton.MyUseable == null ? string.Empty : (actionButton.MyUseable as IDescribable).MyName);
                saveData.isItem = (actionButton.MyUseable == null ? false : (actionButton.MyUseable is Item ? true : false));
                //Debug.Log("Savemanager.SaveActionBarData(): saveData.MyName:" + saveData.MyName + "; saveData.isItem" + saveData.isItem);
                anyRPGSaveData.actionBarSaveData.Add(saveData);
            }
        }

        public void SaveInventorySlotData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveInventorySlotData()");
            foreach (SlotScript slotScript in InventoryManager.MyInstance.GetSlots()) {
                InventorySlotSaveData saveData = new InventorySlotSaveData();
                saveData.MyName = (slotScript.MyItem == null ? string.Empty : slotScript.MyItem.MyName);
                saveData.stackCount = (slotScript.MyItem == null ? 0 : slotScript.MyCount);
                anyRPGSaveData.inventorySlotSaveData.Add(saveData);
            }
        }

        public void SaveReputationData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveReputationData()");
            foreach (FactionDisposition factionDisposition in PlayerManager.MyInstance.MyCharacter.MyCharacterFactionManager.MyDispositionDictionary) {
                ReputationSaveData saveData = new ReputationSaveData();
                saveData.MyName = factionDisposition.MyFaction.MyName;
                saveData.MyAmount = factionDisposition.disposition;
                anyRPGSaveData.reputationSaveData.Add(saveData);
            }
        }

        public void SaveCurrencyData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveCurrencyData()");
            foreach (CurrencyNode currencyNode in PlayerManager.MyInstance.MyCharacter.MyPlayerCurrencyManager.MyCurrencyList.Values) {
                CurrencySaveData currencySaveData = new CurrencySaveData();
                currencySaveData.MyAmount = currencyNode.MyAmount;
                currencySaveData.MyName = currencyNode.currency.MyName;
                anyRPGSaveData.currencySaveData.Add(currencySaveData);
            }
        }

        public void SaveEquippedBagData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveEquippedBagData()");
            foreach (BagNode bagNode in InventoryManager.MyInstance.MyBagNodes) {
                //Debug.Log("Savemanager.SaveEquippedBagData(): got bagNode");
                EquippedBagSaveData saveData = new EquippedBagSaveData();
                saveData.MyName = (bagNode.MyBag != null ? bagNode.MyBag.MyName : string.Empty);
                saveData.slotCount = (bagNode.MyBag != null ? bagNode.MyBag.MySlots : 0);
                saveData.isBankBag = bagNode.MyIsBankNode;
                anyRPGSaveData.equippedBagSaveData.Add(saveData);
            }
        }

        public void SaveAbilityData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveAbilityData()");
            foreach (BaseAbility baseAbility in PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.MyAbilityList.Values) {
                AbilitySaveData saveData = new AbilitySaveData();
                saveData.MyName = baseAbility.MyName;
                anyRPGSaveData.abilitySaveData.Add(saveData);
            }
        }

        public void SaveEquipmentData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveEquipmentData()");
            if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager != null) {
                foreach (Equipment equipment in PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager.MyCurrentEquipment.Values) {
                    EquipmentSaveData saveData = new EquipmentSaveData();
                    saveData.MyName = (equipment == null ? string.Empty : equipment.MyName);
                    anyRPGSaveData.equipmentSaveData.Add(saveData);
                }
            }
        }

        public void SaveSkillData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.SaveSkillData()");
            foreach (string skillName in PlayerManager.MyInstance.MyCharacter.MyCharacterSkillManager.MySkillList.Keys) {
                SkillSaveData saveData = new SkillSaveData();
                saveData.MyName = skillName;
                anyRPGSaveData.skillSaveData.Add(saveData);
            }
        }

        public void LoadQuestData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadQuestData()");
            foreach (QuestSaveData questSaveData in anyRPGSaveData.questSaveData) {
                //Debug.Log("Savemanager.LoadQuestData(): loading questsavedata");
                QuestLog.MyInstance.LoadQuest(questSaveData);
            }
        }

        public void LoadDialogData(AnyRPGSaveData anyRPGSaveData) {
            foreach (DialogSaveData dialogSaveData in anyRPGSaveData.dialogSaveData) {
                SystemDialogManager.MyInstance.LoadDialog(dialogSaveData);
            }
        }

        public void LoadSceneNodeData(AnyRPGSaveData anyRPGSaveData) {
            foreach (SceneNodeSaveData sceneNodeSaveData in anyRPGSaveData.sceneNodeSaveData) {
                SystemSceneNodeManager.MyInstance.LoadSceneNode(sceneNodeSaveData);
            }
        }

        public void LoadStatusEffectData(AnyRPGSaveData anyRPGSaveData) {
            foreach (StatusEffectSaveData statusEffectSaveData in anyRPGSaveData.statusEffectSaveData) {
                PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.ApplySavedStatusEffects(statusEffectSaveData);
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
            //int counter = 0;
            foreach (EquipmentSaveData equipmentSaveData in anyRPGSaveData.equipmentSaveData) {
                //Debug.Log("Savemanager.LoadEquipmentData(): checking equipment");
                if (equipmentSaveData.MyName != string.Empty) {
                    //Debug.Log("Savemanager.LoadEquipmentData(): checking equipment: using item: " + equipmentSaveData.MyName);
                    Equipment newItem = (SystemItemManager.MyInstance.GetNewResource(equipmentSaveData.MyName) as Equipment);
                    if (characterEquipmentManager != null) {
                        characterEquipmentManager.Equip(newItem);
                    } else {
                        //Debug.Log("Issue with equipment manager on player");
                    }
                    //newItem.Use();
                }
                //counter++;
            }
        }

        public void LoadReputationData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadReputationData()");
            //int counter = 0;
            foreach (ReputationSaveData reputationSaveData in anyRPGSaveData.reputationSaveData) {
                FactionDisposition factionDisposition = new FactionDisposition();
                factionDisposition.MyFaction = SystemFactionManager.MyInstance.GetResource(reputationSaveData.MyName);
                factionDisposition.disposition = reputationSaveData.MyAmount;
                PlayerManager.MyInstance.MyCharacter.MyCharacterFactionManager.AddReputation(factionDisposition.MyFaction, (int)factionDisposition.disposition);
                //counter++;
            }
        }

        public void LoadCurrencyData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadCurrencyData()");
            foreach (CurrencySaveData currencySaveData in anyRPGSaveData.currencySaveData) {
                PlayerManager.MyInstance.MyCharacter.MyPlayerCurrencyManager.AddCurrency(SystemCurrencyManager.MyInstance.GetResource(currencySaveData.MyName), currencySaveData.MyAmount);
            }
        }

        public void LoadAbilityData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadAbilityData()");

            foreach (AbilitySaveData abilitySaveData in anyRPGSaveData.abilitySaveData) {
                if (abilitySaveData.MyName != string.Empty) {
                    (PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager as PlayerAbilityManager).LoadAbility(abilitySaveData.MyName);
                }
            }

        }

        public void LoadSkillData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadSkillData()");
            foreach (SkillSaveData skillSaveData in anyRPGSaveData.skillSaveData) {
                PlayerManager.MyInstance.MyCharacter.MyCharacterSkillManager.LoadSkill(skillSaveData.MyName);
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
                    List<Item> itemList = InventoryManager.MyInstance.GetItems(actionBarSaveData.MyName, 1);
                    if (itemList.Count > 0) {
                        //Debug.Log("Savemanager.LoadActionBarData(): searching for usable(" + actionBarSaveData.MyName + ") in inventory and itemlist.count was: " + itemList.Count);
                        useable = itemList[0] as IUseable;
                    } else {
                        //Debug.Log("Savemanager.LoadActionBarData(): searching for usable(" + actionBarSaveData.MyName + ") in inventory and itemlist.count was: " + itemList.Count);
                    }
                } else {
                    // find ability from system ability manager
                    //Debug.Log("Savemanager.LoadActionBarData(): searching for usable in ability manager");
                    if (actionBarSaveData.MyName != string.Empty && actionBarSaveData.MyName != null) {
                        useable = SystemAbilityManager.MyInstance.GetResource(actionBarSaveData.MyName);
                    } else {
                        //Debug.Log("Savemanager.LoadActionBarData(): saved action bar had no name");
                    }
                }
                if (useable != null) {
                    //Debug.Log("Savemanager.LoadActionBarData(): setting useable on button: " + counter);
                    UIManager.MyInstance.MyActionBarManager.GetActionButtons()[counter].SetUseable(useable);
                } else {
                    //Debug.Log("Savemanager.LoadActionBarData(): no usable set on this actionbutton");
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

            // initialize inventory so there is a place to put the inventory
            InventoryManager.MyInstance.PerformSetupActivities();

            InventoryManager.MyInstance.CreateDefaultBankBag();
            InventoryManager.MyInstance.CreateDefaultBackpack();

            SystemWindowManager.MyInstance.loadGameWindow.CloseWindow();

            // load default scene
            LevelManager.MyInstance.LoadFirstScene();
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
        public void SetPlayerManagerPrefab(AnyRPGSaveData anyRPGSaveData) {
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

        public void ClearSharedData() {
            //Debug.Log("SaveManager.ClearSharedData()");
            ClearUMASettings();
            PlayerManager.MyInstance.SetDefaultPrefab();

            ClearSystemManagedCharacterData();

            // added to prevent overwriting existing save file after going to main menu after saving game and starting new game
            currentSaveData = new AnyRPGSaveData();
        }


        public void LoadGame(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log("Savemanager.LoadGame()");
            ClearSharedData();
            currentSaveData = anyRPGSaveData;

            // initialize inventory so there is a place to put the inventory
            InventoryManager.MyInstance.PerformSetupActivities();

            // player level
            PlayerManager.MyInstance.MyInitialLevel = anyRPGSaveData.PlayerLevel;

            // scene and location
            Vector3 playerLocation = new Vector3(anyRPGSaveData.PlayerLocationX, anyRPGSaveData.PlayerLocationY, anyRPGSaveData.PlayerLocationZ);
            Vector3 playerRotation = new Vector3(anyRPGSaveData.PlayerRotationX, anyRPGSaveData.PlayerRotationY, anyRPGSaveData.PlayerRotationZ);
            //Debug.Log("Savemanager.LoadGame() rotation: " + anyRPGSaveData.PlayerRotationX + ", " + anyRPGSaveData.PlayerRotationY + ", " + anyRPGSaveData.PlayerRotationZ);

            // disable auto-accept achievements since we haven't loaded the data that tells us if they are complete yet
            SystemQuestManager.MyInstance.CleanupEventSubscriptions();

            // spawn player connection so all the data can be loaded
            PlayerManager.MyInstance.SpawnPlayerConnection();
            PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyCurrentXP = anyRPGSaveData.currentExperience;
            PlayerManager.MyInstance.SetPlayerName(anyRPGSaveData.playerName);

            PlayerManager.MyInstance.MyCharacter.SetCharacterFaction(SystemFactionManager.MyInstance.GetResource(anyRPGSaveData.playerFaction));
            //PlayerManager.MyInstance.MyCharacter.MyCharacterClassName = anyRPGSaveData.characterClass;

            //PlayerManager.MyInstance.MyCharacter.SetCharacterClass(anyRPGSaveData.characterClass);

            PlayerManager.MyInstance.MyCharacter.SetUnitProfile(anyRPGSaveData.unitProfileName);

            // moved to clearshareddata to have central clearing method
            //ClearSystemManagedCharacterData();

            // THIS NEEDS TO BE DOWN HERE SO THE PLAYERSTATS EXISTS TO SUBSCRIBE TO THE EQUIP EVENTS AND INCREASE STATS
            SetPlayerManagerPrefab(anyRPGSaveData);

            // fix for random start order


            // complex data
            LoadEquippedBagData(anyRPGSaveData);
            LoadInventorySlotData(anyRPGSaveData);
            LoadAbilityData(anyRPGSaveData);

            // testing - move here to prevent learning auto-attack ability twice
            LoadEquipmentData(anyRPGSaveData, PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager);

            // testing - move here to prevent learning abilities and filling up bars
            PlayerManager.MyInstance.MyCharacter.SetCharacterClass(SystemCharacterClassManager.MyInstance.GetResource(anyRPGSaveData.characterClass));

            LoadSkillData(anyRPGSaveData);
            LoadReputationData(anyRPGSaveData);
            LoadQuestData(anyRPGSaveData);
            LoadDialogData(anyRPGSaveData);
            LoadSceneNodeData(anyRPGSaveData);

            LoadActionBarData(anyRPGSaveData);


            LoadCurrencyData(anyRPGSaveData);

            LoadStatusEffectData(anyRPGSaveData);

            // necessary?  should be handled by setcharacterclass call
            CharacterClass characterClass = SystemCharacterClassManager.MyInstance.GetResource(anyRPGSaveData.characterClass);
            PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.ApplyClassTraits(characterClass);

            // now that we have loaded the quest data, we can re-enable references
            SystemQuestManager.MyInstance.CreateEventSubscriptions();


            // set health last after equipment loaded for modifiers
            if (anyRPGSaveData.currentHealth > 0) {
                PlayerManager.MyInstance.MyCharacter.MyCharacterStats.SetHealth(anyRPGSaveData.currentHealth);
            }
            if (anyRPGSaveData.currentMana > 0) {
                PlayerManager.MyInstance.MyCharacter.MyCharacterStats.SetMana(anyRPGSaveData.currentMana);
            }

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
                    if (PopupWindowManager.MyInstance.bankWindow.MyCloseableWindowContents != null) {
                        (PopupWindowManager.MyInstance.bankWindow.MyCloseableWindowContents as BankPanel).ClearSlots();
                        (PopupWindowManager.MyInstance.bankWindow.MyCloseableWindowContents as BankPanel).MyBagBarController.ClearBagButtons();
                    } else {
                        //Debug.Log("windowcontents was null");
                    }
                } else {
                    //Debug.Log("bankwindow was null");
                }
            } else {
                //Debug.Log("popupwindowmanager was was null");
            }
            // since this is done on logout anyway, doesn't seem to be any point in doing it here
            /*
            SystemQuestManager.MyInstance.ReloadResourceList();
            SystemDialogManager.MyInstance.ReloadResourceList();
            */
            UIManager.MyInstance.MyActionBarManager.ClearActionBars();
            QuestLog.MyInstance.ClearLog();
            PlayerManager.MyInstance.ResetInitialLevel();
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
                UIManager.MyInstance.MyQuestTrackerWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("QuestTrackerWindowX"), PlayerPrefs.GetFloat("QuestTrackerWindowY"), 0);
            if (PlayerPrefs.HasKey("CombatLogWindowX") && PlayerPrefs.HasKey("CombatLogWindowY"))
                UIManager.MyInstance.MyCombatLogWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("CombatLogWindowX"), PlayerPrefs.GetFloat("CombatLogWindowY"), 0);

            if (PlayerPrefs.HasKey("MessageFeedManagerX") && PlayerPrefs.HasKey("MessageFeedManagerY"))
                MessageFeedManager.MyInstance.MessageFeedGameObject.transform.position = new Vector3(PlayerPrefs.GetFloat("MessageFeedManagerX"), PlayerPrefs.GetFloat("MessageFeedManagerY"), 0);

            if (PlayerPrefs.HasKey("FloatingCastBarControllerX") && PlayerPrefs.HasKey("FloatingCastBarControllerY")) {
                //Debug.Log("UIManager.MyInstance.MyFloatingCastBarController.transform.position: " + UIManager.MyInstance.MyFloatingCastBarController.transform.position);
                UIManager.MyInstance.MyFloatingCastBarController.transform.position = new Vector3(PlayerPrefs.GetFloat("FloatingCastBarControllerX"), PlayerPrefs.GetFloat("FloatingCastBarControllerY"), 0);
                //Debug.Log("UIManager.MyInstance.MyFloatingCastBarController.transform.position after set: " + UIManager.MyInstance.MyFloatingCastBarController.transform.position);
            }

            if (PlayerPrefs.HasKey("StatusEffectPanelControllerX") && PlayerPrefs.HasKey("StatusEffectPanelControllerY"))
                UIManager.MyInstance.MyStatusEffectPanelController.transform.position = new Vector3(PlayerPrefs.GetFloat("StatusEffectPanelControllerX"), PlayerPrefs.GetFloat("StatusEffectPanelControllerY"), 0);

            if (PlayerPrefs.HasKey("PlayerUnitFrameControllerX") && PlayerPrefs.HasKey("PlayerUnitFrameControllerY"))
                UIManager.MyInstance.MyPlayerUnitFrameController.transform.position = new Vector3(PlayerPrefs.GetFloat("PlayerUnitFrameControllerX"), PlayerPrefs.GetFloat("PlayerUnitFrameControllerY"), 0);

            if (PlayerPrefs.HasKey("FocusUnitFrameControllerX") && PlayerPrefs.HasKey("FocusUnitFrameControllerY"))
                UIManager.MyInstance.MyFocusUnitFrameController.transform.position = new Vector3(PlayerPrefs.GetFloat("FocusUnitFrameControllerX"), PlayerPrefs.GetFloat("FocusUnitFrameControllerY"), 0);

            if (PlayerPrefs.HasKey("MiniMapControllerX") && PlayerPrefs.HasKey("MiniMapControllerY"))
                UIManager.MyInstance.MyMiniMapController.transform.position = new Vector3(PlayerPrefs.GetFloat("MiniMapControllerX"), PlayerPrefs.GetFloat("MiniMapControllerY"), 0);

            if (PlayerPrefs.HasKey("XPBarControllerX") && PlayerPrefs.HasKey("XPBarControllerY"))
                UIManager.MyInstance.MyXPBarController.transform.position = new Vector3(PlayerPrefs.GetFloat("XPBarControllerX"), PlayerPrefs.GetFloat("XPBarControllerY"), 0);

            if (PlayerPrefs.HasKey("BottomPanelX") && PlayerPrefs.HasKey("BottomPanelY"))
                UIManager.MyInstance.MyBottomPanel.transform.position = new Vector3(PlayerPrefs.GetFloat("BottomPanelX"), PlayerPrefs.GetFloat("BottomPanelY"), 0);

            if (PlayerPrefs.HasKey("SidePanelX") && PlayerPrefs.HasKey("SidePanelY"))
                UIManager.MyInstance.MySidePanel.transform.position = new Vector3(PlayerPrefs.GetFloat("SidePanelX"), PlayerPrefs.GetFloat("SidePanelY"), 0);

            if (PlayerPrefs.HasKey("MouseOverWindowX") && PlayerPrefs.HasKey("MouseOverWindowY"))
                UIManager.MyInstance.MyMouseOverWindow.transform.position = new Vector3(PlayerPrefs.GetFloat("MouseOverWindowX"), PlayerPrefs.GetFloat("MouseOverWindowY"), 0);

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
            PlayerPrefs.SetFloat("QuestTrackerWindowX", UIManager.MyInstance.MyQuestTrackerWindow.transform.position.x);
            PlayerPrefs.SetFloat("QuestTrackerWindowY", UIManager.MyInstance.MyQuestTrackerWindow.transform.position.y);
            PlayerPrefs.SetFloat("CombatLogWindowX", UIManager.MyInstance.MyCombatLogWindow.transform.position.x);
            PlayerPrefs.SetFloat("CombatLogWindowY", UIManager.MyInstance.MyCombatLogWindow.transform.position.y);

            PlayerPrefs.SetFloat("MessageFeedManagerX", MessageFeedManager.MyInstance.MessageFeedGameObject.transform.position.x);
            PlayerPrefs.SetFloat("MessageFeedManagerY", MessageFeedManager.MyInstance.MessageFeedGameObject.transform.position.y);

            //Debug.Log("Saving FloatingCastBarController: " + UIManager.MyInstance.MyFloatingCastBarController.transform.position.x + "; " + UIManager.MyInstance.MyFloatingCastBarController.transform.position.y);
            PlayerPrefs.SetFloat("FloatingCastBarControllerX", UIManager.MyInstance.MyFloatingCastBarController.transform.position.x);
            PlayerPrefs.SetFloat("FloatingCastBarControllerY", UIManager.MyInstance.MyFloatingCastBarController.transform.position.y);

            PlayerPrefs.SetFloat("StatusEffectPanelControllerX", UIManager.MyInstance.MyStatusEffectPanelController.transform.position.x);
            PlayerPrefs.SetFloat("StatusEffectPanelControllerY", UIManager.MyInstance.MyStatusEffectPanelController.transform.position.y);

            PlayerPrefs.SetFloat("PlayerUnitFrameControllerX", UIManager.MyInstance.MyPlayerUnitFrameController.transform.position.x);
            PlayerPrefs.SetFloat("PlayerUnitFrameControllerY", UIManager.MyInstance.MyPlayerUnitFrameController.transform.position.y);

            PlayerPrefs.SetFloat("FocusUnitFrameControllerX", UIManager.MyInstance.MyFocusUnitFrameController.transform.position.x);
            PlayerPrefs.SetFloat("FocusUnitFrameControllerY", UIManager.MyInstance.MyFocusUnitFrameController.transform.position.y);

            PlayerPrefs.SetFloat("MiniMapControllerX", UIManager.MyInstance.MyMiniMapController.transform.position.x);
            PlayerPrefs.SetFloat("MiniMapControllerY", UIManager.MyInstance.MyMiniMapController.transform.position.y);

            PlayerPrefs.SetFloat("XPBarControllerX", UIManager.MyInstance.MyXPBarController.transform.position.x);
            PlayerPrefs.SetFloat("XPBarControllerY", UIManager.MyInstance.MyXPBarController.transform.position.y);

            PlayerPrefs.SetFloat("BottomPanelX", UIManager.MyInstance.MyBottomPanel.transform.position.x);
            PlayerPrefs.SetFloat("BottomPanelY", UIManager.MyInstance.MyBottomPanel.transform.position.y);

            PlayerPrefs.SetFloat("SidePanelX", UIManager.MyInstance.MySidePanel.transform.position.x);
            PlayerPrefs.SetFloat("SidePanelY", UIManager.MyInstance.MySidePanel.transform.position.y);

            PlayerPrefs.SetFloat("MouseOverWindowX", UIManager.MyInstance.MyMouseOverWindow.transform.position.x);
            PlayerPrefs.SetFloat("MouseOverWindowY", UIManager.MyInstance.MyMouseOverWindow.transform.position.y);

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
                replaceString = regex.Replace(SystemConfigurationManager.MyInstance.MyGameName, "");
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