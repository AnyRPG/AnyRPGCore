using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestStartItem", menuName = "Inventory/Items/QuestStartItem", order = 1)]
public class QuestStartItem : Item, IUseable, IQuestGiver {

    [SerializeField]
    private QuestNode[] quests;

    public QuestNode[] MyQuests { get => quests; }
    public Interactable MyInteractable { get => null; }

    public override void Use() {
        //Debug.Log("QuestStartItem.Use()");
        base.Use();
        if (MyQuests != null) {
            //Debug.Log("QuestStartItem.Use(): showing quests");
            QuestGiverUI.MyInstance.MyQuestGiver = this as IQuestGiver;
            //Debug.Log("QuestStartItem.Use(): opening questgiver window");
            if (PopupWindowManager.MyInstance.questGiverWindow.IsOpen) {
                // safety to prevent deletion
                return;
            }
            OpenQuestGiverWindow();
            QuestGiverUI.MyInstance.ShowDescription((this as IQuestGiver).MyQuests[0].MyQuest);
        }
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

}
