using System.Collections.Generic;

namespace AnyRPG {
    public class FriendList {
        public int playerCharacterId;

        private Dictionary<int, CharacterSummaryData> memberIdList = new Dictionary<int, CharacterSummaryData>();

        public Dictionary<int, CharacterSummaryData> MemberIdList { get => memberIdList; set => memberIdList = value; }

        public FriendList() {
        }

        public FriendList(FriendListNetworkData friendListNetworkData, SystemDataFactory systemDataFactory) {
            this.playerCharacterId = friendListNetworkData.PlayerCharacterId;
            foreach (CharacterSummaryNetworkData memberInfo in friendListNetworkData.MemberIdList) {
                MemberIdList.Add(memberInfo.CharacterId, new CharacterSummaryData(memberInfo, systemDataFactory));
            }
        }

        public FriendList(int playerCharacterId) {
            this.playerCharacterId = playerCharacterId;
        }

        public FriendList(FriendListSaveData friendListSaveData, PlayerCharacterService playerCharacterService) {
            this.playerCharacterId = friendListSaveData.PlayerCharacterId;
            foreach (int memberId in friendListSaveData.PlayerIdList) {
                MemberIdList.Add(memberId, playerCharacterService.GetSummaryData(memberId));
            }
        }

        /*
        public void AddPlayer(int playerCharacterId, string playerName) {
            AddPlayer(new CharacterSummaryData(playerCharacterId, playerName, true));
        }
        */

        public void AddPlayer(CharacterSummaryData characterSummaryData) {
            MemberIdList.Add(characterSummaryData.CharacterId, characterSummaryData);
        }

        public void RemovePlayer(int playerCharacterId) {
            MemberIdList.Remove(playerCharacterId);
        }

    }

}