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
    public class AnyRPGCharacterController : MonoBehaviour {

        public delegate void UpdateDelegate();
        public event UpdateDelegate AfterSingleUpdate;

        void Awake() {
            gameObject.SendMessage("StateStart", SendMessageOptions.DontRequireReceiver);
        }

        void Update() {

            SingleUpdate();
            return;
        }

        void SingleUpdate() {

            gameObject.SendMessage("StateUpdate", SendMessageOptions.DontRequireReceiver);

            if (AfterSingleUpdate != null)
                AfterSingleUpdate();
        }

    }


}