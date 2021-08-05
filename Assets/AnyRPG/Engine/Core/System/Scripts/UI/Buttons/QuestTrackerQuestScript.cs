using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    // this is almost identical to questscript

    public class QuestTrackerQuestScript : ConfiguredMonoBehaviour {
        public Quest MyQuest { get; set; }

        [SerializeField]
        private TextMeshProUGUI text = null;

        //private bool markedComplete = false;

        // game manager references
        private UIManager uIManager = null;

        public TextMeshProUGUI MyText {
            get {
                return text;
            }
        }

        public override void Init(SystemGameManager systemGameManager) {
            base.Init(systemGameManager);

            uIManager = systemGameManager.UIManager;
        }

        public void Select() {
            //Debug.Log("QuestTrackerQuestScript.Select()");
            uIManager.questLogWindow.OpenWindow();
            QuestLogUI.Instance.ShowDescription(MyQuest);
        }

        public void DeSelect() {
            //Debug.Log("QuestTrackerQuestScript.DeSelect()");
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