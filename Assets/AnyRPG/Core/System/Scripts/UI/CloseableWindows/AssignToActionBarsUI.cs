using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class AssignToActionBarsUI : WindowContentController {

        [Header("Assign To Action Bars")]

        [SerializeField]
        List<AssignActionButton> assignActionButtons = new List<AssignActionButton>();

        // game manager references
        private ActionBarManager actionBarManager = null;
        private UIManager uIManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            int buttonIndex = 0;
            foreach (AssignActionButton assignActionButton in assignActionButtons) {
                assignActionButton.SetIndex(buttonIndex);
                buttonIndex++;
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            actionBarManager = systemGameManager.UIManager.ActionBarManager;
            uIManager = systemGameManager.UIManager;
        }



        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("InteractionPanelUI.OnCloseWindow()");
            //ClearButtons();
            base.ReceiveClosedWindowNotification();
            // clear this so window doesn't pop open again when it's closed
            actionBarManager.ClearUseableAssignment();
        }

       
       
    }

}