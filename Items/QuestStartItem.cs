using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "QuestStartItem", menuName = "AnyRPG/Inventory/Items/QuestStartItem", order = 1)]
    public class QuestStartItem : Item, IUseable, IQuestGiver {

        [SerializeField]
        private List<QuestNode> quests = new List<QuestNode>();

        public List<QuestNode> MyQuests { get => quests; }

        public Interactable MyInteractable { get => null; }

        public override bool Use() {
            //Debug.Log("QuestStartItem.Use()");
            // base is currently empty, so doesn't matter if we call it without checking anything
            bool returnValue = base.Use();
            if (returnValue == false) {
                return false;
            }
            if (MyQuests != null) {
                if (QuestLog.MyInstance.HasQuest(MyQuests[0].MyQuest.MyDisplayName)) {
                    MessageFeedManager.MyInstance.WriteMessage("You are already on that quest");
                } else {
                    //Debug.Log("QuestStartItem.Use(): showing quests");
                    //Debug.Log("QuestStartItem.Use(): opening questgiver window");
                    if (PopupWindowManager.MyInstance.questGiverWindow.IsOpen) {
                        // safety to prevent deletion
                        return false;
                    }
                    QuestGiverUI.MyInstance.MyQuestGiver = this as IQuestGiver;
                    OpenQuestGiverWindow();
                    QuestGiverUI.MyInstance.ShowDescription((this as IQuestGiver).MyQuests[0].MyQuest);
                }
            }
            return returnValue;
        }

        public bool QuestRequirementsAreMet() {
            if (MyQuests != null) {
                foreach (QuestNode questNode in MyQuests) {
                    if (questNode.MyQuest.MyPrerequisitesMet && questNode.MyQuest.IsComplete == false && questNode.MyQuest.TurnedIn == false && !QuestLog.MyInstance.HasQuest(questNode.MyQuest.MyDisplayName)) {
                        return true;
                    }
                }
            } else {
                return true;
            }
            return false;
        }

        public override bool RequirementsAreMet() {
            bool returnValue = base.RequirementsAreMet();
            if (returnValue == true) {
                if (!QuestRequirementsAreMet()) {
                    return false;
                }
            }
            return base.RequirementsAreMet();
        }

        public void HandleAcceptQuest() {
            //Debug.Log("QuestStartItem.HandleAcceptQuest()");
            Remove();
        }

        public void HandleCompleteQuest() {
            //Debug.Log("QuestStartItem.HandleAcceptQuest()");
            Remove();
        }

        public bool Interact(CharacterUnit source) {
            // should not need to be used unless a quest item has more than 1 quest, but here for compatibility with IQuestGiver
            Use();
            return true;
        }

        public void OpenQuestGiverWindow() {
            //Debug.Log(gameObject.name + ".QuestStartItem.OpenQuestGiverWindow()");
            if (!PopupWindowManager.MyInstance.questGiverWindow.IsOpen) {
                //Debug.Log(source + " interacting with " + gameObject.name);
                //PopupWindowManager.MyInstance.questGiverWindow.MyCloseableWindowContents.OnOpenWindowHandler += InitWindow;
                PopupWindowManager.MyInstance.questGiverWindow.OpenWindow();
            }
        }

        public void UpdateQuestStatus() {
            // do nothing because we don't have an indicator over our head or a minimap icon
        }

        public override string GetSummary() {
            return base.GetSummary() + string.Format("\n<color=green>Use: This item starts a quest</color>");
        }

        public bool EndsQuest(string questName) {
            foreach (QuestNode questNode in quests) {
                if (SystemResourceManager.MatchResource(questNode.MyQuest.MyDisplayName, questName)) {
                    if (questNode.MyEndQuest == true) {
                        return true;
                    } else {
                        return false;
                    }
                }
            }
            return false;
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();

            if (quests != null) {
                foreach (QuestNode questNode in quests) {
                    questNode.SetupScriptableObjects();
                }
            }
        }
    }

}