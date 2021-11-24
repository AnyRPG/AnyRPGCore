using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class StatusEffectWindowPanel : NavigableInterfaceElement {
        
        [Header("Status Effect Window")]

        [SerializeField]
        private MainStatusEffectPanelController statusEffectPanel = null;

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
            Debug.Log(gameObject.name + ".StatusEffectWindowPanel.SetTarget(" + unitController.DisplayName + ")");
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
            Debug.Log(gameObject.name + ".StatusEffectWindowPanel.AddStatusNode()");
            if (uINavigationControllers[0].ActiveNavigableButtonCount == 0) {
                uIManager.NavigableInterfaceElements.Add(this);
            }
            uINavigationControllers[0].AddActiveButton(statusEffectNodeScript);
        }

        public void RemoveStatusNode(StatusEffectNodeScript statusEffectNodeScript) {
            Debug.Log(gameObject.name + ".StatusEffectWindowPanel.AddStatusNode()");
            uINavigationControllers[0].ClearActiveButton(statusEffectNodeScript);
            if (uINavigationControllers[0].ActiveNavigableButtonCount == 0) {
                uIManager.NavigableInterfaceElements.Remove(this);
                if (windowManager.WindowStack[windowManager.WindowStack.Count - 1] == this) {
                    windowManager.NavigateInterface();
                }
            }
        }

    }

}