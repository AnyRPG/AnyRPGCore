using System;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {

    public class GuildMemberData {
        public GuildRank Rank = GuildRank.Member;
        public CharacterSummaryData CharacterSummaryData;

        public GuildMemberData(GuildMemberNetworkData guildMemberNetworkData, SystemDataFactory systemDataFactory) {
            Rank = guildMemberNetworkData.Rank;
            CharacterSummaryData = new CharacterSummaryData(guildMemberNetworkData.CharacterSummaryNetworkData, systemDataFactory);
        }

        public GuildMemberData(CharacterSummaryData characterSummaryData) {
            CharacterSummaryData = characterSummaryData;
            Rank = GuildRank.Member;
        }

        public GuildMemberData(CharacterSummaryData characterSummaryData, GuildRank rank) {
            CharacterSummaryData = characterSummaryData;
            Rank = rank;
        }

    }
}
