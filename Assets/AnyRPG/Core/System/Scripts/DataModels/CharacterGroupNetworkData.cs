using System;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {

    [Serializable]
    public class CharacterGroupNetworkData {
        public int CharacterGroupId;
        public int LeaderCharacterId;

        public List<CharacterGroupMemberNetworkData> MemberIdList = new List<CharacterGroupMemberNetworkData>();

        public CharacterGroupNetworkData() {
        }

        public CharacterGroupNetworkData(CharacterGroup characterGroup) {
            CharacterGroupId = characterGroup.characterGroupId;
            LeaderCharacterId = characterGroup.leaderPlayerCharacterId;
            foreach (KeyValuePair<int, CharacterGroupMemberData> member in characterGroup.MemberList[UnitControllerMode.Player]) {
                MemberIdList.Add(new CharacterGroupMemberNetworkData(member.Value));
            }
        }

    }
}
