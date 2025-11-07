using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class InspectCharacterService : ConfiguredClass {

        private UnitController targetUnitController;

        // game manager references
        private UIManager uIManager = null;

        public UnitController TargetUnitController { get => targetUnitController; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
        }

        public void SetTargetUnitController(UnitController unitController) {
            if (uIManager.inspectCharacterPanelWindow.IsOpen) {
                uIManager.inspectCharacterPanelWindow.CloseWindow();
            }

            targetUnitController = unitController;
            uIManager.characterPanelWindow.CloseWindow();
            uIManager.inspectCharacterPanelWindow.OpenWindow();
        }
    }

}