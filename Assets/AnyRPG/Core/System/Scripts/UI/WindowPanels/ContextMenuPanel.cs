using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ContextMenuPanel : WindowPanel {

        [Header("Context Menu Panel")]

        [SerializeField]
        private HighlightButton inspectButton = null;

        [SerializeField]
        private HighlightButton messageButton = null;

        [SerializeField]
        private HighlightButton tradeButton = null;

        [SerializeField]
        private HighlightButton inviteButton = null;

        [SerializeField]
        private HighlightButton kickButton = null;

        [SerializeField]
        private HighlightButton disbandButton = null;

        [SerializeField]
        private HighlightButton leaveButton = null;

        [SerializeField]
        private UINavigationController uINavigationController = null;

        // game manager references
        private ContextMenuService contextMenuService = null;
        private InspectCharacterService inspectCharacterService = null;
        private NetworkManagerClient networkManagerClient = null;
        private PlayerManager playerManager = null;
        private UIManager uIManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            contextMenuService = systemGameManager.ContextMenuService;
            inspectCharacterService = systemGameManager.InspectCharacterService;
            playerManager = systemGameManager.PlayerManager;
            uIManager = systemGameManager.UIManager;
            networkManagerClient = systemGameManager.NetworkManagerClient;
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("ContextMenuPanel.ProcessOpenWindowNotification()");

            base.ProcessOpenWindowNotification();
            if (contextMenuService.TargetUnitController != null) {
                SetupButtons();
            } else {
                uIManager.contextMenuWindow.CloseWindow();
            }
        }

        private void SetupButtons() {

            // inspect button
            if (contextMenuService.TargetUnitController == playerManager.UnitController) {
                // inspect button should not be available if the target is the player
                inspectButton.gameObject.SetActive(false);
            } else {
                // button should only available if the target has a neutral or better relationship
                // updated to allow any faction to inspect similar to DOS2
                //if (Faction.RelationWith(contextMenuService.TargetUnitController, playerManager.UnitController) >= 0) {
                    inspectButton.gameObject.SetActive(true);
                //} else {
                //    inspectButton.gameObject.SetActive(false);
                //}
            }

            inviteButton.gameObject.SetActive(false);
            kickButton.gameObject.SetActive(false);
            disbandButton.gameObject.SetActive(false);
            leaveButton.gameObject.SetActive(false);
            tradeButton.gameObject.SetActive(false);
            messageButton.gameObject.SetActive(false);

            uINavigationController.UpdateNavigationList();

            if (uINavigationController.ActiveNavigableButtonCount == 0) {
                uIManager.contextMenuWindow.CloseWindow();
            }
        }

        public void Inspect() {
            //Debug.Log("ContextMenuPanel.Inspect()");

            uIManager.contextMenuWindow.CloseWindow();
            inspectCharacterService.SetTargetUnitController(systemGameManager.ContextMenuService.TargetUnitController);
        }

        public void Trade() {
            Debug.Log("ContextMenuPanel.Trade()");
        }

        public void Message() {
            Debug.Log("ContextMenuPanel.Message()");
        }

        public void Invite() {
            Debug.Log("ContextMenuPanel.Invite()");
        }

        public void Kick() {
            Debug.Log("ContextMenuPanel.Kick()");
        }

        public void Disband() {
            Debug.Log("ContextMenuPanel.Disband()");
        }

        public void Leave() {
            Debug.Log("ContextMenuPanel.Leave()");
        }

    }

}