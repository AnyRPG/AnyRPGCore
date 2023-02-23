using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {

    public class MeshModelGroupButton : HighlightButton {

        SwappableMeshAppearancePanelController swappableMeshAppearancePanelController = null;
        string meshModelGroup = string.Empty;

        public void ConfigureButton(SwappableMeshAppearancePanelController swappableMeshAppearancePanelController, string meshModelGroup) {
            this.swappableMeshAppearancePanelController = swappableMeshAppearancePanelController;
            this.meshModelGroup = meshModelGroup;
            text.text = meshModelGroup;
        }

        public override void Interact() {
            base.Interact();
            ShowModelGroup();
        }

        public void ShowModelGroup() {
            swappableMeshAppearancePanelController.ShowModelGroup(meshModelGroup);
        }
    }

}