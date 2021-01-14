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
        private TextMeshProUGUI keyboardButtonLabel = null;

        [SerializeField]
        private TextMeshProUGUI joystickButtonLabel = null;

        [SerializeField]
        private TextMeshProUGUI mobileButtonLabel = null;

        private KeyBindNode keyBindNode = null;

        public void Initialize(KeyBindNode keyBindNode) {
            //Debug.Log("KeyBindSlotScript.Initialize()");
            this.keyBindNode = keyBindNode;
            this.keyBindID = keyBindNode.MyKeyBindID;
            //Debug.Log("KeyBindSlotScript.Initialize(): keyBindID: " + this.keyBindID);
            this.slotLabel.text = keyBindNode.MyLabel;
            this.keyboardButtonLabel.text = (keyBindNode.MyControl ? "ctrl+" : "") + (keyBindNode.MyShift ? "shift+" : "") + keyBindNode.MyKeyCode.ToString();
            this.joystickButtonLabel.text = keyBindNode.MyJoystickKeyCode.ToString();
            this.mobileButtonLabel.text = keyBindNode.MyMobileKeyCode.ToString();
        }

        /*
        public void SetKeyBind() {
            //Debug.Log("KeyBindSlotScript.SetKeyBind()");
            KeyBindManager.MyInstance.BeginKeyBind(keyBindID);
        }
        */

        public void SetKeyBind(int inputDeviceType) {
            //Debug.Log("KeyBindSlotScript.SetKeyBind(" + inputDeviceType + ")");
            KeyBindManager.MyInstance.BeginKeyBind(keyBindID, (InputDeviceType)inputDeviceType);
        }


    }

}