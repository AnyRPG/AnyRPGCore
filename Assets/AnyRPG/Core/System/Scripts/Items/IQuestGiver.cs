using AnyRPG;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

namespace AnyRPG {
    public interface IQuestGiver {
        QuestGiverProps QuestGiverProps { get; }
        void UpdateQuestStatus(UnitController sourceUnitController);
        Interactable Interactable { get; }
        InteractableOptionComponent InteractableOptionComponent { get; }
        bool Interact(UnitController source, int componentIndex = 0, int choiceIndex = 0);
        void HandleCompleteQuest();
        void HandleAcceptQuest();
        bool EndsQuest(string questName);
        void RequestAcceptQuest(UnitController unitController, Quest currentQuest);
        void RequestCompleteQuest(UnitController unitController, Quest currentQuest, QuestRewardChoices questRewardChoices);
    }
}