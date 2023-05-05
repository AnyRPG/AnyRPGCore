using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class InteractionPanelQuestScript : HighlightButton {

        protected Quest quest = null;

        protected QuestGiverComponent questGiver;

        protected bool markedComplete = false;

        // game manager references
        protected PlayerManager playerManager = null;
        protected QuestLog questLog = null;

        public Quest Quest { get => quest; set => quest = value; }
        public QuestGiverComponent QuestGiver { get => questGiver; set => questGiver = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
            questLog = systemGameManager.QuestLog;
        }

        public override void ButtonClickAction() {
            //Debug.Log("InteractionPanelQuestScript.ButtonClickAction()");
            base.ButtonClickAction();

            if (quest == null) {
                return;
            }

            if (quest.HasOpeningDialog == true && quest.OpeningDialog != null && quest.OpeningDialog.TurnedIn == false) {
                //Debug.Log("InteractionPanelQuestScript.Select(): dialog is not completed, popping dialog with questGiver: " + (questGiver == null ? "null" : questGiver.Interactable.DisplayName));
                questLog.ShowQuestGiverDescription(quest, questGiver);
            } else {
                //Debug.Log("InteractionPanelQuestScript.Select(): has no dialog, or dialog is completed, opening questgiver window");
                questLog.ShowQuestGiverDescription(quest, questGiver);
            }
        }

    }

}