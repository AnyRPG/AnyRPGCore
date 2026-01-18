using System;
using System.Collections;
using System.Collections.Generic;

namespace AnyRPG {
    
    [Serializable]
    public class CharacterSummaryNetworkData {

        public int CharacterId = 0;
        public string CharacterName = string.Empty;
        public string CharacterClass = string.Empty;
        public string UnitProfile = string.Empty;
        public int Level = 1;
        public string CurrentZoneName = string.Empty;
        public bool IsOnline = false;

        public CharacterSummaryNetworkData() { }

        public CharacterSummaryNetworkData(CharacterSummaryData characterSummaryData) {
            CharacterId = characterSummaryData.CharacterId;
            CharacterName = characterSummaryData.CharacterName;
            if (characterSummaryData.CharacterClass != null) {
                CharacterClass = characterSummaryData.CharacterClass.ResourceName;
            }
            if (characterSummaryData.UnitProfile != null) {
                UnitProfile = characterSummaryData.UnitProfile.ResourceName;
            }
            Level = characterSummaryData.Level;
            CurrentZoneName = characterSummaryData.CurrentZoneName;
            IsOnline = characterSummaryData.IsOnline;
        }
    }

}