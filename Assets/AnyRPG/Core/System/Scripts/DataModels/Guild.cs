using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class Guild {
        public int GuildId;
        public string GuildName;
        public int LeaderPlayerCharacterId;

        /// <summary>
        /// characterId, GuildMemberData
        /// </summary>
        private Dictionary<int, GuildMemberData> memberList = new Dictionary<int, GuildMemberData>();

        /// <summary>
        /// characterId, GuildMemberData
        /// </summary>
        public Dictionary<int, GuildMemberData> MemberList { get => memberList; set => memberList = value; }

        public Guild() {
        }

        public Guild(int guildId) {
            this.GuildId = guildId;
        }

        public Guild(GuildNetworkData guildNetworkData, SystemDataFactory systemDataFactory) {
            this.GuildId = guildNetworkData.GuildId;
            this.GuildName = guildNetworkData.GuildName;
            this.LeaderPlayerCharacterId = guildNetworkData.LeaderPlayerCharacterId;
            foreach (GuildMemberNetworkData guildMemberNetworkData in guildNetworkData.MemberList) {
                MemberList.Add(guildMemberNetworkData.CharacterSummaryNetworkData.CharacterId, new GuildMemberData(guildMemberNetworkData, systemDataFactory));
            }
        }

        public Guild(string guildName, GuildMemberData guildMemberData) {
            this.GuildName = guildName;
            this.LeaderPlayerCharacterId = guildMemberData.CharacterSummaryData.CharacterId;
            MemberList.Add(guildMemberData.CharacterSummaryData.CharacterId, guildMemberData);
        }

        public Guild(int guildId, string guildName, GuildMemberData guildMemberData) {
            this.GuildId = guildId;
            this.GuildName = guildName;
            this.LeaderPlayerCharacterId = guildMemberData.CharacterSummaryData.CharacterId;
            MemberList.Add(guildMemberData.CharacterSummaryData.CharacterId, guildMemberData);
        }

        public Guild(GuildSaveData guildSaveData, PlayerCharacterService playerCharacterService) {
            this.GuildId = guildSaveData.GuildId;
            this.GuildName = guildSaveData.GuildName;
            this.LeaderPlayerCharacterId = guildSaveData.LeaderPlayerCharacterId;
            foreach (GuildMemberSaveData guildMemberSaveData in guildSaveData.MemberList) {
                CharacterSummaryData characterSummaryData = playerCharacterService.GetSummaryData(guildMemberSaveData.CharacterId);
                if (characterSummaryData == null) {
                    Debug.LogWarning("Guild() characterSummaryData was null. Skipping member!");
                    continue;
                }
                MemberList.Add(guildMemberSaveData.CharacterId, new GuildMemberData(characterSummaryData, guildMemberSaveData.Rank));
            }
        }

        public void AddPlayer(GuildMemberData guildMemberData) {
            MemberList.Add(guildMemberData.CharacterSummaryData.CharacterId, guildMemberData);
        }

        public void RemovePlayer(int playerCharacterId) {
            MemberList.Remove(playerCharacterId);
        }

        public void PromoteLeader(int newLeaderCharacterId) {
            LeaderPlayerCharacterId = newLeaderCharacterId;
        }

    }

    public enum GuildRank {
        Leader,
        Officer,
        Member
    }

}