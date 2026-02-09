using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace AnyRPG {
    public class CharacterGroupServiceClient : ConfiguredClass {

        public event Action OnJoinGroup = delegate { };
        public event Action OnLeaveGroup = delegate { };
        public event Action OnAddMember = delegate { };
        public event Action OnRemoveMember = delegate { };
        public event Action OnDisbandGroup = delegate { };
        public event Action OnPromoteGroupLeader = delegate { };
        public event Action OnRenameCharacterInGroup = delegate { };
        public event Action OnCharacterGroupMemberStatusChange = delegate { };

        private int inviteGroupId = 0;
        private string inviteLeaderName = string.Empty;

        private CharacterGroup currentCharacterGroup = null;

        // game manager references
        private UIManager uIManager = null;
        private CharacterManager characterManager = null;
        private PlayerManager playerManager = null;
        private MessageLogClient messageLogClient = null;

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
            messageLogClient = systemGameManager.MessageLogClient;
        }

        private void HandleClientConnectionStopped() {
            inviteGroupId = 0;
            inviteLeaderName = string.Empty;
            currentCharacterGroup = null;
        }

        public void DisplayCharacterGroupInvite(int characterGroupId, string leaderName) {
            //Debug.Log($"CharacterGroupServiceClient.DisplayCharacterGroupInvite({characterGroupId}, {leaderName})");

            inviteGroupId = characterGroupId;
            inviteLeaderName = leaderName;
            uIManager.confirmJoinGroupWindow.OpenWindow();
            messageLogClient.WriteSystemMessage($"You have been invited to a group by {leaderName}.");
        }

        public void AcceptCharacterGroupInvite() {
            //Debug.Log($"CharacterGroupServiceClient.AcceptCharacterGroupInvite()");

            networkManagerClient.AcceptCharacterGroupInvite(inviteGroupId);
            inviteGroupId = 0;
            inviteLeaderName = string.Empty;
            uIManager.confirmJoinGroupWindow.CloseWindow();
        }

        public void ProcessJoinGroup(int characterGroupId, CharacterGroupMemberNetworkData characterGroupMemberNetworkData) {
            //Debug.Log($"CharacterGroupServiceClient.ProcessJoinGroup({characterId})");

            if (currentCharacterGroup != null
                && currentCharacterGroup.characterGroupId == characterGroupId
                && characterGroupMemberNetworkData.CharacterSummaryNetworkData.CharacterId == playerManager.UnitController.CharacterId) {
                messageLogClient.WriteSystemMessage("You have joined a group.");
                OnJoinGroup();
                return;
            }

            AddCharacterToGroup(characterGroupId, characterGroupMemberNetworkData);
        }

        public void ProcessLoadGroup(CharacterGroupNetworkData characterGroupNetworkData) {
            //Debug.Log($"CharacterGroupServiceClient.ProcessLoadGroup({characterGroup.characterGroupId})");

            currentCharacterGroup = new CharacterGroup(characterGroupNetworkData, systemDataFactory);
        }

        public void RequestLeaveGroup() {
            //Debug.Log($"CharacterGroupServiceClient.RequestLeaveGroup()");

            if (currentCharacterGroup != null) {
                networkManagerClient.RequestLeaveCharacterGroup();
            }
        }

        public void ProcessLeaveGroup() {
            //Debug.Log($"CharacterGroupServiceClient.ProcessLeaveGroup()");

            currentCharacterGroup = null;
            //uIManager.GroupUnitFramesWindow.CloseWindow();

            OnLeaveGroup();
            messageLogClient.WriteSystemMessage("You have left the group.");
        }

        public void RequestInviteCharacterToGroup(int characterId) {
            //Debug.Log($"CharacterGroupServiceClient.RequestInviteCharacterToGroup({characterId})");

            networkManagerClient.RequestInviteCharacterToGroup(characterId);
        }

        public void RequestInviteCharacterToGroup(string characterName) {
            //Debug.Log($"CharacterGroupServiceClient.RequestInviteCharacterToGroup({characterId})");

            networkManagerClient.RequestInviteCharacterToGroup(characterName);
        }

        public void AddCharacterToGroup(int characterGroupId, CharacterGroupMemberNetworkData characterGroupMemberNetworkData) {
            //Debug.Log($"CharacterGroupServiceClient.AddCharacterToGroup({characterId}, {characterGroupId})");

            if (currentCharacterGroup == null || currentCharacterGroup.characterGroupId != characterGroupId) {
                //Debug.Log("CharacterGroupService.AddCharacterToGroup: character group not found");
                return;
            }
            currentCharacterGroup.AddPlayer(new CharacterGroupMemberData(characterGroupMemberNetworkData, systemDataFactory));
            OnAddMember();
            messageLogClient.WriteSystemMessage($"{characterManager.GetCharacterName(characterGroupMemberNetworkData.CharacterSummaryNetworkData.CharacterId)} has joined the group.");
        }

        public void RequestRemoveCharacterFromGroup(int characterId) {
            //Debug.Log($"CharacterGroupServiceClient.RequestRemoveCharacterFromGroup({characterId})");

            if (currentCharacterGroup != null) {
                networkManagerClient.RequestRemoveCharacterFromGroup(characterId);
            }
        }

        public void RemoveCharacterFromGroup(int removedCharacterId, int characterGroupId) {
            //Debug.Log($"CharacterGroupServiceClient.RemoveCharacterFromGroup({removedCharacterId}, {characterGroupId})");

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
            messageLogClient.WriteSystemMessage($"{characterManager.GetCharacterName(removedCharacterId)} has left the group.");
        }

        public Dictionary<int, UnitController> GetCurrentGroupMemberUnitControllers() {
            //Debug.Log($"CharacterGroupServiceClient.GetCurrentGroupMemberUnitControllers()");

            Dictionary<int, UnitController> returnList = new Dictionary<int, UnitController>();
            if (currentCharacterGroup != null) {
                foreach (int characterId in currentCharacterGroup.MemberList[UnitControllerMode.Player].Keys) {
                    UnitController unitController = characterManager.GetUnitController(UnitControllerMode.Player, characterId);
                    if (unitController != null && unitController != playerManager.UnitController) {
                        //Debug.Log($"CharacterGroupServiceClient.GetCurrentGroupMemberUnitControllers(): adding {unitController.gameObject.name}");
                        returnList.Add(characterId, unitController);
                    } else if (unitController == null) {
                        returnList.Add(characterId, null);
                    }
                }
            }
            return returnList;
        }

        public void RequestDisbandGroup() {
            //Debug.Log($"CharacterGroupServiceClient.RequestDisbandGroup()");

            networkManagerClient.RequestDisbandCharacterGroup(currentCharacterGroup.characterGroupId);
        }

        public void ProcessDisbandGroup(int characterGroupId) {
            //Debug.Log($"CharacterGroupServiceClient.ProcessDisbandGroup({characterGroupId})");

            if (currentCharacterGroup == null || currentCharacterGroup.characterGroupId != characterGroupId) {
                //Debug.Log("CharacterGroupService.ProcessDisbandGroup: character group not found");
                return;
            }

            currentCharacterGroup = null;
            OnDisbandGroup();
            messageLogClient.WriteSystemMessage("Your group has been disbanded.");
        }

        public void ProcessPromoteGroupLeader(int characterGroupId, int newLeaderCharacterId) {
            //Debug.Log($"CharacterGroupService.ProcessPromoteGroupLeader(characterGroupId: {characterGroupId}, newLeaderCharacterId: {newLeaderCharacterId})");

            if (currentCharacterGroup == null || currentCharacterGroup.characterGroupId != characterGroupId) {
                //Debug.Log("CharacterGroupService.ProcessPromoteGroupLeader: character group not found");
                return;
            }

            currentCharacterGroup.leaderPlayerCharacterId = newLeaderCharacterId;

            /*
            if (newLeaderCharacterId == playerManager.UnitController.CharacterId) {
                messageLogClient.WriteSystemMessage("You are now the group leader.");
            } else {
                messageLogClient.WriteSystemMessage($"{characterManager.GetCharacterName(newLeaderCharacterId)} is now the group leader.");
            }
            */
            OnPromoteGroupLeader();
        }

        public void RequestPromoteCharacterToLeader(int characterId) {
            networkManagerClient.RequestPromoteCharacterToLeader(characterId);
        }

        public void ProcessRenameCharacterInGroup(int characterGroupId, int characterId, string newName) {
            if (currentCharacterGroup == null || currentCharacterGroup.characterGroupId != characterGroupId) {
                //Debug.Log("CharacterGroupService.ProcessRenameCharacterInGroup: character group not found");
                return;
            }
            if (currentCharacterGroup.MemberList[UnitControllerMode.Player].ContainsKey(characterId)) {
                currentCharacterGroup.MemberList[UnitControllerMode.Player][characterId].CharacterSummaryData.CharacterName = newName;
            }
            OnRenameCharacterInGroup();
        }

        public void ProcessCharacterGroupMemberStatusChange(int characterGroupId, int playerCharacterId, CharacterGroupMemberNetworkData characterGroupMemberNetworkData) {
            //Debug.Log($"CharacterGroupServiceClient.ProcessCharacterGroupMemberStatusChange(characterGroupId: {characterGroupId}, playerCharacterId: {playerCharacterId}, isOnline: {characterGroupMemberNetworkData.CharacterSummaryNetworkData.IsOnline})");

            if (currentCharacterGroup == null || currentCharacterGroup.characterGroupId != characterGroupId) {
                //Debug.Log("CharacterGroupService.ProcessCharacterGroupMemberOnline: character group not found");
                return;
            }
            if (currentCharacterGroup.MemberList[UnitControllerMode.Player].ContainsKey(playerCharacterId)) {
                CharacterGroupMemberData characterGroupMemberData = new CharacterGroupMemberData(characterGroupMemberNetworkData, systemDataFactory);
                bool promotingLeader = false;
                if (characterGroupMemberData.Rank == CharacterGroupRank.Leader && currentCharacterGroup.MemberList[UnitControllerMode.Player][playerCharacterId].Rank != CharacterGroupRank.Leader) {
                    currentCharacterGroup.leaderPlayerCharacterId = characterGroupMemberData.CharacterSummaryData.CharacterId;
                    promotingLeader = true;
                }
                currentCharacterGroup.MemberList[UnitControllerMode.Player][playerCharacterId] = characterGroupMemberData;
                if (promotingLeader == true) {
                    OnPromoteGroupLeader();
                } else {
                    OnCharacterGroupMemberStatusChange();
                }
            }
        }

        public void AdvertiseGroupMessage(int characterGroupId, string messageText) {
            if (currentCharacterGroup == null || currentCharacterGroup.characterGroupId != characterGroupId) {
                //Debug.Log("CharacterGroupService.AdvertiseGroupMessage: character group not found");
                return;
            }
            messageLogClient.WriteGroupMessage(messageText);
        }

        public CharacterGroupMemberData GetCharacterGroupMemberData(int characterId) {
            if (currentCharacterGroup != null && currentCharacterGroup.MemberList[UnitControllerMode.Player].ContainsKey(characterId)) {
                return currentCharacterGroup.MemberList[UnitControllerMode.Player][characterId];
            }
            return null;
        }

        public void RequestPromoteGroupCharacter(int characterId) {
            networkManagerClient.RequestDemoteGroupCharacter(characterId);
        }

        public void RequestDemoteGroupCharacter(int characterId) {
            networkManagerClient.RequestDemoteGroupCharacter(characterId);
        }
    }

}