using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class NameChangePanel : WindowPanel {

        [SerializeField]
        private TMP_InputField textInput = null;

        // game manager references
        private PlayerManagerClient playerManagerClient = null;
        private NameChangeManagerClient nameChangeManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            systemEventManager.OnNameChange += HandleNameChange;
        }

        private void HandleNameChange(string newName) {
            Close();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManagerClient = systemGameManager.PlayerManagerClient;
            nameChangeManager = systemGameManager.NameChangeManagerClient;
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
            //Debug.Log("NameChangePanelController.CancelAction()");
            Close();
        }

        public void ConfirmAction() {
            //Debug.Log("NameChangePanelController.ConfirmAction()");
            if (textInput.text != null && textInput.text != string.Empty) {
                nameChangeManager.RequestChangePlayerName(playerManagerClient.UnitController, textInput.text);
            }
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("NameChangePanelController.ProcessOpenWindowNotification()");
            base.ProcessOpenWindowNotification();
            textInput.text = playerManagerClient.UnitController.BaseCharacter.CharacterName;
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("NameChangePanelController.ReceiveClosedWindowNotification()");
            base.ReceiveClosedWindowNotification();
            nameChangeManager.EndInteraction();
        }


    }

}