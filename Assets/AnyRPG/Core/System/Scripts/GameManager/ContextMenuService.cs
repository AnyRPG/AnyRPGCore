using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class ContextMenuService : ConfiguredClass {

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
    }

}