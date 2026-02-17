using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class InteractionPanelQuestScript : HighlightButton {

        protected Quest quest = null;

        protected QuestGiverComponent questGiverComponent;

        protected int optionIndex = -1;

        protected bool markedComplete = false;

        // game manager references
        protected PlayerManagerClient playerManager = null;
        protected QuestGiverManagerClient questGiverManagerClient = null;

        public Quest Quest { get => quest;}
        public QuestGiverComponent QuestGiverComponent { get => questGiverComponent;}

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
            questGiverManagerClient = systemGameManager.QuestGiverManagerClient;
        }

        public void SetQuestGiverComponent(SystemGameManager systemGameManager, QuestGiverComponent questGiverComponent, int optionIndex, Quest quest) {
            Configure(systemGameManager);
            this.questGiverComponent = questGiverComponent;
            this.optionIndex = optionIndex;
            this.quest = quest;
        }

        public override void ButtonClickAction() {
            //Debug.Log("InteractionPanelQuestScript.ButtonClickAction()");

            base.ButtonClickAction();

            if (quest == null) {
                return;
            }

            questGiverManagerClient.SetQuestGiver(questGiverComponent, optionIndex, 0, false);
            if (quest.HasOpeningDialog == true && quest.OpeningDialog != null && quest.OpeningDialog.TurnedIn(playerManager.UnitController) == false) {
                //Debug.Log("InteractionPanelQuestScript.Select(): dialog is not completed, popping dialog with questGiver: " + (questGiver == null ? "null" : questGiver.Interactable.DisplayName));
                playerManager.UnitController.CharacterQuestLog.ShowQuestGiverDescription(quest, questGiverComponent);
            } else {
                //Debug.Log("InteractionPanelQuestScript.Select(): has no dialog, or dialog is completed, opening questgiver window");
                playerManager.UnitController.CharacterQuestLog.ShowQuestGiverDescription(quest, questGiverComponent);
            }
        }

    }

}