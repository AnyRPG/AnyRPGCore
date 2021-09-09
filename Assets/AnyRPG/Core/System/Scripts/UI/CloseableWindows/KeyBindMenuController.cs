using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class KeyBindMenuController : WindowContentController {

        [SerializeField]
        private GameObject otherKeyParent = null;

        [SerializeField]
        private GameObject actionKeyParent = null;

        [SerializeField]
        private GameObject systemKeyParent = null;

        [SerializeField]
        private GameObject keyBindButtonPrefab = null;

        //public override event Action<ICloseableWindowContents> OnOpenWindow;

        private KeyBindManager keyBindManager = null;
        private ObjectPooler objectPooler = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            objectPooler = systemGameManager.ObjectPooler;
            keyBindManager = systemGameManager.KeyBindManager;
        }

        private void Start() {
            //Debug.Log("KeyBindMenuController.Start()");
            InitializeKeys();
        }

        private void InitializeKeys() {
            //Debug.Log("KeyBindMenuController.InitializeKeys()");
            foreach (KeyBindNode keyBindNode in keyBindManager.KeyBinds.Values) {
                Transform nodeParent = null;
                if (keyBindNode.KeyBindType == KeyBindType.Action) {
                    nodeParent = actionKeyParent.transform;
                } else if (keyBindNode.KeyBindType == KeyBindType.Normal) {
                    nodeParent = otherKeyParent.transform;
                } else if (keyBindNode.KeyBindType == KeyBindType.Constant) {
                    nodeParent = systemKeyParent.transform;
                }
                KeyBindSlotScript keyBindSlotScript = objectPooler.GetPooledObject(keyBindButtonPrefab, nodeParent).GetComponent<KeyBindSlotScript>();
                keyBindSlotScript.Initialize(keyBindNode);
                keyBindNode.SetSlotScript(keyBindSlotScript);
            }
        }

    }

}