using FishNet.Authenticating;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Logging;
using FishNet.Transporting;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG
{
    /// <summary>
    /// This is an example of a password authenticator.
    /// Never send passwords without encryption.
    /// </summary>
    public class FishNetPasswordAuthenticator : Authenticator
    {
        private SystemGameManager systemGameManager = null;

        //private Dictionary<int, NetworkConnection> connectionRequests = new Dictionary<int, NetworkConnection>();

        #region Public.
        /// <summary>
        /// Called when authenticator has concluded a result for a connection. Boolean is true if authentication passed, false if failed.
        /// Server listens for this event automatically.
        /// </summary>
        public override event Action<NetworkConnection, bool> OnAuthenticationResult;
        #endregion

        /*
        #region Serialized.
        /// <summary>
        /// Password to authenticate.
        /// </summary>
        [Tooltip("Password to authenticate.")]
        [SerializeField]
        private string _password = "HelloWorld";
        #endregion
        */


        public override void InitializeOnce(FishNet.Managing.NetworkManager networkManager)
        {
            base.InitializeOnce(networkManager);

            Debug.Log("FishNetPasswordAuthenticator.InitializeOnce()");

            // get reference to system game manager
            systemGameManager = GameObject.FindObjectOfType<SystemGameManager>();

            //Listen for authentication result
            systemGameManager.NetworkManagerServer.OnAuthenticationResult += HandleAuthenticationResult;

            //Listen for connection state change as client.
            base.NetworkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
            //Listen for broadcast from client. Be sure to set requireAuthentication to false.
            base.NetworkManager.ServerManager.RegisterBroadcast<PasswordBroadcast>(OnPasswordBroadcast, false);
            //Listen to response from server.
            base.NetworkManager.ClientManager.RegisterBroadcast<ResponseBroadcast>(OnResponseBroadcast);
        }

        /*
        public void Start() {
            //Listen for authentication result
            systemGameManager.NetworkManagerServer.OnAuthenticationResult += HandleAuthenticationResult;
        }
        */

        /// <summary>
        /// Called when a connection state changes for the local client.
        /// </summary>
        private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs args)
        {
            Debug.Log($"FishNetPasswordAuthenticator.ClientManager_OnClientConnectionState(): {args.ConnectionState}");

            /* If anything but the started state then exit early.
             * Only try to authenticate on started state. The server
            * doesn't have to send an authentication request before client
            * can authenticate, that is entirely optional and up to you. In this
            * example the client tries to authenticate soon as they connect. */
            if (args.ConnectionState != LocalConnectionState.Started)
                return;

            PasswordBroadcast pb = new PasswordBroadcast()
            {
                Username = systemGameManager.NetworkManagerClient.Username,
                Password = systemGameManager.NetworkManagerClient.Password
            };

            Debug.Log($"FishNetPasswordAuthenticator.ClientManager_OnClientConnectionState(): sending password broadcast: {pb.Username}, {pb.Password}");
            base.NetworkManager.ClientManager.Broadcast(pb);
        }


        /// <summary>
        /// Received on server when a client sends the password broadcast message.
        /// </summary>
        /// <param name="conn">Connection sending broadcast.</param>
        /// <param name="pb"></param>
        private void OnPasswordBroadcast(NetworkConnection conn, PasswordBroadcast pb)
        {
            Debug.Log("FishNetPasswordAuthenticator.OnPasswordBroadcst()");

            /* If client is already authenticated this could be an attack. Connections
             * are removed when a client disconnects so there is no reason they should
             * already be considered authenticated. */
            if (conn.Authenticated)
            {
                NetworkManager.Log($"Client with ID {conn.ClientId} is already authenticated.  Disconnecting");
                conn.Disconnect(true);
                return;
            }

            //bool correctPassword = systemGameManager.NetworkManagerServer.GetLoginToken(conn.ClientId, pb.Username, pb.Password);
            /*
            if (connectionRequests.ContainsKey(conn.ClientId) == true) {
                connectionRequests[conn.ClientId] = conn;
            } else {
                connectionRequests.Add(conn.ClientId, conn);
            }
            */
            systemGameManager.NetworkManagerServer.GetLoginToken(conn.ClientId, pb.Username, pb.Password);
        }

        public void HandleAuthenticationResult(int clientId, bool authenticationPassed) {
            Debug.Log($"FishNetPasswordAuthenticator.HandleAuthenticationResult({clientId}, {authenticationPassed})");
            if (base.NetworkManager.ServerManager.Clients.ContainsKey(clientId) == false) {
                //if (base.NetworkManager.ClientManager.Clients.ContainsKey(clientId) == false) {
                Debug.Log($"FishNetPasswordAuthenticator.HandleAuthenticationResult({clientId}, {authenticationPassed}) COULD NOT FIND CONNECTION FOR CLIENT ID");
                return;
            }
            
            SendAuthenticationResponse(base.NetworkManager.ServerManager.Clients[clientId], authenticationPassed);
            /* Invoke result. This is handled internally to complete the connection or kick client.
             * It's important to call this after sending the broadcast so that the broadcast
             * makes it out to the client before the kick. */
            OnAuthenticationResult?.Invoke(base.NetworkManager.ServerManager.Clients[clientId], authenticationPassed);
        }

        /// <summary>
        /// Received on client after server sends an authentication response.
        /// </summary>
        /// <param name="rb"></param>
        private void OnResponseBroadcast(ResponseBroadcast rb)
        {
            Debug.Log("FishNetPasswordAuthenticator.OnResponseBroadcast()");

            string result = (rb.Passed) ? "Authentication complete." : "Authentication failed.";
            NetworkManager.Log(result);
            if (rb.Passed == false) {
                systemGameManager.NetworkManagerClient.ProcessLoginFailure();
            } else {
                systemGameManager.NetworkManagerClient.ProcessLoginSuccess();
            }
        }

        /// <summary>
        /// Sends an authentication result to a connection.
        /// </summary>
        private void SendAuthenticationResponse(NetworkConnection conn, bool authenticated)
        {
            Debug.Log($"FishNetPasswordAuthenticator.SendAuthenticationResponse({authenticated})");

            /* Tell client if they authenticated or not. This is
            * entirely optional but does demonstrate that you can send
            * broadcasts to client on pass or fail. */
            ResponseBroadcast rb = new ResponseBroadcast()
            {
                Passed = authenticated
            };
            base.NetworkManager.ServerManager.Broadcast(conn, rb, false);
        }

    }


}
