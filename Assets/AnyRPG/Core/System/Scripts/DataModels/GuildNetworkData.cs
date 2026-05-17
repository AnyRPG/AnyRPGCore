using System;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {

    [Serializable]
    public class GuildNetworkData {
        public int GuildId;
        public string GuildName = string.Empty;
        public int LeaderPlayerCharacterId;

        public List<GuildMemberNetworkData> MemberList = new List<GuildMemberNetworkData>();

        public GuildNetworkData() {
        }

        public GuildNetworkData(Guild guild) {
            GuildId = guild.GuildId;
            GuildName = guild.GuildName;
            LeaderPlayerCharacterId = guild.LeaderPlayerCharacterId;
            foreach (KeyValuePair<int, GuildMemberData> member in guild.MemberList) {
                MemberList.Add(new GuildMemberNetworkData(member.Value));
            }
        }

    }
}
