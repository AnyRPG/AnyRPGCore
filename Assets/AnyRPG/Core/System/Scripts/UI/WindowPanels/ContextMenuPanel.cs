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
        private PlayerManager playerManager = null;
        private TradeServiceClient tradeServiceClient = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            characterGroupServiceClient = systemGameManager.CharacterGroupServiceClient;
            contextMenuService = systemGameManager.ContextMenuService;
            inspectCharacterService = systemGameManager.InspectCharacterService;
            playerManager = systemGameManager.PlayerManager;
            tradeServiceClient = systemGameManager.TradeServiceClient;
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("ContextMenuPanel.ProcessOpenWindowNotification()");

            base.ProcessOpenWindowNotification();
            if (contextMenuService.TargetUnitController != null) {
                SetupButtons();
            } else {
                contextMenuService.CloseContextMenu();
            }
        }

        private void SetupButtons() {

            SetupInspectButton();
            SetupInviteButton();
            SetupKickButton();
            SetupDisbandButton();
            SetupLeaveButton();
            SetupPromoteButton();
            SetupTradeButton();
            SetupMessageButton();

            uINavigationController.UpdateNavigationList();

            if (uINavigationController.ActiveNavigableButtonCount == 0) {
                contextMenuService.CloseContextMenu();
            }
        }

        private void SetupInspectButton() {
            // inspect button
            if (contextMenuService.TargetUnitController == playerManager.UnitController) {
                // inspect button should not be available if the target is the player
                inspectButton.gameObject.SetActive(false);
                return;
            }
            
            inspectButton.gameObject.SetActive(true);
        }

        private void SetupTradeButton() {
            if (contextMenuService.TargetUnitController == playerManager.UnitController) {
                // cannot trade with ourselves
                //Debug.Log("ContextMenuPanel.SetupMessageButton() target is player, disabling invite button");
                tradeButton.gameObject.SetActive(false);
                return;
            }

            if (systemGameManager.GameMode == GameMode.Local) {
                // can only trade in network mode
                //Debug.Log("ContextMenuPanel.SetupMessageButton() game mode is local, disabling invite button");
                tradeButton.gameObject.SetActive(false);
                return;
            }

            if (contextMenuService.TargetUnitController.UnitControllerMode != UnitControllerMode.Player) {
                // can only trade players
                //Debug.Log("ContextMenuPanel.SetupMessageButton() target is not player, disabling invite button");
                tradeButton.gameObject.SetActive(false);
                return;
            }

            if (Faction.RelationWith(contextMenuService.TargetUnitController, playerManager.UnitController) < 0) {
                // can only trade neutral or better
                //Debug.Log("ContextMenuPanel.SetupMessageButton() target faction relationship is negative, disabling invite button");
                tradeButton.gameObject.SetActive(false);
                return;
            }

            // all checks passed.  this character can be traded with
            tradeButton.gameObject.SetActive(true);
        }

        private void SetupMessageButton() {
            if (contextMenuService.TargetUnitController == playerManager.UnitController) {
                // cannot message ourselves
                //Debug.Log("ContextMenuPanel.SetupMessageButton() target is player, disabling invite button");
                messageButton.gameObject.SetActive(false);
                return;
            }

            if (systemGameManager.GameMode == GameMode.Local) {
                // can only message in network mode
                //Debug.Log("ContextMenuPanel.SetupMessageButton() game mode is local, disabling invite button");
                messageButton.gameObject.SetActive(false);
                return;
            }

            if (contextMenuService.TargetUnitController.UnitControllerMode != UnitControllerMode.Player) {
                // can only message players
                //Debug.Log("ContextMenuPanel.SetupMessageButton() target is not player, disabling invite button");
                messageButton.gameObject.SetActive(false);
                return;
            }

            if (Faction.RelationWith(contextMenuService.TargetUnitController, playerManager.UnitController) < 0) {
                // can only message neutral or better
                //Debug.Log("ContextMenuPanel.SetupMessageButton() target faction relationship is negative, disabling invite button");
                messageButton.gameObject.SetActive(false);
                return;
            }

            if (systemConfigurationManager.PrivateMessageChatCommand == string.Empty) {
                // system must have a message command to use
                messageButton.gameObject.SetActive(false);
                return;
            }

            // all checks passed.  this character can be messaged
            messageButton.gameObject.SetActive(true);
        }

        private void SetupInviteButton() {
            if (contextMenuService.TargetUnitController == playerManager.UnitController) {
                // invite button should not be available if the target is the player
                //Debug.Log("ContextMenuPanel.SetupInviteButton() target is player, disabling invite button");
                inviteButton.gameObject.SetActive(false);
                return;
            }

            if (systemGameManager.GameMode == GameMode.Local) {
                // can only invite in network mode
                //Debug.Log("ContextMenuPanel.SetupInviteButton() game mode is local, disabling invite button");
                inviteButton.gameObject.SetActive(false);
                return;
            }

            if (contextMenuService.TargetUnitController.UnitControllerMode != UnitControllerMode.Player) {
                // can only invite players
                //Debug.Log("ContextMenuPanel.SetupInviteButton() target is not player, disabling invite button");
                inviteButton.gameObject.SetActive(false);
                return;
            }

            if (Faction.RelationWith(contextMenuService.TargetUnitController, playerManager.UnitController) < 0) {
                // can only invite neutral or better
                //Debug.Log("ContextMenuPanel.SetupInviteButton() target faction relationship is negative, disabling invite button");
                inviteButton.gameObject.SetActive(false);
                return;
            }

            if (contextMenuService.TargetUnitController.CharacterGroupManager.IsInGroup()) {
                // target is already in a group
                //Debug.Log("ContextMenuPanel.SetupInviteButton() target is already in a group, disabling invite button");
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
            if (characterGroup.MemberList[UnitControllerMode.Player].ContainsKey(contextMenuService.TargetUnitController.CharacterId) == false) {
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
            if (characterGroup.MemberList[UnitControllerMode.Player].ContainsKey(contextMenuService.TargetUnitController.CharacterId) == false) {
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
            contextMenuService.CloseContextMenu();
        }

        public void Trade() {
            //Debug.Log("ContextMenuPanel.Trade()");

            tradeServiceClient.RequestBeginTrade(contextMenuService.TargetUnitController.CharacterId);
            contextMenuService.CloseContextMenu();
        }

        public void Message() {
            //Debug.Log("ContextMenuPanel.Message()");

            contextMenuService.BeginPrivateMessage();
            contextMenuService.CloseContextMenu();
        }

        public void Invite() {
            //Debug.Log("ContextMenuPanel.Invite()");

            characterGroupServiceClient.RequestInviteCharacterToGroup(contextMenuService.TargetUnitController.CharacterId);
            contextMenuService.CloseContextMenu();
        }

        public void Kick() {
            //Debug.Log("ContextMenuPanel.Kick()");

            characterGroupServiceClient.RequestRemoveCharacterFromGroup(contextMenuService.TargetUnitController.CharacterId);
            contextMenuService.CloseContextMenu();
        }

        public void Promote() {
            //Debug.Log("ContextMenuPanel.Promote()");

            characterGroupServiceClient.RequestPromoteCharacterToLeader(contextMenuService.TargetUnitController.CharacterId);
            contextMenuService.CloseContextMenu();
        }

        public void Disband() {
            //Debug.Log("ContextMenuPanel.Disband()");

            characterGroupServiceClient.RequestDisbandGroup();
            contextMenuService.CloseContextMenu();
        }

        public void Leave() {
            //Debug.Log("ContextMenuPanel.Leave()");

            characterGroupServiceClient.RequestLeaveGroup();
            contextMenuService.CloseContextMenu();
        }

    }

}