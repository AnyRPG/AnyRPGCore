using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class StatusEffectWindowPanel : NavigableInterfaceElement {
        
        [Header("Status Effect Window")]

        [SerializeField]
        private MainStatusEffectPanelController statusEffectPanel = null;

        [SerializeField]
        private UINavigationGrid uINavigationGrid = null;

        // game manager references
        protected UIManager uIManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            statusEffectPanel.Configure(systemGameManager);
            statusEffectPanel.SetStatusEffectWindowPanel(this);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            uIManager = systemGameManager.UIManager;
        }

        public void SetTarget(UnitController unitController) {
            //Debug.Log(gameObject.name + ".StatusEffectWindowPanel.SetTarget(" + unitController.DisplayName + ")");
            if (statusEffectPanel != null) {
                statusEffectPanel.SetTarget(unitController);
            }
        }

        public void ClearTarget() {
            //Debug.Log(gameObject.name + ".StatusEffectPanelController.ClearTarget()");

            if (statusEffectPanel != null) {
                statusEffectPanel.ClearTarget();
            }
        }

        public void AddStatusNode(StatusEffectNodeScript statusEffectNodeScript) {
            //Debug.Log(gameObject.name + ".StatusEffectWindowPanel.AddStatusNode()");
            if (uINavigationControllers[0].ActiveNavigableButtonCount == 0) {
                uIManager.AddNavigableInterfaceElement(this);
            }
            uINavigationControllers[0].AddActiveButton(statusEffectNodeScript);
            UpdateGrid();
        }

        public void RemoveStatusNode(StatusEffectNodeScript statusEffectNodeScript) {
            //Debug.Log(gameObject.name + ".StatusEffectWindowPanel.AddStatusNode()");
            uINavigationControllers[0].ClearActiveButton(statusEffectNodeScript);
            UpdateGrid();

            // only process active element code if actively browsing this panel
            if (windowManager.WindowStack.Count > 0 && windowManager.WindowStack[windowManager.WindowStack.Count - 1] == this) {
                if (uINavigationControllers[0].ActiveNavigableButtonCount > 0) {
                    uINavigationControllers[0].FocusCurrentButton();
                } else {
                    uIManager.RemoveNavigableInterfaceElement(this);

                    // status effects can be removed when not browsing this window
                    // check if this window is actively browsed, and if so, navigate to the next one instead
                    if (windowManager.WindowStack[windowManager.WindowStack.Count - 1] == this) {
                        windowManager.RemoveWindow(this);
                        UnFocus();
                        windowManager.NavigateInterface();
                    }
                }
            }

        }

        private void UpdateGrid() {
            //Debug.Log(gameObject.name + ".StatusEffectWindowPanel.UpdateGrid()");
            uINavigationGrid.NumRows = Mathf.CeilToInt((float)(uINavigationGrid.ActiveNavigableButtonCount) / 8f);

        }

    }

}