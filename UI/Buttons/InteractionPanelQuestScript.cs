using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

        private QuestGiverComponent questGiver;

        [SerializeField]
        private TextMeshProUGUI text = null;

        private bool markedComplete = false;

        public TextMeshProUGUI MyText {
            get {
                return text;
            }
        }

        public QuestGiverComponent MyQuestGiver { get => questGiver; set => questGiver = value; }

        public void Select() {
            Debug.Log((MyQuest == null ? "null" : MyQuest.DisplayName) + ".InteractionPanelQuestScript.Select()");
            if (MyQuest == null) {
                return;
            }

            if (MyQuest.MyHasOpeningDialog == true && MyQuest.MyOpeningDialog != null && MyQuest.MyOpeningDialog.TurnedIn == false) {
                //Debug.Log("InteractionPanelQuestScript.Select(): dialog is not completed, popping dialog");
                QuestGiverUI.MyInstance.ShowDescription(MyQuest);
            } else {
                //Debug.Log("InteractionPanelQuestScript.Select(): has no dialog, or dialog is completed, opening questgiver window");
                PopupWindowManager.MyInstance.questGiverWindow.OpenWindow();
                QuestGiverUI.MyInstance.ShowDescription(MyQuest, questGiver);
            }

        }

        public void DeSelect() {
            //Debug.Log("QuestTrackerQuestScript.DeSelect()");
        }

        public void IsComplete() {
            //Debug.Log("QuestTrackerQuestScript.IsComplete(): Checking questscript iscomplete on myquest: " + MyQuest.MyTitle);
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