using System.Collections.Generic;

namespace AnyRPG {
    public class Guild {
        public int guildId;
        public string guildName;
        public int leaderPlayerCharacterId;

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
            this.guildId = guildId;
        }

        public Guild(GuildNetworkData guildNetworkData, SystemDataFactory systemDataFactory) {
            this.guildId = guildNetworkData.GuildId;
            this.guildName = guildNetworkData.GuildName;
            this.leaderPlayerCharacterId = guildNetworkData.LeaderPlayerCharacterId;
            foreach (GuildMemberNetworkData guildMemberNetworkData in guildNetworkData.MemberList) {
                MemberList.Add(guildMemberNetworkData.CharacterSummaryNetworkData.CharacterId, new GuildMemberData(guildMemberNetworkData, systemDataFactory));
            }
        }

        public Guild(int guildId, string guildName, GuildMemberData guildMemberData) {
            this.guildId = guildId;
            this.guildName = guildName;
            this.leaderPlayerCharacterId = guildMemberData.CharacterSummaryData.CharacterId;
            MemberList.Add(guildMemberData.CharacterSummaryData.CharacterId, guildMemberData);
        }

        public Guild(GuildSaveData guildSaveData, PlayerCharacterService playerCharacterService) {
            this.guildId = guildSaveData.GuildId;
            this.guildName = guildSaveData.GuildName;
            this.leaderPlayerCharacterId = guildSaveData.LeaderPlayerCharacterId;
            foreach (GuildMemberSaveData guildMemberSaveData in guildSaveData.MemberList) {
                MemberList.Add(guildMemberSaveData.CharacterId, new GuildMemberData(playerCharacterService.GetSummaryData(guildMemberSaveData.CharacterId), guildMemberSaveData.Rank));
            }
        }

        public void AddPlayer(GuildMemberData guildMemberData) {
            MemberList.Add(guildMemberData.CharacterSummaryData.CharacterId, guildMemberData);
        }

        public void RemovePlayer(int playerCharacterId) {
            MemberList.Remove(playerCharacterId);
        }

        public void PromoteLeader(int newLeaderCharacterId) {
            leaderPlayerCharacterId = newLeaderCharacterId;
        }

    }

    public enum GuildRank {
        Leader,
        Officer,
        Member
    }

}