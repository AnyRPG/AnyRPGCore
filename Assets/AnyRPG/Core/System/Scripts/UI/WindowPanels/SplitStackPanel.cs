using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class SplitStackPanel : WindowPanel {

        [SerializeField]
        private TMP_InputField textInput = null;

        private int maxStackSize = 1;

        // game manager references
        private PlayerManagerClient playerManagerClient = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManagerClient = systemGameManager.PlayerManagerClient;
        }

        /// <summary>
        /// disable hotkeys and movement while text input is active
        /// </summary>
        public void ActivateTextInput() {
            controlsManager.ActivateTextInput();
        }

        public void DeativateTextInput() {
            controlsManager.DeactivateTextInput();
        }

        public void CancelAction() {
            //Debug.Log("SplitStackPanel.CancelAction()");

            Close();
        }

        public void CorrectInputValue() {
            if (textInput != null) {
                int stackSize = 1;
                if (int.TryParse(textInput.text, out stackSize)) {
                    if (stackSize < 1) {
                        stackSize = 1;
                    } else if (stackSize > maxStackSize) {
                        stackSize = maxStackSize;
                    }
                } else {
                    stackSize = 1;
                }
                textInput.text = stackSize.ToString();
            }
        }

        public void ConfirmAction() {
            //Debug.Log("SplitStackPanel.ConfirmAction()");

            if (textInput.text != null && textInput.text != string.Empty) {
                // get int from text input and make sure it is between 1 and max stack size
                int stackSize = 1;
                if (int.TryParse(textInput.text, out stackSize)) {
                    if (stackSize < 1) {
                        stackSize = 1;
                    } else if (stackSize > maxStackSize) {
                        stackSize = maxStackSize;
                    }
                } else {
                    stackSize = 1;
                }
                playerManagerClient.UnitController.CharacterInventoryManager.RequestSplitStack(stackSize);
            }
            Close();
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("SplitStackPanel.ProcessOpenWindowNotification()");

            base.ProcessOpenWindowNotification();
            textInput.text = "1";
            maxStackSize = playerManagerClient.UnitController.CharacterInventoryManager.FromSlot.Count - 1;
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("SplitStackPanel.ReceiveClosedWindowNotification()");

            base.ReceiveClosedWindowNotification();
            playerManagerClient.UnitController.CharacterInventoryManager.FromSlot = null;
        }

    }

}