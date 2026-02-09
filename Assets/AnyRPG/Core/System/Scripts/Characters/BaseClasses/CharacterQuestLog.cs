using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

namespace AnyRPG {
    /// <summary>
    /// Maintains a list of all quests
    /// </summary>
    public class CharacterQuestLog : ConfiguredClass {

        public event System.Action<Quest> OnShowQuestLogDescription = delegate { };
        public event System.Action<Quest, IQuestGiver> OnShowQuestGiverDescription = delegate { };

        private Dictionary<string, Quest> quests = new Dictionary<string, Quest>();
        private Dictionary<string, Achievement> achievements = new Dictionary<string, Achievement>();

        // reference to UnitController
        UnitController unitController = null;

        // game manager references
        protected InteractionManager interactionManager = null;
        protected UIManager uIManager = null;
        protected DialogManagerClient dialogManager = null;

        public Dictionary<string, Quest> Quests { get => quests; }
        public Dictionary<string, Achievement> Achievements { get => achievements; }

        private Dictionary<string, QuestSaveData> questSaveDataDictionary = new Dictionary<string, QuestSaveData>();
        private Dictionary<string, QuestSaveData> achievementSaveDataDictionary = new Dictionary<string, QuestSaveData>();

        /// <summary>
        /// [questName][objectiveType][objectiveName] : questObjectiveSavaData
        /// </summary>
        private Dictionary<string, Dictionary<string, Dictionary<string, QuestObjectiveSaveData>>> questObjectiveSaveDataDictionary = new Dictionary<string, Dictionary<string, Dictionary<string, QuestObjectiveSaveData>>>();
        private Dictionary<string, Dictionary<string, Dictionary<string, QuestObjectiveSaveData>>> achievementObjectiveSaveDataDictionary = new Dictionary<string, Dictionary<string, Dictionary<string, QuestObjectiveSaveData>>>();

        public Dictionary<string, QuestSaveData> QuestSaveDataDictionary { get => questSaveDataDictionary; set => questSaveDataDictionary = value; }
        public Dictionary<string, QuestSaveData> AchievementSaveDataDictionary { get => achievementSaveDataDictionary; set => achievementSaveDataDictionary = value; }
        public Dictionary<string, Dictionary<string, Dictionary<string, QuestObjectiveSaveData>>> QuestObjectiveSaveDataDictionary { get => questObjectiveSaveDataDictionary; set => questObjectiveSaveDataDictionary = value; }
        public Dictionary<string, Dictionary<string, Dictionary<string, QuestObjectiveSaveData>>> AchievementObjectiveSaveDataDictionary { get => achievementObjectiveSaveDataDictionary; set => achievementObjectiveSaveDataDictionary = value; }

        public CharacterQuestLog(UnitController unitController, SystemGameManager systemGameManager) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterStats()");
            this.unitController = unitController;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            interactionManager = systemGameManager.InteractionManager;
            uIManager = systemGameManager.UIManager;
            dialogManager = systemGameManager.DialogManagerClient;
        }

        public void LoadQuest(QuestSaveData questSaveData) {

            Quest quest = systemDataFactory.GetResource<Quest>(questSaveData.QuestName);
            if (quest == null) {
                return;
            }
            if (!questSaveData.InLog) {
                return;
            }

            // change to new subscription method in quest to avoid duplicated out of date code not tracking newer objective types
            quest.AcceptQuest(unitController, false, false);
            // gotta check here because kills and ability use are not automatically checked on accept because under normal circumstances those amounts must start at 0
            quest.CheckCompletion(unitController, true, false);
            quests[quest.ResourceName] = quest;

            // just in case one quest was complete but not turned in
            //CheckCompletion();
        }

        public void AcceptQuest(Quest newQuest) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterQuestLog.AcceptQuest({newQuest.ResourceName})");

