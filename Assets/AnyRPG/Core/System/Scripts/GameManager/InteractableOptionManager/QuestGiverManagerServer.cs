using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class QuestGiverManagerServer : InteractableOptionManager {

        public void AcceptQuest(Interactable interactable, int componentIndex, UnitController sourceUnitController, Quest quest) {
            //Debug.Log($"QuestGiverManager.AcceptQuestInternal({sourceUnitController.gameObject.name}, {quest.ResourceName})");

            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables[componentIndex] is QuestGiverComponent) {
                (currentInteractables[componentIndex] as QuestGiverComponent).AcceptQuest(sourceUnitController, quest);
            }
        }

        public void CompleteQuest(Interactable interactable, int componentIndex, UnitController sourceUnitController, Quest quest, QuestRewardChoices questRewardChoices) {

            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables[componentIndex] is QuestGiverComponent) {
                (currentInteractables[componentIndex] as QuestGiverComponent).CompleteQuest(sourceUnitController, quest, questRewardChoices);
            }

        }


    }

}