using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PlayOnlineMenuPanel : WindowPanel {

        /*
        [SerializeField]
        private HighlightButton continueButton = null;

        [SerializeField]
        private HighlightButton newGameButton = null;

        [SerializeField]
        private HighlightButton loadGameButton = null;
        */

        // game manager references
        protected UIManager uIManager = null;
        protected SaveManager saveManager = null;
        protected NetworkManagerClient networkManagerClient = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            
            /*
            continueButton.Configure(systemGameManager);
            newGameButton.Configure(systemGameManager);
            loadGameButton.Configure(systemGameManager);
            */
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            saveManager = systemGameManager.SaveManager;
            networkManagerClient = systemGameManager.NetworkManagerClient;
        }

        public void HostServer() {
            //Debug.Log("PlayOnlineMenuController.NewGame()");
            uIManager.playOnlineMenuWindow.CloseWindow();
            uIManager.hostServerWindow.OpenWindow();
        }

        public void JoinServer() {
            //Debug.Log("PlayOnlineMenuController.JoinServer()");
            uIManager.playOnlineMenuWindow.CloseWindow();
            //networkManagerClient.ClientMode = NetworkClientMode.Lobby;
            uIManager.networkLoginWindow.OpenWindow();
        }

    }

}