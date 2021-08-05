using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class VisitZoneObjective : QuestObjective {

        public override Type ObjectiveType {
            get {
                return typeof(VisitZoneObjective);
            }
        }

        private SceneNode objectiveSceneNode = null;

        // NEW (HOPEFULLY) SAFE COMPLETION CHECK CODE THAT SHOULDN'T RESULT IN RUNAWAY STACK OVERFLOW ETC
        public void CheckCompletionCount() {
            bool completeBefore = IsComplete;
            if (completeBefore) {
                return;
            }

            CurrentAmount++;
            quest.CheckCompletion();
            if (CurrentAmount <= MyAmount && !quest.IsAchievement && CurrentAmount != 0) {
                SystemGameManager.Instance.UIManager.MessageFeedManager.WriteMessage(string.Format("{0}: {1}/{2}", DisplayName, Mathf.Clamp(CurrentAmount, 0, MyAmount), MyAmount));
            }
            if (completeBefore == false && IsComplete && !quest.IsAchievement) {
                SystemGameManager.Instance.UIManager.MessageFeedManager.WriteMessage(string.Format("{0}: Objective Complete", DisplayName));
            }
        }

        public override void UpdateCompletionCount(bool printMessages = true) {
            base.UpdateCompletionCount(printMessages);
            SceneNode sceneNode = SystemDataFactory.Instance.GetResource<SceneNode>(MyType);
            if (sceneNode != null && sceneNode.Visited == true) {
                CurrentAmount++;
            }
        }

        public override void OnAcceptQuest(Quest quest, bool printMessages = true) {
            //Debug.Log("UseInteractableObjective.OnAcceptQuest()");
            base.OnAcceptQuest(quest, printMessages);

            objectiveSceneNode = null;
            if (MyType != null && MyType != string.Empty) {
                objectiveSceneNode = SystemDataFactory.Instance.GetResource<SceneNode>(MyType);
            } else {
                Debug.LogError("VisitZoneObjective.OnAcceptQuest(): Could not find scene node : " + MyType + " while inititalizing a visit zone objective.  CHECK INSPECTOR");
                return;
            }
            UpdateCompletionCount(printMessages);
            objectiveSceneNode.OnVisitZone += CheckCompletionCount;

            // don't forget to remove these later
            //SystemGameManager.Instance.EventManager.OnDialogCompleted += CheckCompletionCount;

        }

        public override void OnAbandonQuest() {
            //Debug.Log("UseInteractableObjective.OnAbandonQuest()");
            base.OnAbandonQuest();
            objectiveSceneNode.OnVisitZone -= CheckCompletionCount;
            //SystemGameManager.Instance.EventManager.OnDialogCompleted -= CheckCompletionCount;
        }

    }
}