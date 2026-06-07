using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class KeyBindSlotScript : ConfiguredMonoBehaviour {

        // a unique string that represents the dictionary key for this keybind throughout the game
        private string actionName = string.Empty;

        [SerializeField]
        private TextMeshProUGUI slotLabel = null;

        [SerializeField]
        private TextMeshProUGUI keyboardButtonLabel = null;

        [SerializeField]
        private HighlightButton keyboardAssignButton = null;

        [SerializeField]
        private HighlightButton unbindButton = null;

        /*
        [SerializeField]
        private TextMeshProUGUI joystickButtonLabel = null;

        [SerializeField]
        private HighlightButton joystickAssignButton = null;

        [SerializeField]
        private TextMeshProUGUI mobileButtonLabel = null;

        [SerializeField]
        private HighlightButton mobileAssignButton = null;
        */

        private InputActionNode inputActionNode = null;

        // game manager references
        private KeyBindManager keyBindManager = null;
        private InputManager inputManager = null;

        public HighlightButton KeyboardAssignButton { get => keyboardAssignButton; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            keyboardAssignButton.Configure(systemGameManager);
            unbindButton.Configure(systemGameManager);
            /*
            joystickAssignButton.Configure(systemGameManager);
            mobileAssignButton.Configure(systemGameManager);
            */
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            keyBindManager = systemGameManager.KeyBindManager;
            inputManager = systemGameManager.InputManager;
        }

        public void Initialize(InputActionNode inputActionNode) {
            //Debug.Log("KeyBindSlotScript.Initialize()");
            this.inputActionNode = inputActionNode;
            actionName = inputActionNode.ActionName;
            slotLabel.text = inputActionNode.Label;

            keyboardButtonLabel.text = inputActionNode.KeyboardString;
            unbindButton.gameObject.SetActive(inputActionNode.KeyboardString != "Click To Bind");
            //this.joystickButtonLabel.text = keyBindNode.JoystickKeyCode.ToString();
            //this.mobileButtonLabel.text = keyBindNode.MobileKeyCode.ToString();
        }

        public void UpdateLabel() {
            //Debug.Log("KeyBindSlotScript.UpdateLabel()");

            if (inputActionNode == null) {
                keyboardButtonLabel.text = string.Empty;
                unbindButton.gameObject.SetActive(false);
                return;
            }
            keyboardButtonLabel.text = inputActionNode.KeyboardString;
            unbindButton.gameObject.SetActive(inputActionNode.KeyboardString != "Click To Bind");
            //this.joystickButtonLabel.text = keyBindNode.JoystickKeyCode.ToString();
            //this.mobileButtonLabel.text = keyBindNode.MobileKeyCode.ToString();
        }

        /*
        public void SetKeyBind() {
            //Debug.Log("KeyBindSlotScript.SetKeyBind()");
            SystemGameManager.Instance.KeyBindManager.BeginKeyBind(keyBindID);
        }
        */

        public void SetKeyBind(int inputDeviceType) {
            //Debug.Log("KeyBindSlotScript.SetKeyBind(" + inputDeviceType + ")");
            keyBindManager.BeginKeyBind(actionName, (InputDeviceType)inputDeviceType);
        }

        public void Unbind() {
            //Debug.Log("KeyBindSlotScript.Unbind()");
            inputManager.UnbindKeyboardAction(actionName);
        }


    }

}