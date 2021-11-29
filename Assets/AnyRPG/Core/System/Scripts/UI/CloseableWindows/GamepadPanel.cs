using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class GamepadPanel : NavigableInterfaceElement {

        [Header("Gamepad")]

        [SerializeField]
        protected List<ActionButton> actionButtons = new List<ActionButton>();

        // game manager references
        protected ActionBarManager actionBarManager = null;
        protected UIManager uIManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            
            int buttonIndex = 0;
            foreach (ActionButton assignActionButton in actionButtons) {
                assignActionButton.SetIndex(buttonIndex);
                assignActionButton.SetPanel(this);
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