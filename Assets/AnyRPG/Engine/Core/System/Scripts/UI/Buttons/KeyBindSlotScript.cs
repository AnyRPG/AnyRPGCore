using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class KeyBindSlotScript : ConfiguredMonoBehaviour {

        // a unique string that represents the dictionary key for this keybind throughout the game
        [SerializeField]
        private string keyBindID = string.Empty;

        [SerializeField]
        private TextMeshProUGUI slotLabel = null;

        [SerializeField]
        private TextMeshProUGUI keyboardButtonLabel = null;

        [SerializeField]
        private HighlightButton keyboardAssignButton = null;

        [SerializeField]
        private TextMeshProUGUI joystickButtonLabel = null;

        [SerializeField]
        private HighlightButton joystickAssignButton = null;

        [SerializeField]
        private TextMeshProUGUI mobileButtonLabel = null;

        [SerializeField]
        private HighlightButton mobileAssignButton = null;

        private KeyBindNode keyBindNode = null;

        // game manager references
        KeyBindManager keyBindManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            keyboardAssignButton.Configure(systemGameManager);
            joystickAssignButton.Configure(systemGameManager);
            mobileAssignButton.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            keyBindManager = systemGameManager.KeyBindManager;
        }

        public void Initialize(KeyBindNode keyBindNode) {
            //Debug.Log("KeyBindSlotScript.Initialize()");
            this.keyBindNode = keyBindNode;
            this.keyBindID = keyBindNode.MyKeyBindID;
            //Debug.Log("KeyBindSlotScript.Initialize(): keyBindID: " + this.keyBindID);
            this.slotLabel.text = keyBindNode.MyLabel;
            this.keyboardButtonLabel.text = (keyBindNode.Control ? "ctrl+" : "") + (keyBindNode.Shift ? "shift+" : "") + keyBindNode.KeyboardKeyCode.ToString();
            this.joystickButtonLabel.text = keyBindNode.JoystickKeyCode.ToString();
            this.mobileButtonLabel.text = keyBindNode.MobileKeyCode.ToString();
        }

        /*
        public void SetKeyBind() {
            //Debug.Log("KeyBindSlotScript.SetKeyBind()");
            SystemGameManager.Instance.KeyBindManager.BeginKeyBind(keyBindID);
        }
        */

        public void SetKeyBind(int inputDeviceType) {
            //Debug.Log("KeyBindSlotScript.SetKeyBind(" + inputDeviceType + ")");
            keyBindManager.BeginKeyBind(keyBindID, (InputDeviceType)inputDeviceType);
        }


    }

}