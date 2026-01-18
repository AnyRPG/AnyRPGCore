using System;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {

    public class CharacterGroupMemberNetworkData {
        public CharacterGroupRank Rank = CharacterGroupRank.Member;
        public CharacterSummaryNetworkData CharacterSummaryNetworkData;

        public CharacterGroupMemberNetworkData() { }

        public CharacterGroupMemberNetworkData(CharacterGroupMemberData characterGroupMemberData) {
            Rank = characterGroupMemberData.Rank;
            CharacterSummaryNetworkData = new CharacterSummaryNetworkData(characterGroupMemberData.CharacterSummaryData);
        }

    }
}
