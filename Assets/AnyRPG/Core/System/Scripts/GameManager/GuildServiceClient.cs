using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace AnyRPG {
    public class GuildServiceClient : ConfiguredClass {

        public event Action OnJoinGuild = delegate { };
        public event Action OnLeaveGuild = delegate { };
        public event Action OnAddMember = delegate { };
        public event Action OnRemoveMember = delegate { };
        public event Action OnDisbandGuild = delegate { };
        public event Action OnPromoteGuildLeader = delegate { };
        public event Action OnRenameCharacterInGuild = delegate { };
        public event Action OnGuildNameAvailable = delegate { };
        public event Action OnGuildMemberStatusChange = delegate { };

        private int inviteGuildId = 0;
        private string inviteLeaderName = string.Empty;

        private Guild currentGuild = null;

        // game manager references
        private UIManager uIManager = null;
        private CharacterManager characterManager = null;
        private PlayerManager playerManager = null;
        private MessageLogClient messageLogClient = null;

        public Guild CurrentGuild { get => currentGuild; }
        public string InviteLeaderName { get => inviteLeaderName; }
        public int InviteGuildId { get => inviteGuildId; }

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
            inviteGuildId = 0;
            inviteLeaderName = string.Empty;
            currentGuild = null;
        }

        public void DisplayGuildInvite(int guildId, string leaderName) {
            //Debug.Log($"GuildServiceClient.DisplayGuildInvite({guildId}, {leaderName})");

            inviteGuildId = guildId;
            inviteLeaderName = leaderName;
            uIManager.confirmJoinGuildWindow.OpenWindow();
            messageLogClient.WriteSystemMessage($"You have been invited to a guild by {leaderName}.");
        }

        public void AcceptGuildInvite() {
            //Debug.Log($"GuildServiceClient.AcceptGuildInvite()");

            networkManagerClient.AcceptGuildInvite(inviteGuildId);
            inviteGuildId = 0;
            inviteLeaderName = string.Empty;
            uIManager.confirmJoinGuildWindow.CloseWindow();
        }

        public void ProcessJoinGuild(int guildId, GuildMemberNetworkData guildMemberNetworkData) {
            //Debug.Log($"GuildServiceClient.ProcessJoinGuild(guildId: {guildId})");

            if (currentGuild == null || currentGuild.GuildId != guildId) {
                Debug.LogWarning($"GuildService.ProcessJoinGuild({guildId}) guildId provided does not match current guildId");
                return;
            }

            if (guildMemberNetworkData.CharacterSummaryNetworkData.CharacterId == playerManager.UnitController.CharacterId) {
                messageLogClient.WriteSystemMessage("You have joined a guild.");
                OnJoinGuild();
                return;
            }

            AddCharacterToGuild(new GuildMemberData(guildMemberNetworkData, systemDataFactory), guildId);
        }

        public void ProcessLoadGuild(GuildNetworkData guildNetworkData) {
            //Debug.Log($"GuildServiceClient.ProcessLoadGuild(guildId: {guildNetworkData.GuildId}, guildName: {guildNetworkData.GuildName})");

            currentGuild = new Guild(guildNetworkData, systemDataFactory);
        }

        public void RequestLeaveGuild() {
            //Debug.Log($"GuildServiceClient.RequestLeaveGuild()");

            if (currentGuild != null) {
                networkManagerClient.RequestLeaveGuild();
            }
        }

        public void ProcessLeaveGuild() {
            //Debug.Log($"GuildServiceClient.ProcessLeaveGuild()");

            currentGuild = null;
            //uIManager.GuildUnitFramesWindow.CloseWindow();

            OnLeaveGuild();
            messageLogClient.WriteSystemMessage("You have left the guild.");
        }

        public void RequestInviteCharacterToGuild(int characterId) {
            //Debug.Log($"GuildServiceClient.RequestInviteCharacterToGuild({characterId})");

            networkManagerClient.RequestInviteCharacterToGuild(characterId);
        }

        public void RequestInviteCharacterToGuild(string characterName) {
            //Debug.Log($"GuildServiceClient.RequestInviteCharacterToGuild({characterId})");

            networkManagerClient.RequestInviteCharacterToGuild(characterName);
        }

        public void AddCharacterToGuild(GuildMemberData guildMemberData, int guildId) {
            //Debug.Log($"GuildServiceClient.AddCharacterToGuild({guildMemberData.CharacterSummaryData.IsOnline}, {guildId})");

            if (currentGuild == null || currentGuild.GuildId != guildId) {
                //Debug.Log("GuildService.AddCharacterToGuild: character guild not found");
                return;
            }
            currentGuild.AddPlayer(guildMemberData);
            OnAddMember();
            messageLogClient.WriteSystemMessage($"{characterManager.GetCharacterName(guildMemberData.CharacterSummaryData.CharacterId)} has joined the guild.");
        }

        public void RequestRemoveCharacterFromGuild(int characterId) {
            //Debug.Log($"GuildServiceClient.RequestRemoveCharacterFromGuild({characterId})");

            if (currentGuild != null) {
                networkManagerClient.RequestRemoveCharacterFromGuild(characterId);
            }
        }

        public void RemoveCharacterFromGuild(int removedCharacterId, int guildId) {
            //Debug.Log($"GuildServiceClient.RemoveCharacterFromGuild({removedCharacterId}, {guildId})");

            if (currentGuild == null || currentGuild.GuildId != guildId) {
                Debug.LogWarning("GuildService.RemoveCharacterFromGuild: character guild not found");
                return;
            }
            if (removedCharacterId == playerManager.UnitController.CharacterId) {
                ProcessLeaveGuild();
                return;
            }
            currentGuild.RemovePlayer(removedCharacterId);
            OnRemoveMember();
            messageLogClient.WriteSystemMessage($"{characterManager.GetCharacterName(removedCharacterId)} has left the guild.");
        }

        public Dictionary<int, UnitController> GetCurrentGuildMemberUnitControllers() {
            //Debug.Log($"GuildServiceClient.GetCurrentGuildMemberUnitControllers()");

            Dictionary<int, UnitController> returnList = new Dictionary<int, UnitController>();
            if (currentGuild != null) {
                foreach (int characterId in currentGuild.MemberList.Keys) {
                    UnitController unitController = characterManager.GetUnitController(UnitControllerMode.Player, characterId);
                    if (unitController != null && unitController != playerManager.UnitController) {
                        //Debug.Log($"GuildServiceClient.GetCurrentGuildMemberUnitControllers(): adding {unitController.gameObject.name}");
                        returnList.Add(characterId, unitController);
                    } else if (unitController == null) {
                        returnList.Add(characterId, null);
                    }
                }
            }
            return returnList;
        }

        public void RequestDisbandGuild() {
            //Debug.Log($"GuildServiceClient.RequestDisbandGuild()");

            networkManagerClient.RequestDisbandGuild(currentGuild.GuildId);
        }

        public void ProcessDisbandGuild(int guildId) {
            //Debug.Log($"GuildServiceClient.ProcessDisbandGuild({guildId})");

            if (currentGuild == null || currentGuild.GuildId != guildId) {
                //Debug.Log("GuildService.ProcessDisbandGuild: character guild not found");
                return;
            }

            currentGuild = null;
            OnDisbandGuild();
            messageLogClient.WriteSystemMessage("Your guild has been disbanded.");
        }

        public void ProcessPromoteGuildLeader(int guildId, int newLeaderCharacterId) {
            if (currentGuild == null || currentGuild.GuildId != guildId) {
                //Debug.Log("GuildService.ProcessPromoteGuildLeader: character guild not found");
                return;
            }

            currentGuild.LeaderPlayerCharacterId = newLeaderCharacterId;

            /*
            if (newLeaderCharacterId == playerManager.UnitController.CharacterId) {
                messageLogClient.WriteSystemMessage("You are now the guild leader.");
            } else {
                messageLogClient.WriteSystemMessage($"{characterManager.GetCharacterName(newLeaderCharacterId)} is now the guild leader.");
            }
            */
            OnPromoteGuildLeader();
        }

        public void RequestPromoteCharacter(int characterId) {
            networkManagerClient.RequestPromoteGuildCharacter(characterId);
        }

        public void RequestDemoteCharacter(int characterId) {
            networkManagerClient.RequestDemoteGuildCharacter(characterId);
        }

        public void ProcessRenameCharacterInGuild(int guildId, int characterId, string newName) {
            if (currentGuild == null || currentGuild.GuildId != guildId) {
                //Debug.Log("GuildService.ProcessRenameCharacterInGuild: character guild not found");
                return;
            }
            if (currentGuild.MemberList.ContainsKey(characterId)) {
                currentGuild.MemberList[characterId].CharacterSummaryData.CharacterName = newName;
            }
            OnRenameCharacterInGuild();
        }

        public void ProcessGuildNameAvailable() {
            OnGuildNameAvailable();
        }

        public void ProcessGuildMemberStatusChange(int guildId, int playerCharacterId, GuildMemberNetworkData guildMemberNetworkData) {
            if (currentGuild == null || currentGuild.GuildId != guildId) {
                //Debug.Log("GuildService.ProcessGuildMemberOnline: guild not found");
                return;
            }
            if (currentGuild.MemberList.ContainsKey(playerCharacterId)) {
                currentGuild.MemberList[playerCharacterId] = new GuildMemberData(guildMemberNetworkData, systemDataFactory);
                if (currentGuild.MemberList[playerCharacterId].Rank == GuildRank.Leader) {
                    currentGuild.LeaderPlayerCharacterId = playerCharacterId;
                }
            }
            OnGuildMemberStatusChange();
        }

        public void AdvertiseGuildMessage(int guildId, string messageText) {
            if (currentGuild == null || currentGuild.GuildId != guildId) {
                //Debug.Log("GuildService.ProcessGuildMemberOnline: guild not found");
                return;
            }
            messageLogClient.WriteGuildMessage(messageText);
        }
    }

}