using AnyRPG;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

namespace AnyRPG {
    public interface IQuestGiver {
        QuestGiverProps Props { get; }
        void UpdateQuestStatus();
        Interactable Interactable { get; }
        InteractableOptionComponent InteractableOptionComponent { get; }
        bool Interact(CharacterUnit source, int optionIndex = 0);
        void HandleCompleteQuest();
        void HandleAcceptQuest();
        bool EndsQuest(string questName);
    }
}