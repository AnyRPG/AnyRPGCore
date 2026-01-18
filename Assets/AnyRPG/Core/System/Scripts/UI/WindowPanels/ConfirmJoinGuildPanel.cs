using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class ConfirmJoinGuildPanel : WindowPanel {

        [Header("Confirm Join Guild Panel")]

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
        private GuildServiceClient guildServiceClient = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            //noButton.Configure(systemGameManager);
            //yesButton.Configure(systemGameManager);

        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            networkManagerClient = systemGameManager.NetworkManagerClient;
            guildServiceClient = systemGameManager.GuildServiceClient;
        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            if (messageText != null) {
                messageText.text = $"You have been invited to a guild by {guildServiceClient.InviteLeaderName}";
            }
        }

        public void CancelAction() {
            //Debug.Log("NewGameMenuController.CancelAction()");
            Close();
            networkManagerClient.DeclineGuildInvite();
        }

        public void ConfirmAction() {
            //Debug.Log("NewGameMenuController.ConfirmAction()");
            Close();
            networkManagerClient.AcceptGuildInvite(guildServiceClient.InviteGuildId);
        }

    }

}