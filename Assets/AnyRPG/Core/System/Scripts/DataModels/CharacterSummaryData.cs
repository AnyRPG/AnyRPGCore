using System;
using System.Collections;
using System.Collections.Generic;

namespace AnyRPG {
    
    [Serializable]
    public class CharacterSummaryData {

        public int CharacterId = 0;
        public string CharacterName = string.Empty;
        public CharacterClass CharacterClass = null;
        public UnitProfile UnitProfile = null;
        public int Level = 1;
        public string CurrentZoneName = "Unknown";
        public bool IsOnline = false;

        public CharacterSummaryData() { }

        public CharacterSummaryData(CharacterSaveData characterSaveData, SystemDataFactory systemDataFactory) {
            CharacterId = characterSaveData.CharacterId;
            Level = characterSaveData.CharacterLevel;
            CharacterName = characterSaveData.CharacterName;
            UnitProfile = systemDataFactory.GetResource<UnitProfile>(characterSaveData.UnitProfileName);
            CharacterClass = systemDataFactory.GetResource<CharacterClass>(characterSaveData.CharacterClass);
            CurrentZoneName = characterSaveData.CurrentScene;
        }

        public CharacterSummaryData(CharacterSummaryNetworkData characterSummaryNetworkData, SystemDataFactory systemDataFactory) {
            CharacterId = characterSummaryNetworkData.CharacterId;
            CharacterName = characterSummaryNetworkData.CharacterName;
            CharacterClass = systemDataFactory.GetResource<CharacterClass>(characterSummaryNetworkData.CharacterClass);
            UnitProfile = systemDataFactory.GetResource<UnitProfile>(characterSummaryNetworkData.UnitProfile);
            Level = characterSummaryNetworkData.Level;
            CurrentZoneName = characterSummaryNetworkData.CurrentZoneName;
            IsOnline = characterSummaryNetworkData.IsOnline;
        }
    }

}