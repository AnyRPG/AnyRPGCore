using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {

    public class NavigableInputField : HighlightButton {

        [SerializeField]
        TMP_InputField inputField = null;

        private bool interacting = false;

        public override bool CaptureCancelButton {
            get {
                if (interacting == true) {
                    return true;
                }
                return false;
            }
        }

        public override void Interact() {
            base.Interact();
            interacting = true;
            inputField.ActivateInputField();
        }

        public override void Cancel() {
            base.Cancel();
            interacting = false;
            inputField.DeactivateInputField();
        }

    }

}