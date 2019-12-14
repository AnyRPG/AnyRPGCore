using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
// this is almost identical to questscript

public class QuestTrackerQuestScript : MonoBehaviour
{
    public Quest MyQuest { get; set; }

    [SerializeField]
    private Text text;

    private bool markedComplete = false;

    public Text MyText {
        get {
            return text;
        }
    }

    public void Select() {
        //Debug.Log("QuestTrackerQuestScript.Select()");
        PopupWindowManager.MyInstance.questLogWindow.OpenWindow();
        QuestLogUI.MyInstance.ShowDescription(MyQuest);
    }

    public void DeSelect() {
        //Debug.Log("QuestTrackerQuestScript.DeSelect()");
    }

    /*
    public void IsComplete() {
        //Debug.Log("QuestTrackerQuestScript.IsComplete(): Checking questscript iscomplete on myquest: " + MyQuest.MyTitle);
        if (MyQuest.IsComplete && !markedComplete) {
            markedComplete = true;
            //Debug.Log("the quest is complete");
            MyText.text = "[" + MyQuest.MyExperienceLevel + "] " + MyQuest.MyTitle + " (Complete)";
        } else if (!MyQuest.IsComplete) {
            markedComplete = false;
            MyText.text = "[" + MyQuest.MyExperienceLevel + "] " + MyQuest.MyTitle;
        }
        MyText.color = LevelEquations.GetTargetColor(PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel, MyQuest.MyExperienceLevel);
    }
    */

}

}