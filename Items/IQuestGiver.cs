using AnyRPG;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

namespace AnyRPG {
    public interface IQuestGiver {
        List<QuestNode> MyQuests { get; }
        void UpdateQuestStatus();
        Interactable MyInteractable { get; }
        bool Interact(CharacterUnit source);
        void HandleCompleteQuest();
        void HandleAcceptQuest();
        bool EndsQuest(string questName);
    }
}