using System;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {

    [Serializable]
    public class GuildMemberSaveData {
        public int CharacterId;
        public GuildRank Rank = GuildRank.Member;
        
        public GuildMemberSaveData(GuildMemberData guildMemberData) {
            CharacterId = guildMemberData.CharacterSummaryData.CharacterId;
            Rank = guildMemberData.Rank;
        }
    }

}