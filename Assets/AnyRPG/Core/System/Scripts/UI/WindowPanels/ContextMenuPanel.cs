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
        private HighlightButton promoteButton = null;

        [SerializeField]
        private UINavigationController uINavigationController = null;

        // game manager references
        private CharacterGroupServiceClient characterGroupServiceClient = null;
        private ContextMenuService contextMenuService = null;
        private InspectCharacterService inspectCharacterService = null;
        private NetworkManagerClient networkManagerClient = null;
        private PlayerManager playerManager = null;
        private UIManager uIManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            characterGroupServiceClient = systemGameManager.CharacterGroupServiceClient;
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

            SetupInviteButton();
            SetupKickButton();
            SetupDisbandButton();
            SetupLeaveButton();
            SetupPromoteButton();
            tradeButton.gameObject.SetActive(false);
            messageButton.gameObject.SetActive(false);

            uINavigationController.UpdateNavigationList();

            if (uINavigationController.ActiveNavigableButtonCount == 0) {
                uIManager.contextMenuWindow.CloseWindow();
            }
        }

        private void SetupInviteButton() {
            if (contextMenuService.TargetUnitController == playerManager.UnitController) {
                // invite button should not be available if the target is the player
                inviteButton.gameObject.SetActive(false);
                return;
            }

            if (systemGameManager.GameMode == GameMode.Local) {
                // can only invite in network mode
                inviteButton.gameObject.SetActive(false);
                return;
            }

            if (contextMenuService.TargetUnitController.UnitControllerMode != UnitControllerMode.Player) {
                // can only invite players
                inviteButton.gameObject.SetActive(false);
                return;
            }

            if (Faction.RelationWith(contextMenuService.TargetUnitController, playerManager.UnitController) < 0) {
                // can only invite neutral or better
                inviteButton.gameObject.SetActive(false);
                return;
            }

            if (contextMenuService.TargetUnitController.CharacterGroupManager.IsInGroup()) {
                // target is already in a group
                inviteButton.gameObject.SetActive(false);
                return;
            }

            // all checks passed.  this character can be invited
            inviteButton.gameObject.SetActive(true);
        }

        private void SetupPromoteButton() {
            // check if the player is in a group
            CharacterGroup characterGroup = characterGroupServiceClient.CurrentCharacterGroup;
            if (characterGroup == null) {
                // player is not in a group
                promoteButton.gameObject.SetActive(false);
                return;
            }

            // check if the player is the leader
            if (characterGroup.leaderPlayerCharacterId != playerManager.UnitController.CharacterId) {
                // player is not the leader
                promoteButton.gameObject.SetActive(false);
                return;
            }

            // check that the target is not the player
            if (contextMenuService.TargetUnitController == playerManager.UnitController) {
                // cannot promote yourself
                promoteButton.gameObject.SetActive(false);
                return;
            }

            // check if the target is in the group
            if (characterGroup.CharacterIdList[UnitControllerMode.Player].Contains(contextMenuService.TargetUnitController.CharacterId) == false) {
                // target is not in the group
                promoteButton.gameObject.SetActive(false);
                return;
            }

            // all checks passed.  this character can be promoted
            promoteButton.gameObject.SetActive(true);
        }

        private void SetupKickButton() {
            if (contextMenuService.TargetUnitController == playerManager.UnitController) {
                // invite button should not be available if the target is the player
                kickButton.gameObject.SetActive(false);
                return;
            }

            if (systemGameManager.GameMode == GameMode.Local) {
                // can only invite in network mode
                kickButton.gameObject.SetActive(false);
                return;
            }

            // check if the player is in a group
            CharacterGroup characterGroup = characterGroupServiceClient.CurrentCharacterGroup;
            if (characterGroup == null) {
                // player is not in a group
                kickButton.gameObject.SetActive(false);
                return;
            }

            // check if the player is the leader
            if (characterGroup.leaderPlayerCharacterId != playerManager.UnitController.CharacterId) {
                // player is not the leader
                kickButton.gameObject.SetActive(false);
                return;
            }

            // check if the target is in the group
            if (characterGroup.CharacterIdList[UnitControllerMode.Player].Contains(contextMenuService.TargetUnitController.CharacterId) == false) {
                // target is not in the group
                kickButton.gameObject.SetActive(false);
                return;
            }
            // all checks passed.  this character can be kicked
            kickButton.gameObject.SetActive(true);
        }

        private void SetupDisbandButton() {
            if (contextMenuService.TargetUnitController != playerManager.UnitController) {
                // only the leader can disband themselves
                disbandButton.gameObject.SetActive(false);
                return;
            }

            if (systemGameManager.GameMode == GameMode.Local) {
                // can only invite in network mode
                disbandButton.gameObject.SetActive(false);
                return;
            }

            // check if the player is in a group
            CharacterGroup characterGroup = characterGroupServiceClient.CurrentCharacterGroup;
            if (characterGroup == null) {
                // player is not in a group
                disbandButton.gameObject.SetActive(false);
                return;
            }

            // check if the player is the leader
            if (characterGroup.leaderPlayerCharacterId != playerManager.UnitController.CharacterId) {
                // player is not the leader
                disbandButton.gameObject.SetActive(false);
                return;
            }

            // all checks passed.  this character can be kicked
            disbandButton.gameObject.SetActive(true);
        }

        private void SetupLeaveButton() {
            if (contextMenuService.TargetUnitController != playerManager.UnitController) {
                // only the leader can leave themselves
                leaveButton.gameObject.SetActive(false);
                return;
            }

            if (systemGameManager.GameMode == GameMode.Local) {
                // can only invite in network mode
                leaveButton.gameObject.SetActive(false);
                return;
            }

            // check if the player is in a group
            CharacterGroup characterGroup = characterGroupServiceClient.CurrentCharacterGroup;
            if (characterGroup == null) {
                // player is not in a group
                leaveButton.gameObject.SetActive(false);
                return;
            }

            // all checks passed.  this character can be kicked
            leaveButton.gameObject.SetActive(true);
        }

        public void Inspect() {
            //Debug.Log("ContextMenuPanel.Inspect()");

            inspectCharacterService.SetTargetUnitController(systemGameManager.ContextMenuService.TargetUnitController);
            uIManager.contextMenuWindow.CloseWindow();
        }

        public void Trade() {
            Debug.Log("ContextMenuPanel.Trade()");
            uIManager.contextMenuWindow.CloseWindow();
        }

        public void Message() {
            Debug.Log("ContextMenuPanel.Message()");
            uIManager.contextMenuWindow.CloseWindow();
        }

        public void Invite() {
            Debug.Log("ContextMenuPanel.Invite()");

            characterGroupServiceClient.RequestInviteCharacterToGroup(contextMenuService.TargetUnitController.CharacterId);
            uIManager.contextMenuWindow.CloseWindow();
        }

        public void Kick() {
            Debug.Log("ContextMenuPanel.Kick()");

            characterGroupServiceClient.RequestRemoveCharacterFromGroup(contextMenuService.TargetUnitController.CharacterId);
            uIManager.contextMenuWindow.CloseWindow();
        }

        public void Promote() {
            Debug.Log("ContextMenuPanel.Promote()");

            characterGroupServiceClient.RequestPromoteCharacterToLeader(contextMenuService.TargetUnitController.CharacterId);
            uIManager.contextMenuWindow.CloseWindow();
        }

        public void Disband() {
            Debug.Log("ContextMenuPanel.Disband()");

            characterGroupServiceClient.RequestDisbandGroup();
            uIManager.contextMenuWindow.CloseWindow();
        }

        public void Leave() {
            Debug.Log("ContextMenuPanel.Leave()");

            characterGroupServiceClient.RequestLeaveGroup();
            uIManager.contextMenuWindow.CloseWindow();
        }

    }

}