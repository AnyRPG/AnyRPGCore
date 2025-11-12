using UnityEngine;

namespace AnyRPG {
    public class LoggedInAccount {

        public int accountId;
        public int clientId;
        public string username;
        public string token;
        public string ipAddress;
        public bool disconnected = false;

        public LoggedInAccount(int clientId, int accountId, string username, string token, string ipAddress) {
            this.accountId = accountId;
            this.clientId = clientId;
            this.username = username;
            this.token = token;
            this.ipAddress = ipAddress;
        }
    }
}
