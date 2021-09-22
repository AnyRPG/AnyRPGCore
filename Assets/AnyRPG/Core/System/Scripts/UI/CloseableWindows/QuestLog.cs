using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    /// <summary>
    /// Maintains a list of all quests
    /// </summary>
    public class QuestLog : ConfiguredMonoBehaviour {

        public event System.Action<Quest> OnShowQuestLogDescription = delegate { };
        public event System.Action<Quest, IQuestGiver> OnShowQuestGiverDescription = delegate { };

        private Dictionary<string, Quest> quests = new Dictionary<string, Quest>();

        // game manager references
        SystemDataFactory systemDataFactory = null;

        public Dictionary<string, Quest> Quests { get => quests; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemDataFactory = systemGameManager.SystemDataFactory;
        }

        public void AcceptQuest(QuestSaveData questSaveData) {
            //Debug.Log("QuestLog.LoadQuest(" + questSaveData.MyName + ")");

            Quest quest = systemDataFactory.GetResource<Quest>(questSaveData.QuestName);
            if (quest == null) {
                //Debug.Log("QuestLog.LoadQuest(" + questSaveData.MyName + "): COULD NOT FIND QUEST!!!");
                return;
            }
            if (!questSaveData.inLog) {
                //Debug.Log("QuestLog.LoadQuest(" + questSaveData.MyName + "): quest is not in log. turnedIn: " + questSaveData.turnedIn);
                return;
            }

            // change to new subscription method in quest to avoid duplicated out of date code not tracking newer objective types
            quest.AcceptQuest(false, false);
            // gotta check here because kills and ability use are not automatically checked on accept because under normal circumstances those amounts must start at 0
            quest.CheckCompletion(true, false);
            string keyName = SystemDataFactory.PrepareStringForMatch(quest.DisplayName);
            quests[keyName] = quest;

            // just in case one quest was complete but not turned in
            //CheckCompletion();
        }

        public void AcceptQuest(Quest newQuest) {
            //Debug.Log("QuestLog.AcceptQuest(" + quest.name + ")");
            if (quests.Count >= systemConfigurationManager.QuestLogSize) {
                // quest log is full. we can't accept the quest
                return;
            }
            // AVOID ACCIDENTALLY ACCEPTING TURNED IN QUESTS THAT ARE NOT REPEATABLE
            if (newQuest != null && (newQuest.TurnedIn == false || newQuest.RepeatableQuest == true)) {
                // add first, then use acceptquest because it needs to be in the log for the accepquest completion check to pass
                string keyName = SystemDataFactory.PrepareStringForMatch(newQuest.DisplayName);
                quests[keyName] = newQuest;
                newQuest.AcceptQuest();
                //CheckCompletion();
            }
        }

        public bool HasQuest(string questName) {
            //Debug.Log("QuestLog.HasQuest(" + questName + ")");
            string keyName = SystemDataFactory.PrepareStringForMatch(questName);
            if (quests.ContainsKey(keyName)) {
                return true;
            }
            return false;
        }

        public void AbandonQuest(Quest oldQuest) {
            //Debug.Log("QuestLog.AbandonQuest(" + quest.name + ")");
            RemoveQuest(oldQuest);

            // moved here instead of inside the above function so turnInQuest doesn't think a quest is available in the middle of turn-in
            oldQuest.RemoveQuest();
        }

        public void TurnInQuest(Quest oldQuest) {
            //Debug.Log("QuestLog.TurnInQuest()");
            // REMOVE FIRST SO WHEN TURNEDIN TRIGGERS STATUSUPDATED CALL, QUEST DOES NOT EXIST IN LOG SO SUBSCRIBERS GET CORRECT STATUS
            RemoveQuest(oldQuest);
            oldQuest.SetTurnedIn(true);

            // moved here instead of inside the above function so turnInQuest doesn't think a quest is available in the middle of turn-in
            oldQuest.RemoveQuest();
        }

        public void RemoveQuest(Quest oldQuest) {
            //Debug.Log("QuestLog.RemoveQuest()");
            string keyName = SystemDataFactory.PrepareStringForMatch(oldQuest.DisplayName);
            if (quests.ContainsKey(keyName)) {
                quests.Remove(keyName);
            }
        }

        public void ClearLog() {
            List<Quest> removeList = new List<Quest>();
            foreach (Quest quest in Quests.Values) {
                removeList.Add(quest);
            }
            foreach (Quest oldQuest in removeList) {
                AbandonQuest(oldQuest);
            }
            Quests.Clear();
        }

        public void ShowQuestLogDescription(Quest quest) {
            OnShowQuestLogDescription(quest);
        }

        public void ShowQuestGiverDescription(Quest quest, IQuestGiver questGiver) {
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
                    if (questNode.Quest.GetStatus() == questStatusType
                        && (requireInQuestLog == true ? HasQuest(questNode.Quest.DisplayName) : true)
                        && (requireStartQuest == true ? questNode.StartQuest : true)
                        && (requireEndQuest == true ? questNode.EndQuest : true)) {
                        //Debug.Log("Quest.GetQuestListByType(" + questStatusType + "): adding quest: " + questNode.MyQuest.DisplayName);
                        returnList.Add(questNode.Quest);
                    }
                }
            }
            return returnList;
        }
    }

}