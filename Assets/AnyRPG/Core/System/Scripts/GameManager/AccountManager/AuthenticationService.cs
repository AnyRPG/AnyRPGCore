using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class AuthenticationService : ConfiguredClass {

        public event Action<int, int, bool, bool> OnAuthenticationResult = delegate { };
        public event Action<int> OnAccountLogin = delegate { };
        public event Action<int> OnAccountLogout = delegate { };

        /// <summary>
        /// clientId, loggedInAccount
        /// </summary>
        private Dictionary<int, LoggedInAccount> loggedInAccountsByClient = new Dictionary<int, LoggedInAccount>();

        /// <summary>
        /// accountId, loggedInAccount
        /// </summary>
        private Dictionary<int, LoggedInAccount> loggedInAccounts = new Dictionary<int, LoggedInAccount>();

        /// <summary>
        /// clientId, username
        /// </summary>
        private Dictionary<int, string> loginRequests = new Dictionary<int, string>();

        // game manager references
        private PlayerManagerServer playerManagerServer = null;
        private CharacterGroupServiceServer characterGroupServiceServer = null;
        private ServerDataService serverDataService = null;

        public Dictionary<int, LoggedInAccount> LoggedInAccounts { get => loggedInAccounts; }
        public Dictionary<int, LoggedInAccount> LoggedInAccountsByClient { get => loggedInAccountsByClient; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManagerServer = systemGameManager.PlayerManagerServer;
            characterGroupServiceServer = systemGameManager.CharacterGroupServiceServer;
            serverDataService = systemGameManager.ServerDataService;
        }

        public void AddLoggedInAccount(int clientId, int accountId, string token) {
            //Debug.Log($"NetworkManagerServer.AddLoggedInAccount(clientId: {clientId}, accountId: {accountId}, token: {token})");

            if (loginRequests.ContainsKey(clientId)) {
                if (loggedInAccounts.ContainsKey(accountId)) {
                    //Debug.Log($"NetworkManagerServer.AddLoggedInAccount({clientId}, {accountId}, {token}) : updating existing object");
                    int oldClientId = loggedInAccounts[accountId].clientId;
                    loggedInAccounts[accountId].clientId = clientId;
                    loggedInAccounts[accountId].token = token;
                    loggedInAccounts[accountId].ipAddress = networkManagerServer.GetClientIPAddress(clientId);
                    loggedInAccounts[accountId].disconnected = false;
                    loggedInAccountsByClient.Remove(oldClientId);
                    loggedInAccountsByClient.Add(clientId, loggedInAccounts[accountId]);
                } else {
                    LoggedInAccount loggedInAccount = new LoggedInAccount(clientId, accountId, loginRequests[clientId], token, networkManagerServer.GetClientIPAddress(clientId));
                    loggedInAccounts.Add(accountId, loggedInAccount);
                    loggedInAccountsByClient.Add(clientId, loggedInAccount);
                }
            }
        }

        public void GetLoginToken(int clientId, string username, string password) {
            //Debug.Log($"NetworkManagerServer.GetLoginToken({clientId}, {username}, {password})");

            loginRequests.Add(clientId, username);
            if (systemConfigurationManager.ServerBackend == ServerBackend.APIServer) {
                serverDataService.Login(clientId, username, password);
            } else if (systemConfigurationManager.ServerBackend == ServerBackend.File) {
                LocalLogin(clientId, username, password);
            }
        }

        public void LocalLogin(int clientId, string username, string password) {
            //Debug.Log($"NetworkManagerServer.LobbyLogin({clientId}, {username}, {password})");
            LoginOrCreateAccount(clientId, username, password);
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
                    ProcessLoginResponse(clientId, -1, false, string.Empty);
                } else {
                    ProcessLoginResponse(clientId, userAccount.Id, true, string.Empty);
                }
                return;
            } else {
                (UserAccount userAccount, string message) = Login(new AuthenticationRequest(username, password));
                if (userAccount != null) {
                    // password correct
                    ProcessLoginResponse(clientId, userAccount.Id, true, message);
                    return;
                } else {
                    // password incorrect
                    ProcessLoginResponse(clientId, -1, false, message);
                    return;
                }
            }
        }

        public void ProcessLoginResponse(int clientId, int accountId, bool correctPassword, string token) {
            //Debug.Log($"AuthenticationService.ProcessLoginResponse(clientId: {clientId}, accountId: {accountId}, correctPassword: {correctPassword}, token: {token})");

            SpawnPlayerRequest spawnPlayerRequest = null;
            if (correctPassword == true) {
                if (loggedInAccounts.ContainsKey(accountId) && loggedInAccounts[accountId].disconnected == false) {
                    if (playerManagerServer.ActiveUnitControllers.ContainsKey(accountId)) {
                        // if the player is already logged in, we need to add a spawn request to match the current position and direction of the player
                        spawnPlayerRequest = new SpawnPlayerRequest() {
                            overrideSpawnDirection = true,
                            spawnForwardDirection = playerManagerServer.ActiveUnitControllers[accountId].transform.forward,
                            overrideSpawnLocation = true,
                            spawnLocation = playerManagerServer.ActiveUnitControllers[accountId].transform.position
                        };
                    }
                    // if the account is already logged in, kick the old client
                    playerManagerServer.DespawnPlayerUnit(accountId);
                    networkManagerServer.KickPlayer(accountId);
                } else if (playerManagerServer.PlayerCharacterMonitors.ContainsKey(accountId)) {
                    //Debug.Log($"NetworkManagerServer.ProcessLoginResponse({clientId}, {accountId}, {correctPassword}, {token}) account was disconnected, using last position");
                    // if the account is disconnected but was already logged in, add a spawn request to match the saved position and direction of the player
                    CharacterSaveData saveData = playerManagerServer.PlayerCharacterMonitors[accountId].characterSaveData;
                    spawnPlayerRequest = new SpawnPlayerRequest() {
                        overrideSpawnDirection = true,
                        spawnForwardDirection = new Vector3(saveData.PlayerRotationX, saveData.PlayerRotationY, saveData.PlayerRotationZ),
                        overrideSpawnLocation = true,
                        spawnLocation = new Vector3(saveData.PlayerLocationX, saveData.PlayerLocationY, saveData.PlayerLocationZ)
                    };
                }
                if (spawnPlayerRequest != null) {
                    playerManagerServer.AddSpawnRequest(accountId, spawnPlayerRequest, false);
                }
                AddLoggedInAccount(clientId, accountId, token);
            }
            loginRequests.Remove(clientId);
            OnAuthenticationResult(clientId, accountId, true, correctPassword);

            if (correctPassword == false) {
                return;
            }
            //if (spawnPlayerRequest != null) {
            //}

            OnAccountLogin(accountId);
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                networkManagerServer.AdvertiseLobbyLogin(accountId, loggedInAccounts[accountId].username);
            }
        }

        public void ProcessClientLogout(int accountId) {
            //Debug.Log($"NetworkManagerServer.ProcessClientLogout(accountId: {accountId})");

            if (loggedInAccounts.ContainsKey(accountId) == false) {
                return;
            }

            // remove the player from any lobby games
            networkManagerServer.RemoveLobbyGamePlayer(accountId);

            int clientId = loggedInAccounts[accountId].clientId;

            loggedInAccounts.Remove(accountId);
            loggedInAccountsByClient.Remove(clientId);

            OnAccountLogout(accountId);
            if (networkManagerServer.ServerMode == NetworkServerMode.Lobby) {
                networkManagerServer.AdvertiseLobbyLogout(accountId);
            }
        }

        public void ProcessDeactivateServerMode() {
            loggedInAccountsByClient.Clear();
        }

        public string GetAccountToken(int accountId) {
            //Debug.Log($"NetworkManagerServer.GetClientToken({accountId})");

            if (loggedInAccounts.ContainsKey(accountId)) {
                return loggedInAccounts[accountId].token;
            }
            return string.Empty;
        }

        public void ProcessClientDisconnect(int clientId) {
            //Debug.Log($"NetworkManagerServer.ProcessClientDisconnect({clientId})");

            if (loggedInAccountsByClient.ContainsKey(clientId) == false) {
                return;
            }
            int accountId = loggedInAccountsByClient[clientId].accountId;
            // don't do this - it will remove them from the lobby game
            //ProcessClientLogout(accountId);
            loggedInAccounts[accountId].disconnected = true;

            // remove the player from any character groups
            int characterId = -1;
            if (playerManagerServer.PlayerCharacterMonitors.ContainsKey(accountId)) {
                characterId = playerManagerServer.PlayerCharacterMonitors[accountId].characterSaveData.CharacterId;
            }
            if (characterId != -1) {
                playerCharacterService.SetCharacterOnline(characterId, false);
            }

            playerManagerServer.ProcessDisconnect(accountId);
        }

        public void Logout(int accountId) {
            //Debug.Log($"NetworkManagerServer.Logout(accountId: {accountId})");

            // remove the player from any character groups
            int characterId = -1;
            if (playerManagerServer.PlayerCharacterMonitors.ContainsKey(accountId)) {
                characterId = playerManagerServer.PlayerCharacterMonitors[accountId].characterSaveData.CharacterId;
            }
            /*
            if (characterId != -1) {
                characterGroupServiceServer.RemoveCharacterFromGroup(characterId);
                guildServiceServer.SetCharacterOnline(characterId, false);
                friendServiceServer.SetCharacterOnline(characterId, false);
            }
            */

            playerManagerServer.StopMonitoringPlayerUnit(accountId);
            networkManagerServer.KickPlayer(accountId);
            ProcessClientLogout(accountId);

            if (characterId != -1) {
                playerCharacterService.SetCharacterOnline(characterId, false);
                characterGroupServiceServer.RemoveCharacterFromGroup(characterId);
            }
        }

        public void LogoutByClientId(int clientId) {
            //Debug.Log($"AuthenticationService.LogoutByClientId(clientId: {clientId})");

            if (loggedInAccountsByClient.ContainsKey(clientId) == false) {
                return;
            }
            Logout(loggedInAccountsByClient[clientId].accountId);
        }

        public int GetAccountId(int clientId) {
            //Debug.Log($"AuthenticationService.GetAccountId(clientId: {clientId})");

            if (loggedInAccountsByClient.ContainsKey(clientId) == false) {
                return -1;
            }
            return loggedInAccountsByClient[clientId].accountId;
        }
    }
}