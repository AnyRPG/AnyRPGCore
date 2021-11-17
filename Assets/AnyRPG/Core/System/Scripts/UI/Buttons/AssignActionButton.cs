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

        // game manager references
        protected ActionBarManager actionBarManager = null;
        protected UIManager uIManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            actionBarManager = systemGameManager.UIManager.ActionBarManager;
        }

        public void SetIndex(int index) {
            actionButtonIndex = index;
        }

        public override void Select() {
            base.Select();
            icon.sprite = actionBarManager.AssigningUseable.Icon;
            icon.color = Color.white;
            backgroundImage.color = Color.black;
        }

        public override void DeSelect() {
            base.DeSelect();
            icon.sprite = null;
            icon.color = hiddenColor;
            backgroundImage.color = hiddenColor;
        }

        public override void Accept() {
            base.Accept();
            actionBarManager.AssignUseableByIndex(actionButtonIndex);
            uIManager.assignToActionBarsWindow.CloseWindow();
        }

    }

}