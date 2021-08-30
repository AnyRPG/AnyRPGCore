using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public abstract class QuestObjective : ConfiguredClass {
        [SerializeField]
        private int amount;

        protected Quest quest;

        [SerializeField]
        private string type = string.Empty;

        [Tooltip("Set this if you want to override the name shown in the quest log objective to be something other than the type")]
        [SerializeField]
        private string overrideDisplayName = string.Empty;

        // game manager references
        protected SaveManager saveManager = null;
        protected SystemDataFactory systemDataFactory = null;
        protected MessageFeedManager messageFeedManager = null;
        protected SystemEventManager systemEventManager = null;
        protected PlayerManager playerManager = null;

        public int MyAmount {
            get {
                return (int)Mathf.Clamp(amount, 1, Mathf.Infinity);
            }
            set {
                amount = value;
            }
        }

        public virtual Type ObjectiveType {
            get {
                return typeof(QuestObjective);
            }
        }

        public int CurrentAmount {
            get {
                return saveManager.GetQuestObjectiveSaveData(quest.DisplayName, ObjectiveType, MyType).MyAmount;
                //return false;
            }
            set {
                QuestObjectiveSaveData saveData = saveManager.GetQuestObjectiveSaveData(quest.DisplayName, ObjectiveType, MyType);
                saveData.MyAmount = value;
                saveManager.QuestObjectiveSaveDataDictionary[quest.DisplayName][ObjectiveType][MyType] = saveData;
            }
        }

        public string MyType { get => type; set => type = value; }

        public virtual bool IsComplete {
            get {
                //Debug.Log("checking if quest objective iscomplete, current: " + MyCurrentAmount.ToString() + "; needed: " + amount.ToString());
                return CurrentAmount >= MyAmount;
            }
        }

        public Quest MyQuest { get => quest; set => quest = value; }
        public string OverrideDisplayName { get => overrideDisplayName; set => overrideDisplayName = value; }
        public string DisplayName {
            get {
                if (overrideDisplayName != string.Empty) {
                    return overrideDisplayName;
                }
                return type;
            }
            set => overrideDisplayName = value;
        }

        public virtual void UpdateCompletionCount(bool printMessages = true) {
            //Debug.Log("QuestObjective.UpdateCompletionCount()");
        }

        public virtual void OnAcceptQuest(Quest quest, bool printMessages = true) {
            this.quest = quest;
        }

        public virtual void SetQuest(Quest quest) {
            this.quest = quest;
        }

        public virtual void OnAbandonQuest() {
            // overwrite me
        }

        public virtual void HandleQuestStatusUpdated() {
            UpdateCompletionCount();
        }

        public virtual void SetupScriptableObjects(SystemGameManager systemGameManager) {
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            saveManager = systemGameManager.SaveManager;
            systemDataFactory = systemGameManager.SystemDataFactory;
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;
            systemEventManager = systemGameManager.SystemEventManager;
            playerManager = systemGameManager.PlayerManager;
        }
    }


}