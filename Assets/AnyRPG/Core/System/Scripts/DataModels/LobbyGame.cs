using UnityEngine;
using System.Collections.Generic;

namespace AnyRPG {
    public class LobbyGame {
        public int LeaderAccountId;
        public string LeaderUserName = string.Empty;
        public int GameId;
        public string GameName = string.Empty;
        public string SceneResourceName = string.Empty;
        public bool InProgress = false;
        public bool AllowLateJoin = false;

        /// <summary>
        /// accountId, LobbyGamePlayerInfo
        /// </summary>
        private Dictionary<int, LobbyGamePlayerInfo> playerList = new Dictionary<int, LobbyGamePlayerInfo>();

        public Dictionary<int, LobbyGamePlayerInfo> PlayerList { get => playerList; set => playerList = value; }

        public LobbyGame() {
            /*
            leaderUserName = string.Empty;
            gameName = string.Empty;
            sceneName = string.Empty;
            */
        }

        public LobbyGame(int accountId, int gameId, string sceneResourceName, string userName, bool allowLateJoin) {
            this.LeaderAccountId = accountId;
            LeaderUserName = userName;
            this.GameId = gameId;
            this.SceneResourceName = sceneResourceName;
            playerList.Add(accountId, new LobbyGamePlayerInfo(accountId, userName));
            this.AllowLateJoin = allowLateJoin;
        }

        public void AddPlayer(int accountId, string userName) {
            playerList.Add(accountId, new LobbyGamePlayerInfo(accountId, userName));
        }

        public void RemovePlayer(int accountId) {
            playerList.Remove(accountId);
        }
    }

    public class LobbyGamePlayerInfo {
        public int accountId;
        public string userName = string.Empty;
        public string unitProfileName = string.Empty;
        public string appearanceString = string.Empty;
        public List<SwappableMeshSaveData> swappableMeshSaveData = new List<SwappableMeshSaveData>();
        public bool ready = false;

        public LobbyGamePlayerInfo() {
        }

        public LobbyGamePlayerInfo(int accountId, string userName) {
            this.accountId = accountId;
            this.userName = userName;
        }
    }

}
