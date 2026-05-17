using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "QuestStartItem", menuName = "AnyRPG/Inventory/Items/QuestStartItem", order = 1)]
    public class QuestStartItem : Item {

        [Header("Quests")]

        [SerializeField]
        private List<QuestNode> quests = new List<QuestNode>();

        private QuestGiverProps questGiverProps = new QuestGiverProps();

        public QuestGiverProps QuestGiverProps { get => questGiverProps; set => questGiverProps = value; }

        public override InstantiatedItem GetNewInstantiatedItem(SystemGameManager systemGameManager, long itemInstanceId, Item item, ItemQuality usedItemQuality) {
            if ((item is QuestStartItem) == false) {
                return null;
            }
            return new InstantiatedQuestStartItem(systemGameManager, itemInstanceId, item as QuestStartItem, usedItemQuality);
        }

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

        public override string GetDescription(ItemQuality usedItemQuality, int usedItemLevel) {
            return base.GetDescription(usedItemQuality, usedItemLevel) + GetQuestStartItemDescription();
        }

        public string GetQuestStartItemDescription() {
            return string.Format("\n<color=green>Use: This item starts a quest</color>");
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