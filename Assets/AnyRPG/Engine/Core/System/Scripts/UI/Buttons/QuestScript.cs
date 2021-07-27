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

        //private string questName;

        private Quest quest = null;

        private bool markedComplete = false;

        public Quest MyQuest { get => quest; }

        public void SetQuest(Quest newQuest) {
            if (newQuest != null) {
                quest = newQuest;
                MyText.text = quest.DisplayName;
                IsComplete();
            }
        }

        public override void Select() {
            //Debug.Log(gameObject.name + ".QuestScript.Select()");

            RawSelect();

            QuestLogUI.Instance.MySelectedQuestScript = this;

            QuestLogUI.Instance.ShowDescription(MyQuest);
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
                MyText.text = "[" + quest.MyExperienceLevel + "] " + quest.DisplayName + " (Complete)";
            } else if (!quest.IsComplete) {
                markedComplete = false;
                MyText.text = "[" + quest.MyExperienceLevel + "] " + quest.DisplayName;
            }
            MyText.color = LevelEquations.GetTargetColor(PlayerManager.Instance.MyCharacter.CharacterStats.Level, quest.MyExperienceLevel);
        }

        /*
        public void CommonSelect() {
            if (QuestLogUI.Instance.MySelectedQuestScript != null && QuestLogUI.Instance.MySelectedQuestScript != this) {
                QuestLogUI.Instance.MySelectedQuestScript.DeSelect();
            }
            QuestLogUI.Instance.MySelectedQuestScript = this;

        }
        */
        /*
        public void RawSelect() {
            CommonSelect();
        }
        */

    }

}