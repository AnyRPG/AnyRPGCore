using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
// this is almost identical to questscript

public class InteractionPanelQuestScript : MonoBehaviour {

    private Quest quest = null;

    public Quest MyQuest {
        get => quest;
        set {
            quest = value;
            //Debug.Log("Just set quest to: " + quest);
        }
        
    }

    private QuestGiver questGiver;

    [SerializeField]
    private Text text;

    private bool markedComplete = false;

    public Text MyText {
        get {
            return text;
        }
    }

    public QuestGiver MyQuestGiver { get => questGiver; set => questGiver = value; }

    public void InitWindow(ICloseableWindowContents questGiverUI) {
        //Debug.Log(gameObject.name + ".QuestGiver.InitWindow()");
        (questGiverUI as QuestGiverUI).ShowQuests(questGiver);
        PopupWindowManager.MyInstance.questGiverWindow.MyCloseableWindowContents.OnOpenWindow -= InitWindow;
        QuestGiverUI.MyInstance.ShowDescription(MyQuest);

    }

    public void Select() {
        //Debug.Log("InteractionPanelQuestScript.Select()");
        QuestGiverUI.MyInstance.MyQuestGiver = questGiver;
        //PopupWindowManager.MyInstance.questGiverWindow.MyCloseableWindowContents.OnOpenWindowHandler -= InitWindow;
        if (MyQuest == null) {
            //Debug.Log("InteractionPanelQuestScript.Select(): MYQUEST IS NULL!");
            return;
        } else {
            //Debug.Log("InteractionPanelQuestScript.Select(): MYQUEST: " + MyQuest.MyName);
        }

        if (MyQuest.MyHasOpeningDialog == true && MyQuest.MyOpeningDialog != null && MyQuest.MyOpeningDialog.TurnedIn == false) {
            //Debug.Log("InteractionPanelQuestScript.Select(): dialog is not completed, popping dialog");
            QuestGiverUI.MyInstance.ShowDescription(MyQuest);
        } else {
            //Debug.Log("InteractionPanelQuestScript.Select(): has no dialog, or dialog is completed, opening questgiver window");
            PopupWindowManager.MyInstance.questGiverWindow.OpenWindow();
            (PopupWindowManager.MyInstance.questGiverWindow.MyCloseableWindowContents as QuestGiverUI).ShowQuests(questGiver);
            PopupWindowManager.MyInstance.questGiverWindow.MyCloseableWindowContents.OnOpenWindow -= InitWindow;
            QuestGiverUI.MyInstance.ShowDescription(MyQuest);

        }

        /*
        if (!PopupWindowManager.MyInstance.questGiverWindow.IsOpen) {
            //Debug.Log(source + " interacting with " + gameObject.name);
            PopupWindowManager.MyInstance.questGiverWindow.MyCloseableWindowContents.OnOpenWindowHandler += InitWindow;
            PopupWindowManager.MyInstance.questGiverWindow.OpenWindow();
        }
        */
    }

    public void DeSelect() {
        //Debug.Log("QuestTrackerQuestScript.DeSelect()");
    }

    public void IsComplete() {
        //Debug.Log("QuestTrackerQuestScript.IsComplete(): Checking questscript iscomplete on myquest: " + MyQuest.MyTitle);
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

}