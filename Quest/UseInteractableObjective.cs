using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


[System.Serializable]
public class UseInteractableObjective : QuestObjective {

    // if just interacting is not enough, but actually finishing using an interactable is required.
    public bool requireCompletion = false;

    public void CheckInteractionStart(string interactableName) {
        //Debug.Log("UseInteractableObjective.CheckInteractableName()");
        CheckInteractableName(interactableName, false);
    }

    public void CheckInteractionComplete(Interactable interactable) {
        //Debug.Log("UseInteractableObjective.CheckInteractableName()");
        CheckInteractableName(interactable.MyName, true);
    }

    public void CheckInteractionComplete(InteractableOption interactable) {
        CheckInteractableName(interactable.MyName, true);
    }

    public void CheckInteractionStart(InteractableOption interactable) {
        CheckInteractableName(interactable.MyName, false);
    }

    public void CheckInteractableName(string interactableName, bool interactionComplete) {
        //Debug.Log("UseInteractableObjective.CheckInteractableName()");
        bool completeBefore = IsComplete;
        if (completeBefore) {
            return;
        }
        if (SystemResourceManager.MatchResource(interactableName, MyType)) {
            if (!interactionComplete && requireCompletion == true) {
                return;
            }
            if (requireCompletion == false && interactionComplete) {
                return;
            }
            if (MyCurrentAmount < MyAmount) {
                MyCurrentAmount++;
                quest.CheckCompletion();
            }
            if (MyCurrentAmount <= MyAmount && !quest.MyIsAchievement && MyCurrentAmount != 0) {
                MessageFeedManager.MyInstance.WriteMessage(string.Format("{0}: {1}/{2}", MyType, Mathf.Clamp(MyCurrentAmount, 0, MyAmount), MyAmount));
            }
            if (completeBefore == false && IsComplete && !quest.MyIsAchievement) {
                MessageFeedManager.MyInstance.WriteMessage(string.Format("{0}: Objective Complete", MyType));
            }
        }
    }


    public override void OnAcceptQuest(Quest quest, bool printMessages = true) {
        //Debug.Log("UseInteractableObjective.OnAcceptQuest()");
        base.OnAcceptQuest(quest, printMessages);

        // don't forget to remove these later
        SystemEventManager.MyInstance.OnInteractionStarted += CheckInteractionStart;
        SystemEventManager.MyInstance.OnInteractionWithOptionStarted += CheckInteractionStart;
        SystemEventManager.MyInstance.OnInteractionCompleted += CheckInteractionComplete;
        SystemEventManager.MyInstance.OnInteractionWithOptionCompleted += CheckInteractionComplete;
    }

    public override void OnAbandonQuest() {
        //Debug.Log("UseInteractableObjective.OnAbandonQuest()");
        base.OnAbandonQuest();
        SystemEventManager.MyInstance.OnInteractionStarted -= CheckInteractionStart;
        SystemEventManager.MyInstance.OnInteractionWithOptionStarted -= CheckInteractionStart;
        SystemEventManager.MyInstance.OnInteractionCompleted -= CheckInteractionComplete;
        SystemEventManager.MyInstance.OnInteractionWithOptionCompleted -= CheckInteractionComplete;
    }

}