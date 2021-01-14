using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
// this is almost identical to questscript

public class QuestGiverQuestScript : MonoBehaviour
{
    public Quest MyQuest { get; set; }

    [SerializeField]
    private TextMeshProUGUI text = null;

    private bool markedComplete = false;

    [SerializeField]
    private Color highlightColor = new Color32(255, 255, 255, 80);

    [SerializeField]
    private Color baseColor = new Color32(0, 0, 0, 0);

    public TextMeshProUGUI MyText {
        get {
            return text;
        }
    }

    public void CommonSelect() {
        if (QuestGiverUI.MyInstance.MySelectedQuestGiverQuestScript != null && QuestGiverUI.MyInstance.MySelectedQuestGiverQuestScript != this) {
            //Debug.Log("QuestGiverQuestScript.Select(): " + MyQuest.MyTitle + ": deselecting old script: " + QuestGiverUI.MyInstance.MySelectedQuestGiverQuestScript.MyQuest.MyTitle);
            QuestGiverUI.MyInstance.MySelectedQuestGiverQuestScript.DeSelect();
        }
        QuestGiverUI.MyInstance.MySelectedQuestGiverQuestScript = this;

        //Debug.Log("QuestGiverQuestScript.Select(): " + MyQuest.MyTitle + ": setting color: " + highlightColor);
        GetComponent<Image>().color = highlightColor;
    }


    public void RawSelect() {
        CommonSelect();
    }

    public void Select() {
        //Debug.Log("QuestGiverQuestScript.Select()");

        CommonSelect();
        QuestGiverUI.MyInstance.ShowDescription(MyQuest);
    }

    public void DeSelect() {
        //Debug.Log("QuestGiverQuestScript.DeSelect(): " + MyQuest.MyTitle + ": setting color: " + baseColor);
        GetComponent<Image>().color = baseColor;
        QuestGiverUI.MyInstance.MySelectedQuestGiverQuestScript = null;
    }

    public void IsComplete() {
        //Debug.Log("Checking questscript iscomplete on myquest: " + MyQuest.MyTitle);
        if (MyQuest.IsComplete && !markedComplete) {
            markedComplete = true;
            //Debug.Log("the quest is complete");
            MyText.text = "[" + MyQuest.MyExperienceLevel + "] " + MyQuest.DisplayName + " (Complete)";
        } else if (!MyQuest.IsComplete) {
            markedComplete = false;
            MyText.text = "[" + MyQuest.MyExperienceLevel + "] " + MyQuest.DisplayName;
        }
        MyText.color = LevelEquations.GetTargetColor(PlayerManager.MyInstance.MyCharacter.CharacterStats.Level, MyQuest.MyExperienceLevel);
    }


}

}