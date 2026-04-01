using System;
using UnityEngine;

namespace AnyRPG {
    public class ContextMenuService : ConfiguredClass {

        public event Action<string> OnBeginPrivateMessage = delegate { };


        private IContextMenuTarget contextMenuTarget = null;

        // game manager references
        private UIManager uIManager = null;

        public IContextMenuTarget ContextMenuTarget { get => contextMenuTarget; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
        }

        public void ShowContextMenu(IContextMenuTarget contextMenuTarget, Vector2 mousePosition) {
            //Debug.Log($"ContextMenuService.ShowContextMenu({mousePosition})");

            if (uIManager.contextMenuWindow.IsOpen) {
                uIManager.contextMenuWindow.CloseWindow();
            }

            this.contextMenuTarget = contextMenuTarget;
            uIManager.contextMenuWindow.RectTransform.position = new Vector3(mousePosition.x, mousePosition.y);
            uIManager.contextMenuWindow.OpenWindow();
        }

        public void ClearContextMenuTarget() {
            //Debug.Log($"ContextMenuService.ClearContextMenuTarget()");
            contextMenuTarget = null;
        }

        public void CloseContextMenu() {
            //Debug.Log($"ContextMenuService.CloseContextMenu()");

            ClearContextMenuTarget();
            uIManager.contextMenuWindow.CloseWindow();
        }

        public void BeginPrivateMessage(UnitController unitController) {
            //Debug.Log($"ContextMenuService.BeginPrivateMessage()");

            if (unitController == null) {
                return;
            }
            OnBeginPrivateMessage($"{systemConfigurationManager.PrivateMessageChatCommand} {unitController.DisplayName} ");
        }
    }

}