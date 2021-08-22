using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New QuestGiver Profile", menuName = "AnyRPG/QuestGiverProfile")]
    [System.Serializable]
    public class QuestGiverProfile : DescribableResource {

        [Header("Quest Giver")]

        [Tooltip("List of quests that this questgiver offers or finishes")]
        [SerializeField]
        private List<QuestNode> quests = new List<QuestNode>();

        public List<QuestNode> MyQuests { get => quests; }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            //Debug.Log(MyName + ".QuestGiverProfile.SetupScriptableObjects()");
            base.SetupScriptableObjects(systemGameManager);

            if (quests != null) {
                foreach (QuestNode questNode in quests) {
                    if (questNode != null) {
                        questNode.SetupScriptableObjects();
                    }
                }
            }
        }
    }

}