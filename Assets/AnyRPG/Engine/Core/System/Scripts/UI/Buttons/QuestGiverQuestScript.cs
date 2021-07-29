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
        if (QuestGiverUI.Instance.SelectedQuestGiverQuestScript != null && QuestGiverUI.Instance.SelectedQuestGiverQuestScript != this) {
            //Debug.Log("QuestGiverQuestScript.Select(): " + MyQuest.MyTitle + ": deselecting old script: " + QuestGiverUI.Instance.MySelectedQuestGiverQuestScript.MyQuest.MyTitle);
            QuestGiverUI.Instance.SelectedQuestGiverQuestScript.DeSelect();
        }
        QuestGiverUI.Instance.SelectedQuestGiverQuestScript = this;

        //Debug.Log("QuestGiverQuestScript.Select(): " + MyQuest.MyTitle + ": setting color: " + highlightColor);
        GetComponent<Image>().color = highlightColor;
    }


    public void RawSelect() {
        CommonSelect();
    }

    public void Select() {
        //Debug.Log("QuestGiverQuestScript.Select()");

        CommonSelect();
        QuestGiverUI.Instance.ShowDescription(MyQuest);
    }

    public void DeSelect() {
        //Debug.Log("QuestGiverQuestScript.DeSelect(): " + MyQuest.MyTitle + ": setting color: " + baseColor);
        GetComponent<Image>().color = baseColor;
        QuestGiverUI.Instance.SelectedQuestGiverQuestScript = null;
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
        MyText.color = LevelEquations.GetTargetColor(SystemGameManager.Instance.PlayerManager.MyCharacter.CharacterStats.Level, MyQuest.MyExperienceLevel);
    }


}

}