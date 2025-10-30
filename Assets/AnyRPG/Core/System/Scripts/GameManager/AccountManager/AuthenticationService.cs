using UnityEngine;

namespace AnyRPG {
    public class AuthenticationService : ConfiguredClass {

        // game manager references
        UserAccountService userAccountService = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            userAccountService = systemGameManager.UserAccountService;
        }

        public (UserAccount, string) Login(AuthenticationRequest authenticationRequest) {
            UserAccount userAccount = userAccountService.GetUserAccount(authenticationRequest.UserName);
            if (userAccount == null) {
                //Debug.Log($"[LOGIN] invalid username {authenticationRequest.UserName}");
                return (null, "Invalid username");
            }

            if (userAccount.PasswordHash != AuthenticationHelpers.ComputeHash(authenticationRequest.Password, userAccount.Salt)) {
                //Debug.Log($"[LOGIN] invalid password for user {authenticationRequest.UserName}");
                return (null, "Invalid password");
            }

            //Debug.Log($"[LOGIN] Successfully logged in user {authenticationRequest.UserName}");

            return (userAccount, string.Empty);
        }

        public void LoginOrCreateAccount(int clientId, string username, string password) {
            //Debug.Log($"AuthenticationService.LoginOrCreateAccount({clientId}, {username}, ****)");

            if (userAccountService.AccountExists(username) == false) {
                UserAccount userAccount = userAccountService.CreateNewAccount(username, password);
                if (userAccount == null) {
                    networkManagerServer.ProcessLoginResponse(clientId, -1, false, string.Empty);
                } else {
                    networkManagerServer.ProcessLoginResponse(clientId, userAccount.Id, true, string.Empty);
                }
                return;
            } else {
                (UserAccount userAccount, string message) = Login(new AuthenticationRequest(username, password));
                if (userAccount != null) {
                    // password correct
                    networkManagerServer.ProcessLoginResponse(clientId, userAccount.Id, true, message);
                    return;
                } else {
                    // password incorrect
                    networkManagerServer.ProcessLoginResponse(clientId, -1, false, message);
                    return;
                }
            }
        }


    }
}