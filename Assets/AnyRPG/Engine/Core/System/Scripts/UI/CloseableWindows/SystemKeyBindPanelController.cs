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


        private void Start() {
            //Debug.Log("KeyBindMenuController.Start()");
            InitializeKeys();
        }

        public void OnEnable() {
            ToggleMovementPanel();
        }

        private void InitializeKeys() {
            //Debug.Log("KeyBindMenuController.InitializeKeys()");
            foreach (KeyBindNode keyBindNode in SystemGameManager.Instance.KeyBindManager.MyKeyBinds.Values) {
                Transform nodeParent = null;
                if (keyBindNode.MyKeyBindType == KeyBindType.Action) {
                    nodeParent = actionBarsKeyParent.transform;
                } else if (keyBindNode.MyKeyBindType == KeyBindType.Normal) {
                    nodeParent = movementKeyParent.transform;
                } else if (keyBindNode.MyKeyBindType == KeyBindType.Constant || keyBindNode.MyKeyBindType == KeyBindType.System) {
                    nodeParent = systemKeyParent.transform;
                }
                KeyBindSlotScript keyBindSlotScript = ObjectPooler.Instance.GetPooledObject(keyBindButtonPrefab, nodeParent).GetComponent<KeyBindSlotScript>();
                keyBindSlotScript.Initialize(keyBindNode);
                keyBindNode.SetSlotScript(keyBindSlotScript);
            }
        }

        public void ResetPanels() {
            // turn off all panels
            PanelMovement.gameObject.SetActive(false);
            PanelCombat.gameObject.SetActive(false);
            PanelGeneral.gameObject.SetActive(false);

        }

        public void ResetButtons() {
            movementButton.DeSelect();
            actionBarsButton.DeSelect();
            systemButton.DeSelect();
        }

        public void ToggleMovementPanel() {
            ResetPanels();
            PanelMovement.gameObject.SetActive(true);

            ResetButtons();
            movementButton.Select();
        }

        public void ToggleActionBarsPanel() {
            ResetPanels();
            PanelCombat.gameObject.SetActive(true);

            ResetButtons();
            actionBarsButton.Select();
        }

        public void ToggleSystemPanel() {
            ResetPanels();
            PanelGeneral.gameObject.SetActive(true);

            ResetButtons();
            systemButton.Select();
        }


    }

}