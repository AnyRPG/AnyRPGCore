using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class AssignActionButton : NavigableElement {

        [Header("Assign Action Button")]

        [SerializeField]
        protected Image backgroundImage = null;

        [SerializeField]
        protected Image icon = null;

        protected int actionButtonIndex = 0;

        protected CloseableWindowContents windowPanel = null;

        // game manager references
        protected ActionBarManager actionBarManager = null;
        protected UIManager uIManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            actionBarManager = systemGameManager.UIManager.ActionBarManager;
            ResetGraphics();
        }

        public void SetIndex(int index) {
            actionButtonIndex = index;
        }

        public void SetWindowPanel(CloseableWindowContents windowPanel) {
            this.windowPanel = windowPanel;
        }

        public override void Select() {
            base.Select();
            icon.sprite = actionBarManager.AssigningUseable.Icon;
            icon.color = Color.white;
            backgroundImage.color = Color.black;
        }

        public override void DeSelect() {
            base.DeSelect();
            ResetGraphics();
        }

        private void ResetGraphics() {
            icon.sprite = null;
            icon.color = hiddenColor;
            backgroundImage.color = hiddenColor;
        }

        public override void Accept() {
            base.Accept();
            actionBarManager.AssignUseableByIndex(actionButtonIndex);
            (uIManager.GamepadWindow.CloseableWindowContents as GamepadPanel).SetNavigationControllerByIndex(windowPanel.GetNavigationControllerIndex());
            (uIManager.GamepadWindow.CloseableWindowContents as GamepadPanel).CurrentNavigationController.SetCurrentIndex(windowPanel.CurrentNavigationController.CurrentIndex);

            uIManager.assignToActionBarsWindow.CloseWindow();
        }

    }

}