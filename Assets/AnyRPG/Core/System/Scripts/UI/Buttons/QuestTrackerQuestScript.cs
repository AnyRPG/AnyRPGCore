using TMPro;
using UnityEngine;

namespace AnyRPG {
    // this is almost identical to questscript

    public class QuestTrackerQuestScript : NavigableElement {
        
        [Header("Quest Tracker")]

        [SerializeField]
        protected TextMeshProUGUI text = null;

        // game manager references
        protected UIManager uIManager = null;
        protected WindowManager windowManager = null;
        protected PlayerManagerClient playerManagerClient = null;

        public Quest Quest { get; set; }
        public TextMeshProUGUI Text {
            get {
                return text;
            }
        }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            windowManager = systemGameManager.WindowManager;
            playerManagerClient = systemGameManager.PlayerManagerClient;
        }

        public override void Interact() {
            //Debug.Log("QuestTrackerQuestScript.Select()");
            windowManager.EndNavigateInterface();
            uIManager.questLogWindow.OpenWindow();
            playerManagerClient.UnitController.CharacterQuestLog.ShowQuestLogDescription(Quest);
        }


        /*
        public void IsComplete() {
            //Debug.Log("QuestTrackerQuestScript.IsComplete(): Checking questscript iscomplete on myquest: " + MyQuest.MyTitle);
            if (MyQuest.IsComplete && !markedComplete) {
                markedComplete = true;
                //Debug.Log("the quest is complete");
                MyText.text = "[" + MyQuest.MyExperienceLevel + "] " + MyQuest.MyTitle + " (Complete)";
            } else if (!MyQuest.IsComplete) {
                markedComplete = false;
                MyText.text = "[" + MyQuest.MyExperienceLevel + "] " + MyQuest.MyTitle;
            }
            MyText.color = LevelEquations.GetTargetColor(SystemGameManager.Instance.PlayerManager.MyCharacter.MyCharacterStats.MyLevel, MyQuest.MyExperienceLevel);
        }
        */

    }

}