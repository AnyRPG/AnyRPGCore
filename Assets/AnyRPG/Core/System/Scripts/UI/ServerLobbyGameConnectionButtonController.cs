using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class ServerLobbyGameConnectionButtonController : ConfiguredMonoBehaviour {

        [SerializeField]
        private TextMeshProUGUI sceneNameText = null;

        [SerializeField]
        private TextMeshProUGUI leaderNameText = null;

        [SerializeField]
        private TextMeshProUGUI gameInfoText = null;

        [SerializeField]
        private TextMeshProUGUI statusText = null;

        //[SerializeField]
        //private HighlightButton joinButton = null;

        private int gameId;
        private LobbyGame lobbyGame = null;

        // game manager references
        NetworkManagerClient networkManagerClient = null;

        public TextMeshProUGUI PlayerNameText { get => leaderNameText; }
        public TextMeshProUGUI PlayerInfoText { get => gameInfoText; }
        //public HighlightButton JoinButton { get => joinButton; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            networkManagerClient = systemGameManager.NetworkManagerClient;
            //joinButton.Configure(systemGameManager);
        }

        public void SetGame(LobbyGame lobbyGame) {
            this.lobbyGame = lobbyGame;
            this.gameId = lobbyGame.GameId;
            sceneNameText.text = lobbyGame.SceneResourceName;
            leaderNameText.text = lobbyGame.LeaderUserName;
            RefreshPlayerCount();
            RefreshStatus();
        }

        public void RefreshPlayerCount() {
            gameInfoText.text = $"{lobbyGame.PlayerList.Count} Player{(lobbyGame.PlayerList.Count == 1 ? "" : "s")}";
        }

        public void RefreshStatus() {
            if (lobbyGame.InProgress) {
                statusText.text = "In Progress";
            } else {
                statusText.text = "Waiting for Players";
            }
        }
    
    }
}
