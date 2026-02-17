using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {
    [System.Serializable]
    public class QuestObjective : ConfiguredClass {
        [SerializeField]
        private int amount = 1;

        protected QuestBase questBase;

        [Tooltip("Set this if you want to override the name shown in the quest log objective to be something other than the type")]
        [SerializeField]
        private string overrideDisplayName = string.Empty;

        // game manager references
        protected SaveManager saveManager = null;
        protected MessageFeedManager messageFeedManager = null;
        protected SystemEventManager systemEventManager = null;
        protected PlayerManagerClient playerManager = null;
        protected LevelManager levelManager = null;

        public int Amount {
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

        public virtual string ObjectiveName { get => string.Empty; }

        public QuestBase QuestBase { get => questBase; set => questBase = value; }
        public string OverrideDisplayName { get => overrideDisplayName; set => overrideDisplayName = value; }
        public string DisplayName {
            get {
                if (overrideDisplayName != string.Empty) {
                    return overrideDisplayName;
                }
                return ObjectiveName;
            }
            set => overrideDisplayName = value;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            saveManager = systemGameManager.SaveManager;
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;
            systemEventManager = systemGameManager.SystemEventManager;
            playerManager = systemGameManager.PlayerManager;
            levelManager = systemGameManager.LevelManager;
        }

        public virtual int CurrentAmount(UnitController sourceUnitController) {
            //return sourceUnitController.CharacterQuestLog.GetQuestObjectiveSaveData(questBase.ResourceName, ObjectiveType.Name, ObjectiveName).Amount;
            return questBase.GetObjectiveCurrentAmount(sourceUnitController, ObjectiveType.Name, ObjectiveName);
        }

        public void SetCurrentAmount(UnitController sourceUnitController, int value) {
            questBase.SetObjectiveCurrentAmount(sourceUnitController, ObjectiveType.Name, ObjectiveName, value);
        }


        public virtual bool IsComplete(UnitController sourceUnitController) {
            //Debug.Log("checking if quest objective iscomplete, current: " + MyCurrentAmount.ToString() + "; needed: " + amount.ToString());
            return CurrentAmount(sourceUnitController) >= Amount;
        }

        public virtual void UpdateCompletionCount(UnitController sourceUnitController, bool printMessages = true) {
            //Debug.Log("QuestObjective.UpdateCompletionCount()");
        }

        public virtual void OnAcceptQuest(UnitController sourceUnitController, QuestBase questBase, bool printMessages = true) {
            this.questBase = questBase;
        }

        private void SetQuest(QuestBase questBase) {
            this.questBase = questBase;
        }

        public virtual void OnAbandonQuest(UnitController sourceUnitController) {
            // overwrite me
        }

        public virtual void HandleQuestStatusUpdated(UnitController sourceUnitController) {
            UpdateCompletionCount(sourceUnitController);
        }

        public virtual string GetUnformattedStatus(UnitController sourceUnitController) {
            return DisplayName + ": " + Mathf.Clamp(CurrentAmount(sourceUnitController), 0, Amount) + "/" + Amount;
        }

        public virtual void SetupScriptableObjects(SystemGameManager systemGameManager, QuestBase quest) {
            Configure(systemGameManager);
            SetQuest(quest);
        }

    }


}