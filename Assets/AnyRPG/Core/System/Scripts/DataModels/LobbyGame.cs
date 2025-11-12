using UnityEngine;
using System.Collections.Generic;

namespace AnyRPG {
    public class LobbyGame {
        public int leaderAccountId;
        public string leaderUserName = string.Empty;
        public int gameId;
        public string gameName = string.Empty;
        public string sceneResourceName = string.Empty;
        public bool inProgress = false;
        public bool allowLateJoin = false;

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
            this.leaderAccountId = accountId;
            leaderUserName = userName;
            this.gameId = gameId;
            this.sceneResourceName = sceneResourceName;
            playerList.Add(accountId, new LobbyGamePlayerInfo(accountId, userName));
            this.allowLateJoin = allowLateJoin;
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
