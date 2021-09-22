using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "QuestStartItem", menuName = "AnyRPG/Inventory/Items/QuestStartItem", order = 1)]
    public class QuestStartItem : Item, IUseable, IQuestGiver {

        [Header("Quests")]

        [SerializeField]
        private List<QuestNode> quests = new List<QuestNode>();

        private QuestGiverProps questGiverProps = new QuestGiverProps();

        // game manager references
        protected QuestLog questLog = null;

        public Interactable Interactable { get => null; }
        public QuestGiverProps Props { get => questGiverProps; set => questGiverProps = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            questLog = systemGameManager.QuestLog;
        }

        public override bool Use() {
            //Debug.Log(DisplayName + ".QuestStartItem.Use()");
            // base is currently empty, so doesn't matter if we call it without checking anything
            bool returnValue = base.Use();
            if (returnValue == false) {
                return false;
            }
            if (questGiverProps.Quests != null) {
                if (questLog.HasQuest(questGiverProps.Quests[0].Quest.DisplayName)) {
                    messageFeedManager.WriteMessage("You are already on that quest");
                } else if (questGiverProps.Quests[0].Quest.TurnedIn == true) {
                    messageFeedManager.WriteMessage("You have already completed that quest");
                } else {
                    //Debug.Log(DisplayName + ".QuestStartItem.Use(): showing quests");
                    //Debug.Log("QuestStartItem.Use(): opening questgiver window");
                    if (uIManager.questGiverWindow.IsOpen) {
                        // safety to prevent deletion
                        return false;
                    }
                    //OpenQuestGiverWindow();
                    questLog.ShowQuestGiverDescription(Props.Quests[0].Quest, this);
                }
            }
            return returnValue;
        }

        public bool QuestRequirementsAreMet() {
            //Debug.Log(DisplayName + ".QuestStartItem.QuestRequirementsAreMet()");
            if (questGiverProps.Quests != null) {
                foreach (QuestNode questNode in questGiverProps.Quests) {
                    if (questNode.Quest.MyPrerequisitesMet
                        // the next condition is failing on raw complete quest start items because they are always considered complete
                        //&& questNode.MyQuest.IsComplete == false
                        && questNode.Quest.TurnedIn == false
                        && !questLog.HasQuest(questNode.Quest.DisplayName)
                        && (questNode.Quest.RepeatableQuest == true || questNode.Quest.TurnedIn == false)) {
                        //Debug.Log(DisplayName + ".QuestStartItem.QuestRequirementsAreMet(): return true");
                        return true;
                    } else {
                        //Debug.Log(DisplayName + ".QuestStartItem.QuestRequirementsAreMet(): prereqs: " + questNode.MyQuest.MyPrerequisitesMet + "; complete: " + questNode.MyQuest.IsComplete + "; " + questNode.MyQuest.TurnedIn + "; has: " + questLog.HasQuest(questNode.MyQuest.DisplayName));
                    }
                }
            } else {
                //Debug.Log(DisplayName + ".QuestStartItem.QuestRequirementsAreMet(): return true");
                return true;
            }
            //Debug.Log(DisplayName + ".QuestStartItem.QuestRequirementsAreMet(): return false");
            return false;
        }

        public override bool RequirementsAreMet() {
            //Debug.Log(DisplayName + ".QuestStartItem.RequirementsAreMet()");
            bool returnValue = base.RequirementsAreMet();
            if (returnValue == true) {
                if (!QuestRequirementsAreMet()) {
                    //Debug.Log(DisplayName + ".QuestStartItem.RequirementsAreMet(): return false");
                    return false;
                }
            }
            return base.RequirementsAreMet();
        }

        public void HandleAcceptQuest() {
            //Debug.Log(DisplayName + ".QuestStartItem.HandleAcceptQuest()");
            Remove();
        }

        public void HandleCompleteQuest() {
            //Debug.Log(DisplayName + ".QuestStartItem.HandleCompleteQuest()");
            Remove();
        }

        public bool Interact(CharacterUnit source, int optionIndex = 0) {
            // should not need to be used unless a quest item has more than 1 quest, but here for compatibility with IQuestGiver
            Use();
            return true;
        }

        /*
         * now handled through questLog
        public void OpenQuestGiverWindow() {
            //Debug.Log(DisplayName + ".QuestStartItem.OpenQuestGiverWindow()");
            if (!uIManager.questGiverWindow.IsOpen) {
                //Debug.Log(source + " interacting with " + gameObject.name);
                //uIManager.questGiverWindow.MyCloseableWindowContents.OnOpenWindowHandler += InitWindow;
                uIManager.questGiverWindow.OpenWindow();
            }
        }
        */

        public void UpdateQuestStatus() {
            //Debug.Log(DisplayName + ".QuestStartItem.UpdateQuestStatus()");
            // do nothing because we don't have an indicator over our head or a minimap icon
        }

        public override string GetSummary(ItemQuality usedItemQuality) {
            return base.GetSummary(usedItemQuality) + string.Format("\n<color=green>Use: This item starts a quest</color>");
        }

        public bool EndsQuest(string questName) {
            foreach (QuestNode questNode in questGiverProps.Quests) {
                if (SystemDataFactory.MatchResource(questNode.Quest.DisplayName, questName)) {
                    if (questNode.EndQuest == true) {
                        return true;
                    } else {
                        return false;
                    }
                }
            }
            return false;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (quests != null) {
                foreach (QuestNode questNode in quests) {
                    questNode.SetupScriptableObjects(systemGameManager);
                }
            }
            questGiverProps.Quests = quests;
        }
    }

}