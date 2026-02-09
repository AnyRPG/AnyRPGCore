using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace AnyRPG {
    public class GuildServiceServer : ConfiguredClass {

        /// <summary>
        /// characterId, guildId
        /// </summary>
        private Dictionary<int, int> guildInvites = new Dictionary<int, int>();

        /// <summary>
        /// guildId, Guild
        /// </summary>
        private Dictionary<int, Guild> guildDictionary = new Dictionary<int, Guild>();

        /// <summary>
        /// playerCharacterId, guildId
        /// </summary>
        private Dictionary<int, int> guildMemberLookup = new Dictionary<int, int>();

        // game manager references
        private PlayerManagerServer playerManagerServer = null;
        private CharacterManager characterManager = null;
        private MessageLogServer messageLogServer = null;
        private ServerDataService serverDataService = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            //networkManagerServer.OnStartServer += HandleStartServer;
            networkManagerServer.OnStopServer += HandleStopServer;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManagerServer = systemGameManager.PlayerManagerServer;
            characterManager = systemGameManager.CharacterManager;
            messageLogServer = systemGameManager.MessageLogServer;
            serverDataService = systemGameManager.ServerDataService;
        }

        private void HandleStopServer() {
            ClearGuilds();
        }

        public void LoadAllGuilds() {
            serverDataService.LoadAllGuilds();
        }

        public void ProcessLoadGuildListResponse(List<GuildSerializedData> guildSerializedDataList) {
            //Debug.Log($"NetworkManagerServer.ProcessLoadCharacterListResponse({accountId})");

            List<GuildSaveData> guildSaveDataList = new List<GuildSaveData>();
            foreach (GuildSerializedData guildSerializedData in guildSerializedDataList) {
                GuildSaveData guildSaveData = JsonUtility.FromJson<GuildSaveData>(guildSerializedData.saveData);
                if (guildSaveData == null) {
                    Debug.LogWarning($"GuildServiceServer.ProcessLoadGuildListResponse() invalid Guild. It will be skipped.");
                    continue;
                }
                guildSaveDataList.Add(guildSaveData);
            }
            ProcessLoadGuildListResponse(guildSaveDataList);
        }

        public void ProcessLoadGuildListResponse(List<GuildSaveData> guildSaveDataList) {
            //Debug.Log($"NetworkManagerServer.ProcessLoadCharacterListResponse({accountId})");

            foreach (GuildSaveData guildSaveData in guildSaveDataList) {
                if (guildSaveData.GuildId == -1) {
                    Debug.LogWarning($"UserAccountService.LoadAllGuilds(): Guild {guildSaveData.GuildName} has invalid id of 0.  This guild will be skipped.");
                    continue;
                }
                if (guildDictionary.ContainsKey(guildSaveData.GuildId)) {
                    Debug.LogWarning($"UserAccountService.LoadAllGuilds(): Duplicate guild id {guildSaveData.GuildId} found in guild {guildSaveData.GuildName}.  This guild will be skipped.");
                    continue;
                }
                Guild guild = new Guild(guildSaveData, playerCharacterService);

                //Debug.Log($"UserAccountService.LoadAllGuilds(): Loaded guild {guild.guildName} with id {guild.guildId} and {guild.MemberIdList.Count} members.");
                guildDictionary.Add(guild.GuildId, guild);
                foreach (int memberId in guild.MemberList.Keys) {
                    if (guildMemberLookup.ContainsKey(memberId)) {
                        Debug.LogWarning($"UserAccountService.LoadAllGuilds(): Duplicate guild member id {memberId} found in guild {guildSaveData.GuildName}.  This member will be skipped.");
                        continue;
                    }
                    guildMemberLookup.Add(memberId, guild.GuildId);
                }
            }
        }

        private void ClearGuilds() {
            guildDictionary.Clear();
            guildMemberLookup.Clear();
        }

        

        public int GetGuildIdFromCharacterId(int characterId) {
            if (guildMemberLookup.ContainsKey(characterId)) {
                return guildMemberLookup[characterId];
            }
            return -1;
        }

        public Guild GetGuildFromCharacterId(int characterId) {
            //Debug.Log($"GuildServiceServer.GetGuildFromCharacterId({characterId})");

            if (guildMemberLookup.ContainsKey(characterId) && guildDictionary.ContainsKey(guildMemberLookup[characterId])) {
                return guildDictionary[guildMemberLookup[characterId]];
            }
            return null;
        }

        public void CheckGuildName(UnitController sourceUnitController, string guildName) {
            //Debug.Log($"GuildServiceServer.CheckGuildName({leaderCharacterId})");
            int accountId = playerManagerServer.GetAccountIdFromUnitController(sourceUnitController);
            // check for duplicate guild names
            foreach (Guild existingGuild in guildDictionary.Values) {
                if (string.Equals(existingGuild.GuildName, guildName, StringComparison.OrdinalIgnoreCase)) {
                    networkManagerServer.AdvertiseConfirmationPopup(accountId, $"The guild name <{guildName}> is already taken");
                    return;
                }
            }
            networkManagerServer.AdvertiseGuildNameAvailable(accountId);
        }

        public void CreateGuild(int leaderCharacterId, string guildName) {
            //Debug.Log($"GuildServiceServer.CreateGuild({leaderCharacterId}, {guildName})");

            UnitController unitController = characterManager.GetUnitController(UnitControllerMode.Player, leaderCharacterId);
            if (unitController == null) {
                return;
            }

            int accountId = playerManagerServer.GetAccountIdFromUnitController(unitController);

            // check if the character is already in a guild
            if (guildMemberLookup.ContainsKey(leaderCharacterId)) {
                networkManagerServer.AdvertiseConfirmationPopup(accountId, $"You are already in a guild");
                return;
            }

            // check for duplicate guild names
            foreach (Guild existingGuild in guildDictionary.Values) {
                if (string.Equals(existingGuild.GuildName, guildName, StringComparison.OrdinalIgnoreCase)) {
                    networkManagerServer.AdvertiseConfirmationPopup(accountId, $"The guild name <{guildName}> is already taken");
                    return;
                }
            }

            // check to make sure guild creation fee exists
            if (systemConfigurationManager.CreateGuildCurrencyAmount > 0
                && unitController.CharacterCurrencyManager.GetBaseCurrencyValue(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency) < systemConfigurationManager.CreateGuildCurrencyAmount) {
                networkManagerServer.AdvertiseConfirmationPopup(accountId, $"You do not have enough money to create a guild");
                return;
            }

            // remove guild creation fee from inventory
            unitController.CharacterCurrencyManager.SpendCurrency(systemConfigurationManager.DefaultCurrencyGroup.BaseCurrency, systemConfigurationManager.CreateGuildCurrencyAmount);

            // create guild
            GuildMemberData leaderGuildMemberData = new GuildMemberData(playerCharacterService.GetSummaryData(leaderCharacterId), GuildRank.Leader);
            Guild guild = new Guild(guildName, leaderGuildMemberData);

            // get guild Id
            serverDataService.GetGuildId(guild);
        }

        public void ProcessGuildIdAssigned(Guild guild, int accountId, UnitController unitController) {

            // add guild to internal lookups
            guildDictionary.Add(guild.GuildId, guild);
            guildMemberLookup.Add(guild.LeaderPlayerCharacterId, guild.GuildId);

            // save guild to storage
            SaveGuild(guild);

            // send guild info to leader
            SendGuildInfo(accountId, guild.LeaderPlayerCharacterId);
            unitController?.CharacterGuildManager.SetGuildId(guild.GuildId, guild.GuildName);
            networkManagerServer.AdvertiseAddCharacterToGuild(accountId, guild.GuildId, new GuildMemberNetworkData(guild.MemberList[guild.LeaderPlayerCharacterId]));
        }

        public void ProcessGuildIdAssigned(Guild guild) {

            UnitController unitController = characterManager.GetUnitController(UnitControllerMode.Player, guild.LeaderPlayerCharacterId);
            if (unitController == null) {
                return;
            }

            int accountId = playerManagerServer.GetAccountIdFromUnitController(unitController);

            ProcessGuildIdAssigned(guild, accountId, unitController);
        }

        public void AddCharacterToGuild(int characterId, int guildId) {
            //Debug.Log($"GuildServiceServer.AddCharacterToGuild({characterId}, {guildId})");

            if (guildDictionary.ContainsKey(guildId) == false) {
                Debug.LogWarning("GuildServiceServer.AddCharacterToGuild: character guild not found");
                return;
            }

            UnitController unitController = characterManager.GetUnitController(UnitControllerMode.Player, characterId);
            if (unitController == null) {
                Debug.LogWarning($"GuildServiceServer.AddCharacterToGuild: unit controller not found for characterId {characterId}");
                return;
            }

            int newCharacterAccountId = playerManagerServer.GetAccountIdFromUnitController(unitController);

            Guild guild = guildDictionary[guildId];
            if (guild == null) {
                Debug.LogWarning($"GuildServiceServer.AddCharacterToGuild({characterId}, {guildId}) guild not found");
                return;
            }

            CharacterSummaryData characterSummaryData = playerCharacterService.GetSummaryData(characterId);
            if (characterSummaryData == null) {
                Debug.LogWarning($"GuildServiceServer.AddCharacterToGuild() CharacterSummaryData was not found");
                return;
            }

            GuildMemberData sourceCharacterGuildMemberData = new GuildMemberData(characterSummaryData);
            guild.AddPlayer(sourceCharacterGuildMemberData);
            SaveGuild(guild);
            guildMemberLookup.Add(characterId, guildId);
            unitController.CharacterGuildManager.SetGuildId(guild.GuildId, guild.GuildName);

            networkManagerServer.AdvertiseGuild(newCharacterAccountId, new GuildNetworkData(guild));

            // notify all members including the new member
            foreach (GuildMemberData guildMemberData in guild.MemberList.Values) {
                // check if existing member is logged in
                int existingAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(guildMemberData.CharacterSummaryData.CharacterId);
                if (existingAccountId == -1) {
                    continue;
                }
                // check if existing member is disconnected
                if (guildMemberData.CharacterSummaryData.IsOnline == false) {
                    continue;
                }
                networkManagerServer.AdvertiseAddCharacterToGuild(existingAccountId, guild.GuildId, new GuildMemberNetworkData(sourceCharacterGuildMemberData));
            }
        }

        private void SaveGuild(Guild guild) {
            serverDataService.SaveGuild(guild);
        }

        public void RemoveCharacterFromGuild(int characterId) {
            //Debug.Log($"GuildServiceServer.RemoveCharacterFromGuild({characterId})");

            if (guildMemberLookup.ContainsKey(characterId) == false) {
                //Debug.Log("GuildServiceServer.RemoveCharacterFromGuild: character guild member not found");
                return;
            }
            int guildId = guildMemberLookup[characterId];
            RemoveCharacterFromGuild(characterId, guildId);
        }

        public void RemoveCharacterFromGuild(int characterId, int guildId) {
            //Debug.Log($"GuildServiceServer.RemoveCharacterFromGuild({characterId}, {guildId})");

            if (guildDictionary.ContainsKey(guildId) == false) {
                Debug.LogWarning($"GuildServiceServer.RemoveCharacterFromGuild({characterId}, {guildId}) character guild not found");
                return;
            }

            Guild guild = guildDictionary[guildId];
            guild.RemovePlayer(characterId);
            SaveGuild(guild);
            guildMemberLookup.Remove(characterId);
            UnitController unitController = characterManager.GetUnitController(UnitControllerMode.Player, characterId);
            unitController?.CharacterGuildManager.LeaveGuild();
            foreach (GuildMemberData guildMemberData in guild.MemberList.Values) {
                // get account id from player id
                int memberAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(guildMemberData.CharacterSummaryData.CharacterId);
                if (memberAccountId == -1) {
                    continue;
                }
                // check if the player is disconnected
                if (guildMemberData.CharacterSummaryData.IsOnline == false) {
                    continue;
                }
                networkManagerServer.AdvertiseRemoveCharacterFromGuild(memberAccountId, characterId, guild.GuildId);
            }

            int removedAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(characterId);
            if (removedAccountId != -1) {
                networkManagerServer.AdvertiseRemoveCharacterFromGuild(removedAccountId, characterId, guild.GuildId);
            }

            // if there are no members left, disband the guild
            if (guild.MemberList.Count == 0) {
                DisbandGuild(guildId);
                return;
            }

            // if the leader left, promote a new leader
            if (guild.LeaderPlayerCharacterId == characterId) {
                PromoteCharacterToLeader(guildId, guild.MemberList.First().Key);
            }
        }

        public void DisbandGuild(int accountId, int guildId) {
            //Debug.Log($"GuildServiceServer.DisbandGuild({accountId}, {guildId})");

            if (guildDictionary.ContainsKey(guildId) == false) {
                return;
            }
            int leaderPlayerCharacterid = playerManagerServer.GetPlayerCharacterId(accountId);
            if (leaderPlayerCharacterid == -1) {
                Debug.LogWarning($"GuildServiceServer.DisbandGuild: player character not found for accountId {accountId}");
                return;
            }
            if (leaderPlayerCharacterid != guildDictionary[guildId].LeaderPlayerCharacterId) {
                Debug.LogWarning("GuildServiceServer.DisbandGuild: only the guild leader can disband the guild");
                return;
            }

            DisbandGuild(guildId);
        }

        public void DisbandGuild(int guildId) {

            Guild guild = guildDictionary[guildId];
            foreach (GuildMemberData guildMemberData in guild.MemberList.Values) {
                guildMemberLookup.Remove(guildMemberData.CharacterSummaryData.CharacterId);

                // get account id from player id
                int memberAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(guildMemberData.CharacterSummaryData.CharacterId);
                if (memberAccountId == -1) {
                    continue;
                }
                // check if the player is disconnected
                if (guildMemberData.CharacterSummaryData.IsOnline == false) {
                    continue;
                }
                networkManagerServer.AdvertiseDisbandGuild(memberAccountId, guildId);
                UnitController unitController = characterManager.GetUnitController(UnitControllerMode.Player, guildMemberData.CharacterSummaryData.CharacterId);
                unitController?.CharacterGuildManager.LeaveGuild();
            }

            guildDictionary.Remove(guildId);

            // remove invites
            List<int> keysToRemove = new List<int>();
            foreach (KeyValuePair<int, int> invite in guildInvites) {
                if (invite.Value == guildId) {
                    keysToRemove.Add(invite.Key);
                }
            }
            foreach (int key in keysToRemove) {
                guildInvites.Remove(key);
            }

            DeleteGuild(guildId);
        }

        private void DeleteGuild(int guildId) {
            serverDataService.DeleteGuild(guildId);
        }

        public void AcceptGuildInvite(int accountId, int guildId) {
            //Debug.Log($"GuildServiceServer.AcceptGuildInvite({accountId}, {guildId})");

            if (guildDictionary.ContainsKey(guildId) == false) {
                Debug.LogWarning($"GuildServiceServer.AcceptGuildInvite({accountId}, {guildId}) character guild not found");
                return;
            }
            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            if (playerCharacterId == -1) {
                Debug.LogWarning($"GuildServiceServer.AcceptGuildInvite({accountId}, {guildId}) player character not found for account");
                return;
            }
            if (guildInvites.ContainsKey(playerCharacterId) == false || guildInvites[playerCharacterId] != guildId) {
                Debug.LogWarning($"GuildServiceServer.AcceptGuildInvite({accountId}, {guildId}) character guild invite not found");
                return;
            }
            guildInvites.Remove(playerCharacterId);
            Guild guild = guildDictionary[guildId];
            
            // add the new member
            AddCharacterToGuild(playerCharacterId, guildId);
        }

        public void DeclineGuildInvite(int accountId) {
            //Debug.Log($"GuildServiceServer.DeclineGuildInvite({accountId})");

            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            if (playerCharacterId == -1) {
                Debug.LogWarning($"GuildServiceServer.DeclineGuildInvite({accountId}) player character not found for accountId");
                return;
            }
            if (guildInvites.ContainsKey(playerCharacterId) == true) {
                // check if the leader is the only person in the guild and silenty disband
                if (guildDictionary.ContainsKey(guildInvites[playerCharacterId])) {
                    Guild guild = guildDictionary[guildInvites[playerCharacterId]];
                    if (guild.MemberList.Count == 1 &&
                        guild.LeaderPlayerCharacterId == guild.MemberList.First().Key) {
                        guildDictionary.Remove(guild.GuildId);
                        guildMemberLookup.Remove(guild.LeaderPlayerCharacterId);
                        UnitController unitController = characterManager.GetUnitController(UnitControllerMode.Player, guild.LeaderPlayerCharacterId);
                        unitController?.CharacterGuildManager.LeaveGuild();
                    }
                    int leaderAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(guild.LeaderPlayerCharacterId);
                    networkManagerServer.AdvertiseDeclineGuildInvite(leaderAccountId, playerCharacterService.GetPlayerNameFromId(playerCharacterId));
                }
                guildInvites.Remove(playerCharacterId);
            }
        }

        public void RequestLeaveGuild(int accountId) {
            //Debug.Log($"GuildServiceServer.RequestLeaveGuild({accountId})");

            int playerCharacterId = playerManagerServer.GetPlayerCharacterId(accountId);
            if (playerCharacterId == -1) {
                //Debug.LogWarning($"GuildServiceServer.RequestLeaveGuild: player character not found for accountId {accountId}");
                return;
            }
            RemoveCharacterFromGuild(playerCharacterId);
        }

        public void RequestRemoveCharacterFromGuild(int leaderAccountId, string playerName) {
            //Debug.Log($"GuildServiceServer.RequestRemoveCharacterFromGuild({leaderAccountId}, {playerName})");

			int inviteCharacterId = playerCharacterService.GetPlayerIdFromName(playerName);
            if (inviteCharacterId != -1) {
                RequestRemoveCharacterFromGuild(leaderAccountId, inviteCharacterId);
            }
        }

        public void RequestRemoveCharacterFromGuild(int actingAccountId, int removedCharacterId) {
            int actingPlayerCharacterId = playerManagerServer.GetPlayerCharacterId(actingAccountId);
            if (actingPlayerCharacterId == -1) {
                //Debug.LogWarning($"GuildServiceServer.RequestInviteCharacterToGuild: player character not found for accountId {accountId}");
                return;
            }
            if (guildMemberLookup.ContainsKey(removedCharacterId) == false) {
                Debug.LogWarning("GuildServiceServer.RequestRemoveCharacterFromGuild: character guild member not found");
                return;
            }
            int guildId = guildMemberLookup[removedCharacterId];
            if (guildDictionary.ContainsKey(guildId) == false) {
                Debug.LogWarning("GuildServiceServer.RequestRemoveCharacterFromGuild: character guild not found");
                return;
            }
            Guild guild = guildDictionary[guildId];
            GuildMemberData actingGuildMemberData = guild.MemberList[actingPlayerCharacterId];
            GuildMemberData removedGuildMemberData = guild.MemberList[removedCharacterId];
            if (actingGuildMemberData.Rank == GuildRank.Member) {
                Debug.LogWarning("GuildServiceServer.RequestRemoveCharacterFromGuild: only the guild leader and officers can remove members from the guild");
                return;
            }
            if (actingGuildMemberData.Rank == GuildRank.Officer && removedGuildMemberData.Rank != GuildRank.Member) {
                Debug.LogWarning("GuildServiceServer.RequestRemoveCharacterFromGuild: officers can only remove members from the guild");
                return;
            }
            RemoveCharacterFromGuild(removedCharacterId);
        }

        public void RequestInviteCharacterToGuild(int actingAccountId, string playerName) {
            //Debug.Log($"GuildServiceServer.RequestInviteCharacterToGuild(actingAccountId: {actingAccountId}, playerName: {playerName})");

            int inviteCharacterId = playerCharacterService.GetPlayerIdFromName(playerName);
            if (inviteCharacterId != -1) {
                RequestInviteCharacterToGuild(actingAccountId, inviteCharacterId);
            }
        }

        public void RequestInviteCharacterToGuild(int actingAccountId, int invitedCharacterId) {
            //Debug.Log($"GuildServiceServer.RequestInviteCharacterToGuild(actingAccountId: {actingAccountId}, invitedCharacterId: {invitedCharacterId})");

            int actingPlayerCharacterId = playerManagerServer.GetPlayerCharacterId(actingAccountId);
            if (actingPlayerCharacterId == -1) {
                Debug.LogWarning($"GuildServiceServer.RequestInviteCharacterToGuild: player character not found for leader accountId {actingAccountId}");
                return;
            }

            if (actingPlayerCharacterId == invitedCharacterId) {
                return;
            }

            int guildId = guildMemberLookup[actingPlayerCharacterId];
            if (guildDictionary.ContainsKey(guildId) == false) {
                Debug.LogWarning($"GuildServiceServer.RequestInviteCharacterToGuild({actingAccountId}, {invitedCharacterId}) character guild ({guildId}) not found");
                return;
            }

            Guild guild = guildDictionary[guildId];
            GuildMemberData actingMemberData = guild.MemberList[actingPlayerCharacterId];
            if (actingMemberData == null) {
                return;
            }
            if (actingMemberData.Rank == GuildRank.Member) {
                Debug.LogWarning($"GuildServiceServer.RequestInviteCharacterToGuild({actingAccountId}, {invitedCharacterId}) only the guild leader and officers can invite members to the guild");
                return;
            }

            guildInvites[invitedCharacterId] = guildId;
            string leaderName = playerManagerServer.GetPlayerName(actingAccountId);
            if (leaderName == string.Empty) {
                leaderName = "Unknown";
            }
            networkManagerServer.AdvertiseGuildInvite(invitedCharacterId, guild.GuildId, leaderName);
        }

        public void SendGuildInfo(int accountId, int playerCharacterId) {
            //Debug.Log($"GuildServiceServer.SendGuildInfo(accountId: {accountId}, playerCharacterId: {playerCharacterId})");

            if (guildMemberLookup.ContainsKey(playerCharacterId) == false) {
                //Debug.Log($"GuildServiceServer.SendGuildInfo: character guild member not found for playerCharacterId {playerCharacterId}");
                return;
            }
            int guildId = guildMemberLookup[playerCharacterId];
            if (guildDictionary.ContainsKey(guildId) == false) {
                //Debug.Log("GuildServiceServer.RequestInviteCharacterToGuild: character guild not found");
                return;
            }

            Guild guild = guildDictionary[guildId];
            //Debug.Log($"GuildServiceServer.SendGuildInfo: advertising guild {guild.guildName} to accountId {accountId}");
            networkManagerServer.AdvertiseGuild(accountId, new GuildNetworkData(guild));
        }

        public void PromoteCharacter(int guildId, int actingCharacterId, int promotedCharacterId) {
            //Debug.Log($"GuildServiceServer.PromoteLeader({guildId}, {oldLeaderCharacterId}, {newLeaderCharacterId})");
            if (guildDictionary.ContainsKey(guildId) == false) {
                Debug.LogWarning("GuildServiceServer.PromoteLeader: character guild not found");
                return;
            }

            Guild guild = guildDictionary[guildId];
            if (guild.MemberList.ContainsKey(promotedCharacterId) == false) {
                Debug.LogWarning("GuildServiceServer.PromoteLeader: new leader must be a member of the guild");
                return;
            }

            GuildMemberData actingCharacterData = guild.MemberList[actingCharacterId];
            GuildMemberData promotedCharacterData = guild.MemberList[promotedCharacterId];

            if (actingCharacterData.Rank == GuildRank.Member) {
                Debug.LogWarning("GuildServiceServer.PromoteLeader: only officers and leaders can promote members");
                return;
            }

            if (actingCharacterData.Rank == GuildRank.Officer) {
                if (promotedCharacterData.Rank != GuildRank.Member) {
                    Debug.LogWarning("GuildServiceServer.PromoteLeader: officers can only promote members to officers");
                    return;
                }
                promotedCharacterData.Rank = GuildRank.Officer;
            }

            if (actingCharacterData.Rank == GuildRank.Leader) {
                if (promotedCharacterData.Rank == GuildRank.Member) {
                    promotedCharacterData.Rank = GuildRank.Officer;
                } else if (promotedCharacterData.Rank == GuildRank.Officer) {
                    promotedCharacterData.Rank = GuildRank.Leader;
                    actingCharacterData.Rank = GuildRank.Officer;
                    guild.LeaderPlayerCharacterId = promotedCharacterId;
                }
            }

            ProcessPromoteCharacter(guild, promotedCharacterData, actingCharacterData);

        }

        private void PromoteCharacterToLeader(int guildId, int promotedCharacterId) {
            //Debug.Log($"GuildServiceServer.PromoteLeader({guildId}, {oldLeaderCharacterId}, {newLeaderCharacterId})");
            if (guildDictionary.ContainsKey(guildId) == false) {
                Debug.LogWarning("GuildServiceServer.PromoteLeader: character guild not found");
                return;
            }

            Guild guild = guildDictionary[guildId];
            if (guild.MemberList.ContainsKey(promotedCharacterId) == false) {
                Debug.LogWarning("GuildServiceServer.PromoteLeader: new leader must be a member of the guild");
                return;
            }

            GuildMemberData promotedCharacterData = guild.MemberList[promotedCharacterId];
            promotedCharacterData.Rank = GuildRank.Leader;
            ProcessPromoteCharacter(guild, promotedCharacterData, null);
        }

        private void ProcessPromoteCharacter(Guild guild, GuildMemberData promotedCharacterData, GuildMemberData actingCharacterData) {
            SaveGuild(guild);

            string promotedCharacterName = playerCharacterService.GetPlayerNameFromId(promotedCharacterData.CharacterSummaryData.CharacterId);

            foreach (GuildMemberData guildMemberData in guild.MemberList.Values) {
                // get account id from player id
                int memberAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(guildMemberData.CharacterSummaryData.CharacterId);
                if (memberAccountId == -1) {
                    continue;
                }
                // check if the player is disconnected
                if (guildMemberData.CharacterSummaryData.IsOnline == false) {
                    continue;
                }
                networkManagerServer.AdvertiseGuildMemberStatusChange(memberAccountId, guild.GuildId, promotedCharacterData.CharacterSummaryData.CharacterId, new GuildMemberNetworkData(promotedCharacterData));
                if (actingCharacterData != null) {
                    networkManagerServer.AdvertiseGuildMemberStatusChange(memberAccountId, guild.GuildId, actingCharacterData.CharacterSummaryData.CharacterId, new GuildMemberNetworkData(actingCharacterData));
                }

                if (promotedCharacterData.CharacterSummaryData.CharacterId == guildMemberData.CharacterSummaryData.CharacterId) {
                    messageLogServer.WriteSystemMessage(memberAccountId, $"You have been promoted in the guild to {promotedCharacterData.Rank.ToString()}.");
                } else {
                    messageLogServer.WriteSystemMessage(memberAccountId, $"{promotedCharacterName} has been promoted in the guild to {promotedCharacterData.Rank.ToString()}.");
                }
            }
        }

        public void DemoteCharacter(int guildId, int actingCharacterId, int demotedCharacterId) {
            //Debug.Log($"GuildServiceServer.DemoteCharacter({guildId}, {oldLeaderCharacterId}, {newLeaderCharacterId})");
            if (guildDictionary.ContainsKey(guildId) == false) {
                Debug.LogWarning("GuildServiceServer.PromoteLeader: character guild not found");
                return;
            }

            Guild guild = guildDictionary[guildId];
            if (guild.MemberList.ContainsKey(demotedCharacterId) == false) {
                Debug.LogWarning("GuildServiceServer.PromoteLeader: new leader must be a member of the guild");
                return;
            }

            GuildMemberData actingCharacterData = guild.MemberList[actingCharacterId];
            GuildMemberData demotedCharacterData = guild.MemberList[demotedCharacterId];

            if (actingCharacterData.Rank != GuildRank.Leader) {
                Debug.LogWarning("GuildServiceServer.PromoteLeader: only leaders can demote officers");
                return;
            }

            if (demotedCharacterData.Rank != GuildRank.Officer) {
                Debug.LogWarning("GuildServiceServer.PromoteLeader: only officers can be demoted");
                return;
            }
            demotedCharacterData.Rank = GuildRank.Member;

            SaveGuild(guild);

            string demotedCharacterName = playerCharacterService.GetPlayerNameFromId(demotedCharacterId);

            foreach (GuildMemberData guildMemberData in guild.MemberList.Values) {
                // get account id from player id
                int memberAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(guildMemberData.CharacterSummaryData.CharacterId);
                if (memberAccountId == -1) {
                    continue;
                }
                // check if the player is disconnected
                if (guildMemberData.CharacterSummaryData.IsOnline == false) {
                    continue;
                }
                networkManagerServer.AdvertiseGuildMemberStatusChange(memberAccountId, guildId, demotedCharacterId, new GuildMemberNetworkData(demotedCharacterData));

                if (demotedCharacterId == guildMemberData.CharacterSummaryData.CharacterId) {
                    messageLogServer.WriteSystemMessage(memberAccountId, $"You have been demoted in the guild to {demotedCharacterData.Rank.ToString()}.");
                } else {
                    messageLogServer.WriteSystemMessage(memberAccountId, $"{demotedCharacterName} has been demoted in the guild to {demotedCharacterData.Rank.ToString()}.");
                }
            }

        }

        public void RequestPromoteCharacter(int leaderAccountId, string playerName) {
            int inviteCharacterId = playerCharacterService.GetPlayerIdFromName(playerName);
            if (inviteCharacterId != -1) {
                RequestPromoteCharacter(leaderAccountId, inviteCharacterId);
            }
        }

        public void RequestPromoteCharacter(int requestingAccountId, int newLeaderCharacterId) {
            
            int requestingPlayerCharacterId = playerManagerServer.GetPlayerCharacterId(requestingAccountId);
            if (requestingPlayerCharacterId == -1) {
                Debug.LogWarning($"GuildServiceServer.RequestPromoteCharacter() player character not found for leader accountId {requestingAccountId}");
                return;
            }

            if (guildMemberLookup.ContainsKey(requestingPlayerCharacterId) == false) {
                Debug.LogWarning("GuildServiceServer.RequestPromoteCharacter() character guild member not found");
                return;
            }

            int guildId = guildMemberLookup[requestingPlayerCharacterId];
            PromoteCharacter(guildId, requestingPlayerCharacterId, newLeaderCharacterId);
        }

        public void RequestDemoteCharacter(int requestingAccountId, string playerName) {
            int demoteCharacterId = playerCharacterService.GetPlayerIdFromName(playerName);
            if (demoteCharacterId != -1) {
                RequestDemoteCharacter(requestingAccountId, demoteCharacterId);
            }
        }

        public void RequestDemoteCharacter(int requestingAccountId, int demotedCharacterId) {

            int requestingPlayerCharacterId = playerManagerServer.GetPlayerCharacterId(requestingAccountId);
            if (requestingPlayerCharacterId == -1) {
                Debug.LogWarning($"GuildServiceServer.RequestDemoteCharacter: player character not found for leader accountId {requestingAccountId}");
                return;
            }

            if (guildMemberLookup.ContainsKey(requestingPlayerCharacterId) == false) {
                Debug.LogWarning("GuildServiceServer.RequestPromoteCharacter() character guild member not found");
                return;
            }

            int guildId = guildMemberLookup[requestingPlayerCharacterId];
            DemoteCharacter(guildId, requestingPlayerCharacterId, demotedCharacterId);
        }


        /*
        public void ProcessRenameCharacter(int characterId, string newName, int guildId) {
            //Debug.Log($"GuildServiceServer.ProcessRenameCharacter({characterId}, {newName}, {guildId})");
            if (guildDictionary.ContainsKey(guildId) == false) {
                Debug.LogWarning("GuildServiceServer.ProcessRenameCharacter: character guild not found");
                return;
            }
            Guild guild = guildDictionary[guildId];
            if (guild.MemberIdList.ContainsKey(characterId) == false) {
                Debug.LogWarning("GuildServiceServer.ProcessRenameCharacter: character guild member not found");
                return;
            }
            guild.MemberIdList[characterId].CharacterName = newName;
            SaveGuildFile(guild);

            foreach (CharacterSummaryData characterSummaryData in guild.MemberIdList.Values) {
                // get account id from player id
                int memberAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(characterSummaryData.CharacterId);
                if (memberAccountId == -1) {
                    continue;
                }
                // check if the player is disconnected
                if (characterSummaryData.IsOnline == false) {
                    continue;
                }
                networkManagerServer.AdvertiseRenameCharacterInGuild(memberAccountId, guildId, characterId, newName);
            }
        }
        */

        public void ProcessStatusChange(int playerCharacterId) {
            // check if character is in a guild
            if (guildMemberLookup.ContainsKey(playerCharacterId) == false) {
                return;
            }
            int guildId = guildMemberLookup[playerCharacterId];
            if (guildDictionary.ContainsKey(guildId) == false) {
                return;
            }
            Guild guild = guildDictionary[guildId];
            if (guild.MemberList.ContainsKey(playerCharacterId) == false) {
                return;
            }

			foreach (GuildMemberData guildMemberData in guild.MemberList.Values) {
                // get account id from player id
                int memberAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(guildMemberData.CharacterSummaryData.CharacterId);
                if (memberAccountId == -1) {
                    continue;
                }
                // check if the player is disconnected
                if (guildMemberData.CharacterSummaryData.IsOnline == false) {
                    continue;
                }
                networkManagerServer.AdvertiseGuildMemberStatusChange(memberAccountId, guild.GuildId, playerCharacterId, new GuildMemberNetworkData(guild.MemberList[playerCharacterId]));
            }
        }

        /*
public void ProcessLevelChanged(UnitController unitController) {
   int playerCharacterId = unitController.CharacterId;
   // check if character is in a guild
   if (guildMemberLookup.ContainsKey(playerCharacterId) == false) {
       return;
   }
   int guildId = guildMemberLookup[playerCharacterId];
   if (guildDictionary.ContainsKey(guildId) == false) {
       return;
   }
   Guild guild = guildDictionary[guildId];
   if (guild.MemberIdList.ContainsKey(playerCharacterId) == false) {
       return;
   }
   guild.MemberIdList[playerCharacterId].Level = unitController.CharacterStats.Level;

   foreach (CharacterSummaryData characterSummaryData in guild.MemberIdList.Values) {
       // get account id from player id
       int memberAccountId = playerManagerServer.GetAccountIdFromPlayerCharacterId(characterSummaryData.CharacterId);
       if (memberAccountId == -1) {
           continue;
       }
       // check if the player is disconnected
       if (characterSummaryData.IsOnline == false) {
           continue;
       }
       networkManagerServer.AdvertiseGuildMemberStatusChange(memberAccountId, guild.guildId, playerCharacterId, new CharacterSummaryNetworkData(guild.MemberIdList[playerCharacterId]));
   }
}
*/

    }

}