using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class InstantiatedQuestStartItem : InstantiatedItem, IQuestGiver {

        private QuestStartItem questStartItem = null;

        //[Header("Quests")]

        /*
        [SerializeField]
        private List<QuestNode> quests = new List<QuestNode>();
        */

        public Interactable Interactable { get => null; }
        public InteractableOptionComponent InteractableOptionComponent { get => null; }

        public QuestGiverProps QuestGiverProps { get => questStartItem.QuestGiverProps; }

        public InstantiatedQuestStartItem(SystemGameManager systemGameManager, long instanceId, QuestStartItem questStartItem, ItemQuality itemQuality) : base(systemGameManager, instanceId, questStartItem, itemQuality) {
            this.questStartItem = questStartItem;
        }

        public override bool Use(UnitController sourceUnitController) {
            //Debug.Log(DisplayName + ".QuestStartItem.Use()");
            // base is currently empty, so doesn't matter if we call it without checking anything
            bool returnValue = base.Use(sourceUnitController);
            if (returnValue == false) {
                return false;
            }
            if (questStartItem.QuestGiverProps.Quests != null) {
                if (sourceUnitController.CharacterQuestLog.HasQuest(questStartItem.QuestGiverProps.Quests[0].Quest.ResourceName)) {
                    sourceUnitController.WriteMessageFeedMessage("You are already on that quest");
                } else if (questStartItem.QuestGiverProps.Quests[0].Quest.TurnedIn(sourceUnitController) == true && questStartItem.QuestGiverProps.Quests[0].Quest.RepeatableQuest == false) {
                    sourceUnitController.WriteMessageFeedMessage("You have already completed that quest");
                } else {
                    //Debug.Log(DisplayName + ".QuestStartItem.Use(): showing quests");
                    //Debug.Log("QuestStartItem.Use(): opening questgiver window");
                    if (uIManager.questGiverWindow.IsOpen) {
                        // safety to prevent deletion
                        return false;
                    }
                    //OpenQuestGiverWindow();
                    sourceUnitController.CharacterQuestLog.ShowQuestGiverDescription(QuestGiverProps.Quests[0].Quest, this);
                    sourceUnitController.UnitEventController.NotifyOnInteractWithQuestStartItem(QuestGiverProps.Quests[0].Quest, slot.GetCurrentInventorySlotIndex(sourceUnitController), instanceId);
                }
            }
            return returnValue;
        }

        public override string GetDescription() {
            //Debug.Log($"{item.ResourceName}.InstantiatedCurrencyItem.GetDescription()");

            return base.GetDescription() + questStartItem.GetQuestStartItemDescription();
        }


        /*
        public bool QuestRequirementsAreMet(UnitController sourceUnitController) {
            //Debug.Log(DisplayName + ".QuestStartItem.QuestRequirementsAreMet()");
            if (questGiverProps.Quests != null) {
                foreach (QuestNode questNode in questGiverProps.Quests) {
                    if (questNode.Quest.PrerequisitesMet(sourceUnitController)
                        // the next condition is failing on raw complete quest start items because they are always considered complete
                        //&& questNode.MyQuest.IsComplete == false
                        && questNode.Quest.TurnedIn(sourceUnitController) == false
                        && !sourceUnitController.CharacterQuestLog.HasQuest(questNode.Quest.ResourceName)
                        && (questNode.Quest.RepeatableQuest == true || questNode.Quest.TurnedIn(sourceUnitController) == false)) {
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
        */

        // FIX ME - SHOULD THIS BE HERE INSTEAD OF ITEM
        /*
        public override bool RequirementsAreMet(UnitController sourceUnitController) {
            //Debug.Log(DisplayName + ".QuestStartItem.RequirementsAreMet()");
            bool returnValue = base.RequirementsAreMet(sourceUnitController);
            if (returnValue == true) {
                if (!QuestRequirementsAreMet(sourceUnitController)) {
                    //Debug.Log(DisplayName + ".QuestStartItem.RequirementsAreMet(): return false");
                    return false;
                }
            }
            return base.RequirementsAreMet(sourceUnitController);
        }
        */

        public void HandleAcceptQuest() {
            //Debug.Log(DisplayName + ".QuestStartItem.HandleAcceptQuest()");
            Remove();
        }

        public void HandleCompleteQuest() {
            //Debug.Log(DisplayName + ".QuestStartItem.HandleCompleteQuest()");
            Remove();
        }

        public bool Interact(UnitController sourceUnitController, int componentIndex = 0, int choiceIndex = 0) {
            // should not need to be used unless a quest item has more than 1 quest, but here for compatibility with IQuestGiver
            Use(sourceUnitController);
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

        public void UpdateQuestStatus(UnitController sourceUnitController) {
            //Debug.Log(DisplayName + ".QuestStartItem.UpdateQuestStatus()");
            // do nothing because we don't have an indicator over our head or a minimap icon
        }

        /*
        public override string GetDescription(ItemQuality usedItemQuality, int usedItemLevel) {
            return base.GetDescription(usedItemQuality, usedItemLevel) + string.Format("\n<color=green>Use: This item starts a quest</color>");
        }
        */

        public bool EndsQuest(string questName) {
            foreach (QuestNode questNode in questStartItem.QuestGiverProps.Quests) {
                if (SystemDataUtility.MatchResource(questNode.Quest.ResourceName, questName)) {
                    if (questNode.EndQuest == true) {
                        return true;
                    } else {
                        return false;
                    }
                }
            }
            return false;
        }

        public void RequestAcceptQuest(UnitController unitController, Quest currentQuest) {
            unitController.CharacterQuestLog.RequestAcceptQuestItemQuest(this, currentQuest);
        }

        public void RequestCompleteQuest(UnitController unitController, Quest currentQuest, QuestRewardChoices questRewardChoices) {
            unitController.CharacterQuestLog.RequestCompleteQuestItemQuest(this, currentQuest, questRewardChoices);
        }

        public void CompleteQuest(UnitController sourceUnitController, Quest quest, QuestRewardChoices questRewardChoices) {
            //Debug.Log("InstantiatedQuestStartItem.CompleteQuest()");
            if (!quest.IsComplete(sourceUnitController)) {
                Debug.LogWarning("QuestGiverManager.CompleteQuest(): currentQuest is not complete, exiting!");
                return;
            }

            quest.CompleteQuest(sourceUnitController, questRewardChoices);
            HandleCompleteQuest();
        }
    }

}