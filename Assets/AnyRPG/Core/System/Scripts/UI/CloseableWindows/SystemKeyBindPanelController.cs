using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemKeyBindPanelController : WindowContentController {

        //public override event Action<ICloseableWindowContents> OnOpenWindow;

        [SerializeField]
        private GameObject movementKeyParent = null;

        [SerializeField]
        private GameObject actionBarsKeyParent = null;

        [SerializeField]
        private GameObject systemKeyParent = null;

        [SerializeField]
        private GameObject keyBindButtonPrefab = null;

        [Header("Panels")]
        [Tooltip("The UI Sub-Panel under KEY BINDINGS for MOVEMENT")]
        public GameObject PanelMovement = null;

        [Tooltip("The UI Sub-Panel under KEY BINDINGS for COMBAT")]
        public GameObject PanelCombat = null;

        [Tooltip("The UI Sub-Panel under KEY BINDINGS for GENERAL")]
        public GameObject PanelGeneral = null;

        [Header("Buttons")]
        public HighlightButton movementButton = null;
        public HighlightButton actionBarsButton = null;
        public HighlightButton systemButton = null;

        // game manager references
        KeyBindManager keyBindManager = null;
        ObjectPooler objectPooler = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            InitializeKeys();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            keyBindManager = systemGameManager.KeyBindManager;
            objectPooler = systemGameManager.ObjectPooler;
        }

        /*
        public void OnEnable() {
            ToggleMovementPanel();
        }
        */

        private void InitializeKeys() {
            //Debug.Log("KeyBindMenuController.InitializeKeys()");
            foreach (KeyBindNode keyBindNode in keyBindManager.KeyBinds.Values) {
                Transform nodeParent = null;
                UINavigationController uINavigationController = null;
                if (keyBindNode.KeyBindType == KeyBindType.Action) {
                    nodeParent = actionBarsKeyParent.transform;
                    uINavigationController = uINavigationControllers[2];
                } else if (keyBindNode.KeyBindType == KeyBindType.Normal) {
                    nodeParent = movementKeyParent.transform;
                    uINavigationController = uINavigationControllers[1];
                } else if (keyBindNode.KeyBindType == KeyBindType.Constant || keyBindNode.KeyBindType == KeyBindType.System) {
                    nodeParent = systemKeyParent.transform;
                    uINavigationController = uINavigationControllers[3];
                }
                if (nodeParent != null) {
                    KeyBindSlotScript keyBindSlotScript = objectPooler.GetPooledObject(keyBindButtonPrefab, nodeParent).GetComponent<KeyBindSlotScript>();
                    keyBindSlotScript.Configure(systemGameManager);
                    keyBindSlotScript.Initialize(keyBindNode);
                    keyBindNode.SetSlotScript(keyBindSlotScript);
                    uINavigationController.AddActiveButton(keyBindSlotScript.KeyboardAssignButton);
                }
            }
        }

        public void ResetPanels() {
            // turn off all panels
            PanelMovement.gameObject.SetActive(false);
            PanelCombat.gameObject.SetActive(false);
            PanelGeneral.gameObject.SetActive(false);

        }

        /*
        public void ResetButtons() {
            movementButton.DeSelect();
            actionBarsButton.DeSelect();
            systemButton.DeSelect();
        }
        */

        public void ToggleMovementPanel() {
            Debug.Log("SystemKeyBindPanelController.ToggleMovementPanel()");
            ResetPanels();
            PanelMovement.gameObject.SetActive(true);

            //ResetButtons();
            movementButton.Select();
            uINavigationControllers[0].UnHightlightButtons(movementButton);
        }

        public void ToggleActionBarsPanel() {
            ResetPanels();
            PanelCombat.gameObject.SetActive(true);

            //ResetButtons();
            actionBarsButton.Select();
            uINavigationControllers[0].UnHightlightButtons(actionBarsButton);
        }

        public void ToggleSystemPanel() {
            ResetPanels();
            PanelGeneral.gameObject.SetActive(true);

            //ResetButtons();
            systemButton.Select();
            uINavigationControllers[0].UnHightlightButtons(systemButton);
        }

        public override void ReceiveOpenWindowNotification() {
            Debug.Log("SystemKeyBindPanelController.ReceiveOpenWindowNotification()");
            base.ReceiveOpenWindowNotification();
            //currentNavigationController.Focus();
            //uINavigationControllers[0].SetCurrentButton(movementButton);
            movementButton.HighlightBackground();
            ToggleMovementPanel();
            uINavigationControllers[0].UnFocus();

        }


    }

}