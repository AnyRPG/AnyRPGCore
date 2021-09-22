using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class QuestNode : ConfiguredClass {

        [SerializeField]
        private bool startQuest = true;

        [SerializeField]
        private bool endQuest = true;

        [SerializeField]
        [ResourceSelector(resourceType = typeof(Quest))]
        private string questName = string.Empty;

        //[SerializeField]
        private Quest questTemplate;

        private GameObject questObject;

        // game manager references
        private SystemDataFactory systemDataFactory = null;

        public bool StartQuest { get => startQuest; set => startQuest = value; }
        public bool EndQuest { get => endQuest; set => endQuest = value; }
        public Quest Quest { get => questTemplate; set => questTemplate = value; }
        public GameObject GameObject { get => questObject; set => questObject = value; }

        public void SetupScriptableObjects(SystemGameManager systemGameManager) {
            Configure(systemGameManager);
            questTemplate = null;
            if (questName != null && questName != string.Empty) {
                Quest quest = systemDataFactory.GetResource<Quest>(questName);
                if (quest != null) {
                    questTemplate = quest;
                } else {
                    Debug.LogError("QuestNode.SetupScriptableObjects(): Could not find quest : " + questName + " while inititalizing a quest node.  CHECK INSPECTOR");
                }
            } else {
                Debug.LogError("QuestNode.SetupScriptableObjects(): questName was null or empty while inititalizing a quest node.  CHECK INSPECTOR");
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemDataFactory = systemGameManager.SystemDataFactory;
        }

    }

}