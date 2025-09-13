using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class CharacterSaveManager : ConfiguredClass {

        private UnitController unitController;

        private AnyRPGSaveData saveData = null;

        private Dictionary<string, BehaviorSaveData> behaviorSaveDataDictionary = new Dictionary<string, BehaviorSaveData>();
        private Dictionary<string, DialogSaveData> dialogSaveDataDictionary = new Dictionary<string, DialogSaveData>();

        private Dictionary<string, SceneNodeSaveData> sceneNodeSaveDataDictionary = new Dictionary<string, SceneNodeSaveData>();

        private bool eventSubscriptionsInitialized = false;

        // game manager references
        private ActionBarManager actionBarManager = null;
        private SystemItemManager systemItemManager = null;
        private SaveManager saveManager = null;
        private LevelManager levelManager = null;

        public Dictionary<string, DialogSaveData> DialogSaveDataDictionary { get => dialogSaveDataDictionary; }

        public Dictionary<string, SceneNodeSaveData> SceneNodeSaveDataDictionary { get => sceneNodeSaveDataDictionary; set => sceneNodeSaveDataDictionary = value; }

        public AnyRPGSaveData SaveData { get => saveData; }

        public CharacterSaveManager(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
            //saveData = saveManager.CreateSaveData().SaveData;
            saveData = new AnyRPGSaveData();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            actionBarManager = systemGameManager.UIManager.ActionBarManager;
            systemItemManager = systemGameManager.SystemItemManager;
            saveManager = systemGameManager.SaveManager;
            levelManager = systemGameManager.LevelManager;
        }

        public void CreateEventSubscriptions() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized == false) {
                unitController.UnitEventController.OnLevelChanged += HandleLevelChanged;
                unitController.UnitEventController.OnGainXP += HandleGainXP;
                unitController.UnitEventController.OnNameChange += HandleNameChange;
                unitController.UnitEventController.OnFactionChange += HandleFactionChange;
                unitController.UnitEventController.OnRaceChange += HandleRaceChange;
                unitController.UnitEventController.OnClassChange += HandleClassChange;
                unitController.UnitEventController.OnSpecializationChange += HandleSpecializationChange;
                unitController.UnitEventController.OnLearnAbility += HandleLearnAbility;
                unitController.UnitEventController.OnUnlearnAbility += HandleUnlearnAbility;
                unitController.UnitEventController.OnLearnSkill += HandleLearnSkill;
                unitController.UnitEventController.OnUnLearnSkill += HandleUnLearnSkill;
                unitController.UnitEventController.OnResourceAmountChanged += HandleResourceAmountChanged;
                unitController.UnitEventController.OnBeforeDie += HandleBeforeDie;
                unitController.UnitEventController.OnReviveComplete += HandleReviveComplete;
                unitController.UnitEventController.OnRebuildModelAppearance += HandleRebuildModelAppearance;
                unitController.UnitEventController.OnAbandonQuest += HandleAbandonQuest;
                unitController.UnitEventController.OnAcceptQuest += HandleAcceptQuest;
                unitController.UnitEventController.OnTurnInQuest += HandleTurnInQuest;
                unitController.UnitEventController.OnMarkQuestComplete += HandleMarkQuestComplete;
                unitController.UnitEventController.OnMarkAchievementComplete += HandleMarkAchievementComplete;
                unitController.UnitEventController.OnQuestObjectiveStatusUpdated += HandleQuestObjectiveStatusUpdated;
                unitController.UnitEventController.OnAchievementObjectiveStatusUpdated += HandleAchievementObjectiveStatusUpdated;
                unitController.UnitEventController.OnSetQuestObjectiveCurrentAmount += HandleSetQuestObjectiveCurrentAmount;
                unitController.UnitEventController.OnSetAchievementObjectiveCurrentAmount += HandleSetAchievementObjectiveCurrentAmount;
                unitController.UnitEventController.OnAddItemToInventorySlot += HandleAddItemToInventorySlot;
                unitController.UnitEventController.OnRemoveItemFromInventorySlot += HandleRemoveItemFromInventorySlot;
                unitController.UnitEventController.OnAddItemToBankSlot += HandleAddItemToBankSlot;
                unitController.UnitEventController.OnRemoveItemFromBankSlot += HandleRemoveItemFromBankSlot;
                unitController.UnitEventController.OnAddBag += HandleAddBag;
                unitController.UnitEventController.OnRemoveBag += HandleRemoveBag;
                unitController.UnitEventController.OnLearnRecipe += HandleLearnRecipe;
                unitController.UnitEventController.OnUnlearnRecipe += HandleUnlearnRecipe;
                unitController.UnitEventController.OnReputationChange += HandleReputationChange;
                unitController.UnitEventController.OnAddEquipment += HandleAddEquipment;
                unitController.UnitEventController.OnRemoveEquipment += HandleRemoveEquipment;
                unitController.UnitEventController.OnCurrencyChange += HandleCurrencyChange;
                unitController.UnitEventController.OnStatusEffectAdd += HandleStatusEffectAdd;
                unitController.UnitEventController.OnAddStatusEffectStack += HandleAddStatusEffectStack;
                unitController.UnitEventController.OnCancelStatusEffect += HandleCancelStatusEffect;
                unitController.UnitEventController.OnAddPet += HandleAddPet;
                unitController.UnitEventController.OnSetReputationAmount += HandleSetReputationAmount;
                unitController.UnitEventController.OnActivateMountedState += HandleActivateMountedState;
                eventSubscriptionsInitialized = true;
            }
        }

        public void SaveGameData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveGameData()");

            saveData.unitProfileName = unitController.UnitProfile.ResourceName;
            saveData.playerName = unitController.BaseCharacter.CharacterName;
            saveData.PlayerLevel = unitController.CharacterStats.Level;
            saveData.currentExperience = unitController.CharacterStats.CurrentXP;
            saveData.isDead = !unitController.CharacterStats.IsAlive;
            //saveData.isMounted = unitController.IsMounted;
            if (unitController.BaseCharacter.Faction != null) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveGameData() faction: {unitController.BaseCharacter.Faction.ResourceName}");
                saveData.playerFaction = unitController.BaseCharacter.Faction.ResourceName;
            } else {
                //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveGameData() no faction");
                saveData.playerFaction = string.Empty;
            }
            if (unitController.BaseCharacter.CharacterClass != null) {
                saveData.characterClass = unitController.BaseCharacter.CharacterClass.ResourceName;
            } else {
                saveData.characterClass = string.Empty;
            }
            if (unitController.BaseCharacter.ClassSpecialization != null) {
                saveData.classSpecialization = unitController.BaseCharacter.ClassSpecialization.ResourceName;
            } else {
                saveData.classSpecialization = string.Empty;
            }
            if (unitController.BaseCharacter.CharacterRace != null) {
                saveData.characterRace = unitController.BaseCharacter.CharacterRace.ResourceName;
            } else {
                saveData.characterRace = string.Empty;
            }

            SavePlayerLocation();
            saveData.CurrentScene = unitController.gameObject.scene.name;

            SaveResourcePowerData();
            SaveAppearanceData();

            SaveQuestData();
            SaveAchievementData();

            SaveDialogData();
            SaveBehaviorData();
            SaveActionBarData();
            SaveInventorySlotData();
            SaveBankSlotData();
            SaveEquippedBagData();
            SaveEquippedBankBagData();
            SaveAbilityData();
            SaveSkillData();
            SaveRecipeData();
            SaveReputationData();
            SaveEquipmentData();
            SaveCurrencyData();
            SaveSceneNodeData();
            SaveStatusEffectData();
            SavePetData();
        }

        private void HandleActivateMountedState(UnitController controller) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.HandleActivateMountedState()");

            saveData.isMounted = true;
        }


        private void HandleReviveComplete(UnitController controller) {
            saveData.isDead = false;
        }

        private void HandleBeforeDie(UnitController controller) {
            saveData.isDead = true;
        }

        private void HandleSetReputationAmount(Faction faction, float amount) {
            SaveReputationData();
        }

        public void HandleAddPet(UnitProfile profile) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.HandleAddPet({profile.ResourceName})");

            SavePetData();
        }

        public void HandleAddStatusEffectStack(string obj) {
            SaveStatusEffectData();
        }

        public void HandleCancelStatusEffect(StatusEffectProperties properties) {
            SaveStatusEffectData();
        }

        public void HandleStatusEffectAdd(UnitController sourceUnitController, StatusEffectNode node) {
            SaveStatusEffectData();
        }

        public void HandleCurrencyChange(string currencyResourceName, int amount) {
            SaveCurrencyData();
        }

        public void HandleRemoveEquipment(EquipmentSlotProfile profile, InstantiatedEquipment equipment) {
            SaveEquipmentData();
        }

        public void HandleAddEquipment(EquipmentSlotProfile profile, InstantiatedEquipment equipment) {
            SaveEquipmentData();
        }

        public void HandleReputationChange(UnitController sourceUnitController) {
            SaveReputationData();
        }

        public void HandleUnlearnRecipe(Recipe recipe) {
            SaveRecipeData();
        }

        public void HandleLearnRecipe(Recipe recipe) {
            SaveRecipeData();
        }

        private void HandleRemoveBag(InstantiatedBag bag) {
            SaveEquippedBagData();
            SaveEquippedBankBagData();
        }

        private void HandleAddBag(InstantiatedBag bag, BagNode node) {
            SaveEquippedBagData();
            SaveEquippedBankBagData();
        }


        private void HandleRemoveItemFromBankSlot(InventorySlot slot, InstantiatedItem item) {
            SaveBankSlotData();
        }

        private void HandleAddItemToBankSlot(InventorySlot slot, InstantiatedItem item) {
            SaveBankSlotData();
        }


        private void HandleRemoveItemFromInventorySlot(InventorySlot slot, InstantiatedItem item) {
            SaveInventorySlotData();
        }

        private void HandleAddItemToInventorySlot(InventorySlot slot, InstantiatedItem item) {
            SaveInventorySlotData();
        }


        private void HandleSetQuestObjectiveCurrentAmount(string arg1, string arg2, string arg3, int amount) {
            SaveQuestData();
            //SaveAchievementData();
        }

        private void HandleSetAchievementObjectiveCurrentAmount(string arg1, string arg2, string arg3, int amount) {
            //SaveQuestData();
            SaveAchievementData();
        }

        private void HandleQuestObjectiveStatusUpdated(UnitController controller, Quest quest) {
            SaveQuestData();
        }

        private void HandleAchievementObjectiveStatusUpdated(UnitController controller, Achievement achievement) {
            SaveQuestData();
            SaveAchievementData();
        }

        private void HandleMarkQuestComplete(UnitController controller, QuestBase questBase) {
            SaveQuestData();
        }

        private void HandleMarkAchievementComplete(UnitController controller, Achievement achievement) {
            SaveAchievementData();
        }

        private void HandleTurnInQuest(UnitController controller, QuestBase questBase) {
            SaveQuestData();
            SaveAchievementData();
        }

        private void HandleAcceptQuest(UnitController controller, QuestBase questBase) {
            SaveQuestData();
            SaveAchievementData();
        }

        private void HandleAbandonQuest(UnitController controller, QuestBase questBase) {
            SaveQuestData();
            SaveAchievementData();
        }

        private void HandleRebuildModelAppearance() {
            SaveAppearanceData();
        }

        private void HandleResourceAmountChanged(PowerResource resource, int arg2, int arg3) {
            SaveResourcePowerData();
        }

        private void HandleUnLearnSkill(UnitController controller, Skill skill) {
            SaveSkillData();
        }

        private void HandleLearnSkill(UnitController controller, Skill skill) {
            SaveSkillData();
        }

        private void HandleUnlearnAbility(AbilityProperties abilityProperties) {
            SaveAbilityData();
        }

        private void HandleLearnAbility(UnitController controller, AbilityProperties properties) {
            SaveAbilityData();
        }

        private void HandleSpecializationChange(UnitController sourceUnitController, ClassSpecialization newSpecialization, ClassSpecialization oldSpecialization) {
            if (newSpecialization != null) {
                saveData.classSpecialization = newSpecialization.ResourceName;
            } else {
                saveData.classSpecialization = string.Empty;
            }
        }

        private void HandleClassChange(UnitController sourceUnitController, CharacterClass newClass, CharacterClass oldClass) {
            if (newClass != null) {
                saveData.characterClass = newClass.ResourceName;
            } else {
                saveData.characterClass = string.Empty;
            }
        }

        private void HandleRaceChange(CharacterRace newRace, CharacterRace oldRace) {
            if (newRace != null) {
                saveData.characterRace = newRace.ResourceName;
            } else {
                saveData.characterRace = string.Empty;
            }
        }

        private void HandleFactionChange(Faction newFaction, Faction oldFaction) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSaveManager.HandleFactionChange(new: {newFaction?.ResourceName}, old: {oldFaction?.ResourceName})");

            if (newFaction != null) {
                saveData.playerFaction = newFaction.ResourceName;
            } else {
                saveData.playerFaction = string.Empty;
            }
        }

        private void HandleNameChange(string newName) {
            saveData.playerName = newName;
        }

        private void HandleGainXP(UnitController sourceUnitController, int gainedXP, int currentXP) {
            saveData.currentExperience = currentXP;
        }

        public void HandleLevelChanged(int newLevel) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.HandleLevelChanged({newLevel})");

            saveData.PlayerLevel = newLevel;
        }

        public void SetSaveData(CharacterRequestData characterRequestData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SetSaveData()");

            this.saveData = characterRequestData.saveData;
            characterRequestData.characterConfigurationRequest.characterName = saveData.playerName;
            characterRequestData.characterConfigurationRequest.characterAppearanceData = new CharacterAppearanceData(saveData);
            characterRequestData.characterConfigurationRequest.unitLevel = saveData.PlayerLevel;
            characterRequestData.characterConfigurationRequest.currentExperience = saveData.currentExperience;
            characterRequestData.characterConfigurationRequest.isDead = saveData.isDead;
            if (saveData.characterClass != string.Empty) {
                characterRequestData.characterConfigurationRequest.characterClass = systemDataFactory.GetResource<CharacterClass>(saveData.characterClass);
            } else {
                characterRequestData.characterConfigurationRequest.characterClass = null;
            }
            if (saveData.classSpecialization != string.Empty) {
                characterRequestData.characterConfigurationRequest.classSpecialization = systemDataFactory.GetResource<ClassSpecialization>(saveData.classSpecialization);
            } else {
                characterRequestData.characterConfigurationRequest.classSpecialization = null;
            }
            if (saveData.characterRace != string.Empty) {
                characterRequestData.characterConfigurationRequest.characterRace = systemDataFactory.GetResource<CharacterRace>(saveData.characterRace);
            } else {
                characterRequestData.characterConfigurationRequest.characterRace = null;
            }
            if (saveData.playerFaction != string.Empty) {
                characterRequestData.characterConfigurationRequest.faction = systemDataFactory.GetResource<Faction>(saveData.playerFaction);
            } else {
                characterRequestData.characterConfigurationRequest.faction = null;
            }
        }

        public DialogSaveData GetDialogSaveData(Dialog dialog) {
            if (dialogSaveDataDictionary.ContainsKey(dialog.ResourceName)) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.GetDialogSaveData({dialog.ResourceName}): dialogSaveData size {dialogSaveDataDictionary[dialog.resourceName].dialogNodeShown.Count}");
                return dialogSaveDataDictionary[dialog.ResourceName];
            } else {
                DialogSaveData saveData = new DialogSaveData();
                saveData.dialogNodeShown = new List<bool>(new bool[dialog.DialogNodes.Count]);
                //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.GetDialogSaveData({dialog.ResourceName}): initialized list size {dialog.DialogNodes.Count} : {saveData.dialogNodeShown.Count}");
                saveData.DialogName = dialog.ResourceName;
                dialogSaveDataDictionary.Add(dialog.ResourceName, saveData);
                //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.GetDialogSaveData({dialog.ResourceName}): dialogSaveData size {dialogSaveDataDictionary[dialog.resourceName].dialogNodeShown.Count}");
                return saveData;
            }
            //return saveData;
        }

        public BehaviorSaveData GetBehaviorSaveData(BehaviorProfile behaviorProfile) {
            BehaviorSaveData saveData;
            if (behaviorSaveDataDictionary.ContainsKey(behaviorProfile.ResourceName)) {
                saveData = behaviorSaveDataDictionary[behaviorProfile.ResourceName];
            } else {
                saveData = new BehaviorSaveData();
                saveData.BehaviorName = behaviorProfile.ResourceName;
                behaviorSaveDataDictionary.Add(behaviorProfile.ResourceName, saveData);
            }
            return saveData;
        }

        public bool GetDialogNodeShown(Dialog dialog, int index) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.GetDialogNodeShown({dialog.ResourceName}, {index})");
            DialogSaveData saveData = GetDialogSaveData(dialog);
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.GetDialogNodeShown({dialog.ResourceName}, {index}) count: {saveData.dialogNodeShown.Count}");
            return saveData.dialogNodeShown[index];
        }

        public void SetDialogNodeShown(Dialog dialog, bool value, int index) {
            DialogSaveData saveData = GetDialogSaveData(dialog);
            saveData.dialogNodeShown[index] = value;
            dialogSaveDataDictionary[dialog.ResourceName] = saveData;
            SaveDialogData();
        }

        public void ResetDialogNodes(Dialog dialog) {
            DialogSaveData saveData = GetDialogSaveData(dialog);
            saveData.dialogNodeShown = new List<bool>(new bool[dialog.DialogNodes.Count]);
            dialogSaveDataDictionary[dialog.ResourceName] = saveData;
            SaveDialogData();
        }

        public SceneNodeSaveData GetSceneNodeSaveData(SceneNode sceneNode) {
            SceneNodeSaveData saveData;
            if (sceneNodeSaveDataDictionary.ContainsKey(sceneNode.ResourceName)) {
                saveData = sceneNodeSaveDataDictionary[sceneNode.ResourceName];
            } else {
                saveData = new SceneNodeSaveData();
                saveData.persistentObjects = new List<PersistentObjectSaveData>();
                saveData.SceneName = sceneNode.ResourceName;
                sceneNodeSaveDataDictionary.Add(sceneNode.ResourceName, saveData);
            }
            return saveData;
        }


        public void LoadSaveDataToCharacter() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadSaveDataToCharacter()");

            //SetSaveData(saveData);

            LoadDialogData(saveData);
            LoadBehaviorData(saveData);
            LoadSceneNodeData(saveData);

            // complex data
            LoadEquippedBagData(saveData);
            LoadInventorySlotData(saveData);
            LoadBankSlotData(saveData);
            LoadAbilityData(saveData);

            // testing - move here to prevent learning auto-attack ability twice
            LoadEquipmentData(saveData);

            LoadSkillData(saveData);
            LoadRecipeData(saveData);
            LoadReputationData(saveData);

            // test loading this earlier to avoid having duplicates on bars
            LoadActionBarData(saveData);

            LoadCurrencyData(saveData);
            LoadStatusEffectData();
            LoadPetData(saveData);

            // set resources after equipment loaded for modifiers
            LoadResourcePowerData(saveData);

            // quest data gets loaded last because it could rely on other data such as dialog completion status, which don't get saved because they are inferred
            LoadQuestData(saveData);
            LoadAchievementData(saveData);
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

        public void LoadQuestData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.Savemanager.LoadQuestData()");

            unitController.CharacterQuestLog.QuestSaveDataDictionary.Clear();
            unitController.CharacterQuestLog.QuestObjectiveSaveDataDictionary.Clear();
            foreach (QuestSaveData questSaveData in anyRPGSaveData.questSaveData) {

                if (questSaveData.QuestName == null || questSaveData.QuestName == string.Empty) {
                    // don't load invalid quest data
                    continue;
                }
                unitController.CharacterQuestLog.QuestSaveDataDictionary.Add(questSaveData.QuestName, questSaveData);

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

                unitController.CharacterQuestLog.QuestObjectiveSaveDataDictionary.Add(questSaveData.QuestName, objectiveDictionary);
            }

            foreach (QuestSaveData questSaveData in anyRPGSaveData.questSaveData) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadQuestData(): loading questsavedata");
                unitController.CharacterQuestLog.LoadQuest(questSaveData);
            }
        }

        public void LoadAchievementData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.Savemanager.LoadAchievementData()");

            unitController.CharacterQuestLog.AchievementSaveDataDictionary.Clear();
            unitController.CharacterQuestLog.AchievementObjectiveSaveDataDictionary.Clear();
            foreach (QuestSaveData achievementSaveData in anyRPGSaveData.achievementSaveData) {

                if (achievementSaveData.QuestName == null || achievementSaveData.QuestName == string.Empty) {
                    // don't load invalid quest data
                    continue;
                }
                unitController.CharacterQuestLog.AchievementSaveDataDictionary.Add(achievementSaveData.QuestName, achievementSaveData);

                Dictionary<string, Dictionary<string, QuestObjectiveSaveData>> objectiveDictionary = new Dictionary<string, Dictionary<string, QuestObjectiveSaveData>>();

                // add objectives to dictionary
                foreach (QuestObjectiveSaveData achievementObjectiveSaveData in achievementSaveData.questObjectives) {
                    // perform null check to allow opening of older save files without null reference
                    if (achievementObjectiveSaveData.ObjectiveType != null && achievementObjectiveSaveData.ObjectiveType != string.Empty) {
                        if (!objectiveDictionary.ContainsKey(achievementObjectiveSaveData.ObjectiveType)) {
                            objectiveDictionary.Add(achievementObjectiveSaveData.ObjectiveType, new Dictionary<string, QuestObjectiveSaveData>());
                        }
                        objectiveDictionary[achievementObjectiveSaveData.ObjectiveType].Add(achievementObjectiveSaveData.ObjectiveName, achievementObjectiveSaveData);
                    }
                }

                unitController.CharacterQuestLog.AchievementObjectiveSaveDataDictionary.Add(achievementSaveData.QuestName, objectiveDictionary);
            }

            foreach (QuestSaveData questSaveData in anyRPGSaveData.achievementSaveData) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadQuestData(): loading questsavedata");
                unitController.CharacterQuestLog.LoadAchievement(questSaveData);
            }
        }
        public void LoadResourcePowerData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadResourcePowerData()");

            foreach (ResourcePowerSaveData resourcePowerSaveData in anyRPGSaveData.resourcePowerSaveData) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadResourcePowerData(): loading questsavedata");
                //unitController.CharacterStats.SetResourceAmount(resourcePowerSaveData.ResourceName, resourcePowerSaveData.amount);
                unitController.CharacterStats.LoadResourceAmount(resourcePowerSaveData.ResourceName, resourcePowerSaveData.amount);
            }

        }

        public void LoadStatusEffectData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadStatusEffectData()");
            foreach (StatusEffectSaveData statusEffectSaveData in saveData.statusEffectSaveData) {
                unitController.CharacterAbilityManager.ApplySavedStatusEffects(statusEffectSaveData);
            }
        }

        public void LoadCurrencyData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadCurrencyData()");
            foreach (CurrencySaveData currencySaveData in anyRPGSaveData.currencySaveData) {
                unitController.CharacterCurrencyManager.AddCurrency(systemDataFactory.GetResource<Currency>(currencySaveData.CurrencyName), currencySaveData.Amount);
            }
        }


        public void LoadPetData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSaveManager.LoadPetData()");

            foreach (PetSaveData petSaveData in anyRPGSaveData.petSaveData) {
                if (petSaveData.PetName != string.Empty) {
                    unitController.CharacterPetManager.AddPet(petSaveData.PetName);
                }
            }

        }

        public void LoadEquippedBagData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadEquippedBagData()");
            unitController.CharacterInventoryManager.LoadEquippedBagData(anyRPGSaveData.equippedBagSaveData, false);
            unitController.CharacterInventoryManager.LoadEquippedBagData(anyRPGSaveData.equippedBankBagSaveData, true);
        }

        public void LoadInventorySlotData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadInventorySlotData()");
            int counter = 0;
            foreach (InventorySlotSaveData inventorySlotSaveData in anyRPGSaveData.inventorySlotSaveData) {
                LoadSlotData(inventorySlotSaveData, counter, false);
                counter++;
            }
        }

        public void LoadBankSlotData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadBankSlotData()");

            int counter = 0;
            foreach (InventorySlotSaveData inventorySlotSaveData in anyRPGSaveData.bankSlotSaveData) {
                LoadSlotData(inventorySlotSaveData, counter, true);
                counter++;
            }
        }

        private void LoadSlotData(InventorySlotSaveData inventorySlotSaveData, int counter, bool bank) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadSlotData({inventorySlotSaveData.ItemName}, {counter}, {bank}) stack: {inventorySlotSaveData.stackCount}");

            if (inventorySlotSaveData.ItemName != string.Empty && inventorySlotSaveData.ItemName != null) {
                for (int i = 0; i < inventorySlotSaveData.stackCount; i++) {
                    InstantiatedItem newInstantiatedItem = unitController.CharacterInventoryManager.GetNewInstantiatedItemFromSaveData(inventorySlotSaveData);
                    if (newInstantiatedItem == null) {
                        Debug.LogWarning("CharacterSavemanager.LoadInventorySlotData(): COULD NOT LOAD ITEM FROM ITEM MANAGER: {inventorySlotSaveData.ItemName}");
                    } else {
                        if (bank == true) {
                            unitController.CharacterInventoryManager.AddBankItem(newInstantiatedItem, counter);
                        } else {
                            unitController.CharacterInventoryManager.AddInventoryItem(newInstantiatedItem, counter);
                        }
                    }
                }
            }
        }

        public void LoadAbilityData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadAbilityData()");

            foreach (AbilitySaveData abilitySaveData in anyRPGSaveData.abilitySaveData) {
                if (abilitySaveData.AbilityName != string.Empty) {
                    unitController.CharacterAbilityManager.LoadAbility(abilitySaveData.AbilityName);
                }
            }
        }

        public void LoadEquipmentData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadEquipmentData()");

            foreach (EquipmentSaveData equipmentSaveData in anyRPGSaveData.equipmentSaveData) {
                if (equipmentSaveData.EquipmentName != string.Empty) {
                    InstantiatedEquipment newInstantiatedEquipment = unitController.CharacterInventoryManager.GetNewInstantiatedEquipmentFromSaveData(equipmentSaveData);
                    if (newInstantiatedEquipment != null) {
                        unitController.CharacterEquipmentManager.Equip(newInstantiatedEquipment, null);
                    }
                }
            }
        }

        public void LoadReputationData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadReputationData()");
            //int counter = 0;
            foreach (ReputationSaveData reputationSaveData in anyRPGSaveData.reputationSaveData) {
                FactionDisposition factionDisposition = new FactionDisposition();
                factionDisposition.Faction = systemDataFactory.GetResource<Faction>(reputationSaveData.ReputationName);
                factionDisposition.disposition = reputationSaveData.Amount;
                unitController.CharacterFactionManager.LoadReputation(factionDisposition.Faction, (int)factionDisposition.disposition);
                //counter++;
            }
        }


        public void LoadSkillData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadSkillData()");
            foreach (SkillSaveData skillSaveData in anyRPGSaveData.skillSaveData) {
                unitController.CharacterSkillManager.LoadSkill(skillSaveData.SkillName);
            }
        }

        public void LoadRecipeData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadRecipeData()");
            foreach (RecipeSaveData recipeSaveData in anyRPGSaveData.recipeSaveData) {
                unitController.CharacterRecipeManager.LoadRecipe(recipeSaveData.RecipeName);
            }
        }

        public void LoadActionBarData(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadActionBarData()");

            LoadMouseActionButtonData(anyRPGSaveData.actionBarSaveData);
            LoadGamepadActionButtonData(anyRPGSaveData.gamepadActionBarSaveData);
            actionBarManager.SetGamepadActionButtonSet(anyRPGSaveData.GamepadActionButtonSet, false);
        }

        private void LoadMouseActionButtonData(List<ActionBarSaveData> actionBarSaveDatas) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadActionButtonData(saveDataCount: {actionBarSaveDatas.Count})");

            IUseable useable = null;
            int counter = 0;
            foreach (ActionBarSaveData actionBarSaveData in actionBarSaveDatas) {
                useable = null;
                if (actionBarSaveData.isItem == true) {
                    // find item in bag
                    //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadActionBarData(): searching for usable(" + actionBarSaveData.MyName + ") in inventory");
                    //useable = systemDataFactory.GetResource<Item>(actionBarSaveData.DisplayName);
                    useable = systemItemManager.GetNewInstantiatedItem(actionBarSaveData.DisplayName);
                } else {
                    // find ability from system ability manager
                    //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadActionBarData(): searching for usable in ability manager");
                    if (actionBarSaveData.DisplayName != null && actionBarSaveData.DisplayName != string.Empty) {
                        useable = systemDataFactory.GetResource<Ability>(actionBarSaveData.DisplayName).AbilityProperties;
                    } else {
                        //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadActionBarData(): saved action bar had no name");
                    }
                    if (actionBarSaveData.savedName != null && actionBarSaveData.savedName != string.Empty) {
                        IUseable savedUseable = systemDataFactory.GetResource<Ability>(actionBarSaveData.savedName).AbilityProperties;
                        if (savedUseable != null) {
                            unitController.CharacterActionBarManager.MouseActionButtons[counter].SavedUseable = savedUseable;
                        }
                    }
                }
                if (useable != null) {
                    unitController.CharacterActionBarManager.SetMouseActionButton(useable, counter);
                } else {
                    //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadActionBarData(): no usable set on this actionbutton");
                    // testing remove things that weren't saved, it will prevent duplicate abilities if they are moved
                    // this means if new abilities are added to a class/etc between play sessions they won't be on the bars
                    unitController.CharacterActionBarManager.UnSetMouseActionButton(counter);
                }
                counter++;
                if (counter >= unitController.CharacterActionBarManager.MouseActionButtons.Count) {
                    //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadActionBarData(): counter exceeded action buttons count, breaking");
                    break; // prevent out of bounds
                }
            }
        }

        private void LoadGamepadActionButtonData(List<ActionBarSaveData> actionBarSaveDatas) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadGamepadActionButtonData(saveDataCount: {actionBarSaveDatas.Count})");

            IUseable useable = null;
            int counter = 0;
            foreach (ActionBarSaveData actionBarSaveData in actionBarSaveDatas) {
                useable = null;
                if (actionBarSaveData.isItem == true) {
                    // find item in bag
                    //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadActionBarData(): searching for usable(" + actionBarSaveData.MyName + ") in inventory");
                    //useable = systemDataFactory.GetResource<Item>(actionBarSaveData.DisplayName);
                    useable = systemItemManager.GetNewInstantiatedItem(actionBarSaveData.DisplayName);
                } else {
                    // find ability from system ability manager
                    //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadActionBarData(): searching for usable in ability manager");
                    if (actionBarSaveData.DisplayName != null && actionBarSaveData.DisplayName != string.Empty) {
                        useable = systemDataFactory.GetResource<Ability>(actionBarSaveData.DisplayName).AbilityProperties;
                    } else {
                        //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadActionBarData(): saved action bar had no name");
                    }
                    if (actionBarSaveData.savedName != null && actionBarSaveData.savedName != string.Empty) {
                        IUseable savedUseable = systemDataFactory.GetResource<Ability>(actionBarSaveData.savedName).AbilityProperties;
                        if (savedUseable != null) {
                            unitController.CharacterActionBarManager.GamepadActionButtons[counter].SavedUseable = savedUseable;
                        }
                    }
                }
                if (useable != null) {
                    unitController.CharacterActionBarManager.SetGamepadActionButton(useable, counter);
                } else {
                    //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadActionBarData(): no usable set on this actionbutton");
                    // testing remove things that weren't saved, it will prevent duplicate abilities if they are moved
                    // this means if new abilities are added to a class/etc between play sessions they won't be on the bars
                    unitController.CharacterActionBarManager.UnSetGamepadActionButton(counter);
                }
                counter++;
            }
        }

        public void VisitSceneNode(SceneNode sceneNode) {
            SceneNodeSaveData saveData = GetSceneNodeSaveData(sceneNode);
            if (saveData.visited == false) {
                saveData.visited = true;
                sceneNodeSaveDataDictionary[saveData.SceneName] = saveData;
                SaveSceneNodeData();
            }
        }

        public bool IsSceneNodeVisited(SceneNode sceneNode) {
            SceneNodeSaveData saveData = GetSceneNodeSaveData(sceneNode);
            return saveData.visited;
        }

        public void SavePlayerLocation() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SavePlayerLocation()");

            saveData.OverrideLocation = true;
            saveData.OverrideRotation = true;
            saveData.PlayerLocationX = unitController.transform.position.x;
            saveData.PlayerLocationY = unitController.transform.position.y;
            saveData.PlayerLocationZ = unitController.transform.position.z;
            saveData.PlayerRotationX = unitController.transform.forward.x;
            saveData.PlayerRotationY = unitController.transform.forward.y;
            saveData.PlayerRotationZ = unitController.transform.forward.z;

        }

        public void SaveResourcePowerData() {
            saveData.resourcePowerSaveData.Clear();
            foreach (PowerResource powerResource in unitController.CharacterStats.PowerResourceDictionary.Keys) {
                ResourcePowerSaveData resourcePowerData = new ResourcePowerSaveData();
                resourcePowerData.ResourceName = powerResource.ResourceName;
                resourcePowerData.amount = unitController.CharacterStats.PowerResourceDictionary[powerResource].currentValue;
                saveData.resourcePowerSaveData.Add(resourcePowerData);
            }
        }

        public void SaveAppearanceData() {
            unitController.UnitModelController.SaveAppearanceSettings(/*this,*/ saveData);
        }

        public void SaveQuestData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveQuestData()");

            saveData.questSaveData.Clear();
            foreach (QuestSaveData questSaveData in unitController.CharacterQuestLog.QuestSaveDataDictionary.Values) {
                QuestSaveData finalSaveData = questSaveData;
                if (unitController.CharacterQuestLog.QuestObjectiveSaveDataDictionary.ContainsKey(questSaveData.QuestName)) {

                    List<QuestObjectiveSaveData> questObjectiveSaveDataList = new List<QuestObjectiveSaveData>();
                    foreach (string typeName in unitController.CharacterQuestLog.QuestObjectiveSaveDataDictionary[questSaveData.QuestName].Keys) {
                        foreach (QuestObjectiveSaveData saveData in unitController.CharacterQuestLog.QuestObjectiveSaveDataDictionary[questSaveData.QuestName][typeName].Values) {
                            questObjectiveSaveDataList.Add(saveData);
                        }
                    }
                    finalSaveData.questObjectives = questObjectiveSaveDataList;
                }
                finalSaveData.inLog = unitController.CharacterQuestLog.HasQuest(questSaveData.QuestName);
                saveData.questSaveData.Add(finalSaveData);
            }
        }

        public void SaveAchievementData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveAchievementData()");

            saveData.achievementSaveData.Clear();
            foreach (QuestSaveData achievementSaveData in unitController.CharacterQuestLog.AchievementSaveDataDictionary.Values) {
                QuestSaveData finalSaveData = achievementSaveData;
                if (unitController.CharacterQuestLog.AchievementObjectiveSaveDataDictionary.ContainsKey(achievementSaveData.QuestName)) {

                    List<QuestObjectiveSaveData> achievementObjectiveSaveDataList = new List<QuestObjectiveSaveData>();
                    foreach (string typeName in unitController.CharacterQuestLog.AchievementObjectiveSaveDataDictionary[achievementSaveData.QuestName].Keys) {
                        foreach (QuestObjectiveSaveData saveData in unitController.CharacterQuestLog.AchievementObjectiveSaveDataDictionary[achievementSaveData.QuestName][typeName].Values) {
                            achievementObjectiveSaveDataList.Add(saveData);
                        }

                    }

                    finalSaveData.questObjectives = achievementObjectiveSaveDataList;
                }
                finalSaveData.inLog = unitController.CharacterQuestLog.HasAchievement(achievementSaveData.QuestName);
                // print if finalsavadata complete and turnedin are true
                //Debug.Log($"CharacterSavemanager.SaveAchievementData(): Saving achievement {finalSaveData.QuestName} complete: {finalSaveData.markedComplete} turnedIn: {finalSaveData.turnedIn}");
                saveData.achievementSaveData.Add(finalSaveData);
            }
        }

        public void SaveDialogData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveDialogData()");
            saveData.dialogSaveData.Clear();
            foreach (DialogSaveData dialogSaveData in dialogSaveDataDictionary.Values) {
                saveData.dialogSaveData.Add(dialogSaveData);
            }
        }

        public void SaveBehaviorData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveQuestData()");
            saveData.behaviorSaveData.Clear();
            foreach (BehaviorSaveData behaviorSaveData in behaviorSaveDataDictionary.Values) {
                saveData.behaviorSaveData.Add(behaviorSaveData);
            }
        }

        public void SaveSceneNodeData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveSceneNodeData()");

            saveData.sceneNodeSaveData.Clear();
            foreach (SceneNodeSaveData sceneNodeSaveData in sceneNodeSaveDataDictionary.Values) {
                saveData.sceneNodeSaveData.Add(sceneNodeSaveData);
            }
        }

        public void SaveStatusEffectData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveSceneNodeData()");
            saveData.statusEffectSaveData.Clear();
            foreach (StatusEffectNode statusEffectNode in unitController.CharacterStats.StatusEffects.Values) {
                if (statusEffectNode.StatusEffect.ClassTrait == false
                    && statusEffectNode.StatusEffect.SaveEffect == true
                    && statusEffectNode.AbilityEffectContext.AbilityCaster == (unitController as IAbilityCaster)) {
                    StatusEffectSaveData statusEffectSaveData = new StatusEffectSaveData();
                    statusEffectSaveData.StatusEffectName = statusEffectNode.StatusEffect.DisplayName;
                    statusEffectSaveData.remainingSeconds = (int)statusEffectNode.GetRemainingDuration();
                    saveData.statusEffectSaveData.Add(statusEffectSaveData);
                }
            }
        }

        public void SaveActionBarData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveActionBarData()");

            saveData.GamepadActionButtonSet = actionBarManager.CurrentActionBarSet;
            saveData.actionBarSaveData.Clear();
            saveData.gamepadActionBarSaveData.Clear();
            foreach (ActionButtonNode actionButtonNode in unitController.CharacterActionBarManager.MouseActionButtons) {
                SaveActionButtonNodeSaveData(actionButtonNode, saveData.actionBarSaveData);
            }
            foreach (ActionButtonNode actionButtonNode in unitController.CharacterActionBarManager.GamepadActionButtons) {
                SaveActionButtonNodeSaveData(actionButtonNode, saveData.gamepadActionBarSaveData);
            }
        }

        /*
        private void SaveActionButtonSaveData(ActionButton actionButton, List<ActionBarSaveData> actionBarSaveDataList) {
            Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveActionButtonSaveData({actionButton.name}");

            ActionBarSaveData actionBarSaveData = new ActionBarSaveData();
            actionBarSaveData.DisplayName = (actionButton.Useable == null ? string.Empty : (actionButton.Useable as IDescribable).DisplayName);
            actionBarSaveData.savedName = (actionButton.SavedUseable == null ? string.Empty : (actionButton.SavedUseable as IDescribable).DisplayName);
            actionBarSaveData.isItem = (actionButton.Useable == null ? false : (actionButton.Useable is InstantiatedItem ? true : false));
            actionBarSaveDataList.Add(actionBarSaveData);
        }
        */

        private void SaveActionButtonNodeSaveData(ActionButtonNode actionButtonNode, List<ActionBarSaveData> actionBarSaveDataList) {
            ActionBarSaveData actionBarSaveData = new ActionBarSaveData();
            actionBarSaveData.DisplayName = (actionButtonNode.Useable == null ? string.Empty : (actionButtonNode.Useable as IDescribable).ResourceName);
            actionBarSaveData.savedName = (actionButtonNode.SavedUseable == null ? string.Empty : (actionButtonNode.SavedUseable as IDescribable).ResourceName);
            actionBarSaveData.isItem = (actionButtonNode.Useable == null ? false : (actionButtonNode.Useable is InstantiatedItem ? true : false));
            actionBarSaveDataList.Add(actionBarSaveData);
        }

        private InventorySlotSaveData GetSlotSaveData(InventorySlot inventorySlot) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.GetSlotSaveData(): getting slot save data for slot with item {(inventorySlot.InstantiatedItem != null ? inventorySlot.InstantiatedItem.ResourceName : "empty")}");

            InventorySlotSaveData saveData = new InventorySlotSaveData();
            if (inventorySlot.InstantiatedItem != null) {
                saveData = inventorySlot.InstantiatedItem.GetSlotSaveData();
            } else {
                saveData = saveManager.GetEmptySlotSaveData();
            }
            saveData.stackCount = (inventorySlot.InstantiatedItem == null ? 0 : inventorySlot.Count);
            return saveData;
        }

        public void SaveInventorySlotData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveInventorySlotData()");
            saveData.inventorySlotSaveData.Clear();
            foreach (InventorySlot inventorySlot in unitController.CharacterInventoryManager.InventorySlots) {
                saveData.inventorySlotSaveData.Add(GetSlotSaveData(inventorySlot));
            }
        }

        public void SaveBankSlotData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveBankSlotData()");

            saveData.bankSlotSaveData.Clear();
            foreach (InventorySlot inventorySlot in unitController.CharacterInventoryManager.BankSlots) {
                saveData.bankSlotSaveData.Add(GetSlotSaveData(inventorySlot));
            }
        }

        public void SaveReputationData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveReputationData()");
            saveData.reputationSaveData.Clear();
            foreach (FactionDisposition factionDisposition in unitController.CharacterFactionManager.DispositionDictionary) {
                if (factionDisposition == null) {
                    Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveReputationData(): no disposition");
                    continue;
                }
                if (factionDisposition.Faction == null) {
                    Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveReputationData() no faction");
                    continue;
                }
                ReputationSaveData reputationSaveData = new ReputationSaveData();
                reputationSaveData.ReputationName = factionDisposition.Faction.ResourceName;
                reputationSaveData.Amount = factionDisposition.disposition;
                saveData.reputationSaveData.Add(reputationSaveData);
            }
        }

        public void SaveCurrencyData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveCurrencyData()");
            saveData.currencySaveData.Clear();
            foreach (CurrencyNode currencyNode in unitController.CharacterCurrencyManager.CurrencyList.Values) {
                CurrencySaveData currencySaveData = new CurrencySaveData();
                currencySaveData.Amount = currencyNode.Amount;
                currencySaveData.CurrencyName = currencyNode.currency.ResourceName;
                saveData.currencySaveData.Add(currencySaveData);
            }
        }

        public void SaveEquippedBagData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveEquippedBagData()");
            saveData.equippedBagSaveData.Clear();
            foreach (BagNode bagNode in unitController.CharacterInventoryManager.BagNodes) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveEquippedBagData(): got bagNode");
                saveData.equippedBagSaveData.Add(GetBagSaveData(bagNode));
            }
        }

        public void SaveEquippedBankBagData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveEquippedBagData()");
            saveData.equippedBankBagSaveData.Clear();
            foreach (BagNode bagNode in unitController.CharacterInventoryManager.BankNodes) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveEquippedBagData(): got bagNode");
                saveData.equippedBankBagSaveData.Add(GetBagSaveData(bagNode));
            }
        }

        private EquippedBagSaveData GetBagSaveData(BagNode bagNode) {
            EquippedBagSaveData saveData = new EquippedBagSaveData();
            saveData.BagName = (bagNode.InstantiatedBag != null ? bagNode.InstantiatedBag.ResourceName : string.Empty);
            saveData.slotCount = (bagNode.InstantiatedBag != null ? bagNode.InstantiatedBag.Slots : 0);
            if (bagNode.InstantiatedBag != null) {
                saveData.DisplayName = bagNode.InstantiatedBag.DisplayName;
                saveData.dropLevel = bagNode.InstantiatedBag.DropLevel;
                saveData.itemInstanceId = bagNode.InstantiatedBag.InstanceId;
            }

            return saveData;
        }

        public void SaveAbilityData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSaveManager.SaveAbilityData()");
            saveData.abilitySaveData.Clear();
            foreach (AbilityProperties baseAbility in unitController.CharacterAbilityManager.RawAbilityList.Values) {
                AbilitySaveData abilitySaveData = new AbilitySaveData();
                abilitySaveData.AbilityName = baseAbility.DisplayName;
                saveData.abilitySaveData.Add(abilitySaveData);
            }
        }

        public void SavePetData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSaveManager.SavePetData()");

            saveData.petSaveData.Clear();
            foreach (UnitProfile unitProfile in unitController.CharacterPetManager.UnitProfiles) {
                PetSaveData petSaveData = new PetSaveData();
                petSaveData.PetName = unitProfile.ResourceName;
                saveData.petSaveData.Add(petSaveData);
            }
        }

        public void SaveEquipmentData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSaveManager..SaveEquipmentData()");
            saveData.equipmentSaveData.Clear();
            if (unitController.CharacterEquipmentManager != null) {
                foreach (EquipmentInventorySlot equipmentInventorySlot in unitController.CharacterEquipmentManager.CurrentEquipment.Values) {
                    EquipmentSaveData equipmentSaveData = new EquipmentSaveData();
                    equipmentSaveData.EquipmentName = (equipmentInventorySlot.InstantiatedEquipment == null ? string.Empty : equipmentInventorySlot.InstantiatedEquipment.ResourceName);
                    equipmentSaveData.DisplayName = (equipmentInventorySlot.InstantiatedEquipment == null ? string.Empty : equipmentInventorySlot.InstantiatedEquipment.DisplayName);
                    if (equipmentInventorySlot.InstantiatedEquipment != null) {
                        if (equipmentInventorySlot.InstantiatedEquipment.ItemQuality != null) {
                            equipmentSaveData.itemQuality = (equipmentInventorySlot.InstantiatedEquipment == null ? string.Empty : equipmentInventorySlot.InstantiatedEquipment.ItemQuality.ResourceName);
                        }
                        equipmentSaveData.dropLevel = equipmentInventorySlot.InstantiatedEquipment.DropLevel;
                        equipmentSaveData.itemInstanceId = equipmentInventorySlot.InstantiatedEquipment.InstanceId;
                        equipmentSaveData.randomSecondaryStatIndexes = (equipmentInventorySlot.InstantiatedEquipment == null ? null : equipmentInventorySlot.InstantiatedEquipment.RandomStatIndexes);
                    }
                    saveData.equipmentSaveData.Add(equipmentSaveData);
                }
            }
        }

        public void SaveSkillData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveSkillData()");
            saveData.skillSaveData.Clear();
            foreach (string skillName in unitController.CharacterSkillManager.MySkillList.Keys) {
                SkillSaveData skillSaveData = new SkillSaveData();
                skillSaveData.SkillName = skillName;
                saveData.skillSaveData.Add(skillSaveData);
            }
        }

        public void SaveRecipeData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveRecipeData()");
            saveData.recipeSaveData.Clear();
            foreach (string recipeName in unitController.CharacterRecipeManager.RecipeList.Keys) {
                RecipeSaveData recipeSaveData = new RecipeSaveData();
                recipeSaveData.RecipeName = recipeName;
                saveData.recipeSaveData.Add(recipeSaveData);
            }
        }

        public void SetDialogTurnedIn(Dialog dialog, bool turnedIn) {
            DialogSaveData saveData = GetDialogSaveData(dialog);
            saveData.turnedIn = turnedIn;
            dialogSaveDataDictionary[saveData.DialogName] = saveData;
            SaveDialogData();
        }

        public void SetBehaviorCompleted(BehaviorProfile behaviorProfile, bool value) {
            BehaviorSaveData saveData = GetBehaviorSaveData(behaviorProfile);
            saveData.completed = value;
            behaviorSaveDataDictionary[saveData.BehaviorName] = saveData;
        }
    }

}