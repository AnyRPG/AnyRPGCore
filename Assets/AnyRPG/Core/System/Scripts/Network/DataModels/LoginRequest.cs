using UnityEngine;

namespace AnyRPG {

    public class LoginRequest {
        public string UserName = string.Empty;
        public string Password = string.Empty;

        public LoginRequest(string username, string password) {
            UserName = username;
            Password = password;
        }
    }
}
