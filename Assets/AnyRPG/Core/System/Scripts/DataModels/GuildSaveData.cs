using System;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {

    [Serializable]
    public class GuildSaveData {
        public int GuildId;
        public string GuildName = string.Empty;
        public int LeaderPlayerCharacterId;

        public List<GuildMemberSaveData> MemberList = new List<GuildMemberSaveData>();

        public GuildSaveData() {
        }

        public GuildSaveData(Guild guild) {
            GuildId = guild.GuildId;
            GuildName = guild.GuildName;
            LeaderPlayerCharacterId = guild.LeaderPlayerCharacterId;
            foreach (KeyValuePair<int, GuildMemberData> member in guild.MemberList) {
                MemberList.Add(new GuildMemberSaveData(member.Value));
            }
        }

    }
}
