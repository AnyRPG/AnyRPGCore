using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class UseInteractableObjective : QuestObjective {

        public override Type ObjectiveType {
            get {
                return typeof(UseInteractableObjective);
            }
        }

        // if just interacting is not enough, but actually finishing using an interactable is required.
        public bool requireCompletion = false;

        public void CheckInteractionStart(string interactableName) {
            //Debug.Log("UseInteractableObjective.CheckInteractableName()");
            CheckInteractableName(interactableName, false);
        }

        public void CheckInteractionComplete(Interactable interactable) {
            //Debug.Log("UseInteractableObjective.CheckInteractableName()");
            CheckInteractableName(interactable.DisplayName, true);
        }

        public void CheckInteractionComplete(InteractableOptionComponent interactableOption) {
            CheckInteractableName(interactableOption.DisplayName, true);
            CheckInteractableName(interactableOption.Interactable.DisplayName, true);
        }

        public void CheckInteractionStart(InteractableOptionComponent interactableOption) {
            CheckInteractableName(interactableOption.DisplayName, false);
            CheckInteractableName(interactableOption.Interactable.DisplayName, false);
        }

        public void CheckInteractableName(string interactableName, bool interactionComplete) {
            //Debug.Log("UseInteractableObjective.CheckInteractableName()");
            bool completeBefore = IsComplete;
            if (completeBefore) {
                return;
            }
            if (SystemDataFactory.MatchResource(interactableName, MyType)) {
                if (!interactionComplete && requireCompletion == true) {
                    return;
                }
                if (requireCompletion == false && interactionComplete) {
                    return;
                }
                if (CurrentAmount < MyAmount) {
                    CurrentAmount++;
                    quest.CheckCompletion();
                }
                if (CurrentAmount <= MyAmount && !quest.IsAchievement && CurrentAmount != 0) {
                    messageFeedManager.WriteMessage(string.Format("{0}: {1}/{2}", DisplayName, Mathf.Clamp(CurrentAmount, 0, MyAmount), MyAmount));
                }
                if (completeBefore == false && IsComplete && !quest.IsAchievement) {
                    messageFeedManager.WriteMessage(string.Format("{0}: Objective Complete", DisplayName));
                }
            }
        }


        public override void OnAcceptQuest(Quest quest, bool printMessages = true) {
            //Debug.Log("UseInteractableObjective.OnAcceptQuest()");
            base.OnAcceptQuest(quest, printMessages);

            // don't forget to remove these later
            systemEventManager.OnInteractionStarted += CheckInteractionStart;
            systemEventManager.OnInteractionWithOptionStarted += CheckInteractionStart;
            systemEventManager.OnInteractionCompleted += CheckInteractionComplete;
            systemEventManager.OnInteractionWithOptionCompleted += CheckInteractionComplete;
        }

        public override void OnAbandonQuest() {
            //Debug.Log("UseInteractableObjective.OnAbandonQuest()");
            base.OnAbandonQuest();
            systemEventManager.OnInteractionStarted -= CheckInteractionStart;
            systemEventManager.OnInteractionWithOptionStarted -= CheckInteractionStart;
            systemEventManager.OnInteractionCompleted -= CheckInteractionComplete;
            systemEventManager.OnInteractionWithOptionCompleted -= CheckInteractionComplete;
        }

    }
}