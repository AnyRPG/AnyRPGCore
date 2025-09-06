using FishNet.Component.Transforming;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {
    public class FishNetActivatableObject : NetworkBehaviour {

        public readonly SyncVar<bool> objectActive = new SyncVar<bool>();

        private bool initialized = false;

        private void OnEnable() {
            //Debug.Log($"{gameObject.name}.FishNetActivatableObject.OnEnable()");

            if (initialized == false) {
                return;
            }
            if (base.IsServerStarted == false) {
                return;
            }
            objectActive.Value = true;
            HandleEnableClient(true);
        }

        private void OnDisable() {
            //Debug.Log($"{gameObject.name}.FishNetActivatableObject.OnDisable()");

            if (initialized == false) {
                return;
            }
            if (base.IsServerStarted == false) {
                return;
            }
            objectActive.Value = false;
            HandleEnableClient(false);
        }

        [ObserversRpc]
        public void HandleEnableClient(bool enabled) {
            //Debug.Log($"{gameObject.name}.FishNetActivatableObject.HandleEnableClient({enabled})");

            HandleEnable(enabled);
        }

        public void HandleEnable(bool enabled) {
            //Debug.Log($"{gameObject.name}.FishNetActivatableObject.HandleEnable({enabled})");

            gameObject.SetActive(enabled);
        }

        public override void OnStartClient() {
            //Debug.Log($"{gameObject.name}.FishNetActivatableObject.OnStartClient()");

            base.OnStartClient();
            initialized = true;
            if (objectActive.Value == false) {
                HandleEnable(false);
            }
        }

        public override void OnStartServer() {
            //Debug.Log($"{gameObject.name}.FishNetActivatableObject.OnStartServer()");

            base.OnStartServer();
            initialized = true;
            objectActive.Value = gameObject.activeSelf;
        }

    }
}

