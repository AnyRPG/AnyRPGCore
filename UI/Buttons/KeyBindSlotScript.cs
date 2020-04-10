using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class KeyBindSlotScript : MonoBehaviour {

        // a unique string that represents the dictionary key for this keybind throughout the game
        [SerializeField]
        private string keyBindID = string.Empty;

        [SerializeField]
        private TextMeshProUGUI slotLabel = null;

        [SerializeField]
        private TextMeshProUGUI buttonLabel = null;

        private KeyBindNode keyBindNode = null;

        public void Initialize(KeyBindNode keyBindNode) {
            this.keyBindNode = keyBindNode;
            this.keyBindID = keyBindNode.MyKeyBindID;
            this.slotLabel.text = keyBindNode.MyLabel;
            this.buttonLabel.text = (keyBindNode.MyControl ? "ctrl+" : "") + (keyBindNode.MyShift ? "shift+" : "") + keyBindNode.MyKeyCode.ToString();
        }

        public void SetKeyBind() {
            //Debug.Log("KeyBindSlotScript.SetKeyBind()");
            KeyBindManager.MyInstance.BeginKeyBind(keyBindID);
        }
    }

}