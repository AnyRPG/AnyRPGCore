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

        public event System.Action<UnitController> OnQuestBaseStatusUpdated = delegate { };
        public event System.Action<UnitController> OnQuestBaseObjectiveStatusUpdated = delegate { };

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
        protected NetworkManagerServer networkManagerServer = null;

        public virtual bool PrintObjectiveCompletionMessages {
            get => false;
        }

        public virtual Quest QuestTemplate { get => questTemplate; set => questTemplate = value; }

        public List<QuestStep> Steps { get => steps; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            saveManager = systemGameManager.SaveManager;
            playerManager = systemGameManager.PlayerManager;
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;
            networkManagerServer = systemGameManager.NetworkManagerServer;
        }

        protected abstract QuestSaveData GetSaveData(UnitController sourceUnitController);
        protected abstract void SetSaveData(UnitController sourceUnitController, string QuestName, QuestSaveData questSaveData);
        protected abstract bool HasQuest(UnitController sourceUnitController);

        public virtual bool IsComplete(UnitController sourceUnitController) {
            if (steps.Count > 0) {
                foreach (QuestObjective questObjective in steps[steps.Count -1].QuestObjectives) {
                    if (!questObjective.IsComplete(sourceUnitController)) {
                        return false;
                    }
                }
            }

            return true;
        }

        public virtual void SetTurnedIn(UnitController sourceUnitController, bool turnedIn, bool notify = true) {
            
            QuestSaveData saveData = GetSaveData(sourceUnitController);
            saveData.TurnedIn = turnedIn;
            SetSaveData(sourceUnitController, saveData.QuestName, saveData);

            //Debug.Log(DisplayName + ".Quest.TurnedIn = " + value);
            if (notify) {
                OnQuestBaseStatusUpdated(sourceUnitController);
            }
        }

        public virtual bool PrerequisitesMet(UnitController sourceUnitController) {
            foreach (PrerequisiteConditions prerequisiteCondition in prerequisiteConditions) {
                if (!prerequisiteCondition.IsMet(sourceUnitController)) {
                    return false;
                }
            }
            // there are no prerequisites, or all prerequisites are complete
            return true;
        }


        public bool TurnedIn(UnitController sourceUnitController) {
            //Debug.Log($"{ResourceName}.QuestBase.TurnedIn({sourceUnitController.gameObject.name})");

            return GetSaveData(sourceUnitController).TurnedIn;
        }

        public int CurrentStep(UnitController sourceUnitController) {
            return GetSaveData(sourceUnitController).QuestStep;
        }

        public virtual void SetCurrentStep(UnitController sourceUnitController, int value) {
            //Debug.Log($"{ResourceName}.QuestBase.SetCurrentStep({sourceUnitController.gameObject.name}, {value})");

            QuestSaveData saveData = GetSaveData(sourceUnitController);
            saveData.QuestStep = value;
            SetSaveData(sourceUnitController, saveData.QuestName, saveData);
        }

        public bool MarkedComplete(UnitController sourceUnitController) {
            return GetSaveData(sourceUnitController).MarkedComplete;
        }

        public void SetMarkedComplete(UnitController sourceUnitController, bool value) { 
            QuestSaveData saveData = GetSaveData(sourceUnitController);
            saveData.MarkedComplete = value;
            SetSaveData(sourceUnitController, saveData.QuestName, saveData);
        }

        public virtual void RemoveQuest(UnitController sourceUnitController, bool resetQuestStep = true) {
            //Debug.Log($"{ResourceName}.QuestBase.RemoveQuest({sourceUnitController.gameObject.name}, {resetQuestStep})");

            OnAbandonQuest(sourceUnitController);

            // reset current step so the correct objective shows up in the quest giver window when the quest is picked back up
            if (resetQuestStep == true) {
                SetCurrentStep(sourceUnitController, 0);
            }
        }

        protected virtual void NotifyOnQuestBaseStatusUpdated(UnitController sourceUnitController) {
            //Debug.Log($"{ResourceName}.QuestBase.NotifyOnQuestBaseStatusUpdated({sourceUnitController.gameObject.name})");
            OnQuestBaseStatusUpdated(sourceUnitController);
        }

        protected virtual void ProcessMarkComplete(UnitController sourceUnitController, bool printMessages) {
            // nothing to do here
        }

        public virtual void MarkComplete(UnitController sourceUnitController, bool notifyOnUpdate = true, bool printMessages = true) {
            //Debug.Log($"{ResourceName}.QuestBase.MarkComplete({sourceUnitController.gameObject.name}, {notifyOnUpdate}, {printMessages})");

            if (MarkedComplete(sourceUnitController) == true) {
                return;
            }

            SetMarkedComplete(sourceUnitController, true);
            ProcessMarkComplete(sourceUnitController, printMessages);

            if (notifyOnUpdate == true) {
                NotifyOnMarkComplete(sourceUnitController);
                OnQuestBaseStatusUpdated(sourceUnitController);
            }
        }

        public virtual void OnAbandonQuest(UnitController sourceUnitController) {
            
            if (steps.Count == 0) {
                return;
            }

            foreach (QuestObjective questObjective in steps[CurrentStep(sourceUnitController)].QuestObjectives) {
                questObjective.OnAbandonQuest(sourceUnitController);
            }

        }

        public virtual string GetStatus(UnitController sourceUnitController) {
            //Debug.Log(DisplayName + ".Quest.GetStatus()");

            if (StatusCompleted(sourceUnitController)) {
                //Debug.Log(DisplayName + ".Quest.GetStatus(): returning completed");
                return "completed";
            }

            if (StatusComplete(sourceUnitController)) {
                return "complete";
            }

            if (StatusInProgress(sourceUnitController)) {
                return "inprogress";
            }

            if (StatusAvailable(sourceUnitController)) {
                //Debug.Log(DisplayName + ".Quest.GetStatus(): returning available");
                return "available";
            }

            // this quest prerequisites were not met
            //Debug.Log(DisplayName + ".Quest.GetStatus(): returning unavailable");
            return "unavailable";
        }

        protected virtual bool StatusCompleted(UnitController sourceUnitController) {
            if (TurnedIn(sourceUnitController) == true) {
                return true;
            }

            return false;
        }

        protected virtual bool StatusAvailable(UnitController sourceUnitController) {
            if (PrerequisitesMet(sourceUnitController) == false) {
                return false;
            }

            if (HasQuest(sourceUnitController) == true) {
                return false;
            }

            return true;
        }

        protected virtual bool StatusComplete(UnitController sourceUnitController) {
            if (HasQuest(sourceUnitController) && IsComplete(sourceUnitController) == true) {
                //Debug.Log(DisplayName + ".Quest.GetStatus(): returning complete");
                return true;
            }

            return false;
        }

        protected virtual bool StatusInProgress(UnitController sourceUnitController) {
            if (HasQuest(sourceUnitController) && IsComplete(sourceUnitController) == false) {
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

        public virtual string GetObjectiveDescription(UnitController sourceUnitController) {

            Color titleColor = GetTitleColor();
            return string.Format("<size=30><b><color=#{0}>{1}</color></b></size>\n\n<size=18>{2}</size>\n\n<b><size=24>Objectives:</size></b>\n\n<size=18>{3}</size>", ColorUtility.ToHtmlStringRGB(titleColor), DisplayName, Description, GetUnformattedObjectiveList(sourceUnitController));

        }

        public virtual string GetUnformattedObjectiveList(UnitController sourceUnitController) {
            string objectives = string.Empty;
            List<string> objectiveList = new List<string>();
            if (steps.Count > 0) {
                foreach (QuestObjective questObjective in steps[GetSaveData(sourceUnitController).QuestStep].QuestObjectives) {
                    objectiveList.Add(questObjective.GetUnformattedStatus(sourceUnitController));
                }
            }
            objectives = string.Join("\n", objectiveList);
            if (objectives == string.Empty) {
                objectives = DisplayName;
            }
            return objectives;
        }

        protected virtual void ProcessAcceptQuest(UnitController sourceUnitController) {
            // nothing to do here
        }

        public virtual void AcceptQuest(UnitController sourceUnitController, bool printMessages = true, bool resetStep = true) {
            //Debug.Log($"{ResourceName}.QuestBase.AcceptQuest({sourceUnitController.gameObject.name}, {printMessages}, {resetStep})");

            QuestSaveData questSaveData = GetSaveData(sourceUnitController);
            if (resetStep == true) {
                questSaveData.QuestStep = 0;
            }
            questSaveData.MarkedComplete = false;
            questSaveData.TurnedIn = false;
            SetSaveData(sourceUnitController, ResourceName, questSaveData);

            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true) {
                if (steps.Count > 0) {
                    foreach (QuestObjective questObjective in steps[CurrentStep(sourceUnitController)].QuestObjectives) {
                        questObjective.OnAcceptQuest(sourceUnitController, this, printMessages);
                    }
                }
            }

            if (printMessages == true) {
                ProcessAcceptQuest(sourceUnitController);
            }

            NotifyOnAcceptQuest(sourceUnitController);
            OnQuestBaseStatusUpdated(sourceUnitController);
        }

        public virtual void CheckCompletion(UnitController sourceUnitController, bool notifyOnUpdate = true, bool printMessages = true) {
            //Debug.Log($"{ResourceName}.QuestBase.CheckCompletion({sourceUnitController.gameObject.name}, {notifyOnUpdate}, {printMessages})");

            if (MarkedComplete(sourceUnitController)) {
                // no need to waste cycles checking, we are already done
                return;
            }

            if (StepsComplete(sourceUnitController, printMessages)) {
                MarkComplete(sourceUnitController, notifyOnUpdate, printMessages);
            } else {
                // TESTING - moved these notifications to after StepsComplete() so things that get notified have the correct step
                // since this method only gets called as a result of a quest objective status updating, we need to notify for that
                // anything subscribing to this will also subscribe to the complete above so it's redundant to call both
                OnQuestBaseObjectiveStatusUpdated(sourceUnitController);
                NotifyOnObjectiveStatusUpdated(sourceUnitController);
            }
        }

        public bool StepsComplete(UnitController sourceUnitController, bool printMessages) {
            //Debug.Log($"{ResourceName}.QuestBase.StepsComplete({sourceUnitController.gameObject.name}, {printMessages})");

            if (steps.Count == 0) {
                return true;
            }

            for (int i = CurrentStep(sourceUnitController); i < steps.Count; i++) {
                // advance current step to ensure quest tracker and log show proper objectives
                if (CurrentStep(sourceUnitController) != i) {
                    SetCurrentStep(sourceUnitController, i);

                    // unsubscribe the previous step objectives
                    foreach (QuestObjective questObjective in steps[i - 1].QuestObjectives) {
                        questObjective.OnAbandonQuest(sourceUnitController);
                    }

                    // reset save data from this step in case the next step contains an objective of the same type, but different amount
                    ResetObjectiveSaveData(sourceUnitController);

                    // subscribe the current step objectives
                    foreach (QuestObjective questObjective in steps[i].QuestObjectives) {
                        questObjective.OnAcceptQuest(sourceUnitController, this, printMessages);
                    }
                }
                foreach (QuestObjective questObjective in steps[i].QuestObjectives) {
                    if (!questObjective.IsComplete(sourceUnitController)) {
                        return false;
                    }
                }
            }

            return true;
        }

        // force prerequisite status update outside normal event notification
        public virtual void UpdatePrerequisites(UnitController sourceUnitController, bool notify = true) {
            foreach (PrerequisiteConditions conditions in prerequisiteConditions) {
                conditions.UpdatePrerequisites(sourceUnitController, notify);
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

        public virtual void HandlePrerequisiteUpdates(UnitController sourceUnitController) {
            OnQuestBaseStatusUpdated(sourceUnitController);
        }

        public abstract int GetObjectiveCurrentAmount(UnitController sourceUnitController, string name, string objectiveName);
        public abstract void SetObjectiveCurrentAmount(UnitController sourceUnitController, string name, string objectiveName, int value);
        public abstract void ResetObjectiveSaveData(UnitController sourceUnitController);
        public abstract void NotifyOnObjectiveStatusUpdated(UnitController sourceUnitController);
        public abstract void NotifyOnMarkComplete(UnitController sourceUnitController);
        public abstract void NotifyOnAcceptQuest(UnitController sourceUnitController);
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