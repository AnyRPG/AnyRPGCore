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

        public Interactable Interactable { get => null; }
        public QuestGiverProps Props { get => questGiverProps; set => questGiverProps = value; }

        public override bool Use() {
            Debug.Log(DisplayName + ".QuestStartItem.Use()");
            // base is currently empty, so doesn't matter if we call it without checking anything
            bool returnValue = base.Use();
            if (returnValue == false) {
                return false;
            }
            if (questGiverProps.Quests != null) {
                if (QuestLog.MyInstance.HasQuest(questGiverProps.Quests[0].MyQuest.DisplayName)) {
                    MessageFeedManager.MyInstance.WriteMessage("You are already on that quest");
                } else {
                    Debug.Log(DisplayName + ".QuestStartItem.Use(): showing quests");
                    //Debug.Log("QuestStartItem.Use(): opening questgiver window");
                    if (PopupWindowManager.MyInstance.questGiverWindow.IsOpen) {
                        // safety to prevent deletion
                        return false;
                    }
                    OpenQuestGiverWindow();
                    QuestGiverUI.MyInstance.ShowDescription(Props.Quests[0].MyQuest, this);
                }
            }
            return returnValue;
        }

        public bool QuestRequirementsAreMet() {
            //Debug.Log(DisplayName + ".QuestStartItem.QuestRequirementsAreMet()");
            if (questGiverProps.Quests != null) {
                foreach (QuestNode questNode in questGiverProps.Quests) {
                    if (questNode.MyQuest.MyPrerequisitesMet
                        // the next condition is failing on raw complete quest start items because they are always considered complete
                        //&& questNode.MyQuest.IsComplete == false
                        && questNode.MyQuest.TurnedIn == false
                        && !QuestLog.MyInstance.HasQuest(questNode.MyQuest.DisplayName)) {
                        //Debug.Log(DisplayName + ".QuestStartItem.QuestRequirementsAreMet(): return true");
                        return true;
                    } else {
                        //Debug.Log(DisplayName + ".QuestStartItem.QuestRequirementsAreMet(): prereqs: " + questNode.MyQuest.MyPrerequisitesMet + "; complete: " + questNode.MyQuest.IsComplete + "; " + questNode.MyQuest.TurnedIn + "; has: " + QuestLog.MyInstance.HasQuest(questNode.MyQuest.DisplayName));
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
            Debug.Log(DisplayName + ".QuestStartItem.HandleAcceptQuest()");
            Remove();
        }

        public void HandleCompleteQuest() {
            Debug.Log(DisplayName + ".QuestStartItem.HandleCompleteQuest()");
            Remove();
        }

        public bool Interact(CharacterUnit source) {
            // should not need to be used unless a quest item has more than 1 quest, but here for compatibility with IQuestGiver
            Use();
            return true;
        }

        public void OpenQuestGiverWindow() {
            Debug.Log(DisplayName + ".QuestStartItem.OpenQuestGiverWindow()");
            if (!PopupWindowManager.MyInstance.questGiverWindow.IsOpen) {
                //Debug.Log(source + " interacting with " + gameObject.name);
                //PopupWindowManager.MyInstance.questGiverWindow.MyCloseableWindowContents.OnOpenWindowHandler += InitWindow;
                PopupWindowManager.MyInstance.questGiverWindow.OpenWindow();
            }
        }

        public void UpdateQuestStatus() {
            Debug.Log(DisplayName + ".QuestStartItem.UpdateQuestStatus()");
            // do nothing because we don't have an indicator over our head or a minimap icon
        }

        public override string GetSummary() {
            return base.GetSummary() + string.Format("\n<color=green>Use: This item starts a quest</color>");
        }

        public bool EndsQuest(string questName) {
            foreach (QuestNode questNode in questGiverProps.Quests) {
                if (SystemResourceManager.MatchResource(questNode.MyQuest.DisplayName, questName)) {
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
            questGiverProps.Quests = quests;
        }
    }

}