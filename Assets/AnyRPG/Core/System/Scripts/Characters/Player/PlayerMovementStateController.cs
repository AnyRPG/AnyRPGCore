using AnyRPG;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace AnyRPG {
    /// <summary>
    /// Custom character controller, to be used by attaching the component to an object
    /// and writing scripts attached to the same object that recieve the "StateUpdate" message
    /// </summary>
    public class PlayerMovementStateController : ConfiguredMonoBehaviour {

        public delegate void UpdateDelegate();
        public event UpdateDelegate AfterSingleUpdate;

        // game manager references
        protected PlayerManager playerManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
        }

        public void Init() {
            //Debug.Log($"{gameObject.name}.AnyRPGCharacterController.OrchestrateStartup()");
            gameObject.SendMessage("StateStart", SendMessageOptions.DontRequireReceiver);
        }

        void Update() {
            if (playerManager.ActiveUnitController == null) {
                return;
            }
            SingleUpdate();
            return;
        }

        void SingleUpdate() {
            //Debug.Log($"{gameObject.name}.AnyRPGCharacterController.SingleUpdate()");

            gameObject.SendMessage("StateUpdate", SendMessageOptions.DontRequireReceiver);

            if (AfterSingleUpdate != null)
                AfterSingleUpdate();
        }

        private void OnDisable() {
            //Debug.Log($"{gameObject.name}.AnyRPGCharacterController.OnDisable()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
        }

        private void OnEnable() {
            //Debug.Log($"{gameObject.name}.AnyRPGCharacterController.OnEnable()");
        }

    }


}