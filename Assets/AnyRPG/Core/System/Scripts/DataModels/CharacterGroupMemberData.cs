using System;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {

    public class CharacterGroupMemberData {
        public CharacterGroupRank Rank = CharacterGroupRank.Member;
        public CharacterSummaryData CharacterSummaryData;

        public CharacterGroupMemberData(CharacterGroupMemberNetworkData characterGroupMemberNetworkData, SystemDataFactory systemDataFactory) {
            Rank = characterGroupMemberNetworkData.Rank;
            CharacterSummaryData = new CharacterSummaryData(characterGroupMemberNetworkData.CharacterSummaryNetworkData, systemDataFactory);
        }

        public CharacterGroupMemberData(CharacterSummaryData characterSummaryData) {
            CharacterSummaryData = characterSummaryData;
            Rank = CharacterGroupRank.Member;
        }

        public CharacterGroupMemberData(CharacterSummaryData characterSummaryData, CharacterGroupRank rank) {
            CharacterSummaryData = characterSummaryData;
            Rank = rank;
        }

    }
}
