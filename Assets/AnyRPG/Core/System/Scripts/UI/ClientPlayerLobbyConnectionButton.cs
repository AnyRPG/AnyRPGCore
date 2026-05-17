using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class ClientPlayerLobbyConnectionButton : ConfiguredMonoBehaviour {

        [SerializeField]
        private TextMeshProUGUI playerNameText = null;

        private int accountId;

        // game manager references
        NetworkManagerServer networkManagerServer = null;

        public TextMeshProUGUI PlayerNameText { get => playerNameText; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            networkManagerServer = systemGameManager.NetworkManagerServer;
        }

        public void SetAccountId(int accountId, string userName) {
            this.accountId = accountId;
            playerNameText.text = userName;
        }

    }

}
