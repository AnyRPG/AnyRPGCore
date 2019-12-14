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

        /*
        [SerializeField]
        private BaseCharacter killType;
        */

        // this will be much better if we make this a reference to the actual class

        [SerializeField]
        private string type;

        public int MyAmount {
            get {
                return (int)Mathf.Clamp(amount, 1, Mathf.Infinity);
            }
            set {
                amount = value;
            }
        }

        public int MyCurrentAmount { get => currentAmount; set => currentAmount = value; }
        public string MyType { get => type; set => type = value; }

        public virtual bool IsComplete {
            get {
                //Debug.Log("checking if quest objective iscomplete, current: " + MyCurrentAmount.ToString() + "; needed: " + amount.ToString());
                return MyCurrentAmount >= MyAmount;
            }
        }

        public Quest MyQuest { get => quest; set => quest = value; }

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
    }


}