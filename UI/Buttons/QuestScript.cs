using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sits on a quest item in the quest log
/// </summary>
public class QuestScript : HighlightButton {

    private Quest quest;

    private bool markedComplete = false;


    public Quest MyQuest { get => quest; }

    public void CommonSelect() {
        if (QuestLogUI.MyInstance.MySelectedQuestScript != null && QuestLogUI.MyInstance.MySelectedQuestScript != this) {
            QuestLogUI.MyInstance.MySelectedQuestScript.DeSelect();
        }
        QuestLogUI.MyInstance.MySelectedQuestScript = this;

    }

    public void RawSelect() {
        CommonSelect();
    }

    public override void Select() {
        base.Select();
        CommonSelect();
        QuestLogUI.MyInstance.ShowDescription(MyQuest);
    }

    public void SetQuest(Quest quest) {
        this.quest = quest;
        MyText.text = quest.MyName;
        IsComplete();
    }

    public void IsComplete() {
        //Debug.Log("Checking questscript iscomplete on myquest: " + MyQuest.MyTitle);
        if (MyQuest.IsComplete && !markedComplete) {
            markedComplete = true;
            //Debug.Log("the quest is complete");
            MyText.text = "[" + MyQuest.MyExperienceLevel + "] " + MyQuest.MyName + " (Complete)";
        } else if (!MyQuest.IsComplete) {
            markedComplete = false;
            MyText.text = "[" + MyQuest.MyExperienceLevel + "] " + MyQuest.MyName;
        }
        MyText.color = LevelEquations.GetTargetColor(PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel, MyQuest.MyExperienceLevel);
    }
}
