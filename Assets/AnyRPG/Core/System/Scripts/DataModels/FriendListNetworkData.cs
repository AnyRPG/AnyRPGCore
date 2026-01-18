using System;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {

    [Serializable]
    public class FriendListNetworkData {
        public int PlayerCharacterId;

        public List<CharacterSummaryNetworkData> MemberIdList = new List<CharacterSummaryNetworkData>();

        public FriendListNetworkData() {
        }

        public FriendListNetworkData(FriendList friendList) {
            PlayerCharacterId = friendList.playerCharacterId;
            foreach (KeyValuePair<int, CharacterSummaryData> member in friendList.MemberIdList) {
                MemberIdList.Add(new CharacterSummaryNetworkData(member.Value));
            }
        }

    }
}
