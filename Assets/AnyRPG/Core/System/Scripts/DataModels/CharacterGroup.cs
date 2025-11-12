using System.Collections.Generic;

namespace AnyRPG {
    public class CharacterGroup {
        public int characterGroupId;
        public int leaderPlayerCharacterId;

        private Dictionary<UnitControllerMode, List<int>> characterIdList = new Dictionary<UnitControllerMode, List<int>>() {
            { UnitControllerMode.Player, new List<int>() },
            { UnitControllerMode.Pet, new List<int>() },
            { UnitControllerMode.AI, new List<int>() },
            { UnitControllerMode.Mount, new List<int>() },
            { UnitControllerMode.Preview, new List<int>() },
            { UnitControllerMode.Inanimate, new List<int>() },
        };
        
        public Dictionary<UnitControllerMode, List<int>> CharacterIdList { get => characterIdList; set => characterIdList = value; }

        public CharacterGroup() {
        }

        public CharacterGroup(int characterGroupId) {
            this.characterGroupId = characterGroupId;
        }

        public CharacterGroup(int characterGroupId, int leaderCharacterId) {
            this.characterGroupId = characterGroupId;
            this.leaderPlayerCharacterId = leaderCharacterId;
            CharacterIdList[UnitControllerMode.Player].Add(leaderCharacterId);
        }

        public void AddPlayer(int playerCharacterId) {
            CharacterIdList[UnitControllerMode.Player].Add(playerCharacterId);
        }

        public void RemovePlayer(int playerCharacterId) {
            CharacterIdList[UnitControllerMode.Player].Remove(playerCharacterId);
        }
    }

    /*
    public class CharacterGroupMemberInfo {
        public int accountId;
        public int playerCharacterId;

        public CharacterGroupMemberInfo() {
        }

        public CharacterGroupMemberInfo(int accountId, int playerCharacterId) {
            this.accountId = accountId;
            this.playerCharacterId = playerCharacterId;
        }
    }
    */

}
