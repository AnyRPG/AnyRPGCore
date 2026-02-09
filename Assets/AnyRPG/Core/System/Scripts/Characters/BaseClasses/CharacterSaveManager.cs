using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class CharacterSaveManager : ConfiguredClass {

        private UnitController unitController;

        private CharacterSaveData saveData = null;

        private Dictionary<string, BehaviorSaveData> behaviorSaveDataDictionary = new Dictionary<string, BehaviorSaveData>();
        private Dictionary<string, DialogSaveData> dialogSaveDataDictionary = new Dictionary<string, DialogSaveData>();

        private Dictionary<string, SceneNodeSaveData> sceneNodeSaveDataDictionary = new Dictionary<string, SceneNodeSaveData>();

        private bool eventSubscriptionsInitialized = false;

        // game manager references
        private ActionBarManager actionBarManager = null;
        private SaveManager saveManager = null;
        private LevelManager levelManager = null;

        public Dictionary<string, DialogSaveData> DialogSaveDataDictionary { get => dialogSaveDataDictionary; }

        public Dictionary<string, SceneNodeSaveData> SceneNodeSaveDataDictionary { get => sceneNodeSaveDataDictionary; set => sceneNodeSaveDataDictionary = value; }

        public CharacterSaveData SaveData { get => saveData; }

        public CharacterSaveManager(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
            //saveData = saveManager.CreateSaveData().SaveData;
            saveData = new CharacterSaveData();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            actionBarManager = systemGameManager.UIManager.ActionBarManager;
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
                unitController.UnitEventController.OnSetMouseActionButton += HandleSetMouseActionButton;
                unitController.UnitEventController.OnUnsetMouseActionButton += HandleUnsetMouseActionButton;
                unitController.UnitEventController.OnSetGamepadActionButton += HandleSetGamepadActionButton;
                unitController.UnitEventController.OnUnsetGamepadActionButton += HandleUnsetGamepadActionButton;
                eventSubscriptionsInitialized = true;
            }
        }

        public void SaveGameData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveGameData()");

            saveData.UnitProfileName = unitController.UnitProfile.ResourceName;
            saveData.CharacterName = unitController.BaseCharacter.CharacterName;
            saveData.CharacterLevel = unitController.CharacterStats.Level;
            saveData.CurrentExperience = unitController.CharacterStats.CurrentXP;
            saveData.IsDead = !unitController.CharacterStats.IsAlive;
            //saveData.isMounted = unitController.IsMounted;
            if (unitController.BaseCharacter.Faction != null) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveGameData() faction: {unitController.BaseCharacter.Faction.ResourceName}");
                saveData.CharacterFaction = unitController.BaseCharacter.Faction.ResourceName;
            } else {
                //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveGameData() no faction");
                saveData.CharacterFaction = string.Empty;
            }
            if (unitController.BaseCharacter.CharacterClass != null) {
                saveData.CharacterClass = unitController.BaseCharacter.CharacterClass.ResourceName;
            } else {
                saveData.CharacterClass = string.Empty;
            }
            if (unitController.BaseCharacter.ClassSpecialization != null) {
                saveData.ClassSpecialization = unitController.BaseCharacter.ClassSpecialization.ResourceName;
            } else {
                saveData.ClassSpecialization = string.Empty;
            }
            if (unitController.BaseCharacter.CharacterRace != null) {
                saveData.CharacterRace = unitController.BaseCharacter.CharacterRace.ResourceName;
            } else {
                saveData.CharacterRace = string.Empty;
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
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleUnsetGamepadActionButton(int buttonIndex) {
            SaveActionBarData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleSetGamepadActionButton(IUseable useable, int buttonIndex) {
            SaveActionBarData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleUnsetMouseActionButton(int buttonIndex) {
            SaveActionBarData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleSetMouseActionButton(IUseable useable, int buttonIndex) {
            SaveActionBarData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleActivateMountedState(UnitController controller) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.HandleActivateMountedState()");

            saveData.IsMounted = true;
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }


        private void HandleReviveComplete(UnitController controller) {
            saveData.IsDead = false;
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleBeforeDie(UnitController controller) {
            saveData.IsDead = true;
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleSetReputationAmount(Faction faction, float amount) {
            SaveReputationData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        public void HandleAddPet(UnitProfile profile) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.HandleAddPet({profile.ResourceName})");

            SavePetData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        public void HandleAddStatusEffectStack(string obj) {
            SaveStatusEffectData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        public void HandleCancelStatusEffect(StatusEffectProperties properties) {
            SaveStatusEffectData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        public void HandleStatusEffectAdd(UnitController sourceUnitController, StatusEffectNode node) {
            SaveStatusEffectData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        public void HandleCurrencyChange(string currencyResourceName, int amount) {
            SaveCurrencyData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        public void HandleRemoveEquipment(EquipmentSlotProfile profile, InstantiatedEquipment equipment) {
            SaveEquipmentData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        public void HandleAddEquipment(EquipmentSlotProfile profile, InstantiatedEquipment equipment) {
            SaveEquipmentData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        public void HandleReputationChange(UnitController sourceUnitController) {
            SaveReputationData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        public void HandleUnlearnRecipe(Recipe recipe) {
            SaveRecipeData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        public void HandleLearnRecipe(Recipe recipe) {
            SaveRecipeData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleRemoveBag(InstantiatedBag bag) {
            SaveEquippedBagData();
            SaveEquippedBankBagData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleAddBag(InstantiatedBag bag, BagNode node) {
            SaveEquippedBagData();
            SaveEquippedBankBagData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }


        private void HandleRemoveItemFromBankSlot(InventorySlot slot, InstantiatedItem item) {
            SaveBankSlotData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleAddItemToBankSlot(InventorySlot slot, InstantiatedItem item) {
            SaveBankSlotData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }


        private void HandleRemoveItemFromInventorySlot(InventorySlot slot, InstantiatedItem item) {
            SaveInventorySlotData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleAddItemToInventorySlot(InventorySlot slot, InstantiatedItem item) {
            SaveInventorySlotData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }


        private void HandleSetQuestObjectiveCurrentAmount(string arg1, string arg2, string arg3, int amount) {
            SaveQuestData();
            //SaveAchievementData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleSetAchievementObjectiveCurrentAmount(string arg1, string arg2, string arg3, int amount) {
            //SaveQuestData();
            SaveAchievementData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleQuestObjectiveStatusUpdated(UnitController controller, Quest quest) {
            SaveQuestData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleAchievementObjectiveStatusUpdated(UnitController controller, Achievement achievement) {
            SaveQuestData();
            SaveAchievementData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleMarkQuestComplete(UnitController controller, QuestBase questBase) {
            SaveQuestData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleMarkAchievementComplete(UnitController controller, Achievement achievement) {
            SaveAchievementData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleTurnInQuest(UnitController controller, QuestBase questBase) {
            SaveQuestData();
            SaveAchievementData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleAcceptQuest(UnitController controller, QuestBase questBase) {
            SaveQuestData();
            SaveAchievementData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleAbandonQuest(UnitController controller, QuestBase questBase) {
            SaveQuestData();
            SaveAchievementData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleRebuildModelAppearance() {
            SaveAppearanceData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleResourceAmountChanged(PowerResource resource, int arg2, int arg3) {
            SaveResourcePowerData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleUnLearnSkill(UnitController controller, Skill skill) {
            SaveSkillData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleLearnSkill(UnitController controller, Skill skill) {
            SaveSkillData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleUnlearnAbility(AbilityProperties abilityProperties) {
            SaveAbilityData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleLearnAbility(UnitController controller, AbilityProperties properties) {
            SaveAbilityData();
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleSpecializationChange(UnitController sourceUnitController, ClassSpecialization newSpecialization, ClassSpecialization oldSpecialization) {
            if (newSpecialization != null) {
                saveData.ClassSpecialization = newSpecialization.ResourceName;
            } else {
                saveData.ClassSpecialization = string.Empty;
            }
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleClassChange(UnitController sourceUnitController, CharacterClass newClass, CharacterClass oldClass) {
            if (newClass != null) {
                saveData.CharacterClass = newClass.ResourceName;
            } else {
                saveData.CharacterClass = string.Empty;
            }
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleRaceChange(CharacterRace newRace, CharacterRace oldRace) {
            if (newRace != null) {
                saveData.CharacterRace = newRace.ResourceName;
            } else {
                saveData.CharacterRace = string.Empty;
            }
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleFactionChange(Faction newFaction, Faction oldFaction) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSaveManager.HandleFactionChange(new: {newFaction?.ResourceName}, old: {oldFaction?.ResourceName})");

            if (newFaction != null) {
                saveData.CharacterFaction = newFaction.ResourceName;
            } else {
                saveData.CharacterFaction = string.Empty;
            }
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleNameChange(string newName) {
            saveData.CharacterName = newName;
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        private void HandleGainXP(UnitController sourceUnitController, int gainedXP, int currentXP) {
            saveData.CurrentExperience = currentXP;
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        public void HandleLevelChanged(int newLevel) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.HandleLevelChanged({newLevel})");

            saveData.CharacterLevel = newLevel;
            unitController.UnitEventController.NotifyOnSaveDataUpdated();
        }

        public void SetSaveData(CharacterRequestData characterRequestData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SetSaveData()");

            this.saveData = characterRequestData.saveData;
            characterRequestData.characterConfigurationRequest.characterName = saveData.CharacterName;
            characterRequestData.characterConfigurationRequest.characterAppearanceData = new CharacterAppearanceData(saveData);
            characterRequestData.characterConfigurationRequest.unitLevel = saveData.CharacterLevel;
            characterRequestData.characterConfigurationRequest.currentExperience = saveData.CurrentExperience;
            characterRequestData.characterConfigurationRequest.isDead = saveData.IsDead;
            if (saveData.CharacterClass != string.Empty) {
                characterRequestData.characterConfigurationRequest.characterClass = systemDataFactory.GetResource<CharacterClass>(saveData.CharacterClass);
            } else {
                characterRequestData.characterConfigurationRequest.characterClass = null;
            }
            if (saveData.ClassSpecialization != string.Empty) {
                characterRequestData.characterConfigurationRequest.classSpecialization = systemDataFactory.GetResource<ClassSpecialization>(saveData.ClassSpecialization);
            } else {
                characterRequestData.characterConfigurationRequest.classSpecialization = null;
            }
            if (saveData.CharacterRace != string.Empty) {
                characterRequestData.characterConfigurationRequest.characterRace = systemDataFactory.GetResource<CharacterRace>(saveData.CharacterRace);
            } else {
                characterRequestData.characterConfigurationRequest.characterRace = null;
            }
            if (saveData.CharacterFaction != string.Empty) {
                characterRequestData.characterConfigurationRequest.faction = systemDataFactory.GetResource<Faction>(saveData.CharacterFaction);
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
                saveData.DialogNodeShown = new List<bool>(new bool[dialog.DialogNodes.Count]);
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
            return saveData.DialogNodeShown[index];
        }

        public void SetDialogNodeShown(Dialog dialog, bool value, int index) {
            DialogSaveData saveData = GetDialogSaveData(dialog);
            saveData.DialogNodeShown[index] = value;
            dialogSaveDataDictionary[dialog.ResourceName] = saveData;
            SaveDialogData();
        }

        public void ResetDialogNodes(Dialog dialog) {
            DialogSaveData saveData = GetDialogSaveData(dialog);
            saveData.DialogNodeShown = new List<bool>(new bool[dialog.DialogNodes.Count]);
            dialogSaveDataDictionary[dialog.ResourceName] = saveData;
            SaveDialogData();
        }

        public SceneNodeSaveData GetSceneNodeSaveData(SceneNode sceneNode) {
            SceneNodeSaveData saveData;
            if (sceneNodeSaveDataDictionary.ContainsKey(sceneNode.ResourceName)) {
                saveData = sceneNodeSaveDataDictionary[sceneNode.ResourceName];
            } else {
                saveData = new SceneNodeSaveData();
                saveData.PersistentObjects = new List<PersistentObjectSaveData>();
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

        public void LoadDialogData(CharacterSaveData characterSaveData) {
            dialogSaveDataDictionary.Clear();
            foreach (DialogSaveData dialogSaveData in characterSaveData.DialogSaveData) {
                if (dialogSaveData.DialogName != null && dialogSaveData.DialogName != string.Empty) {
                    dialogSaveDataDictionary.Add(dialogSaveData.DialogName, dialogSaveData);
                }
            }
        }

        public void LoadBehaviorData(CharacterSaveData characterSaveData) {
            behaviorSaveDataDictionary.Clear();
            foreach (BehaviorSaveData behaviorSaveData in characterSaveData.BehaviorSaveData) {
                if (behaviorSaveData.BehaviorName != null && behaviorSaveData.BehaviorName != string.Empty) {
                    behaviorSaveDataDictionary.Add(behaviorSaveData.BehaviorName, behaviorSaveData);
                }
            }
        }

        public void LoadSceneNodeData(CharacterSaveData characterSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadSceneNodeData()");

            sceneNodeSaveDataDictionary.Clear();
            foreach (SceneNodeSaveData sceneNodeSaveData in characterSaveData.SceneNodeSaveData) {
                if (sceneNodeSaveData.SceneName != null && sceneNodeSaveData.SceneName != string.Empty) {
                    sceneNodeSaveDataDictionary.Add(sceneNodeSaveData.SceneName, sceneNodeSaveData);
                }
            }
        }

        public void LoadQuestData(CharacterSaveData characterSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.Savemanager.LoadQuestData()");

            unitController.CharacterQuestLog.QuestSaveDataDictionary.Clear();
            unitController.CharacterQuestLog.QuestObjectiveSaveDataDictionary.Clear();
            foreach (QuestSaveData questSaveData in characterSaveData.QuestSaveData) {

                if (questSaveData.QuestName == null || questSaveData.QuestName == string.Empty) {
                    // don't load invalid quest data
                    continue;
                }
                unitController.CharacterQuestLog.QuestSaveDataDictionary.Add(questSaveData.QuestName, questSaveData);

                Dictionary<string, Dictionary<string, QuestObjectiveSaveData>> objectiveDictionary = new Dictionary<string, Dictionary<string, QuestObjectiveSaveData>>();

                // add objectives to dictionary
                foreach (QuestObjectiveSaveData questObjectiveSaveData in questSaveData.QuestObjectives) {
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

            foreach (QuestSaveData questSaveData in characterSaveData.QuestSaveData) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadQuestData(): loading questsavedata");
                unitController.CharacterQuestLog.LoadQuest(questSaveData);
            }
        }

        public void LoadAchievementData(CharacterSaveData characterSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.Savemanager.LoadAchievementData()");

            unitController.CharacterQuestLog.AchievementSaveDataDictionary.Clear();
            unitController.CharacterQuestLog.AchievementObjectiveSaveDataDictionary.Clear();
            foreach (QuestSaveData achievementSaveData in characterSaveData.AchievementSaveData) {

                if (achievementSaveData.QuestName == null || achievementSaveData.QuestName == string.Empty) {
                    // don't load invalid quest data
                    continue;
                }
                unitController.CharacterQuestLog.AchievementSaveDataDictionary.Add(achievementSaveData.QuestName, achievementSaveData);

                Dictionary<string, Dictionary<string, QuestObjectiveSaveData>> objectiveDictionary = new Dictionary<string, Dictionary<string, QuestObjectiveSaveData>>();

                // add objectives to dictionary
                foreach (QuestObjectiveSaveData achievementObjectiveSaveData in achievementSaveData.QuestObjectives) {
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

            foreach (QuestSaveData questSaveData in characterSaveData.AchievementSaveData) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadQuestData(): loading questsavedata");
                unitController.CharacterQuestLog.LoadAchievement(questSaveData);
            }
        }
        public void LoadResourcePowerData(CharacterSaveData characterSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadResourcePowerData()");

            foreach (ResourcePowerSaveData resourcePowerSaveData in characterSaveData.ResourcePowerSaveData) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadResourcePowerData(): loading questsavedata");
                //unitController.CharacterStats.SetResourceAmount(resourcePowerSaveData.ResourceName, resourcePowerSaveData.amount);
                unitController.CharacterStats.LoadResourceAmount(resourcePowerSaveData.ResourceName, resourcePowerSaveData.Amount);
            }

        }

        public void LoadStatusEffectData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadStatusEffectData()");
            foreach (StatusEffectSaveData statusEffectSaveData in saveData.StatusEffectSaveData) {
                unitController.CharacterAbilityManager.ApplySavedStatusEffects(statusEffectSaveData);
            }
        }

        public void LoadCurrencyData(CharacterSaveData characterSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadCurrencyData()");
            foreach (CurrencySaveData currencySaveData in characterSaveData.CurrencySaveData) {
                unitController.CharacterCurrencyManager.AddCurrency(systemDataFactory.GetResource<Currency>(currencySaveData.CurrencyName), currencySaveData.Amount);
            }
        }


        public void LoadPetData(CharacterSaveData characterSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSaveManager.LoadPetData()");

            foreach (PetSaveData petSaveData in characterSaveData.PetSaveData) {
                if (petSaveData.PetName != string.Empty) {
                    unitController.CharacterPetManager.AddPet(petSaveData.PetName);
                }
            }

        }

        public void LoadEquippedBagData(CharacterSaveData characterSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadEquippedBagData()");
            unitController.CharacterInventoryManager.LoadEquippedBagData(characterSaveData.EquippedBagSaveData, false);
            unitController.CharacterInventoryManager.LoadEquippedBagData(characterSaveData.EquippedBankBagSaveData, true);
        }

        public void LoadInventorySlotData(CharacterSaveData characterSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadInventorySlotData()");
            int counter = 0;
            foreach (InventorySlotSaveData inventorySlotSaveData in characterSaveData.InventorySlotSaveData) {
                LoadSlotData(inventorySlotSaveData, counter, false);
                counter++;
            }
        }

        public void LoadBankSlotData(CharacterSaveData characterSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadBankSlotData()");

            int counter = 0;
            foreach (InventorySlotSaveData inventorySlotSaveData in characterSaveData.BankSlotSaveData) {
                LoadSlotData(inventorySlotSaveData, counter, true);
                counter++;
            }
        }

        private void LoadSlotData(InventorySlotSaveData inventorySlotSaveData, int counter, bool bank) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadSlotData({inventorySlotSaveData.ItemInstanceIds.Count}, {counter}, {bank})");

            foreach (long itemInstanceId in inventorySlotSaveData.ItemInstanceIds) {
                InstantiatedItem newInstantiatedItem = systemItemManager.GetExistingInstantiatedItem(itemInstanceId);
                if (newInstantiatedItem == null) {
                    Debug.LogWarning($"{unitController.gameObject.name}.CharacterSavemanager.LoadInventorySlotData(): item is null for itemInstanceId {itemInstanceId}, skipping load");
                } else {
                    if (bank == true) {
                        unitController.CharacterInventoryManager.AddBankItem(newInstantiatedItem, counter);
                    } else {
                        unitController.CharacterInventoryManager.AddInventoryItem(newInstantiatedItem, counter);
                    }
                }
            }
        }

        public void LoadAbilityData(CharacterSaveData characterSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadAbilityData()");

            foreach (AbilitySaveData abilitySaveData in characterSaveData.AbilitySaveData) {
                if (abilitySaveData.AbilityName != string.Empty) {
                    unitController.CharacterAbilityManager.LoadAbility(abilitySaveData.AbilityName);
                }
            }
        }

        public void LoadEquipmentData(CharacterSaveData characterSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadEquipmentData()");

            foreach (EquipmentInventorySlotSaveData equipmentSaveData in characterSaveData.EquipmentSaveData) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadEquipmentData() {(equipmentSaveData.EquipmentName)}");
                InstantiatedEquipment newInstantiatedEquipment = unitController.CharacterInventoryManager.GetInstantiatedEquipmentFromSaveData(equipmentSaveData);
                if (newInstantiatedEquipment != null) {
                    unitController.CharacterEquipmentManager.Equip(newInstantiatedEquipment, null);
                }
            }
        }

        public void LoadReputationData(CharacterSaveData characterSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadReputationData()");
            //int counter = 0;
            foreach (ReputationSaveData reputationSaveData in characterSaveData.ReputationSaveData) {
                FactionDisposition factionDisposition = new FactionDisposition();
                factionDisposition.Faction = systemDataFactory.GetResource<Faction>(reputationSaveData.ReputationName);
                factionDisposition.disposition = reputationSaveData.Amount;
                unitController.CharacterFactionManager.LoadReputation(factionDisposition.Faction, (int)factionDisposition.disposition);
                //counter++;
            }
        }


        public void LoadSkillData(CharacterSaveData characterSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadSkillData()");
            foreach (SkillSaveData skillSaveData in characterSaveData.SkillSaveData) {
                unitController.CharacterSkillManager.LoadSkill(skillSaveData.SkillName);
            }
        }

        public void LoadRecipeData(CharacterSaveData characterSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadRecipeData()");
            foreach (RecipeSaveData recipeSaveData in characterSaveData.RecipeSaveData) {
                unitController.CharacterRecipeManager.LoadRecipe(recipeSaveData.RecipeName);
            }
        }

        public void LoadActionBarData(CharacterSaveData characterSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadActionBarData()");

            LoadMouseActionButtonData(characterSaveData.ActionBarSaveData);
            LoadGamepadActionButtonData(characterSaveData.GamepadActionBarSaveData);
            actionBarManager.SetGamepadActionButtonSet(characterSaveData.GamepadActionButtonSet, false);
        }

        private void LoadMouseActionButtonData(List<ActionBarSaveData> actionBarSaveDatas) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadActionButtonData(saveDataCount: {actionBarSaveDatas.Count})");

            IUseable useable = null;
            IUseable savedUseable = null;
            int counter = 0;
            foreach (ActionBarSaveData actionBarSaveData in actionBarSaveDatas) {
                useable = null;
                savedUseable = null;
                if (actionBarSaveData.DisplayName != string.Empty) {
                    if (actionBarSaveData.IsItem == true) {
                        // find item in bag
                        //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadMouseActionButtonData(): searching for usable {actionBarSaveData.DisplayName} as item");
                        useable = systemItemManager.GetNewInstantiatedItem(actionBarSaveData.DisplayName);
                    } else {
                        //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadMouseActionButtonData(): searching for usable {actionBarSaveData.DisplayName} as ability");
                        Ability ability = systemDataFactory.GetResource<Ability>(actionBarSaveData.DisplayName);
                        if (ability != null) {
                            useable = ability.AbilityProperties;
                        }
                    }
                }
                if (actionBarSaveData.SavedName != string.Empty) {
                    if (actionBarSaveData.SavedIsItem == true) {
                        // find item in bag
                        //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadMouseActionButtonData(): searching for saved usable {actionBarSaveData.savedName} as item");
                        savedUseable = systemItemManager.GetNewInstantiatedItem(actionBarSaveData.SavedName);
                    } else {
                        //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.LoadMouseActionButtonData(): searching for saved usable {actionBarSaveData.savedName} as ability");
                        Ability ability = systemDataFactory.GetResource<Ability>(actionBarSaveData.SavedName);
                        if (ability != null) {
                            savedUseable = ability.AbilityProperties;
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
                if (savedUseable != null) {
                    unitController.CharacterActionBarManager.MouseActionButtons[counter].SavedUseable = savedUseable;
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
                if (actionBarSaveData.IsItem == true) {
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
                    if (actionBarSaveData.SavedName != null && actionBarSaveData.SavedName != string.Empty) {
                        IUseable savedUseable = systemDataFactory.GetResource<Ability>(actionBarSaveData.SavedName).AbilityProperties;
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

        public void VisitSceneNode() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.VisitSceneNode()");

            SceneNode sceneNode = levelManager.SceneDictionary[unitController.gameObject.scene.name];
            if (sceneNode != null) {
                VisitSceneNode(sceneNode);
            }
        }

        public void VisitSceneNode(SceneNode sceneNode) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.VisitSceneNode({sceneNode.ResourceName})");

            SceneNodeSaveData saveData = GetSceneNodeSaveData(sceneNode);
            if (saveData.Visited == false) {
                saveData.Visited = true;
                sceneNodeSaveDataDictionary[saveData.SceneName] = saveData;
                SaveSceneNodeData();
            }
        }

        public bool IsSceneNodeVisited(SceneNode sceneNode) {
            SceneNodeSaveData saveData = GetSceneNodeSaveData(sceneNode);
            return saveData.Visited;
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
            saveData.ResourcePowerSaveData.Clear();
            foreach (PowerResource powerResource in unitController.CharacterStats.PowerResourceDictionary.Keys) {
                ResourcePowerSaveData resourcePowerData = new ResourcePowerSaveData();
                resourcePowerData.ResourceName = powerResource.ResourceName;
                resourcePowerData.Amount = unitController.CharacterStats.PowerResourceDictionary[powerResource].currentValue;
                saveData.ResourcePowerSaveData.Add(resourcePowerData);
            }
        }

        public void SaveAppearanceData() {
            unitController.UnitModelController.SaveAppearanceSettings(/*this,*/ saveData);
        }

        public void SaveQuestData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveQuestData()");

            saveData.QuestSaveData.Clear();
            foreach (QuestSaveData questSaveData in unitController.CharacterQuestLog.QuestSaveDataDictionary.Values) {
                QuestSaveData finalSaveData = questSaveData;
                if (unitController.CharacterQuestLog.QuestObjectiveSaveDataDictionary.ContainsKey(questSaveData.QuestName)) {

                    List<QuestObjectiveSaveData> questObjectiveSaveDataList = new List<QuestObjectiveSaveData>();
                    foreach (string typeName in unitController.CharacterQuestLog.QuestObjectiveSaveDataDictionary[questSaveData.QuestName].Keys) {
                        foreach (QuestObjectiveSaveData saveData in unitController.CharacterQuestLog.QuestObjectiveSaveDataDictionary[questSaveData.QuestName][typeName].Values) {
                            questObjectiveSaveDataList.Add(saveData);
                        }
                    }
                    finalSaveData.QuestObjectives = questObjectiveSaveDataList;
                }
                finalSaveData.InLog = unitController.CharacterQuestLog.HasQuest(questSaveData.QuestName);
                saveData.QuestSaveData.Add(finalSaveData);
            }
        }

        public void SaveAchievementData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveAchievementData()");

            saveData.AchievementSaveData.Clear();
            foreach (QuestSaveData achievementSaveData in unitController.CharacterQuestLog.AchievementSaveDataDictionary.Values) {
                QuestSaveData finalSaveData = achievementSaveData;
                if (unitController.CharacterQuestLog.AchievementObjectiveSaveDataDictionary.ContainsKey(achievementSaveData.QuestName)) {

                    List<QuestObjectiveSaveData> achievementObjectiveSaveDataList = new List<QuestObjectiveSaveData>();
                    foreach (string typeName in unitController.CharacterQuestLog.AchievementObjectiveSaveDataDictionary[achievementSaveData.QuestName].Keys) {
                        foreach (QuestObjectiveSaveData saveData in unitController.CharacterQuestLog.AchievementObjectiveSaveDataDictionary[achievementSaveData.QuestName][typeName].Values) {
                            achievementObjectiveSaveDataList.Add(saveData);
                        }

                    }

                    finalSaveData.QuestObjectives = achievementObjectiveSaveDataList;
                }
                finalSaveData.InLog = unitController.CharacterQuestLog.HasAchievement(achievementSaveData.QuestName);
                // print if finalsavadata complete and turnedin are true
                //Debug.Log($"CharacterSavemanager.SaveAchievementData(): Saving achievement {finalSaveData.QuestName} complete: {finalSaveData.markedComplete} turnedIn: {finalSaveData.turnedIn}");
                saveData.AchievementSaveData.Add(finalSaveData);
            }
        }

        public void SaveDialogData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveDialogData()");
            saveData.DialogSaveData.Clear();
            foreach (DialogSaveData dialogSaveData in dialogSaveDataDictionary.Values) {
                saveData.DialogSaveData.Add(dialogSaveData);
            }
        }

        public void SaveBehaviorData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveQuestData()");
            saveData.BehaviorSaveData.Clear();
            foreach (BehaviorSaveData behaviorSaveData in behaviorSaveDataDictionary.Values) {
                saveData.BehaviorSaveData.Add(behaviorSaveData);
            }
        }

        public void SaveSceneNodeData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveSceneNodeData()");

            saveData.SceneNodeSaveData.Clear();
            foreach (SceneNodeSaveData sceneNodeSaveData in sceneNodeSaveDataDictionary.Values) {
                saveData.SceneNodeSaveData.Add(sceneNodeSaveData);
            }
        }

        public void SaveStatusEffectData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveSceneNodeData()");

            saveData.StatusEffectSaveData.Clear();
            foreach (StatusEffectNode statusEffectNode in unitController.CharacterStats.StatusEffects.Values) {
                if (statusEffectNode.StatusEffect.ClassTrait == false
                    && statusEffectNode.StatusEffect.SaveEffect == true
                    && statusEffectNode.AbilityEffectContext.AbilityCaster == (unitController as IAbilityCaster)) {
                    StatusEffectSaveData statusEffectSaveData = new StatusEffectSaveData();
                    statusEffectSaveData.StatusEffectName = statusEffectNode.StatusEffect.DisplayName;
                    statusEffectSaveData.RemainingSeconds = (int)statusEffectNode.GetRemainingDuration();
                    saveData.StatusEffectSaveData.Add(statusEffectSaveData);
                }
            }
        }

        public void SaveActionBarData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveActionBarData()");

            saveData.GamepadActionButtonSet = actionBarManager.CurrentActionBarSet;
            saveData.ActionBarSaveData.Clear();
            saveData.GamepadActionBarSaveData.Clear();
            foreach (ActionButtonNode actionButtonNode in unitController.CharacterActionBarManager.MouseActionButtons) {
                SaveActionButtonNodeSaveData(actionButtonNode, saveData.ActionBarSaveData);
            }
            foreach (ActionButtonNode actionButtonNode in unitController.CharacterActionBarManager.GamepadActionButtons) {
                SaveActionButtonNodeSaveData(actionButtonNode, saveData.GamepadActionBarSaveData);
            }
        }

        /*
        private void SaveActionButtonSaveData(ActionButton actionButton, List<ActionBarSaveData> actionBarSaveDataList) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveActionButtonSaveData({actionButton.name}");

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
            actionBarSaveData.SavedName = (actionButtonNode.SavedUseable == null ? string.Empty : (actionButtonNode.SavedUseable as IDescribable).ResourceName);
            if (actionButtonNode.Useable != null) {
                actionBarSaveData.IsItem = (actionButtonNode.Useable is InstantiatedItem ? true : false);
            } else {
                actionBarSaveData.IsItem = false;
            }
            if (actionButtonNode.SavedUseable != null) {
                actionBarSaveData.SavedIsItem = (actionButtonNode.SavedUseable is InstantiatedItem ? true : false);
            } else {
                actionBarSaveData.SavedIsItem = false;
            }
            actionBarSaveDataList.Add(actionBarSaveData);
        }

        private InventorySlotSaveData GetSlotSaveData(InventorySlot inventorySlot) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.GetSlotSaveData(): getting slot save data for slot with item {(inventorySlot.InstantiatedItem != null ? inventorySlot.InstantiatedItem.ResourceName : "empty")}");

            InventorySlotSaveData saveData = new InventorySlotSaveData();
            saveData.ItemInstanceIds = inventorySlot.InstantiatedItems.Keys.ToList();
            return saveData;
        }

        public void SaveInventorySlotData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveInventorySlotData()");
            saveData.InventorySlotSaveData.Clear();
            foreach (InventorySlot inventorySlot in unitController.CharacterInventoryManager.InventorySlots) {
                saveData.InventorySlotSaveData.Add(GetSlotSaveData(inventorySlot));
            }
        }

        public void SaveBankSlotData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveBankSlotData()");

            saveData.BankSlotSaveData.Clear();
            foreach (InventorySlot inventorySlot in unitController.CharacterInventoryManager.BankSlots) {
                saveData.BankSlotSaveData.Add(GetSlotSaveData(inventorySlot));
            }
        }

        public void SaveReputationData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveReputationData()");
            saveData.ReputationSaveData.Clear();
            foreach (FactionDisposition factionDisposition in unitController.CharacterFactionManager.DispositionDictionary) {
                if (factionDisposition == null) {
                    //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveReputationData(): no disposition");
                    continue;
                }
                if (factionDisposition.Faction == null) {
                    //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveReputationData() no faction");
                    continue;
                }
                ReputationSaveData reputationSaveData = new ReputationSaveData();
                reputationSaveData.ReputationName = factionDisposition.Faction.ResourceName;
                reputationSaveData.Amount = factionDisposition.disposition;
                saveData.ReputationSaveData.Add(reputationSaveData);
            }
        }

        public void SaveCurrencyData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveCurrencyData()");
            saveData.CurrencySaveData.Clear();
            foreach (CurrencyNode currencyNode in unitController.CharacterCurrencyManager.CurrencyList.Values) {
                CurrencySaveData currencySaveData = new CurrencySaveData();
                currencySaveData.Amount = currencyNode.Amount;
                currencySaveData.CurrencyName = currencyNode.currency.ResourceName;
                saveData.CurrencySaveData.Add(currencySaveData);
            }
        }

        public void SaveEquippedBagData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveEquippedBagData()");
            saveData.EquippedBagSaveData.Clear();
            foreach (BagNode bagNode in unitController.CharacterInventoryManager.BagNodes) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveEquippedBagData(): got bagNode");
                saveData.EquippedBagSaveData.Add(GetBagSaveData(bagNode));
            }
        }

        public void SaveEquippedBankBagData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveEquippedBagData()");
            saveData.EquippedBankBagSaveData.Clear();
            foreach (BagNode bagNode in unitController.CharacterInventoryManager.BankNodes) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveEquippedBagData(): got bagNode");
                saveData.EquippedBankBagSaveData.Add(GetBagSaveData(bagNode));
            }
        }

        private EquippedBagSaveData GetBagSaveData(BagNode bagNode) {
            EquippedBagSaveData saveData = new EquippedBagSaveData();
            if (bagNode.InstantiatedBag != null) {
                saveData.HasItem = true;
                saveData.ItemInstanceId = bagNode.InstantiatedBag.InstanceId;
            }

            return saveData;
        }

        public void SaveAbilityData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSaveManager.SaveAbilityData()");
            saveData.AbilitySaveData.Clear();
            foreach (AbilityProperties baseAbility in unitController.CharacterAbilityManager.RawAbilityList.Values) {
                AbilitySaveData abilitySaveData = new AbilitySaveData();
                abilitySaveData.AbilityName = baseAbility.DisplayName;
                saveData.AbilitySaveData.Add(abilitySaveData);
            }
        }

        public void SavePetData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSaveManager.SavePetData()");

            saveData.PetSaveData.Clear();
            foreach (UnitProfile unitProfile in unitController.CharacterPetManager.UnitProfiles) {
                PetSaveData petSaveData = new PetSaveData();
                petSaveData.PetName = unitProfile.ResourceName;
                saveData.PetSaveData.Add(petSaveData);
            }
        }

        public void SaveEquipmentData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSaveManager..SaveEquipmentData()");
            saveData.EquipmentSaveData.Clear();
            if (unitController.CharacterEquipmentManager != null) {
                foreach (EquipmentInventorySlot equipmentInventorySlot in unitController.CharacterEquipmentManager.CurrentEquipment.Values) {
                    EquipmentInventorySlotSaveData equipmentSaveData = new EquipmentInventorySlotSaveData();
                    if (equipmentInventorySlot.InstantiatedEquipment != null) {
                        equipmentSaveData.HasItem = true;
                        equipmentSaveData.ItemInstanceId = equipmentInventorySlot.InstantiatedEquipment.InstanceId;
                    }
                    saveData.EquipmentSaveData.Add(equipmentSaveData);
                }
            }
        }

        public void SaveSkillData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveSkillData()");
            saveData.SkillSaveData.Clear();
            foreach (string skillName in unitController.CharacterSkillManager.MySkillList.Keys) {
                SkillSaveData skillSaveData = new SkillSaveData();
                skillSaveData.SkillName = skillName;
                saveData.SkillSaveData.Add(skillSaveData);
            }
        }

        public void SaveRecipeData() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterSavemanager.SaveRecipeData()");
            saveData.RecipeSaveData.Clear();
            foreach (string recipeName in unitController.CharacterRecipeManager.RecipeList.Keys) {
                RecipeSaveData recipeSaveData = new RecipeSaveData();
                recipeSaveData.RecipeName = recipeName;
                saveData.RecipeSaveData.Add(recipeSaveData);
            }
        }

        public void SetDialogTurnedIn(Dialog dialog, bool turnedIn) {
            DialogSaveData saveData = GetDialogSaveData(dialog);
            saveData.TurnedIn = turnedIn;
            dialogSaveDataDictionary[saveData.DialogName] = saveData;
            SaveDialogData();
        }

        public void SetBehaviorCompleted(BehaviorProfile behaviorProfile, bool value) {
            BehaviorSaveData saveData = GetBehaviorSaveData(behaviorProfile);
            saveData.Completed = value;
            behaviorSaveDataDictionary[saveData.BehaviorName] = saveData;
        }
    }

}