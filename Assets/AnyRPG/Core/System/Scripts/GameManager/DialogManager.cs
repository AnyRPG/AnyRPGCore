using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class DialogManager : ConfiguredMonoBehaviour {

        public event System.Action OnClearSettings = delegate { };
        public event System.Action OnConfirmAction = delegate { };
        public event System.Action OnEndInteraction = delegate { };

        private Interactable interactable = null;
        private Dialog dialog = null;
        private Quest quest = null;

        public Dialog Dialog { get => dialog; set => dialog = value; }
        public Interactable Interactable { get => interactable; set => interactable = value; }
        public Quest Quest { get => quest; set => quest = value; }

        // game manager references
        private UIManager uIManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
        }

        public void ConfirmAction() {
            OnConfirmAction();
        }

        public void EndInteraction() {
            OnEndInteraction();
        }

        public void ClearSettings() {
            interactable = null;
            quest = null;
            dialog = null;
            OnClearSettings();
        }

        public void ViewQuestDialog(Quest quest, Interactable interactable) {
            //Debug.Log("DialogPanelController.Setup(" + (quest == null ? "null" : quest.DisplayName) + ", " + (interactable == null ? "null" : interactable.DisplayName) + ")");
            ClearSettings();
            this.quest = quest;
            this.interactable = interactable;
            dialog = quest.OpeningDialog;
            uIManager.dialogWindow.OpenWindow();
        }

        public void ViewDialog(Dialog dialog, Interactable interactable) {
            //Debug.Log("DialogPanelController.Setup(" + dialog.DisplayName + ", " + interactable.DisplayName + ")");
            ClearSettings();
            this.interactable = interactable;
            this.dialog = dialog;
            uIManager.dialogWindow.OpenWindow();
        }

    }

}