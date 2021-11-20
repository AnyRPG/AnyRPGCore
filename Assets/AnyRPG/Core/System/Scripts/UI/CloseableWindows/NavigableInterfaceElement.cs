using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class NavigableInterfaceElement : CloseableWindowContents {

        [Header("Navigable Interface Element")]

        [SerializeField]
        protected Image outline = null;

        protected Color hiddenColor = new Color32(0, 0, 0, 0);

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            UnFocus();
        }

        public virtual void Focus() {
            Debug.Log(gameObject.name + ".NavigableInterfaceElement.Focus()");
            outline.color = Color.white;
            if (currentNavigationController != null) {
                currentNavigationController.Focus();
            }
        }

        public virtual void UnFocus() {
            Debug.Log(gameObject.name + ".NavigableInterfaceElement.UnFocus()");
            outline.color = hiddenColor;
            HideControllerHints();
            if (currentNavigationController != null) {
                currentNavigationController.UnFocus();
            }
        }

        public override void ReceiveOpenWindowNotification() {
            Debug.Log(gameObject.name + ".NavigableInterfaceElement.ReceiveOpenWindowNotification()");
            base.ReceiveOpenWindowNotification();
            if (currentNavigationController != null) {
                currentNavigationController.UnFocus();
            }
        }

    }

}