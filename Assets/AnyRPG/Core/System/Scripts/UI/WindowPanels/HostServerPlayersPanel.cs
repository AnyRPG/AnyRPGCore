using AnyRPG;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;

namespace AnyRPG {
    public class HostServerPlayersPanel : WindowPanel {

        [Header("Host Server Players Panel")]

        [SerializeField]
        protected GameObject playerConnectionTemplate = null;

        [SerializeField]
        protected Transform playerConnectionContainer = null;

        [SerializeField]
        protected UINavigationController playerListNavigationController = null;

        // game manager references
        NetworkManagerServer networkManagerServer = null;
        ObjectPooler objectPooler = null;
        AuthenticationService authenticationService = null;

        /// <summary>
        /// accountId, PlayerConnectionButtonController
        /// </summary>
        private Dictionary<int, PlayerConnectionButtonController> playerButtons = new Dictionary<int, PlayerConnectionButtonController>();

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            networkManagerServer = systemGameManager.NetworkManagerServer;
            objectPooler = systemGameManager.ObjectPooler;
            authenticationService = systemGameManager.AuthenticationService;
        }

        public void PopulatePlayerList() {
            //Debug.Log($"HostServerPanelController.PopulatePlayerList()");

            foreach (KeyValuePair<int, LoggedInAccount> loggedInAccount in authenticationService.LoggedInAccounts) {
                AddPlayerToList(loggedInAccount.Value.accountId, loggedInAccount.Value.username);
            }
        }

        public void AddPlayerToList(int accountId, string userName) {
            //Debug.Log($"HostServerPanelController.AddPlayerToList({accountId}, {userName})");

            if (playerButtons.ContainsKey(accountId)) {
                //Debug.Warning($"HostServerPanelController.AddPlayerToList() - player was already connected, and is reconnecting");
                playerButtons[accountId].UpdateIPAddress(authenticationService.LoggedInAccounts[accountId].ipAddress);
                return;
            }
            GameObject go = objectPooler.GetPooledObject(playerConnectionTemplate, playerConnectionContainer);
            PlayerConnectionButtonController playerConnectionButtonController = go.GetComponent<PlayerConnectionButtonController>();
            playerConnectionButtonController.Configure(systemGameManager);
            playerConnectionButtonController.SetAccountId(accountId, userName, authenticationService.LoggedInAccounts[accountId].ipAddress);
            playerListNavigationController.AddActiveButton(playerConnectionButtonController.KickButton);
            playerButtons.Add(accountId, playerConnectionButtonController);
        }

        public void RemovePlayerFromList(int accountId) {
            //Debug.Log($"HostServerPanelController.RemovePlayerFromList({accountId})");

            if (playerButtons.ContainsKey(accountId)) {
                playerListNavigationController.ClearActiveButton(playerButtons[accountId].KickButton);
                if (playerButtons[accountId].gameObject != null) {
                    playerButtons[accountId].gameObject.transform.SetParent(null);
                    objectPooler.ReturnObjectToPool(playerButtons[accountId].gameObject);
                }
                playerButtons.Remove(accountId);
            }
        }

        public void ClearPlayerList() {

            // clear the skill list so any skill left over from a previous time opening the window aren't shown
            foreach (PlayerConnectionButtonController playerConnectionButtonController in playerButtons.Values) {
                if (playerConnectionButtonController.gameObject != null) {
                    playerConnectionButtonController.gameObject.transform.SetParent(null);
                    objectPooler.ReturnObjectToPool(playerConnectionButtonController.gameObject);
                }
            }
            playerButtons.Clear();
            playerListNavigationController.ClearActiveButtons();
        }

    }
}