using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    /// <summary>
    /// Sits on a quest item in the quest log
    /// </summary>
    public class QuestScript : HighlightButton {

        private string questName;

        private Quest quest = null;

        private bool markedComplete = false;

        public Quest MyQuest { get => quest; }

        public void SetQuestName(string questName) {
            if (questName != null && questName != string.Empty) {
                this.questName = questName;
                MyText.text = questName;
                IsComplete();
            }
        }

        public override void Select() {
            //Debug.Log(gameObject.name + ".QuestScript.Select()");

            RawSelect();

            QuestLogUI.MyInstance.MySelectedQuestScript = this;

            QuestLogUI.MyInstance.ShowDescription(MyQuest);
        }

        public void RawSelect() {
            // questtracker can show description directly so we need a way to just highlight the script
            base.Select();
        }

        public void IsComplete() {
            //Debug.Log("Checking questscript iscomplete on myquest: " + MyQuest.MyTitle);

            if (quest.IsComplete && !markedComplete) {
                markedComplete = true;
                //Debug.Log("the quest is complete");
                MyText.text = "[" + quest.MyExperienceLevel + "] " + quest.MyName + " (Complete)";
            } else if (!quest.IsComplete) {
                markedComplete = false;
                MyText.text = "[" + quest.MyExperienceLevel + "] " + quest.MyName;
            }
            MyText.color = LevelEquations.GetTargetColor(PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel, quest.MyExperienceLevel);
        }

        /*
        public void CommonSelect() {
            if (QuestLogUI.MyInstance.MySelectedQuestScript != null && QuestLogUI.MyInstance.MySelectedQuestScript != this) {
                QuestLogUI.MyInstance.MySelectedQuestScript.DeSelect();
            }
            QuestLogUI.MyInstance.MySelectedQuestScript = this;

        }
        */
        /*
        public void RawSelect() {
            CommonSelect();
        }
        */

    }

}