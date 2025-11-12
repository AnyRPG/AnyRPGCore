using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterGroupServiceClient : ConfiguredClass {

        public event Action OnJoinGroup = delegate { };
        public event Action OnLeaveGroup = delegate { };
        public event Action OnAddMember = delegate { };
        public event Action OnRemoveMember = delegate { };
        public event Action OnDisbandGroup = delegate { };

        private int inviteGroupId = 0;
        private string inviteLeaderName = string.Empty;

        private CharacterGroup currentCharacterGroup = null;

        // game manager references
        private UIManager uIManager = null;
        private CharacterManager characterManager = null;
        private PlayerManager playerManager = null;
        private LogManager logManager = null;
        private SystemEventManager systemEventManager = null;

        public CharacterGroup CurrentCharacterGroup { get => currentCharacterGroup; }
        public string InviteLeaderName { get => inviteLeaderName; }
        public int InviteGroupId { get => inviteGroupId; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            networkManagerClient.OnClientConnectionStopped += HandleClientConnectionStopped;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            characterManager = systemGameManager.CharacterManager;
            playerManager = systemGameManager.PlayerManager;
            logManager = systemGameManager.LogManager;
            systemEventManager = systemGameManager.SystemEventManager;
        }

        private void HandleClientConnectionStopped() {
            inviteGroupId = 0;
            inviteLeaderName = string.Empty;
            currentCharacterGroup = null;
        }

        public void DisplayCharacterGroupInvite(int characterGroupId, string leaderName) {
            Debug.Log($"CharacterGroupServiceClient.DisplayCharacterGroupInvite({characterGroupId}, {leaderName})");

            inviteGroupId = characterGroupId;
            inviteLeaderName = leaderName;
            uIManager.confirmJoinGroupWindow.OpenWindow();
            logManager.WriteSystemMessage($"You have been invited to a group by {leaderName}.");
        }

        public void AcceptCharacterGroupInvite() {
            Debug.Log($"CharacterGroupServiceClient.AcceptCharacterGroupInvite()");

            networkManagerClient.AcceptCharacterGroupInvite(inviteGroupId);
            inviteGroupId = 0;
            inviteLeaderName = string.Empty;
            uIManager.confirmJoinGroupWindow.CloseWindow();
        }

        public void ProcessJoinGroup(int characterId, CharacterGroup characterGroup) {
            Debug.Log($"CharacterGroupServiceClient.ProcessJoinGroup({characterId})");

            if (characterId == playerManager.UnitController.CharacterId) {
                currentCharacterGroup = characterGroup;
                logManager.WriteSystemMessage("You have joined a group.");
                OnJoinGroup();
                return;
            }

            AddCharacterToGroup(characterId, characterGroup.characterGroupId);
        }

        public void ProcessLoadGroup(CharacterGroup characterGroup) {
            Debug.Log($"CharacterGroupServiceClient.ProcessLoadGroup({characterGroup.characterGroupId})");

            currentCharacterGroup = characterGroup;
        }

        public void RequestLeaveGroup() {
            Debug.Log($"CharacterGroupServiceClient.RequestLeaveGroup()");

            if (currentCharacterGroup != null) {
                networkManagerClient.RequestLeaveCharacterGroup();
            }
        }

        public void ProcessLeaveGroup() {
            Debug.Log($"CharacterGroupServiceClient.ProcessLeaveGroup()");

            currentCharacterGroup = null;
            //uIManager.GroupUnitFramesWindow.CloseWindow();

            OnLeaveGroup();
            logManager.WriteSystemMessage("You have left the group.");
        }

        public void RequestInviteCharacterToGroup(int characterId) {
            Debug.Log($"CharacterGroupServiceClient.RequestInviteCharacterToGroup({characterId})");

            networkManagerClient.RequestInviteCharacterToGroup(characterId);
        }

        public void AddCharacterToGroup(int characterId, int characterGroupId) {
            Debug.Log($"CharacterGroupServiceClient.AddCharacterToGroup({characterId}, {characterGroupId})");

            if (currentCharacterGroup == null || currentCharacterGroup.characterGroupId != characterGroupId) {
                //Debug.Log("CharacterGroupService.AddCharacterToGroup: character group not found");
                return;
            }
            currentCharacterGroup.AddPlayer(characterId);
            OnAddMember();
            logManager.WriteSystemMessage($"{characterManager.GetCharacterName(characterId)} has joined the group.");
        }

        public void RequestRemoveCharacterFromGroup(int characterId) {
            Debug.Log($"CharacterGroupServiceClient.RequestRemoveCharacterFromGroup({characterId})");
            if (currentCharacterGroup != null) {
                networkManagerClient.RequestRemoveCharacterFromGroup(characterId);
            }
        }

        public void RemoveCharacterFromGroup(int removedCharacterId, int characterGroupId) {
            Debug.Log($"CharacterGroupServiceClient.RemoveCharacterFromGroup({removedCharacterId}, {characterGroupId})");

            if (currentCharacterGroup == null || currentCharacterGroup.characterGroupId != characterGroupId) {
                Debug.LogWarning("CharacterGroupService.RemoveCharacterFromGroup: character group not found");
                return;
            }
            if (removedCharacterId == playerManager.UnitController.CharacterId) {
                ProcessLeaveGroup();
                return;
            }
            currentCharacterGroup.RemovePlayer(removedCharacterId);
            OnRemoveMember();
            logManager.WriteSystemMessage($"{characterManager.GetCharacterName(removedCharacterId)} has left the group.");
        }

        public List<UnitController> GetCurrentGroupMemberUnitControllers() {
            Debug.Log($"CharacterGroupServiceClient.GetCurrentGroupMemberUnitControllers()");

            List<UnitController> returnList = new List<UnitController>();
            if (currentCharacterGroup != null) {
                foreach (int characterId in currentCharacterGroup.CharacterIdList[UnitControllerMode.Player]) {
                    UnitController unitController = characterManager.GetUnitController(UnitControllerMode.Player, characterId);
                    if (unitController != null && unitController != playerManager.UnitController) {
                        Debug.Log($"CharacterGroupServiceClient.GetCurrentGroupMemberUnitControllers(): adding {unitController.gameObject.name}");
                        returnList.Add(unitController);
                    }
                }
            }
            return returnList;
        }

        public void RequestDisbandGroup() {
            Debug.Log($"CharacterGroupServiceClient.RequestDisbandGroup()");

            networkManagerClient.RequestDisbandCharacterGroup(currentCharacterGroup.characterGroupId);
        }

        public void ProcessDisbandGroup(int characterGroupId) {
            Debug.Log($"CharacterGroupServiceClient.ProcessDisbandGroup({characterGroupId})");

            if (currentCharacterGroup == null || currentCharacterGroup.characterGroupId != characterGroupId) {
                //Debug.Log("CharacterGroupService.ProcessDisbandGroup: character group not found");
                return;
            }

            currentCharacterGroup = null;
            OnDisbandGroup();
            logManager.WriteSystemMessage("Your group has been disbanded.");
        }
    }

}