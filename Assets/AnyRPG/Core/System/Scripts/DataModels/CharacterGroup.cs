using System.Collections.Generic;

namespace AnyRPG {
    public class CharacterGroup {
        public int characterGroupId;
        public int leaderPlayerCharacterId;

        private Dictionary<UnitControllerMode, Dictionary<int, string>> characterIdList = new Dictionary<UnitControllerMode, Dictionary<int, string>>() {
            { UnitControllerMode.Player, new Dictionary<int, string>() },
            { UnitControllerMode.Pet, new Dictionary < int, string >() },
            { UnitControllerMode.AI, new Dictionary < int, string >() },
            { UnitControllerMode.Mount, new Dictionary < int, string >() },
            { UnitControllerMode.Preview, new Dictionary < int, string >() },
            { UnitControllerMode.Inanimate, new Dictionary < int, string >() },
        };
        
        public Dictionary<UnitControllerMode, Dictionary<int, string>> CharacterIdList { get => characterIdList; set => characterIdList = value; }

        public CharacterGroup() {
        }

        public CharacterGroup(int characterGroupId) {
            this.characterGroupId = characterGroupId;
        }

        public CharacterGroup(int characterGroupId, int leaderCharacterId, string leaderName) {
            this.characterGroupId = characterGroupId;
            this.leaderPlayerCharacterId = leaderCharacterId;
            CharacterIdList[UnitControllerMode.Player].Add(leaderCharacterId, leaderName);
        }

        public void AddPlayer(int playerCharacterId, string playerName) {
            CharacterIdList[UnitControllerMode.Player].Add(playerCharacterId, playerName);
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
