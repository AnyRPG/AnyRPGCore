using System;
using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class ClientPlayerLobbyGameConnectionButton : ConfiguredMonoBehaviour {

        [SerializeField]
        private TextMeshProUGUI playerNameText = null;

        [SerializeField]
        private TextMeshProUGUI unitProfileNameText = null;

        [SerializeField]
        private TextMeshProUGUI readyText = null;

        private int accountId;

        // game manager references
        NetworkManagerServer networkManagerServer = null;

        public TextMeshProUGUI PlayerNameText { get => playerNameText; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            networkManagerServer = systemGameManager.NetworkManagerServer;
        }

        public void SetClientId(int accountId, string userName, string unitProfileName) {
            this.accountId = accountId;
            playerNameText.text = userName;
            unitProfileNameText.text = unitProfileName;
            SetReadyStatus(false);
        }

        public void SetUnitProfileName(string unitProfileName) {
            unitProfileNameText.text = unitProfileName;
        }

        public void SetReadyStatus(bool ready) {
            //Debug.Log($"ClientPlayerLobbyGameConnectionButton.SetReadyStatus({ready})");
            if (ready) {
                readyText.text = "Ready";
            } else {
                readyText.text = "Not Ready";
            }
        }
    }

}
