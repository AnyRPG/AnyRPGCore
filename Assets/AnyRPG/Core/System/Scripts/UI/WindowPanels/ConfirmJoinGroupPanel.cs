using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class ConfirmJoinGroupPanel : WindowPanel {

        [Header("Confirm Join Group Panel")]

        [SerializeField]
        private TextMeshProUGUI messageText = null;

        /*
        [SerializeField]
        private HighlightButton noButton = null;

        [SerializeField]
        private HighlightButton yesButton = null;
        */

        // game manager references
        private UIManager uIManager = null;
        private NetworkManagerClient networkManagerClient = null;
        private CharacterGroupServiceClient characterGroupServiceClient = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            //noButton.Configure(systemGameManager);
            //yesButton.Configure(systemGameManager);

        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            networkManagerClient = systemGameManager.NetworkManagerClient;
            characterGroupServiceClient = systemGameManager.CharacterGroupServiceClient;
        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            if (messageText != null) {
                messageText.text = $"You have been invited to a group by {characterGroupServiceClient.InviteLeaderName}";
            }
        }

        public void CancelAction() {
            //Debug.Log("NewGameMenuController.CancelAction()");
            uIManager.confirmJoinGroupWindow.CloseWindow();
            networkManagerClient.DeclineCharacterGroupInvite();
        }

        public void ConfirmAction() {
            //Debug.Log("NewGameMenuController.ConfirmAction()");
            uIManager.confirmJoinGroupWindow.CloseWindow();
            networkManagerClient.AcceptCharacterGroupInvite(characterGroupServiceClient.InviteGroupId);
        }

    }

}