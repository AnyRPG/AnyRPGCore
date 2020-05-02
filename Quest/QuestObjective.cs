using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public abstract class QuestObjective {
        [SerializeField]
        private int amount;

        private int currentAmount;

        protected Quest quest;

        [SerializeField]
        private string type = string.Empty;

        [Tooltip("Set this if you want to override the name shown in the quest log objective to be something other than the type")]
        [SerializeField]
        private string overrideDisplayName = string.Empty;

        public int MyAmount {
            get {
                return (int)Mathf.Clamp(amount, 1, Mathf.Infinity);
            }
            set {
                amount = value;
            }
        }

        public int CurrentAmount { get => currentAmount; set => currentAmount = value; }
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

        public virtual void OnAbandonQuest() {
            // overwrite me
        }

        public virtual void HandleQuestStatusUpdated() {
            UpdateCompletionCount();
        }

        public virtual void SetupScriptableObjects() {
            // overwrite me
        }
    }


}