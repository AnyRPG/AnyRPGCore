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

        protected Quest quest = null;

        //protected bool markedComplete = false;

        protected QuestLogPanel questLogUI = null;

        // game manager references
        protected PlayerManagerClient playerManagerClient = null;

        public Quest Quest { get => quest; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            playerManagerClient = systemGameManager.PlayerManagerClient;
        }

        public void SetQuest(QuestLogPanel questLogUI, Quest newQuest) {
            this.questLogUI = questLogUI;
            if (newQuest != null) {
                quest = newQuest;
                Text.text = "[" + quest.ExperienceLevel(playerManagerClient.UnitController) + "] " + quest.DisplayName;
                IsComplete();
            }
        }

        public override void Select() {
            //Debug.Log($"{gameObject.name}.QuestScript.Select()");

            RawSelect();

            questLogUI.SelectedQuestScript = this;

            questLogUI.ShowDescription(Quest);
        }

        public void RawSelect() {
            // questtracker can show description directly so we need a way to just highlight the script
            base.Select();
        }

        public void IsComplete() {
            //Debug.Log("Checking questscript iscomplete on myquest: " + MyQuest.MyTitle);

            //if (quest.IsComplete && !markedComplete) {
            if (quest.IsComplete(playerManagerClient.UnitController)) {
                //markedComplete = true;
                //Debug.Log("the quest is complete");
                //Text.text = "[" + quest.ExperienceLevel + "] " + quest.DisplayName + " (Complete)";
                Text.text += " (Complete)";
            } else if (!quest.IsComplete(playerManagerClient.UnitController)) {
                //markedComplete = false;
                //Text.text = "[" + quest.ExperienceLevel + "] " + quest.DisplayName;
            }
            Text.color = LevelEquations.GetTargetColor(playerManagerClient.UnitController.CharacterStats.Level, quest.ExperienceLevel(playerManagerClient.UnitController));
        }


    }

}