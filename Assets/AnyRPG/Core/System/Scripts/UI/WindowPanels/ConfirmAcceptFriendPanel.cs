using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class ConfirmAcceptFriendPanel : WindowPanel {

        [Header("Confirm Accept Friend Panel")]

        [SerializeField]
        private TextMeshProUGUI messageText = null;

        /*
        [SerializeField]
        private HighlightButton noButton = null;

        [SerializeField]
        private HighlightButton yesButton = null;
        */

        // game manager references
        private FriendServiceClient friendServiceClient = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            //noButton.Configure(systemGameManager);
            //yesButton.Configure(systemGameManager);

        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            friendServiceClient = systemGameManager.FriendServiceClient;
        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            if (messageText != null) {
                messageText.text = $"You have a friend request from {friendServiceClient.InviteCharacterName}";
            }
        }

        public void CancelAction() {
            //Debug.Log("NewGameMenuController.CancelAction()");
            Close();
            friendServiceClient.DeclineFriendInvite();
        }

        public void ConfirmAction() {
            //Debug.Log("NewGameMenuController.ConfirmAction()");
            Close();
            friendServiceClient.AcceptFriendInvite();
        }

    }

}