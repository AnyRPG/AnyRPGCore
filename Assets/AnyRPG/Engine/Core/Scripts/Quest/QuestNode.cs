using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class QuestNode {

        [SerializeField]
        private bool startQuest = true;

        [SerializeField]
        private bool endQuest = true;

        [SerializeField]
        private string questName = string.Empty;

        //[SerializeField]
        private Quest questTemplate;

        private GameObject questObject;

        public bool MyStartQuest { get => startQuest; set => startQuest = value; }
        public bool MyEndQuest { get => endQuest; set => endQuest = value; }
        public Quest MyQuest { get => questTemplate; set => questTemplate = value; }
        public GameObject MyGameObject { get => questObject; set => questObject = value; }

        public void SetupScriptableObjects() {

            questTemplate = null;
            if (questName != null && questName != string.Empty) {
                Quest quest = SystemQuestManager.MyInstance.GetResource(questName);
                if (quest != null) {
                    questTemplate = quest;
                } else {
                    Debug.LogError("QuestNode.SetupScriptableObjects(): Could not find quest : " + questName + " while inititalizing a quest node.  CHECK INSPECTOR");
                }
            } else {
                Debug.LogError("QuestNode.SetupScriptableObjects(): questName was null or empty while inititalizing a quest node.  CHECK INSPECTOR");
            }
        }

    }

}