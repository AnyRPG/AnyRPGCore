using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class DialogManager : InteractableOptionManager {

        //public event System.Action OnClearSettings = delegate { };

        private Interactable interactable = null;
        private Dialog dialog = null;
        private Quest quest = null;

        public Dialog Dialog { get => dialog; set => dialog = value; }
        public Interactable Interactable { get => interactable; set => interactable = value; }
        public Quest Quest { get => quest; set => quest = value; }

        public void SetQuestDialog(Quest quest, Interactable interactable, InteractableOptionComponent interactableOptionComponent) {
            //Debug.Log("DialogPanelController.Setup(" + (quest == null ? "null" : quest.DisplayName) + ", " + (interactable == null ? "null" : interactable.DisplayName) + ")");
            this.quest = quest;
            this.interactable = interactable;
            dialog = quest.OpeningDialog;

            BeginInteraction(interactableOptionComponent, false);
        }

        public void SetDialog(Dialog dialog, Interactable interactable, InteractableOptionComponent interactableOptionComponent) {
            //Debug.Log("DialogPanelController.Setup(" + dialog.DisplayName + ", " + interactable.DisplayName + ")");
            this.interactable = interactable;
            this.dialog = dialog;

            BeginInteraction(interactableOptionComponent);
        }

        public override void EndInteraction() {
            base.EndInteraction();

            interactable = null;
            quest = null;
            dialog = null;
        }

    }

}