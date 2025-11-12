using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class DialogManagerClient : InteractableOptionManager {

        //public event System.Action OnClearSettings = delegate { };

        private DialogComponent dialogComponent = null;
        private Interactable interactable = null;
        private Dialog dialog = null;
        private Quest quest = null;

        public Dialog Dialog { get => dialog; set => dialog = value; }
        public Interactable Interactable { get => interactable; set => interactable = value; }
        public Quest Quest { get => quest; set => quest = value; }

        public void SetQuestDialog(Quest quest, Interactable interactable, InteractableOptionComponent interactableOptionComponent, int componentIndex, int choiceIndex) {
            //Debug.Log("DialogPanelController.Setup(" + (quest == null ? "null" : quest.DisplayName) + ", " + (interactable == null ? "null" : interactable.DisplayName) + ")");
            this.quest = quest;
            this.interactable = interactable;
            dialog = quest.OpeningDialog;

            BeginInteraction(interactableOptionComponent, componentIndex, choiceIndex, false);
        }

        public void SetDialog(Dialog dialog, Interactable interactable, DialogComponent dialogComponent, int componentIndex, int choiceIndex) {
            //Debug.Log("DialogPanelController.Setup(" + dialog.DisplayName + ", " + interactable.DisplayName + ")");
            this.interactable = interactable;
            this.dialog = dialog;
            this.dialogComponent = dialogComponent;

            BeginInteraction(dialogComponent, componentIndex, choiceIndex);
        }

        public void RequestTurnInDialog(UnitController sourceUnitController) {
            //Debug.Log("DialogPanelController.Setup(" + (quest == null ? "null" : quest.DisplayName) + ", " + (interactable == null ? "null" : interactable.DisplayName) + ")");
            if (systemGameManager.GameMode == GameMode.Local) {
                dialogComponent.TurnInDialog(sourceUnitController, dialog);
            } else {
                networkManagerClient.RequestTurnInDialog(dialogComponent.Interactable, componentIndex, dialog);
            }
        }

        public void RequestTurnInQuestDialog(UnitController sourceUnitController) {
            //Debug.Log($"DialogManagerClient.RequestTurnInQuestDialog({sourceUnitController.gameObject.name})");
            if (systemGameManager.GameMode == GameMode.Local) {
                sourceUnitController.CharacterDialogManager.TurnInDialog(dialog);
            } else {
                networkManagerClient.RequestTurnInQuestDialog(dialog);
            }
        }

        public override void EndInteraction() {
            base.EndInteraction();

            interactable = null;
            quest = null;
            dialog = null;
        }

    }

}