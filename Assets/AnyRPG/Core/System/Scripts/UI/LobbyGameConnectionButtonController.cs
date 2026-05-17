using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class LobbyGameConnectionButtonController : ConfiguredMonoBehaviour {

        [SerializeField]
        private TextMeshProUGUI playerNameText = null;

        [SerializeField]
        private TextMeshProUGUI playerInfoText = null;

        [SerializeField]
        private HighlightButton kickButton = null;

        private int accountId;

        // game manager references
        NetworkManagerServer networkManagerServer = null;

        public TextMeshProUGUI PlayerNameText { get => playerNameText; }
        public TextMeshProUGUI PlayerInfoText { get => playerInfoText; }
        public HighlightButton KickButton { get => kickButton; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            networkManagerServer = systemGameManager.NetworkManagerServer;
            kickButton.Configure(systemGameManager);
        }

        public void SetAccountId(int accountId, string userName) {
            this.accountId = accountId;
            playerNameText.text = userName;
            //playerInfoText.text = ipAddress;

        }

        public void KickPlayer() {
            networkManagerServer.Logout(accountId);
        }


    }

}
