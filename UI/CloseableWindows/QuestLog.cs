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
    public class QuestLog : MonoBehaviour {

        #region Singleton
        private static QuestLog instance;

        public static QuestLog MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<QuestLog>();
                }

                return instance;
            }
        }

        #endregion

        [SerializeField]
        private int maxCount = 0;

        private Dictionary<string, Quest> quests = new Dictionary<string, Quest>();

        public Dictionary<string, Quest> MyQuests { get => quests; }

        private void Start() {
        }

        public void LoadQuest(QuestSaveData questSaveData) {
            //Debug.Log("QuestLog.LoadQuest(" + questSaveData.MyName + ")");

            Quest quest = SystemQuestManager.MyInstance.GetResource(questSaveData.MyName);
            if (quest == null) {
                //Debug.Log("QuestLog.LoadQuest(" + questSaveData.MyName + "): COULD NOT FIND QUEST!!!");
                return;
            }
            if (!questSaveData.inLog) {
                //Debug.Log("QuestLog.LoadQuest(" + questSaveData.MyName + "): quest is not in log. turnedIn: " + questSaveData.turnedIn);
                //quest.TurnedIn = questSaveData.turnedIn;
                quest.SetTurnedIn(questSaveData.turnedIn, false);
                return;
            }


            // add tracked objectives to quest
            List<KillObjective> killObjectiveSaveDataList = new List<KillObjective>();
            foreach (QuestObjectiveSaveData objectiveSaveData in questSaveData.killObjectives) {
                //Debug.Log("QuestLog.LoadQuest(" + questSaveData.MyName + "): loading kill objectives");
                foreach (QuestObjective existingQuestObjective in quest.MyKillObjectives) {
                    //Debug.Log("QuestLog.LoadQuest(" + questSaveData.MyName + "): loading kill objective: " + existingQuestObjective.MyType);
                    if (SystemResourceManager.MatchResource(existingQuestObjective.MyType, objectiveSaveData.MyName)) {
                        //Debug.Log("QuestLog.LoadQuest(" + questSaveData.MyName + "): loading kill objective: " + existingQuestObjective.MyType + " matches!!! myamount: " + objectiveSaveData.MyAmount);
                        existingQuestObjective.MyCurrentAmount = objectiveSaveData.MyAmount;
                    }
                }
            }
            List<CollectObjective> collectObjectiveSaveDataList = new List<CollectObjective>();
            foreach (QuestObjectiveSaveData objectiveSaveData in questSaveData.collectObjectives) {
                foreach (QuestObjective existingQuestObjective in quest.MyCollectObjectives) {
                    if (SystemResourceManager.MatchResource(existingQuestObjective.MyType, objectiveSaveData.MyName)) {
                        existingQuestObjective.MyCurrentAmount = objectiveSaveData.MyAmount;
                    }
                }
            }
            List<TradeSkillObjective> tradeSkillObjectiveSaveDataList = new List<TradeSkillObjective>();
            foreach (QuestObjectiveSaveData objectiveSaveData in questSaveData.tradeSkillObjectives) {
                //Debug.Log("QuestLog.LoadQuest(" + questSaveData.MyName + "): loading tradeskill objective");
                foreach (QuestObjective existingQuestObjective in quest.MyTradeSkillObjectives) {
                    if (SystemResourceManager.MatchResource(existingQuestObjective.MyType, objectiveSaveData.MyName)) {
                        existingQuestObjective.MyCurrentAmount = objectiveSaveData.MyAmount;
                    }
                }
            }
            List<UseInteractableObjective> useInteractableObjectiveSaveDataList = new List<UseInteractableObjective>();
            foreach (QuestObjectiveSaveData objectiveSaveData in questSaveData.useInteractableObjectives) {
                foreach (QuestObjective existingQuestObjective in quest.MyUseInteractableObjectives) {
                    if (SystemResourceManager.MatchResource(existingQuestObjective.MyType, objectiveSaveData.MyName)) {
                        existingQuestObjective.MyCurrentAmount = objectiveSaveData.MyAmount;
                    }
                }
            }

            List<AbilityObjective> abilityObjectiveSaveDataList = new List<AbilityObjective>();
            foreach (QuestObjectiveSaveData objectiveSaveData in questSaveData.abilityObjectives) {
                foreach (QuestObjective existingQuestObjective in quest.MyAbilityObjectives) {
                    if (SystemResourceManager.MatchResource(existingQuestObjective.MyType, objectiveSaveData.MyName)) {
                        //Debug.Log("QuestLog.LoadQuest(" + questSaveData.MyName + "): loading ability objective: " + existingQuestObjective.MyType + " matches!!! myamount: " + objectiveSaveData.MyAmount);
                        existingQuestObjective.MyCurrentAmount = objectiveSaveData.MyAmount;
                    }
                }
            }

            // just in case one quest was complete but not turned in
            //CheckCompletion();
        }

        public void AcceptQuest(QuestSaveData questSaveData) {
            //Debug.Log("QuestLog.LoadQuest(" + questSaveData.MyName + ")");

            Quest quest = SystemQuestManager.MyInstance.GetResource(questSaveData.MyName);
            if (quest == null) {
                //Debug.Log("QuestLog.LoadQuest(" + questSaveData.MyName + "): COULD NOT FIND QUEST!!!");
                return;
            }
            if (!questSaveData.inLog) {
                //Debug.Log("QuestLog.LoadQuest(" + questSaveData.MyName + "): quest is not in log. turnedIn: " + questSaveData.turnedIn);
                return;
            }

            // change to new subscription method in quest to avoid duplicated out of date code not tracking newer objective types
            quest.AcceptQuest(false);
            // gotta check here because kills and ability use are not automatically checked on accept because under normal circumstances those amounts must start at 0
            quest.CheckCompletion(true, false);
            string keyName = SystemResourceManager.prepareStringForMatch(quest.MyName);
            quests[keyName] = quest;

            // just in case one quest was complete but not turned in
            //CheckCompletion();
        }

        public void AcceptQuest(Quest newQuest) {
            //Debug.Log("QuestLog.AcceptQuest(" + quest.name + ")");
            if (quests.Count >= maxCount) {
                // quest log is full. we can't accept the quest
                return;
            }
            // AVOID ACCIDENTALLY ACCEPTING TURNED IN QUESTS THAT ARE NOT REPEATABLE
            if (newQuest != null && (newQuest.TurnedIn == false || newQuest.MyRepeatableQuest == true)) {
                // add first, then use acceptquest because it needs to be in the log for the accepquest completion check to pass
                string keyName = SystemResourceManager.prepareStringForMatch(newQuest.MyName);
                quests[keyName] = newQuest;
                newQuest.AcceptQuest();
                //CheckCompletion();
            }
        }

        public bool HasQuest(string questName) {
            //Debug.Log("QuestLog.HasQuest(" + questName + ")");
            string keyName = SystemResourceManager.prepareStringForMatch(questName);
            if (quests.ContainsKey(keyName)) {
                return true;
            }
            return false;
        }

        public void AbandonQuest(Quest oldQuest) {
            //Debug.Log("QuestLog.AbandonQuest(" + quest.name + ")");
            oldQuest.OnAbandonQuest();
            RemoveQuest(oldQuest);
            SystemEventManager.MyInstance.NotifyOnQuestStatusUpdated();
        }

        public void TurnInQuest(Quest oldQuest) {
            //Debug.Log("QuestLog.TurnInQuest()");
            // REMOVE FIRST SO WHEN TURNEDIN TRIGGERS STATUSUPDATED CALL, QUEST DOES NOT EXIST IN LOG SO SUBSCRIBERS GET CORRECT STATUS
            RemoveQuest(oldQuest);
            oldQuest.SetTurnedIn(true);
            oldQuest.OnAbandonQuest();
            // moved here from questgiverUI
        }

        public void RemoveQuest(Quest oldQuest) {
            //Debug.Log("QuestLog.RemoveQuest()");
            string keyName = SystemResourceManager.prepareStringForMatch(oldQuest.MyName);
            if (quests.ContainsKey(keyName)) {
                quests.Remove(keyName);
            }
        }

        public void ClearLog() {
            List<Quest> removeList = new List<Quest>();
            foreach (Quest quest in MyQuests.Values) {
                removeList.Add(quest);
            }
            foreach (Quest oldQuest in removeList) {
                AbandonQuest(oldQuest);
            }
            MyQuests.Clear();
        }
    }

}