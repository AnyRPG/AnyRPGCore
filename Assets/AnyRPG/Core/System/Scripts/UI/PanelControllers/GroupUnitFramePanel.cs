using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class GroupUnitFramePanel : UnitFramePanelBase {

        protected override void HandleLeftClick(Vector2 mousePosition) {
            Debug.Log($"GroupUnitFramePanel.HandleLeftClick({mousePosition})");
            base.HandleLeftClick(mousePosition);
            playerManager.UnitController.SetTarget(namePlateController.Interactable);
        }

    }

}