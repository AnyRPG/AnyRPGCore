using System;
using UnityEngine;

namespace AnyRPG {
    public class ContextMenuService : ConfiguredClass {

        public event Action<string> OnBeginPrivateMessage = delegate { };

        private UnitController targetUnitController;

        // game manager references
        private UIManager uIManager = null;

        public UnitController TargetUnitController { get => targetUnitController; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
        }

        public void ShowContextMenu(UnitController unitController, Vector2 mousePosition) {
            //Debug.Log($"ContextMenuService.ShowContextMenu({unitController.gameObject.name}, {mousePosition})");

            if (uIManager.contextMenuWindow.IsOpen) {
                uIManager.contextMenuWindow.CloseWindow();
            }

            targetUnitController = unitController;
            uIManager.contextMenuWindow.RectTransform.position = new Vector3(mousePosition.x, mousePosition.y);
            uIManager.contextMenuWindow.OpenWindow();
        }

        public void ClearContextMenuTarget() {
            //Debug.Log($"ContextMenuService.ClearContextMenuTarget()");
            targetUnitController = null;
        }

        public void CloseContextMenu() {
            //Debug.Log($"ContextMenuService.CloseContextMenu()");
            ClearContextMenuTarget();
            uIManager.contextMenuWindow.CloseWindow();
        }

        public void BeginPrivateMessage() {
            //Debug.Log($"ContextMenuService.BeginPrivateMessage()");

            if (targetUnitController == null) {
                return;
            }
            OnBeginPrivateMessage($"{systemConfigurationManager.PrivateMessageChatCommand} {targetUnitController.DisplayName} ");
        }
    }

}