            if (quests.Count >= systemConfigurationManager.QuestLogSize) {
                // quest log is full. we can't accept the quest
                return;
            }
            // AVOID ACCIDENTALLY ACCEPTING TURNED IN QUESTS THAT ARE NOT REPEATABLE
            if (newQuest != null && (newQuest.TurnedIn(unitController) == false || newQuest.RepeatableQuest == true)) {
                // add first, then use acceptquest because it needs to be in the log for the accepquest completion check to pass
                quests[newQuest.ResourceName] = newQuest;
                newQuest.AcceptQuest(unitController);

                // if the quest has steps, then the completion check will be triggered by the objectives
                // if the quest has no steps, then checking completion should be done here
                if (newQuest.Steps.Count == 0) {
                    newQuest.CheckCompletion(unitController);
                }
            }
        }

        public bool HasQuest(string questName) {
            //Debug.Log("QuestLog.HasQuest(" + questName + ")");
            if (quests.ContainsKey(questName)) {
                return true;
            }
            return false;
        }

        public void AbandonQuest(Quest oldQuest, bool resetQuestStep = true) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterQuestLog.AbandonQuest({oldQuest.name})");

            RemoveQuest(oldQuest);
            // moved here instead of inside the above function so turnInQuest doesn't think a quest is available in the middle of turn-in
            oldQuest.RemoveQuest(unitController, resetQuestStep);
            unitController.UnitEventController.NotifyOnAbandonQuest(oldQuest);
        }

        public void TurnInQuest(Quest oldQuest) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterQuestLog.TurnInQuest()");

            // REMOVE FIRST SO WHEN TURNEDIN TRIGGERS STATUSUPDATED CALL, QUEST DOES NOT EXIST IN LOG SO SUBSCRIBERS GET CORRECT STATUS
            RemoveQuest(oldQuest);
            oldQuest.SetTurnedIn(unitController, true);

            // moved here instead of inside the above function so turnInQuest doesn't think a quest is available in the middle of turn-in
            oldQuest.RemoveQuest(unitController);
            unitController.UnitEventController.NotifyOnTurnInQuest(oldQuest);
        }

        public void RemoveQuest(Quest oldQuest) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterQuestLog.RemoveQuest({oldQuest.ResourceName})");

            if (quests.ContainsKey(oldQuest.ResourceName)) {
                quests.Remove(oldQuest.ResourceName);
            }
            // reset the quest objective save data so any completed portion is reset in case the quest is picked back up
            ResetQuestObjectiveSaveData(oldQuest.ResourceName);
        }

        public void ClearQuestLog() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterQuestLog.ClearQuestLog()");

            List<Quest> removeList = new List<Quest>();
            foreach (Quest quest in Quests.Values) {
                removeList.Add(quest);
            }
            foreach (Quest oldQuest in removeList) {
                RemoveQuest(oldQuest);
                oldQuest.RemoveQuest(unitController, false);
            }
            Quests.Clear();
        }

        public void ClearAchievementLog() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterQuestLog.ClearAchievementLog()");

            List<Achievement> removeList = new List<Achievement>();
            foreach (Achievement achievement in achievements.Values) {
                removeList.Add(achievement);
            }
            foreach (Achievement oldAchievement in removeList) {
                oldAchievement.RemoveQuest(unitController, false);
                ResetAchievementObjectiveSaveData(oldAchievement.ResourceName);
            }
            achievements.Clear();
        }


        public void ShowQuestLogDescription(Quest quest) {
            OnShowQuestLogDescription(quest);
        }

        public void ShowQuestGiverDescription(Quest quest, IQuestGiver questGiver) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterQuestLog.ShowQuestGiverDescription({quest.name})");

            OnShowQuestGiverDescription(quest, questGiver);
        }

        public List<Quest> GetCompleteQuests(List<QuestNode> questNodeArray, bool requireInQuestLog = false) {
            return GetQuestListByType("complete", questNodeArray, requireInQuestLog, false, true);
        }

        public List<Quest> GetInProgressQuests(List<QuestNode> questNodeArray, bool requireInQuestLog = true) {
            return GetQuestListByType("inprogress", questNodeArray, requireInQuestLog, false, true);
        }

        public List<Quest> GetAvailableQuests(List<QuestNode> questNodeArray, bool requireInQuestLog = false) {
            return GetQuestListByType("available", questNodeArray, requireInQuestLog, true, false);
        }

        public List<Quest> GetQuestListByType(string questStatusType, List<QuestNode> questNodeArray, bool requireInQuestLog = false, bool requireStartQuest = false, bool requireEndQuest = false) {
            List<Quest> returnList = new List<Quest>();
            foreach (QuestNode questNode in questNodeArray) {
                if (questNode.Quest != null) {
                    if (questNode.Quest.GetStatus(unitController) == questStatusType
                        && (requireInQuestLog == true ? HasQuest(questNode.Quest.ResourceName) : true)
                        && (requireStartQuest == true ? questNode.StartQuest : true)
                        && (requireEndQuest == true ? questNode.EndQuest : true)) {
                        //Debug.Log("Quest.GetQuestListByType(" + questStatusType + "): adding quest: " + questNode.MyQuest.DisplayName);
                        returnList.Add(questNode.Quest);
                    }
                }
            }
            return returnList;
        }

        public void SetQuestSaveData(string questName, QuestSaveData questSaveData) {
            if (questSaveDataDictionary.ContainsKey(questName)) {
                questSaveDataDictionary[questName] = questSaveData;
            } else {
                questSaveDataDictionary.Add(questName, questSaveData);
            }
        }

        public void SetAchievementSaveData(string questName, QuestSaveData questSaveData) {
            if (achievementSaveDataDictionary.ContainsKey(questName)) {
                achievementSaveDataDictionary[questName] = questSaveData;
            } else {
                achievementSaveDataDictionary.Add(questName, questSaveData);
            }
        }

        public QuestSaveData GetQuestSaveData(QuestBase quest) {
            QuestSaveData saveData;
            if (questSaveDataDictionary.ContainsKey(quest.ResourceName)) {
                saveData = questSaveDataDictionary[quest.ResourceName];
            } else {
                saveData = new QuestSaveData();
                saveData.QuestName = quest.ResourceName;
                questSaveDataDictionary.Add(quest.ResourceName, saveData);
            }
            return saveData;
        }

        public QuestSaveData GetAchievementSaveData(QuestBase quest) {
            QuestSaveData saveData;
            if (achievementSaveDataDictionary.ContainsKey(quest.ResourceName)) {
                saveData = achievementSaveDataDictionary[quest.ResourceName];
                //Debug.Log($"{unitController.gameObject.name}.CharacterQuestLog.GetAchievementSaveData({quest.ResourceName}) - using existing achievement save data");
            } else {
                //Debug.Log($"{unitController.gameObject.name}.CharacterQuestLog.GetAchievementSaveData({quest.ResourceName}) - creating new achievement save data");
                saveData = new QuestSaveData();
                saveData.QuestName = quest.ResourceName;
                achievementSaveDataDictionary.Add(quest.ResourceName, saveData);
            }
            return saveData;
        }

        public void ResetQuestObjectiveSaveData(string questName) {
            questObjectiveSaveDataDictionary[questName] = new Dictionary<string, Dictionary<string, QuestObjectiveSaveData>>();
        }

        public void ResetAchievementObjectiveSaveData(string questName) {
            achievementObjectiveSaveDataDictionary[questName] = new Dictionary<string, Dictionary<string, QuestObjectiveSaveData>>();
        }


        public QuestObjectiveSaveData GetObjectiveSaveData(Dictionary<string, Dictionary<string, Dictionary<string, QuestObjectiveSaveData>>> dictionary, string questName, string objectiveType, string objectiveName) {
            QuestObjectiveSaveData saveData;

            // first, check if this quest is in the main objective dictionary.  If not, add it.
            Dictionary<string, Dictionary<string, QuestObjectiveSaveData>> questObjectiveSaveData;
            if (dictionary.ContainsKey(questName)) {
                questObjectiveSaveData = dictionary[questName];
            } else {
                questObjectiveSaveData = new Dictionary<string, Dictionary<string, QuestObjectiveSaveData>>();
                dictionary.Add(questName, questObjectiveSaveData);
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

        public QuestObjectiveSaveData GetAchievementObjectiveSaveData(string questName, string objectiveType, string objectiveName) {

            return GetObjectiveSaveData(achievementObjectiveSaveDataDictionary, questName, objectiveType, objectiveName);
        }

        public QuestObjectiveSaveData GetQuestObjectiveSaveData(string questName, string objectiveType, string objectiveName) {

            return GetObjectiveSaveData(questObjectiveSaveDataDictionary, questName, objectiveType, objectiveName);
        }

        public void LoadAchievement(QuestSaveData questSaveData) {

            Achievement achievement = systemDataFactory.GetResource<Achievement>(questSaveData.QuestName);
            if (achievement == null) {
                return;
            }
            if (!questSaveData.InLog) {
                return;
            }

            LoadAchievement(achievement, questSaveData);
        }

        public void LoadAchievement(Achievement achievement, QuestSaveData questSaveData) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterQuestLog.LoadAchievement({achievement.ResourceName})");

            if (achievements.ContainsKey(achievement.ResourceName)) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterQuestLog.LoadAchievement({achievement.ResourceName}) already in log");
                return;
            }
            achievements[achievement.ResourceName] = achievement;
            if (questSaveData.MarkedComplete == true) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterQuestLog.LoadAchievement({achievement.ResourceName}): already marked complete");
                achievementSaveDataDictionary[achievement.ResourceName] = questSaveData;
                return;
            }
            // change to new subscription method in quest to avoid duplicated out of date code not tracking newer objective types
            achievement.AcceptQuest(unitController, false, false);
            // gotta check here because kills and ability use are not automatically checked on accept because under normal circumstances those amounts must start at 0
            achievement.CheckCompletion(unitController, true, false);
        }

        public void AcceptAchievement(Achievement achievement) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterQuestLog.AcceptAchievement({achievement.ResourceName})");

            if (achievements.ContainsKey(achievement.ResourceName)) {
                //Debug.Log("QuestLog.AcceptAchievement(" + achievement.ResourceName + "): already in log");
                return;
            }

            achievements[achievement.ResourceName] = achievement;
            // change to new subscription method in quest to avoid duplicated out of date code not tracking newer objective types
            achievement.AcceptQuest(unitController, false, false);
            // gotta check here because kills and ability use are not automatically checked on accept because under normal circumstances those amounts must start at 0
            achievement.CheckCompletion(unitController, true, false);
        }

        public bool HasAchievement(string achievementName) {
            //Debug.Log("QuestLog.HasAchievement(" + questName + ")");
            if (achievements.ContainsKey(achievementName)) {
                return true;
            }
            return false;
        }

        public void SetQuestObjectiveCurrentAmount(string questName, string objectiveType, string objectiveName, int amount) {
            QuestObjectiveSaveData saveData = GetQuestObjectiveSaveData(questName, objectiveType, objectiveName);
            saveData.Amount = amount;
            SetQuestObjectiveCurrentAmount(questName, objectiveType, objectiveName, saveData);
        }

        private void SetQuestObjectiveCurrentAmount(string questName, string objectiveType, string objectiveName, QuestObjectiveSaveData saveData) {
            questObjectiveSaveDataDictionary[questName][objectiveType][objectiveName] = saveData;
            unitController.UnitEventController.NotifyOnSetQuestObjectiveCurrentAmount(questName, objectiveType, objectiveName, saveData.Amount);
        }

        public void SetAchievementObjectiveCurrentAmount(string questName, string objectiveType, string objectiveName, int amount) {
            QuestObjectiveSaveData saveData = GetAchievementObjectiveSaveData(questName, objectiveType, objectiveName);
            saveData.Amount = amount;
            SetAchievementObjectiveCurrentAmount(questName, objectiveType, objectiveName, saveData);
        }

        private void SetAchievementObjectiveCurrentAmount(string questName, string objectiveType, string objectiveName, QuestObjectiveSaveData saveData) {
            achievementObjectiveSaveDataDictionary[questName][objectiveType][objectiveName] = saveData;
            unitController.UnitEventController.NotifyOnSetAchievementObjectiveCurrentAmount(questName, objectiveType, objectiveName, saveData.Amount);
        }

        public void MarkQuestComplete(Quest quest) {
            quest.MarkComplete(unitController, true, false);
        }

        public void MarkAchievementComplete(Achievement achievement) {
            achievement.MarkComplete(unitController, true, false);
        }

        public void HandleCharacterUnitDespawn() {
            ClearQuestLog();
            ClearAchievementLog();
        }

        public void InteractWithQuestStartItem(Quest quest, int slotIndex, long itemInstanceId) {
            if (uIManager.questGiverWindow.IsOpen) {
                // safety to prevent deletion
                return;
            }
            if (unitController.CharacterInventoryManager.InventorySlots.Count > slotIndex
                && unitController.CharacterInventoryManager.InventorySlots[slotIndex].InstantiatedItem.InstanceId == itemInstanceId) {
                ShowQuestGiverDescription(quest, unitController.CharacterInventoryManager.InventorySlots[slotIndex].InstantiatedItem as InstantiatedQuestStartItem);
            }

        }

        public void RequestAcceptQuestItemQuest(InstantiatedQuestStartItem instantiatedQuestStartItem, Quest currentQuest) {
            if (systemGameManager.GameMode == GameMode.Local) {
                AcceptQuestItemQuest(instantiatedQuestStartItem, currentQuest);
            } else {
                unitController.UnitEventController.NotifyOnRequestAcceptQuestItemQuest(instantiatedQuestStartItem.Slot.GetCurrentInventorySlotIndex(unitController), instantiatedQuestStartItem.InstanceId, currentQuest);
            }
        }

        public void AcceptQuestItemQuest(InstantiatedQuestStartItem instantiatedQuestStartItem, Quest currentQuest) {
            AcceptQuest(currentQuest);
            instantiatedQuestStartItem.HandleAcceptQuest();
        }

        public void CompleteQuestItemQuest(InstantiatedQuestStartItem instantiatedQuestStartItem, Quest currentQuest, QuestRewardChoices questRewardChoices) {
            instantiatedQuestStartItem.CompleteQuest(unitController, currentQuest, questRewardChoices);
        }

        public void RequestCompleteQuestItemQuest(InstantiatedQuestStartItem instantiatedQuestStartItem, Quest currentQuest, QuestRewardChoices questRewardChoices) {
            if (systemGameManager.GameMode == GameMode.Local) {
                CompleteQuestItemQuest(instantiatedQuestStartItem, currentQuest, questRewardChoices);
            } else {
                unitController.UnitEventController.NotifyOnRequestCompleteQuestItemQuest(instantiatedQuestStartItem.Slot.GetCurrentInventorySlotIndex(unitController), instantiatedQuestStartItem.InstanceId, currentQuest, questRewardChoices);
            }
        }
    }
}