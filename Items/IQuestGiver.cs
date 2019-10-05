using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

public interface IQuestGiver {
    QuestNode[] MyQuests { get; }
    void UpdateQuestStatus();
    Interactable MyInteractable { get; }
    bool Interact(CharacterUnit source);
    void HandleCompleteQuest();
    void HandleAcceptQuest();
    bool EndsQuest(string questName);
}