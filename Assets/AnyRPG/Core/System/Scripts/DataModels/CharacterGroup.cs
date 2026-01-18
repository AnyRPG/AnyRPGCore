using System.Collections.Generic;

namespace AnyRPG {
    public class CharacterGroup {
        public int characterGroupId;
        public int leaderPlayerCharacterId;

        private Dictionary<UnitControllerMode, Dictionary<int, CharacterGroupMemberData>> memberList = new Dictionary<UnitControllerMode, Dictionary<int, CharacterGroupMemberData>>() {
            { UnitControllerMode.Player, new Dictionary<int, CharacterGroupMemberData>() },
            { UnitControllerMode.Pet, new Dictionary < int, CharacterGroupMemberData >() },
            { UnitControllerMode.AI, new Dictionary < int, CharacterGroupMemberData >() },
            { UnitControllerMode.Mount, new Dictionary < int, CharacterGroupMemberData >() },
            { UnitControllerMode.Preview, new Dictionary < int, CharacterGroupMemberData >() },
            { UnitControllerMode.Inanimate, new Dictionary < int, CharacterGroupMemberData >() },
        };
        
        public Dictionary<UnitControllerMode, Dictionary<int, CharacterGroupMemberData>> MemberList { get => memberList; set => memberList = value; }

        public CharacterGroup() {
        }

        public CharacterGroup(int characterGroupId) {
            this.characterGroupId = characterGroupId;
        }

        public CharacterGroup(int characterGroupId, CharacterGroupMemberData characterGroupMemberData) {
            this.characterGroupId = characterGroupId;
            this.leaderPlayerCharacterId = characterGroupMemberData.CharacterSummaryData.CharacterId;
            MemberList[UnitControllerMode.Player].Add(characterGroupMemberData.CharacterSummaryData.CharacterId, characterGroupMemberData);
        }

        public CharacterGroup(CharacterGroupNetworkData characterGroupNetworkData, SystemDataFactory systemDataFactory) {
            this.characterGroupId = characterGroupNetworkData.CharacterGroupId;
            this.leaderPlayerCharacterId = characterGroupNetworkData.LeaderCharacterId;
            foreach (CharacterGroupMemberNetworkData member in characterGroupNetworkData.MemberIdList) {
                MemberList[UnitControllerMode.Player].Add(member.CharacterSummaryNetworkData.CharacterId, new CharacterGroupMemberData(member, systemDataFactory));
            }
        }

        public void AddPlayer(CharacterGroupMemberData characterGroupMemberData) {
            MemberList[UnitControllerMode.Player].Add(characterGroupMemberData.CharacterSummaryData.CharacterId, characterGroupMemberData);
        }

        public void RemovePlayer(int playerCharacterId) {
            MemberList[UnitControllerMode.Player].Remove(playerCharacterId);
        }

    }

    public enum CharacterGroupRank {
        Member,
        Assistant,
        Leader
    }

}
