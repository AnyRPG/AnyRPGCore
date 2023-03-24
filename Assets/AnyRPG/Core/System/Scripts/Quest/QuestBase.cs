using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {

    public abstract class QuestBase : DescribableResource, IPrerequisiteOwner {

        public event System.Action OnQuestStatusUpdated = delegate { };
        public event System.Action OnQuestObjectiveStatusUpdated = delegate { };

        [Header("Objectives")]

        [SerializeField]
        protected List<QuestStep> steps = new List<QuestStep>();

        [Header("Prerequisites")]

        [SerializeField]
        protected List<PrerequisiteConditions> prerequisiteConditions = new List<PrerequisiteConditions>();

        protected Quest questTemplate = null;

        // game manager references
        protected SaveManager saveManager = null;
        protected PlayerManager playerManager = null;
        protected MessageFeedManager messageFeedManager = null;

        public virtual bool PrintObjectiveCompletionMessages {
            get => false;
        }

        public virtual bool IsComplete {
            get {
                //Debug.Log("Quest.IsComplete: " + MyTitle);
                // disabled because if a quest is raw completable (not required to be in log), it shouldn't have objectives anyway since there is no way to track them
                // therefore the default true at the bottom should return true anyway
                /*
                if (MyAllowRawComplete == true) {
                    return true;
                }
                */

                if (steps.Count > 0) {
                    foreach (QuestObjective questObjective in steps[steps.Count -1].QuestObjectives) {
                        if (!questObjective.IsComplete) {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        public virtual void SetTurnedIn(bool turnedIn, bool notify = true) {
            this.TurnedIn = turnedIn;
            //Debug.Log(DisplayName + ".Quest.TurnedIn = " + value);
            if (notify) {
                if (playerManager.PlayerUnitSpawned == false) {
                    // STOP STUFF FROM REACTING WHEN PLAYER ISN'T SPAWNED
                    return;
                }
                SystemEventManager.TriggerEvent("OnQuestStatusUpdated", new EventParamProperties());
                SystemEventManager.TriggerEvent("OnAfterQuestStatusUpdated", new EventParamProperties());
                OnQuestStatusUpdated();
            }
        }

        public virtual bool PrerequisitesMet {
            get {
                foreach (PrerequisiteConditions prerequisiteCondition in prerequisiteConditions) {
                    if (!prerequisiteCondition.IsMet()) {
                        return false;
                    }
                }
                // there are no prerequisites, or all prerequisites are complete
                return true;
            }
        }

        public virtual Quest QuestTemplate { get => questTemplate; set => questTemplate = value; }

        public bool TurnedIn {
            get {
                return GetSaveData().turnedIn;
                //return false;
            }
            set {
                QuestSaveData saveData = GetSaveData();
                saveData.turnedIn = value;
                SetSaveData(saveData.QuestName, saveData);
            }
        }

        public int CurrentStep {
            get {
                return GetSaveData().questStep;
                //return false;
            }
            set {
                QuestSaveData saveData = GetSaveData();
                saveData.questStep = value;
                SetSaveData(saveData.QuestName, saveData);
            }
        }
        public bool MarkedComplete {
            get {
                return GetSaveData().markedComplete;
                //return false;
            }
            set {
                QuestSaveData saveData = GetSaveData();
                saveData.markedComplete = value;
                SetSaveData(saveData.QuestName, saveData);
            }
        }

        public List<QuestStep> Steps { get => steps; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            saveManager = systemGameManager.SaveManager;
            playerManager = systemGameManager.PlayerManager;
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;
        }

        protected abstract void ResetObjectiveSaveData();
        protected abstract QuestSaveData GetSaveData();
        protected abstract void SetSaveData(string QuestName, QuestSaveData questSaveData);
        protected abstract bool HasQuest();

        public virtual void RemoveQuest(bool resetQuestStep = true) {
            //Debug.Log("Quest.RemoveQuest(): " + DisplayName + " calling OnQuestStatusUpdated()");

            OnAbandonQuest();

            // reset the quest objective save data so any completed portion is reset in case the quest is picked back up
            ResetObjectiveSaveData();

            // reset current step so the correct objective shows up in the quest giver window when the quest is picked back up
            if (resetQuestStep == true) {
                CurrentStep = 0;
            }
            MarkedComplete = false;

            if (playerManager != null && playerManager.PlayerUnitSpawned == false) {
                // STOP STUFF FROM REACTING WHEN PLAYER ISN'T SPAWNED
                return;
            }
            SystemEventManager.TriggerEvent("OnQuestStatusUpdated", new EventParamProperties());
            SystemEventManager.TriggerEvent("OnAfterQuestStatusUpdated", new EventParamProperties());
            OnQuestStatusUpdated();
        }

        protected virtual void ProcessMarkComplete(bool printMessages) {
            // nothing to do here
        }

        public virtual void MarkComplete(bool notifyOnUpdate = true, bool printMessages = true) {
            if (MarkedComplete == true) {
                return;
            }

            ProcessMarkComplete(printMessages);

            MarkedComplete = true;
            if (notifyOnUpdate == true) {
                if (playerManager != null && playerManager.PlayerUnitSpawned == false) {
                    // STOP STUFF FROM REACTING WHEN PLAYER ISN'T SPAWNED
                    return;
                }
                SystemEventManager.TriggerEvent("OnQuestStatusUpdated", new EventParamProperties());
                SystemEventManager.TriggerEvent("OnAfterQuestStatusUpdated", new EventParamProperties());
                OnQuestStatusUpdated();
            }
        }

        public virtual void OnAbandonQuest() {
            
            if (steps.Count == 0) {
                return;
            }

            foreach (QuestObjective questObjective in steps[CurrentStep].QuestObjectives) {
                questObjective.OnAbandonQuest();
            }

        }

        public virtual string GetStatus() {
            //Debug.Log(DisplayName + ".Quest.GetStatus()");

            if (StatusCompleted()) {
                //Debug.Log(DisplayName + ".Quest.GetStatus(): returning completed");
                return "completed";
            }

            if (StatusComplete()) {
                return "complete";
            }

            if (StatusInProgress()) {
                return "inprogress";
            }

            if (StatusAvailable()) {
                //Debug.Log(DisplayName + ".Quest.GetStatus(): returning available");
                return "available";
            }

            // this quest prerequisites were not met
            //Debug.Log(DisplayName + ".Quest.GetStatus(): returning unavailable");
            return "unavailable";
        }

        protected virtual bool StatusCompleted() {
            if (TurnedIn == true) {
                return true;
            }

            return false;
        }

        protected virtual bool StatusAvailable() {
            if (PrerequisitesMet == false) {
                return false;
            }

            if (HasQuest() == true) {
                return false;
            }

            return true;
        }

        protected virtual bool StatusComplete() {
            if (HasQuest() && IsComplete == true) {
                //Debug.Log(DisplayName + ".Quest.GetStatus(): returning complete");
                return true;
            }

            return false;
        }

        protected virtual bool StatusInProgress() {
            if (HasQuest() && IsComplete == false) {
                //Debug.Log(DisplayName + ".Quest.GetStatus(): returning inprogress");
                return true;
            }

            return false;
        }

        public override string GetDescription() {
            //return string.Format("{0}\n{1} Points", description, baseExperienceReward);
            return string.Format("{0}", description);
        }

        protected virtual Color GetTitleColor() {
            return Color.yellow;
        }

        public virtual string GetObjectiveDescription() {

            Color titleColor = GetTitleColor();
            return string.Format("<size=30><b><color=#{0}>{1}</color></b></size>\n\n<size=18>{2}</size>\n\n<b><size=24>Objectives:</size></b>\n\n<size=18>{3}</size>", ColorUtility.ToHtmlStringRGB(titleColor), DisplayName, Description, GetUnformattedObjectiveList());

        }

        public virtual string GetUnformattedObjectiveList() {
            string objectives = string.Empty;
            List<string> objectiveList = new List<string>();
            if (steps.Count > 0) {
                foreach (QuestObjective questObjective in steps[GetSaveData().questStep].QuestObjectives) {
                    objectiveList.Add(questObjective.GetUnformattedStatus());
                }
            }
            objectives = string.Join("\n", objectiveList);
            if (objectives == string.Empty) {
                objectives = DisplayName;
            }
            return objectives;
        }

        protected virtual void ProcessAcceptQuest() {
            // nothing to do here
        }

        public virtual void AcceptQuest(bool printMessages = true, bool resetStep = true) {
            QuestSaveData questSaveData = GetSaveData();
            if (resetStep == true) {
                questSaveData.questStep = 0;
            }
            questSaveData.markedComplete = false;
            questSaveData.turnedIn = false;
            SetSaveData(ResourceName, questSaveData);
            if (steps.Count > 0) {
                foreach (QuestObjective questObjective in steps[CurrentStep].QuestObjectives) {
                    questObjective.OnAcceptQuest(this, printMessages);
                }
            }

            if (printMessages == true) {
                ProcessAcceptQuest();
            }

            // this next statement seems unnecessary.  is it a holdover from when quests were cloned ?
            // disable for now and see if anything breaks
            //if (!MarkedComplete) {
                // needs to be done here if quest wasn't auto-completed in checkcompletion
                if (playerManager != null && playerManager.PlayerUnitSpawned == false) {
                    // STOP STUFF FROM REACTING WHEN PLAYER ISN'T SPAWNED
                    return;
                }
                SystemEventManager.TriggerEvent("OnQuestStatusUpdated", new EventParamProperties());
                SystemEventManager.TriggerEvent("OnAfterQuestStatusUpdated", new EventParamProperties());
                OnQuestStatusUpdated();
            //}
        }

        public virtual void CheckCompletion(bool notifyOnUpdate = true, bool printMessages = true) {
            //Debug.Log("QuestLog.CheckCompletion()");
            if (MarkedComplete) {
                // no need to waste cycles checking, we are already done
                return;
            }

            if (StepsComplete(printMessages)) {
                MarkComplete(notifyOnUpdate, printMessages);
            } else {
                // since this method only gets called as a result of a quest objective status updating, we need to notify for that at minimum
                //Debug.Log(DisplayName + ".Quest.CheckCompletion(): about to notify for objective status updated");
                SystemEventManager.TriggerEvent("OnQuestObjectiveStatusUpdated", new EventParamProperties());
                OnQuestObjectiveStatusUpdated();
            }
        }

        private bool StepsComplete(bool printMessages) {
            if (steps.Count == 0) {
                return true;
            }

            for (int i = CurrentStep; i < steps.Count; i++) {
                // advance current step to ensure quest tracker and log show proper objectives
                if (CurrentStep != i) {
                    CurrentStep = i;

                    // unsubscribe the previous step objectives
                    foreach (QuestObjective questObjective in steps[i - 1].QuestObjectives) {
                        questObjective.OnAbandonQuest();
                    }

                    // reset save data from this step in case the next step contains an objective of the same type, but different amount
                    ResetObjectiveSaveData();

                    // subscribe the current step objectives
                    foreach (QuestObjective questObjective in steps[i].QuestObjectives) {
                        questObjective.OnAcceptQuest(this, printMessages);
                    }
                }
                foreach (QuestObjective questObjective in steps[i].QuestObjectives) {
                    if (!questObjective.IsComplete) {
                        return false;
                    }
                }
            }

            return true;
        }

        // force prerequisite status update outside normal event notification
        public virtual void UpdatePrerequisites(bool notify = true) {
            foreach (PrerequisiteConditions conditions in prerequisiteConditions) {
                conditions.UpdatePrerequisites(notify);
            }
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            //Debug.Log(DisplayName + ".Quest.SetupScriptableObjects(" + (systemGameManager == null ? "null" : systemGameManager.gameObject.name) + "): ID: " + GetInstanceID());

            base.SetupScriptableObjects(systemGameManager);

            foreach (QuestStep questStep in steps) {
                questStep.SetupScriptableObjects(this, systemGameManager);
            }

            foreach (PrerequisiteConditions conditions in prerequisiteConditions) {
                conditions.SetupScriptableObjects(systemGameManager, this);
            }

        }

        public override void CleanupScriptableObjects() {
            base.CleanupScriptableObjects();
            foreach (PrerequisiteConditions conditions in prerequisiteConditions) {
                conditions.CleanupScriptableObjects(this);
            }
        }

        public virtual void HandlePrerequisiteUpdates() {
            OnQuestStatusUpdated();
        }
    }

    [System.Serializable]
    public class QuestStep : ConfiguredClass{
        [SerializeReference]
        [SerializeReferenceButton]
        private List<QuestObjective> questObjectives = new List<QuestObjective>();

        public List<QuestObjective> QuestObjectives { get => questObjectives; }

        public void SetupScriptableObjects(QuestBase quest, SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            foreach (QuestObjective objective in questObjectives) {
                objective.SetupScriptableObjects(systemGameManager, quest);
                //objective.SetQuest(quest);
            }
        }

    }
}