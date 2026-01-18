using System;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {

    public class GuildMemberNetworkData {
        public GuildRank Rank = GuildRank.Member;
        public CharacterSummaryNetworkData CharacterSummaryNetworkData;

        public GuildMemberNetworkData() { }

        public GuildMemberNetworkData(GuildMemberData guildMemberData) {
            Rank = guildMemberData.Rank;
            CharacterSummaryNetworkData = new CharacterSummaryNetworkData(guildMemberData.CharacterSummaryData);
        }

    }
}
