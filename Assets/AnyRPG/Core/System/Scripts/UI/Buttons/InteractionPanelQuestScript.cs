using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    // this is almost identical to questscript

    public class InteractionPanelQuestScript : ConfiguredMonoBehaviour {

        [SerializeField]
        private TextMeshProUGUI text = null;

        private Quest quest = null;

        private QuestGiverComponent questGiver;

        private bool markedComplete = false;

        // game manager references
        private UIManager uIManager = null;
        private PlayerManager playerManager = null;
        private QuestLog questLog = null;

        public TextMeshProUGUI Text { get => text; }
        public Quest Quest { get => quest; set => quest = value; }
        public QuestGiverComponent QuestGiver { get => questGiver; set => questGiver = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            uIManager = systemGameManager.UIManager;
            playerManager = systemGameManager.PlayerManager;
            questLog = systemGameManager.QuestLog;
        }

        public void Select() {
            //Debug.Log((MyQuest == null ? "null" : MyQuest.DisplayName) + ".InteractionPanelQuestScript.Select()");
            if (quest == null) {
                return;
            }

            if (quest.HasOpeningDialog == true && quest.OpeningDialog != null && quest.OpeningDialog.TurnedIn == false) {
                //Debug.Log("InteractionPanelQuestScript.Select(): dialog is not completed, popping dialog with questGiver: " + (questGiver == null ? "null" : questGiver.Interactable.DisplayName));
                questLog.ShowQuestGiverDescription(quest, questGiver);
            } else {
                //Debug.Log("InteractionPanelQuestScript.Select(): has no dialog, or dialog is completed, opening questgiver window");
                uIManager.questGiverWindow.OpenWindow();
                questLog.ShowQuestGiverDescription(quest, questGiver);
            }
        }

        public void DeSelect() {
            //Debug.Log("QuestTrackerQuestScript.DeSelect()");
        }

        public void IsComplete() {
            //Debug.Log("QuestTrackerQuestScript.IsComplete(): Checking questscript iscomplete on myquest: " + MyQuest.MyTitle);
            if (quest.IsComplete && !markedComplete) {
                markedComplete = true;
                //Debug.Log("the quest is complete");
                Text.text = "[" + quest.ExperienceLevel + "] " + quest.DisplayName + " (Complete)";
            } else if (!quest.IsComplete) {
                markedComplete = false;
                Text.text = "[" + quest.ExperienceLevel + "] " + quest.DisplayName;
            }
            Text.color = LevelEquations.GetTargetColor(playerManager.MyCharacter.CharacterStats.Level, quest.ExperienceLevel);
        }


    }

}