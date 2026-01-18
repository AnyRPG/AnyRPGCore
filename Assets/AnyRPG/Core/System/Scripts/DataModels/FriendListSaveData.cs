using System;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {

    [Serializable]
    public class FriendListSaveData {
        public int PlayerCharacterId;

        public List<int> PlayerIdList = new List<int>();

        public FriendListSaveData() {
        }

        public FriendListSaveData(FriendList friendList) {
            PlayerCharacterId = friendList.playerCharacterId;
            foreach (KeyValuePair<int, CharacterSummaryData> member in friendList.MemberIdList) {
                PlayerIdList.Add(member.Key);
            }
        }

    }
}
