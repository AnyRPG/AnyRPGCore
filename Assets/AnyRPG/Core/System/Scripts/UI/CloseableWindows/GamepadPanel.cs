using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class GamepadPanel : NavigableInterfaceElement {

        [Header("Gamepad")]

        [SerializeField]
        protected GameObject setDisplay = null;

        [SerializeField]
        protected TMP_Text actionBarSetText = null;

        [SerializeField]
        protected Image leftBackground = null;

        [SerializeField]
        protected GamepadActionBarController leftActionBarController = null;

        [SerializeField]
        protected Image rightBackground = null;

        [SerializeField]
        protected GamepadActionBarController rightActionBarController = null;

        [SerializeField]
        protected Color normalColor = new Color32(0, 0, 0, 0);

        [SerializeField]
        protected Color pressedColor = new Color32(255, 255, 255, 128);


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
                assignActionButton.SetTooltipTransform(rectTransform);
                buttonIndex++;
            }

            /*
            if (controlsManager.GamePadModeActive == false) {
                setDisplay.SetActive(false);
            }
            */
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            actionBarManager = systemGameManager.UIManager.ActionBarManager;
            uIManager = systemGameManager.UIManager;
        }

        public void PressLeftTrigger() {
            //Debug.Log("ActionBarManager.PressLeftTrigger()");
            leftBackground.color = pressedColor;
            leftActionBarController.ShowHints();
        }

        public void LiftLeftTrigger() {
            //Debug.Log("ActionBarManager.LiftLeftTrigger()");
            leftBackground.color = normalColor;
            leftActionBarController.HideHints();
        }

        public void PressRightTrigger() {
            //Debug.Log("ActionBarManager.PressRightTrigger()");
            rightBackground.color = pressedColor;
            rightActionBarController.ShowHints();
        }

        public void LiftRightTrigger() {
            //Debug.Log("ActionBarManager.LiftRightTrigger()");
            rightBackground.color = normalColor;
            rightActionBarController.HideHints();
        }

        public void SetGamepadActionButtonSet(int actionButtonSet) {
            //Debug.Log("ActionBarmanager.SetGamepadActionButtonSet(" + actionButtonSet + ")");
            actionBarSetText.text = "Set " + (actionButtonSet + 1);
        }

        /*
        public void ShowGamepad() {
            leftBackground.gameObject.SetActive(true);
            rightBackground.gameObject.SetActive(true);
        }

        public void HideGamepad() {
            leftBackground.gameObject.SetActive(false);
            rightBackground.gameObject.SetActive(false);
        }
        */

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("InteractionPanelUI.OnCloseWindow()");
            //ClearButtons();
            base.ReceiveClosedWindowNotification();
            // clear this so window doesn't pop open again when it's closed
            actionBarManager.ClearUseableAssignment();
        }

       
       
    }

}